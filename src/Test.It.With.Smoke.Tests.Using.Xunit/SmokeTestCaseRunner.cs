using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestCaseRunner : TestCaseRunner<SmokeTestCase>
    {
        private readonly string _displayName;
        private readonly SmokeTestSpecification _specification;

        public SmokeTestCaseRunner(SmokeTestSpecification specification,
            SmokeTestCase testCase,
            string displayName,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(testCase, messageBus, aggregator, cancellationTokenSource)
        {
            _specification = specification;
            _displayName = displayName;
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            var timer = new ExecutionTimer();
            var testClass = TestCase.TestMethod.TestClass.Class.ToRuntimeType();
            var testMethod = TestCase.TestMethod.Method.ToRuntimeMethod();
            var test = new SmokeTest(TestCase, _displayName);

            return new SmokeTestRunner(_specification, test, MessageBus, timer, testClass, testMethod, Aggregator, CancellationTokenSource).RunAsync();
        }
    }
}