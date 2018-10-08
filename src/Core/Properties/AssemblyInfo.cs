using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("Core")]

[assembly: InternalsVisibleTo("Fettle.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Make internals visible to Moq for testing