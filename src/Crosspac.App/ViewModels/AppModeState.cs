using CommunityToolkit.Mvvm.ComponentModel;

namespace Crosspac.App.ViewModels;

/// <summary>
/// App-wide safety mode shared across the shell and tabs. Read-only mode is ON by default and
/// is intentionally not persisted, so every launch starts safe — destructive environment
/// operations (create / reset / delete) are only enabled after the user explicitly opts out.
/// </summary>
public sealed partial class AppModeState : ObservableObject
{
    [ObservableProperty] private bool _isReadOnly = true;
}
