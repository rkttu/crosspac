using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

public interface ISolutionService
{
    Task<IReadOnlyList<Solution>> ListAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> ExportAsync(string name, string path, bool managed, CancellationToken cancellationToken = default);
    Task<PacCommandResult> ImportAsync(string path, CancellationToken cancellationToken = default);
    Task<PacCommandResult> PublishAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> DeleteAsync(string solutionName, CancellationToken cancellationToken = default);
}
