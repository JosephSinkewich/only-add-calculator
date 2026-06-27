using System.Globalization;
using OnlyAddCalculator.Core;

namespace OnlyAddCalculator.Application;

public sealed class CalculatorPresenter
{
    private const string ErrorResult = "Error";

    private readonly ICalculatorView _view;
    private readonly AdditionCalculator _calculator;
    private readonly IAppStateStore _stateStore;
    private readonly List<HistoryItem> _history = [];

    public CalculatorPresenter(ICalculatorView view, AdditionCalculator calculator, IAppStateStore stateStore)
    {
        _view = view;
        _calculator = calculator;
        _stateStore = stateStore;

        view.OnViewLoaded += OnViewLoaded;
        view.OnResultRequested += OnResultRequested;
        view.OnViewClosing += OnViewClosing;
    }

    private void OnViewLoaded(object? sender, EventArgs e)
    {
        var state = _stateStore.Load();

        _history.Clear();
        _history.AddRange(state.History);

        _view.Input = state.CurrentInput;
        _view.SetHistory(_history);
    }

    private void OnResultRequested(object? sender, EventArgs e)
    {
        var input = _view.Input;
        var result = _calculator.Calculate(input);

        var historyItem = result.IsSuccess
            ? new HistoryItem
            {
                Input = input,
                Result = result.Value!.Value.ToString(CultureInfo.InvariantCulture),
                IsError = false,
            }
            : new HistoryItem
            {
                Input = input,
                Result = ErrorResult,
                IsError = true,
            };

        _history.Add(historyItem);
        _view.SetHistory(_history);

        if (!result.IsSuccess)
        {
            _view.ShowInvalidInputMessage();
        }
    }

    private void OnViewClosing(object? sender, EventArgs e)
    {
        _stateStore.Save(new AppState
        {
            CurrentInput = _view.Input,
            History = _history.ToArray(),
        });
    }
}
