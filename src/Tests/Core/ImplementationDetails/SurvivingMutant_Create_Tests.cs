using System.Linq;
using System.Threading.Tasks;
using Fettle.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class SurvivingMutant_Create_Tests
    {
        [Test]
        public async Task File_path_and_line_are_extracted_from_document()
        {
            var survivingMutant = await CreateSurvivingMutantFromExpression(
                originalExpression: "a > 0",
                mutatedExpression: "a < 0");

            Assert.Multiple(() =>
            {
                Assert.That(survivingMutant.SourceFilePath, Is.EqualTo(@"c:\someproject\somefile.cs"));
                Assert.That(survivingMutant.SourceLine, Is.EqualTo(7));
            });
        }

        [Test]
        public async Task Source_text_of_original_and_mutated_lines_are_extracted()
        {
            const string originalSource = "a > 0";
            const string mutatedSource = "a < 0";
            var survivingMutant = await CreateSurvivingMutantFromExpression(originalSource, mutatedSource);

            Assert.Multiple(() =>
            {
                Assert.That(survivingMutant.OriginalLine.TrimStart(), Does.Contain(originalSource));
                Assert.That(survivingMutant.MutatedLine.TrimStart(), Does.Contain(mutatedSource));
            });
        }

        [Test]
        public async Task Source_text_of_original_and_mutated_lines_are_extracted_when_they_span_multiple_lines()
        {
            const string originalSource = 
@"a > 0 &&
    a != -1";
            const string mutatedSource = 
@"a < 0 &&
    a != -1";
            var survivingMutant = await CreateSurvivingMutantFromExpression(originalSource, mutatedSource);

            Assert.Multiple(() =>
            {
                Assert.That(survivingMutant.OriginalLine.TrimStart(), Does.Contain(originalSource));
                Assert.That(survivingMutant.MutatedLine.TrimStart(), Does.Contain(mutatedSource));
            });
        }

        [Test]
        public async Task The_source_text_of_if_statements_does_not_include_the_if_statement_body()
        {
            const string originalExpression = 
@"a > 0 &&
    a != -1";
            const string mutatedExpression = 
@"a < 0 &&
    a != -1";
            var survivingMutant = await CreateSurvivingMutantFromIfStatement(originalExpression, mutatedExpression);

            Assert.Multiple(() =>
            {
                Assert.That(survivingMutant.OriginalLine.TrimStart(), Is.EqualTo(
@"if (a > 0 &&
    a != -1)"));
                Assert.That(survivingMutant.MutatedLine.TrimStart(), Is.EqualTo(
@"if (a < 0 &&
    a != -1)"));
            });
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

                return emptyProject.AddDocument("somefile.cs", source, filePath: @"c:\someproject\somefile.cs");
            }
        }

        private async Task<SurvivingMutant> CreateSurvivingMutantFromExpression(
            string originalExpression, 
            string mutatedExpression)
        {
            var originalDocument = SourceToDocument(
$@"namespace DummyNamespace
{{
    public static class DummyClass
    {{
        public static bool IsPositive(int a)
        {{
            return {originalExpression};
        }}
    }}
}}");
            var originalSyntaxRoot = await originalDocument.GetSyntaxRootAsync();
            var originalNode = originalSyntaxRoot.DescendantNodes().OfType<BinaryExpressionSyntax>().First();

            var mutatedNode = SyntaxFactory.ParseExpression(mutatedExpression);

            var mutatedRoot = originalSyntaxRoot.ReplaceNode(originalNode, mutatedNode);

            return await SurvivingMutant.Create(originalDocument, originalNode, mutatedRoot);
        }
        
        private async Task<SurvivingMutant> CreateSurvivingMutantFromIfStatement(
            string originalExpression, 
            string mutatedExpression)
        {
            var originalDocument = SourceToDocument(
$@"namespace DummyNamespace
{{
    public static class DummyClass
    {{
        public static bool IsPositive(int a)
        {{
            if ({originalExpression})
            {{
                return true;
            }}
            return false;
        }}
    }}
}}");
            var originalSyntaxRoot = await originalDocument.GetSyntaxRootAsync();
            var originalNode = originalSyntaxRoot.DescendantNodes().OfType<IfStatementSyntax>().First();

            var mutatedNode = originalNode.WithCondition(SyntaxFactory.ParseExpression(mutatedExpression));

            var mutatedRoot = originalSyntaxRoot.ReplaceNode(originalNode, mutatedNode);

            return await SurvivingMutant.Create(originalDocument, originalNode, mutatedRoot);
        }
    }
}
