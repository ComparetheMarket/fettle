namespace Fettle.Core
{
    public interface IEventListener
    {
        void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total);
        void MethodMutating(string name);
        void SyntaxNodeMutating(int index, int total);
        void MutantSurvived(SurvivingMutant survivingMutant);
        void EndMutationOfFile(string filePath);
    }
}