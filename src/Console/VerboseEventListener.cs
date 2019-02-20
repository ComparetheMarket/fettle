using System;
using System.Diagnostics;
using System.Linq;
using Fettle.Core;

namespace Fettle.Console
{
    internal class VerboseEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;
        private bool anyMutationsMadeForCurrentFile;
        private string baseSourceDir;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public VerboseEventListener(IOutputWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total)
        {
            outputWriter.WriteLine($"Analysing coverage: {fullTestName}");
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
            baseSourceDir = baseSourceDirectory;
            anyMutationsMadeForCurrentFile = false;

            outputWriter.WriteLine($"{Indentation(0)}Found file: {ToRelativePath(filePath)}");
        }

        public void MemberMutating(string name)
        {
            anyMutationsMadeForCurrentFile = true;

            outputWriter.WriteLine($"{Indentation(1)}Found member {MemberName.Simplify(name)}");
        }

        public void SyntaxNodeMutating(int index, int total)
        {
            outputWriter.WriteLine($"{Indentation(2)}Mutating syntax node {index+1}");

            stopwatch.Restart();
        }

        public void MutantSurvived(Mutant survivingMutant)
        {
            stopwatch.Stop();

            outputWriter.WriteLine($"{Indentation(3)}Mutated in {FormatMutationDuration(stopwatch.Elapsed)}");
            outputWriter.WriteLine($"{Indentation(3)}Line {survivingMutant.SourceLine}:");
            outputWriter.WriteLine($"{Indentation(4)}Original: {survivingMutant.OriginalLine}");
            outputWriter.WriteLine($"{Indentation(4)}Mutated:  {survivingMutant.MutatedLine}");
            outputWriter.WriteFailureLine($"{Indentation(3)}Mutant SURVIVED");
        }

        public void MutantKilled(Mutant killedMutant, string testFailureDescription)
        {
            stopwatch.Stop();

            outputWriter.WriteLine($"{Indentation(3)}Mutated in {FormatMutationDuration(stopwatch.Elapsed)}");
            outputWriter.WriteLine($"{Indentation(3)}Line {killedMutant.SourceLine}:");
            outputWriter.WriteLine($"{Indentation(4)}Original: {killedMutant.OriginalLine}");
            outputWriter.WriteLine($"{Indentation(4)}Mutated:  {killedMutant.MutatedLine}");
            outputWriter.WriteSuccessLine($"{Indentation(3)}Mutant killed because of test failure:");
            outputWriter.WriteDebugLine($"{FormatTestFailureDescription(testFailureDescription)}");
        }

        public void MutantSkipped(Mutant skippedMutant, string reason)
        {
            stopwatch.Stop();

            outputWriter.WriteLine($"{Indentation(3)}Mutation skipped, reason: {reason}");
            outputWriter.WriteLine($"{Indentation(3)}Line {skippedMutant.SourceLine}:");
            outputWriter.WriteLine($"{Indentation(4)}Original: {skippedMutant.OriginalLine}");
            outputWriter.WriteLine($"{Indentation(4)}Mutated:  {skippedMutant.MutatedLine}");
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
                outputWriter.Write($"{Indentation(0)}Nothing found to mutate.");
            }
        }

        private string FormatTestFailureDescription(string testFailureDescription)
        {
            var modifiedLines = testFailureDescription.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(line => line.TrimStart())
                                                      .Select(line => $"{Indentation(4)}{line}");
            return string.Join(Environment.NewLine, modifiedLines);
        }

        private static string FormatMutationDuration(TimeSpan duration) => $"{duration.TotalMilliseconds:.}ms";

        private static string Indentation(int level) => new string(Enumerable.Repeat(' ', level*3).ToArray());

        private string ToRelativePath(string filePath) => filePath.Substring(baseSourceDir.Length + 1);
    }
}