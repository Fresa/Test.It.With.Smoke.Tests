using System;
using System.Collections.Generic;
using FluentAssertions;
using Test.It.With.Smoke.Tests.Using.Xunit;

[assembly: SmokeTestFramework]
namespace Test.It.With.Smoke.Tests.Using.Xunit.Tests
{
    public partial class Given_a_new_stack
    {
        // Retrieve forwarding data from properties (what ever access modifier) 
        // Forward data through constructor. Match parameter name and type with argument name and type
        public class First_When_adding_an_item_and_adding_it_again : 
            IFirst<When_adding_an_item>,
            IAnd<When_adding_an_item_again>
        {
        }

        public class When_adding_an_item_and_adding_it_again_with_process3 : Process3
        {
            public When_adding_an_item_and_adding_it_again_with_process3()
            {
                Start<When_adding_an_item>()
                    .Next<When_adding_an_item_again>();
            }
        }

        public class When_adding_an_item_and_adding_it_again_with_process2 : Process2<When_adding_an_item>
        {
            protected override void Start(INext<When_adding_an_item> fromFunc)
            {
                fromFunc.Next(item => new When_adding_an_item_again(null));
            }
        }

        public class When_adding_an_item_and_adding_it_again_with_process : Process
        {
            public When_adding_an_item_and_adding_it_again_with_process()
            {
                Start(() => new When_adding_an_item())
                    .Next(item => new When_adding_an_item_again(null));
            }
        }

        public class When_adding_an_item_and_adding_it_again : 
            IStart<When_adding_an_item>,
            INext<When_adding_an_item, When_adding_an_item_again>
        {
            public When_adding_an_item Start()
            {
                return new When_adding_an_item();
            }

            public When_adding_an_item_again Next(When_adding_an_item @from)
            {
                throw new NotImplementedException();
            }
        }

        public class When_adding_an_item : SmokeTestSpecification<When_adding_an_item_again>
        {
            public Stack<string> _stack { get; private set; }

            protected override When_adding_an_item_again Next()
            {
                return new When_adding_an_item_again(_stack);
            }

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

        public class When_adding_an_item_again : SmokeTestSpecification
        {
            Stack<string> _stack;

            public When_adding_an_item_again(Stack<string> stack)
            {
                _stack = stack;
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