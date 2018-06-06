using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Xml.Linq;
using NUnit.Engine;

namespace Fettle.Core.Internal.NUnit
{
    internal sealed class NUnitCoverageCollector : ITestEventListener, IDisposable
    {
        private readonly IDictionary<long, string> methodIdsToNames;
        private readonly Action<string, int> onAnalysingTestCase;
        private int numTestCasesExecuted;
        private MemoryMappedFile memoryMappedFile;
        private readonly byte[] zeroedMemoryMapBuffer;
        private readonly List<string> testMethodsInCurrentFixture = new List<string>();

        private readonly Dictionary<string, ImmutableHashSet<string>> methodsAndCoveringTests 
            = new Dictionary<string, ImmutableHashSet<string>>();

        public IReadOnlyDictionary<string, ImmutableHashSet<string>> MethodsAndCoveringTests { get; }

        private string MethodIdToMethodName(long methodId) => methodIdsToNames[methodId];

        private int Capacity => methodIdsToNames.Count;

        public NUnitCoverageCollector(IDictionary<long, string> methodIdsToNames, Action<string, int> onAnalysingTestCase)
        {
            this.methodIdsToNames = methodIdsToNames;
            this.onAnalysingTestCase = onAnalysingTestCase;

            MethodsAndCoveringTests = new ReadOnlyDictionary<string, ImmutableHashSet<string>>(methodsAndCoveringTests);

            zeroedMemoryMapBuffer = new byte[Capacity];
            //using (var stream = File.Create(@"C:\temp\fettle_coverage.bin"))
            //{
            //    stream.Write(zeroedMemoryMapBuffer, 0, zeroedMemoryMapBuffer.Length);   
            //}
            //memoryMappedFile = MemoryMappedFile.CreateFromFile(@"c:\temp\fettle_coverage.bin", FileMode.Open, @"fettle_coverage");
            memoryMappedFile = MemoryMappedFile.CreateNew("fettle_coverage", Capacity);
            using (var accessor = memoryMappedFile.CreateViewAccessor())
            {
                accessor.WriteArray(0, zeroedMemoryMapBuffer, 0, Capacity);
            }
            var x = ExecutedMethodIds().ToList();
        }

        public void Dispose()
        {
            memoryMappedFile.Dispose();
        }

        public void OnTestEvent(string report)
        {
            if (report.StartsWith("<start-test"))
            {
                // test method starting

                var reportDoc = XDocument.Parse(report);
                var testMethodName = reportDoc.Root.Attribute("fullname").Value;

                testMethodsInCurrentFixture.Add(testMethodName);

                onAnalysingTestCase(testMethodName, numTestCasesExecuted);
                numTestCasesExecuted++;
            }
            else if (report.StartsWith("<test-case"))
            {
                // test method complete

                var reportDoc = XDocument.Parse(report);
                var testMethodName = reportDoc.Root.Attribute("fullname").Value;

                using (var mutex = new System.Threading.Mutex(false, "Global\\fettle_coverage_741CBFBB-EEB3-4E4B-88F9-E885F0EE8ADA"))
                {
                    try
                    {
                        mutex.WaitOne();

                        var executedMethodIds = ExecutedMethodIds().ToList();
                        var executedMethodNames = executedMethodIds
                            .Select(MethodIdToMethodName)
                            .ToList();

                        executedMethodNames.ForEach(methodName => 
                            RecordThatTestCoversMethod(testMethodName, methodName));

                        ClearExecutedMethods(executedMethodIds);
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
                
            }
            else if (report.StartsWith("<test-suite"))
            {
                var reportDoc = XDocument.Parse(report);
                if (reportDoc.Root.Attribute("type").Value == "TestFixture")
                {
                    // test fixture complete
                    using (var mutex = new System.Threading.Mutex(false,
                        "Global\\fettle_coverage_741CBFBB-EEB3-4E4B-88F9-E885F0EE8ADA"))
                    {
                        try
                        {
                            mutex.WaitOne();

                            var executedMethodIds = ExecutedMethodIds().ToList();
                            var executedMethodNames = executedMethodIds
                                .Select(MethodIdToMethodName)
                                .ToList();

                            foreach (var executedMethodName in executedMethodNames)
                            {
                                testMethodsInCurrentFixture.ForEach(testMethodName =>
                                    RecordThatTestCoversMethod(testMethodName, executedMethodName));
                            }

                            testMethodsInCurrentFixture.Clear();

                            ClearExecutedMethods(executedMethodIds);
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
        }

        private void RecordThatTestCoversMethod(string testMethodName, string executedMethodName)
        {
            if (!methodsAndCoveringTests.ContainsKey(executedMethodName))
            {
                methodsAndCoveringTests.Add(executedMethodName, ImmutableHashSet<string>.Empty);
            }

            methodsAndCoveringTests[executedMethodName] =
                methodsAndCoveringTests[executedMethodName].Add(testMethodName);
        }

        private IEnumerable<long> ExecutedMethodIds()
        {
            var result = new List<long>();
            var buffer = new byte[Capacity];

            using (var mutex =
                new System.Threading.Mutex(false, "Global\\fettle_coverage_741CBFBB-EEB3-4E4B-88F9-E885F0EE8ADA"))
            {
                try
                {
                    mutex.WaitOne();

                    using (var file = MemoryMappedFile.OpenExisting("fettle_coverage"))
                    using (var accessor = file.CreateViewAccessor())
                    {
                        accessor.ReadArray(0, buffer, 0, Capacity);
                        //long loc = 0;s
                        //while (loc < methodIdsToNames.Count)
                        //{
                        //    var value = accessor.ReadInt32(loc);
                        //    if (value == 0x000000FF)
                        //    {
                        //        result.Add(loc);
                        //    }

                        //    loc += sizeof(int);
                        //}
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            for (long i = 0; i < buffer.Length; i++)
            {
                var thing = buffer[i];
                if (thing == 0xAA)
                    yield return (long) i;
            }            
        }

        private void ClearExecutedMethods(IReadOnlyCollection<long> executedMethodIds)
        {
            //if (!executedMethodIds.Any())
            //{
            //    return;
            //}

            using (var mutex = new System.Threading.Mutex(false, "Global\\fettle_coverage_741CBFBB-EEB3-4E4B-88F9-E885F0EE8ADA"))
            {
                try
                {
                    mutex.WaitOne();

                    using (var file = MemoryMappedFile.OpenExisting("fettle_coverage"))
                    {
                        //using (var accessor = file.CreateViewAccessor())
                        //{
                        //    accessor.WriteArray(0, zeroedMemoryMapBuffer, 0, Capacity);
                        //    accessor.Flush();
                        //}
                        using (var accessor = file.CreateViewStream(0, 0))
                        {
                            accessor.Write(zeroedMemoryMapBuffer, 0, Capacity);
                            accessor.Flush();
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }

            
                //foreach (var methodId in executedMethodIds)
                //{
                //    using (var accessor = file.CreateViewAccessor(methodId*4, 4))
                //    {
                //        accessor.Write(0, (int)0);
                //    }
                //}
                //using (var accessor = file.CreateViewAccessor())
                //{

                //    ////
                //    //var b = new byte[100];
                //    //accessor.ReadArray(0, b, 0, 100);
                //    /// 


                //    for (long i = 0; i <methodIdsToNames.Count; ++i)
                //    {
                //        accessor.Write(i, 0xAA);
                //    }
                //}
            
        }
    }
}