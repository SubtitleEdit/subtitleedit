using Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;

namespace LibUiLogicTests.Ocr.Paddle;

public class PaddleOcrResultJsonTests
{
    // Real output shape from the PaddleOCR CLI (--save_path -> "<n>_res.json").
    private const string SingleLine = """
        {
            "input_path": "0001.png",
            "rec_texts": ["Hello world"],
            "rec_scores": [0.9967420101165771],
            "rec_polys": [[[12, 11], [203, 14], [203, 56], [12, 54]]],
            "rec_boxes": [[12, 11, 203, 56]]
        }
        """;

    [Fact]
    public void TryParse_SingleLine_ReturnsTextScoreAndBox()
    {
        Assert.True(PaddleOcrResultJson.TryParse(SingleLine, out var regions, out var error));

        Assert.Null(error);
        var region = Assert.Single(regions);
        Assert.Equal("Hello world", region.Text);
        Assert.Equal(0.9967, region.Confidence, precision: 4);
        Assert.Equal(12, region.BoundingBox.TopLeft.X);
        Assert.Equal(11, region.BoundingBox.TopLeft.Y);
        Assert.Equal(203, region.BoundingBox.TopRight.X);
    }

    [Fact]
    public void TryParse_TwoLines_ParsesBoth()
    {
        const string json = """
            {
                "rec_texts": ["First line", "Second line"],
                "rec_scores": [0.99, 0.98],
                "rec_polys": [
                    [[10, 10], [200, 10], [200, 40], [10, 40]],
                    [[10, 50], [200, 50], [200, 80], [10, 80]]
                ]
            }
            """;

        Assert.True(PaddleOcrResultJson.TryParse(json, out var regions, out _));

        Assert.Equal(2, regions.Count);
        Assert.Equal("First line", regions[0].Text);
        Assert.Equal("Second line", regions[1].Text);
        Assert.Equal(50, regions[1].BoundingBox.TopLeft.Y);
    }

    [Fact]
    public void TryParse_NoRecTexts_SucceedsWithNothingRecognised()
    {
        // PaddleOCR writes a result file even for an image it found no text in.
        Assert.True(PaddleOcrResultJson.TryParse("""{ "input_path": "0001.png" }""", out var regions, out var error));
        Assert.Empty(regions);
        Assert.Null(error);
    }

    [Fact]
    public void TryParse_MissingPolysAndScores_StillReturnsText()
    {
        Assert.True(PaddleOcrResultJson.TryParse("""{ "rec_texts": ["Alpha", "Beta"] }""", out var regions, out _));

        Assert.Equal(2, regions.Count);
        Assert.Equal("Alpha", regions[0].Text);
        Assert.Equal(0, regions[0].Confidence);
        Assert.Equal(0, regions[0].BoundingBox.Width);
    }

    [Fact]
    public void TryParse_Malformed_FailsWithAReasonAndNoRegions()
    {
        Assert.False(PaddleOcrResultJson.TryParse("""{ "rec_texts": ["Alpha" """, out var regions, out var error));
        Assert.Empty(regions);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("""{ "rec_texts": [] }""", true)]
    [InlineData("""{ "rec_texts": ["Alpha"] }   """, true)]
    [InlineData("""{ "rec_texts": ["Alpha" """, false)]
    [InlineData("", false)]
    public void IsComplete_TellsAFinishedFileFromOneStillBeingWritten(string json, bool expected)
    {
        // The poller reads files while PaddleOCR is still writing them, so "did the object
        // close" is what decides whether a file is taken now or left for the next round.
        Assert.Equal(expected, PaddleOcrResultJson.IsComplete(json));
    }
}
