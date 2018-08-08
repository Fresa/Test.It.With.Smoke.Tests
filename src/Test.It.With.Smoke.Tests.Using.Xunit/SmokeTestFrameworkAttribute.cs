using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    [TestFrameworkDiscoverer("Test.It.With.Smoke.Tests.Using.Xunit.SmokeTestFrameworkAttribute", "Test.It.With.Smoke.Tests.Using.Xunit")]
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class SmokeTestFrameworkAttribute : Attribute, ITestFrameworkAttribute, ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            return typeof(SmokeTestFramework);
        }
    }
}