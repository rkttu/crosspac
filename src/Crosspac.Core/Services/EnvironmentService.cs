using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

/// <summary>Wraps <c>pac env</c>. Uses <c>--json</c> when available; else content-aware text parsing.</summary>
public sealed class EnvironmentService : IEnvironmentService
{
    private static readonly string[] ListVerb = { "env", "list" };

    private readonly IPacRunner _runner;
    private readonly IPacCapabilityProbe? _capabilities;

    public EnvironmentService(IPacRunner runner, IPacCapabilityProbe? capabilities = null)
    {
        _runner = runner;
        _capabilities = capabilities;
    }

    public async Task<IReadOnlyList<DataverseEnvironment>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_capabilities is not null &&
            await _capabilities.SupportsFlagAsync(ListVerb, "--json", cancellationToken).ConfigureAwait(false))
        {
            var json = await _runner.RunAsync(new[] { "env", "list", "--json" }, cancellationToken).ConfigureAwait(false);
            return ParseJson(json.StandardOutput);
        }

        var result = await _runner.RunAsync(ListVerb, cancellationToken).ConfigureAwait(false);
        return ParseText(result.StandardOutput);
    }

    public Task<PacCommandResult> SelectAsync(string environment, CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "env", "select", "--environment", environment }, cancellationToken);

    private static IReadOnlyList<DataverseEnvironment> ParseText(string output)
    {
        var environments = new List<DataverseEnvironment>();
        var lines = TableParser.NonEmptyLines(output).ToList();

        // Skip the "Connected as ..." preamble; the header row contains "Environment ID".
        var headerIndex = lines.FindIndex(l => l.Contains("Environment ID", StringComparison.OrdinalIgnoreCase));
        if (headerIndex < 0)
            return environments;

        // Content-aware parsing: pac aligns columns with as little as one space between
        // them, so whitespace-splitting merges the GUID/URL/unique-name. Instead we pull
        // the GUID (Environment ID) and URL out by shape, which is spacing-independent.
        for (var i = headerIndex + 1; i < lines.Count; i++)
        {
            var line = lines[i];

            var guid = TableParser.Guid().Match(line);
            if (!guid.Success)
                continue; // not a data row

            var url = TableParser.Url().Match(line);
            var lastToken = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";

            // Inactive rows start with spaces (empty Active column); an active row has a
            // marker at column 0.
            var isActive = line.Length > 0 && !char.IsWhiteSpace(line[0]);

            var displayName = line[..guid.Index].Trim();
            if (isActive)
            {
                var parts = displayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    displayName = parts[1].Trim();
            }

            environments.Add(new DataverseEnvironment
            {
                IsActive = isActive,
                DisplayName = displayName,
                EnvironmentId = guid.Value,
                Url = url.Success ? url.Value : "",
                UniqueName = lastToken,
                Raw = line,
            });
        }

        return environments;
    }

    private static IReadOnlyList<DataverseEnvironment> ParseJson(string json)
    {
        var environments = new List<DataverseEnvironment>();
        foreach (var o in PacJson.ParseObjects(json))
        {
            environments.Add(new DataverseEnvironment
            {
                IsActive = PacJson.Bool(o, "IsActive", "Active"),
                DisplayName = PacJson.Str(o, "DisplayName", "FriendlyName", "Name"),
                EnvironmentId = PacJson.Str(o, "EnvironmentId", "Id"),
                Url = PacJson.Str(o, "EnvironmentUrl", "Url"),
                UniqueName = PacJson.Str(o, "UniqueName", "Name"),
                Raw = PacJson.Str(o, "DisplayName", "Name"),
            });
        }
        return environments;
    }
}
