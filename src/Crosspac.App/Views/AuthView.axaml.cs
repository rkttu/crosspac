using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Crosspac.App.ViewModels;

namespace Crosspac.App.Views;

public partial class AuthView : UserControl
{
    public AuthView() => InitializeComponent();

    // Double-clicking a profile row activates it — selecting the active profile is the key
    // action here. Ignore double-taps on the header/empty space so sorting isn't hijacked.
    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        if ((e.Source as Visual)?.FindAncestorOfType<DataGridRow>(includeSelf: true) is null)
            return;

        (DataContext as AuthViewModel)?.SetActiveCommand.Execute(null);
    }
}
