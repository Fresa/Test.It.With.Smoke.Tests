using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Test.It.With.Smoke.Tests.Using.Xunit
{
    public class SmokeTestCase : TestMethodTestCase
    {
        private readonly ITestClass _subclass;
        private readonly IMessageSink _messageSink;

        [Obsolete("For de-serialization purposes only", error: true)]
        public SmokeTestCase() { }

        public SmokeTestCase(ITestClass subclass, IMessageSink messageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions testMethodDisplayOptions, ITestMethod testMethod)
            : base(defaultMethodDisplay, testMethodDisplayOptions, testMethod)
        {
            _subclass = subclass;
            _messageSink = messageSink;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Initialize2();
            // Resharper seems to use this property to identify the method for linking it within the Unit Test Session window.
            // NCrunch seems to use the TestMethod
            DisplayName = $"{TestMethod.Method.Name}";//.Replace('_', ' ');
        }

        public Task<RunSummary> RunAsync(SmokeTestSpecification specification,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new SmokeTestCaseRunner(specification, this, DisplayName, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue(nameof(SourceInformation), SourceInformation);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            SourceInformation = data.GetValue<ISourceInformation>(nameof(SourceInformation));
        }

        private static readonly ConcurrentDictionary<string, IEnumerable<IAttributeInfo>> AssemblyTraitAttributeCache = new ConcurrentDictionary<string, IEnumerable<IAttributeInfo>>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, IEnumerable<IAttributeInfo>> TypeTraitAttributeCache = new ConcurrentDictionary<string, IEnumerable<IAttributeInfo>>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);

        protected void Initialize2()
        {
            IAttributeInfo factAttribute = TestMethod.Method.GetCustomAttributes(typeof(SmokeTestAttribute)).First<IAttributeInfo>();
//            string displayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;
            //this.DisplayName = this.GetDisplayName(factAttribute, displayName);
            //this.SkipReason = this.GetSkipReason(factAttribute);
//            this.Timeout = this.GetTimeout(factAttribute);
            foreach (IAttributeInfo attributeInfo in GetTraitAttributesData(TestMethod))
            {
                IAttributeInfo traitDiscovererAttribute = attributeInfo.GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault<IAttributeInfo>();
                if (traitDiscovererAttribute != null)
                {
                    ITraitDiscoverer traitDiscoverer = ExtensibilityPointFactory.GetTraitDiscoverer(_messageSink, traitDiscovererAttribute);
                    if (traitDiscoverer != null)
                    {
                        foreach (KeyValuePair<string, string> trait in traitDiscoverer.GetTraits(attributeInfo))
                            Traits.Add<string, string>(trait.Key, trait.Value);
                    }
                }
                //else
                //    this.DiagnosticMessageSink.OnMessage((IMessageSinkMessage)new DiagnosticMessage(string.Format("Trait attribute on '{0}' did not have [TraitDiscoverer]", (object)this.DisplayName)));
            }
        }

        private static IEnumerable<IAttributeInfo> GetCachedTraitAttributes(IAssemblyInfo assembly)
        {
            return AssemblyTraitAttributeCache.GetOrAdd(assembly.Name, (Func<IEnumerable<IAttributeInfo>>)(() => assembly.GetCustomAttributes(typeof(ITraitAttribute))));
        }

        private static IEnumerable<IAttributeInfo> GetCachedTraitAttributes(ITypeInfo type)
        {
            return TypeTraitAttributeCache.GetOrAdd(type.Name, () => type.GetCustomAttributes(typeof(ITraitAttribute)));
        }

        private IEnumerable<IAttributeInfo> GetTraitAttributesData(ITestMethod testMethod)
        {
            return GetCachedTraitAttributes(testMethod.TestClass.Class.Assembly)
                .Concat(testMethod.Method.GetCustomAttributes(typeof(ITraitAttribute)))
                .Concat<IAttributeInfo>(GetCachedTraitAttributes(testMethod.TestClass.Class))
                .Concat<IAttributeInfo>(GetCachedTraitAttributes(_subclass.Class));
        }
        
    }

    internal static class DictionaryExtensions2
    {
        public static void Add<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            dictionary.GetOrAdd<TKey, List<TValue>>(key).Add(value);
        }

        public static bool Contains<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value, IEqualityComparer<TValue> valueComparer)
        {
            List<TValue> source;
            if (!dictionary.TryGetValue(key, out source))
                return false;
            return source.Contains<TValue>(value, valueComparer);
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            return dictionary.GetOrAdd<TKey, TValue>(key, (Func<TValue>)(() => Activator.CreateInstance<TValue>()));
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> newValue)
        {
            TValue obj;
            if (!dictionary.TryGetValue(key, out obj))
            {
                obj = newValue();
                dictionary[key] = obj;
            }
            return obj;
        }

        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
        {
            IEnumerable<TValue> inputValues = values;
            Func<TValue, TKey> keySelector1 = keySelector;
            return inputValues.ToDictionaryIgnoringDuplicateKeys<TValue, TKey, TValue>(keySelector1, (Func<TValue, TValue>)(x => x));
        }

        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TInput, TKey, TValue>(this IEnumerable<TInput> inputValues, Func<TInput, TKey> keySelector, Func<TInput, TValue> valueSelector, IEqualityComparer<TKey> comparer = null)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (TInput inputValue in inputValues)
            {
                TKey key = keySelector(inputValue);
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, valueSelector(inputValue));
            }
            return dictionary;
        }
    }
}