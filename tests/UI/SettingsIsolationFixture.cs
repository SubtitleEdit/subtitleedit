using Nikse.SubtitleEdit.Logic.Config;

[assembly: AssemblyFixture(typeof(UITests.SettingsIsolationFixture))]

namespace UITests;

public sealed class SettingsIsolationFixture : IDisposable
{
    public static string SettingsDirectory { get; private set; } = string.Empty;

    public SettingsIsolationFixture()
    {
        SettingsDirectory = Path.Combine(
            Path.GetTempPath(),
            "SubtitleEdit.UITests",
            Guid.NewGuid().ToString("N"));
        Se.SettingsFilePathOverride = Path.Combine(SettingsDirectory, "Settings.json");
    }

    public void Dispose()
    {
        Se.SettingsFilePathOverride = null;

        if (Directory.Exists(SettingsDirectory))
        {
            Directory.Delete(SettingsDirectory, recursive: true);
        }
    }
}

public class SettingsIsolationTests
{
    [Fact]
    public void SettingsFilePath_IsRedirectedToTestDirectory()
    {
        Assert.Equal(
            Path.Combine(SettingsIsolationFixture.SettingsDirectory, "Settings.json"),
            Se.GetSettingsFilePath());
    }
}
