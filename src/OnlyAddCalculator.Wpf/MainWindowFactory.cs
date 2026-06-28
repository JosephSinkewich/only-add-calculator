using OnlyAddCalculator.Application;
using OnlyAddCalculator.Core;

namespace OnlyAddCalculator.Wpf;

public sealed class MainWindowFactory
{
    private readonly AdditionCalculator _calculator;
    private readonly IAppStateStore _stateStore;

    public MainWindowFactory(AdditionCalculator calculator, IAppStateStore stateStore)
    {
        _calculator = calculator;
        _stateStore = stateStore;
    }

    public MainWindowSession Create()
    {
        var window = new MainWindow();
        var presenter = new CalculatorPresenter(window, _calculator, _stateStore);

        return new MainWindowSession(window, presenter);
    }
}
