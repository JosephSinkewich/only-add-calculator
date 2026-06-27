using System.Globalization;
using System.Numerics;

namespace OnlyAddCalculator.Core;

public sealed class AdditionCalculator
{
    public CalculationResult Calculate(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return CalculationResult.Failure();
        }

        if (!expression.Contains('+', StringComparison.Ordinal))
        {
            return CalculationResult.Failure();
        }

        if (expression.Any(static character => !char.IsDigit(character) && character != '+' && !char.IsWhiteSpace(character)))
        {
            return CalculationResult.Failure();
        }

        var operands = expression.Split('+');
        var sum = BigInteger.Zero;

        foreach (var operand in operands)
        {
            var trimmedOperand = operand.Trim();

            if (trimmedOperand.Length == 0 || !trimmedOperand.All(char.IsDigit))
            {
                return CalculationResult.Failure();
            }

            sum += BigInteger.Parse(trimmedOperand, CultureInfo.InvariantCulture);
        }

        return CalculationResult.Success(sum);
    }
}

public sealed record CalculationResult
{
    private CalculationResult(bool isSuccess, BigInteger? value)
    {
        IsSuccess = isSuccess;
        Value = value;
    }

    public bool IsSuccess { get; }

    public BigInteger? Value { get; }

    public static CalculationResult Success(BigInteger value)
    {
        return new CalculationResult(true, value);
    }

    public static CalculationResult Failure()
    {
        return new CalculationResult(false, null);
    }
}
