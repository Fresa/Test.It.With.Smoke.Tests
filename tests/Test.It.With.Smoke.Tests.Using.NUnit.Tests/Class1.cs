using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Tests
{
    [TestFixture]
    public class MyTests : SmokeTestSpecification
    {
        [SmokeTest]
        public void Test()
        {

        }
    }

    [MyTestFixture]
    public class MyFixtureGeneratedTests
    {
        private int i = 0;

        [MyTest]
        public void Hej()
        {
            i++; 
            Assert.AreEqual(1, i);
        }
         
        [MyTest]
        public void B()
        { 
            i++;
            Assert.AreEqual(2, i);
        }

        [MyTest]
        public void A()
        {
            i++;
            Assert.AreEqual(3, i);
        }

        [MyTest]
        public void F()
        {
            i++;
            Assert.AreEqual(4, i);
        }

        [MyTest]
        public void H()
        {
            i++;
            Assert.AreEqual(5, i);
        }

        [MyTest]
        public void K()
        {
            i++;
            Assert.AreEqual(6, i);
        }

        [MyTest]
        public void J()
        {
            i++;
            Assert.AreEqual(7, i);
        }

        [MyTest]
        public void I()
        {
            i++;
            Assert.AreEqual(8, i);
        }

        [MyTest]
        public void AA()
        {
            i++;
            Assert.AreEqual(9, i);
        }

        [MyTest]
        public void AB()
        {
            i++;
            Assert.AreEqual(10, i);
        }

        [MyTest]
        public void AC()
        {
            i++;
            Assert.AreEqual(11, i);
        }
    }

    public class MyTestFixtureAttribute : NUnitAttribute, IFixtureBuilder
    {
        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var a = _builder.BuildFrom(typeInfo);

            yield return a;
        }
    }

    public class MyTestAttribute : NUnitAttribute, ISimpleTestBuilder, IApplyToTest, IImplyFixture
    {
        private readonly string _filePath;
        private readonly int _lineNumber;

        public MyTestAttribute([CallerFilePath] string filePath = "",
                [CallerLineNumber] int lineNumber = 0)
        {
            _filePath = filePath;
            _lineNumber = lineNumber;
        }

        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        public TestMethod BuildFrom(IMethodInfo method, global::NUnit.Framework.Internal.Test suite)
        {
            var testMethod = _builder.BuildTestMethod(method, suite, null);

            var index = suite.TypeInfo.Type.GetMethods()
                .Where(info => info.GetCustomAttributes<MyTestAttribute>().Any())
                .OrderBy(info => info.GetCustomAttribute<MyTestAttribute>()._lineNumber)
                .ToList()
                .IndexOf(method.MethodInfo);

            testMethod.Name = index.ToString("D3") + ". " + testMethod.Name.Replace('_', ' ');
            return testMethod;
        }

        public void ApplyToTest(global::NUnit.Framework.Internal.Test test)
        {
            test.Properties.Set(PropertyNames.Order, _lineNumber);
        }
    }
}
