using System.Threading.Tasks;

namespace Crosspac.App.Services;

/// <summary>Native file/folder pickers, abstracted so view models stay UI-agnostic.</summary>
public interface IStoragePickerService
{
    /// <summary>Picks a single <c>.zip</c> solution package. Returns null if cancelled.</summary>
    Task<string?> PickZipFileAsync();

    /// <summary>Picks a single folder. Returns null if cancelled.</summary>
    Task<string?> PickFolderAsync();

    /// <summary>Picks a single file (no type filter). Returns null if cancelled.</summary>
    Task<string?> PickFileAsync(string title);
}
