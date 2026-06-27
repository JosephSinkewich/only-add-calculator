using OnlyAddCalculator.Application;
using OnlyAddCalculator.Core;

namespace OnlyAddCalculator.Application.Tests;

public sealed class CalculatorPresenterTests
{
    [Fact]
    public void ViewLoaded_LoadsSavedInputAndHistory()
    {
        var savedHistory = new[]
        {
            new HistoryItem { Input = "54+21", Result = "75", IsError = false },
        };
        var store = new FakeAppStateStore
        {
            StateToLoad = new AppState
            {
                CurrentInput = "45+00",
                History = savedHistory,
            },
        };
        var view = new FakeCalculatorView();
        _ = CreatePresenter(view, store);

        view.RaiseViewLoaded();

        Assert.Equal("45+00", view.Input);
        Assert.Equal(savedHistory, view.DisplayedHistory);
    }

    [Fact]
    public void ResultRequested_WithValidInput_AppendsCalculatedHistoryItem()
    {
        var store = new FakeAppStateStore();
        var view = new FakeCalculatorView { Input = "54+21" };
        _ = CreatePresenter(view, store);

        view.RaiseResultRequested();

        var item = Assert.Single(view.DisplayedHistory);
        Assert.Equal("54+21", item.Input);
        Assert.Equal("75", item.Result);
        Assert.False(item.IsError);
        Assert.Equal(0, view.InvalidInputMessageCount);
    }

    [Fact]
    public void ResultRequested_WithInvalidInput_AppendsErrorAndShowsValidationMessage()
    {
        var store = new FakeAppStateStore();
        var view = new FakeCalculatorView { Input = "45+-88" };
        _ = CreatePresenter(view, store);

        view.RaiseResultRequested();

        var item = Assert.Single(view.DisplayedHistory);
        Assert.Equal("45+-88", item.Input);
        Assert.Equal("Error", item.Result);
        Assert.True(item.IsError);
        Assert.Equal(1, view.InvalidInputMessageCount);
        Assert.Equal("45+-88", view.Input);
    }

    [Fact]
    public void ResultRequested_AppendsNewHistoryAfterExistingHistory()
    {
        var store = new FakeAppStateStore
        {
            StateToLoad = new AppState
            {
                History =
                [
                    new HistoryItem { Input = "1+1", Result = "2", IsError = false },
                ],
            },
        };
        var view = new FakeCalculatorView();
        _ = CreatePresenter(view, store);

        view.RaiseViewLoaded();
        view.Input = "2+3";
        view.RaiseResultRequested();

        Assert.Collection(
            view.DisplayedHistory,
            first => Assert.Equal("1+1", first.Input),
            second =>
            {
                Assert.Equal("2+3", second.Input);
                Assert.Equal("5", second.Result);
            });
    }

    [Fact]
    public void ViewClosing_SavesCurrentInputAndHistory()
    {
        var store = new FakeAppStateStore();
        var view = new FakeCalculatorView { Input = "54+21" };
        _ = CreatePresenter(view, store);

        view.RaiseResultRequested();
        view.Input = "55+13";
        view.RaiseViewClosing();

        Assert.NotNull(store.SavedState);
        Assert.Equal("55+13", store.SavedState.CurrentInput);
        var item = Assert.Single(store.SavedState.History);
        Assert.Equal("54+21", item.Input);
        Assert.Equal("75", item.Result);
    }

    private static CalculatorPresenter CreatePresenter(FakeCalculatorView view, FakeAppStateStore store)
    {
        return new CalculatorPresenter(view, new AdditionCalculator(), store);
    }

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

    private sealed class FakeAppStateStore : IAppStateStore
    {
        public AppState StateToLoad { get; init; } = AppState.Empty;

        public AppState? SavedState { get; private set; }

        public AppState Load()
        {
            return StateToLoad;
        }

        public void Save(AppState state)
        {
            SavedState = state;
        }
    }
}