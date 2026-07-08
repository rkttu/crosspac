using PacDesk.Core.Services;
using Xunit;

namespace PacDesk.Core.Tests;

public class PacCapabilityProbeTests
{
    // Captured from a real `pac solution list help` (pac 2.5.1) — note: no --json.
    private const string RealSolutionListHelp =
        "Help:\n" +
        "List all Solutions from the current Dataverse organization\n\n" +
        "Commands:\n" +
        "Usage: pac solution list [--environment] [--includeSystemSolutions]\n\n" +
        "  --environment               Specifies the target Dataverse. (alias: -env)\n" +
        "  --includeSystemSolutions    Include system solutions like those published by Microsoft\n";

    [Fact]
    public void ParseFlags_reads_flags_from_real_help_and_finds_no_json()
    {
        var flags = PacCapabilityProbe.ParseFlags(RealSolutionListHelp);

        Assert.Contains("environment", flags);
        Assert.Contains("includesystemsolutions", flags);
        Assert.DoesNotContain("json", flags);
    }

    [Fact]
    public async Task SupportsFlagAsync_true_when_help_advertises_json()
    {
        var runner = new ScriptedPacRunner(_ =>
            "Usage: pac foo bar [--environment] [--json]\n  --json   Returns output as JSON.\n");
        var probe = new PacCapabilityProbe(runner);

        Assert.True(await probe.SupportsFlagAsync(new[] { "foo", "bar" }, "--json"));
    }

    [Fact]
    public async Task SupportsFlagAsync_false_when_flag_absent()
    {
        var runner = new ScriptedPacRunner(_ => "  --environment   Specifies the target Dataverse.\n");
        var probe = new PacCapabilityProbe(runner);

        Assert.False(await probe.SupportsFlagAsync(new[] { "foo", "bar" }, "--json"));
    }

    [Fact]
    public async Task GetFlagsAsync_caches_and_probes_each_verb_once()
    {
        var runner = new ScriptedPacRunner(_ => "  --json   x\n");
        var probe = new PacCapabilityProbe(runner);

        await probe.GetFlagsAsync(new[] { "auth", "list" });
        await probe.GetFlagsAsync(new[] { "auth", "list" });

        Assert.Single(runner.Invocations);
        Assert.Equal(new[] { "auth", "list", "help" }, runner.Invocations[0]);
    }
}
