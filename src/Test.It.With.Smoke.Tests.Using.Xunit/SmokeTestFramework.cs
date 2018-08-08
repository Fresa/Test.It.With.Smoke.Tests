using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestFramework : TestFramework
    {
        public SmokeTestFramework(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new SmokeTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new SmokeTestExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }
}