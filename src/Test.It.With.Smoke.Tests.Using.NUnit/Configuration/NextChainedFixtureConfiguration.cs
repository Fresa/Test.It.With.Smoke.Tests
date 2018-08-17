using System;
using System.Linq;
using System.Linq.Expressions;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Configuration
{
    internal class NextChainedFixtureConfiguration<TPreviousFixture> : IConfigureNextFixtureChain<TPreviousFixture>
    {
        private readonly IConfigureFixtures _fixtureConfigurer;

        public NextChainedFixtureConfiguration(IConfigureFixtures fixtureConfigurer)
        {
            _fixtureConfigurer = fixtureConfigurer;
        }

        public IConfigureNextFixtureChain<TNextFixture> Next<TNextFixture>(Expression<Func<TPreviousFixture, TNextFixture>> expression)
        {
            if (!(expression.Body is NewExpression constructor))
            {
                throw new NotSupportedException("Expected a constructor");
            }

            _fixtureConfigurer.Add(new Fixture<TNextFixture>(
                fixture => constructor.ResolveConstructorParameters(expression.Parameters.Single(), fixture)));

            return new NextChainedFixtureConfiguration<TNextFixture>(_fixtureConfigurer);
        }

        public IConfigureNextFixtureChain<TPreviousFixture> Next(Action<IConfigureNextFixtureChain<TPreviousFixture>> fixtureGroup)
        {
            fixtureGroup(new NextChainedFixtureConfiguration<TPreviousFixture>(_fixtureConfigurer));

            return this;
        }

        public IFixtureIterator Build()
        {
            return _fixtureConfigurer.Build();
        }
    }
}