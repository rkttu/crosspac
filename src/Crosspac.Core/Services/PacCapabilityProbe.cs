using System.Text.RegularExpressions;
using Crosspac.Core.Execution;

namespace Crosspac.Core.Services;

/// <summary>
/// Runs <c>pac &lt;verb&gt; help</c> and parses the advertised flags. Results are cached per
/// verb (help output is stable for a given pac install), and access is serialized so
/// concurrent callers don't each spawn a probe for the same verb.
/// </summary>
public sealed partial class PacCapabilityProbe : IPacCapabilityProbe
{
    private readonly IPacRunner _runner;
    private readonly Dictionary<string, IReadOnlySet<string>> _cache = new();
    private readonly SemaphoreSlim _gate = new(1, 1);

    public PacCapabilityProbe(IPacRunner runner) => _runner = runner;

    public async Task<bool> SupportsFlagAsync(IReadOnlyList<string> verb, string flag, CancellationToken cancellationToken = default)
    {
        var flags = await GetFlagsAsync(verb, cancellationToken).ConfigureAwait(false);
        return flags.Contains(Normalize(flag));
    }

    public async Task<IReadOnlySet<string>> GetFlagsAsync(IReadOnlyList<string> verb, CancellationToken cancellationToken = default)
    {
        var key = string.Join(' ', verb);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var args = new List<string>(verb) { "help" };
            var result = await _runner.RunAsync(args, cancellationToken).ConfigureAwait(false);
            var flags = ParseFlags(result.StandardOutput);
            _cache[key] = flags;
            return flags;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Extracts flag names from help text — both the indented parameter list
    /// (<c>  --name   description</c>) and the usage line (<c>[--name]</c>).
    /// </summary>
    internal static IReadOnlySet<string> ParseFlags(string helpText)
    {
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in helpText.Split('\n'))
        {
            var detail = DetailFlag().Match(line);
            if (detail.Success)
                flags.Add(detail.Groups[1].Value.ToLowerInvariant());

            foreach (Match usage in UsageFlag().Matches(line))
                flags.Add(usage.Groups[1].Value.ToLowerInvariant());
        }

        return flags;
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        // Take the same gate the probes use so a clear can't race an in-flight probe writing
        // a stale entry back into the cache.
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _cache.Clear();
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string Normalize(string flag) => flag.TrimStart('-').ToLowerInvariant();

    [GeneratedRegex(@"^\s+--([A-Za-z0-9][A-Za-z0-9-]*)")]
    private static partial Regex DetailFlag();

    [GeneratedRegex(@"\[--([A-Za-z0-9][A-Za-z0-9-]*)\]")]
    private static partial Regex UsageFlag();
}
