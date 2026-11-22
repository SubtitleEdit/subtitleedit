using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

public class CrispAsrQwen3ModelTests
{
    [Fact]
    public void RumbleQ8LeadsTheModelList()
    {
        // The first entry is what a new user gets; Rumble q8_0 scored best on FLEURS (9.0% pooled).
        var models = new CrispAsrQwen3().Models;

        Assert.Equal("rumble-2-flash-q8_0.gguf", models[0].Name);
        Assert.Equal("rumble-2-flash-q4_k.gguf", models[1].Name);
    }

    [Fact]
    public void MinimumCrispAsrVersion_RumbleModelsNeed0855()
    {
        var engine = new CrispAsrQwen3();
        var rumble = engine.Models.Where(m => m.Name.StartsWith("rumble-")).ToList();

        Assert.Equal(2, rumble.Count);
        Assert.All(rumble, m => Assert.Equal(new Version(0, 8, 55), engine.GetMinimumCrispAsrVersion(m)));
    }

    [Fact]
    public void MinimumCrispAsrVersion_QwenModelsHaveNone()
    {
        var engine = new CrispAsrQwen3();

        Assert.All(engine.Models.Where(m => m.Name.StartsWith("qwen3-")), m => Assert.Null(engine.GetMinimumCrispAsrVersion(m)));
    }

    [Fact]
    public void RumbleModelFileNamesMatchTheirUrls()
    {
        foreach (var model in new CrispAsrQwen3().Models.Where(m => m.Name.StartsWith("rumble-")))
        {
            var url = Assert.Single(model.Urls);
            Assert.EndsWith("/" + model.Name, url);
            Assert.StartsWith("https://huggingface.co/cstr/rumble-2-flash-GGUF/resolve/main/", url);
        }
    }
}
