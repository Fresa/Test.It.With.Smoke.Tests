using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
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
            TestMethodDisplayOptions methodDisplayOptions,
            bool includeSourceInformation,
            IMessageBus messageBus)
        {
            var smokeTestAttribute = testMethod.Method.GetCustomAttributes(typeof(SmokeTestAttribute)).FirstOrDefault();
            if (smokeTestAttribute == null)
            {
                return true;
            }

            var testCase = new SmokeTestCase(index, defaultMethodDisplay, methodDisplayOptions, testMethod)
            {
                SourceInformation = new SourceInformation
                {
                    LineNumber = smokeTestAttribute.GetNamedArgument<int>(nameof(SmokeTestAttribute.LineNumber))
                }
            };

            return ReportDiscoveredTestCase(testCase, includeSourceInformation, messageBus);
        }

        protected override bool FindTestsForType(ITestClass testClass,
                                                 bool includeSourceInformation,
                                                 IMessageBus messageBus,
                                                 ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
            var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();
            return testClass
                .Class
                .GetMethods(includePrivateMethods: false)
                .Select((value, index) => new { Value = value, Index = index })
                .All(method => FindTestsForMethod(method.Index, new TestMethod(testClass, method.Value), methodDisplay, methodDisplayOptions, includeSourceInformation, messageBus));
        }
    }
}