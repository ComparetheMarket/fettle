namespace HasSurvivingMutants.Implementation
{
    public static class PartiallyTestedNumberComparison
    {
        public static bool IsPositive(int a)
        {
            return a > 0;
        }

        public static bool IsNegative(int a)
        {
            return a < 0;
        }

        public static bool IsZero(int a)
        {
            return a == 0;
        }

        public static bool AreBothZero(int a, int b)
        {
            return a == 0 && b == 0;
        }

        public static int Sum(int a, int b)
        {
            return a + b;
        }

        public static int Preincrement(int a)
        {
            return ++a;
        }
        
        public static int Postincrement(int a)
        {
            a++;
            return a;
        }

        public static string PositiveOrNegative(int n)
        {
            return IsPositive(n) ? "positive" : "negative";
        }

        public static int AddNumbers_should_be_ignored(int a)
        {
            // Fettle: begin ignore
            var result = a > 0 ? a + 1 : a;
            // Fettle: end ignore

            //fettle: begin ignore   
            result = result < 0 ?
                result - 1 :
                result;
            //fettle: end ignore   

            return result;
        }

        // An empty method.
        // This exists to check that coverage analysis and mutation testing can handle empty methods.
        public static void EmptyMethod()
        {
        }

        //
        // This exists to show that Fettle can compile projects that use the new tuples syntax.
        //
        // In .NET <= 4.7 this requires an additional reference (System.ValueTuple) which is also present in Fettle itself.
        // There was an issue where Fettle picked its own version instead of the project's one, causing a compilation error
        // during coverage analysis.
        public static (int, string) MethodThatReturnsATuple()
        {
            return (0, "");
        }
    }
}
