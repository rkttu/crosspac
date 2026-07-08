using PacDesk.Core.Models;

namespace PacDesk.Core.Services;

public interface IContextService
{
    /// <summary>Resolves the active auth profile + active environment.</summary>
    Task<ActiveContext> GetActiveAsync(CancellationToken cancellationToken = default);
}
