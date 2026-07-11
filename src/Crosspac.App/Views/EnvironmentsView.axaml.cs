using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Crosspac.App.ViewModels;

namespace Crosspac.App.Views;

public partial class EnvironmentsView : UserControl
{
    public EnvironmentsView() => InitializeComponent();

    // Double-clicking an environment row makes it the active environment. Ignore double-taps
    // on the header/empty space so column sorting isn't hijacked.
    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((e.Source as Visual)?.FindAncestorOfType<DataGridRow>(includeSelf: true) is null)
            return;

        (DataContext as EnvironmentsViewModel)?.SelectCommand.Execute(null);
    }
}
