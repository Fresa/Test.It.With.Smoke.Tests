using System;
using System.Collections;
using System.Collections.Generic;

namespace Test.It.With.Smoke.Tests
{
    public class TestProvider : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new Action(() => { })
            };
            yield return new object[]
            {
                new Action(() => { })
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
