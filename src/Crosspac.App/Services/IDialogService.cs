using System.Threading.Tasks;
using Crosspac.Core.Models;

namespace Crosspac.App.Services;

/// <summary>Abstraction for modal dialogs so view models stay UI-framework-agnostic.</summary>
public interface IDialogService
{
    /// <summary>Shows a modal confirm dialog. Returns true only if the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>
    /// Shows the solution export dialog (destination and export switches). Returns the chosen
    /// options, or null if the user cancelled.
    /// </summary>
    Task<ExportOptions?> RequestExportAsync(string solutionUniqueName);

    /// <summary>
    /// Shows the create-environment dialog. Returns the chosen options, or null if cancelled.
    /// </summary>
    Task<EnvironmentCreateOptions?> RequestCreateEnvironmentAsync();
}

/// <summary>Result of the export dialog: the destination .zip and the export switches.</summary>
public sealed record ExportOptions(string Destination, bool Managed, bool Overwrite, bool Async);
