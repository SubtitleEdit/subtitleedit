using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Assa;

/// <summary>
/// Reveal effects on speech-to-text style input: one line per word with the active word
/// underlined, and lines with no marked word for the pauses.
/// </summary>
public class MarkedRevealEffectsTests
{
    private static readonly string Header = AdvancedSubStationAlpha.DefaultHeader;

    private static SubtitleLineViewModel Line(string text, double start, double end) =>
        new() { Text = text, StartTime = TimeSpan.FromSeconds(start), EndTime = TimeSpan.FromSeconds(end) };

    // pause, "Hi", pause, "there"
    private static List<SubtitleLineViewModel> Sequence() => new()
    {
        Line("Hi there", 9, 10),
        Line("<u>Hi</u> there", 10, 11),
        Line("Hi there", 11, 12),
        Line("Hi <u>there</u>", 12, 13),
    };

    [Fact]
    public void Plan_IsNull_WithoutMarks()
    {
        Assert.Null(MarkedReveal.Plan(new List<SubtitleLineViewModel> { Line("Hi there", 1, 2) }));
    }

    [Fact]
    public void Plan_TracksShownWordsAndCharacters()
    {
        var plan = MarkedReveal.Plan(Sequence())!;

        Assert.Equal((0, -1, 0, 0), (plan[0]!.ShownWords, plan[0]!.ActiveWord, plan[0]!.CharFrom, plan[0]!.CharTo));
        Assert.Equal((0, 0, 0, 2), (plan[1]!.ShownWords, plan[1]!.ActiveWord, plan[1]!.CharFrom, plan[1]!.CharTo));
        Assert.Equal((1, -1, 2, 2), (plan[2]!.ShownWords, plan[2]!.ActiveWord, plan[2]!.CharFrom, plan[2]!.CharTo));
        Assert.Equal((1, 1, 3, 8), (plan[3]!.ShownWords, plan[3]!.ActiveWord, plan[3]!.CharFrom, plan[3]!.CharTo));
        Assert.All(plan, p => Assert.Equal("Hi there", p!.Text));
    }

    [Fact]
    public void Typewriter_TypesOnlyTheMarkedWordWithinItsLine()
    {
        var result = new AdvancedEffectTypewriter().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        // "Hi" = 2 steps, pause = 1, "there" = 5; the first pause shows nothing
        Assert.Equal(8, result.Count);
        Assert.Equal(new[] { "H", "Hi", "Hi", "Hi t", "Hi th", "Hi the", "Hi ther", "Hi there" }, result.Select(r => r.Text));
        Assert.Equal(TimeSpan.FromSeconds(10), result[0].StartTime);
        Assert.Equal(TimeSpan.FromSeconds(13), result[^1].EndTime);
        Assert.All(result, r => Assert.DoesNotContain("<u>", r.Text));
    }

    [Fact]
    public void TypewriterWithHighlight_PauseHasNoGlowingCharacter()
    {
        var result = new AdvancedEffectTypewriterWithHighlight().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        var pause = result.Single(r => r.StartTime == TimeSpan.FromSeconds(11));
        Assert.Equal("{\\r}Hi", pause.Text);
        Assert.EndsWith("{\\bord3\\blur8\\3c&HFFFFFF&\\1c&HFFFFFF&}e", result[^1].Text);
    }

    [Fact]
    public void ScrambleReveal_KeepsPlaceholdersAcrossLines()
    {
        var result = new AdvancedEffectScrambleReveal().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        Assert.Equal(9, result.Count); // pause 1 + "Hi" 2 + pause 1 + "there" 5
        Assert.Equal(result[0].Text[3..], result[1].Text[3..]); // "there" stays scrambled the same way
        Assert.StartsWith("Hi ", result[3].Text);
        Assert.Equal("Hi there", result[^1].Text);
    }

    [Fact]
    public void Karaoke_ColorsOnlyTheMarkedWord()
    {
        var result = new AdvancedEffectKaraoke().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        Assert.Equal(9, result.Count);
        Assert.Equal("{\\1c&H808080&}Hi there", result[0].Text);
        Assert.Equal("{\\1c&HFFFFFF&}Hi{\\1c&H808080&} there", result[3].Text);
    }

    [Fact]
    public void WordByWord_ShowsWordsThroughTheMarkedOne()
    {
        var result = new AdvancedEffectWordByWord().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        Assert.Equal(new[] { "Hi", "Hi", "Hi there" }, result.Select(r => r.Text));
        Assert.Equal(TimeSpan.FromSeconds(10), result[0].StartTime);
    }

    [Fact]
    public void WordFlip3D_FlipsOnlyTheMarkedWord()
    {
        var result = new AdvancedEffectWordFlip3D().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        Assert.Equal(3, result.Count); // the first pause has nothing to show
        Assert.Equal(TimeSpan.FromSeconds(10), result[0].StartTime);
        Assert.Equal(TimeSpan.FromSeconds(11), result[0].EndTime);
        Assert.StartsWith("{\\alpha&H00&\\frx90", result[0].Text);
        Assert.Equal("{\\alpha&H00&\\frx0\\fscy100}Hi {\\alpha&HFF&\\frx0\\fscy100}there", result[1].Text);
        Assert.StartsWith("{\\alpha&H00&\\frx0\\fscy100}Hi {\\alpha&H00&\\frx90", result[2].Text);
    }

    [Fact]
    public void SmoothFill_FillsOnlyTheMarkedWord()
    {
        var result = new AdvancedEffectSmoothFill().ApplyEffect(Header, Sequence(), 1920, 1080, null);

        Assert.Equal(4, result.Count);
        Assert.EndsWith("{\\k100}{\\k100}Hi {\\k100}there", result[0].Text); // pause: wait, then nothing fills
        Assert.EndsWith("{\\kf100}Hi {\\k100}there", result[1].Text);
        Assert.EndsWith("{\\kf0}Hi {\\k100}{\\k100}there", result[2].Text);
        Assert.EndsWith("{\\kf0}Hi {\\kf100}there", result[3].Text);
    }

    [Fact]
    public void Typewriter_WithoutMarks_IsUnchanged()
    {
        var result = new AdvancedEffectTypewriter().ApplyEffect(Header, new List<SubtitleLineViewModel> { Line("Hi", 1, 2) }, 1920, 1080, null);

        Assert.Equal(new[] { "H", "Hi" }, result.Select(r => r.Text));
    }
}
