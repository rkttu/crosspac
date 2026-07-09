namespace Crosspac.Core.Execution;

/// <summary>Abstraction over launching the <c>pac</c> CLI. Mockable for tests.</summary>
public interface IPacRunner
{
    /// <summary>Raised for every line of stdout/stderr as the process runs.</summary>
    event EventHandler<PacOutputLine>? OutputReceived;

    /// <summary>Runs <c>pac</c> with the given argument list and awaits completion.</summary>
    Task<PacCommandResult> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken = default);

    /// <summary>Returns true if <c>pac</c> can be started and responds successfully.</summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
