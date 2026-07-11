using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

/// <summary>Wraps <c>pac admin</c> for tenant-level environment lifecycle operations.</summary>
public sealed class EnvironmentAdminService : IEnvironmentAdminService
{
    private readonly IPacRunner _runner;

    public EnvironmentAdminService(IPacRunner runner) => _runner = runner;

    public Task<PacCommandResult> CreateAsync(EnvironmentCreateOptions options, CancellationToken cancellationToken = default)
    {
        // --type is required; the optional flags are added only when supplied so pac falls back
        // to its own defaults (USD / English / unitedstates) otherwise.
        var args = new List<string> { "admin", "create", "--type", options.Type.ToString() };
        AddOptional(args, "--name", options.Name);
        AddOptional(args, "--domain", options.Domain);
        AddOptional(args, "--currency", options.Currency);
        AddOptional(args, "--region", options.Region);
        AddOptional(args, "--language", options.Language);
        return _runner.RunAsync(args, cancellationToken);
    }

    public Task<PacCommandResult> ResetAsync(string environment, CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "admin", "reset", "--environment", environment }, cancellationToken);

    public Task<PacCommandResult> DeleteAsync(string environment, CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "admin", "delete", "--environment", environment }, cancellationToken);

    private static void AddOptional(List<string> args, string flag, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        args.Add(flag);
        args.Add(value.Trim());
    }
}
