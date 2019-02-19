using System;
using Fettle.Core;

namespace Fettle.Console
{
    internal class VerboseEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;
        private bool anyMutationsMadeForCurrentFile;
        private string baseSourceDir;

        public VerboseEventListener(IOutputWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total)
        {
            outputWriter.WriteLine($"Analysing coverage [{index+1,3}/{total}]: {fullTestName}");
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
            baseSourceDir = baseSourceDirectory;
            anyMutationsMadeForCurrentFile = false;

            outputWriter.WriteLine("");
            outputWriter.WriteLine("");
            outputWriter.WriteLine($">>>>> Found file: {ToRelativePath(filePath)}");
        }

        public void MemberMutating(string name)
        {
            anyMutationsMadeForCurrentFile = true;

            outputWriter.WriteLine("");
            outputWriter.WriteLine($"Found member {MemberName.Simplify(name)}");
        }

        public void SyntaxNodeMutating(int index, int total)
        {
            outputWriter.WriteLine($"Mutating syntax node {index+1}");
        }

        public void MutantSurvived(Mutant survivingMutant)
        {
            outputWriter.WriteFailureLine($"Mutant SURVIVED at line {survivingMutant.SourceLine}");
            outputWriter.WriteFailureLine($"   Original: {survivingMutant.OriginalLine}");
            outputWriter.WriteFailureLine($"   Mutated:  {survivingMutant.MutatedLine}");
        }

        public void MutantKilled(Mutant killedMutant)
        {
            outputWriter.WriteLine($"Mutant killed at line {killedMutant.SourceLine}");
            outputWriter.WriteLine($"   Original: {killedMutant.OriginalLine}");
            outputWriter.WriteLine($"   Mutated:  {killedMutant.MutatedLine}");
        }

        public void EndMutationOfFile(string filePath)
        {
            if (anyMutationsMadeForCurrentFile)
            {
                outputWriter.WriteLine("");
            }
            else
            {
                outputWriter.Write(Environment.NewLine);
                outputWriter.Write("  Nothing found to mutate.");
            }
        }

        private string ToRelativePath(string filePath) => filePath.Substring(baseSourceDir.Length + 1);
    }
}