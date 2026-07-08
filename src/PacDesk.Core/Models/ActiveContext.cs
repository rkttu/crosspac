namespace PacDesk.Core.Models;

/// <summary>
/// The currently active pac context — which auth profile and which environment
/// subsequent commands will target. Surfaced in the UI status bar so destructive
/// operations always show where they will land.
/// </summary>
public sealed record ActiveContext(
    string ProfileUser,
    string ProfileIndex,
    string EnvironmentName,
    string EnvironmentUrl)
{
    public static ActiveContext Empty { get; } = new("(none)", "", "(none)", "");
}
