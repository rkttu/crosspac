using PacDesk.Core.Services;
using Xunit;

namespace PacDesk.Core.Tests;

public class AuthServiceTests
{
    // Mirrors the real `pac auth list` layout (a single active UNIVERSAL profile).
    private const string SampleAuthList =
        "Index Active Kind      Name User                                          Cloud  Type Environment Environment Url\n" +
        "[1]   *      UNIVERSAL      user@contoso.onmicrosoft.com Public User             \n";

    [Fact]
    public async Task ListAsync_parses_active_universal_profile()
    {
        var runner = new FakePacRunner(SampleAuthList);
        var service = new AuthService(runner);

        var profiles = await service.ListAsync();

        var profile = Assert.Single(profiles);
        Assert.Equal("1", profile.Index);
        Assert.True(profile.IsActive);
        Assert.Equal("UNIVERSAL", profile.Kind);
        Assert.Contains("user@contoso.onmicrosoft.com", profile.User);
    }

    [Fact]
    public async Task ListAsync_invokes_pac_auth_list()
    {
        var runner = new FakePacRunner(SampleAuthList);
        var service = new AuthService(runner);

        await service.ListAsync();

        Assert.Equal(new[] { "auth", "list" }, runner.Invocations[0]);
    }

    [Fact]
    public async Task SelectAsync_builds_expected_arguments()
    {
        var runner = new FakePacRunner("");
        var service = new AuthService(runner);

        await service.SelectAsync("2");

        Assert.Equal(new[] { "auth", "select", "--index", "2" }, runner.Invocations[0]);
    }

    [Fact]
    public async Task CreateAsync_includes_name_when_provided()
    {
        var runner = new FakePacRunner("");
        var service = new AuthService(runner);

        await service.CreateAsync("HR-Dev", "MyOrg");

        Assert.Equal(
            new[] { "auth", "create", "--environment", "HR-Dev", "--name", "MyOrg" },
            runner.Invocations[0]);
    }
}
