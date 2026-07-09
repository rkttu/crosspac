namespace Crosspac.Core.Models;

/// <summary>A Dataverse environment (organization) as listed by <c>pac env list</c>.</summary>
public sealed class DataverseEnvironment
{
    public bool IsActive { get; init; }
    public string DisplayName { get; init; } = "";
    public string EnvironmentId { get; init; } = "";
    public string Url { get; init; } = "";
    public string UniqueName { get; init; } = "";
    public string Raw { get; init; } = "";

    /// <summary>Preferred value to pass to <c>pac env select --environment</c>.</summary>
    public string SelectionTarget =>
        !string.IsNullOrWhiteSpace(EnvironmentId) ? EnvironmentId : Url;
}
