using PacDesk.Core.Services;
using Xunit;

namespace PacDesk.Core.Tests;

/// <summary>
/// Verifies the JSON path: when the capability probe reports <c>--json</c> support, the
/// service requests JSON and parses it. Exercises the forward-compatible branch that
/// activates once a pac version emits JSON for `solution list`.
/// </summary>
public class SolutionServiceJsonTests
{
    private const string SolutionListJson =
        "[" +
        "{\"SolutionUniqueName\":\"mysolution\",\"FriendlyName\":\"My Solution\",\"VersionNumber\":\"1.0.0.0\",\"IsManaged\":false}," +
        "{\"SolutionUniqueName\":\"anothersolution\",\"FriendlyName\":\"Another Solution\",\"VersionNumber\":\"2.1.0.0\",\"IsManaged\":true}" +
        "]";

    [Fact]
    public async Task ListAsync_uses_json_path_when_supported()
    {
        var runner = new ScriptedPacRunner(args =>
        {
            if (args.Contains("help"))
                return "Usage: pac solution list [--environment] [--json]\n  --json   Returns output as JSON.\n";
            if (args.Contains("--json"))
                return SolutionListJson;
            return "SHOULD_NOT_USE_TEXT_PATH";
        });

        var service = new SolutionService(runner, new PacCapabilityProbe(runner));

        var solutions = await service.ListAsync();

        Assert.Equal(2, solutions.Count);
        Assert.Equal("mysolution", solutions[0].UniqueName);
        Assert.Equal("My Solution", solutions[0].FriendlyName);
        Assert.Equal("1.0.0.0", solutions[0].Version);
        Assert.False(solutions[0].IsManaged);
        Assert.True(solutions[1].IsManaged);

        // The actual data call carried --json.
        Assert.Contains(runner.Invocations, a => a.Contains("--json"));
    }
}
