using System;
using System.Linq;
using System.Linq.Expressions;

namespace Test.It.With.Smoke.Tests.Using.NUnit
{
    public static class NewExpressionExtensions
    {
        public static object[] ResolveConstructorParameters(this NewExpression constructor, ParameterExpression fixtureParameterExpression, object fixture)
        {
            return constructor.ResolveConstructorParameters(
                new Tuple<ParameterExpression, object>(fixtureParameterExpression, fixture));
        }

        public static object[] ResolveConstructorParameters(this NewExpression constructor, params Tuple<ParameterExpression, object>[] parameters)
        {
            return constructor.Arguments
                .Select(argument => Expression
                    .Lambda(argument, parameters.Select(tuple => tuple.Item1).ToArray())
                    .Compile()
                    .DynamicInvoke(parameters.Select(tuple => tuple.Item2).ToArray()))
                .ToArray();
        }
    }
}