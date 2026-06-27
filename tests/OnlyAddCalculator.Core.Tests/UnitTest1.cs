using OnlyAddCalculator.Core;

namespace OnlyAddCalculator.Core.Tests;

public sealed class AdditionCalculatorTests
{
    private readonly AdditionCalculator calculator = new();

    [Theory]
    [InlineData("54+21", "75")]
    [InlineData("45+00", "45")]
    [InlineData("55+13+45+11", "124")]
    [InlineData("54 + 21", "75")]
    [InlineData("55 + 13 + 45 + 11", "124")]
    [InlineData("999999999999999999999999999999+1", "1000000000000000000000000000000")]
    public void Calculate_ReturnsSum_ForValidAdditionExpression(string expression, string expectedResult)
    {
        var result = calculator.Calculate(expression);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedResult, result.Value.Value.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("45+-88")]
    [InlineData("98.12+48.1")]
    [InlineData("1+")]
    [InlineData("+1")]
    [InlineData("1++2")]
    [InlineData("abc")]
    [InlineData("1-2")]
    [InlineData("1*2")]
    [InlineData("55")]
    public void Calculate_ReturnsFailure_ForInvalidExpression(string? expression)
    {
        var result = calculator.Calculate(expression);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
    }
}