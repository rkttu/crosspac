namespace Crosspac.App.Messages;

/// <summary>
/// Broadcast when the active auth profile or environment changes, so the status bar
/// (owned by <c>MainWindowViewModel</c>) can refresh. Sent via the CommunityToolkit
/// <c>WeakReferenceMessenger</c> to keep view models decoupled.
/// </summary>
public sealed record ActiveContextChangedMessage;
