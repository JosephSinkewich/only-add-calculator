namespace OnlyAddCalculator.Application;

public interface ICalculatorView
{
    event EventHandler OnViewLoaded;

    event EventHandler OnResultRequested;

    event EventHandler OnViewClosing;

    string Input { get; set; }

    void SetHistory(IReadOnlyList<HistoryItem> history);

    void ShowInvalidInputMessage();
}
