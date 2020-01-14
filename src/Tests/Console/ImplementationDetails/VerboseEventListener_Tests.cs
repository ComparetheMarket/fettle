using Fettle.Console;
using Fettle.Core;
using Fettle.Tests.Console.Contexts;
using NUnit.Framework;

namespace Fettle.Tests.Console.ImplementationDetails
{
    [TestFixture]
    public class VerboseEventListener_Tests
    {
        [TestFixture]
        public class When_performing_mutation_testing
        {
            private readonly SpyOutputWriter spyOutputWriter;

            public When_performing_mutation_testing()
            {
                spyOutputWriter = new SpyOutputWriter();
                var listener = new VerboseEventListener(spyOutputWriter);

                var testFailureDescription = 
@"       Expected:
            true
         but was:
            false";


                listener.BeginMutationOfFile(@"c:\some-project\src\Class1.cs", @"c:\some-project", 0, 2);
                listener.MemberMutating("System.Void SomeProject.Class1::MethodA(System.Int32)");
                listener.SyntaxNodeMutating(0, 2);
                listener.SyntaxNodeMutating(1, 2);
                listener.MemberMutating("System.Int32 SomeProject.Class1::PropertyA");
                listener.SyntaxNodeMutating(0, 2);
                listener.SyntaxNodeMutating(1, 2);
                listener.MutantSkipped(
                    new Mutant
                    {
                        OriginalLine = "a == 0",
                        MutatedLine = "a != 0",
                        SourceFilePath = @"c:\some-project\src\Class1.cs",
                        SourceLine = 14
                    }, "filtered out");
                listener.EndMutationOfFile(@"src\Class1.cs");

                listener.BeginMutationOfFile(@"c:\some-project\src\Class2.cs", @"c:\some-project", 1, 2);
                listener.MemberMutating("System.Int32[] SomeProject.SomeOtherNamespace.Class2::MethodA()");
                listener.SyntaxNodeMutating(0, 2);
                listener.MutantKilled(
                    new Mutant
                    {
                        OriginalLine = "a > 0",
                        MutatedLine = "a >= 0",
                        SourceFilePath = @"c:\some-project\src\Class2.cs",
                        SourceLine = 20
                    }, 
                    testFailureDescription);
                listener.SyntaxNodeMutating(1, 2);
                listener.MutantSurvived(
                    new Mutant
                    {
                        OriginalLine = "a && b",
                        MutatedLine = "a || b",
                        SourceFilePath = @"c:\some-project\src\Class2.cs",
                        SourceLine = 23
                    });
                listener.EndMutationOfFile(@"src\Class2.cs");
            }

            [Test]
            public void Then_output_shows_when_each_file_is_being_mutated()
            {
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains(@"src\Class1.cs"));
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains(@"src\Class2.cs"));
            }

            [Test]
            public void Then_output_shows_a_simplified_member_name_when_each_member_is_being_mutated()
            {
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("Class1.MethodA"));
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("Class1.PropertyA"));
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("Class2.MethodA"));
            }

            [Test]
            public void Then_output_shows_when_each_syntax_node_is_being_mutated()
            {
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.Exactly(3).Contains("syntax node").And.Contains("1"));
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.Exactly(3).Contains("syntax node").And.Contains("2"));
            }

            [Test]
            public void Then_output_indicates_when_a_mutant_is_killed()
            {
                Assert.That(spyOutputWriter.WrittenSuccessLines, Has.One.Contains("killed").IgnoreCase);
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("a > 0"));
            }

            [Test]
            public void Then_output_contains_formatted_test_failure_description_when_mutant_is_killed()
            {
                Assert.That(spyOutputWriter.AllOutput, Does.Contain("            Expected:"));
                Assert.That(spyOutputWriter.AllOutput, Does.Contain("            true"));
                Assert.That(spyOutputWriter.AllOutput, Does.Contain("            but was:"));
                Assert.That(spyOutputWriter.AllOutput, Does.Contain("            false"));
            }

            [Test]
            public void Then_output_indicates_when_a_mutant_is_skipped()
            {
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("skipped").IgnoreCase);
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("filtered out").IgnoreCase);
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("a != 0"));
            }

            [Test]
            public void Then_output_indicates_when_a_mutant_survives()
            {
                Assert.That(spyOutputWriter.WrittenFailureLines, Has.One.Contains("survived").IgnoreCase);
                Assert.That(spyOutputWriter.WrittenNormalLines, Has.One.Contains("a && b"));
            }
        }

        [TestFixture]
        public class When_performing_mutation_testing_but_nothing_found_to_mutate
        {
            private readonly SpyOutputWriter spyOutputWriter;

            public When_performing_mutation_testing_but_nothing_found_to_mutate()
            {
                spyOutputWriter = new SpyOutputWriter();
                var listener = new VerboseEventListener(spyOutputWriter);

                listener.BeginMutationOfFile(@"c:\some-project\src\Class1.cs", @"c:\some-project", 0, 2);
                listener.EndMutationOfFile(@"c:\some-project\src\Class1.cs");
            }

            [Test]
            public void Then_output_indicates_that_nothing_was_found_to_mutate()
            {
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.Contains("Nothing found to mutate"));
            }
        }

        [TestFixture]
        public class When_performing_coverage_analysis
        {
            private readonly SpyOutputWriter spyOutputWriter;

            public When_performing_coverage_analysis()
            {
                spyOutputWriter = new SpyOutputWriter();
                var listener = new VerboseEventListener(spyOutputWriter);

                listener.BeginCoverageAnalysisOfTestCase("Tests.TestA", 0, 3);
                listener.BeginCoverageAnalysisOfTestCase("Tests.TestB", 1, 3);
                listener.BeginCoverageAnalysisOfTestCase("Tests.TestC", 2, 3);

                listener.MemberCoveredByTests("example.methodA");
                listener.MemberCoveredByTests("example.methodA");
            }

            [Test]
            public void Then_output_includes_the_distinct_set_of_covered_methods()
            {
                Assert.That(spyOutputWriter.WrittenSuccessLines, Has.One.Contains("example.methodA"));
            }
        }
    }
}
