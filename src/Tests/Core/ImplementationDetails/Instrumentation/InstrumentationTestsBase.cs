using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fettle.Tests.Core.ImplementationDetails.Instrumentation
{
    class InstrumentationTestsBase
    {
        protected InstrumentationTestsBase()
        {
        }

        protected class InstrumentationInput
        {
            public Document OriginalDocument { get; set; }
            public SyntaxTree OriginalSyntaxTree { get; set; }
            public MemberDeclarationSyntax MemberToInstrument { get; set;}
        }

        protected async Task<InstrumentationInput> CreateInput<T>(string originalSource) where T : MemberDeclarationSyntax
        {
            var document = SourceToDocument(originalSource);
            return new InstrumentationInput
            {
                OriginalDocument = document,
                OriginalSyntaxTree = await document.GetSyntaxTreeAsync(),
                MemberToInstrument = ExtractLastSyntaxNodeFromSource<T>(originalSource)
            };
        }

        private static T ExtractLastSyntaxNodeFromSource<T>(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            return syntaxTree.GetRoot().DescendantNodes().OfType<T>().Last();
        }

        protected static string[] SourceOfInstrumentedMember<T>(SyntaxTree instrumentedSyntaxTree) where T : MemberDeclarationSyntax
        {
            return instrumentedSyntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<T>()
                .Last()
                .NormalizeWhitespace()
                .ToString()
                .Split(new []{ Environment.NewLine }, StringSplitOptions.None);
        }

        private static Document SourceToDocument(string source)
        {
            using (var ws = new AdhocWorkspace())
            {
                var emptyProject = ws.AddProject(
                    ProjectInfo.Create(
                        ProjectId.CreateNewId(),
                        VersionStamp.Default,
                        "test",
                        "test.dll",
                        LanguageNames.CSharp));

                return emptyProject.AddDocument("test.cs", source);
            }
        }
    }
}
