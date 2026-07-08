using PacDesk.Core.Execution;
using PacDesk.Core.Models;

namespace PacDesk.Core.Services;

public interface ISolutionService
{
    Task<IReadOnlyList<Solution>> ListAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> ExportAsync(string name, string path, bool managed, CancellationToken cancellationToken = default);
    Task<PacCommandResult> ImportAsync(string path, CancellationToken cancellationToken = default);
    Task<PacCommandResult> PublishAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> DeleteAsync(string solutionName, CancellationToken cancellationToken = default);
}
