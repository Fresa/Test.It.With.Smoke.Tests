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

        public static Queue<object> _instances = new Queue<object>();


        public IEnumerable<TestFixture> TestSource(ITestProcessIterator _process)
        {
            //var scenario = 0;
            //var _instances = new Queue<object>();
            TestFixture previous = null;
            while (_process.Next(_instances.Dequeue))
            {
                var type = _process.Current.Type;
                var argumentResolver = _process.Current.ParameterResolver;

                // todo: behöver koppla ihop argumentResolver (som löser argument genom expressions) med previous.ArgumentProvider (argumentet till expressionet som ska lösas och som kommer returnera det argument som fixturen ska ha)

                var nextTypeInfo = new TypeWrapper(type);
                previous = new MyTestSuite(nextTypeInfo, previous);
                yield return previous;
                //var instanceFactory = new Lazy<object>(() =>
                //{
                //    var instance = Activator.CreateInstance(type, argumentResolver());
                //    _instances.Enqueue(instance);
                //    return instance;
                //});

                //while (methods.Any())
                //{
                //    testContexts.Add(new TestContext(() => instanceFactory.Value, ref methods));
                //}

                //var cleanupManager = new CleanupManager(new ExceptionHandler());
                //var testContextParserFactory = new TestContextParserFactory(cleanupManager);
                //var testCaseBuilder = new TestCaseBuilder(testContexts, testContextParserFactory);

                //var testRunner = new TestRunner(testCaseBuilder.TestCount);
                //testRunner.OnComplete(cleanupManager.Cleanup);
                //testRunner.OnFailure(info =>
                //{
                //    _testRunners.ForEach(runner => runner.StopProcessing());
                //    cleanupManager.Cleanup(info);
                //});
                //_testRunners.Add(testRunner);

                //var testCases = testCaseBuilder.BuildTests(testRunner);

                //++scenario;
                //foreach (var testCase in testCases)
                //{
                //    testCase.SetName(string.Format("{0}. {1} - {2}", scenario, type.Name, testCase.TestName));
                //    yield return testCase;
                //}
            }
        }

        //private static bool AttributeIs<T>(MethodInfo method) where T : Attribute
        //{
        //    return method.GetCustomAttributes<T>().Any();
        //}

        //private static int GetLineNumber(MemberInfo info)
        //{
        //    return info.GetCustomAttribute<OrderedAttribute>().LineNumber;
        //}

        //private static int LineNumber(Method method)
        //{
        //    return method.LineNumber;
        //}
    }

    public class ProcessChain<T>
    {
        private readonly Queue<Func<Func<object[]>, Process>> _processes = new Queue<Func<Func<object[]>, Process>>();

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

            _processes.Enqueue(parameterResolver => new Process(typeof(T), () => ConstructorParameterResolver(constructor)));

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
        private readonly Action<Func<Func<object[]>, Process>> _register;
        private readonly Func<ITestProcessIterator> _iteratorFactory;

        public ProcessChainStep(Action<Func<Func<object[]>, Process>> register, Func<ITestProcessIterator> iteratorFactory)
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

            _register(parameterValuesResolver => new Process(
                typeof(TOut),
                () => ConstructorParameterResolver(constructor, expression.Parameters.ToArray(), parameterValuesResolver())
            ));

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
        private readonly Queue<Func<Func<object[]>, Process>> _processes;

        public ChainedTestProcessIterator(Queue<Func<Func<object[]>, Process>> processes)
        {
            _processes = processes;
        }

        public Process Current { get; private set; }

        public bool Next(Func<object> instance)
        {
            if (_processes.Any())
            {
                Current = _processes.Dequeue()(() => new[]
                {
                    instance()
                });
                return Current != null;
            }
            return false;
        }
    }

    public interface ITestProcessIterator
    {
        Process Current { get; }
        bool Next(Func<object> instance);
    }

    public class Process
    {
        public Process(Type type)
            : this(type, () => null)
        {
        }

        public Process(Type type, Func<object[]> parameterResolver)
        {
            Type = type;
            ParameterResolver = parameterResolver;
        }

        public Type Type { get; private set; }
        public Func<object[]> ParameterResolver { get; private set; }
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
        internal Func<object[]> ArgumentProvider => GetArguments;

        public MyTestSuite(ITypeInfo fixtureType, TestFixture previous) : base(fixtureType)
        {
            _previous = previous;
        }

        public MyTestSuite(ITypeInfo fixtureType, Func<object[]> argumentProvider) : base(fixtureType)
        {
            //ArgumentProvider = argumentProvider;
        }

        private object[] GetArguments()
        {

            if (_previous != null && _previous.Properties.ContainsKey("arguments"))
            {
                return new[] {_previous.Properties.Get("arguments")};
            }

            return new object[0];
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
