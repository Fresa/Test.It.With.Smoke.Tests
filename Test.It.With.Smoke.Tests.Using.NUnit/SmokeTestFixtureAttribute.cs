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
    public abstract class ProcessBuildingAttribute : Attribute
    {
        public ITestProcessIterator Iterator { get; protected set; }
    }

    public class SmokeTestFixtureAttribute : NUnitAttribute, IFixtureBuilder
    {
        //  public Type NextFixture { get; set; }

        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            var a = typeInfo.GetCustomAttributes<ProcessBuildingAttribute>(true);
            if (a.Any())
            {
                var fixtures = TestSource(a.Single().Iterator);

                foreach (var testFixture in fixtures)
                {
                    testFixture.ApplyAttributesToTest(testFixture.TypeInfo.Type.GetTypeInfo());
                    AddTestCasesToFixture(testFixture);
                    yield return testFixture;

                }
                yield break;

            }


            //var fixture = _builder.BuildFrom(typeInfo);
            //yield return fixture;

            //if (NextFixture != null)
            //{
            //    var nextTypeInfo = new TypeWrapper(NextFixture);
            //    var nextFixture = new TestFixture(nextTypeInfo, new[] { (object)1 });

            //    nextFixture.ApplyAttributesToTest(nextTypeInfo.Type.GetTypeInfo());
            //    AddTestCasesToFixture(nextFixture);
            //    yield return nextFixture;
            //}
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

        //public static Queue<object> _instances = new Queue<object>();


        public IEnumerable<TestFixture> TestSource(ITestProcessIterator _process)
        {
            MyTestSuite previous = null;
            while (_process.Next())
            {
                var type = _process.Current.Type;
                var argumentResolver = _process.Current.ParameterResolver;

                var nextTypeInfo = new TypeWrapper(type);
                previous = new MyTestSuite(nextTypeInfo, previous, argumentResolver);

                yield return previous;
            }
        }
    }

    public class ProcessChain<T>
    {
        private readonly Queue<Func<Process>> _processes = new Queue<Func<Process>>();

        public ProcessChain()
        {

        }

        public ProcessChain(Action initializer)
        {
            initializer();
        }

        public ProcessChainStep<T> Start(Expression<Func<T>> expression)
        {
            var constructor = expression.Body as NewExpression;
            if (constructor == null)
            {
                throw new NotSupportedException("Expected a constructor");
            }

            _processes.Enqueue(() => new Process(typeof(T), fixture => ConstructorParameterResolver(constructor)));

            return new ProcessChainStep<T>(_processes.Enqueue, Build);
        }

        private static object[] ConstructorParameterResolver(NewExpression constructor)
        {
            return constructor.Arguments
                .Select(argument => Expression
                    .Lambda(argument)
                    .Compile()
                    .DynamicInvoke())
                .ToArray();
        }

        public ITestProcessIterator Build()
        {
            return new ChainedTestProcessIterator(_processes);
        }
    }

    public class ProcessChainStep<TIn>
    {
        private readonly Action<Func<Process>> _register;
        private readonly Func<ITestProcessIterator> _iteratorFactory;

        public ProcessChainStep(Action<Func<Process>> register, Func<ITestProcessIterator> iteratorFactory)
        {
            _register = register;
            _iteratorFactory = iteratorFactory;
        }

        private static object[] ConstructorParameterResolver(NewExpression constructor, ParameterExpression[] parameters, object[] parameterValues)
        {
            return constructor.Arguments
                .Select(argument => Expression
                    .Lambda(argument, parameters)
                    .Compile()
                    .DynamicInvoke(parameterValues))
                .ToArray();
        }

        public ProcessChainStep<TOut> Next<TOut>(Expression<Func<TIn, TOut>> expression)
        {
            var constructor = expression.Body as NewExpression;
            if (constructor == null)
            {
                throw new NotSupportedException("Expected a constructor");
            }

            _register(() => new Process(
                typeof(TOut),
                fixture => ConstructorParameterResolver(constructor, expression.Parameters.ToArray(), new[] { fixture })));

            return new ProcessChainStep<TOut>(_register, _iteratorFactory);
        }

        public ProcessChainStep<TIn> Next(Action<ProcessChainStep<TIn>> caller)
        {
            caller(new ProcessChainStep<TIn>(_register, _iteratorFactory));

            return this;
        }

        public ITestProcessIterator Build()
        {
            return _iteratorFactory();
        }
    }

    public class ChainedTestProcessIterator : ITestProcessIterator
    {
        private readonly Queue<Func<Process>> _processes;

        public ChainedTestProcessIterator(Queue<Func<Process>> processes)
        {
            _processes = processes;
        }

        public Process Current { get; private set; }

        public bool Next()
        {
            if (_processes.Any())
            {
                Current = _processes.Dequeue()();
                return Current != null;
            }
            return false;
        }
    }

    public interface ITestProcessIterator
    {
        Process Current { get; }
        bool Next();
    }

    public class Process
    {
        public Process(Type type)
            : this(type, fixture => null)
        {
        }

        public Process(Type type, Func<object, object[]> parameterResolver)
        {
            Type = type;
            ParameterResolver = parameterResolver;
        }

        public Type Type { get; }
        public Func<object, object[]> ParameterResolver { get; }
    }

    public class Testar
    {
        public Testar()
        {
            ChainBuilder<ClassWithConstructor>.Start(() => new ClassWithConstructor(1));
        }
    }

    public class ClassWithConstructor
    {
        public ClassWithConstructor(int i)
        {

        }
    }
    public static class ChainBuilder<T>
    {
        public static NextLinkBuilder<T> Start(Expression<Func<T>> expression)
        {
            if (!(expression.Body is NewExpression constructor))
            {
                throw new NotSupportedException("Expected a constructor");
            }

            var testSuites = new Queue<TestFixture>();

            //testSuites.Enqueue(new MyTestSuite(new TypeWrapper(typeof(T)), arguments => constructor.ResolveConstructorParameters()));

            return new NextLinkBuilder<T>(testSuites);
        }
    }

    public class MyTestSuite : TestFixture
    {
        private readonly TestFixture _previous;
        private readonly Func<object, object[]> _argumentProvider;
        internal Func<object[]> ArgumentProvider => () => _argumentProvider(GetPreviousFixture());

        public MyTestSuite(ITypeInfo fixtureType, TestFixture previous, Func<object, object[]> argumentProvider) : 
            base(fixtureType)
        {
            _previous = previous;
            _argumentProvider = argumentProvider;
        }

        private object GetPreviousFixture()
        {
            if (_previous != null && _previous.Properties.ContainsKey("arguments"))
            {
                return _previous.Properties.Get("arguments");
            }

            return null;
        }

        public override object[] Arguments => ArgumentProvider.Invoke();
    }

    public static class NewExpressionExtensions
    {
        public static object[] ResolveConstructorParameters(this NewExpression constructor, params Tuple<ParameterExpression, object>[] parameters)
        {
            return constructor.Arguments
                .Select(argument => Expression
                    .Lambda(argument, parameters.Select(tuple => tuple.Item1))
                    .Compile()
                    .DynamicInvoke(parameters.Select(tuple => tuple.Item2)))
                .ToArray();
        }
    }

    public class NextLinkBuilder<TIn>
    {
        private readonly Queue<TestFixture> _register;

        public NextLinkBuilder(Queue<TestFixture> register)
        {
            _register = register;
        }

        public NextLinkBuilder<TOut> Next<TOut>(Expression<Func<TIn, TOut>> expression)
        {
            if (!(expression.Body is NewExpression constructor))
            {
                throw new NotSupportedException("Expected a constructor");
            }

            ////  _register.Enqueue(new MyTestSuite(new TypeWrapper(typeof(TOut)),

            //      arguments => constructor.ResolveConstructorParameters(expression.Parameters.Select((parameterExpression, i) => new Tuple<ParameterExpression, object>(parameterExpression, arguments[i])).ToArray())
            //  ));

            return new NextLinkBuilder<TOut>(_register);
        }

        public NextLinkBuilder<TIn> Next(Action<NextLinkBuilder<TIn>> caller)
        {
            caller(new NextLinkBuilder<TIn>(_register));

            return this;
        }

        public IEnumerable<TestFixture> Get(object[] argumeents)
        {
            yield return _register.Dequeue();
        }
    }
}
