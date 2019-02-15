using System;

namespace Fettle.Console
{
    internal class ConsoleOutputWriter : IOutputWriter
    {
        public void Write(string output)
        {
            System.Console.Write(output);
        }

        public void WriteLine(string output)
        {
            System.Console.WriteLine(output);
        }

        public void WriteWarningLine(string output)
        {
            ColourWriteLine(output, ConsoleColor.Yellow);
        }

        public void WriteFailureLine(string output)
        {
            ColourWriteLine(output, ConsoleColor.Red);
        }

        public void WriteSuccessLine(string output)
        {
            ColourWriteLine(output, ConsoleColor.Green);
        }

        public void ClearLine()
        {            
            int currentLineCursor = System.Console.CursorTop;
            System.Console.SetCursorPosition(0, System.Console.CursorTop);
            System.Console.Write(new string(' ', System.Console.WindowWidth)); 
            System.Console.SetCursorPosition(0, currentLineCursor);
        }

        public void MoveUp(int numLines)
        {            
            System.Console.SetCursorPosition(0, System.Console.CursorTop - numLines);
        }
        
        private static void ColourWriteLine(string output, ConsoleColor colour)
        {
            var prevColour = System.Console.ForegroundColor;
            try
            {
                System.Console.ForegroundColor = colour;
                System.Console.WriteLine(output);
            }
            finally
            {
                System.Console.ForegroundColor = prevColour;
            }            
        }
    }
}