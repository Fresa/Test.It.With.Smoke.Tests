using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public class SmokeTestAttribute : NUnitAttribute, ISimpleTestBuilder, IApplyToTest, IImplyFixture
    {
        private readonly int _lineNumber;

        public SmokeTestAttribute([CallerLineNumber] int lineNumber = 0)
        {
            _lineNumber = lineNumber;
        }

        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        public TestMethod BuildFrom(IMethodInfo method, global::NUnit.Framework.Internal.Test suite)
        {
            var testMethod = _builder.BuildTestMethod(method, suite, null);

            var index = suite.TypeInfo.Type.GetMethods()
                .Where(info => info.GetCustomAttributes<SmokeTestAttribute>().Any())
                .OrderBy(info => info.GetCustomAttribute<SmokeTestAttribute>()._lineNumber)
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