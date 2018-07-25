using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestMethodRunner : TestMethodRunner<SmokeTestCase>
    {
        private readonly SmokeTestSpecification _specification;

        public SmokeTestMethodRunner(SmokeTestSpecification specification,
            ITestMethod testMethod,
            IReflectionTypeInfo @class,
            IReflectionMethodInfo method,
            IEnumerable<SmokeTestCase> testCases,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
        {
            _specification = specification;
        }

        protected override Task<RunSummary> RunTestCaseAsync(SmokeTestCase testCase)
        {
            return testCase.RunAsync(_specification, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource);
        }
    }
}