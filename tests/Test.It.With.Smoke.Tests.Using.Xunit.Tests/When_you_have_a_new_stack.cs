using System;
using System.Collections.Generic;
using FluentAssertions;
using Test.It.With.Smoke.Tests.Using.Xunit;

[assembly: SmokeTestFramework]

namespace Test.It.With.Smoke.Tests.Using.Xunit.Tests
{
    // Retrieve forwarding data from properties (what ever access modifier) 
    // Forward data through constructor. Match parameter name and type with argument name and type

    [Begin(typeof(Given_a_new_stack.When_adding_an_item))]
    [Continue(typeof(By_having_an_existing_stack.When_adding_an_item_again))]
    public class First_When_adding_an_item_and_adding_it_again
    {
    }


    public partial class Given_a_new_stack
    {
        
        public class When_adding_an_item_and_adding_it_again_with_process3 : Process3
        {
            public When_adding_an_item_and_adding_it_again_with_process3()
            {
                Start<When_adding_an_item>()
                    .Next<By_having_an_existing_stack.When_adding_an_item_again>();
            }
        }

        public class When_adding_an_item_and_adding_it_again_with_process2 : Process2<When_adding_an_item>
        {
            protected override void Start(INext<When_adding_an_item> fromFunc)
            {
                fromFunc.Next(item => new By_having_an_existing_stack.When_adding_an_item_again(null));
            }
        }

        public class When_adding_an_item_and_adding_it_again_with_process : Process
        {
            public When_adding_an_item_and_adding_it_again_with_process()
            {
                Start(() => new When_adding_an_item())
                    .Next(item => new By_having_an_existing_stack.When_adding_an_item_again(null));
            }
        }

        public class When_adding_an_item_and_adding_it_again :
            IStart2<When_adding_an_item>,
            INext<When_adding_an_item, By_having_an_existing_stack.When_adding_an_item_again>
        {
            public When_adding_an_item Start()
            {
                return new When_adding_an_item();
            }

            public By_having_an_existing_stack.When_adding_an_item_again Next(When_adding_an_item @from)
            {
                throw new NotImplementedException();
            }
        }

        public class When_adding_an_item : SmokeTestSpecification<By_having_an_existing_stack.When_adding_an_item_again>
        {
            public Stack<string> _stack { get; private set; }

            protected override By_having_an_existing_stack.When_adding_an_item_again Next()
            {
                return new By_having_an_existing_stack.When_adding_an_item_again(_stack);
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
                _stack.Count.Should().Be(2);
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

    public partial class By_having_an_existing_stack
    {
        public class When_adding_an_item_again : SmokeTestSpecification
        {
            Stack<string> _stack;

            public When_adding_an_item_again()
            {
             _stack = new Stack<string>();   
            }

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