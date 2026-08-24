using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Features.Main;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for the advanced ASSA effect generators.
/// </summary>
public class AdvancedEffectTests
{
    private static SubtitleLineViewModel MakeLine(string text, double startMs = 0, double durationMs = 2000)
    {
        return new SubtitleLineViewModel
        {
            Text = text,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(startMs + durationMs),
        };
    }

    /// <summary>
    /// An unmatched '{' used to stall BuildSegments forever (neither the tag loop nor the
    /// plain-text scan advanced), hanging the preview's background rebuild and OK.
    /// </summary>
    [Fact]
    public void FancyKaraoke_UnmatchedBrace_Terminates()
    {
        var effect = new AdvancedEffectFancyKaraoke { AutoDetectActiveWord = false };
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Hello {world")], 1280, 720, null);

        Assert.Single(result);
        Assert.Contains("Hello", result[0].Text);
    }

    /// <summary>
    /// A \move copied verbatim into each auto-sequenced word-line restarts the motion at
    /// every word boundary; it must be rewritten to one continuous motion instead.
    /// </summary>
    [Fact]
    public void FancyKaraoke_AutoSequence_RewritesMovePerWordLine()
    {
        var effect = new AdvancedEffectFancyKaraoke { AutoDetectActiveWord = true };
        var result = effect.ApplyEffect(string.Empty, [MakeLine(@"{\move(0,0,100,100)}a b")], 1280, 720, null);

        Assert.Equal(2, result.Count);
        Assert.Contains(@"\move(0,0,50,50,0,1000)", result[0].Text);
        Assert.Contains(@"\move(50,50,100,100,0,1000)", result[1].Text);
    }

    /// <summary>
    /// Override tag arguments must be culture-invariant: under a comma-decimal locale a
    /// fractional value interpolated with the current culture corrupts the tag.
    /// </summary>
    [Fact]
    public void WordSpacing_FractionalSpacing_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("da-DK");
            var effect = new AdvancedEffectWordSpacing { SpacingPixels = 12.5m };
            var result = effect.ApplyEffect(string.Empty, [MakeLine("a b")], 1280, 720, null);

            Assert.Contains(@"{\fsp12.5}", result[0].Text);
            Assert.DoesNotContain("12,5", result[0].Text);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <inheritdoc cref="WordSpacing_FractionalSpacing_IsCultureInvariant"/>
    [Fact]
    public void AudioTextPulse_FractionalBorder_IsCultureInvariant()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("da-DK");
            var effect = new AdvancedEffectAudioTextPulse();
            var result = effect.ApplyEffect(string.Empty, [MakeLine("Hello", durationMs: 100)], 1280, 720, null);

            // With no wave data the amplitude is 0, so every frame gets the resting border 1.5
            Assert.NotEmpty(result);
            Assert.All(result, line => Assert.Contains(@"\bord1.5", line.Text));
            Assert.All(result, line => Assert.DoesNotContain("1,5", line.Text));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The fade duration is emitted as a rounded integer - a fractional line duration must
    /// not leak a decimal point (or comma) into \fad.
    /// </summary>
    [Fact]
    public void FadeIn_FractionalDuration_EmitsIntegerFad()
    {
        var effect = new AdvancedEffectFadeIn();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Hello", durationMs: 1500.5)], 1280, 720, null);

        var overlay = result[0];
        Assert.Contains(@"\fad(0,1500)", overlay.Text);
    }

    /// <summary>
    /// Character stagger used to be computed from the raw char index (including spaces), so
    /// long lines produced events starting after the subtitle already ended.
    /// </summary>
    [Fact]
    public void BounceIn_LongLine_NeverStartsAfterLineEnd()
    {
        var text = string.Join(" ", Enumerable.Repeat("word", 25)); // 124 chars, 100 visible
        var effect = new AdvancedEffectBounceIn();
        var result = effect.ApplyEffect(string.Empty, [MakeLine(text, durationMs: 1000)], 1280, 720, null);

        Assert.All(result, line => Assert.True(line.StartTime < line.EndTime));
    }

    /// <summary>
    /// Effects must never put the caller's live view-model instances into the returned
    /// list - pass-through lines have to be clones.
    /// </summary>
    [Fact]
    public void Karaoke_EmptyLine_PassesThroughAsClone()
    {
        var source = MakeLine(string.Empty);
        var effect = new AdvancedEffectKaraoke();
        var result = effect.ApplyEffect(string.Empty, [source], 1280, 720, null);

        Assert.Single(result);
        Assert.NotSame(source, result[0]);
        Assert.NotEqual(source.Id, result[0].Id);
    }

    /// <summary>
    /// The per-drop advance has a floor now; a long span must terminate and respect the
    /// generator's global event cap.
    /// </summary>
    [Fact]
    public void Rain_LongSpan_IsCappedAndTerminates()
    {
        var lines = new List<SubtitleLineViewModel>
        {
            MakeLine("a", startMs: 0),
            MakeLine("b", startMs: 3_600_000 - 2000),
        };
        var effect = new AdvancedEffectRain();
        var result = effect.ApplyEffect(string.Empty, lines, 1280, 720, null);

        Assert.True(result.Count <= AdvancedEffectUtil.MaxGeneratedEvents + lines.Count);
    }

    /// <summary>
    /// The vignette's inner rectangle must be wound opposite to the outer one so the fill
    /// rule cuts a hole; same-direction winding filled (and dimmed) the entire screen.
    /// </summary>
    [Fact]
    public void OldMovie_VignetteInnerContour_IsReverseWound()
    {
        var effect = new AdvancedEffectOldMovie();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Hello")], 1280, 720, null);

        var vignette = result.First(l => l.Text.Contains(@"\be90"));
        // Inner contour runs top-left -> bottom-left (counter-clockwise), unlike the outer
        Assert.Contains("m 180 180 l 180 540", vignette.Text);
    }

    /// <summary>
    /// The two neon layers overlap in time at the same position; they must sit on distinct
    /// ASSA layers or libass collision avoidance stacks them vertically.
    /// </summary>
    [Fact]
    public void NeonBurst_GlowAndCore_GetDistinctLayers()
    {
        var effect = new AdvancedEffectNeonBurst();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Hello")], 1280, 720, null);

        Assert.Equal(2, result.Count);
        Assert.NotEqual(result[0].Layer, result[1].Layer);
    }

    /// <summary>
    /// A literal \N must survive the wave effect intact - it used to be split into a stray
    /// animated backslash glyph plus a plain 'N', destroying the line break.
    /// </summary>
    [Fact]
    public void Wave_InlineLineBreak_IsPreserved()
    {
        var effect = new AdvancedEffectWave();
        var result = effect.ApplyEffect(string.Empty, [MakeLine(@"ab\Ncd")], 1280, 720, null);

        Assert.Equal(4, result.Count); // one line per visible char; \N is not a char
        Assert.All(result, line => Assert.Contains(@"\N", line.Text));
    }

    /// <summary>
    /// Hearts pre-roll starts before the subtitle; near t=0 that must clamp to zero instead
    /// of emitting negative timestamps.
    /// </summary>
    [Fact]
    public void Hearts_NearZeroStart_HasNoNegativeTimestamps()
    {
        var effect = new AdvancedEffectHearts();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Hello", startMs: 300, durationMs: 4000)], 1280, 720, null);

        Assert.All(result, line => Assert.True(line.StartTime >= TimeSpan.Zero));
    }

    [Fact]
    public void WordFlip3D_TwoWords_ProducesSequentialFlipEvents()
    {
        var effect = new AdvancedEffectWordFlip3D();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("aa bb")], 1280, 720, null);

        Assert.Equal(2, result.Count);
        // Step 0: first word flips in, second word is a hidden placeholder
        Assert.Contains(@"\frx90", result[0].Text);
        Assert.Contains(@"\alpha&HFF&", result[0].Text);
        // Step 1: first word is shown solid, second word flips in
        Assert.Contains(@"\alpha&H00&\frx0", result[1].Text);
        Assert.Contains(@"\frx90", result[1].Text);
        // Sequential, non-overlapping timing
        Assert.Equal(TimeSpan.Zero, result[0].StartTime);
        Assert.Equal(result[0].EndTime, result[1].StartTime);
        Assert.Equal(TimeSpan.FromMilliseconds(2000), result[1].EndTime);
    }

    /// <summary>
    /// A source \move must continue seamlessly across the word-events instead of
    /// restarting at every word boundary (shared AdjustMoveForSegment rewrite).
    /// </summary>
    [Fact]
    public void WordFlip3D_SourceMove_ContinuesAcrossWords()
    {
        var effect = new AdvancedEffectWordFlip3D();
        var result = effect.ApplyEffect(string.Empty, [MakeLine(@"{\move(0,0,100,100)}a b")], 1280, 720, null);

        Assert.Equal(2, result.Count);
        Assert.Contains(@"\move(0,0,50,50,0,1000)", result[0].Text);
        Assert.Contains(@"\move(50,50,100,100,0,1000)", result[1].Text);
    }

    [Fact]
    public void CinematicTitle_AppliesTrackingAndFocusPull()
    {
        var effect = new AdvancedEffectCinematicTitle();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("THE END", durationMs: 5000)], 1280, 720, null);

        Assert.Single(result);
        var text = result[0].Text;
        Assert.Contains(@"\fsp20\blur16", text);            // wide tracking, out of focus
        Assert.Contains(@"\t(0,1200,\fsp0\blur0)", text);   // resolve to normal
        Assert.Contains(@"\t(4200,5000,\fsp12\blur10)", text); // mirrored exit on long lines
        Assert.Contains(@"\fad(", text);
        Assert.EndsWith("THE END", text);
    }

    [Fact]
    public void CinematicTitle_ShortLine_SkipsTheExit()
    {
        var effect = new AdvancedEffectCinematicTitle();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Hi", durationMs: 1500)], 1280, 720, null);

        Assert.DoesNotContain(@"\fsp12\blur10", result[0].Text);
    }

    /// <summary>
    /// First line is the name, following lines the role; an accent bar slides in between.
    /// Three events on distinct ascending layers.
    /// </summary>
    [Fact]
    public void LowerThird_NameAndRole_ProducesBarNameAndRole()
    {
        var effect = new AdvancedEffectLowerThird();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Jane Doe\\NDirector of Photography".Replace("\\N", "\n"))], 1280, 720, null);

        Assert.Equal(3, result.Count);
        Assert.True(result[0].Layer < result[1].Layer && result[1].Layer < result[2].Layer);
        Assert.Contains(@"\p1", result[0].Text);              // accent bar drawing
        Assert.Contains(@"\move(", result[0].Text);
        Assert.Contains(@"\b1", result[1].Text);              // bold name
        Assert.EndsWith("Jane Doe", result[1].Text);
        Assert.Contains(@"\fscx80\fscy80", result[2].Text);   // smaller role
        Assert.EndsWith("Director of Photography", result[2].Text);
    }

    [Fact]
    public void LowerThird_SingleLine_OmitsTheRoleEvent()
    {
        var effect = new AdvancedEffectLowerThird();
        var result = effect.ApplyEffect(string.Empty, [MakeLine("Jane Doe")], 1280, 720, null);

        Assert.Equal(2, result.Count); // bar + name only
    }

    /// <summary>
    /// \pos/\move geometry is in SCRIPT space: with a header whose PlayRes differs from
    /// the video size, the banner must use the header's resolution or it renders
    /// off-screen (the Spotlight reveal lesson).
    /// </summary>
    [Fact]
    public void LowerThird_UsesHeaderPlayResForGeometry()
    {
        const string header = "[Script Info]\nPlayResX: 384\nPlayResY: 288\n\n[V4+ Styles]\n";
        var effect = new AdvancedEffectLowerThird();
        var result = effect.ApplyEffect(header, [MakeLine("Jane Doe")], 1920, 1080, null);

        // bar: width 384*0.30=115, y 288*0.845=243, target x 384*0.06=23
        Assert.Contains(@"\move(-115,243,23,243", result[0].Text);
    }

    [Fact]
    public void Factory_ContainsTheNewEffects()
    {
        var list = Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.AdvancedEffectDisplayFactory.List();

        Assert.Contains(list, e => e is AdvancedEffectWordFlip3D);
        Assert.Contains(list, e => e is AdvancedEffectCinematicTitle);
        Assert.Contains(list, e => e is AdvancedEffectLowerThird);
    }
}
