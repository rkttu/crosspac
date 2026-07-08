using System.Threading.Tasks;

namespace PacDesk.App.Services;

/// <summary>Abstraction for modal dialogs so view models stay UI-framework-agnostic.</summary>
public interface IDialogService
{
    /// <summary>Shows a modal confirm dialog. Returns true only if the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message);
}
