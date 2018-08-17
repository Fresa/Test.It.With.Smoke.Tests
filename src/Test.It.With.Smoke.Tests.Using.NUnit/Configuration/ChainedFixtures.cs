using System;
using System.Linq.Expressions;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Configuration
{
    public class ChainedFixtures<TFixture>
    {
        public static IConfigureNextFixtureChain<TFixture> Start(Expression<Func<TFixture>> expression)
            => new StartChainedFixtureConfiguration<TFixture>().Start(expression);
    }
}