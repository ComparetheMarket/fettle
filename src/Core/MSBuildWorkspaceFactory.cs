using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Fettle.Core
{
    internal static class MSBuildWorkspaceFactory
    {
        static MSBuildWorkspaceFactory()
        {
            MSBuildLocator.RegisterDefaults();
        }

        public static MSBuildWorkspace Create()
        {
            return MSBuildWorkspace.Create(
                new Dictionary<string, string>
                {
                    { "CheckForSystemRuntimeDependency", "true" }
                });
        }
    }
}