using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Crosspac.App.Messages;
using Crosspac.App.Services;
using Crosspac.Core.Execution;
using Crosspac.Core.Models;
using Crosspac.Core.Services;

namespace Crosspac.App.ViewModels;

public sealed partial class EnvironmentsViewModel : ViewModelBase, IRefreshableTab
{
    private readonly IEnvironmentService _environments;
    private readonly IEnvironmentAdminService _admin;
    private readonly IDialogService _dialogs;
    private readonly AppModeState _mode;
    private CancellationTokenSource? _cts;

    public EnvironmentsViewModel(
        IEnvironmentService environments,
        IEnvironmentAdminService admin,
        IDialogService dialogs,
        AppModeState mode)
    {
        _environments = environments;
        _admin = admin;
        _dialogs = dialogs;
        _mode = mode;

        // Re-evaluate the destructive commands whenever read-only mode is toggled.
        _mode.PropertyChanged += OnModeChanged;
    }

    public ObservableCollection<DataverseEnvironment> Environments { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private DataverseEnvironment? _selectedEnvironment;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasLoaded;
    [ObservableProperty] private string? _status;

    private void OnModeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppModeState.IsReadOnly)) return;
        CreateCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }

    // Destructive environment operations are only allowed out of read-only mode.
    private bool CanModify() => !_mode.IsReadOnly;
    private bool CanModifySelected() => !_mode.IsReadOnly && SelectedEnvironment is not null;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _cts = new CancellationTokenSource();
        HasLoaded = true;
        IsBusy = true;
        Status = "Loading environments…";
        try
        {
            Environments.Clear();
            foreach (var environment in await _environments.ListAsync(_cts.Token))
                Environments.Add(environment);
            Status = $"{Environments.Count} environment(s).";
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectAsync()
    {
        if (SelectedEnvironment is null) return;
        _cts = new CancellationTokenSource();
        // Raise the busy state up front (before the select call, not just during the follow-up
        // refresh) so the modal overlay pops immediately on double-click and blocks further input.
        IsBusy = true;
        Status = "Selecting environment…";
        try
        {
            await _environments.SelectAsync(SelectedEnvironment.SelectionTarget, _cts.Token);
            await RefreshAsync();
            WeakReferenceMessenger.Default.Send(new ActiveContextChangedMessage(ContextChangeScope.Environment));
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModify))]
    private async Task CreateAsync()
    {
        var options = await _dialogs.RequestCreateEnvironmentAsync();
        if (options is null)
        {
            Status = "Create cancelled.";
            return;
        }

        var label = string.IsNullOrWhiteSpace(options.Name) ? options.Type.ToString() : options.Name;
        await RunAsync($"Creating {label}…", token => _admin.CreateAsync(options, token));
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task ResetAsync()
    {
        if (SelectedEnvironment is null) return;

        var confirmed = await _dialogs.ConfirmAsync(
            "Confirm environment reset",
            "Reset this environment?\n\n" +
            $"Environment:  {SelectedEnvironment.DisplayName}\n" +
            $"URL:          {SelectedEnvironment.Url}\n" +
            $"ID:           {SelectedEnvironment.EnvironmentId}\n\n" +
            "Resetting wipes all data and customizations and returns it to a clean state. " +
            "This cannot be undone.");

        if (!confirmed)
        {
            Status = "Reset cancelled.";
            return;
        }

        await RunAsync($"Resetting {SelectedEnvironment.DisplayName}…",
            token => _admin.ResetAsync(SelectedEnvironment.SelectionTarget, token));
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteAsync()
    {
        if (SelectedEnvironment is null) return;

        var confirmed = await _dialogs.ConfirmAsync(
            "Confirm environment delete",
            "Delete this environment from your tenant?\n\n" +
            $"Environment:  {SelectedEnvironment.DisplayName}\n" +
            $"URL:          {SelectedEnvironment.Url}\n" +
            $"ID:           {SelectedEnvironment.EnvironmentId}\n\n" +
            "This permanently deletes the environment and all of its data. This cannot be undone.");

        if (!confirmed)
        {
            Status = "Delete cancelled.";
            return;
        }

        await RunAsync($"Deleting {SelectedEnvironment.DisplayName}…",
            token => _admin.DeleteAsync(SelectedEnvironment.SelectionTarget, token));
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    public void Invalidate()
    {
        Environments.Clear();
        SelectedEnvironment = null;
        HasLoaded = false;
        Status = null;
    }

    /// <summary>Shared execution wrapper for admin operations: busy state, cancellation, status, refresh.</summary>
    private async Task RunAsync(string startStatus, Func<CancellationToken, Task<PacCommandResult>> operation)
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;
        Status = startStatus;
        try
        {
            var result = await operation(_cts.Token);
            Status = result.Succeeded
                ? $"Done ({result.Duration.TotalSeconds:0.0}s)."
                : $"Failed (exit {result.ExitCode}). See log.";
            if (result.Succeeded)
                await RefreshAsync();
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
