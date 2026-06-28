using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using OnlyAddCalculator.Application;
using OnlyAddCalculator.Localization.Resources;

namespace OnlyAddCalculator.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, ICalculatorView
{
    private readonly ObservableCollection<string> _historyLines = [];

    public MainWindow()
    {
        InitializeComponent();

        HistoryItemsControl.ItemsSource = _historyLines;
    }

    public event EventHandler? OnViewLoaded;

    public event EventHandler? OnResultRequested;

    public event EventHandler? OnViewClosing;

    public string Input
    {
        get => InputTextBox.Text;
        set
        {
            InputTextBox.Text = value;
            UpdateInputVisualState();
        }
    }

    public void SetHistory(IReadOnlyList<HistoryItem> history)
    {
        _historyLines.Clear();

        foreach (var item in history.Reverse())
        {
            var result = item.IsError ? Strings.HistoryErrorResult : item.Result;
            _historyLines.Add($"{item.Input}={result}");
        }

        Dispatcher.BeginInvoke(HistoryScrollViewer.ScrollToTop, DispatcherPriority.Background);
    }

    public void ShowInvalidInputMessage()
    {
        var dialog = new InvalidInputDialog
        {
            Owner = this,
        };

        dialog.ShowDialog();
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        OnViewLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void HandleResultButtonClick(object sender, RoutedEventArgs e)
    {
        OnResultRequested?.Invoke(this, EventArgs.Empty);
    }

    private void HandleClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        OnViewClosing?.Invoke(this, EventArgs.Empty);
    }

    private void HandleInputTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateInputVisualState();
    }

    private void UpdateInputVisualState()
    {
        InputPlaceholderTextBlock.Visibility = string.IsNullOrEmpty(InputTextBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}