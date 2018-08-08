using System;
using System.Runtime.CompilerServices;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SmokeTestAttribute : Attribute
    {
        public SmokeTestAttribute([CallerLineNumber]int lineNumber = 0)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }
    }
}