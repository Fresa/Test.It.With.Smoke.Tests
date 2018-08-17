namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public interface IFixtureIterator
    {
        Fixture Current { get; }
        bool Next();
    }
}