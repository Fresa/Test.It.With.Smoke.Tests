using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{


    public abstract class SmokeTestSpecification
    {
        [Test2]
        public void TestA(Action<Type> action)
        {
            action(GetType());
        }

        //[MyTestCaseSourceAttribute(typeof(Source), nameof(Source.GetTestCases), new object[]{ typeof(Type)})]
        //public void DivideTest(Action<Type> action)
        //{
        //    action(GetType());
        //}
        //public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, global::NUnit.Framework.Internal.Test suite)
        //{
        //    foreach (TestCaseParameters parms in Source.GetTestCases(null))
        //    {
        //        yield return _builder.BuildTestMethod(method, suite, parms);
        //    }
        //}
    }

    internal class Source
    {
        public static IEnumerable<TestCaseData> GetTestCases(Type specification)
        {
            yield return new TestCaseData(new Action<Type>(type =>
            {
                var method = type.GetMethods().First(info => info.GetCustomAttributes<SmokeTestAttribute>().Any());
                var attr = method.GetCustomAttribute<SmokeTestAttribute>();

                Console.WriteLine($"{method} in");
                Console.WriteLine($"{attr.FilePath}:line {attr.LineNumber}");
            })).SetName("Test1");
            yield return new TestCaseData(new Action<Type>(type => Console.WriteLine("test1"))).SetName("Test2");
        }
    }

    public sealed class SmokeTestAttribute : Attribute
    {
        public string FilePath { get; }
        public int LineNumber { get; }

        public SmokeTestAttribute
        (
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }

    public class Test2Attribute : NUnitAttribute, ITestBuilder
    {
        private NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, global::NUnit.Framework.Internal.Test suite)
        {
            foreach (TestCaseParameters parms in Source.GetTestCases(null))
            {
                yield return _builder.BuildTestMethod(method, suite, parms);
            }
        }
    }

    public class MyTestCaseSourceAttribute : TestCaseSourceAttribute, ITestBuilder
    {
        public Type SuiteType { get; private set; }

        public new IEnumerable<TestMethod> BuildFrom(IMethodInfo method, global::NUnit.Framework.Internal.Test suite)
        {
            SuiteType = suite.TypeInfo.Type;
            return base.BuildFrom(method, suite);
        }

        public MyTestCaseSourceAttribute(string sourceName) : base(sourceName)
        {
        }

        public MyTestCaseSourceAttribute(Type sourceType, string sourceName, object[] methodParams) : base(sourceType, sourceName, methodParams)
        {
        }

        public MyTestCaseSourceAttribute(Type sourceType, string sourceName) : base(sourceType, sourceName)
        {
        }

        public MyTestCaseSourceAttribute(string sourceName, object[] methodParams) : base(sourceName, methodParams)
        {
        }

        public MyTestCaseSourceAttribute(Type sourceType) : base(sourceType)
        {
        }
    }
}
