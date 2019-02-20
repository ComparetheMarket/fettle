using System;
using System.Linq;
using Fettle.Console;
using Fettle.Core;
using Fettle.Tests.Console.Contexts;
using NUnit.Framework;

namespace Fettle.Tests.Console.ImplementationDetails
{
    [TestFixture]
    public class DefaultEventListener_Tests
    {
        [TestFixture]
        public class When_performing_mutation_testing
        {
            private readonly SpyOutputWriter spyOutputWriter;

            public When_performing_mutation_testing()
            {
                spyOutputWriter = new SpyOutputWriter();
                var listener = new DefaultEventListener(spyOutputWriter);
                
                listener.BeginMutationOfFile(@"c:\some-project\src\Class1.cs", @"c:\some-project", 0, 2);
                listener.MemberMutating("System.Void SomeProject.Class1::MethodA(System.Int32)");
                listener.SyntaxNodeMutating(0, 2);
                listener.SyntaxNodeMutating(1, 2);
                listener.MemberMutating("System.Int32 SomeProject.Class1::PropertyA");
                listener.SyntaxNodeMutating(0, 2);
                listener.SyntaxNodeMutating(1, 2);
                listener.EndMutationOfFile(@"src\Class1.cs");

                listener.BeginMutationOfFile(@"c:\some-project\src\Class2.cs", @"c:\some-project", 1, 2);
                listener.MemberMutating("System.Int32[] SomeProject.SomeOtherNamespace.Class2::MethodA()");
                listener.SyntaxNodeMutating(0, 2);
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
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.Contains(@"src\Class1.cs"));
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.Contains(@"src\Class2.cs"));
            }

            [Test]
            public void Then_output_shows_a_simplified_member_name_when_each_member_is_being_mutated()
            {
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.Contains("Found: Class1.MethodA"));
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.Contains("Found: Class1.PropertyA"));
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.Contains("Found: Class2.MethodA"));
            }

            [Test]
            public void Then_output_shows_progress_when_each_syntax_node_is_being_mutated()
            {
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.Exactly(6).EqualTo("."));
            }
            
            [Test]
            public void Then_output_indicates_when_a_mutant_survives()
            {
                Assert.That(spyOutputWriter.WrittenLineSegments, Has.One.EqualTo("✗"));
            }
        }

        [TestFixture]
        public class When_a_mutant_is_killed_or_skipped_during_mutation_testing
        {
            private readonly SpyOutputWriter spyOutputWriter;

            public When_a_mutant_is_killed_or_skipped_during_mutation_testing()
            {
                spyOutputWriter = new SpyOutputWriter();
                var listener = new DefaultEventListener(spyOutputWriter);

                listener.MutantKilled(new Mutant(), "Expected true but was false");
                listener.MutantSkipped(new Mutant(), "reason");
            }

            [Test]
            public void Then_no_output_is_produced()
            {
                Assert.That(spyOutputWriter.WrittenLineSegments
                        .Concat(spyOutputWriter.WrittenNormalLines)
                        .Concat(spyOutputWriter.WrittenFailureLines)
                        .Concat(spyOutputWriter.WrittenSuccessLines)
                        .Concat(spyOutputWriter.WrittenWarningLines)
                        .ToArray(),
                    Has.Length.Zero);
            }
        }

        [TestFixture]
        public class When_performing_mutation_testing_but_nothing_found_to_mutate
        {
            private readonly SpyOutputWriter spyOutputWriter;

            public When_performing_mutation_testing_but_nothing_found_to_mutate()
            {
                spyOutputWriter = new SpyOutputWriter();
                var listener = new DefaultEventListener(spyOutputWriter);

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
                var listener = new DefaultEventListener(spyOutputWriter);

                for (var testIndex = 0; testIndex < 15; ++testIndex)
                {
                    listener.BeginCoverageAnalysisOfTestCase($"Tests.Test{testIndex}", testIndex, 15);
                }
            }

            [Test]
            public void Then_progress_is_output_for_every_five_test_cases_analysed_on_a_single_line()
            {
                Assert.That(string.Join("", spyOutputWriter.WrittenLineSegments), Is.EqualTo($"...{Environment.NewLine}"));
            }
        }
    }
}
