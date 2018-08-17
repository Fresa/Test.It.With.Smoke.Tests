using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using Test.It.With.Smoke.Tests.Using.NUnit.Configuration;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class ChainedFixtureSpecificationAttribute : NUnitAttribute, IFixtureBuilder
    {
        public abstract IBuildFixtures FixtureBuilder { get; }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            foreach (var testFixture in GetFixtures())
            {
                testFixture.ApplyAttributesToTest(testFixture.TypeInfo.Type.GetTypeInfo());
                AddTestCasesToFixture(testFixture);
                yield return testFixture;
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

        private IEnumerable<TestFixture> GetFixtures()
        {
            ChainTestFixture previousFixture = null;
            var fixtureIterator = FixtureBuilder.Build();
            while (fixtureIterator.Next())
            {
                var fixture = fixtureIterator.Current.Type;
                var argumentResolver = fixtureIterator.Current.ParameterResolver;

                var fixtureTypeInfo = new TypeWrapper(fixture);
                previousFixture = new ChainTestFixture(fixtureTypeInfo, previousFixture, argumentResolver);

                yield return previousFixture;
            }
        }
    }
}
