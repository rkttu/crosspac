using System;
using System.ComponentModel;
using System.Threading;
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
    private readonly IPacRunner _runner;
    private readonly IPacCapabilityProbe _capabilities;
    private readonly IContextService _context;
    private readonly ISettingsStore _settingsStore;
    private readonly AppSettings _settings;
    private bool _pacAvailable;
    private CancellationTokenSource? _initCts;

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

    /// <summary>True while Crosspac probes the pac CLI at startup (version/capabilities check).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    [NotifyPropertyChangedFor(nameof(BusyMessage))]
    private bool _isInitializing;

    /// <summary>Status line for the startup probe, surfaced in the busy overlay.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BusyMessage))]
    private string? _initializingMessage;

    /// <summary>True while the startup probe or any data tab is querying pac; drives the modal busy overlay.</summary>
    public bool IsBusy => IsInitializing || Auth.IsBusy || Environments.IsBusy || Solutions.IsBusy;

    /// <summary>The active operation's status line, surfaced in the busy overlay.</summary>
    public string? BusyMessage =>
        IsInitializing ? InitializingMessage :
        Auth.IsBusy ? Auth.Status :
        Environments.IsBusy ? Environments.Status :
        Solutions.IsBusy ? Solutions.Status :
        null;

    public MainWindowViewModel(
        PacRunner runner,
        IPacCapabilityProbe capabilities,
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
        _runner = runner;
        _capabilities = capabilities;
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

        // Refresh the status bar and cascade to dependent tabs whenever a child view model
        // changes the active context.
        WeakReferenceMessenger.Default.Register<ActiveContextChangedMessage>(
            this, (_, m) => OnActiveContextChanged(m.Scope));

        // Reset and reload everything when the pac executable is reconfigured in Settings.
        WeakReferenceMessenger.Default.Register<PacExecutableChangedMessage>(
            this, (_, _) => OnPacExecutableChanged());

        _ = InitializeAsync();
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

    private async Task InitializeAsync()
    {
        // Surface the startup pac probe (its `pac help` capability/version check) through the
        // same modal busy overlay the data tabs use, rather than a silent header-only status.
        // Also runs on a pac path change, so drop any capability results cached for the old binary.
        _initCts = new CancellationTokenSource();
        var token = _initCts.Token;
        IsInitializing = true;
        InitializingMessage = "Checking pac CLI (version & capabilities)…";
        try
        {
            await _capabilities.ResetAsync(token);
            var available = await _runner.IsAvailableAsync(token);
            _pacAvailable = available;
            PacStatus = available
                ? "pac CLI detected."
                : "pac CLI not found on PATH — install it to use Crosspac.";

            if (available)
            {
                InitializingMessage = "Reading active context…";
                await RefreshContextAsync(token);
                // Kick off the first tab's load while the overlay is still up so it stays up
                // seamlessly; the rest load lazily the first time they are opened.
                TryAutoLoadTab(SelectedTabIndex);
            }
            else
            {
                ActiveProfile = "(pac not found)";
                ActiveEnvironment = "(pac not found)";
            }
        }
        catch (OperationCanceledException)
        {
            PacStatus = "pac CLI check cancelled.";
            ActiveProfile = "(cancelled)";
            ActiveEnvironment = "(cancelled)";
        }
        finally
        {
            IsInitializing = false;
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

    /// <summary>Cancels the startup probe or whichever tab is currently running (invoked from the busy overlay).</summary>
    [RelayCommand]
    private void CancelActive()
    {
        if (IsInitializing) _initCts?.Cancel();
        else if (Auth.IsBusy) Auth.CancelCommand.Execute(null);
        else if (Environments.IsBusy) Environments.CancelCommand.Execute(null);
        else if (Solutions.IsBusy) Solutions.CancelCommand.Execute(null);
    }

    /// <summary>
    /// Cascades an auth/environment change down the Auth → Environments → Solutions chain,
    /// so dependent tabs drop data for the old context instead of showing it as stale.
    /// </summary>
    private void OnActiveContextChanged(ContextChangeScope scope)
    {
        // A new profile changes which environments AND solutions are visible; a new
        // environment changes which solutions are visible.
        if (scope == ContextChangeScope.Profile)
        {
            Environments.Invalidate();
            Solutions.Invalidate();
        }
        else if (scope == ContextChangeScope.Environment)
        {
            Solutions.Invalidate();
        }

        // Reload an invalidated dependent that is on screen now; the rest reload when next shown.
        TryAutoLoadTab(SelectedTabIndex);

        _ = RefreshContextAsync();
    }

    /// <summary>
    /// Reacts to a pac executable change from Settings: discards every tab's data (it was loaded
    /// by the old binary) and re-runs the startup probe against the new one behind the modal.
    /// Data tabs then reload lazily the next time they are shown.
    /// </summary>
    private void OnPacExecutableChanged()
    {
        Auth.Invalidate();
        Environments.Invalidate();
        Solutions.Invalidate();
        _ = InitializeAsync();
    }

    private async Task RefreshContextAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = await _context.GetActiveAsync(cancellationToken);
            ActiveProfile = context.ProfileUser;
            ActiveEnvironment = context.EnvironmentName;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ActiveProfile = "(error)";
            ActiveEnvironment = ex.Message;
        }
    }
}
