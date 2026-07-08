using Avalonia.Controls;
using PacDesk.App.ViewModels;

namespace PacDesk.App.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var width = double.IsNaN(Width) || Width <= 0 ? Bounds.Width : Width;
            var height = double.IsNaN(Height) || Height <= 0 ? Bounds.Height : Height;
            viewModel.SaveWindowState(width, height);
        }

        base.OnClosing(e);
    }
}
