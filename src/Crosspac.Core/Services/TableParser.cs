using System.Text.RegularExpressions;

namespace Crosspac.Core.Services;

/// <summary>
/// Helpers for reading pac's column-aligned text tables. pac packs columns with as
/// little as a single space and left-aligns wide (CJK) values by display width, so
/// splitting on whitespace runs is unreliable; services instead anchor on shape
/// (bracketed index, email, GUID/URL, first/last tokens). These helpers just isolate
/// the pieces that are shape-detectable. Prefer <c>--json</c> where a verb supports it.
/// See ARCHITECTURE.md § "Output parsing — the known fragile point".
/// </summary>
internal static partial class TableParser
{
    [GeneratedRegex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
    internal static partial Regex Guid();

    [GeneratedRegex(@"https?://\S+")]
    internal static partial Regex Url();

    /// <summary>Splits raw stdout into non-empty, trimmed lines.</summary>
    public static IEnumerable<string> NonEmptyLines(string output) =>
        output.Split('\n')
              .Select(l => l.TrimEnd('\r'))
              .Where(l => !string.IsNullOrWhiteSpace(l));
}
