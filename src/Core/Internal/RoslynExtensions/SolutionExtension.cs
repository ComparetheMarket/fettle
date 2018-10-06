﻿using System.Linq;
using Microsoft.CodeAnalysis;

namespace Fettle.Core.Internal.RoslynExtensions
{
    internal static class SolutionExtension
    {
        public static Document[] MutatableClasses(this Solution solution, Config config)
        {
            var projectsToMutate = solution.Projects
                .Where(p => Filtering.ShouldMutateProject(p, config))
                .ToArray();

            var classesToMutate = projectsToMutate
                .SelectMany(p => p.Documents)
                .Where(c => !c.IsAutomaticallyGenerated())
                .Where(c => Filtering.ShouldMutateDocument(c, config))
                .ToArray();

            return classesToMutate;
        }
    }
}
