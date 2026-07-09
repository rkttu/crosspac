using Crosspac.Core.Services;
using Xunit;

namespace Crosspac.Core.Tests;

public class EnvironmentServiceTests
{
    // Mirrors the real `pac env list` layout: a preamble line, a header, then rows
    // with a blank (inactive) Active column.
    private const string SampleEnvList =
        "Connected as user@contoso.onmicrosoft.com\n" +
        "Active Display Name         Environment ID                       Environment URL                         Unique Name\n" +
        "       Default Environment  4887d30e-e1d4-4850-977d-787b6d88f872 https://orgdddecdd8.crm21.dynamics.com/ unqfb4dd4d4a429f111a7e5002248f86\n" +
        "       mzc-dev-environment  06d8bc5f-3826-e8c6-84c4-2924f604399e https://orgc658acec.crm21.dynamics.com/ unq7dd581444c74f1118068002248f86\n";

    [Fact]
    public async Task ListAsync_skips_preamble_and_header_and_parses_rows()
    {
        var runner = new FakePacRunner(SampleEnvList);
        var service = new EnvironmentService(runner);

        var environments = await service.ListAsync();

        Assert.Equal(2, environments.Count);

        var first = environments[0];
        Assert.Equal("Default Environment", first.DisplayName);
        Assert.Equal("4887d30e-e1d4-4850-977d-787b6d88f872", first.EnvironmentId);
        Assert.Equal("https://orgdddecdd8.crm21.dynamics.com/", first.Url);
        Assert.Equal("unqfb4dd4d4a429f111a7e5002248f86", first.UniqueName);
        Assert.False(first.IsActive);
    }

    [Fact]
    public async Task ListAsync_environment_id_is_the_selection_target()
    {
        var runner = new FakePacRunner(SampleEnvList);
        var service = new EnvironmentService(runner);

        var environments = await service.ListAsync();

        Assert.Equal(environments[1].EnvironmentId, environments[1].SelectionTarget);
    }

    [Fact]
    public async Task SelectAsync_builds_expected_arguments()
    {
        var runner = new FakePacRunner("");
        var service = new EnvironmentService(runner);

        await service.SelectAsync("https://orgc658acec.crm21.dynamics.com/");

        Assert.Equal(
            new[] { "env", "select", "--environment", "https://orgc658acec.crm21.dynamics.com/" },
            runner.Invocations[0]);
    }
}
