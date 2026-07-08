namespace PacDesk.Core.Execution;

/// <summary>
/// Thrown when the <c>pac</c> executable cannot be started at all
/// (e.g. not installed or not on PATH), as opposed to a non-zero exit code.
/// </summary>
public sealed class PacExecutionException : Exception
{
    public PacExecutionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
