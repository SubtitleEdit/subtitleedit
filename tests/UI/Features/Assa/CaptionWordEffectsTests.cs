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
    public void SmoothFill_RightToLeft_UsesWholeLineBidiAndFillsFromTheRight()
    {
        var effect = new AdvancedEffectSmoothFill { RightToLeft = true };
        var lines = Lines("صد سال تنهایی");
        lines.Add(new SubtitleLineViewModel { Text = "Hi there", StartTime = TimeSpan.FromSeconds(14), EndTime = TimeSpan.FromSeconds(16) });

        var result = effect.ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, lines, 1920, 1080, null);

        Assert.Equal(2, result.Count);
        Assert.Contains("\\fe-1", result[0].Text);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(result[0].Text, @"\\frx180\\fry180\\frz180[\\}]"));
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(result[0].Text, @"\\kf\d+").Count);
        Assert.True(result[0].Text.IndexOf("صد", StringComparison.Ordinal) < result[0].Text.IndexOf("تنهایی", StringComparison.Ordinal));
        Assert.DoesNotContain("\\fr", result[1].Text); // left-to-right lines are untouched
        Assert.DoesNotContain("\\fe", result[1].Text);
    }

    [Fact]
    public void SmoothFill_RightToLeft_LatinWordsInAMixedLineStillFillLeftToRight()
    {
        var effect = new AdvancedEffectSmoothFill { RightToLeft = true };

        var result = effect.ApplyEffect(AdvancedSubStationAlpha.DefaultHeader, Lines("سلام John 2024 دنیا"), 1920, 1080, null);

        var text = Assert.Single(result).Text;
        var blocks = System.Text.RegularExpressions.Regex.Matches(text, @"\{\\kf\d+([^}]*)\}").Select(m => m.Groups[1].Value).ToList();
        Assert.Equal(new[] { "\\frx180\\fry180\\frz180", "\\frx0\\fry0\\frz0", "", "\\frx180\\fry180\\frz180" }, blocks);
    }

    [Fact]
    public void SmoothFill_RightToLeft_KeepsTheStyleAngle()
    {
        var header = AdvancedSubStationAlpha.DefaultHeader;
        var defaultStyle = AdvancedSubStationAlpha.GetSsaStyle("Default", header);
        defaultStyle.Angle = 25;
        header = AdvancedSubStationAlpha.UpdateOrAddStyle(header, defaultStyle);
        var effect = new AdvancedEffectSmoothFill { RightToLeft = true };

        var result = effect.ApplyEffect(header, Lines("سلام John"), 1920, 1080, null);

        var text = Assert.Single(result).Text;
        Assert.Contains("\\frx180\\fry180\\frz205", text); // a half turn on top of frx180\fry180 nets out at 25 degrees
        Assert.Contains("\\frx0\\fry0\\frz25", text);
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
