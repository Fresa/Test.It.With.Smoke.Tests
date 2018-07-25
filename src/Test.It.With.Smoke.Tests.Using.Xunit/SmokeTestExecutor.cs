using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestExecutor : TestFrameworkExecutor<SmokeTestCase>
    {
        public SmokeTestExecutor(AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink) { }

        protected override ITestFrameworkDiscoverer CreateDiscoverer()
        {
            return new SmokeTestDiscoverer(AssemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected override async void RunTestCases(IEnumerable<SmokeTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            var testAssembly = new TestAssembly(AssemblyInfo);

            using (var assemblyRunner = new SmokeTestAssemblyRunner(testAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
            {
                await assemblyRunner.RunAsync();
            }
        }
    }
}