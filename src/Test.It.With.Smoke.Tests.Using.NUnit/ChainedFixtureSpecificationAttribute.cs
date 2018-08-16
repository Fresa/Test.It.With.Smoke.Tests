using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public abstract class ChainedFixtureSpecificationAttribute : NUnitAttribute, IFixtureBuilder
    {
        public IFixtureIterator FixtureIterator { get; protected set; }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            foreach (var testFixture in GetFixtures())
            {
                testFixture.ApplyAttributesToTest(testFixture.TypeInfo.Type.GetTypeInfo());
                AddTestCasesToFixture(testFixture);
                yield return testFixture;
            }
        }

        private readonly ITestCaseBuilder _testBuilder = new DefaultTestCaseBuilder();
        private void AddTestCasesToFixture(TestSuite fixture)
        {
            var methods = fixture.TypeInfo.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            var tests = methods
                .Where(method => _testBuilder.CanBuildFrom(method, fixture))
                .Select(method => _testBuilder.BuildFrom(method, fixture));

            foreach (var test in tests)
            {
                fixture.Add(test);
            }
        }

        private IEnumerable<TestFixture> GetFixtures()
        {
            ChainTestFixture previousFixture = null;
            while (FixtureIterator.Next())
            {
                var fixture = FixtureIterator.Current.Type;
                var argumentResolver = FixtureIterator.Current.ParameterResolver;

                var fixtureTypeInfo = new TypeWrapper(fixture);
                previousFixture = new ChainTestFixture(fixtureTypeInfo, previousFixture, argumentResolver);

                yield return previousFixture;
            }
        }
    }

    public class ChainedFixtures<TFixture>
    {
        public static IConfigureNextFixtureChain<TFixture> Start(Expression<Func<TFixture>> expression)
            => new StartChainedFixtureConfiguration<TFixture>().Start(expression);
    }

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

    public interface IStartConfigureFixtureChain<TFixture> : IBuildFixtures
    {
        IConfigureNextFixtureChain<TFixture> Start(Expression<Func<TFixture>> expression);
    }

    public interface IConfigureNextFixtureChain<TPreviousFixture> : IBuildFixtures
    {
        IConfigureNextFixtureChain<TNextFixture> Next<TNextFixture>(
            Expression<Func<TPreviousFixture, TNextFixture>> expression);

        IConfigureNextFixtureChain<TPreviousFixture> Next(Action<IConfigureNextFixtureChain<TPreviousFixture>> fixtureGroup);
    }

    internal interface IConfigureFixtures : IBuildFixtures
    {
        void Add(Fixture fixture);
    }

    public interface IBuildFixtures
    {
        IFixtureIterator Build();
    }

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

    public class ChainedFixtureIterator : IFixtureIterator
    {
        private readonly Queue<Fixture> _processes;

        public ChainedFixtureIterator(Queue<Fixture> processes)
        {
            _processes = processes;
        }

        public Fixture Current { get; private set; }

        public bool Next()
        {
            if (_processes.Any())
            {
                Current = _processes.Dequeue();
                return Current != null;
            }
            return false;
        }
    }

    public interface IFixtureIterator
    {
        Fixture Current { get; }
        bool Next();
    }


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

    public delegate object[] ResolveNextFixturesArgumentsFromPreviousFixture(object fixture);

    public class ChainTestFixture : TestFixture
    {
        private readonly TestFixture _previous;
        private readonly ResolveNextFixturesArgumentsFromPreviousFixture _getArguments;

        public ChainTestFixture(ITypeInfo fixtureType, TestFixture previous, ResolveNextFixturesArgumentsFromPreviousFixture argumentProvider) :
            base(fixtureType)
        {
            _previous = previous;
            _getArguments = argumentProvider;
        }

        private object GetPreviousFixture()
        {
            if (_previous != null && _previous.Properties.ContainsKey("fixture"))
            {
                return _previous.Properties.Get("fixture");
            }

            return null;
        }

        public override object[] Arguments => _getArguments(GetPreviousFixture());
    }

    public static class NewExpressionExtensions
    {
        public static object[] ResolveConstructorParameters(this NewExpression constructor, ParameterExpression fixtureParameterExpression, object fixture)
        {
            return constructor.ResolveConstructorParameters(
                new Tuple<ParameterExpression, object>(fixtureParameterExpression, fixture));
        }

        public static object[] ResolveConstructorParameters(this NewExpression constructor, params Tuple<ParameterExpression, object>[] parameters)
        {
            return constructor.Arguments
                .Select(argument => Expression
                    .Lambda(argument, parameters.Select(tuple => tuple.Item1).ToArray())
                    .Compile()
                    .DynamicInvoke(parameters.Select(tuple => tuple.Item2).ToArray()))
                .ToArray();
        }
    }

}
