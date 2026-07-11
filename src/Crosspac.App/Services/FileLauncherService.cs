using System;
using System.Diagnostics;

namespace Crosspac.App.Services;

/// <summary>
/// Opens a path with the OS default handler. Each platform has its own launcher:
/// Windows resolves the file association via the shell, macOS uses <c>open</c>, and other
/// Unix-likes use <c>xdg-open</c>. Arguments go through <see cref="ProcessStartInfo.ArgumentList"/>
/// so paths with spaces don't need manual quoting.
/// </summary>
public sealed class FileLauncherService : IFileLauncherService
{
    public bool Open(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                // UseShellExecute lets the Windows shell pick the associated app for the file type.
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", new[] { path });
            }
            else
            {
                Process.Start("xdg-open", new[] { path });
            }

            return true;
        }
        catch
        {
            // No associated handler / launcher missing — report failure so the caller can surface it.
            return false;
        }
    }
}
