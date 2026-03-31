using System.Linq;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

public class Qwen3AsrCppEngineTests
{
    [Fact]
    public void Models_Includes_ExpectedQwen3Variants()
    {
        var engine = new Qwen3AsrCppEngine();
        var modelNames = engine.Models.Select(x => x.Name).ToList();

        Assert.Contains("qwen3-asr-0.6b-f16.gguf", modelNames);
        Assert.Contains("qwen3-asr-0.6b-q8_0.gguf", modelNames);
        Assert.Contains("qwen3-asr-1.7b-f16.gguf", modelNames);
        Assert.Contains("qwen3-asr-1.7b-q8_0.gguf", modelNames);
    }
}
