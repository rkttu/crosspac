using System.Threading.Tasks;

namespace Crosspac.App.Services;

/// <summary>System clipboard access, abstracted so view models stay UI-agnostic.</summary>
public interface IClipboardService
{
    /// <summary>Copies text to the system clipboard.</summary>
    Task SetTextAsync(string text);
}
