using Test.It.Specifications;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public abstract class SmokeTestSpecification : Specification
    {
        internal void OnSetup()
        {
            Setup();
        }
    }
}