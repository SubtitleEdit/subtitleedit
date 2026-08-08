using System;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for discussion #13350: the "Set position" preview rendered the line with the
/// first style in the header instead of the style the line actually uses.
/// </summary>
public class AssaSetPositionPreviewStyleTests
{
    private const string HeaderWithTwoStyles = @"[Script Info]
; This is an Advanced Sub Station Alpha v4+ script.
Title:
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1
Style: Big,Verdana,72,&H0000FFFF,&H0300FFFF,&H00000000,&H02000000,-1,0,0,0,100,100,0,0,1,3,2,8,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";

    private static Subtitle MakeAssaSubtitle(string style)
    {
        var subtitle = new Subtitle { Header = HeaderWithTwoStyles };
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 3000) { Extra = style });
        return subtitle;
    }

    private static SubtitleLineViewModel MakeLine(Subtitle subtitle)
    {
        return new SubtitleLineViewModel(subtitle.Paragraphs[0], new AdvancedSubStationAlpha());
    }

    private static string GetDialogueStyle(Subtitle subtitle)
    {
        var text = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);
        var dialogue = text.SplitToLines().First(l => l.StartsWith("Dialogue:", StringComparison.Ordinal));

        // Layer, Start, End, Style, ...
        return dialogue.Substring("Dialogue:".Length).Split(',')[3].Trim();
    }

    [Fact]
    public void PreviewSubtitle_KeepsTheStyleOfTheLine()
    {
        var subtitle = MakeAssaSubtitle("Big");

        var preview = AssaSetPositionViewModel.MakePreviewSubtitle(subtitle, MakeLine(subtitle));

        Assert.Equal("Big", GetDialogueStyle(preview));
    }

    [Fact]
    public void PreviewSubtitle_KeepsTheHeaderStyles()
    {
        var subtitle = MakeAssaSubtitle("Big");

        var preview = AssaSetPositionViewModel.MakePreviewSubtitle(subtitle, MakeLine(subtitle));

        var styles = AdvancedSubStationAlpha.GetStylesFromHeader(preview.Header);
        Assert.Equal(new[] { "Default", "Big" }, styles);
    }

    [Fact]
    public void PreviewSubtitle_HasASingleParagraphStartingAtZero()
    {
        var subtitle = MakeAssaSubtitle("Default");
        subtitle.Paragraphs.Add(new Paragraph("Second line", 4000, 6000) { Extra = "Big" });

        var preview = AssaSetPositionViewModel.MakePreviewSubtitle(subtitle, MakeLine(subtitle));

        var p = Assert.Single(preview.Paragraphs);
        Assert.Equal("Hello world", p.Text);
        Assert.Equal(0, p.StartTime.TotalSeconds);
        Assert.Equal(10, p.EndTime.TotalSeconds);
    }

    [Fact]
    public void PreviewSubtitle_DoesNotTouchTheSourceSubtitle()
    {
        var subtitle = MakeAssaSubtitle("Big");

        _ = AssaSetPositionViewModel.MakePreviewSubtitle(subtitle, MakeLine(subtitle));

        var p = Assert.Single(subtitle.Paragraphs);
        Assert.Equal(1000, p.StartTime.TotalMilliseconds);
        Assert.Equal(3000, p.EndTime.TotalMilliseconds);
    }
}
