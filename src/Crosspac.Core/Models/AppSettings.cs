namespace Crosspac.Core.Models;

/// <summary>User-persisted application settings. Mutable for JSON round-tripping.</summary>
public sealed class AppSettings
{
    public double WindowWidth { get; set; } = 1000;
    public double WindowHeight { get; set; } = 720;

    /// <summary>Override for the pac executable path. Null/empty means resolve "pac" on PATH.</summary>
    public string? PacExecutablePath { get; set; }
}
