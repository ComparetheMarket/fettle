using System.Collections.Generic;
using System.Linq;
using Fettle.Core;

namespace Fettle.Tests.Core.Contexts
{
    class SpyEventListener : IEventListener
    {
        private readonly List<string> begunFiles = new List<string>();
        public IReadOnlyList<string> BegunFiles => begunFiles;

        private readonly List<string> begunMembers = new List<string>();
        public IReadOnlyList<string> BegunMembers => begunMembers;
        
        public bool HaveAnyFilesBegun => begunFiles.Any();
        public bool HaveAnyMembersBegun => begunMembers.Any();
        public bool HaveAnySyntaxNodesBegun { get; private set; }
        public bool HaveAnyFilesEnded { get; private set; }
        public bool HaveAnyMutantsSurvived { get; private set; }
        public bool HaveAnyMutantsBeenKilled { get; private set; }
        public bool HaveAnyMutantsBeenSkipped { get; private set; }

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total)
        {
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
            begunFiles.Add(filePath);
        }

        public void MemberMutating(string name)
        {
            begunMembers.Add(name);    
        }

        public void SyntaxNodeMutating(int index, int total)
        {
            HaveAnySyntaxNodesBegun = true;
        }

        public void MutantSurvived(Mutant survivingMutant)
        {
            HaveAnyMutantsSurvived = true;
        }

        public void MutantKilled(Mutant killedMutant)
        {
            HaveAnyMutantsBeenKilled = true;
        }

        public void MutantSkipped(Mutant skippedMutant, string reason)
        {
            HaveAnyMutantsBeenSkipped = true;
        }

        public void EndMutationOfFile(string filePath)
        {
            HaveAnyFilesEnded = true;
        }
    }
}