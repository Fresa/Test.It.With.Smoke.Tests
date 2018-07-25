using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
//using Test.It.Specifications;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;


namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    //public class Class1
    //{
    //    [Theory]
    //    [ClassData(typeof(TestProvider))]
    //    public void TestProvider(Action test)
    //    {
    //        test();
    //    }
    //}

    //public class BarTestData : IEnumerable<object[]>
    //{
    //    public IEnumerator<object[]> GetEnumerator()
    //    {
    //        yield return new object[] { 1, 2 };
    //        yield return new object[] { -4, -6 };
    //        yield return new object[] { 2, 4 };
    //    }

    //    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    //}

    //public class Test1
    //{

    //    [Theory]
    //    [ClassData(typeof(BarTestData))]
    //    public void BarTest(int value1, int value2)
    //    {
    //        Assert.True(value1 + value2 < 7);
    //    }

    //    [Theory]
    //    [MemberData(nameof(BazTestData))]
    //    public void BazTest(int value1, int value2)
    //    {
    //        Assert.True(value1 + value2 < 7);
    //    }

    //    public static IEnumerable<object[]> BazTestData => new List<object[]>
    //    {
    //        new object[] {1, 2},
    //        new object[] {-4, -6},
    //        new object[] {2, 40},
    //    };

    //}

    [TestFrameworkDiscoverer("Test.It.With.Smoke.Tests.Using.Xunit.SmokeTestFrameworkAttribute", "Test.It.With.Smoke.Tests.Using.Xunit")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class SmokeTestFrameworkAttribute : Attribute, ITestFrameworkAttribute, ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            return typeof(SmokeTestFramework);
        }
    }
    
    public class SmokeTestFramework : TestFramework
    {
        public SmokeTestFramework(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new SmokeTestDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new SmokeTestExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }

    public class SmokeTestExecutor : TestFrameworkExecutor<SmokeTestCase>
    {
        public SmokeTestExecutor(AssemblyName assemblyName,
            ISourceInformationProvider sourceInformationProvider,
            IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink) { }

        protected override ITestFrameworkDiscoverer CreateDiscoverer()
        {
            return new SmokeTestDiscoverer(AssemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
        }

        protected override async void RunTestCases(IEnumerable<SmokeTestCase> testCases,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
        {
            var testAssembly = new TestAssembly(AssemblyInfo);

            using (var assemblyRunner = new SmokeTestAssemblyRunner(testAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions))
                await assemblyRunner.RunAsync();
        }
    }

    public class SmokeTestAssemblyRunner : TestAssemblyRunner<SmokeTestCase>
    {
        public SmokeTestAssemblyRunner(ITestAssembly testAssembly,
            IEnumerable<SmokeTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        {
            TestCaseOrderer = new SmokeTestCaseOrderer();
        }

        protected override string GetTestFrameworkDisplayName()
        {
            return "Smoke Test Framework";
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<SmokeTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
        {
            return new SmokeTestCollectionRunner(testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
        }
    }

    public class SmokeTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            return testCases;
        }
    }

    public class SmokeTestCollectionRunner : TestCollectionRunner<SmokeTestCase>
    {
        readonly IMessageSink diagnosticMessageSink;

        public SmokeTestCollectionRunner(ITestCollection testCollection,
                                               IEnumerable<SmokeTestCase> testCases,
                                               IMessageSink diagnosticMessageSink,
                                               IMessageBus messageBus,
                                               ITestCaseOrderer testCaseOrderer,
                                               ExceptionAggregator aggregator,
                                               CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override async Task<RunSummary> RunTestClassAsync(ITestClass testClass,
                                                                    IReflectionTypeInfo @class,
                                                                    IEnumerable<SmokeTestCase> testCases)
        {
            var timer = new ExecutionTimer();
            object testClassInstance = null;

            Aggregator.Run(() => testClassInstance = Activator.CreateInstance(testClass.Class.ToRuntimeType()));

            if (Aggregator.HasExceptions)
                return FailEntireClass(testCases, timer);

            var specification = testClassInstance as SmokeTestSpecification;
            if (specification == null)
            {
                Aggregator.Add(new InvalidOperationException(String.Format("Test class {0} cannot be static, and must derive from Specification.", testClass.Class.Name)));
                return FailEntireClass(testCases, timer);
            }

            Aggregator.Run(specification.OnSetup);
            if (Aggregator.HasExceptions)
                return FailEntireClass(testCases, timer);

            var result = await new SmokeTestClassRunner(specification, testClass, @class, testCases, diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();

            if (specification is IDisposable disposable)
                timer.Aggregate(disposable.Dispose);

            return result;
        }

        private RunSummary FailEntireClass(IEnumerable<SmokeTestCase> testCases, ExecutionTimer timer)
        {
            foreach (var testCase in testCases)
            {
                MessageBus.QueueMessage(new TestFailed(new SmokeTest(testCase, testCase.DisplayName), timer.Total,
                    "Exception was thrown in class constructor", Aggregator.ToException()));
            }
            int count = testCases.Count();
            return new RunSummary { Failed = count, Total = count };
        }
    }

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

    //public class SmokeTestDiscoverer : TestFrameworkDiscoverer
    //{
    //    private readonly CollectionPerClassTestCollectionFactory _testCollectionFactory;

    //    public SmokeTestDiscoverer(IAssemblyInfo assemblyInfo,
    //                                 ISourceInformationProvider sourceProvider,
    //                                 IMessageSink diagnosticMessageSink)
    //        : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
    //    {
    //        var testAssembly = new TestAssembly(assemblyInfo);
    //        // todo: might wanna collect and group test classes in another way when chaining test classes
    //        _testCollectionFactory = new CollectionPerClassTestCollectionFactory(testAssembly, diagnosticMessageSink);
    //    }

    //    protected override ITestClass CreateTestClass(ITypeInfo @class)
    //    {
    //        return new TestClass(_testCollectionFactory.Get(@class), @class);
    //    }

    //    private bool FindTestsForMethod(int index, ITestMethod testMethod,
    //        TestMethodDisplay defaultMethodDisplay,
    //        TestMethodDisplayOptions methodDisplayOptions,
    //        bool includeSourceInformation,
    //        IMessageBus messageBus)
    //    {
    //        var smokeTestAttribute = testMethod.Method.GetCustomAttributes(typeof(SmokeTestAttribute)).FirstOrDefault();
    //        if (smokeTestAttribute == null)
    //        {
    //            return true;
    //        }

    //        var testCase = new SmokeTestCase(index, defaultMethodDisplay, methodDisplayOptions, testMethod);
    //        return ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
    //    }

    //    protected override bool FindTestsForType(ITestClass testClass,
    //                                             bool includeSourceInformation,
    //                                             IMessageBus messageBus,
    //                                             ITestFrameworkDiscoveryOptions discoveryOptions)
    //    {
    //        var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
    //        var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();
    //        return testClass
    //            .Class
    //            .GetMethods(includePrivateMethods: false)
    //            .Select((value, index) => new { Value = value, Index = index })
    //            .All(method => FindTestsForMethod(method.Index, new TestMethod(testClass, method.Value), methodDisplay, methodDisplayOptions, includeSourceInformation, messageBus));
    //    }
    //}

    public class SmokeTestDiscoverer : TestFrameworkDiscoverer
    {
        private readonly CollectionPerClassTestCollectionFactory _testCollectionFactory;

        public SmokeTestDiscoverer(IAssemblyInfo assemblyInfo,
                                     ISourceInformationProvider sourceProvider,
                                     IMessageSink diagnosticMessageSink)
            : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
        {
            var testAssembly = new TestAssembly(assemblyInfo);
            // todo: might wanna collect and group test classes in another way when chaining test classes
            _testCollectionFactory = new CollectionPerClassTestCollectionFactory(testAssembly, diagnosticMessageSink);
        }

        protected override ITestClass CreateTestClass(ITypeInfo @class)
        {
            return new TestClass(_testCollectionFactory.Get(@class), @class);
        }

        private bool FindTestsForMethod(int index, ITestMethod testMethod,
            TestMethodDisplay defaultMethodDisplay,
            bool includeSourceInformation,
            IMessageBus messageBus)
        {
            var smokeTestAttribute = testMethod.Method.GetCustomAttributes(typeof(SmokeTestAttribute)).FirstOrDefault();
            if (smokeTestAttribute == null)
            {
                return true;
            }

            var testCase = new SmokeTestCase(index, defaultMethodDisplay, testMethod);
            return ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
        }

        protected override bool FindTestsForType(ITestClass testClass,
                                                 bool includeSourceInformation,
                                                 IMessageBus messageBus,
                                                 ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
            return testClass
                .Class
                .GetMethods(includePrivateMethods: false)
                .Select((value, index) => new { Value = value, Index = index })
                .All(method => FindTestsForMethod(method.Index, new TestMethod(testClass, method.Value), methodDisplay, includeSourceInformation, messageBus));
        }
    }

    [XunitTestCaseDiscoverer("Test.It.With.Smoke.Tests.Using.Xunit.SmokeTestDiscoverer", "Test.It.With.Smoke.Tests.Using.Xunit")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SmokeTestAttribute : FactAttribute { }

    //public class SmokeTestCase : TestMethodTestCase
    //{
    //    private readonly int _index;

    //    [Obsolete("For de-serialization purposes only", error: true)]
    //    public SmokeTestCase() { }

    //    public SmokeTestCase(int index, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions testMethodDisplayOptions, ITestMethod testMethod)
    //        : base(defaultMethodDisplay, testMethodDisplayOptions, testMethod)
    //    {
    //        _index = index;
    //    }

    //    protected override void Initialize()
    //    {
    //        base.Initialize();

    //        DisplayName = $"{_index}. {TestMethod.TestClass.Class.Name.Replace('_', ' ')}, {TestMethod.Method.Name.Replace('_', ' ')}";
    //    }

    //    public Task<RunSummary> RunAsync(SmokeTestSpecification specification,
    //        IMessageBus messageBus,
    //        ExceptionAggregator aggregator,
    //        CancellationTokenSource cancellationTokenSource)
    //    {
    //        return new SmokeTestCaseRunner(specification, this, DisplayName, messageBus, aggregator, cancellationTokenSource).RunAsync();
    //    }
    //}

    public class SmokeTestCase : TestMethodTestCase
    {
        private readonly int _index;

        [Obsolete("For de-serialization purposes only", error: true)]
        public SmokeTestCase() { }

        public SmokeTestCase(int index, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod)
            : base(defaultMethodDisplay, testMethod)
        {
            _index = index;
        }

        protected override void Initialize()
        {
            base.Initialize();

            DisplayName = $"{_index}. {TestMethod.TestClass.Class.Name.Replace('_', ' ')}, {TestMethod.Method.Name.Replace('_', ' ')}";
        }

        public Task<RunSummary> RunAsync(SmokeTestSpecification specification,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new SmokeTestCaseRunner(specification, this, DisplayName, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }
    }

    public abstract class SmokeTestSpecification //: Specification
    {
        internal void OnSetup()
        {
            //Setup();
        }
    }

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

    public class SmokeTest : LongLivedMarshalByRefObject, ITest
    {
        public SmokeTest(SmokeTestCase testCase, string displayName)
        {
            TestCase = testCase;
            DisplayName = displayName;
        }

        public string DisplayName { get; }

        public SmokeTestCase TestCase { get; }

        ITestCase ITest.TestCase => TestCase;
    }

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
