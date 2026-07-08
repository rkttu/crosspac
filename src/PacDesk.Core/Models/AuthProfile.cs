namespace PacDesk.Core.Models;

/// <summary>A <c>pac auth</c> profile row. <see cref="Raw"/> preserves the source line.</summary>
public sealed class AuthProfile
{
    public string Index { get; init; } = "";
    public bool IsActive { get; init; }
    public string Kind { get; init; } = "";
    public string Name { get; init; } = "";
    public string User { get; init; } = "";
    public string Raw { get; init; } = "";

    public string Display =>
        $"[{Index}]{(IsActive ? " ★" : "")} {User} {Name}".Trim();
}
