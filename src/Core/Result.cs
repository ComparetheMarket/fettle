using System.Collections.Generic;
using System.Linq;

namespace Fettle.Core
{
    public class Result
    {
        public IReadOnlyCollection<SurvivingMutant> SurvivingMutants { get; private set; } = new List<SurvivingMutant>();
        public IReadOnlyCollection<string> Errors { get; private set; } = new List<string>();

        public Result WithErrors(IEnumerable<string> errors)
        {
            Errors = errors.ToList();
            return this;
        }

        public Result WithError(string error)
        {
            Errors = new List<string>{ error };
            return this;
        }

        public Result WithSurvivingMutants(IEnumerable<SurvivingMutant> survivingMutants)
        {
            SurvivingMutants = survivingMutants.ToList();
            return this;
        }
    }
}