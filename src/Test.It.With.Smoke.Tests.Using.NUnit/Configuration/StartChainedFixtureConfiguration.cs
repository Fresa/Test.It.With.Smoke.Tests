using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Configuration
{
    internal class StartChainedFixtureConfiguration<TFixture> : IStartConfigureFixtureChain<TFixture>, IConfigureFixtures
    {
        private readonly Queue<Fixture> _fixtures = new Queue<Fixture>();

        public IConfigureNextFixtureChain<TFixture> Start(Expression<Func<TFixture>> expression)
        {
            if (!(expression.Body is NewExpression constructor))
            {
                throw new NotSupportedException("Expected a constructor");
            }

            _fixtures.Enqueue(new Fixture<TFixture>(
                fixture => constructor.ResolveConstructorParameters()));

            return new NextChainedFixtureConfiguration<TFixture>(this);
        }

        void IConfigureFixtures.Add(Fixture fixture)
        {
            _fixtures.Enqueue(fixture);
        }

        public IFixtureIterator Build()
        {
            return new ChainedFixtureIterator(_fixtures);
        }
    }
}