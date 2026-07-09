using System.Diagnostics;
using System.Text;

namespace Crosspac.Core.Execution;

/// <summary>
/// Default <see cref="IPacRunner"/> that shells out to the real <c>pac</c> binary via
/// <see cref="Process"/>. Arguments are passed through <see cref="ProcessStartInfo.ArgumentList"/>
/// so quoting/escaping is handled by the runtime — never build a command string by hand.
/// </summary>
public sealed class PacRunner : IPacRunner, IPacExecutable
{
    /// <param name="executablePath">
    /// Path to the pac executable. Defaults to <c>"pac"</c>, resolved via PATH on all platforms.
    /// </param>
    public PacRunner(string? executablePath = null)
        => ExecutablePath = string.IsNullOrWhiteSpace(executablePath) ? "pac" : executablePath;

    /// <summary>The pac executable path; can be reconfigured at runtime via Settings.</summary>
    public string ExecutablePath { get; set; }

    public event EventHandler<PacOutputLine>? OutputReceived;

    public async Task<PacCommandResult> RunAsync(
        IReadOnlyList<string> args,
        CancellationToken cancellationToken = default)
    {
        var executable = ExecutablePath;

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        var commandLine = $"{executable} {string.Join(' ', args)}";
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            stdout.AppendLine(e.Data);
            OutputReceived?.Invoke(this, new PacOutputLine(e.Data, IsError: false));
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            stderr.AppendLine(e.Data);
            OutputReceived?.Invoke(this, new PacOutputLine(e.Data, IsError: true));
        };

        // Echo the command being run so the log reads like a terminal.
        OutputReceived?.Invoke(this, new PacOutputLine($"> {commandLine}", IsError: false));

        var stopwatch = Stopwatch.StartNew();
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new PacExecutionException(
                $"Could not start '{executable}'. Is the Power Platform CLI installed and on PATH?",
                ex);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Cancellation must actually terminate the child process, not just abandon the wait.
        using var registration = cancellationToken.Register(static state =>
        {
            var proc = (Process)state!;
            try
            {
                if (!proc.HasExited)
                    proc.Kill(entireProcessTree: true);
            }
            catch
            {
                // Process already exited between the check and the kill — nothing to do.
            }
        }, process);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        return new PacCommandResult(
            commandLine,
            process.ExitCode,
            stdout.ToString(),
            stderr.ToString(),
            stopwatch.Elapsed);
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RunAsync(new[] { "help" }, cancellationToken).ConfigureAwait(false);
            return result.ExitCode == 0;
        }
        catch (PacExecutionException)
        {
            return false;
        }
    }
}
