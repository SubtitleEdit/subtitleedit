using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

/// <summary>
/// Tests for the subtitle that "Apply custom override tags" builds for both the mpv preview and
/// the OK result. Regressions covered: the preview dropped the file's header (all lines rendered
/// in the app default style), the Dialogue style column fell back to the first header style, and
/// OK without a video silently discarded the tag.
/// </summary>
public class AssaApplyCustomOverrideTagsBuildTests
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

    private static readonly AdvancedSubStationAlpha Format = new();

    private static List<SubtitleLineViewModel> MakeLines(params string[] styles)
    {
        var lines = new List<SubtitleLineViewModel>();
        for (var i = 0; i < styles.Length; i++)
        {
            var p = new Paragraph($"Line {i + 1}", i * 2000, i * 2000 + 1000) { Extra = styles[i] };
            lines.Add(new SubtitleLineViewModel(p, Format));
        }

        return lines;
    }

    private static Subtitle Build(
        List<SubtitleLineViewModel> lines,
        List<SubtitleLineViewModel> selected,
        string tag,
        bool all = false,
        bool selectedOnly = false,
        bool selectedAndForward = false)
    {
        return AssaApplyCustomOverrideTagsViewModel.BuildTaggedSubtitle(
            HeaderWithTwoStyles, null, lines, selected, tag, all, selectedOnly, selectedAndForward, Format);
    }

    private static string[] GetDialogueStyles(Subtitle subtitle)
    {
        var text = Format.ToText(subtitle, string.Empty);
        return text.SplitToLines()
            .Where(l => l.StartsWith("Dialogue:", StringComparison.Ordinal))
            .Select(l => l.Substring("Dialogue:".Length).Split(',')[3].Trim())
            .ToArray();
    }

    [Fact]
    public void KeepsHeaderAndPerLineStyles()
    {
        var lines = MakeLines("Default", "Big", "Default");

        var result = Build(lines, new List<SubtitleLineViewModel>(), "{\\blur2}", all: true);

        Assert.Equal(HeaderWithTwoStyles, result.Header);
        Assert.Equal(new[] { "Default", "Big", "Default" }, GetDialogueStyles(result));
    }

    [Fact]
    public void AdjustAll_TagsEveryLine()
    {
        var lines = MakeLines("Default", "Big");

        var result = Build(lines, new List<SubtitleLineViewModel>(), "{\\blur2}", all: true);

        Assert.All(result.Paragraphs, p => Assert.StartsWith("{\\blur2}", p.Text));
    }

    [Fact]
    public void AdjustSelectedLines_TagsOnlySelected()
    {
        var lines = MakeLines("Default", "Big", "Default");
        var selected = new List<SubtitleLineViewModel> { lines[1] };

        var result = Build(lines, selected, "{\\blur2}", selectedOnly: true);

        Assert.Equal("Line 1", result.Paragraphs[0].Text);
        Assert.Equal("{\\blur2}Line 2", result.Paragraphs[1].Text);
        Assert.Equal("Line 3", result.Paragraphs[2].Text);
    }

    [Fact]
    public void AdjustSelectedLinesAndForward_TagsFromFirstSelected()
    {
        var lines = MakeLines("Default", "Big", "Default");
        var selected = new List<SubtitleLineViewModel> { lines[1] };

        var result = Build(lines, selected, "{\\blur2}", selectedAndForward: true);

        Assert.Equal("Line 1", result.Paragraphs[0].Text);
        Assert.Equal("{\\blur2}Line 2", result.Paragraphs[1].Text);
        Assert.Equal("{\\blur2}Line 3", result.Paragraphs[2].Text);
    }

    [Fact]
    public void EmptySelection_DoesNotThrowAndTagsNothing()
    {
        var lines = MakeLines("Default", "Big");

        var selectedOnly = Build(lines, new List<SubtitleLineViewModel>(), "{\\blur2}", selectedOnly: true);
        var forward = Build(lines, new List<SubtitleLineViewModel>(), "{\\blur2}", selectedAndForward: true);

        Assert.All(selectedOnly.Paragraphs, p => Assert.DoesNotContain("\\blur2", p.Text));
        Assert.All(forward.Paragraphs, p => Assert.DoesNotContain("\\blur2", p.Text));
    }

    [Fact]
    public void SourceLinesAreNotMutated()
    {
        var lines = MakeLines("Default", "Big");

        _ = Build(lines, new List<SubtitleLineViewModel>(), "{\\blur2}", all: true);

        Assert.All(lines, l => Assert.DoesNotContain("\\blur2", l.Text));
    }
}
