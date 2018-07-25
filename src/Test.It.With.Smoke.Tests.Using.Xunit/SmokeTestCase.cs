using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestCase : TestMethodTestCase
    {
        private readonly int _index;

        [Obsolete("For de-serialization purposes only", error: true)]
        public SmokeTestCase() { }

        public SmokeTestCase(int index, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions testMethodDisplayOptions, ITestMethod testMethod)
            : base(defaultMethodDisplay, testMethodDisplayOptions, testMethod)
        {
            _index = index;
        }

        protected override void Initialize()
        {
            base.Initialize();

            DisplayName = $"{TestMethod.Method.Name.Replace('_', ' ')}";
        }

        public Task<RunSummary> RunAsync(SmokeTestSpecification specification,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new SmokeTestCaseRunner(specification, this, DisplayName, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }
    }
}