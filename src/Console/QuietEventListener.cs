using Fettle.Core;

namespace Fettle.Console
{
    internal class QuietEventListener : IEventListener
    {
        public QuietEventListener(IOutputWriter _)
        {
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

        public void MutantSurvived(SurvivingMutant survivor)
        {
        }

        public void MutantKilled(Mutant killedMutant)
        {
        }

        public void EndMutationOfFile(string filePath)
        {
        }
    }
}