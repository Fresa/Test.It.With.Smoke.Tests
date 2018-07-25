﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestClassRunner : TestClassRunner<SmokeTestCase>
    {
        readonly SmokeTestSpecification specification;

        public SmokeTestClassRunner(SmokeTestSpecification specification,
            ITestClass testClass,
            IReflectionTypeInfo @class,
            IEnumerable<SmokeTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.specification = specification;
        }

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod,
            IReflectionMethodInfo method,
            IEnumerable<SmokeTestCase> testCases,
            object[] constructorArguments)
        {
            return new SmokeTestMethodRunner(specification, testMethod, Class, method, testCases, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();
        }
    }
}