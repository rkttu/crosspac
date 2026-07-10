using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Crosspac.App.Services;

public sealed class StoragePickerService : IStoragePickerService
{
    private static Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    public async Task<string?> PickZipFileAsync()
    {
        var provider = MainWindow?.StorageProvider;
        if (provider is null) return null;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a solution .zip",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Solution package") { Patterns = new[] { "*.zip" } },
            },
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickFileAsync(string title)
    {
        var provider = MainWindow?.StorageProvider;
        if (provider is null) return null;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickSaveFilePathAsync(string title, string suggestedFileName, string defaultExtension)
    {
        var provider = MainWindow?.StorageProvider;
        if (provider is null) return null;

        var ext = defaultExtension.TrimStart('.').ToLowerInvariant();
        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = ext,
            FileTypeChoices = new[]
            {
                new FilePickerFileType($"{ext.ToUpperInvariant()} file") { Patterns = new[] { $"*.{ext}" } },
                new FilePickerFileType("All files") { Patterns = new[] { "*" } },
            },
        });

        return file?.TryGetLocalPath();
    }
}
