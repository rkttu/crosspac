using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Crosspac.App.Services;

public sealed class ClipboardService : IClipboardService
{
    private static Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    public async Task SetTextAsync(string text)
    {
        var clipboard = MainWindow?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(text);
    }
}
