namespace Crosspac.Core.Models;

/// <summary>A Dataverse solution as listed by <c>pac solution list</c>.</summary>
public sealed class Solution
{
    public string UniqueName { get; init; } = "";
    public string FriendlyName { get; init; } = "";
    public string Version { get; init; } = "";
    public bool IsManaged { get; init; }
    public string Raw { get; init; } = "";
}
