using PacDesk.Core.Execution;

namespace PacDesk.Core.Tests;

/// <summary>
/// Test double that computes its stdout from the argument list, so a single runner can
/// answer both a capability probe (<c>… help</c>) and the subsequent data call differently.
/// </summary>
internal sealed class ScriptedPacRunner : IPacRunner
{
    private readonly Func<IReadOnlyList<string>, string> _responder;

    public ScriptedPacRunner(Func<IReadOnlyList<string>, string> responder) => _responder = responder;

    public List<string[]> Invocations { get; } = new();

    public event EventHandler<PacOutputLine>? OutputReceived;

    public Task<PacCommandResult> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken = default)
    {
        Invocations.Add(args.ToArray());
        var output = _responder(args);

        foreach (var line in output.Split('\n'))
            OutputReceived?.Invoke(this, new PacOutputLine(line.TrimEnd('\r'), IsError: false));

        return Task.FromResult(new PacCommandResult(
            "pac " + string.Join(' ', args), 0, output, "", TimeSpan.Zero));
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
