using Fettle.Core;

namespace Fettle.Tests.Console.Contexts
{
    class InvalidConfig : Default
    {
        protected static Config WithNonExistentSolutionFile(Config config)
        {
            config.SolutionFilePath = "non-existent-file";
            return config;
        }
        
        protected static Config WithNullTestProject(Config config)
        {
            config.TestProjectFilters = new []
            {
                "HasSurvivingMutants.Tests",
                null
            };
            return config;
        }
        
        protected static Config WithNullProjectFilter(Config config)
        {
            config.ProjectFilters = new [] { "Implementation", null};
            return config;
        }
        
        protected static Config WithNullSourceFileFilter(Config config)
        {
            config.SourceFileFilters = new [] { "Implementation/*", null};
            return config;
        }

        protected static Config WithNoTestProjects(Config config)
        {
            config.TestProjectFilters = new string[0];
            return config;
        }

        protected static Config WithNoSolutionFile(Config config)
        {
            config.SolutionFilePath = null;
            return config;
        }
    }
}