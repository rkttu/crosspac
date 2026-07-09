using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

/// <summary>Wraps <c>pac auth</c>. Uses <c>--json</c> when the installed pac advertises it.</summary>
public sealed class AuthService : IAuthService
{
    private static readonly string[] ListVerb = { "auth", "list" };

    private readonly IPacRunner _runner;
    private readonly IPacCapabilityProbe? _capabilities;

    public AuthService(IPacRunner runner, IPacCapabilityProbe? capabilities = null)
    {
        _runner = runner;
        _capabilities = capabilities;
    }

    public async Task<IReadOnlyList<AuthProfile>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_capabilities is not null &&
            await _capabilities.SupportsFlagAsync(ListVerb, "--json", cancellationToken).ConfigureAwait(false))
        {
            var json = await _runner.RunAsync(new[] { "auth", "list", "--json" }, cancellationToken).ConfigureAwait(false);
            return ParseJson(json.StandardOutput);
        }

        var result = await _runner.RunAsync(ListVerb, cancellationToken).ConfigureAwait(false);
        return ParseText(result.StandardOutput);
    }

    public Task<PacCommandResult> SelectAsync(string index, CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "auth", "select", "--index", index }, cancellationToken);

    public Task<PacCommandResult> CreateAsync(string environment, string? name = null, CancellationToken cancellationToken = default)
    {
        var args = new List<string> { "auth", "create", "--environment", environment };
        if (!string.IsNullOrWhiteSpace(name))
        {
            args.Add("--name");
            args.Add(name!);
        }
        return _runner.RunAsync(args, cancellationToken);
    }

    private static IReadOnlyList<AuthProfile> ParseText(string output)
    {
        var profiles = new List<AuthProfile>();
        foreach (var line in TableParser.NonEmptyLines(output))
        {
            // Data rows begin with a bracketed index, e.g. "[1] * UNIVERSAL ...".
            if (!line.TrimStart().StartsWith('[')) continue;

            var columns = TableParser.SplitColumns(line);
            var index = columns.ElementAtOrDefault(0)?.Trim('[', ']', '*', ' ') ?? "";

            profiles.Add(new AuthProfile
            {
                Index = index,
                IsActive = line.Contains('*'),
                Kind = columns.FirstOrDefault(c =>
                    c.Equals("UNIVERSAL", StringComparison.OrdinalIgnoreCase) ||
                    c.Equals("DATAVERSE", StringComparison.OrdinalIgnoreCase)) ?? "",
                User = columns.FirstOrDefault(c => c.Contains('@')) ?? "",
                Raw = line,
            });
        }
        return profiles;
    }

    private static IReadOnlyList<AuthProfile> ParseJson(string json)
    {
        var profiles = new List<AuthProfile>();
        foreach (var o in PacJson.ParseObjects(json))
        {
            var index = PacJson.Str(o, "Index", "Number");
            var user = PacJson.Str(o, "User", "UserDisplayName", "UserPrincipalName", "Upn");
            profiles.Add(new AuthProfile
            {
                Index = index,
                IsActive = PacJson.Bool(o, "IsActive", "Active"),
                Kind = PacJson.Str(o, "Kind", "AuthKind"),
                Name = PacJson.Str(o, "Name", "FriendlyName"),
                User = user,
                Raw = $"[{index}] {user}".Trim(),
            });
        }
        return profiles;
    }
}
