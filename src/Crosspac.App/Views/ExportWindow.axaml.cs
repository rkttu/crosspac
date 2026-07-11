using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Crosspac.App.Services;

namespace Crosspac.App.Views;

public partial class ExportWindow : Window
{
    private string _solution = "";

    public ExportWindow() => InitializeComponent();

    public ExportWindow(string solutionUniqueName) : this()
    {
        _solution = solutionUniqueName?.Trim() ?? "";
        Heading.Text = $"Export solution: {_solution}";
        // Pre-fill with the solution's original name; the user can override it.
        NameBox.Text = _solution;
    }

    private async void OnBrowse(object? sender, RoutedEventArgs e)
    {
        var suffix = ManagedCheck.IsChecked == true ? "_managed" : "";
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export solution to…",
            SuggestedFileName = $"{_solution}{suffix}.zip",
            DefaultExtension = "zip",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("ZIP file") { Patterns = new[] { "*.zip" } },
                new FilePickerFileType("All files") { Patterns = new[] { "*" } },
            },
        });

        var path = file?.TryGetLocalPath();
        if (path is null)
            return;

        DestinationBox.Text = path;
        ExportButton.IsEnabled = true;
    }

    private void OnExport(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DestinationBox.Text))
            return;

        // Trim the entered name and fall back to the solution's original name when it's blank
        // or whitespace-only, so --name always receives a clean, non-empty value.
        var trimmed = NameBox.Text?.Trim();
        var name = string.IsNullOrEmpty(trimmed) ? _solution : trimmed;

        Close(new ExportOptions(
            name,
            DestinationBox.Text!,
            Managed: ManagedCheck.IsChecked == true,
            Overwrite: OverwriteCheck.IsChecked == true,
            Async: AsyncCheck.IsChecked == true));
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
