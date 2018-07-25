using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    // todo: test if it can be internal
    public class SmokeTestInvoker : TestInvoker<SmokeTestCase>
    {
        private readonly SmokeTestSpecification _specification;

        public SmokeTestInvoker(SmokeTestSpecification specification,
            ITest test,
            IMessageBus messageBus,
            Type testClass,
            MethodInfo testMethod,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        //todo: should allow constructur parameters like ITestHelperOutput
            : base(test, messageBus, testClass, null, testMethod, null, aggregator, cancellationTokenSource)
        {
            _specification = specification;
        }

        public new Task<decimal> RunAsync()
        {
            return Aggregator.RunAsync(async () =>
            {
                if (CancellationTokenSource.IsCancellationRequested == false)
                {
                    if (Aggregator.HasExceptions == false)
                    {
                        await Timer.AggregateAsync(() => InvokeTestMethodAsync(_specification));
                    }
                }

                return Timer.Total;
            });
        }
    }
}
