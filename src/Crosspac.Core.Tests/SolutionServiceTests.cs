using Crosspac.Core.Services;
using Xunit;

namespace Crosspac.Core.Tests;

public class SolutionServiceTests
{
    private const string SampleSolutionList =
        "Unique Name              Friendly Name            Version      Managed\n" +
        "mysolution               My Solution              1.0.0.0      No\n" +
        "anothersolution          Another Solution         2.1.0.0      Yes\n";

    [Fact]
    public async Task ListAsync_parses_solutions_and_managed_flag()
    {
        var runner = new FakePacRunner(SampleSolutionList);
        var service = new SolutionService(runner);

        var solutions = await service.ListAsync();

        Assert.Equal(2, solutions.Count);
        Assert.Equal("mysolution", solutions[0].UniqueName);
        Assert.Equal("My Solution", solutions[0].FriendlyName);
        Assert.Equal("1.0.0.0", solutions[0].Version);
        Assert.False(solutions[0].IsManaged);
        Assert.True(solutions[1].IsManaged);
    }

    [Fact]
    public async Task ExportAsync_adds_switch_flags_only_when_requested()
    {
        var runner = new FakePacRunner("");
        var service = new SolutionService(runner);

        await service.ExportAsync("mysolution", "./out", managed: true, overwrite: true, async: true);
        await service.ExportAsync("mysolution", "./out", managed: false, overwrite: false, async: false);

        Assert.Equal(
            new[] { "solution", "export", "--name", "mysolution", "--path", "./out", "--managed", "--overwrite", "--async" },
            runner.Invocations[0]);
        Assert.Equal(
            new[] { "solution", "export", "--name", "mysolution", "--path", "./out" },
            runner.Invocations[1]);
    }

    [Fact]
    public async Task ImportAsync_builds_expected_arguments()
    {
        var runner = new FakePacRunner("");
        var service = new SolutionService(runner);

        await service.ImportAsync("./MySolution.zip");

        Assert.Equal(
            new[] { "solution", "import", "--path", "./MySolution.zip" },
            runner.Invocations[0]);
    }
}
