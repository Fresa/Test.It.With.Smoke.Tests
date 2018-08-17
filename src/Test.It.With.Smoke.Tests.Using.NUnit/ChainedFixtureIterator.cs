using System.Collections.Generic;
using System.Linq;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
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
}