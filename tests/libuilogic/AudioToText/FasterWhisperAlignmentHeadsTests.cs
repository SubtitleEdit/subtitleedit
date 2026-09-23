using Nikse.SubtitleEdit.UiLogic.AudioToText;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace LibUiLogicTests.AudioToText;

public class FasterWhisperAlignmentHeadsTests
{
    // The anime-whisper CT2 conversion's config.json (#15223): large-v3 heads on a 2-layer decoder.
    private const string AnimeWhisperConfig =
        "{\"alignment_heads\": [[7, 0], [10, 17], [12, 18], [13, 12], [16, 1], [17, 14], [19, 11], [21, 4], [24, 1], [25, 6]], \"lang_ids\": [50259], \"suppress_ids\": [1, 2]}";

    [Fact]
    public void OutOfRangeHeadsAreReplacedByAllHeadsOfLastLayer()
    {
        var json = FasterWhisperAlignmentHeads.RepairJson(AnimeWhisperConfig, 2, 20);

        Assert.NotNull(json);
        var root = JsonNode.Parse(json!)!.AsObject();
        var heads = root["alignment_heads"]!.AsArray()
            .Select(h => (Layer: h![0]!.GetValue<int>(), Head: h[1]!.GetValue<int>()))
            .ToList();
        Assert.Equal(20, heads.Count);
        Assert.All(heads, h => Assert.Equal(1, h.Layer));
        Assert.Equal(Enumerable.Range(0, 20), heads.Select(h => h.Head));

        // Everything else is kept.
        Assert.Equal(50259, root["lang_ids"]![0]!.GetValue<int>());
        Assert.Equal(2, root["suppress_ids"]!.AsArray().Count);
    }

    [Fact]
    public void ValidHeadsAreLeftAlone()
    {
        Assert.Null(FasterWhisperAlignmentHeads.RepairJson("{\"alignment_heads\": [[1, 0], [1, 5]]}", 2, 20));
        Assert.Null(FasterWhisperAlignmentHeads.RepairJson(AnimeWhisperConfig, 32, 20));
    }

    [Fact]
    public void RepairedJsonNeedsNoSecondRepair()
    {
        var json = FasterWhisperAlignmentHeads.RepairJson(AnimeWhisperConfig, 2, 20);

        Assert.Null(FasterWhisperAlignmentHeads.RepairJson(json!, 2, 20));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("not json")]
    [InlineData("[1, 2]")]
    public void MissingOrUnreadableConfigIsLeftAlone(string json)
    {
        Assert.Null(FasterWhisperAlignmentHeads.RepairJson(json, 2, 20));
    }
}
