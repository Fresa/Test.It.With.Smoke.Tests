using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public class SmokeTestFixtureAttribute : NUnitAttribute, IFixtureBuilder
    {
        public Type Next { get; set; }

        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var a = _builder.BuildFrom(typeInfo);
            yield return a;

            if (Next != null)
            {
                var nextTypeInfo = new TypeWrapper(Next);
                //var b = _builder.BuildFrom(nextTypeInfo);
                var b = new TestFixture(nextTypeInfo, new []{ (object)1});

                b.ApplyAttributesToTest(nextTypeInfo.Type.GetTypeInfo());
                AddTestCasesToFixture(b);
                yield return b;
            }
        }

        private void AddTestCasesToFixture(TestFixture fixture)
        {
            
            var methods = fixture.TypeInfo.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (IMethodInfo method in methods)
            {
                global::NUnit.Framework.Internal.Test test = BuildTestCase(method, fixture);

                if (test != null)
                    fixture.Add(test);
            }
        }


        private ITestCaseBuilder _testBuilder = new DefaultTestCaseBuilder();
        private global::NUnit.Framework.Internal.Test BuildTestCase(IMethodInfo method, TestSuite suite)
        {
            return _testBuilder.CanBuildFrom(method, suite)
                ? _testBuilder.BuildFrom(method, suite)
                : null;
        }
    }
}
