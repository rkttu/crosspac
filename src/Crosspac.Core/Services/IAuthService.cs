using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

public interface IAuthService
{
    Task<IReadOnlyList<AuthProfile>> ListAsync(CancellationToken cancellationToken = default);
    Task<PacCommandResult> SelectAsync(string index, CancellationToken cancellationToken = default);
    Task<PacCommandResult> CreateAsync(string environment, string? name = null, CancellationToken cancellationToken = default);
}
