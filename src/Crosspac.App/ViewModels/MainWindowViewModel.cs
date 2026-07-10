using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private bool _pacAvailable;

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
    [ObservableProperty] private GridLength _logPanelHeight;
    [ObservableProperty] private int _selectedTabIndex;

    /// <summary>True while any data tab is querying pac; drives the modal busy overlay.</summary>
    public bool IsBusy => Auth.IsBusy || Environments.IsBusy || Solutions.IsBusy;

    /// <summary>The running tab's status line, surfaced in the busy overlay.</summary>
    public string? BusyMessage =>
        Auth.IsBusy ? Auth.Status :
        Environments.IsBusy ? Environments.Status :
        Solutions.IsBusy ? Solutions.Status :
        null;

    public MainWindowViewModel(
        PacRunner runner,
        IAuthService auth,
        IEnvironmentService environments,
        ISolutionService solutions,
        IContextService context,
        IDialogService dialogs,
        IStoragePickerService picker,
        IClipboardService clipboard,
        ISettingsStore settingsStore,
        AppSettings settings)
    {
        _context = context;
        _settingsStore = settingsStore;
        _settings = settings;

        _windowWidth = settings.WindowWidth;
        _windowHeight = settings.WindowHeight;
        _logPanelHeight = new GridLength(settings.LogPanelHeight, GridUnitType.Pixel);

        Auth = new AuthViewModel(auth);
        Environments = new EnvironmentsViewModel(environments);
        Solutions = new SolutionsViewModel(solutions, context, dialogs, picker);
        Settings = new SettingsViewModel(settingsStore, settings, runner, picker);

        // Re-derive the shared busy overlay state whenever a tab starts/stops working.
        Auth.PropertyChanged += OnTabPropertyChanged;
        Environments.PropertyChanged += OnTabPropertyChanged;
        Solutions.PropertyChanged += OnTabPropertyChanged;

        Log = new CommandLogViewModel(clipboard, picker);
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
        if (LogPanelHeight.IsAbsolute && LogPanelHeight.Value > 0)
            _settings.LogPanelHeight = LogPanelHeight.Value;
        _settingsStore.Save(_settings);
    }

    private async Task InitializeAsync(IPacRunner runner)
    {
        var available = await runner.IsAvailableAsync();
        _pacAvailable = available;
        PacStatus = available
            ? "pac CLI detected."
            : "pac CLI not found on PATH — install it to use Crosspac.";

        if (available)
        {
            await RefreshContextAsync();
            // Load whichever tab is currently shown (Auth on first launch); the rest load
            // lazily the first time they are opened.
            TryAutoLoadTab(SelectedTabIndex);
        }
        else
        {
            ActiveProfile = "(pac not found)";
            ActiveEnvironment = "(pac not found)";
        }
    }

    partial void OnSelectedTabIndexChanged(int value) => TryAutoLoadTab(value);

    /// <summary>Loads a data tab the first time it is shown, if pac is available.</summary>
    private void TryAutoLoadTab(int index)
    {
        if (!_pacAvailable)
            return;

        IRefreshableTab? tab = index switch
        {
            0 => Auth,
            1 => Environments,
            2 => Solutions,
            _ => null,
        };

        if (tab is { HasLoaded: false, IsBusy: false })
            _ = tab.RefreshCommand.ExecuteAsync(null);
    }

    private void OnTabPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IRefreshableTab.IsBusy) or nameof(IRefreshableTab.Status))
        {
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(BusyMessage));
        }
    }

    /// <summary>Cancels whichever tab is currently running (invoked from the busy overlay).</summary>
    [RelayCommand]
    private void CancelActive()
    {
        if (Auth.IsBusy) Auth.CancelCommand.Execute(null);
        else if (Environments.IsBusy) Environments.CancelCommand.Execute(null);
        else if (Solutions.IsBusy) Solutions.CancelCommand.Execute(null);
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
