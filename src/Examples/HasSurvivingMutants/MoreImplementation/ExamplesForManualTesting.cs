using System;

// Example class which contains some of the more unusual types of members available in C#.
// It exists purely for manual testing.
//
// Uncomment class below, compile in Debug, and run:
//  src\Console\bin\Debug\Fettle.Console.exe -c .\fettle.config.example2.yml --skipcoverageanalysis
//

namespace HasSurvivingMutants.MoreImplementation
{
 /*
    public class ExamplesForManualTesting
    {
        private event EventHandler<EventArgs> someEvent;
        private int wibble;
        private int[] arr = new int[100];

        public ExamplesForManualTesting()
        {
            wibble++;
        }

        ~ExamplesForManualTesting()
        {
            wibble--;
        }

        public event EventHandler<EventArgs> SomeEvent
        {
            add
            {
                if (value == null) wibble++;
                someEvent += value;
            }
            remove { someEvent -= value; }
        }

        public int this[int i]
        {
            get { return arr[i]; }
            set { arr[i] = value + 1; }
        }

        public class DummyClass
        {
            public int x, y;

            public static DummyClass operator *(DummyClass a, DummyClass b)
            {
                return new DummyClass { x = a.x + b.x, y = a.y + b.y };
            }
            
            public static implicit operator string(DummyClass d)
            {
                return d == null ? "" : d.x.ToString();
            }
        }
    }
*/
}
