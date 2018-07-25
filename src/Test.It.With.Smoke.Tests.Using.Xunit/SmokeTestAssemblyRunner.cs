using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestAssemblyRunner : TestAssemblyRunner<SmokeTestCase>
    {
        public SmokeTestAssemblyRunner(ITestAssembly testAssembly,
            IEnumerable<SmokeTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        {
            TestCaseOrderer = new SmokeTestCaseOrderer();
        }

        protected override string GetTestFrameworkDisplayName()
        {
            return "Smoke Test Framework";
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<SmokeTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
            return new SmokeTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
        }
    }
}