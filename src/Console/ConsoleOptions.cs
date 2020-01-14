namespace Fettle.Console
{
    internal class ConsoleOptions
    {
        public Verbosity Verbosity { get; set; }
        public bool SkipCoverageAnalysis { get; set; }
        public bool ModificationsOnly { get; set; }
    }
}