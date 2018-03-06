﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal;
using Fettle.Core.Internal.NUnit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.MSBuild;

namespace Fettle.Core
{
    public class MethodCoverage : IMethodCoverage
    {
        private readonly ITestFinder testFinder;
        private readonly ITestRunner testRunner;

        public MethodCoverage() : this(new NUnitTestEngine(), new NUnitTestEngine())
        {
        }

        internal MethodCoverage(ITestFinder testFinder, ITestRunner testRunner)
        {
            this.testFinder = testFinder;
            this.testRunner = testRunner;
        }

        public async Task<CoverageAnalysisResult> AnalyseMethodCoverage(Config config)
        {
            var methodIdsToNames = new Dictionary<string, string>();
            var methodsAndCoveringTests = new Dictionary<string, ImmutableHashSet<string>>();

            var tests = testFinder.FindTests(config.TestAssemblyFilePaths);

            using (var workspace = MSBuildWorkspace.Create())
            {
                var solution = await workspace.OpenSolutionAsync(config.SolutionFilePath);

                foreach (var project in solution.Projects.Where(p => Filtering.ShouldMutateProject(p, config)))
                {
                    var originalSyntaxTrees = new List<SyntaxTree>();
                    var modifiedSyntaxTrees = new List<SyntaxTree>();
                    
                    foreach (var originalClass in project.Documents)
                    {
                        var originalSyntaxTree = await originalClass.GetSyntaxTreeAsync().ConfigureAwait(false);
                        originalSyntaxTrees.Add(originalSyntaxTree);
                        modifiedSyntaxTrees.Add(await InstrumentDocument(originalSyntaxTree, originalClass, methodIdsToNames));
                    }

                    var compilation = (await project.GetCompilationAsync().ConfigureAwait(false))
                        .RemoveSyntaxTrees(originalSyntaxTrees)
                        .AddSyntaxTrees(modifiedSyntaxTrees);

                    var compilationResult = ProjectCompilation.CompileProject(
                        $@"c:\temp\fettletemp\{project.AssemblyName}.dll", 
                        compilation);
                    if (!compilationResult.Success)
                    {
                        throw new Exception($"compilation failed! {string.Join(Environment.NewLine, compilationResult.Diagnostics)}");
                    }

                    var copiedTestAssemblyFilePaths = config.TestAssemblyFilePaths
                        .Select(x => Path.Combine($@"c:\temp\fettletemp\{Path.GetFileName(x)}"))
                        .ToList();

                    foreach (var test in tests)
                    {
                        var runResult = testRunner.RunTests(copiedTestAssemblyFilePaths, new[] {test});
                        if (runResult.Status != TestRunStatus.AllTestsPassed)
                        {
                            return CoverageAnalysisResult.Error($"The test \"{test}\" failed");
                        }

                        var calledMethodIds = new HashSet<string>();
                        var outputLines = runResult.ConsoleOutput.Split(new []{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var outputLine in outputLines)
                        {
                            if (outputLine.StartsWith("fettle_covered_method:"))
                            {
                                var method = outputLine.Substring("fettle_covered_method:".Length);
                                calledMethodIds.Add(method);
                            }
                        }

                        var calledMethodNames = calledMethodIds
                            .Where(id => methodIdsToNames.ContainsKey(id)) // todo: remove once guarded against
                            .Select(id => methodIdsToNames[id]);

                        foreach (var calledMethodName in calledMethodNames)
                        {
                            if (!methodsAndCoveringTests.ContainsKey(calledMethodName))
                                methodsAndCoveringTests.Add(calledMethodName, ImmutableHashSet<string>.Empty);

                            methodsAndCoveringTests[calledMethodName] = methodsAndCoveringTests[calledMethodName].Add(test);
                        }
                    }
                }
            }

            return CoverageAnalysisResult.Success(methodsAndCoveringTests);
        }

        private async Task<SyntaxTree> InstrumentDocument(
            SyntaxTree originalSyntaxTree,
            Document document,
            IDictionary<string,string> methodIds)
        {
            var root = await originalSyntaxTree.GetRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var documentEditor = DocumentEditor.CreateAsync(document).Result;

            foreach (var classNode in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                foreach (var methodNode in classNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    var fullMethodName = methodNode.ChildNodes().First().NameOfContainingMethod(semanticModel);
                    var methodId = Guid.NewGuid().ToString();
                    methodIds.Add(methodId, fullMethodName);

                    var newNode = SyntaxFactory.ParseStatement(
                        $"System.Console.WriteLine(\"fettle_covered_method:{methodId}\");");
                    
                    documentEditor.InsertBefore(methodNode.Body.ChildNodes().First(), newNode);
                }
            }

            return await documentEditor.GetChangedDocument().GetSyntaxTreeAsync();
        }
    }
}
