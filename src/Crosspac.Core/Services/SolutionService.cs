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

            var columns = TableParser.SplitColumns(line);
            solutions.Add(new Solution
            {
                UniqueName = columns.ElementAtOrDefault(0) ?? "",
                FriendlyName = columns.ElementAtOrDefault(1) ?? "",
                Version = columns.ElementAtOrDefault(2) ?? "",
                // The managed column renders as a boolean-ish token (Yes/True/Managed).
                IsManaged = columns.Any(c =>
                    c.Equals("Managed", StringComparison.OrdinalIgnoreCase) ||
                    c.Equals("True", StringComparison.OrdinalIgnoreCase) ||
                    c.Equals("Yes", StringComparison.OrdinalIgnoreCase)),
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
