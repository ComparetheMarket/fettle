using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Fettle.Core.Internal.NUnit
{
    internal class NUnitTestRunner : ITestRunner
    {
        public TestRunnerResult RunTests(IEnumerable<string> testAssemblyFilePaths, string nunitTestRunnerFilePath, string tempDirectory)
        {
            var resultFilePath = Path.Combine(
                tempDirectory,
                "fettle.nunit-results.xml");

            var nunitProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = nunitTestRunnerFilePath,
                    Arguments = $"{string.Join(" ", testAssemblyFilePaths)} --result:{resultFilePath};format=nunit3 --stoponerror --noheader --trace=off"
                }
            };

            nunitProcess.Start();
            nunitProcess.WaitForExit();

            return NUnitTestResultFile.ParseResultFileContents(
                File.ReadAllText(resultFilePath));
        }
    }
}