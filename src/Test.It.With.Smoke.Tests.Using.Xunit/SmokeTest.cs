using Xunit;
using Xunit.Abstractions;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTest : LongLivedMarshalByRefObject, ITest
    {
        public SmokeTest(SmokeTestCase testCase, string displayName)
        {
            TestCase = testCase;
            DisplayName = displayName;
        }

        public string DisplayName { get; }

        public SmokeTestCase TestCase { get; }

        ITestCase ITest.TestCase => TestCase;
    }
}