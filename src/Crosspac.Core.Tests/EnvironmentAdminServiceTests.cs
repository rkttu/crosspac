using Crosspac.Core.Models;
using Crosspac.Core.Services;
using Xunit;

namespace Crosspac.Core.Tests;

public class EnvironmentAdminServiceTests
{
    [Fact]
    public async Task CreateAsync_includes_only_supplied_optional_flags()
    {
        var runner = new FakePacRunner("");
        var service = new EnvironmentAdminService(runner);

        await service.CreateAsync(new EnvironmentCreateOptions(
            EnvironmentType.Sandbox, Name: "Contoso Test", Domain: "contosotest"));

        Assert.Equal(
            new[] { "admin", "create", "--type", "Sandbox", "--name", "Contoso Test", "--domain", "contosotest" },
            runner.Invocations[0]);
    }

    [Fact]
    public async Task CreateAsync_with_only_type_omits_optional_flags()
    {
        var runner = new FakePacRunner("");
        var service = new EnvironmentAdminService(runner);

        await service.CreateAsync(new EnvironmentCreateOptions(EnvironmentType.Developer));

        Assert.Equal(
            new[] { "admin", "create", "--type", "Developer" },
            runner.Invocations[0]);
    }

    [Fact]
    public async Task CreateAsync_trims_optional_values_and_skips_whitespace()
    {
        var runner = new FakePacRunner("");
        var service = new EnvironmentAdminService(runner);

        await service.CreateAsync(new EnvironmentCreateOptions(
            EnvironmentType.Production, Name: "  Prod  ", Currency: "   ", Region: "europe"));

        Assert.Equal(
            new[] { "admin", "create", "--type", "Production", "--name", "Prod", "--region", "europe" },
            runner.Invocations[0]);
    }

    [Fact]
    public async Task ResetAsync_and_DeleteAsync_target_the_environment()
    {
        var runner = new FakePacRunner("");
        var service = new EnvironmentAdminService(runner);

        await service.ResetAsync("00000000-0000-0000-0000-000000000000");
        await service.DeleteAsync("https://contoso.crm.dynamics.com/");

        Assert.Equal(
            new[] { "admin", "reset", "--environment", "00000000-0000-0000-0000-000000000000" },
            runner.Invocations[0]);
        Assert.Equal(
            new[] { "admin", "delete", "--environment", "https://contoso.crm.dynamics.com/" },
            runner.Invocations[1]);
    }
}
