using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Crosspac.App.Views;

public partial class ConfirmWindow : Window
{
    public ConfirmWindow() => InitializeComponent();

    public ConfirmWindow(string title, string message) : this()
    {
        Title = title;
        var text = this.FindControl<TextBlock>("MessageText");
        if (text is not null)
            text.Text = message;
    }

    private void OnConfirm(object? sender, RoutedEventArgs e) => Close(true);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);
}
