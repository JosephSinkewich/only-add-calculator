namespace OnlyAddCalculator.Application;

public sealed record AppState
{
    public string CurrentInput { get; init; } = string.Empty;

    public IReadOnlyList<HistoryItem> History { get; init; } = Array.Empty<HistoryItem>();

    public static AppState Empty { get; } = new();
}
