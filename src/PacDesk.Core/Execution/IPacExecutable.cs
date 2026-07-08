namespace PacDesk.Core.Execution;

/// <summary>Exposes the pac executable path so it can be reconfigured at runtime (Settings).</summary>
public interface IPacExecutable
{
    string ExecutablePath { get; set; }
}
