using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic.Config;

public class AutoBreakSettingsTests
{
    [Fact]
    public void SeToolsAutoBreakDefaults_MatchLibSeToolsSettings()
    {
        var seTools = new SeTools();
        var libSeTools = new ToolsSettings();

        Assert.Equal(libSeTools.AutoBreakLineEndingEarly, seTools.AutoBreakLineEndingEarly);
        Assert.Equal(libSeTools.AutoBreakCommaBreakEarly, seTools.AutoBreakCommaBreakEarly);
        Assert.Equal(libSeTools.AutoBreakDashEarly, seTools.AutoBreakDashEarly);
        Assert.Equal(libSeTools.AutoBreakUsePixelWidth, seTools.AutoBreakUsePixelWidth);
        Assert.Equal(libSeTools.AutoBreakPreferBottomHeavy, seTools.AutoBreakPreferBottomHeavy);
        Assert.Equal(libSeTools.AutoBreakPreferBottomPercent, seTools.AutoBreakPreferBottomPercent);
        Assert.Equal(libSeTools.UseNoLineBreakAfter, seTools.UseNoLineBreakAfter);
    }

    [Fact]
    public void UpdateLibSeSettings_CopiesAutoBreakSettings()
    {
        var savedSettings = Se.Settings;
        var savedTools = Configuration.Settings.Tools;
        try
        {
            Configuration.Settings.Tools = new ToolsSettings();
            Se.Settings = new Se();
            Se.Settings.Tools.AutoBreakLineEndingEarly = true;
            Se.Settings.Tools.AutoBreakCommaBreakEarly = true;
            Se.Settings.Tools.AutoBreakDashEarly = false;
            Se.Settings.Tools.AutoBreakUsePixelWidth = false;
            Se.Settings.Tools.AutoBreakPreferBottomHeavy = false;
            Se.Settings.Tools.AutoBreakPreferBottomPercent = 12;
            Se.Settings.Tools.UseNoLineBreakAfter = true;

            Se.UpdateLibSeSettings();

            Assert.True(Configuration.Settings.Tools.AutoBreakLineEndingEarly);
            Assert.True(Configuration.Settings.Tools.AutoBreakCommaBreakEarly);
            Assert.False(Configuration.Settings.Tools.AutoBreakDashEarly);
            Assert.False(Configuration.Settings.Tools.AutoBreakUsePixelWidth);
            Assert.False(Configuration.Settings.Tools.AutoBreakPreferBottomHeavy);
            Assert.Equal(12, Configuration.Settings.Tools.AutoBreakPreferBottomPercent);
            Assert.True(Configuration.Settings.Tools.UseNoLineBreakAfter);
        }
        finally
        {
            Se.Settings = savedSettings;
            Configuration.Settings.Tools = savedTools;
        }
    }
}
