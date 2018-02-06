using System;
using System.Text.RegularExpressions;
using Fettle.Core;

namespace Fettle.Console
{
    internal class VerboseEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;
        private string baseSourceDir;
        private bool anyEventsForFile;

        public VerboseEventListener(IOutputWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
            baseSourceDir = baseSourceDirectory;

            outputWriter.Write(Environment.NewLine);
            outputWriter.Write($"Looking in {ToRelativePath(filePath)} ({index+1}/{total}):");

            anyEventsForFile = false;
        }

        public void MethodMutating(string name)
        {
            var shortName = Regex.Match(name, @"\s.*\.(.*)\(").Groups[1].Value.Replace("::", ".");
            outputWriter.Write(Environment.NewLine);
            outputWriter.Write($"  Found method: {shortName}\t");

            anyEventsForFile = true;
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
            if (anyEventsForFile)
            {
                outputWriter.WriteLine("");
            }
            else
            {
                outputWriter.Write(Environment.NewLine);
                outputWriter.Write("  Nothing found to mutate.");
            }
        }

        private string ToRelativePath(string filePath)
        {
            return filePath.Substring(baseSourceDir.Length + 1);
        }
    }
}