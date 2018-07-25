using System;
using System.Collections.Generic;
using Test.It.With.Smoke.Tests.Using.Xunit;
using Xunit;

[assembly: SmokeTestFramework]

public class When_you_have_a_new_stack : SmokeTestSpecification
{
    Stack<string> stack = new Stack<string>();

    //protected override void Given()
    //{
    //    stack = new Stack<string>();
    //}

    [SmokeTest]
    public void it_should_be_empty()
    {
        Assert.True(stack.Count == 0);
    }

    [SmokeTest]
    public void should_not_allow_you_to_call_Pop()
    {
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }

    [SmokeTest]
    public void Should_not_allow_you_to_call_Peek()
    {
        Assert.Throws<InvalidOperationException>(() => { string unused = stack.Peek(); });
    }
}