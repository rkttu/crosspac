namespace Crosspac.App.Services;

/// <summary>Opens files or folders in the OS default handler, abstracted so view models stay UI-agnostic.</summary>
public interface IFileLauncherService
{
    /// <summary>
    /// Opens a file or folder with the operating system's default application.
    /// Returns false if it could not be launched (e.g. no handler, path missing).
    /// </summary>
    bool Open(string path);
}
