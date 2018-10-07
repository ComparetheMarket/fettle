namespace Fettle.Core
{
    public interface ISourceControlIntegration
    {
        string[] FindLocallyModifiedFiles(Config config);
    }
}
