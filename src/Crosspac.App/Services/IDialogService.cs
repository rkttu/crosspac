using System.Threading.Tasks;

namespace Crosspac.App.Services;

/// <summary>Abstraction for modal dialogs so view models stay UI-framework-agnostic.</summary>
public interface IDialogService
{
    /// <summary>Shows a modal confirm dialog. Returns true only if the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>
    /// Shows the solution export dialog (package type + destination). Returns the chosen
    /// options, or null if the user cancelled.
    /// </summary>
    Task<ExportOptions?> RequestExportAsync(string solutionUniqueName);
}

/// <summary>Result of the export dialog: the package type and the chosen .zip destination.</summary>
public sealed record ExportOptions(bool Managed, string Destination);
