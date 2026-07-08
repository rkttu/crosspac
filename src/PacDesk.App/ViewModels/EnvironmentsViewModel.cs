using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PacDesk.App.Messages;
using PacDesk.Core.Models;
using PacDesk.Core.Services;

namespace PacDesk.App.ViewModels;

public sealed partial class EnvironmentsViewModel : ViewModelBase
{
    private readonly IEnvironmentService _environments;
    private CancellationTokenSource? _cts;

    public EnvironmentsViewModel(IEnvironmentService environments) => _environments = environments;

    public ObservableCollection<DataverseEnvironment> Environments { get; } = new();

    [ObservableProperty] private DataverseEnvironment? _selectedEnvironment;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _status;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _cts = new CancellationTokenSource();
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
        try
        {
            await _environments.SelectAsync(SelectedEnvironment.SelectionTarget, _cts.Token);
            await RefreshAsync();
            WeakReferenceMessenger.Default.Send(new ActiveContextChangedMessage());
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled.";
        }
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();
}
