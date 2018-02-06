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
        
        internal static async Task<SurvivingMutant> CreateFrom(MutatedClass mutatedClass)
        {
            var originalClassRoot = await mutatedClass.OriginalClass.GetSyntaxRootAsync();

            var originalLineNumber = originalClassRoot.SyntaxTree.GetLineSpan(mutatedClass.OriginalNode.Span)
                .StartLinePosition.Line;

            var mutatedLineNumber = mutatedClass.MutatedClassRoot.SyntaxTree.GetLineSpan(mutatedClass.OriginalNode.Span)
                .StartLinePosition.Line;

            var originalSource = SourceText.From(originalClassRoot.GetText().ToString());
            var mutatedSource = SourceText.From(mutatedClass.MutatedClassRoot.GetText().ToString());

            return new SurvivingMutant
            {
                SourceFilePath = mutatedClass.OriginalClass.FilePath,
                SourceLine = originalLineNumber + 1,
                OriginalLine = originalSource.Lines[originalLineNumber].ToString(),
                MutatedLine = mutatedSource.Lines[mutatedLineNumber].ToString()
            };
        }
    }
}