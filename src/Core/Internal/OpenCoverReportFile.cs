using System.Collections.Immutable;
using System.Linq;
using System.Xml;

namespace Fettle.Core.Internal
{
    internal static class OpenCoverReportFile
    {
        public static MethodCoverage.MethodCoverage Parse(string fileContents)
        {
            bool IsCompilerGeneratedMethod(string methodName)
            {            
                return methodName == "System.UInt32 <PrivateImplementationDetails>::ComputeStringHash(System.String)";
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(fileContents);

            // ReSharper disable AssignNullToNotNullAttribute
            // ReSharper disable PossibleNullReferenceException

            var coveredMethodNames =
                xmlDocument.SelectNodes(".//Method")
                    .Cast<XmlNode>()
                    .Where(methodNode => bool.Parse(methodNode.Attributes["visited"].Value))
                    .Select(methodNode => methodNode.SelectSingleNode("Name").InnerText)
                    .Where(methodName => !IsCompilerGeneratedMethod(methodName))
                    .Distinct()
                    .ToImmutableHashSet();
            
            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore AssignNullToNotNullAttribute
            
            return new MethodCoverage.MethodCoverage(coveredMethodNames);
        }
    }
}