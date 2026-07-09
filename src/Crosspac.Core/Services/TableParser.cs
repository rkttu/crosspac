using System.Text.RegularExpressions;

namespace Crosspac.Core.Services;

/// <summary>
/// Heuristic parser for pac's column-aligned text tables. Splits a line on runs of
/// 2+ whitespace characters. This is intentionally simple and lives behind the service
/// layer so it can be swapped for <c>--json</c> parsing where a verb supports it.
/// See ARCHITECTURE.md § "Output parsing — the known fragile point".
/// </summary>
internal static partial class TableParser
{
    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex ColumnSeparator();

    [GeneratedRegex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
    internal static partial Regex Guid();

    [GeneratedRegex(@"https?://\S+")]
    internal static partial Regex Url();

    public static string[] SplitColumns(string line) =>
        ColumnSeparator().Split(line.Trim());

    /// <summary>Splits raw stdout into non-empty, trimmed lines.</summary>
    public static IEnumerable<string> NonEmptyLines(string output) =>
        output.Split('\n')
              .Select(l => l.TrimEnd('\r'))
              .Where(l => !string.IsNullOrWhiteSpace(l));
}
