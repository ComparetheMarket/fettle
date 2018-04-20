namespace HasSurvivingMutants_452.Implementation
{
    public class PartiallyTestedValueTupleComparison
    {
        public static (int, string) TupleExistsAndHasAValue((bool Exists, string Value) tuple)
        {
            return (tuple.Exists ? 0 : 1, string.IsNullOrEmpty(tuple.Value) ? string.Empty : tuple.Value);
        }
    }
}
