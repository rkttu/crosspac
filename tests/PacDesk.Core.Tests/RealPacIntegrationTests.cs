using PacDesk.Core.Execution;
using PacDesk.Core.Services;
using Xunit;

namespace PacDesk.Core.Tests;

/// <summary>
/// Integration tests that drive the REAL <c>pac</c> binary. They verify the end-to-end
/// wrapper data path against whatever the machine is logged into. If <c>pac</c> isn't
/// installed/on PATH, each test returns early (passes trivially) so the suite stays
/// green on clean machines. Run just these with:
/// <c>dotnet test --filter Category=Integration</c>.
/// </summary>
[Trait("Category", "Integration")]
public class RealPacIntegrationTests
{
    [Fact]
    public async Task Real_pac_lists_at_least_one_auth_profile()
    {
        var runner = new PacRunner();
        if (!await runner.IsAvailableAsync())
            return; // pac not available — skip

        var profiles = await new AuthService(runner).ListAsync();

        Assert.NotEmpty(profiles);
        Assert.All(profiles, p => Assert.False(string.IsNullOrWhiteSpace(p.Raw)));
    }

    [Fact]
    public async Task Real_pac_lists_environments()
    {
        var runner = new PacRunner();
        if (!await runner.IsAvailableAsync())
            return; // pac not available — skip

        var environments = await new EnvironmentService(runner).ListAsync();

        // A logged-in tenant always exposes at least the Default environment.
        Assert.NotEmpty(environments);
        Assert.All(environments, e => Assert.StartsWith("https://", e.Url));
    }

    [Fact]
    public async Task Real_pac_resolves_active_context()
    {
        var runner = new PacRunner();
        if (!await runner.IsAvailableAsync())
            return; // pac not available — skip

        var auth = new AuthService(runner);
        var env = new EnvironmentService(runner);
        var context = await new ContextService(auth, env).GetActiveAsync();

        Assert.NotNull(context.ProfileUser);
    }

    [Fact]
    public async Task Real_pac_capability_probe_reads_solution_list_flags()
    {
        var runner = new PacRunner();
        if (!await runner.IsAvailableAsync())
            return; // pac not available — skip

        var probe = new PacCapabilityProbe(runner);
        var flags = await probe.GetFlagsAsync(new[] { "solution", "list" });

        // `--environment` is a stable flag on this verb; the probe must discover it.
        Assert.Contains("environment", flags);
    }
}
