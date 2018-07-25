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
        readonly SmokeTestSpecification specification;
        readonly ExecutionTimer timer;

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
            this.specification = specification;
            this.timer = timer;
        }

        protected override async Task<Tuple<decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            var duration = await new SmokeTestInvoker(specification, Test, MessageBus, TestClass, TestMethod, aggregator, CancellationTokenSource).RunAsync();
            return Tuple.Create(duration, String.Empty);
        }
    }
}