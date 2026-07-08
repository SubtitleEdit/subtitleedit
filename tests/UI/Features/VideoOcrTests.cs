using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features;

public class VideoOcrTests
{
    private static VideoOcrFrameGroup MakeGroup(int startFrame, int endFrame, string text, bool isBlank = false)
    {
        return new VideoOcrFrameGroup
        {
            StartFrame = startFrame,
            EndFrame = endFrame,
            Text = text,
            IsBlank = isBlank,
        };
    }

    [Fact]
    public void Build_SimilarConsecutiveTexts_MergedIntoOneLine()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 4, "Hello world"),
            MakeGroup(5, 14, "Hello world"),
            MakeGroup(15, 19, "Hello worId"), // OCR glitch: capital I instead of l
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Single(lines);
        Assert.Equal("Hello world", lines[0].Text); // majority text wins
        Assert.Equal(0, lines[0].StartMs);
        Assert.Equal(4000, lines[0].EndMs);
    }

    [Fact]
    public void Build_DifferentTexts_SeparateLines()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 9, "First subtitle"),
            MakeGroup(10, 19, "A completely different text"),
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Equal(2, lines.Count);
        Assert.Equal("First subtitle", lines[0].Text);
        Assert.Equal("A completely different text", lines[1].Text);
    }

    [Fact]
    public void Build_LongBlankStretch_SameTextBecomesTwoLines()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 9, "Hello"),
            MakeGroup(10, 19, string.Empty, isBlank: true), // 2000 ms blank > max gap
            MakeGroup(20, 29, "Hello"),
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Equal(2, lines.Count);
    }

    [Fact]
    public void Build_OneFrameBlankFlicker_BridgedByMaxGap()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 9, "Hello"),
            MakeGroup(10, 10, string.Empty, isBlank: true), // 200 ms flicker <= 250 ms max gap
            MakeGroup(11, 20, "Hello"),
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Single(lines);
        Assert.Equal(0, lines[0].StartMs);
        Assert.Equal(4200, lines[0].EndMs);
    }

    [Fact]
    public void Build_ShortBlip_Dropped()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 0, "logo"), // single frame at 5 fps = 200 ms
            MakeGroup(5, 14, "Real subtitle text"),
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Single(lines);
        Assert.Equal("Real subtitle text", lines[0].Text);
    }

    [Fact]
    public void Build_EmptyOcrResults_NoLines()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 9, "   "),
            MakeGroup(10, 19, string.Empty),
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Empty(lines);
    }

    [Fact]
    public void Build_MajorityVote_LongestShownTextWins()
    {
        var groups = new List<VideoOcrFrameGroup>
        {
            MakeGroup(0, 1, "He1lo there"),
            MakeGroup(2, 20, "Hello there"),
            MakeGroup(21, 22, "Hell0 there"),
        };

        var lines = VideoOcrLineBuilder.Build(groups, 5, 80, 250, 250);

        Assert.Single(lines);
        Assert.Equal("Hello there", lines[0].Text);
    }

    [Theory]
    [InlineData("Hello world", "Hello world", 100)]
    [InlineData("Hello world", "HELLO  WORLD", 100)] // case and whitespace ignored
    [InlineData("Hello world", "", 0)]
    [InlineData("", "", 100)]
    public void GetTextSimilarityPercent_KnownValues(string a, string b, int expected)
    {
        Assert.Equal(expected, VideoOcrLineBuilder.GetTextSimilarityPercent(a, b));
    }

    [Fact]
    public void GetTextSimilarityPercent_SmallOcrJitter_IsHigh()
    {
        var similarity = VideoOcrLineBuilder.GetTextSimilarityPercent("My mommy always said", "My mornmy a1ways said");
        Assert.True(similarity >= 80, $"Expected >= 80 but was {similarity}");
    }

    [Fact]
    public void GetTextSimilarityPercent_DifferentTexts_IsLow()
    {
        var similarity = VideoOcrLineBuilder.GetTextSimilarityPercent("My mommy always said", "Life is like a box of chocolates");
        Assert.True(similarity < 50, $"Expected < 50 but was {similarity}");
    }

    [Theory]
    [InlineData(0.5, 0.9, "")] // bottom center - default, no tag
    [InlineData(0.5, 0.1, "{\\an8}")] // top center
    [InlineData(0.1, 0.9, "{\\an1}")] // bottom left
    [InlineData(0.9, 0.5, "{\\an6}")] // middle right
    public void GetAssaAlignmentTag_KnownPositions(double relativeX, double relativeY, string expected)
    {
        Assert.Equal(expected, VideoOcrLineBuilder.GetAssaAlignmentTag(relativeX, relativeY));
    }

    [Fact]
    public void FrameGroup_Timing_UsesFrameRate()
    {
        var group = MakeGroup(10, 19, "x");
        Assert.Equal(2000, group.GetStartMs(5));
        Assert.Equal(4000, group.GetEndMs(5));
    }

    [Theory]
    [InlineData("Hello world", "Hello world")]
    [InlineData("Hello\nworld", "Hello\nworld")]
    [InlineData("```markdown\nHello world\n```", "Hello world")] // markdown fences stripped
    [InlineData("Hello world\nHello world\nHello world", "Hello world")] // repeated lines deduped
    [InlineData("```markdown\n\n```\n+\n```\n```", "")] // pure hallucination becomes empty
    [InlineData("You are an OCR engine. The language is English.", "")] // prompt echo removed
    [InlineData("", "")]
    public void CleanOcrResult_KnownValues(string input, string expected)
    {
        Assert.Equal(expected, VideoOcrLineBuilder.CleanOcrResult(input));
    }

    [Fact]
    public void GetMaskSimilarityPercent_SameMask_Is100()
    {
        var a = new byte[] { 0, 255, 255, 0, 0, 0, 0, 0 };
        Assert.Equal(100, VideoOcrFrameGrouper.GetMaskSimilarityPercent(a, (byte[])a.Clone()));
    }

    [Fact]
    public void GetMaskSimilarityPercent_RelativeToMaskSizeNotImageSize()
    {
        // 2 of 3 bright pixels overlap in a 12-pixel image: a plain pixel diff would say
        // ~83% similar, but relative to the bright mask it is 50% (2 of 4 union pixels).
        var a = new byte[] { 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var b = new byte[] { 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0 };
        Assert.Equal(50, VideoOcrFrameGrouper.GetMaskSimilarityPercent(a, b));
    }

    [Fact]
    public void GetMaskSimilarityPercent_BothEmpty_Is100()
    {
        var a = new byte[8];
        Assert.Equal(100, VideoOcrFrameGrouper.GetMaskSimilarityPercent(a, (byte[])a.Clone()));
    }

    [Fact]
    public void GetSimilarityPercent_IdenticalThumbnails_Is100()
    {
        var a = new byte[] { 0, 255, 128, 0 };
        Assert.Equal(100, VideoOcrFrameGrouper.GetSimilarityPercent(a, (byte[])a.Clone()));
    }

    [Fact]
    public void GetSimilarityPercent_OppositeThumbnails_Is0()
    {
        var a = new byte[] { 0, 0, 0, 0 };
        var b = new byte[] { 255, 255, 255, 255 };
        Assert.Equal(0, VideoOcrFrameGrouper.GetSimilarityPercent(a, b));
    }
}
