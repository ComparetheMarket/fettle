using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fettle.Core.Internal.RoslynExtensions;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal
{
    internal class MutationJobList
    {
        private readonly Config config;

        private readonly Dictionary<MutationJobMetadata, MutationJob> jobsWithMetadata
            = new Dictionary<MutationJobMetadata, MutationJob>();

        private MutationJobList(Config config)
        {
            this.config = config;
        }

        public static async Task<MutationJobList> Create(Config config, ICoverageAnalysisResult coverageAnalysisResult)
        {
            var jobs = new MutationJobList(config);
            
            var documentsToMutate = await config.FindMutatableDocuments();

            for (var classIndex = 0; classIndex < documentsToMutate.Length; classIndex++)
            {
                var documentToMutate = documentsToMutate[classIndex];
                var documentSyntaxRoot = await documentToMutate.GetSyntaxRootAsync();
                var documentSemanticModel = await documentToMutate.GetSemanticModelAsync();

                var nodesToMutate = documentSyntaxRoot.DescendantNodes().ToArray();
                var isIgnoring = false;

                for (var nodeIndex = 0; nodeIndex < nodesToMutate.Length; nodeIndex++)
                {
                    var nodeToMutate = nodesToMutate[nodeIndex];

                    var memberName = nodeToMutate.NameOfContainingMember(documentSemanticModel);
                    if (memberName == null)
                    {
                        // The node is not within a member (e.g. it's a class or namespace declaration)
                        // Or, the node is within a member, but the member is not one Fettle supports.
                        // Either way, there is no code to mutate.
                        continue;
                    }

                    if (coverageAnalysisResult != null &&
                        !coverageAnalysisResult.IsMemberCovered(memberName))
                    {
                        continue;
                    }

                    if (Ignoring.NodeHasBeginIgnoreComment(nodeToMutate)) isIgnoring = true;
                    else if (Ignoring.NodeHasEndIgnoreComment(nodeToMutate)) isIgnoring = false;

                    if (isIgnoring)
                    {
                        continue;
                    }

                    foreach (var mutator in nodeToMutate.SupportedMutators())
                    {
                        var job = new MutationJob(
                            documentSyntaxRoot,
                            nodeToMutate,
                            documentToMutate,
                            memberName,
                            config,
                            mutator,
                            coverageAnalysisResult);

                        var jobMetadata = new MutationJobMetadata
                        {
                            SourceFilePath = documentToMutate.FilePath,
                            SourceFileIndex = classIndex,
                            SourceFilesTotal = documentsToMutate.Length,

                            MemberName = memberName,
                                
                            SyntaxNodeIndex = nodeIndex,
                            SyntaxNodesTotal = nodesToMutate.Length
                        };

                        jobs.AddJob(job, jobMetadata);
                    }
                }
            }

            return jobs;
        }

        private void AddJob(MutationJob job, MutationJobMetadata metadata)
        {
            jobsWithMetadata.Add(metadata, job);
        }

        public async Task<IEnumerable<SurvivingMutant>> RunAll( 
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