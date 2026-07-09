namespace Crosspac.Core.Execution;

/// <summary>The outcome of a completed <c>pac</c> invocation.</summary>
public sealed record PacCommandResult(
    string CommandLine,
    int ExitCode,
    string StandardOutput,
    string StandardError,
    TimeSpan Duration)
{
    public bool Succeeded => ExitCode == 0;
}
