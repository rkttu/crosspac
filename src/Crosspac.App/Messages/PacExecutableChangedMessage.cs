namespace Crosspac.App.Messages;

/// <summary>
/// Broadcast when the configured pac executable path changes (via Settings). The shell
/// (<c>MainWindowViewModel</c>) responds by discarding all state derived from the old binary —
/// the capability cache and every tab's data — and re-running the startup probe against the
/// new pac. Sent via the CommunityToolkit <c>WeakReferenceMessenger</c> to keep view models
/// decoupled.
/// </summary>
public sealed record PacExecutableChangedMessage;
