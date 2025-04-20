using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var text = @"
            1
            00:03:44,037-- > 00:03:45,997
            Text 1";

        var s = new Subtitle();
        new SubRip().LoadSubtitle(s, text.SplitToLines(), null);
        var res = new AdvancedSubStationAlpha().ToText(s, string.Empty);
        Assert.Contains("0,0:03:44.04,0:03:46.00", res);
    }


    [Fact]
    public void TestDialogThreeLines2ReBreak()
    {
        Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

        var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#00ffff\">Got a sinking feeling</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ffff\">about all of this.</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ff00\">Don't worry.</font> ", 0, 2000) });

        ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, true);
        var result = subtitle.Paragraphs.First().Text;

        Assert.Equal("- Got a sinking feeling about all of this." + Environment.NewLine + "- Don't worry.", result);
    }
}