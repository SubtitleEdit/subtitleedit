using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

public class CaptionWordEffectsTests
{
    private static List<SubtitleLineViewModel> Lines(string text = "Hi there everyone") => new()
    {
        new SubtitleLineViewModel { Text = text, StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(13) },
    };

    [Fact]
    public void HighlightBox_AddsOpaqueBoxStyle()
    {
        var effect = new AdvancedEffectHighlightBox();
        var header = effect.UpdateHeader(AdvancedSubStationAlpha.DefaultHeader, Lines());

        var style = AdvancedSubStationAlpha.GetSsaStyle(AdvancedEffectHighlightBox.GetBoxStyleName("Default"), header);
        Assert.True(style.LoadedFromHeader);
        Assert.Equal("3", style.BorderStyle);
    }

    [Fact]
    public void HighlightBox_MakesBoxAndTextEventPerWord()
    {
        var effect = new AdvancedEffectHighlightBox();
        var lines = Lines();
        var result = effect.ApplyEffect(effect.UpdateHeader(AdvancedSubStationAlpha.DefaultHeader, lines), lines, 1920, 1080, null);

        Assert.Equal(6, result.Count);
        var boxes = result.Where(l => l.Layer == 0).ToList();
        var texts = result.Where(l => l.Layer == 1).ToList();
        Assert.Equal(3, boxes.Count);
        Assert.Equal(3, texts.Count);
        Assert.All(boxes, b => Assert.Equal(AdvancedEffectHighlightBox.GetBoxStyleName("Default"), b.Style));
        Assert.All(boxes, b => Assert.StartsWith("{\\1a&HFF&", b.Text));

        // the slices cover the line exactly, back to back
        Assert.Equal(lines[0].StartTime, texts[0].StartTime);
        Assert.Equal(lines[0].EndTime, texts[^1].EndTime);
        Assert.Equal(texts[0].EndTime, texts[1].StartTime);
    }

    [Fact]
    public void WordTiming_LongerWordsGetMoreTime()
    {
        var result = new AdvancedEffectBoldCaptions().ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, Lines("a extraordinarily"), 1920, 1080, null);

        Assert.Equal(2, result.Count);
        Assert.True(result[1].Duration > result[0].Duration * 3);
    }

    [Fact]
    public void SingleWordPop_ShowsOneUpperCaseWordPerEvent()
    {
        var result = new AdvancedEffectSingleWordPop().ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, Lines("Hi {\\i1}there{\\i0}\\Nfriend"), 1920, 1080, null);

        Assert.Equal(3, result.Count);
        Assert.EndsWith("}HI", result[0].Text);
        Assert.EndsWith("}THERE", result[1].Text);
        Assert.EndsWith("}FRIEND", result[2].Text);
    }

    [Fact]
    public void BoldCaptions_KeepsPositionTags()
    {
        var result = new AdvancedEffectBoldCaptions().ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, Lines("{\\an8\\pos(100,200)}Hi there"), 1920, 1080, null);

        Assert.All(result, l => Assert.StartsWith("{\\an8\\pos(100,200)}", l.Text));
        Assert.Contains("THERE", result[1].Text);
    }

    [Fact]
    public void SmoothFill_KaraokeDurationsCoverTheLine()
    {
        var result = new AdvancedEffectSmoothFill().ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, Lines("Hi there everyone"), 1920, 1080, null);

        var line = Assert.Single(result);
        var durations = System.Text.RegularExpressions.Regex.Matches(line.Text, @"\\kf(\d+)").Select(m => int.Parse(m.Groups[1].Value)).ToList();
        Assert.Equal(3, durations.Count);
        Assert.Equal(300, durations.Sum()); // 3 s line = 300 cs
    }

    [Fact]
    public void BlurInWords_RevealsEachWordAtItsTime()
    {
        var result = new AdvancedEffectBlurInWords().ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, Lines("Hi there"), 1920, 1080, null);

        var line = Assert.Single(result);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(line.Text, @"\\alpha&HFF&").Count);
        Assert.Contains("\\t(0,", line.Text);
    }

    [Fact]
    public void RollingCaptions_PushesPreviousLineUp_WhenNextFollowsClosely()
    {
        var lines = new List<SubtitleLineViewModel>
        {
            new() { Text = "One", StartTime = TimeSpan.FromSeconds(0), EndTime = TimeSpan.FromSeconds(2) },
            new() { Text = "Two", StartTime = TimeSpan.FromSeconds(2.5), EndTime = TimeSpan.FromSeconds(4) },
            new() { Text = "Three", StartTime = TimeSpan.FromSeconds(10), EndTime = TimeSpan.FromSeconds(12) },
        };

        var result = new AdvancedEffectRollingCaptions().ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, lines, 1920, 1080, null);

        // "One" rolls into "Two" (0.5 s gap); "Three" comes after a pause and starts fresh
        Assert.Equal(4, result.Count);
        var one = result.Where(l => l.Text.EndsWith("One")).OrderBy(l => l.StartTime).ToList();
        Assert.Equal(2, one.Count);
        Assert.Equal(TimeSpan.FromSeconds(2.5), one[0].EndTime);
        Assert.Equal(TimeSpan.FromSeconds(2.5), one[1].StartTime);
        Assert.Equal(TimeSpan.FromSeconds(4), one[1].EndTime);
        Assert.Single(result, l => l.Text.EndsWith("Three"));
    }
}
