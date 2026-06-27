namespace OnlyAddCalculator.Application;

public sealed record HistoryItem
{
    public required string Input { get; init; }

    public required string Result { get; init; }

    public required bool IsError { get; init; }
}
