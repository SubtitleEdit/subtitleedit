using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class FrameRateSettingsTests
{
    [Fact]
    public void LoadSettings_SeedsCurrentFrameRateFromDefault()
    {
        var savedSettings = Se.Settings;
        var savedLibSeRate = Configuration.Settings.General.CurrentFrameRate;
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se_framerate_load_" + System.Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.DefaultFrameRate = 25.0;
            Se.Settings.General.CurrentFrameRate = 23.976; // stale persisted value from before #13113
            Se.SaveSettings(path);

            Se.Settings = new Se();
            Se.LoadSettings(path);

            // The explicit default wins at startup in both the UI settings store and libse.
            Assert.Equal(25.0, Se.Settings.General.CurrentFrameRate);
            Assert.Equal(25.0, Configuration.Settings.General.CurrentFrameRate);
        }
        finally
        {
            Se.Settings = savedSettings;
            Configuration.Settings.General.CurrentFrameRate = savedLibSeRate;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }

    [Fact]
    public void SaveSettings_DoesNotOverwriteVideoDerivedFrameRate()
    {
        var savedSettings = Se.Settings;
        var savedLibSeRate = Configuration.Settings.General.CurrentFrameRate;
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se_framerate_save_" + System.Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.DefaultFrameRate = 25.0;
            Se.Settings.General.CurrentFrameRate = 25.0;
            Configuration.Settings.General.CurrentFrameRate = 29.97; // video-derived rate, e.g. from the parser

            Se.SaveSettings(path);

            // SaveSettings must not push a stale UI value over the parser's rate (the
            // CurrentFrameRate bridge was removed from UpdateLibSeSettings for this reason).
            Assert.Equal(29.97, Configuration.Settings.General.CurrentFrameRate);
        }
        finally
        {
            Se.Settings = savedSettings;
            Configuration.Settings.General.CurrentFrameRate = savedLibSeRate;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
