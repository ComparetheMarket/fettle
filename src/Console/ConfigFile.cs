using Fettle.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fettle.Console
{
    internal static class ConfigFile
    {
        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class InternalConfigRepresentation
        {
            public string Solution { get; set; }
            public string[] ProjectFilters { get; set; }
            public string[] TestProjectFilters { get; set; }
            public string[] SourceFileFilters { get; set; }
        
            public Config ToConfig()
            {
                return new Config
                {
                    SolutionFilePath = Solution,
                    ProjectFilters = ProjectFilters,
                    TestProjectFilters = TestProjectFilters,
                    SourceFileFilters = SourceFileFilters
                };
            }
        }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore MemberCanBePrivate.Local        
        // ReSharper restore ClassNeverInstantiated.Local

        public static Config Parse(string fileContents)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var result = deserializer.Deserialize<InternalConfigRepresentation>(fileContents);
            return result != null ? result.ToConfig() : new Config();
        }
    }
}
