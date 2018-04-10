using System;

namespace HasSurvivingMutants.Implementation
{
    public abstract class SkipAbstractMethodMutation
    {
       public abstract void BuildSomething();

        public static int AddTwoNumbers(int a, int b)
        {
            return a + b;
        }
    }
}
