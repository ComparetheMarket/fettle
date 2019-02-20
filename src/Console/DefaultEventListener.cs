using System;
using Fettle.Core;

namespace Fettle.Console
{
    internal class DefaultEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;
        private string baseSourceDir;
        private bool anyMutationsMadeForCurrentFile;

        public DefaultEventListener(IOutputWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total)
        {
            if (index % 5 == 0)
            {
                outputWriter.Write(".");
            }

            var isLastOne = index == total - 1;
            if (isLastOne)
            {
                outputWriter.Write(Environment.NewLine);
            }
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
            baseSourceDir = baseSourceDirectory;

            outputWriter.Write(Environment.NewLine);
            outputWriter.Write($"Looking in {ToRelativePath(filePath)} ({index+1}/{total}):");

            anyMutationsMadeForCurrentFile = false;
        }

        public void MemberMutating(string name)
        {
            outputWriter.Write(Environment.NewLine);
            outputWriter.Write($"  Found: {MemberName.Simplify(name)}\t");

            anyMutationsMadeForCurrentFile = true;
        }

        public void SyntaxNodeMutating(int index, int total)
        {
            outputWriter.Write(".");
        }

        public void MutantSurvived(Mutant survivingMutant)
        {
            outputWriter.Write("✗");
        }

        public void MutantKilled(Mutant killedMutant)
        {
        }

        public void MutantSkipped(Mutant skippedMutant, string reason)
        {
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