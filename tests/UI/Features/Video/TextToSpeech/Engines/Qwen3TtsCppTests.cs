using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

public class Qwen3TtsCppTests
{
    [Fact]
    public async Task GetModels_Returns_AllVariants()
    {
        var engine = new Qwen3TtsCpp();
        var models = await engine.GetModels();

        Assert.Equal(new[] { Qwen3TtsCpp.ModelKey06B, Qwen3TtsCpp.ModelKey17BBase, Qwen3TtsCpp.ModelKey17BVoiceDesign }, models);
    }

    [Theory]
    [InlineData(Qwen3TtsCpp.ModelKey06B, Qwen3TtsCpp.TtsModelFileName06B)]
    [InlineData(Qwen3TtsCpp.ModelKey17BBase, Qwen3TtsCpp.TtsModelFileName17BBase)]
    [InlineData(Qwen3TtsCpp.ModelKey17BVoiceDesign, Qwen3TtsCpp.TtsModelFileName17BVoiceDesign)]
    [InlineData("unknown", Qwen3TtsCpp.TtsModelFileName06B)]
    [InlineData(null, Qwen3TtsCpp.TtsModelFileName06B)]
    public void GetModelFileName_MapsKeyToFile(string? key, string expected)
    {
        Assert.Equal(expected, Qwen3TtsCpp.GetModelFileName(key));
    }
}
