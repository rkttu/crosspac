using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Crosspac.App.Services;
using Crosspac.App.ViewModels;
using Crosspac.App.Views;
using Crosspac.Core.Configuration;
using Crosspac.Core.Execution;
using Crosspac.Core.Services;

namespace Crosspac.App;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Composition root: manual object graph (no DI container needed at MVP size).
            var settingsStore = new JsonSettingsStore();
            var settings = settingsStore.Load();

            var runner = new PacRunner(settings.PacExecutablePath);
            var capabilities = new PacCapabilityProbe(runner);
            var authService = new AuthService(runner, capabilities);
            var environmentService = new EnvironmentService(runner, capabilities);
            var solutionService = new SolutionService(runner, capabilities);
            var contextService = new ContextService(authService, environmentService);
            var dialogService = new DialogService();
            var pickerService = new StoragePickerService();

            var mainViewModel = new MainWindowViewModel(
                runner, authService, environmentService, solutionService,
                contextService, dialogService, pickerService, settingsStore, settings);

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
