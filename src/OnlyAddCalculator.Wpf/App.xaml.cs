using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace OnlyAddCalculator.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private MainWindowSession? _mainWindowSession;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _serviceProvider = ServiceConfiguration.CreateServiceProvider();
        _mainWindowSession = _serviceProvider.GetRequiredService<MainWindowFactory>().Create();

        MainWindow = _mainWindowSession.Window;
        _mainWindowSession.Window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();

        base.OnExit(e);
    }
}

