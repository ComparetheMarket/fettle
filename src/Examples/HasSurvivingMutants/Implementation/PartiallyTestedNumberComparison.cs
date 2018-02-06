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
    }
}
