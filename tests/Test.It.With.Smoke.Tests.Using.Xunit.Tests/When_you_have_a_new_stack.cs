using System;
using System.Collections.Generic;
using FluentAssertions;
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
    public partial class Given_a_new_stack
    {
        public class When_adding_an_item : SmokeTestSpecification
        {
            Stack<string> _stack;

            protected override void Given()
            {
                _stack = new Stack<string>();
            }

            protected override void When()
            {
                _stack.Push("item1");
            }

            [SmokeTest]
            public void It_should_have_one_item()
            {
                _stack.Count.Should().Be(1);
            }

            [SmokeTest]
            public void It_should_be_possible_to_pop()
            {
                _stack.Pop().Should().Be("item1");
            }

            [SmokeTest]
            public void It_should_have_no_more_items()
            {
                _stack.Should().BeEmpty();
            }
        }
    }
}