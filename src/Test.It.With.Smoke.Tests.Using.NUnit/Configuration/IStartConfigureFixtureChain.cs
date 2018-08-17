using System;
using System.Linq.Expressions;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Configuration
{
    public interface IStartConfigureFixtureChain<TFixture> : IBuildFixtures
    {
        IConfigureNextFixtureChain<TFixture> Start(Expression<Func<TFixture>> expression);
    }
}