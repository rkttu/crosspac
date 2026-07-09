using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Crosspac.App.Views;

namespace Crosspac.App.Services;

public sealed class DialogService : IDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var dialog = new ConfirmWindow(title, message);

        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (owner is not null)
            return await dialog.ShowDialog<bool>(owner);

        dialog.Show();
        return false;
    }
}
