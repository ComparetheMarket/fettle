using System.Threading.Tasks;
using Fettle.Core.Internal;

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
            var originalSyntaxRoot = await mutationJob.OriginalClass.GetSyntaxRootAsync();

            var originalLineNumber = originalSyntaxRoot.SyntaxTree.GetLineSpan(mutationJob.OriginalNode.Span)
                .StartLinePosition.Line;

            return new SurvivingMutant
            {
                SourceFilePath = mutationJob.OriginalClass.FilePath,
                SourceLine = originalLineNumber + 1,
                OriginalLine = mutationJob.OriginalNode.Span.ToSourceText(originalSyntaxRoot),
                MutatedLine = mutationJob.OriginalNode.Span.ToSourceText(mutationJob.MutatedSyntaxRoot)
            };
        }
    }
}