using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestRunner : TestRunner<SmokeTestCase>
    {
        private readonly SmokeTestSpecification _specification;
        private readonly ExecutionTimer _timer;

        public SmokeTestRunner(SmokeTestSpecification specification,
            ITest test,
            IMessageBus messageBus,
            ExecutionTimer timer,
            Type testClass,
            MethodInfo testMethod,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, null, testMethod, null, null, aggregator, cancellationTokenSource)
        {
            _specification = specification;
            _timer = timer;
        }

        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            var duration = await new SmokeTestInvoker(_specification, Test, MessageBus, TestClass, TestMethod, aggregator, CancellationTokenSource).RunAsync();
            return Tuple.Create(duration, string.Empty);
        }
    }
}