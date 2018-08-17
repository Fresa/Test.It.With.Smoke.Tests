namespace Test.It.With.Smoke.Tests.Using.NUnit.Configuration
{
    internal interface IConfigureFixtures : IBuildFixtures
    {
        void Add(Fixture fixture);
    }
}