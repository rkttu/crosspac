using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crosspac.App.Services;
using Crosspac.Core.Configuration;
using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.App.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore _store;
    private readonly AppSettings _settings;
    private readonly IPacExecutable _pac;
    private readonly IStoragePickerService _picker;

    public SettingsViewModel(
        ISettingsStore store,
        AppSettings settings,
        IPacExecutable pac,
        IStoragePickerService picker)
    {
        _store = store;
        _settings = settings;
        _pac = pac;
        _picker = picker;
        _pacPath = string.IsNullOrWhiteSpace(settings.PacExecutablePath) ? "pac" : settings.PacExecutablePath!;
    }

    [ObservableProperty] private string _pacPath;
    [ObservableProperty] private string? _status;

    public string SettingsFilePath => _store.FilePath;

    [RelayCommand]
    private async Task BrowsePacAsync()
    {
        var path = await _picker.PickFileAsync("Select the pac executable");
        if (path is not null)
            PacPath = path;
    }

    [RelayCommand]
    private void Apply()
    {
        // "pac" (or blank) means "resolve on PATH" → store null.
        var normalized = string.IsNullOrWhiteSpace(PacPath) || PacPath.Trim() == "pac" ? null : PacPath.Trim();

        _settings.PacExecutablePath = normalized;
        _pac.ExecutablePath = normalized ?? "pac"; // applies immediately to the shared runner
        _store.Save(_settings);

        Status = "Saved. New pac path is in effect.";
    }
}
