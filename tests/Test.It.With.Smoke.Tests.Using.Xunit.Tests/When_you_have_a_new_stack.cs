using System;
using System.Collections.Generic;
using Test.It.With.Smoke.Tests.Using.Xunit;
using Xunit;

[assembly: SmokeTestFramework]

//public class Class1
//{
//    [Theory]
//    [ClassData(typeof(TestProvider))]
//    public void TestProvider(Action test)
//    {
//        test();
//    }
//}

//public class BarTestData : IEnumerable<object[]>
//{
//    public IEnumerator<object[]> GetEnumerator()
//    {
//        yield return new object[] { 1, 2 };
//        yield return new object[] { -4, -6 };
//        yield return new object[] { 2, 4 };
//    }

//    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//}

//public class Test1
//{

//    [Theory]
//    [ClassData(typeof(BarTestData))]
//    public void BarTest(int value1, int value2)
//    {
//        Assert.True(value1 + value2 < 7);
//    }

//    [Theory]
//    [MemberData(nameof(BazTestData))]
//    public void BazTest(int value1, int value2)
//    {
//        Assert.True(value1 + value2 < 7);
//    }

//    public static IEnumerable<object[]> BazTestData => new List<object[]>
//    {
//        new object[] {1, 2},
//        new object[] {-4, -6},
//        new object[] {2, 40},
//    };

//}


namespace Test.It.With.Smoke.Tests.Using.Xunit.Tests
{
    public class When_you_have_a_new_stack : SmokeTestSpecification
    {
        Stack<string> _stack;

        protected override void Given()
        {
            _stack = new Stack<string>();
        }

        [SmokeTest]
        public void It_should_be_empty()
        {
            Assert.True(_stack.Count == 0);
        }

        [SmokeTest]
        public void It_should_not_allow_you_to_call_Pop()
        {
            Assert.Throws<InvalidOperationException>(() => _stack.Pop());
        }

        [SmokeTest]
        public void It_should_not_allow_you_to_call_Peek()
        {
            Assert.Throws<InvalidOperationException>(() => { string unused = _stack.Peek(); });
        }
    }
}