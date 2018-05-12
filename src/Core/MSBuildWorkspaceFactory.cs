using System;
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
            var registeredInstance = MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault();
            if (registeredInstance == null)
            {
                MSBuildLocator.RegisterDefaults();
                return;
            }

            MSBuildLocator.RegisterInstance(registeredInstance);
        }

        public static MSBuildWorkspace Create()
        {
            return MSBuildWorkspace.Create(
                new Dictionary<string, string>
                {
                    {"CheckForSystemRuntimeDependency", "true"}
                });
        }
    }
}