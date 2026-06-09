using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class ContinuationStyleSettingsTests
{
    [Fact]
    public void ApplyContinuationStyleToLibSe_CopiesActiveCustomStyleToLibSeConfiguration()
    {
        var savedSettings = Se.Settings;
        var savedStyle = Configuration.Settings.General.ContinuationStyle;
        var savedCustom = CustomContinuationStyle.FromGeneralSettings(Configuration.Settings.General);
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ContinuationStyle = ContinuationStyle.Custom.ToString();
            Se.Settings.General.CustomContinuationStyle = new CustomContinuationStyle
            {
                Pause = 777,
                Suffix = "..",
                Prefix = "-",
                GapSuffix = "…",
                UseDifferentStyleGap = false,
            };

            Se.ApplyContinuationStyleToLibSe();

            Assert.Equal(ContinuationStyle.Custom, Configuration.Settings.General.ContinuationStyle);
            Assert.Equal(777, Configuration.Settings.General.ContinuationPause);
            Assert.Equal("..", Configuration.Settings.General.CustomContinuationStyleSuffix);
            Assert.Equal("-", Configuration.Settings.General.CustomContinuationStylePrefix);
            Assert.Equal("…", Configuration.Settings.General.CustomContinuationStyleGapSuffix);
            Assert.False(Configuration.Settings.General.CustomContinuationStyleUseDifferentStyleGap);
        }
        finally
        {
            Se.Settings = savedSettings;
            Configuration.Settings.General.ContinuationStyle = savedStyle;
            savedCustom.ApplyToGeneralSettings(Configuration.Settings.General);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesPerProfileCustomContinuationStyle()
    {
        var savedSettings = Se.Settings;
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se_continuation_roundtrip_" + System.Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.Profiles = new List<RulesProfile>
            {
                new()
                {
                    Name = "A",
                    CustomContinuationStyle = new CustomContinuationStyle { Suffix = "AA", Pause = 111 },
                },
                new()
                {
                    Name = "B",
                    CustomContinuationStyle = new CustomContinuationStyle { Suffix = "BB", Pause = 222, GapPrefix = "—" },
                },
            };

            Se.SaveSettings(path);
            Se.Settings = new Se(); // ensure we are really reading from disk
            Se.LoadSettings(path);

            var a = Se.Settings.General.Profiles.First(p => p.Name == "A");
            var b = Se.Settings.General.Profiles.First(p => p.Name == "B");

            Assert.Equal("AA", a.CustomContinuationStyle.Suffix);
            Assert.Equal(111, a.CustomContinuationStyle.Pause);
            Assert.Equal("BB", b.CustomContinuationStyle.Suffix);
            Assert.Equal(222, b.CustomContinuationStyle.Pause);
            Assert.Equal("—", b.CustomContinuationStyle.GapPrefix);
        }
        finally
        {
            Se.Settings = savedSettings;
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }
}
