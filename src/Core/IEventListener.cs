namespace Fettle.Core
{
    public interface IEventListener
    {
        void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total);
        void MemberCoveredByTests(string memberName);

        void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total);
        void MemberMutating(string name);
        void SyntaxNodeMutating(int index, int total);
        void MutantSurvived(Mutant survivingMutant);
        void MutantKilled(Mutant killedMutant, string testFailureDescription);
        void MutantSkipped(Mutant skippedMutant, string reason);
        void EndMutationOfFile(string filePath);
    }
}