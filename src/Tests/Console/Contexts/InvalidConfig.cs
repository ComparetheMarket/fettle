﻿using Fettle.Core;

namespace Fettle.Tests.Console.Contexts
{
    class InvalidConfig : Default
    {
        protected static Config WithNonExistentSolutionFile(Config config)
        {
            config.SolutionFilePath = "non-existent-file";
            return config;
        }
        
        protected static Config WithNonExistentTestAssembly(Config config)
        {
            config.TestAssemblyFilePaths = new[] { "non-existent-file" };
            return config;
        }
        
        protected static Config WithNoTestAssemblies(Config config)
        {
            config.TestAssemblyFilePaths = new string[0];
            return config;
        }

        protected static Config WithNoSolutionFile(Config config)
        {
            config.SolutionFilePath = null;
            return config;
        }
    }
}