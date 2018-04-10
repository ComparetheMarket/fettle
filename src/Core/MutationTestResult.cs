using System.Collections.Generic;
using System.Linq;

namespace Fettle.Core
{
    public class MutationTestResult
    {
        public IReadOnlyCollection<SurvivingMutant> SurvivingMutants { get; private set; } = new List<SurvivingMutant>();
        public IReadOnlyCollection<string> Errors { get; private set; } = new List<string>();

        public MutationTestResult WithErrors(IEnumerable<string> errors)
        {
            Errors = errors.ToList();
            return this;
        }

        public MutationTestResult WithError(string error)
        {
            Errors = new List<string>{ error };
            return this;
        }

        public MutationTestResult WithSurvivingMutants(IEnumerable<SurvivingMutant> survivingMutants)
        {
            SurvivingMutants = survivingMutants.ToList();
            return this;
        }
    }
}