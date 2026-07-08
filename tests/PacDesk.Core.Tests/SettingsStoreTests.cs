using PacDesk.Core.Configuration;
using PacDesk.Core.Models;
using Xunit;

namespace PacDesk.Core.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void Save_then_Load_round_trips_values()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pacdesk-{Guid.NewGuid():N}.json");
        try
        {
            var store = new JsonSettingsStore(path);
            store.Save(new AppSettings
            {
                WindowWidth = 1234,
                WindowHeight = 850,
                PacExecutablePath = "/usr/local/bin/pac",
            });

            var loaded = store.Load();

            Assert.Equal(1234, loaded.WindowWidth);
            Assert.Equal(850, loaded.WindowHeight);
            Assert.Equal("/usr/local/bin/pac", loaded.PacExecutablePath);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void Load_returns_defaults_when_file_missing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pacdesk-missing-{Guid.NewGuid():N}.json");

        var loaded = new JsonSettingsStore(path).Load();

        Assert.Equal(1000, loaded.WindowWidth);
        Assert.Equal(720, loaded.WindowHeight);
        Assert.Null(loaded.PacExecutablePath);
    }

    [Fact]
    public void Load_returns_defaults_when_file_is_corrupt()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pacdesk-corrupt-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "{ this is not valid json");
        try
        {
            var loaded = new JsonSettingsStore(path).Load();
            Assert.Equal(1000, loaded.WindowWidth);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
