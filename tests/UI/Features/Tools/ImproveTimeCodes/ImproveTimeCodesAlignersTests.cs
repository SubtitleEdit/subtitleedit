using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class ImproveTimeCodesAlignersTests
{
    [Theory]
    [InlineData("en", ForcedAlignerOption.Wav2Vec2EnChoice, ForcedAlignerOption.CanaryCtcChoice, ForcedAlignerOption.Qwen3Choice)]
    [InlineData("da", ForcedAlignerOption.CanaryCtcChoice, ForcedAlignerOption.Qwen3Choice, null)]
    [InlineData("ja", ForcedAlignerOption.Wav2Vec2JaChoice, ForcedAlignerOption.Qwen3Choice, ForcedAlignerOption.CanaryCtcChoice)]
    [InlineData("ko", ForcedAlignerOption.Qwen3Choice, ForcedAlignerOption.CanaryCtcChoice, null)]
    [InlineData("", ForcedAlignerOption.CanaryCtcChoice, ForcedAlignerOption.Qwen3Choice, null)]
    public void Rank_PutsTheBestAlignersForTheLanguageFirst(string language, string first, string second, string? third)
    {
        var ranked = ImproveTimeCodesAligners.Rank(language);

        Assert.Equal(first, ranked[0].Choice);
        Assert.Equal(second, ranked[1].Choice);
        if (third != null)
        {
            Assert.Equal(third, ranked[2].Choice);
        }
    }

    [Fact]
    public void Rank_OffersEveryAlignerOnce_AndNeverTheBuiltInOne()
    {
        var ranked = ImproveTimeCodesAligners.Rank("fr");

        Assert.Equal(ForcedAlignerOption.All().Count(o => !o.IsBuiltIn), ranked.Count);
        Assert.Equal(ranked.Count, ranked.Select(o => o.Choice).Distinct().Count());
        Assert.DoesNotContain(ranked, o => o.IsBuiltIn);
    }
}
