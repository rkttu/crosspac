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
        _solution = solutionUniqueName;
        Heading.Text = $"Export solution: {solutionUniqueName}";
    }

    private async void OnBrowse(object? sender, RoutedEventArgs e)
    {
        var suffix = ManagedRadio.IsChecked == true ? "_managed" : "";
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

        Close(new ExportOptions(ManagedRadio.IsChecked == true, DestinationBox.Text!));
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
