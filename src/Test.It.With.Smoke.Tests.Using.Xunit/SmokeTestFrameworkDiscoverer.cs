using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestFrameworkDiscoverer : TestFrameworkDiscoverer
    {
        private readonly CollectionPerClassTestCollectionFactory _testCollectionFactory;

        public SmokeTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo,
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

        private bool FindTestsForMethod(
            ITestClass subClass,
            ITestMethod testMethod,
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

            var testCase = new SmokeTestCase(subClass, DiagnosticMessageSink, defaultMethodDisplay, methodDisplayOptions, testMethod)
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
                .All(method => FindTestsForMethod(testClass, new TestMethod(new TestClass(testClass.TestCollection, subClass), method), methodDisplay, methodDisplayOptions, includeSourceInformation, messageBus));
        }
    }

    internal static class TypeExtensions
    {
        public static ITypeInfo ToTypeInfo(this Type type)
        {
            return new TestTypeInfo(new ReflectionTypeInfo(type));
        }
    }

    internal class TestTypeInfo : ITypeInfo
    {
        private readonly ReflectionTypeInfo _typeInfo;

        public TestTypeInfo(ReflectionTypeInfo typeInfo)
        {
            _typeInfo = typeInfo;
        }

        public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName)
        {
            return _typeInfo.GetCustomAttributes(assemblyQualifiedAttributeTypeName);
        }

        public IEnumerable<ITypeInfo> GetGenericArguments()
        {
            return _typeInfo.GetGenericArguments();
        }

        public IMethodInfo GetMethod(string methodName, bool includePrivateMethod)
        {
            return _typeInfo.GetMethod(methodName, includePrivateMethod);
        }

        public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods)
        {
            return _typeInfo.GetMethods(includePrivateMethods);
        }

        public IAssemblyInfo Assembly => _typeInfo.Assembly;
        public ITypeInfo BaseType => _typeInfo.BaseType;
        public IEnumerable<ITypeInfo> Interfaces => _typeInfo.Interfaces;
        public bool IsAbstract => _typeInfo.IsAbstract;
        public bool IsGenericParameter => _typeInfo.IsGenericParameter;
        public bool IsGenericType => _typeInfo.IsGenericType;
        public bool IsSealed => _typeInfo.IsSealed;
        public bool IsValueType => _typeInfo.IsValueType;
        public string Name => _typeInfo.Name;
    }
}