using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestCollectionRunner : TestCollectionRunner<SmokeTestCase>
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public SmokeTestCollectionRunner(ITestCollection testCollection,
            IEnumerable<SmokeTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override async Task<RunSummary> RunTestClassAsync(ITestClass testClass,
            IReflectionTypeInfo @class,
            IEnumerable<SmokeTestCase> testCases)
        {
            var timer = new ExecutionTimer();
            object testClassInstance = null;

            Aggregator.Run(() => testClassInstance = Activator.CreateInstance(testClass.Class.ToRuntimeType()));

            if (Aggregator.HasExceptions)
            {
                return FailEntireClass(testCases, timer);
            }

            if (!(testClassInstance is SmokeTestSpecification specification))
            {
                Aggregator.Add(new InvalidOperationException(
                    $"Test class {testClass.Class.Name} cannot be static, and must derive from Specification."));
                return FailEntireClass(testCases, timer);
            }

            Aggregator.Run(specification.OnSetup);
            if (Aggregator.HasExceptions)
            {
                return FailEntireClass(testCases, timer);
            }

            var result = await new SmokeTestClassRunner(specification, testClass, @class, testCases, _diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();

            if (specification is IDisposable disposable)
            {
                timer.Aggregate(disposable.Dispose);
            }

            return result;
        }

        private RunSummary FailEntireClass(IEnumerable<SmokeTestCase> testCases, ExecutionTimer timer)
        {
            var count = testCases
                .Select(testCase =>
                    MessageBus.QueueMessage(new TestFailed(
                        new SmokeTest(testCase, testCase.DisplayName),
                        timer.Total,
                        "Exception was thrown in class constructor", Aggregator.ToException())))
                .Count();

            return new RunSummary { Failed = count, Total = count };
        }
    }
}