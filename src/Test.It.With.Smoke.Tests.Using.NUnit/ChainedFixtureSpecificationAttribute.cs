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
    public class ChainedFixtureSpecificationAttribute : NUnitAttribute, IFixtureBuilder
    {
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var fixtureBuilder = GetFixtureBuilder(typeInfo);

            foreach (var testFixture in GetFixtures(fixtureBuilder.Build()))
            {
                testFixture.ApplyAttributesToTest(testFixture.TypeInfo.Type.GetTypeInfo());
                // group by the chain building specification class
                testFixture.Properties.Add(PropertyNames.Category, typeInfo.FullName);

                AddTestCasesToFixture(testFixture);

                // Pre-pad with the chained fixture name in order to re-use specs in different chained fixtures
                testFixture.FullName = (typeInfo.FullName + "." + testFixture.FullName).Replace(".", "_");
                testFixture.Name = (typeInfo.Name + "." + testFixture.Name).Replace(".", "_");

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

        private static IEnumerable<TestFixture> GetFixtures(IFixtureIterator fixtureIterator)
        {
            ChainTestFixture previousFixture = null;
            while (fixtureIterator.Next())
            {
                var fixture = fixtureIterator.Current.Type;
                var argumentResolver = fixtureIterator.Current.ParameterResolver;

                var fixtureTypeInfo = new TypeWrapper(fixture);
                previousFixture = new ChainTestFixture(fixtureTypeInfo, previousFixture, argumentResolver);

                yield return previousFixture;
            }
        }

        private static IBuildFixtures GetFixtureBuilder(ITypeInfo typeInfo)
        {
            var properties = typeInfo.Type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            if (properties.Any() == false)
            {
                throw new InvalidOperationException("Could not find any static public properties.");
            }

            properties = properties.Where(info => info.CanRead).ToArray();
            if (properties.Any() == false)
            {
                throw new InvalidOperationException("Could not find any static public properties with a getter.");
            }

            var fixtureBuilderProperty = properties.First();
            if (fixtureBuilderProperty.GetMethod.ReturnType != typeof(IBuildFixtures))
            {
                throw new InvalidOperationException($"The return type of property {fixtureBuilderProperty.Name} must be of type {typeof(IBuildFixtures)}.");
            }

            return (IBuildFixtures)fixtureBuilderProperty.GetValue(null, null);
        }
    }
}
