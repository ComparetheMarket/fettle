namespace Fettle.Core
{
    public interface ICoverageAnalysisResult
    {
        bool WasSuccessful { get; }

        string ErrorDescription { get; }
        
        string[] AllAnalysedMembers { get; }

        string[] TestsThatCoverMember(string member, string testAssemblyFilePath);
        
        bool IsMemberCovered(string member);
    }
}