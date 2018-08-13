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
        public Type NextFixture { get; set; }

        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var fixture = _builder.BuildFrom(typeInfo);
            yield return fixture;

            if (NextFixture != null)
            {
                var nextTypeInfo = new TypeWrapper(NextFixture);
                var nextFixture = new TestFixture(nextTypeInfo, new []{ (object)1});

                nextFixture.ApplyAttributesToTest(nextTypeInfo.Type.GetTypeInfo());
                AddTestCasesToFixture(nextFixture);
                yield return nextFixture;
            }
        }

        private readonly ITestCaseBuilder _testBuilder = new DefaultTestCaseBuilder();
        private void AddTestCasesToFixture(TestSuite fixture)
        {
            var methods = fixture.TypeInfo.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            var tests = methods
                .Where(method => _testBuilder.CanBuildFrom(method, fixture))
                .Select(method => _testBuilder.BuildFrom(method, fixture));

            foreach (var test in tests)
            {
                fixture.Add(test);
            }
        }
    }
}
