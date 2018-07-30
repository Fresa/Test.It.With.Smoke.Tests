using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    //public class SmokeTestDiscoverer2 : TestFrameworkDiscoverer
    //{
    //    private readonly CollectionPerClassTestCollectionFactory _testCollectionFactory;

    //    public SmokeTestDiscoverer2(IAssemblyInfo assemblyInfo,
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

    //    private bool FindTestsForMethod(ITestMethod testMethod,
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

    //        var testCase = new SmokeTestCase(defaultMethodDisplay, methodDisplayOptions, testMethod)
    //        {
    //            SourceInformation = new SourceInformation
    //            {
    //                LineNumber = smokeTestAttribute.GetNamedArgument<int>(nameof(SmokeTestAttribute.LineNumber))
    //            }
    //        };

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
    //            .All(method => FindTestsForMethod(new TestMethod(testClass, method), methodDisplay, methodDisplayOptions, includeSourceInformation, messageBus));
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

        protected override bool IsValidTestClass(ITypeInfo type)
        {
            return type.GetCustomAttributes(typeof(BeginAttribute)).Any();
        }

        private bool FindTestsForMethod(ITestMethod testMethod,
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

            var testCase = new SmokeTestCase(defaultMethodDisplay, methodDisplayOptions, testMethod)
            {
                SourceInformation = new SourceInformation
                {
                    LineNumber = smokeTestAttribute.GetNamedArgument<int>(nameof(SmokeTestAttribute.LineNumber))
                }
            };

            return ReportDiscoveredTestCase(testCase, false, messageBus);
        }

        protected override bool FindTestsForType(ITestClass testClass,
                                                 bool includeSourceInformation,
                                                 IMessageBus messageBus,
                                                 ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            if (FindTests(testClass, testClass
                        .Class
                        .GetCustomAttributes(typeof(BeginAttribute)).Single()
                        .GetNamedArgument<Type>(nameof(BeginAttribute.TestClass)).ToTypeInfo(),
                    includeSourceInformation,
                    messageBus,
                    discoveryOptions) == false)
            {
                return false;
            }

            return testClass
                .Class
                .GetCustomAttributes(typeof(ContinueAttribute))
                .Select(info => info
                    .GetNamedArgument<Type>(nameof(ContinueAttribute.TestClass)).ToTypeInfo())
                .All(info => FindTests(testClass, info, includeSourceInformation, messageBus, discoveryOptions));
        }

        private bool FindTests(ITestClass testClass,
            ITypeInfo subClass,
            bool includeSourceInformation,
            IMessageBus messageBus,
            ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();
            var methodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();
            return subClass
                .GetMethods(includePrivateMethods: false)
                .All(method => FindTestsForMethod(new TestMethod(new TestClass(testClass.TestCollection, subClass), method), methodDisplay, methodDisplayOptions, includeSourceInformation, messageBus));
        }
    }

    internal static class TypeExtensions
    {
        public static ITypeInfo ToTypeInfo(this Type type)
        {
            return new ReflectionTypeInfo(type);
        }
    }
}