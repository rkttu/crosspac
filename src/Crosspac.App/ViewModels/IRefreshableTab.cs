using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Crosspac.App.ViewModels;

/// <summary>
/// A tab that loads its data through a refresh command. The shell uses this to lazily
/// load a tab the first time it is shown and to drive the shared busy overlay + cancel.
/// </summary>
public interface IRefreshableTab : INotifyPropertyChanged
{
    /// <summary>True while a query is running.</summary>
    bool IsBusy { get; }

    /// <summary>True once a load has been started at least once (manual or automatic).</summary>
    bool HasLoaded { get; }

    /// <summary>Latest human-readable status, shown in the busy overlay.</summary>
    string? Status { get; }

    IAsyncRelayCommand RefreshCommand { get; }
    IRelayCommand CancelCommand { get; }

    /// <summary>
    /// Discards loaded data and resets to the unloaded state so the tab reloads the next time
    /// it is shown. The shell calls this to cascade an auth/environment change to dependent tabs.
    /// </summary>
    void Invalidate();
}
