using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

/// <summary>
/// Tenant-level environment administration via <c>pac admin</c> (create / reset / delete).
/// These are destructive operations; callers gate them behind the app's write mode.
/// </summary>
public interface IEnvironmentAdminService
{
    /// <summary>Creates a new Dataverse environment (<c>pac admin create</c>).</summary>
    Task<PacCommandResult> CreateAsync(EnvironmentCreateOptions options, CancellationToken cancellationToken = default);

    /// <summary>Resets an existing environment (<c>pac admin reset</c>). <paramref name="environment"/> is a URL or ID.</summary>
    Task<PacCommandResult> ResetAsync(string environment, CancellationToken cancellationToken = default);

    /// <summary>Deletes an environment from the tenant (<c>pac admin delete</c>). <paramref name="environment"/> is a URL or ID.</summary>
    Task<PacCommandResult> DeleteAsync(string environment, CancellationToken cancellationToken = default);
}
