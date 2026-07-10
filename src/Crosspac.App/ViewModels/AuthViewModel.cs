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

public sealed partial class AuthViewModel : ViewModelBase, IRefreshableTab
{
    private readonly IAuthService _auth;
    private CancellationTokenSource? _cts;

    public AuthViewModel(IAuthService auth) => _auth = auth;

    public ObservableCollection<AuthProfile> Profiles { get; } = new();

    [ObservableProperty] private AuthProfile? _selectedProfile;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _hasLoaded;
    [ObservableProperty] private string? _status;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _cts = new CancellationTokenSource();
        HasLoaded = true;
        IsBusy = true;
        Status = "Loading auth profiles…";
        try
        {
            Profiles.Clear();
            foreach (var profile in await _auth.ListAsync(_cts.Token))
                Profiles.Add(profile);
            Status = $"{Profiles.Count} profile(s).";
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
    private async Task SetActiveAsync()
    {
        if (SelectedProfile is null) return;
        _cts = new CancellationTokenSource();
        try
        {
            await _auth.SelectAsync(SelectedProfile.Index, _cts.Token);
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
