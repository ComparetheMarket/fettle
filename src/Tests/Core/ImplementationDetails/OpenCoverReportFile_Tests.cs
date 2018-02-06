using Fettle.Core.Internal;
using NUnit.Framework;

namespace Fettle.Tests.Core.ImplementationDetails
{
    [TestFixture]
    class OpenCoverReportFile_Tests
    {
        [Test]
        public void Type_names_are_decoded()
        {
            var coverageFileContents = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<CoverageSession>
  <Modules>
    <Module>      
      <Classes>
        <Class>          
          <Methods>
            <Method visited=""true"">
              <Name>System.Void ExampleApp.ExampleClass::set_Things(System.Collections.Generic.IList`1&lt;ExampleApp.Thing&gt;)</Name>            
            </Method>            
          </Methods>
        </Class>        
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
";
            var methodCoverage = OpenCoverReportFile.Parse(coverageFileContents);
            
            Assert.That(methodCoverage.IsMethodCovered(
                    "System.Void ExampleApp.ExampleClass::set_Things(System.Collections.Generic.IList`1<ExampleApp.Thing>)"),
                Is.True);
        }

        [Test]
        public void Each_overload_of_a_method_is_treated_as_a_separate_method()
        {            
            var coverageFileContents = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<CoverageSession>
  <Modules>
    <Module>      
      <Classes>
        <Class>          
          <Methods>
            <Method visited=""true"">
              <Name>System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)</Name>            
            </Method>
            <Method visited=""true"">
              <Name>System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Decimal)</Name>            
            </Method>

            <Method visited=""false"">
              <Name>T HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)</Name>            
            </Method>
            <Method visited=""false"">
              <Name>System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Double)</Name>            
            </Method>
          </Methods>
        </Class>        
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
";
            var methodCoverage = OpenCoverReportFile.Parse(coverageFileContents);

            Assert.That(methodCoverage.IsMethodCovered("System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)"), Is.True);
            Assert.That(methodCoverage.IsMethodCovered("System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Decimal)"), Is.True);

            Assert.That(methodCoverage.IsMethodCovered("T HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Int32)"), Is.False);
            Assert.That(methodCoverage.IsMethodCovered("System.Boolean HasSurvivingMutants.Implementation.PartiallyTestedNumberComparison::IsPositive(System.Double)"), Is.False);            
        }

        [Test]
        public void Internal_compiler_generated_methods_are_ignored()
        {
            var coverageFileContents = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<CoverageSession>
  <Modules>
    <Module>      
      <Classes>
        <Class>
          <Methods>
            <Method visited=""true"">
              <Name>System.UInt32 &lt;PrivateImplementationDetails&gt;::ComputeStringHash(System.String)</Name>            
            </Method>
            <Method visited=""true"">
              <Name>System.UInt32 DummyNamespace.DummyClass::DummyMethod(System.String)</Name>            
            </Method>
          </Methods>
        </Class>
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
";
            var methodCoverage = OpenCoverReportFile.Parse(coverageFileContents);
            
            Assert.That(methodCoverage.IsMethodCovered(
                    "System.UInt32 <PrivateImplementationDetails>::ComputeStringHash(System.String)"),
                Is.False);
        }

        [Test]
        public void Duplicates_are_coalesced()
        {
            var coverageFileContents = 
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<CoverageSession>
  <Modules>
    <Module>      
      <Classes>
        <Class>
          <Methods>
            <Method visited=""true"">
              <Name>System.UInt32 DummyNamespace.DummyClass::DummyMethod(System.String)</Name>
            </Method>
            <Method visited=""false"">
              <Name>System.UInt32 DummyNamespace.DummyClass::DummyMethod(System.String)</Name>
            </Method>
          </Methods>
        </Class>
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
";
            var methodCoverage = OpenCoverReportFile.Parse(coverageFileContents);
            
            Assert.That(methodCoverage.IsMethodCovered(
                    "System.UInt32 DummyNamespace.DummyClass::DummyMethod(System.String)"),
                Is.True);
        }
    }
}
