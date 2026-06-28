using OnlyAddCalculator.Application;
using OnlyAddCalculator.Core;
using OnlyAddCalculator.Persistence;

namespace OnlyAddCalculator.Integration.Tests;

public sealed class CalculatorWorkflowTests
{
    [Fact]
    public void Startup_WithMissingState_LoadsEmptyInputAndHistory()
    {
        var session = CreateSession();

        session.View.RaiseViewLoaded();

        Assert.Equal(string.Empty, session.View.Input);
        Assert.Empty(session.View.DisplayedHistory);
    }

    [Fact]
    public void Startup_WithMalformedState_FallsBackToEmptyInputAndHistory()
    {
        var filePath = CreateStateFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ malformed json");
        var session = CreateSession(filePath);

        session.View.RaiseViewLoaded();

        Assert.Equal(string.Empty, session.View.Input);
        Assert.Empty(session.View.DisplayedHistory);
    }

    [Fact]
    public void ValidCalculation_WhenClosedAndReopened_RestoresInputAndAppendedHistory()
    {
        var filePath = CreateStateFilePath();
        var firstSession = CreateSession(filePath);
        firstSession.View.RaiseViewLoaded();
        firstSession.View.Input = "54+21";

        firstSession.View.RaiseResultRequested();
        firstSession.View.RaiseViewClosing();

        var secondSession = CreateSession(filePath);
        secondSession.View.RaiseViewLoaded();

        Assert.Equal("54+21", secondSession.View.Input);
        var item = Assert.Single(secondSession.View.DisplayedHistory);
        Assert.Equal("54+21", item.Input);
        Assert.Equal("75", item.Result);
        Assert.False(item.IsError);
    }

    [Fact]
    public void InvalidCalculation_WhenClosedAndReopened_RestoresInputAndErrorHistory()
    {
        var filePath = CreateStateFilePath();
        var firstSession = CreateSession(filePath);
        firstSession.View.RaiseViewLoaded();
        firstSession.View.Input = "45+-88";

        firstSession.View.RaiseResultRequested();

        Assert.Equal(1, firstSession.View.InvalidInputMessageCount);
        Assert.Equal("45+-88", firstSession.View.Input);

        firstSession.View.RaiseViewClosing();

        var secondSession = CreateSession(filePath);
        secondSession.View.RaiseViewLoaded();

        Assert.Equal("45+-88", secondSession.View.Input);
        var item = Assert.Single(secondSession.View.DisplayedHistory);
        Assert.Equal("45+-88", item.Input);
        Assert.Equal("Error", item.Result);
        Assert.True(item.IsError);
    }

    [Fact]
    public void MultipleCalculations_PreserveHistoryOrderAcrossSessions()
    {
        var filePath = CreateStateFilePath();
        var firstSession = CreateSession(filePath);
        firstSession.View.RaiseViewLoaded();

        firstSession.View.Input = "1+1";
        firstSession.View.RaiseResultRequested();
        firstSession.View.Input = "2+3";
        firstSession.View.RaiseResultRequested();
        firstSession.View.RaiseViewClosing();

        var secondSession = CreateSession(filePath);
        secondSession.View.RaiseViewLoaded();

        Assert.Collection(
            secondSession.View.DisplayedHistory,
            first =>
            {
                Assert.Equal("1+1", first.Input);
                Assert.Equal("2", first.Result);
            },
            second =>
            {
                Assert.Equal("2+3", second.Input);
                Assert.Equal("5", second.Result);
            });
    }

    private static CalculatorSession CreateSession(string? filePath = null)
    {
        var view = new FakeCalculatorView();
        var store = new JsonAppStateStore(filePath ?? CreateStateFilePath());
        var presenter = new CalculatorPresenter(view, new AdditionCalculator(), store);

        return new CalculatorSession(view, presenter);
    }

    private static string CreateStateFilePath()
    {
        return Path.Combine(Path.GetTempPath(), "OnlyAddCalculator.Integration.Tests", Guid.NewGuid().ToString("N"), "state.json");
    }

    private sealed record CalculatorSession(FakeCalculatorView View, CalculatorPresenter Presenter);

    private sealed class FakeCalculatorView : ICalculatorView
    {
        public event EventHandler? OnViewLoaded;

        public event EventHandler? OnResultRequested;

        public event EventHandler? OnViewClosing;

        public string Input { get; set; } = string.Empty;

        public IReadOnlyList<HistoryItem> DisplayedHistory { get; private set; } = [];

        public int InvalidInputMessageCount { get; private set; }

        public void SetHistory(IReadOnlyList<HistoryItem> history)
        {
            DisplayedHistory = history.ToArray();
        }

        public void ShowInvalidInputMessage()
        {
            InvalidInputMessageCount++;
        }

        public void RaiseViewLoaded()
        {
            OnViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseResultRequested()
        {
            OnResultRequested?.Invoke(this, EventArgs.Empty);
        }

        public void RaiseViewClosing()
        {
            OnViewClosing?.Invoke(this, EventArgs.Empty);
        }
    }
}
