namespace Fettle.Core
{
    public interface ICoverageAnalysisResult
    {
        string ErrorDescription { get; }
        
        string[] AllAnalysedMethods { get; }

        string[] TestsThatCoverMethod(string method, string testAssemblyFilePath);
        
        bool IsMethodCovered(string method);
    }
}