using OnlyAddCalculator.Application;

namespace OnlyAddCalculator.Wpf;

public sealed class MainWindowSession
{
    public MainWindowSession(MainWindow window, CalculatorPresenter presenter)
    {
        Window = window;
        Presenter = presenter;
    }

    public MainWindow Window { get; }

    public CalculatorPresenter Presenter { get; }
}
