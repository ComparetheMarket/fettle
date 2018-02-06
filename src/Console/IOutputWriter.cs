namespace Fettle.Console
{
    internal interface IOutputWriter
    {
        void Write(string output);
        void WriteLine(string output);
        void WriteFailureLine(string output);
        void WriteSuccessLine(string output);
        
        void ClearLine();
        
        void MoveUp(int numLines);
    }
}