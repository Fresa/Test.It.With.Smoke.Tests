using System;
using Xunit;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    [XunitTestCaseDiscoverer("Test.It.With.Smoke.Tests.Using.Xunit.SmokeTestDiscoverer", "Test.It.With.Smoke.Tests.Using.Xunit")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SmokeTestAttribute : FactAttribute { }
}