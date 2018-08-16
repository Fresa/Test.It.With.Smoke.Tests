using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
using NUnit.Framework.Internal.Commands;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public class SmokeTestAttribute : NUnitAttribute, ISimpleTestBuilder, IApplyToTest, IApplyToContext, IWrapTestMethod
    {
        private readonly int _lineNumber;

        public SmokeTestAttribute([CallerLineNumber] int lineNumber = 0)
        {
            _lineNumber = lineNumber;
        }

        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        public TestMethod BuildFrom(IMethodInfo method, global::NUnit.Framework.Internal.Test suite)
        {
            var index = suite.TypeInfo.Type.GetMethods()
                .Where(info => info.GetCustomAttributes<SmokeTestAttribute>().Any())
                .OrderBy(info => info.GetCustomAttribute<SmokeTestAttribute>()._lineNumber)
                .ToList()
                .IndexOf(method.MethodInfo);

            var testMethod = _builder.BuildTestMethod(method, suite, new TestCaseParameters
            {
                TestName = index.ToString("D3") + " " + method.Name.Replace('_', ' ')
            });

            return testMethod;
        }

        public void ApplyToTest(global::NUnit.Framework.Internal.Test test)
        {
            test.Properties.Set(PropertyNames.Order, _lineNumber);
        }

        public void ApplyToContext(TestExecutionContext context)
        {
            context.IsSingleThreaded = true;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new SkipIfPreviousTestsFailedCommand(command);
        }

        private class SkipIfPreviousTestsFailedCommand : DelegatingTestCommand
        {
            public SkipIfPreviousTestsFailedCommand(TestCommand innerCommand)
                : base(innerCommand)
            {
            }

            private static readonly ConcurrentDictionary<Type, Type> FailedFixtures = new ConcurrentDictionary<Type, Type>();

            public override TestResult Execute(TestExecutionContext context)
            {
                var fixtureType = context.CurrentTest.Method.TypeInfo.Type;
                if (FailedFixtures.ContainsKey(fixtureType))
                {
                    var result = context.CurrentResult;
                    result.SetResult(new ResultState(TestStatus.Skipped, FailureSite.Parent), "Previous tests failed");
                    return result;
                }

                try
                {
                    return innerCommand.Execute(context);
                }
                catch (Exception)
                {
                    FailedFixtures.AddOrUpdate(fixtureType, type => type,
                        (type, _) => type);

                    throw;
                }
                finally
                {
                    context.CurrentTest.Parent.Properties.Set("arguments", context.TestObject);
                    //if (SmokeTestFixtureAttribute._instances.Contains(context.TestObject) == false)
                    //    SmokeTestFixtureAttribute._instances.Enqueue(context.TestObject);
                }
            }
        }
    }
}