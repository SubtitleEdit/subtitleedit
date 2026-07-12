using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// Alignment shortcuts should toggle: pressing e.g. "an8" when the line already has {\an8}
// removes the tag again, and "an2" (default bottom-center) is only written when the
// WriteAn2Tag setting is enabled (issue #12393).
public class AlignmentTagHelperTests
{
    [Theory]
    [InlineData("{\\an8}Hello", "an8", true)]
    [InlineData("{\\an8\\i1}Hello", "an8", true)]
    [InlineData("Hello", "an8", false)]
    [InlineData("{\\an7}Hello", "an8", false)]
    public void HasAlignment(string text, string alignment, bool expected)
    {
        Assert.Equal(expected, AlignmentTagHelper.HasAlignment(text, alignment));
    }

    [Theory]
    [InlineData("{\\an8}Hello", "Hello")]
    [InlineData("{\\an8\\i1}Hello", "{\\i1}Hello")]
    [InlineData("Hello", "Hello")]
    [InlineData("{\\an1}One\n{\\an9}Two", "One\nTwo")]
    public void RemoveAlignmentTags(string text, string expected)
    {
        Assert.Equal(expected, AlignmentTagHelper.RemoveAlignmentTags(text));
    }

    [Fact]
    public void SetAlignment_PlainText_AddsTag()
    {
        Assert.Equal("{\\an8}Hello", AlignmentTagHelper.SetAlignment("Hello", "an8", writeAn2Tag: false));
    }

    [Fact]
    public void SetAlignment_ReplacesExistingAlignment()
    {
        Assert.Equal("{\\an8}Hello", AlignmentTagHelper.SetAlignment("{\\an7}Hello", "an8", writeAn2Tag: false));
    }

    [Fact]
    public void SetAlignment_ExistingOverrideBlock_InsertsIntoBlock()
    {
        Assert.Equal("{\\an8\\i1}Hello", AlignmentTagHelper.SetAlignment("{\\i1}Hello", "an8", writeAn2Tag: false));
    }

    [Fact]
    public void SetAlignment_An2WithoutWriteAn2Tag_OnlyRemoves()
    {
        Assert.Equal("Hello", AlignmentTagHelper.SetAlignment("{\\an8}Hello", "an2", writeAn2Tag: false));
    }

    [Fact]
    public void SetAlignment_An2WithWriteAn2Tag_AddsTag()
    {
        Assert.Equal("{\\an2}Hello", AlignmentTagHelper.SetAlignment("Hello", "an2", writeAn2Tag: true));
    }

    [Fact]
    public void SetAlignment_EmptyText_StaysEmpty()
    {
        Assert.Equal(string.Empty, AlignmentTagHelper.SetAlignment(string.Empty, "an8", writeAn2Tag: false));
    }
}
