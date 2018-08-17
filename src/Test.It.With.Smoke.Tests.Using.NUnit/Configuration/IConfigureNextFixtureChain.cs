using System;
using System.Linq.Expressions;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Configuration
{
    public interface IConfigureNextFixtureChain<TPreviousFixture> : IBuildFixtures
    {
        IConfigureNextFixtureChain<TNextFixture> Next<TNextFixture>(
            Expression<Func<TPreviousFixture, TNextFixture>> expression);

        IConfigureNextFixtureChain<TPreviousFixture> Next(Action<IConfigureNextFixtureChain<TPreviousFixture>> fixtureGroup);
    }
}