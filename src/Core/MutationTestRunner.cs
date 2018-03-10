using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Fettle.Core
{
    public class MutationTestRunner : IMutationTestRunner
    {
        private readonly ITestRunner testRunner;
        private readonly IReadOnlyDictionary<string, ImmutableHashSet<string>> methodsAndTheirCoveringTests;
        private readonly IEventListener eventListener;

        public MutationTestRunner(
            IReadOnlyDictionary<string, ImmutableHashSet<string>> methodsAndTheirCoveringTests,
            IEventListener eventListener = null) :
            this(new NUnitTestEngine(), methodsAndTheirCoveringTests, eventListener)
        {
        }

        internal MutationTestRunner(
            ITestRunner testRunner,
            IReadOnlyDictionary<string, ImmutableHashSet<string>> methodsAndTheirCoveringTests,
            IEventListener eventListener = null)
        {
            this.testRunner = testRunner;
            this.methodsAndTheirCoveringTests = methodsAndTheirCoveringTests;
            this.eventListener = eventListener ?? new NullEventListener();
        }

        public async Task<MutationTestResult> Run(Config config)
        {
            var validationErrors = config.Validate().ToList();
            if (validationErrors.Any())
            {
                return new MutationTestResult().WithErrors(validationErrors);
            }

            var baseTempDirectory = TempDirectory.Create();
            try
            {
                CreateTempDirectories(baseTempDirectory, config);

                var survivingMutants = await MutateSolution(config, baseTempDirectory);
                return new MutationTestResult().WithSurvivingMutants(survivingMutants);
            }
            finally
            {
                Directory.Delete(baseTempDirectory, recursive: true);
            }
        }

        private async Task<IEnumerable<SurvivingMutant>> MutateSolution(
            Config config, 
            string tempDirectory)
        {
            var survivingMutants = new List<SurvivingMutant>();

            using (var workspace = MSBuildWorkspace.Create())
            {
                var solution = await workspace.OpenSolutionAsync(config.SolutionFilePath);

                var classesToMutate = solution.MutatableClasses(config);

                for (int classIndex = 0; classIndex < classesToMutate.Length; ++classIndex)
                {
                    var classToMutate = classesToMutate[classIndex];

                    eventListener.BeginMutationOfFile(classToMutate.FilePath,
                        Path.GetDirectoryName(config.SolutionFilePath), classIndex, classesToMutate.Length);

                    var survivorsInClass = await MutateClass(config, classToMutate, tempDirectory)
                        .ConfigureAwait(false);

                    eventListener.EndMutationOfFile(classToMutate.FilePath);

                    survivingMutants.AddRange(survivorsInClass);
                }
            }

            return survivingMutants;
        }

        private async Task<IEnumerable<SurvivingMutant>> MutateClass(
            Config config, 
            Document classToMutate, 
            string tempDirectory)
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

                if (!methodsAndTheirCoveringTests.TryGetValue(methodName, out var testsToRun))
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
                var survivor = await MutateSyntaxNode(config, classToMutate, nodeToMutate, classRoot, mutators,
                    testsToRun.ToArray(), tempDirectory);

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
            string[] testsToRun,
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
                    .Test(mutant, config, testRunner, testsToRun, tempDirectory)
                    .ConfigureAwait(false);
                
                if (survivor != null)
                    return survivor;
            }

            return null;
        }

        private static void CreateTempDirectories(string baseTempDirectory, Config config)
        {
            foreach (var testAssemblyFilePath in config.TestAssemblyFilePaths)
            {
                var from = Path.GetDirectoryName(testAssemblyFilePath);
                var to = Path.Combine(baseTempDirectory, Path.GetFileNameWithoutExtension(testAssemblyFilePath));
                Directory.CreateDirectory(to);
                DirectoryUtils.CopyDirectoryContents(from, to);
            }
        }
    }
}

