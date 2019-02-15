using System.Collections.Generic;
using Fettle.Console;

namespace Fettle.Tests.Console.Contexts
{
    class SpyOutputWriter : IOutputWriter
    {
        private readonly List<string> writtenLineSegments = new List<string>();
        private readonly List<string> writtenNormalLines = new List<string>();
        private readonly List<string> writtenFailureLines = new List<string>();
        private readonly List<string> writtenWarningLines = new List<string>();
        private readonly List<string> writtenSuccessLines = new List<string>();

        public IReadOnlyList<string> WrittenLineSegments => writtenLineSegments;
        public IReadOnlyList<string> WrittenNormalLines => writtenNormalLines;
        public IReadOnlyList<string> WrittenWarningLines => writtenWarningLines;
        public IReadOnlyList<string> WrittenFailureLines => writtenFailureLines;
        public IReadOnlyList<string> WrittenSuccessLines => writtenSuccessLines;

        public void Write(string output)
        {
            writtenLineSegments.Add(output);
        }

        public void WriteLine(string output)
        {
            writtenNormalLines.Add(output);
        }

        public void WriteFailureLine(string output)
        {
            writtenFailureLines.Add(output);
        }

        public void WriteWarningLine(string output)
        {
            writtenWarningLines.Add(output);
        }

        public void WriteSuccessLine(string output)
        {
            writtenSuccessLines.Add(output);
        }

        public void ClearLine()
        {
        }

        public void MoveUp(int numLines)
        {
        }
    }
}