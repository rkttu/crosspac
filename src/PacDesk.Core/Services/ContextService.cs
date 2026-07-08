using PacDesk.Core.Models;

namespace PacDesk.Core.Services;

/// <summary>
/// Derives the active context by reusing the auth and environment services and picking
/// the rows flagged active. Kept separate so the UI has a single call for the status bar.
/// </summary>
public sealed class ContextService : IContextService
{
    private readonly IAuthService _auth;
    private readonly IEnvironmentService _environments;

    public ContextService(IAuthService auth, IEnvironmentService environments)
    {
        _auth = auth;
        _environments = environments;
    }

    public async Task<ActiveContext> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _auth.ListAsync(cancellationToken).ConfigureAwait(false);
        var environments = await _environments.ListAsync(cancellationToken).ConfigureAwait(false);

        var profile = profiles.FirstOrDefault(p => p.IsActive);
        var environment = environments.FirstOrDefault(e => e.IsActive);

        return new ActiveContext(
            ProfileUser: string.IsNullOrWhiteSpace(profile?.User) ? "(none)" : profile!.User,
            ProfileIndex: profile?.Index ?? "",
            EnvironmentName: string.IsNullOrWhiteSpace(environment?.DisplayName) ? "(none selected)" : environment!.DisplayName,
            EnvironmentUrl: environment?.Url ?? "");
    }
}
