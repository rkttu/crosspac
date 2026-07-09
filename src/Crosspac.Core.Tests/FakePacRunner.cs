using Crosspac.Core.Execution;

namespace Crosspac.Core.Tests;

/// <summary>
/// Test double for <see cref="IPacRunner"/> that returns canned stdout instead of
/// launching a real process. Records every invocation's argument list so tests can
/// assert the exact pac command a service built.
/// </summary>
internal sealed class FakePacRunner : IPacRunner
{
    private readonly string _standardOutput;
    private readonly int _exitCode;

    public FakePacRunner(string standardOutput, int exitCode = 0)
    {
        _standardOutput = standardOutput;
        _exitCode = exitCode;
    }

    public List<string[]> Invocations { get; } = new();

    public event EventHandler<PacOutputLine>? OutputReceived;

    public Task<PacCommandResult> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken = default)
    {
        Invocations.Add(args.ToArray());

        foreach (var line in _standardOutput.Split('\n'))
            OutputReceived?.Invoke(this, new PacOutputLine(line.TrimEnd('\r'), IsError: false));

        return Task.FromResult(new PacCommandResult(
            "pac " + string.Join(' ', args),
            _exitCode,
            _standardOutput,
            StandardError: "",
            Duration: TimeSpan.Zero));
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
