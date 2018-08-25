using System;
using System.Linq;
using System.Text.RegularExpressions;
using Fettle.Core;

namespace Fettle.Console
{
    internal class VerboseEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;
        private string baseSourceDir;
        private bool anyMutationsMadeForCurrentFile;

        public VerboseEventListener(IOutputWriter outputWriter)
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
            var tokensInReverseOrder = name.Split(new[] {"::"}, StringSplitOptions.None).Reverse().ToArray();
            var memberNameWithoutParens = tokensInReverseOrder.First().Split('(').First();
            var className = tokensInReverseOrder.Skip(1).First().Split('.').Last();

            outputWriter.Write(Environment.NewLine);
            outputWriter.Write($"  Found: {className}.{memberNameWithoutParens}\t");

            anyMutationsMadeForCurrentFile = true;
        }

        public void SyntaxNodeMutating(int index, int total)
        {
            outputWriter.Write(".");
        }

        public void MutantSurvived(SurvivingMutant survivingMutant)
        {
            outputWriter.Write("✗");
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