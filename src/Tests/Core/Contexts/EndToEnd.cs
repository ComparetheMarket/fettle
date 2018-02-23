using System.Collections.Generic;
using System.IO;
using Fettle.Core;
using Moq;
using NUnit.Framework;

namespace Fettle.Tests.Core.Contexts
{
    class EndToEnd
    {
        private readonly Mock<IMethodCoverage> mockMethodCoverage = new Mock<IMethodCoverage>();

        private readonly Config config = new Config
        {
            ProjectFilters = new []{ "HasSurvivingMutants.Implementation" },
            SourceFileFilters = new [] { @"Implementation\*" }
        };

        protected Result Result { get; private set; }
        
        protected void Given_an_app_which_has_gaps_in_its_tests()
        {
            var baseDir = Path.Combine(TestContext.CurrentContext.TestDirectory,
                "..", "..", "..", "Examples", "HasSurvivingMutants");

            config.SolutionFilePath = Path.Combine(baseDir, "HasSurvivingMutants.sln");

            var binDir = Path.Combine(baseDir, "Tests", "bin", BuildConfig.AsString);

            config.TestAssemblyFilePaths = new[]
            {
                Path.Combine(binDir, "HasSurvivingMutants.Tests.dll")
            };

            SetupMockMethodCoverage();
        }

        private void SetupMockMethodCoverage()
        {
            var methodsAndTestsThatCoverThem = new Dictionary<string, string[]>
            {
                {
                    "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::IsPositive()"}
                },
                {
                    "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsNegative(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::IsNegative()"}
                },
                {
                    "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsZero(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::IsZero()"}
                },
                {
                    "System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AreBothZero(System.Int32,System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::AreBothZero()"}
                },
                {
                    "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Sum(System.Int32,System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::Sum()"}
                },
                {
                    "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Preincrement(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::Preincrement()"}
                },
                {
                    "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::Postincrement(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::Postincrement()"}
                },
                {
                    "System.String HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::PositiveOrNegative(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::PositiveOrNegative()"}
                },                
                {
                    "System.Int32 HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::AddNumbers_should_be_ignoredAddNumbers_should_be_ignored(System.Int32)",
                    new[] {"System.Void HasSurvivingMutants.Tests.PartialNumberComparisonTests::Methods_with_ignored_statements()"}
                }                
            };
            mockMethodCoverage
                .Setup(x => x.TestsThatCoverMethod(It.IsAny<string>()))
                .Returns<string>(methodName => 
                    methodsAndTestsThatCoverThem.TryGetValue(methodName, out var results) ? results : new string[0]);
        }

        protected void When_mutation_testing_the_app()
        {
            Result = new MutationTestRunner()
                .Run(config, mockMethodCoverage.Object)
                .Result;
        }
    }
}