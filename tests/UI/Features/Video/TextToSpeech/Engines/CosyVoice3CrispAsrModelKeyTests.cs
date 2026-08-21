using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// CosyVoice3 offers the same bundle with four different talkers: two quantisations of upstream's
/// pre-trained <c>llm.pt</c> and two of its RL-tuned <c>llm.rl.pt</c> (#13272). Only the LLM file
/// differs between them — every companion (flow / HiFT / s3tok / CAMPPlus / voice bank) is shared —
/// so the mapping from model key to file name is the whole of what "picking RL" means.
/// </summary>
public class CosyVoice3CrispAsrModelKeyTests
{
    [Theory]
    [InlineData(CosyVoice3CrispAsr.ModelKeyQ4K, CosyVoice3CrispAsr.LlmQ4KFileName)]
    [InlineData(CosyVoice3CrispAsr.ModelKeyF16, CosyVoice3CrispAsr.LlmF16FileName)]
    [InlineData(CosyVoice3CrispAsr.ModelKeyRlQ4K, CosyVoice3CrispAsr.LlmRlQ4KFileName)]
    [InlineData(CosyVoice3CrispAsr.ModelKeyRlF16, CosyVoice3CrispAsr.LlmRlF16FileName)]
    public void EachModelKey_MapsToItsOwnLlmGguf(string modelKey, string expectedLlm)
    {
        Assert.Equal(expectedLlm, CosyVoice3CrispAsr.GetLlmFileName(modelKey));
        Assert.Equal(modelKey, CosyVoice3CrispAsr.ResolveModelKey(modelKey));
    }

    [Fact]
    public async Task AllModelKeys_AreOfferedAndDistinct()
    {
        var models = await new CosyVoice3CrispAsr().GetModels();

        Assert.Equal(4, models.Length);
        Assert.Equal(models.Length, models.Distinct().Count());
        Assert.Contains(CosyVoice3CrispAsr.ModelKeyRlQ4K, models);
        Assert.Contains(CosyVoice3CrispAsr.ModelKeyRlF16, models);
    }

    [Theory]
    [InlineData(CosyVoice3CrispAsr.ModelKeyRlQ4K)]
    [InlineData(CosyVoice3CrispAsr.ModelKeyRlF16)]
    public void RlBundle_DiffersFromTheBaseBundleOnlyInTheTalker(string rlModelKey)
    {
        var rl = CosyVoice3CrispAsr.GetRequiredFileNames(rlModelKey);
        var baseline = CosyVoice3CrispAsr.GetRequiredFileNames(CosyVoice3CrispAsr.ModelKeyQ4K);

        // Same six files, and everything after the talker is shared - that is what makes an RL
        // pick a single extra download for someone who already has a bundle installed.
        Assert.Equal(baseline.Length, rl.Length);
        Assert.Equal(baseline.Skip(1), rl.Skip(1));
        Assert.NotEqual(baseline[0], rl[0]);
        Assert.Contains("-rl-", rl[0]);
    }
}
