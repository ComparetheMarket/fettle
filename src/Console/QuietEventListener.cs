﻿using Fettle.Core;

namespace Fettle.Console
{
    internal class QuietEventListener : IEventListener
    {
        public QuietEventListener(IOutputWriter _) {}

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total) { }
        public void MemberCoveredByTests(string memberName) { }
        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total) { }
        public void MemberMutating(string name) { }
        public void SyntaxNodeMutating(int index, int total) { }
        public void MutantSurvived(Mutant survivingMutant) { }
        public void MutantKilled(Mutant killedMutant, string testFailureDescription) { }
        public void MutantSkipped(Mutant skippedMutant, string reason) { }

        public void EndMutationOfFile(string filePath) { }
    }
}