using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
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
}