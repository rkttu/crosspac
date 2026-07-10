using Crosspac.Core.Execution;
using Crosspac.Core.Models;

namespace Crosspac.Core.Services;

/// <summary>Wraps <c>pac solution</c>. Uses <c>--json</c> for listing when available.</summary>
public sealed class SolutionService : ISolutionService
{
    private static readonly string[] ListVerb = { "solution", "list" };

    private readonly IPacRunner _runner;
    private readonly IPacCapabilityProbe? _capabilities;

    public SolutionService(IPacRunner runner, IPacCapabilityProbe? capabilities = null)
    {
        _runner = runner;
        _capabilities = capabilities;
    }

    public async Task<IReadOnlyList<Solution>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_capabilities is not null &&
            await _capabilities.SupportsFlagAsync(ListVerb, "--json", cancellationToken).ConfigureAwait(false))
        {
            var json = await _runner.RunAsync(new[] { "solution", "list", "--json" }, cancellationToken).ConfigureAwait(false);
            return ParseJson(json.StandardOutput);
        }

        var result = await _runner.RunAsync(ListVerb, cancellationToken).ConfigureAwait(false);
        return ParseText(result.StandardOutput);
    }

    public Task<PacCommandResult> ExportAsync(string name, string path, bool managed, CancellationToken cancellationToken = default)
    {
        var args = new List<string> { "solution", "export", "--name", name, "--path", path };
        if (managed)
            args.Add("--managed");
        return _runner.RunAsync(args, cancellationToken);
    }

    public Task<PacCommandResult> ImportAsync(string path, CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "solution", "import", "--path", path }, cancellationToken);

    public Task<PacCommandResult> PublishAsync(CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "solution", "publish" }, cancellationToken);

    public Task<PacCommandResult> DeleteAsync(string solutionName, CancellationToken cancellationToken = default)
        => _runner.RunAsync(new[] { "solution", "delete", "--solution-name", solutionName }, cancellationToken);

    private static IReadOnlyList<Solution> ParseText(string output)
    {
        var solutions = new List<Solution>();
        var pastHeader = false;

        foreach (var line in TableParser.NonEmptyLines(output))
        {
            if (line.Contains("Unique Name", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("Friendly Name", StringComparison.OrdinalIgnoreCase))
            {
                pastHeader = true;
                continue;
            }
            if (!pastHeader) continue;

            // Columns are Unique Name | Friendly Name | Version | Managed. A friendly name
            // can contain spaces (incl. CJK) and pac separates columns with a single space,
            // so a whitespace split can't be mapped positionally in the middle. Instead we
            // anchor on the outer tokens — unique name first, managed last, version next —
            // and treat everything between as the (possibly multi-word) friendly name.
            var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3) continue;

            var managed = tokens[^1];
            solutions.Add(new Solution
            {
                UniqueName = tokens[0],
                FriendlyName = string.Join(' ', tokens[1..^2]),
                Version = tokens[^2],
                // The managed column renders as a boolean-ish token (Yes/True/Managed).
                IsManaged = managed.Equals("Managed", StringComparison.OrdinalIgnoreCase) ||
                            managed.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                            managed.Equals("Yes", StringComparison.OrdinalIgnoreCase),
                Raw = line,
            });
        }

        return solutions;
    }

    private static IReadOnlyList<Solution> ParseJson(string json)
    {
        var solutions = new List<Solution>();
        foreach (var o in PacJson.ParseObjects(json))
        {
            solutions.Add(new Solution
            {
                UniqueName = PacJson.Str(o, "SolutionUniqueName", "UniqueName", "Name"),
                FriendlyName = PacJson.Str(o, "FriendlyName", "DisplayName"),
                Version = PacJson.Str(o, "VersionNumber", "Version"),
                IsManaged = PacJson.Bool(o, "IsManaged", "Managed"),
                Raw = PacJson.Str(o, "SolutionUniqueName", "UniqueName", "Name"),
            });
        }
        return solutions;
    }
}
