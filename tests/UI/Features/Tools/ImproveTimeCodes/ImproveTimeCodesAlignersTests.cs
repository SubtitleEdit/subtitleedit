using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class ImproveTimeCodesAlignersTests
{
    [Theory]
    [InlineData("en", ForcedAlignerOption.CanaryCtcChoice, ForcedAlignerOption.Wav2Vec2EnChoice, ForcedAlignerOption.Qwen3Choice)]
    [InlineData("de", ForcedAlignerOption.Wav2Vec2DeChoice, ForcedAlignerOption.CanaryCtcChoice, ForcedAlignerOption.Qwen3Choice)]
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

    [Theory]
    [InlineData("en", "phonon2-q8_0.gguf")]
    [InlineData("EN", "phonon2-q8_0.gguf")]
    [InlineData("de", "parakeet-tdt-0.6b-v3-q4_k.gguf")]
    [InlineData("", "parakeet-tdt-0.6b-v3-q4_k.gguf")]
    public void PickSpeechToTextModel_UsesInstalledPhonon2OnlyForEnglish(string language, string expected)
    {
        var model = ImproveTimeCodesAligners.PickSpeechToTextModel(new CrispAsrParakeet(), language, m => m.Name == "phonon2-q8_0.gguf");

        Assert.Equal(expected, model.Name);
    }

    [Fact]
    public void PickSpeechToTextModel_PrefersMultilingualModelsOverPhonon2()
    {
        var installed = new HashSet<string> { "phonon2-f16.gguf", "parakeet-tdt-0.6b-v3-q8_0.gguf" };

        var model = ImproveTimeCodesAligners.PickSpeechToTextModel(new CrispAsrParakeet(), "en", m => installed.Contains(m.Name));

        Assert.Equal("parakeet-tdt-0.6b-v3-q8_0.gguf", model.Name);
    }

    [Fact]
    public void PickSpeechToTextModel_PrefersPhonon2OverOtherEnglishOnlyModels()
    {
        var installed = new HashSet<string> { "parakeet-tdt-1.1b-q4_k.gguf", "phonon2-q4_k.gguf" };

        var model = ImproveTimeCodesAligners.PickSpeechToTextModel(new CrispAsrParakeet(), "en", m => installed.Contains(m.Name));

        Assert.Equal("phonon2-q4_k.gguf", model.Name);
    }
}
