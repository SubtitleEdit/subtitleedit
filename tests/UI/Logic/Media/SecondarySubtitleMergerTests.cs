using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

public class SecondarySubtitleMergerTests
{
    private static SsaStyle MakeStyle(string name)
    {
        return new SsaStyle
        {
            Name = name,
            FontName = "Arial",
            FontSize = 20,
            Alignment = "8",
            OutlineWidth = 2,
            ShadowWidth = 1,
            MarginLeft = 10,
            MarginRight = 10,
            MarginVertical = 10,
        };
    }

    private static Subtitle MakeSubtitleWithPlayRes(int width, int height, string styleName, string? text = null)
    {
        var subtitle = new Subtitle();
        SecondarySubtitleStyler.SetHeader(subtitle, MakeStyle(styleName), width, height);
        if (text != null)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, 0, 1000) { Extra = styleName });
        }

        return subtitle;
    }

    [Fact]
    public void AddSecondarySubtitle_SamePlayRes_WithPos_AddsByReference()
    {
        var secondary = MakeSubtitleWithPlayRes(1920, 1080, "Secondary", "{\\an7\\pos(100,200)}line");
        var main = MakeSubtitleWithPlayRes(1920, 1080, "Default");

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: false);

        var added = Assert.Single(main.Paragraphs);
        Assert.Same(secondary.Paragraphs[0], added);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_WithoutPos_StillAddsByReference()
    {
        var secondary = MakeSubtitleWithPlayRes(1920, 1080, "Secondary", "plain line, no override tags");
        var main = MakeSubtitleWithPlayRes(640, 360, "Default");

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: false);

        var added = Assert.Single(main.Paragraphs);
        Assert.Same(secondary.Paragraphs[0], added);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_WithPos_RescalesOnACopy()
    {
        var secondary = MakeSubtitleWithPlayRes(1920, 1080, "Secondary", "{\\an7\\pos(960,540)}line");
        var main = MakeSubtitleWithPlayRes(640, 360, "Default");

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: false);

        var added = Assert.Single(main.Paragraphs);
        Assert.NotSame(secondary.Paragraphs[0], added);
        Assert.Contains("\\pos(320,180)", added.Text);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_WithPos_LeavesSourceParagraphUntouched()
    {
        var secondary = MakeSubtitleWithPlayRes(1920, 1080, "Secondary", "{\\an7\\pos(960,540)}line");
        var main = MakeSubtitleWithPlayRes(640, 360, "Default");

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: false);

        Assert.Contains("\\pos(960,540)", secondary.Paragraphs[0].Text);
    }

    [Fact]
    public void AddSecondarySubtitle_SmpteModeAndDifferentPlayRes_WithPos_AppliesBothStretchAndRescale()
    {
        var secondary = MakeSubtitleWithPlayRes(1920, 1080, "Secondary", "{\\an7\\pos(960,540)}line");
        secondary.Paragraphs[0].StartTime = new TimeCode(5000);
        secondary.Paragraphs[0].EndTime = new TimeCode(7000);
        var main = MakeSubtitleWithPlayRes(640, 360, "Default");

        SecondarySubtitleMerger.AddSecondarySubtitle(main, secondary, smpteMode: true);

        var added = Assert.Single(main.Paragraphs);
        Assert.NotSame(secondary.Paragraphs[0], added);
        Assert.Contains("\\pos(320,180)", added.Text);
        Assert.Equal(5005, added.StartTime.TotalMilliseconds, 3);
    }
}
