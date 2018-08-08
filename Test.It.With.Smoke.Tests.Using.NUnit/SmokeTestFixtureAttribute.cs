using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public class SmokeTestFixtureAttribute : NUnitAttribute, IFixtureBuilder
    {
        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var a = _builder.BuildFrom(typeInfo);
            yield return a;
        }
    }
}
