using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

public interface IEnvironmentService
{
    Task<IReadOnlyList<DataverseEnvironment>> ListAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> SelectAsync(string environment, CancellationToken cancellationToken = default);
}
