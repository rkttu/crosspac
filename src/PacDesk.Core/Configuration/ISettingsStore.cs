using PacDesk.Core.Models;

namespace PacDesk.Core.Configuration;

public interface ISettingsStore
{
    /// <summary>Absolute path of the backing settings file (shown to the user).</summary>
    string FilePath { get; }

    /// <summary>Loads settings, returning defaults if the file is missing or unreadable.</summary>
    AppSettings Load();

    /// <summary>Persists settings (best-effort; never throws).</summary>
    void Save(AppSettings settings);
}
