namespace Fettle.Core
{
    public class Mutant
    {
        public string SourceFilePath { get; set; }
        public int SourceLine { get; set; }
        public string OriginalLine { get; set; }
        public string MutatedLine { get; set; }
    }
}