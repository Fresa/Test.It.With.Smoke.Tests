using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestCase : TestMethodTestCase
    {
        [Obsolete("For de-serialization purposes only", error: true)]
        public SmokeTestCase() { }

        public SmokeTestCase(TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions testMethodDisplayOptions, ITestMethod testMethod)
            : base(defaultMethodDisplay, testMethodDisplayOptions, testMethod)
        {
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

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue(nameof(SourceInformation), SourceInformation);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            SourceInformation = data.GetValue<ISourceInformation>(nameof(SourceInformation));
        }
    }
}