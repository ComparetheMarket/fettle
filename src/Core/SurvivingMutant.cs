using System.Threading.Tasks;
using Fettle.Core.Internal;
using Microsoft.CodeAnalysis.Text;

namespace Fettle.Core
{
    public class SurvivingMutant
    {
        public string SourceFilePath { get; set; }
        public int SourceLine { get; set; }
        public string OriginalLine { get; set; }
        public string MutatedLine { get; set; }
        
        internal static async Task<SurvivingMutant> CreateFrom(MutationJob mutationJob)
        {
            var originalClassRoot = await mutationJob.OriginalClass.GetSyntaxRootAsync();

            var originalLineNumber = originalClassRoot.SyntaxTree.GetLineSpan(mutationJob.OriginalNode.Span)
                .StartLinePosition.Line;

            var mutatedLineNumber = mutationJob.MutatedClassRoot.SyntaxTree.GetLineSpan(mutationJob.OriginalNode.Span)
                .StartLinePosition.Line;

            var originalSource = SourceText.From(originalClassRoot.GetText().ToString());
            var mutatedSource = SourceText.From(mutationJob.MutatedClassRoot.GetText().ToString());

            return new SurvivingMutant
            {
                SourceFilePath = mutationJob.OriginalClass.FilePath,
                SourceLine = originalLineNumber + 1,
                OriginalLine = originalSource.Lines[originalLineNumber].ToString(),
                MutatedLine = mutatedSource.Lines[mutatedLineNumber].ToString()
            };
        }
    }
}