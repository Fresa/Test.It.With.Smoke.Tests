using System;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public class Fixture<T> : Fixture
    {
        public Fixture(ResolveNextFixturesArgumentsFromPreviousFixture parameterResolver) : base(typeof(T), parameterResolver)
        {
        }
    }

    public class Fixture
    {
        public Fixture(Type type, ResolveNextFixturesArgumentsFromPreviousFixture parameterResolver)
        {
            Type = type;
            ParameterResolver = parameterResolver;
        }

        public Type Type { get; }
        public ResolveNextFixturesArgumentsFromPreviousFixture ParameterResolver { get; }
    }
}