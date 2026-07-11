namespace Crosspac.App.Messages;

/// <summary>Identifies which part of the active context changed, so the shell knows how far
/// down the Auth → Environments → Solutions dependency chain the change must cascade.</summary>
public enum ContextChangeScope
{
    /// <summary>The active auth profile changed; the visible environments and solutions both depend on it.</summary>
    Profile,

    /// <summary>The active environment changed; the visible solutions depend on it.</summary>
    Environment,
}

/// <summary>
/// Broadcast when the active auth profile or environment changes, so the status bar and the
/// dependent tabs (owned by <c>MainWindowViewModel</c>) can refresh. Sent via the
/// CommunityToolkit <c>WeakReferenceMessenger</c> to keep view models decoupled.
/// </summary>
public sealed record ActiveContextChangedMessage(ContextChangeScope Scope);
