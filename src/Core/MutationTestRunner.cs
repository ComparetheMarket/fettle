using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.MethodCoverage;
using Fettle.Core.Internal.NUnit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

namespace Fettle.Core
{
    public class MutationTestRunner : IMutationTestRunner
    {
        private readonly ITestRunner testRunner;
        private readonly IEventListener eventListener;

        public MutationTestRunner(IEventListener eventListener = null) :
            this(new NUnitTestRunner(), eventListener)
        {
        }

        internal MutationTestRunner(ITestRunner testRunner, IEventListener eventListener = null)
        {
            this.testRunner = testRunner;
            this.eventListener = eventListener ?? new NullEventListener();
        }

        public async Task<Result> Run(Config config)
        {
            var result = new Result();
            
            var tempDirectory = Path.Combine(Path.GetTempPath(), $"fettle-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDirectory);

            var methodCoverage = config.CoverageReportFilePath != null ?
                (IMethodCoverage)OpenCoverReportFile.Parse(File.ReadAllText(config.CoverageReportFilePath)) :
                (IMethodCoverage)new NullMethodCoverage();
            
            try
            {
                foreach (var testAssemblyFilePath in config.TestAssemblyFilePaths)
                {
                    var from = Path.GetDirectoryName(testAssemblyFilePath);
                    var to = Path.Combine(tempDirectory, Path.GetFileNameWithoutExtension(testAssemblyFilePath));
                    Directory.CreateDirectory(to);
                    DirectoryUtils.CopyDirectoryContents(from, to);
                }

                using (var workspace = MSBuildWorkspace.Create())
                {
                    var solution = await workspace.OpenSolutionAsync(config.SolutionFilePath);

                    var classesToMutate = solution.MutatableClasses(config);

                    for (int classIndex = 0; classIndex < classesToMutate.Length; ++classIndex)
                    {
                        var classToMutate = classesToMutate[classIndex];
                        
                        eventListener.BeginMutationOfFile(classToMutate.FilePath, 
                            Path.GetDirectoryName(config.SolutionFilePath), classIndex, classesToMutate.Length);

                        var survivorsInClass = await MutateClass(config, classToMutate, tempDirectory, methodCoverage)
                            .ConfigureAwait(false);
                        
                        eventListener.EndMutationOfFile(classToMutate.FilePath);

                        result.SurvivingMutants.AddRange(survivorsInClass);
                    }
                }
            }
            finally
            {
                Directory.Delete(tempDirectory, recursive: true);
            }

            return result;
        }

        private async Task<IEnumerable<SurvivingMutant>> MutateClass(
            Config config, 
            Document classToMutate, 
            string tempDirectory,
            IMethodCoverage methodCoverage)
        {
            var survivors = new List<SurvivingMutant>();
            var classRoot = await classToMutate.GetSyntaxRootAsync();
            var documentSemanticModel = await classToMutate.GetSemanticModelAsync();

            var nodesToMutate = classRoot.DescendantNodes().ToArray();
            var reportedMethods = new HashSet<string>();

            var isIgnoring = false;

            for (var nodeIndex = 0; nodeIndex < nodesToMutate.Length; ++nodeIndex)
            {
                var nodeToMutate = nodesToMutate[nodeIndex];

                var methodName = nodeToMutate.NameOfContainingMethod(documentSemanticModel);
                if (methodName == null)
                {
                    // The node is not within a method, e.g. it's a class or namespace declaration.
                    // Therefore there is no code to mutate.
                    continue;
                }

                if (!methodCoverage.IsMethodCovered(methodName))
                {
                    continue;
                }

                if (Ignoring.NodeHasBeginIgnoreComment(nodeToMutate)) isIgnoring = true;
                else if (Ignoring.NodeHasEndIgnoreComment(nodeToMutate)) isIgnoring = false;
                
                if (isIgnoring)
                {
                    continue;
                }

                var mutators = nodeToMutate.SupportedMutators();
                if (!mutators.Any())
                {
                    continue;
                }

                if (!reportedMethods.Contains(methodName))
                {
                    eventListener.MethodMutating(methodName);
                    reportedMethods.Add(methodName);
                }

                eventListener.SyntaxNodeMutating(nodeIndex, nodesToMutate.Length);
                var survivor = await MutateSyntaxNode(config, classToMutate, nodeToMutate, classRoot, mutators, tempDirectory);

                if (survivor != null)
                {
                    survivors.Add(survivor);
                    eventListener.MutantSurvived(survivor);
                }
            }

            return survivors;
        }

        private async Task<SurvivingMutant> MutateSyntaxNode(
            Config config, 
            Document classToMutate,
            SyntaxNode nodeToMutate,
            SyntaxNode classRoot,
            IEnumerable<IMutator> mutators,
            string tempDirectory)
        {
            MutatedClass CreateMutant(IMutator mutator)
            {
                var mutatedNode = mutator.Mutate(nodeToMutate);
                
                return new MutatedClass(
                    mutatedClassRoot: classRoot.ReplaceNode(nodeToMutate, mutatedNode),
                    originalNode: nodeToMutate,
                    originalClass: classToMutate);
            }

            foreach (var mutator in mutators)
            {
                var mutant = CreateMutant(mutator);
                
                var survivor = await mutant
                    .Test(mutant, config, testRunner, tempDirectory)
                    .ConfigureAwait(false);
                
                if (survivor != null)
                    return survivor;
            }

            return null;
        }
    }
}

