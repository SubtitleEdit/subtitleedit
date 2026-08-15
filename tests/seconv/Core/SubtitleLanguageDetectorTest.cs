using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// The memoized detector must be indistinguishable from calling libse directly — the cache only
/// exists so a run combining several language-dependent operations does not repeat a ~60 ms
/// whole-file detection per operation.
/// </summary>
public class SubtitleLanguageDetectorTest
{
    private static Subtitle Build(params string[] texts)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < texts.Length; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(texts[i], i * 2000, i * 2000 + 1500));
        }

        return subtitle;
    }

    private static Subtitle English() => Build(
        "I think we should go now, and take the car with us.",
        "She said that the weather would be much better tomorrow.",
        "Do you really want to know what happened to them?",
        "It was the last thing anyone expected him to say.");

    private static Subtitle Spanish() => Build(
        "Creo que deberíamos irnos ahora y llevarnos el coche.",
        "Ella dijo que el tiempo sería mucho mejor mañana.",
        "¿De verdad quieres saber lo que les pasó?",
        "Fue lo último que nadie esperaba que él dijera.");

    [Fact]
    public void DetectOrNull_MatchesLibSe()
    {
        var subtitle = English();

        Assert.Equal(LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle), SubtitleLanguageDetector.DetectOrNull(subtitle));
    }

    [Fact]
    public void Detect_FallsBackToEnglish()
    {
        var subtitle = English();

        Assert.Equal(LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle), SubtitleLanguageDetector.Detect(subtitle));
    }

    [Fact]
    public void DetectOrNull_RepeatedCallsAgree()
    {
        var subtitle = English();

        var first = SubtitleLanguageDetector.DetectOrNull(subtitle);
        var second = SubtitleLanguageDetector.DetectOrNull(subtitle);

        Assert.Equal(first, second);
    }

    /// <summary>
    /// The cache is keyed on the subtitle's content, not its identity: an operation that rewrites
    /// the text between two detections must get a fresh answer, not the previous one.
    /// </summary>
    [Fact]
    public void DetectOrNull_RedetectsAfterTextChanges()
    {
        var subtitle = English();
        var english = SubtitleLanguageDetector.DetectOrNull(subtitle);

        var spanish = Spanish();
        subtitle.Paragraphs.Clear();
        subtitle.Paragraphs.AddRange(spanish.Paragraphs);

        var redetected = SubtitleLanguageDetector.DetectOrNull(subtitle);

        Assert.Equal(LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle), redetected);
        Assert.NotEqual(english, redetected);
    }

    /// <summary>A different subtitle instance must not read the previous one's cached answer.</summary>
    [Fact]
    public void DetectOrNull_DoesNotLeakAcrossSubtitles()
    {
        var english = English();
        var spanish = Spanish();

        SubtitleLanguageDetector.DetectOrNull(english);

        Assert.Equal(LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(spanish), SubtitleLanguageDetector.DetectOrNull(spanish));
        Assert.Equal(LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(english), SubtitleLanguageDetector.DetectOrNull(english));
    }

    [Fact]
    public void DetectOrNull_NullSubtitle_ReturnsNull()
    {
        Assert.Null(SubtitleLanguageDetector.DetectOrNull(null!));
    }
}
