using System.Threading.Tasks;

namespace Crosspac.App.Services;

/// <summary>Abstraction for modal dialogs so view models stay UI-framework-agnostic.</summary>
public interface IDialogService
{
    /// <summary>Shows a modal confirm dialog. Returns true only if the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>
    /// Shows the solution export dialog (name, destination, and export switches). Returns the
    /// chosen options, or null if the user cancelled.
    /// </summary>
    Task<ExportOptions?> RequestExportAsync(string solutionUniqueName);
}

/// <summary>
/// Result of the export dialog. <see cref="Name"/> is already trimmed and defaulted to the
/// solution's unique name, so it can be passed straight to <c>--name</c>.
/// </summary>
public sealed record ExportOptions(string Name, string Destination, bool Managed, bool Overwrite, bool Async);
