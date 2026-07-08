using PacDesk.Core.Execution;
using PacDesk.Core.Models;

namespace PacDesk.Core.Services;

public interface IEnvironmentService
{
    Task<IReadOnlyList<DataverseEnvironment>> ListAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> SelectAsync(string environment, CancellationToken cancellationToken = default);
}
