using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal class MutationJobList
    {
        private readonly Dictionary<MutationJobMetadata, MutationJob> jobsWithMetadata
            = new Dictionary<MutationJobMetadata, MutationJob>();

        public void AddJob(MutationJob job, MutationJobMetadata metadata)
        {
            jobsWithMetadata.Add(metadata, job);
        }

        public async Task<IEnumerable<SurvivingMutant>> RunAll(
            Config config, 
            ITestRunner testRunner, 
            string baseTempDirectory, 
            IEventListener eventListener)
        {
            var survivingMutants = new List<SurvivingMutant>();
            var survivingSyntaxNodes = new HashSet<SyntaxNode>();
            var reportedMembers = new HashSet<string>();

            var jobsBySourceFile = jobsWithMetadata.GroupBy(x => x.Key.SourceFilePath, x => x).ToArray();

            for (var sourceFileIndex = 0; sourceFileIndex < jobsBySourceFile.Length; sourceFileIndex++)
            {
                var jobsForSourceFile = jobsBySourceFile[sourceFileIndex];
                var sourceFilePath = jobsForSourceFile.Key;

                eventListener.BeginMutationOfFile(
                    sourceFilePath, Path.GetDirectoryName(config.SolutionFilePath), sourceFileIndex, jobsBySourceFile.Length);

                foreach (var mutationJobAndMetadata in jobsForSourceFile)
                {
                    var metadata = mutationJobAndMetadata.Key;
                    var mutationJob = mutationJobAndMetadata.Value;

                    var syntaxNodeAlreadyHadSurvivingMutant = survivingSyntaxNodes.Contains(mutationJob.OriginalNode);
                    if (syntaxNodeAlreadyHadSurvivingMutant)
                    {
                        continue;
                    }

                    if (!reportedMembers.Contains(metadata.MemberName))
                    {
                        eventListener.MemberMutating(metadata.MemberName);
                        reportedMembers.Add(metadata.MemberName);
                    }
                    eventListener.SyntaxNodeMutating(metadata.SyntaxNodeIndex, metadata.SyntaxNodesTotal);

                    var survivingMutant = await mutationJob.Run(testRunner, baseTempDirectory, eventListener);
                    if (survivingMutant != null)
                    {
                        survivingMutants.Add(survivingMutant);

                        survivingSyntaxNodes.Add(mutationJob.OriginalNode);
                        eventListener.MutantSurvived(survivingMutant);
                    }
                }

                eventListener.EndMutationOfFile(sourceFilePath);
            }

            return survivingMutants;
        }
    }
}