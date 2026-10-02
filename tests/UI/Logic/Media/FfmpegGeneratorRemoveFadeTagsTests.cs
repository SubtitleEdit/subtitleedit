using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Regression tests for #15580: the libass measurement render (Set position / Set background)
/// only draws the frame at t=0, where a fade-in is fully transparent, so fades must be dropped.
/// </summary>
public class FfmpegGeneratorRemoveFadeTagsTests
{
    [Theory]
    [InlineData("{\\fade(1000,1000)}Hello", "Hello")]
    [InlineData("{\\fad(500,0)}Hello", "Hello")]
    [InlineData("{\\fade(255,0,255,0,500,1500,2000)}Hello", "Hello")]
    [InlineData("{\\b1\\fad(500,0)\\i1}Hello", "{\\b1\\i1}Hello")]
    [InlineData("{\\pos(100,200)}{\\fad(300,300)}Hello", "{\\pos(100,200)}Hello")]
    [InlineData("{\\fad (300,300)}Hello", "Hello")]
    public void RemoveFadeTags_RemovesFades(string input, string expected)
    {
        Assert.Equal(expected, FfmpegGenerator.RemoveFadeTags(input));
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("{\\pos(100,200)\\frz10}Hello")]
    [InlineData("{\\an8}fade(1000,1000) is plain text")]
    [InlineData("")]
    public void RemoveFadeTags_LeavesOtherTextUntouched(string input)
    {
        Assert.Equal(input, FfmpegGenerator.RemoveFadeTags(input));
    }
}
