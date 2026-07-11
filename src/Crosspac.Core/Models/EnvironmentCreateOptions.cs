namespace Crosspac.Core.Models;

/// <summary>The environment kinds accepted by <c>pac admin create --type</c>.</summary>
public enum EnvironmentType
{
    Trial,
    Sandbox,
    Production,
    Developer,
    Teams,
    SubscriptionBasedTrial,
}

/// <summary>
/// Parameters for <c>pac admin create</c>. Only <see cref="Type"/> is required; the rest map to
/// optional flags and are omitted when blank so pac applies its own defaults (USD / English /
/// unitedstates).
/// </summary>
public sealed record EnvironmentCreateOptions(
    EnvironmentType Type,
    string? Name = null,
    string? Domain = null,
    string? Currency = null,
    string? Region = null,
    string? Language = null);
