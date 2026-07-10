using System.Threading.Tasks;

namespace Crosspac.App.Services;

/// <summary>Native file pickers, abstracted so view models stay UI-agnostic.</summary>
public interface IStoragePickerService
{
    /// <summary>Picks a single <c>.zip</c> solution package. Returns null if cancelled.</summary>
    Task<string?> PickZipFileAsync();

    /// <summary>Picks a single file (no type filter). Returns null if cancelled.</summary>
    Task<string?> PickFileAsync(string title);

    /// <summary>
    /// Prompts for a save location, filtered to the given extension (e.g. "zip", "log").
    /// Returns the chosen path, or null if cancelled.
    /// </summary>
    Task<string?> PickSaveFilePathAsync(string title, string suggestedFileName, string defaultExtension);
}
