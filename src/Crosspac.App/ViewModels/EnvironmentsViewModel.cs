using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Crosspac.App.Messages;
using Crosspac.Core.Models;
using Crosspac.Core.Services;

namespace Crosspac.App.ViewModels;

public sealed partial class EnvironmentsViewModel : ViewModelBase, IRefreshableTab
{
    private readonly IEnvironmentService _environments;
    private CancellationTokenSource? _cts;

    public EnvironmentsViewModel(IEnvironmentService environments) => _environments = environments;

    public ObservableCollection<DataverseEnvironment> Environments { get; } = new();

    [ObservableProperty] private DataverseEnvironment? _selectedEnvironment;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasLoaded;
    [ObservableProperty] private string? _status;

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

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    public void Invalidate()
    {
        Environments.Clear();
        SelectedEnvironment = null;
        HasLoaded = false;
        Status = null;
    }
}
