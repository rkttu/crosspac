using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Crosspac.App.Messages;
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
    private readonly IFileLauncherService _launcher;

    public SettingsViewModel(
        ISettingsStore store,
        AppSettings settings,
        IPacExecutable pac,
        IStoragePickerService picker,
        IFileLauncherService launcher)
    {
        _store = store;
        _settings = settings;
        _pac = pac;
        _picker = picker;
        _launcher = launcher;
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

        var changed = !string.Equals(_settings.PacExecutablePath, normalized, StringComparison.Ordinal);

        _settings.PacExecutablePath = normalized;
        _pac.ExecutablePath = normalized ?? "pac"; // applies immediately to the shared runner
        _store.Save(_settings);

        Status = "Saved. New pac path is in effect.";

        // Only re-probe/reload when the binary actually changed — a new pac can differ in
        // availability and advertised capabilities, invalidating everything loaded so far.
        if (changed)
            WeakReferenceMessenger.Default.Send(new PacExecutableChangedMessage());
    }

    [RelayCommand]
    private void OpenSettingsFile()
    {
        // The file is only written on first save, so ensure it exists before handing it to the OS.
        _store.Save(_settings);
        Status = _launcher.Open(_store.FilePath)
            ? "Opened the settings file in its default app."
            : "Could not open the settings file.";
    }
}
