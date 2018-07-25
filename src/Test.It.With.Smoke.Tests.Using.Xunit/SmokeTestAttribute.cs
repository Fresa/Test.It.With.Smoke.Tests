using System;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    [XunitTestCaseDiscoverer("Test.It.With.Smoke.Tests.Using.Xunit.SmokeTestDiscoverer",
        "Test.It.With.Smoke.Tests.Using.Xunit")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SmokeTestAttribute : FactAttribute
    {
        public SmokeTestAttribute([CallerLineNumber]int lineNumber = 0)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }
    }
}