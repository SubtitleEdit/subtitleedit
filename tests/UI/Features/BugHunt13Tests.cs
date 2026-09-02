using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.WebVtt;
using System.Linq;

namespace UITests.Features;

/// <summary>
/// Guard tests for the 2026-08-27 bug hunt: defects found by auditing settings that are saved
/// but never loaded back, dialogs that mutate the caller's object before Cancel, and emitted
/// ASSA/WebVTT markup that does not match what the reader accepts.
/// </summary>
public class BugHunt13Tests
{
    [Fact]
    public void FadeOverrideTag_IsWellFormed()
    {
        // "{\fad(300,300}" - libass tolerates the missing parenthesis, other renderers need not,
        // and this tag is written into the subtitle the user saves and shares.
        var fade = OverrideTagDisplay.List().FirstOrDefault(p => p.Tag.Contains("\\fad"));

        Assert.NotNull(fade);
        Assert.Contains("\\fad(300,300)", fade!.Tag, System.StringComparison.Ordinal);
    }

    [Fact]
    public void BurnInFadeEffect_IsWellFormed()
    {
        var effect = BurnInEffectItem.List().First(p => p.Type == BurnInEffectType.FadeInOut);

        var result = effect.ApplyEffect("Hello", 1920, 1080, 40, 2000);

        Assert.Contains("\\fad(250,250)", result, System.StringComparison.Ordinal);
    }

    [Fact]
    public void BurnInLogo_Clone_IsIndependentOfTheOriginal()
    {
        // The logo dialog edits its instance live, so it must get a copy - otherwise Cancel keeps
        // every drag and slider change.
        var original = new BurnInLogo { LogoFileName = "logo.png", X = 10, Y = 20, Alpha = 50, Size = 75 };

        var clone = original.Clone();
        clone.X = 999;
        clone.Alpha = 1;

        Assert.Equal(10, original.X);
        Assert.Equal(50, original.Alpha);
        Assert.Equal("logo.png", clone.LogoFileName);
        Assert.Equal(20, clone.Y);
        Assert.Equal(75, clone.Size);
    }

    [Fact]
    public void WebVttStyleDisplay_ClassSelector_KeepsItsDot()
    {
        var display = new WebVttStyleDisplay(new WebVttStyle { Name = ".red" });

        Assert.True(display.IsClassSelector);
        Assert.Equal("red", display.Name);
        Assert.Equal(".red", display.ToWebVttStyle().Name);
    }

    [Fact]
    public void WebVttStyleDisplay_ElementSelector_DoesNotBecomeAClass()
    {
        // "::cue(b)" styles bold text; rewriting it as "::cue(.b)" moves the styling to cues
        // carrying class "b" instead, so bold text silently loses it.
        var display = new WebVttStyleDisplay(new WebVttStyle { Name = "b" });

        Assert.False(display.IsClassSelector);
        Assert.Equal("b", display.ToWebVttStyle().Name);
    }

    [Fact]
    public void WebVttStyleDisplay_NewStyle_DefaultsToAClassSelector()
    {
        Assert.True(new WebVttStyleDisplay().IsClassSelector);
    }
}
