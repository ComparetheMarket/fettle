using Fettle.Core;

namespace Fettle.Console
{
    internal class VerboseEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;

        public VerboseEventListener(IOutputWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total)
        {
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
        }

        public void MemberMutating(string name)
        {
        }

        public void SyntaxNodeMutating(int index, int total)
        {
        }

        public void MutantSurvived(SurvivingMutant survivingMutant)
        {
        }

        public void MutantKilled(Mutant killedMutant)
        {
            outputWriter.WriteLine($"{killedMutant.OriginalLine} => {killedMutant.MutatedLine} - mutant killed");
        }

        public void EndMutationOfFile(string filePath)
        {
        }
    }
}