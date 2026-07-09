using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Crosspac.App.Messages;
using Crosspac.App.Services;
using Crosspac.Core.Configuration;
using Crosspac.Core.Execution;
using Crosspac.Core.Models;
using Crosspac.Core.Services;

namespace Crosspac.App.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly IContextService _context;
    private readonly ISettingsStore _settingsStore;
    private readonly AppSettings _settings;

    public AuthViewModel Auth { get; }
    public EnvironmentsViewModel Environments { get; }
    public SolutionsViewModel Solutions { get; }
    public SettingsViewModel Settings { get; }
    public CommandLogViewModel Log { get; }

    [ObservableProperty] private string _pacStatus = "Checking for pac CLI…";
    [ObservableProperty] private string _activeProfile = "…";
    [ObservableProperty] private string _activeEnvironment = "…";
    [ObservableProperty] private double _windowWidth;
    [ObservableProperty] private double _windowHeight;

    public MainWindowViewModel(
        PacRunner runner,
        IAuthService auth,
        IEnvironmentService environments,
        ISolutionService solutions,
        IContextService context,
        IDialogService dialogs,
        IStoragePickerService picker,
        ISettingsStore settingsStore,
        AppSettings settings)
    {
        _context = context;
        _settingsStore = settingsStore;
        _settings = settings;

        _windowWidth = settings.WindowWidth;
        _windowHeight = settings.WindowHeight;

        Auth = new AuthViewModel(auth);
        Environments = new EnvironmentsViewModel(environments);
        Solutions = new SolutionsViewModel(solutions, context, dialogs, picker);
        Settings = new SettingsViewModel(settingsStore, settings, runner, picker);

        Log = new CommandLogViewModel();
        Log.Attach(runner);

        // Refresh the status bar whenever a child view model changes the active context.
        WeakReferenceMessenger.Default.Register<ActiveContextChangedMessage>(
            this, (_, _) => _ = RefreshContextAsync());

        _ = InitializeAsync(runner);
    }

    /// <summary>Persists the latest window size. Called by the window on close.</summary>
    public void SaveWindowState(double width, double height)
    {
        if (double.IsNaN(width) || double.IsNaN(height) || width <= 0 || height <= 0)
            return;

        _settings.WindowWidth = width;
        _settings.WindowHeight = height;
        _settingsStore.Save(_settings);
    }

    private async Task InitializeAsync(IPacRunner runner)
    {
        var available = await runner.IsAvailableAsync();
        PacStatus = available
            ? "pac CLI detected."
            : "pac CLI not found on PATH — install it to use Crosspac.";

        if (available)
            await RefreshContextAsync();
        else
        {
            ActiveProfile = "(pac not found)";
            ActiveEnvironment = "(pac not found)";
        }
    }

    private async Task RefreshContextAsync()
    {
        try
        {
            var context = await _context.GetActiveAsync();
            ActiveProfile = context.ProfileUser;
            ActiveEnvironment = context.EnvironmentName;
        }
        catch (Exception ex)
        {
            ActiveProfile = "(error)";
            ActiveEnvironment = ex.Message;
        }
    }
}
