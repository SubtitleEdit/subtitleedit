using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.SkipNoiseLines;

namespace UITests.Features.Video.TextToSpeech.SkipNoiseLines;

public class NoiseLineDetectorTests
{
    [Theory]
    [InlineData("♪")]
    [InlineData("♪ [Suspenseful]")]
    [InlineData("[ambient sounds]")]
    [InlineData("(sighs)")]
    [InlineData("[door slams] [glass breaks]")]
    [InlineData("<i>♪</i>")]
    [InlineData("{\\an8}[music]")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("<i></i>")]
    public void SoundAndMusicOnlyLinesAreNoise(string? text)
    {
        Assert.True(NoiseLineDetector.IsNoiseOnly(text));
    }

    [Theory]
    [InlineData("Hello there.")]
    [InlineData("NARRATOR: Text")]
    [InlineData("[gunshot] Get down!")]
    [InlineData("♪ sweet dreams are made of this ♪")]
    [InlineData("(Speaker 1) Henry.")]
    public void LinesWithSpeechAreKept(string text)
    {
        Assert.False(NoiseLineDetector.IsNoiseOnly(text));
    }

    [Fact]
    public void DetectReturnsOnlyTheNoiseLines()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("NARRATOR:\nText", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("♪ [Suspenseful]", 2000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("More text", 4000, 5000));
        subtitle.Paragraphs.Add(new Paragraph("♪", 6000, 7000));

        var noise = NoiseLineDetector.Detect(subtitle);

        Assert.Equal(2, noise.Count);
        Assert.Same(subtitle.Paragraphs[1], noise[0]);
        Assert.Same(subtitle.Paragraphs[3], noise[1]);
    }
}
