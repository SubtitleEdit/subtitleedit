using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr;

// OCR "capture alignment" should only add an alignment tag when the text is NOT at the
// default bottom-center position (an2) - previously every OCR'ed line got a tag, even
// bottom-center ones (issue #12393).
public class OcrCaptureAlignmentTests
{
    [Fact]
    public void ApplyAlignmentTag_TopCenter_AddsAn8()
    {
        var result = OcrViewModel.ApplyAlignmentTag("Hello", "an8", writeAn2Tag: false);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an8}Hello", result.Text);
    }

    [Fact]
    public void ApplyAlignmentTag_BottomCenter_NoTagAdded()
    {
        var result = OcrViewModel.ApplyAlignmentTag("Hello", "an2", writeAn2Tag: false);

        Assert.False(result.AlignmentAdded);
        Assert.Equal("Hello", result.Text);
    }

    [Fact]
    public void ApplyAlignmentTag_BottomCenterWithWriteAn2Tag_AddsAn2()
    {
        var result = OcrViewModel.ApplyAlignmentTag("Hello", "an2", writeAn2Tag: true);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an2}Hello", result.Text);
    }

    [Fact]
    public void ApplyLineAlignmentTags_AllBottomCenter_NoTagsAdded()
    {
        var lines = new List<string> { "One", "Two" };
        var alignments = new List<string> { "an2", "an2" };

        var result = OcrViewModel.ApplyLineAlignmentTags(lines, alignments, "One\nTwo", writeAn2Tag: false);

        Assert.False(result.AlignmentAdded);
        Assert.Equal("One\nTwo", result.Text);
    }

    [Fact]
    public void ApplyLineAlignmentTags_MixedAlignments_TagsAllLines()
    {
        var lines = new List<string> { "One", "Two" };
        var alignments = new List<string> { "an8", "an2" };

        var result = OcrViewModel.ApplyLineAlignmentTags(lines, alignments, "One\nTwo", writeAn2Tag: false);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an8}One\n{\\an2}Two", result.Text);
    }

    [Fact]
    public void ApplyLineAlignmentTags_AllBottomCenterWithWriteAn2Tag_TagsAllLines()
    {
        var lines = new List<string> { "One", "Two" };
        var alignments = new List<string> { "an2", "an2" };

        var result = OcrViewModel.ApplyLineAlignmentTags(lines, alignments, "One\nTwo", writeAn2Tag: true);

        Assert.True(result.AlignmentAdded);
        Assert.Equal("{\\an2}One\n{\\an2}Two", result.Text);
    }

    [Theory]
    [InlineData(0.5, 0.1, "an8")] // top-center of screen
    [InlineData(0.5, 0.9, "an2")] // bottom-center of screen
    [InlineData(0.5, 0.5, "an5")] // middle-center of screen
    [InlineData(0.1, 0.9, "an1")] // bottom-left of screen
    [InlineData(0.9, 0.1, "an9")] // top-right of screen
    public void GetAssaPositionFromScreen_MapsScreenPositionToAlignment(double relativeX, double relativeY, string expected)
    {
        Assert.Equal(expected, OcrViewModel.GetAssaPositionFromScreen(relativeX, relativeY));
    }
}
