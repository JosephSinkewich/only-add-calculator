using System.Windows;

namespace OnlyAddCalculator.Wpf;

public partial class InvalidInputDialog : Window
{
    public InvalidInputDialog()
    {
        InitializeComponent();
    }

    private void HandleGotItButtonClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
