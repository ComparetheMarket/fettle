﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Fettle.Tests.Core.EndToEnd
{
    class Happy_path_mutating_nunit_tests : Contexts.EndToEnd
    {
        public Happy_path_mutating_nunit_tests()
        {
            Given_an_app_which_has_nunit_tests();

            When_mutation_testing_the_app();
        }

        [Test]
        public void Then_mutation_testing_is_successful()
        {
            Assert.That(MutationTestResult.Errors, Is.Empty);
        }

        [Test]
        public void Then_the_expected_surviving_mutants_are_returned()
        {
            Assert.That(MutationTestResult.SurvivingMutants.Count, Is.EqualTo(11),
                string.Join(Environment.NewLine, MutationTestResult.SurvivingMutants.Select(sm =>
                    $"{Path.GetFileName(sm.SourceFilePath)}:{sm.SourceLine} \"{sm.OriginalLine}\" => \"{sm.MutatedLine}\"")));

            Assert.That(MutationTestResult.SurvivingMutants.All(sm => sm.SourceFilePath.EndsWith(@"PartiallyTestedNumberComparison.cs")));

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 7 &&
                      sm.OriginalLine.EndsWith("return a > 0;") &&
                      sm.MutatedLine.EndsWith("return a >= 0;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 12 &&
                      sm.OriginalLine.EndsWith("return a < 0;") &&
                      sm.MutatedLine.EndsWith("return a <= 0;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 17 &&
                      sm.OriginalLine.EndsWith("return a == 0;") &&
                      sm.MutatedLine.EndsWith("return a != 0;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 22 &&
                      sm.OriginalLine.EndsWith("return a == 0 && b == 0;") &&
                      sm.MutatedLine.EndsWith("return a == 0 || b == 0;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 27 &&
                      sm.OriginalLine.EndsWith("return a + b;") &&
                      sm.MutatedLine.EndsWith("return a - b;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 32 &&
                      sm.OriginalLine.EndsWith("return ++a;") &&
                      sm.MutatedLine.EndsWith("return --a;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 37 &&
                      sm.OriginalLine.EndsWith("a++;") &&
                      sm.MutatedLine.EndsWith("a--;")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                sm => sm.SourceLine == 43 &&
                      sm.OriginalLine.EndsWith("IsPositive(n) ? \"positive\" : \"negative\";") &&
                      sm.MutatedLine.EndsWith("IsPositive(n) ? \"negative\" : \"positive\";")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                    sm => sm.SourceLine == 79 &&
                          sm.OriginalLine.EndsWith("IsPositive(n) ? \"positive\" : \"negative\";") &&
                          sm.MutatedLine.EndsWith("IsPositive(n) ? \"negative\" : \"positive\";")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                    sm => sm.SourceLine == 84 &&
                          sm.OriginalLine.EndsWith("get { return 40 + 2; }") &&
                          sm.MutatedLine.EndsWith("get { return 40 - 2; }")),
                Is.Not.Null);

            Assert.That(MutationTestResult.SurvivingMutants.SingleOrDefault(
                    sm => sm.SourceLine == 89 &&
                          sm.OriginalLine.EndsWith("if (b)") &&
                          sm.MutatedLine.EndsWith("if (!(b))")),
                Is.Not.Null);
        }
    }
}
