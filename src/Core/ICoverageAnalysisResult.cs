namespace Fettle.Core
{
    public interface ICoverageAnalysisResult
    {
        string ErrorDescription { get; }
        
        string[] AllAnalysedMembers { get; }

        string[] TestsThatCoverMember(string member, string testAssemblyFilePath);
        
        bool IsMemberCovered(string member);
    }
}