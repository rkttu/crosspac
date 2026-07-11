namespace Crosspac.Core.Services;

/// <summary>
/// Mechanically discovers what a given pac verb supports by reading its own
/// <c>pac &lt;verb&gt; help</c> output. Lets services opt into <c>--json</c> only when the
/// installed pac actually advertises it, instead of assuming and failing at runtime.
/// </summary>
public interface IPacCapabilityProbe
{
    /// <summary>All long flag names (without leading dashes, lower-cased) the verb accepts.</summary>
    Task<IReadOnlySet<string>> GetFlagsAsync(IReadOnlyList<string> verb, CancellationToken cancellationToken = default);

    /// <summary>True if the verb advertises the given flag (e.g. <c>"--json"</c>).</summary>
    Task<bool> SupportsFlagAsync(IReadOnlyList<string> verb, string flag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discards every cached probe result. Call when the pac executable is reconfigured, since
    /// a different binary can advertise different flags for the same verb.
    /// </summary>
    Task ResetAsync(CancellationToken cancellationToken = default);
}
