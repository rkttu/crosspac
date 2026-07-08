using Avalonia;

namespace PacDesk.App;

internal static class Program
{
    // Avalonia requires an STA thread and must not use any Avalonia/UI APIs before
    // AppMain is called: things aren't initialized yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
