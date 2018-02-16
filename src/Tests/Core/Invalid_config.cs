using System;
using Fettle.Core;
using NUnit.Framework;

namespace Fettle.Tests.Core
{    
    [TestFixtureSource("TestCases")]
    class Invalid_config : Contexts.InvalidConfig
    {
        private static readonly object[] TestCases =
        {
            new object[] { new Func<Config,Config>(WithNonExistentSolutionFile) },
            new object[] { new Func<Config,Config>(WithNonExistentTestAssembly) },
            new object[] { new Func<Config,Config>(WithNonExistentCoverageReport) },

            new object[] { new Func<Config,Config>(WithNoSolutionFile) },
            new object[] { new Func<Config,Config>(WithNoTestAssemblies) },
        };

        public Invalid_config(Func<Config, Config> modifier)
        {
            Given_an_app_to_be_mutation_tested();
            Given_config_is_invalid(modifier);

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_an_error_is_returned()
        {
            Assert.That(Result.Errors, Is.Not.Null);
            Assert.That(Result.Errors.Count, Is.EqualTo(1),
                string.Join(Environment.NewLine, Result.Errors));
        }
    }
}
