namespace Crosspac.Core.Execution;

/// <summary>A single line of output streamed from a <c>pac</c> process.</summary>
public sealed record PacOutputLine(string Text, bool IsError)
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
}
