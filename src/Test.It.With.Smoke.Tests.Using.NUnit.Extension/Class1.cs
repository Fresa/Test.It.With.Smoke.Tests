using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Engine;
using NUnit.Engine.Extensibility;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Extension
{
    [Extension]
    public class MyOwnFrameworkDriverFactory : IDriverFactory
    {
        public bool IsSupportedTestFramework(AssemblyName reference)
        {
            return true;
        }

        public IFrameworkDriver GetDriver(AppDomain domain, AssemblyName reference)
        {
            return new SmokeTestFrameworkDriver();
        }
    }

    public class SmokeTestFrameworkDriver : IFrameworkDriver
    {
        public string Load(string testAssemblyPath, IDictionary<string, object> settings)
        {
            throw new NotImplementedException();
        }

        public int CountTestCases(string filter)
        {
            throw new NotImplementedException();
        }

        public string Run(ITestEventListener listener, string filter)
        {
            throw new NotImplementedException();
        }

        public string Explore(string filter)
        {
            throw new NotImplementedException();
        }

        public void StopRun(bool force)
        {
            throw new NotImplementedException();
        }

        public string ID { get; set; }
    }
}
