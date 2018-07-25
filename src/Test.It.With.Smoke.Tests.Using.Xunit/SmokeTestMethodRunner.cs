using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestMethodRunner : TestMethodRunner<SmokeTestCase>
    {
        readonly SmokeTestSpecification specification;

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
            this.specification = specification;
        }

        protected override Task<RunSummary> RunTestCaseAsync(SmokeTestCase testCase)
        {
            return testCase.RunAsync(specification, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource);
        }
    }
}