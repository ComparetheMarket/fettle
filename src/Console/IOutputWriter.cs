namespace Fettle.Console
{
    internal interface IOutputWriter
    {
        void Write(string output);
        void WriteLine(string output);
        void WriteFailureLine(string output);
        void WriteWarningLine(string output);
        void WriteSuccessLine(string output);
        void WriteDebugLine(string output);
        
        void ClearLine();
        
        void MoveUp(int numLines);
    }
}