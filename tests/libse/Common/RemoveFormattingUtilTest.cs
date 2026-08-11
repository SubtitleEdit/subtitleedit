using Nikse.SubtitleEdit.Core.Common;
using Xunit;

namespace LibSETests.Common;

/// <summary>
/// RemoveFormattingUtil is the single source of truth for both the GUI batch convert
/// "Remove formatting" function and seconv's --remove-formatting / --remove-formatting-rules
/// (#13518). These tests pin down each category's behaviour and the key design distinction:
/// <see cref="RemoveFormattingType.All"/> is wholesale (strips tags no named category
/// covers, e.g. {\pos(..)}), while the union of the named categories leaves those alone.
/// </summary>
public class RemoveFormattingUtilTest
{
    private const RemoveFormattingType AllNamed =
        RemoveFormattingType.Italic |
        RemoveFormattingType.Bold |
        RemoveFormattingType.Underline |
        RemoveFormattingType.FontName |
        RemoveFormattingType.Alignment |
        RemoveFormattingType.Color;

    [Fact]
    public void None_LeavesTextUnchanged()
    {
        Assert.Equal("<i>Hi</i>", RemoveFormattingUtil.Remove("<i>Hi</i>", RemoveFormattingType.None));
    }

    [Fact]
    public void Italic_RemovesHtmlAndAssaItalic_KeepsBold()
    {
        Assert.Equal("Hi <b>there</b>", RemoveFormattingUtil.Remove("<i>Hi</i> <b>there</b>", RemoveFormattingType.Italic));
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("{\\i1}Hi{\\i0}", RemoveFormattingType.Italic));
    }

    [Fact]
    public void Bold_RemovesHtmlAndAssaBold_KeepsItalic()
    {
        Assert.Equal("<i>Hi</i> there", RemoveFormattingUtil.Remove("<i>Hi</i> <b>there</b>", RemoveFormattingType.Bold));
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("{\\b1}Hi{\\b0}", RemoveFormattingType.Bold));
    }

    [Fact]
    public void Underline_RemovesHtmlAndAssaUnderline()
    {
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("<u>Hi</u>", RemoveFormattingType.Underline));
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("{\\u1}Hi{\\u0}", RemoveFormattingType.Underline));
    }

    [Fact]
    public void Color_RemovesFontColorAndAssaColor()
    {
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("<font color=\"red\">Hi</font>", RemoveFormattingType.Color));
        Assert.Equal("{\\an8}Hi", RemoveFormattingUtil.Remove("{\\an8}{\\c&H0000FF&}Hi", RemoveFormattingType.Color));
    }

    [Fact]
    public void FontName_RemovesFaceAttributeAndAssaFn()
    {
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("<font face=\"Arial\">Hi</font>", RemoveFormattingType.FontName));
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("{\\fnArial}Hi", RemoveFormattingType.FontName));
    }

    [Fact]
    public void Alignment_RemovesAssAlignmentTag_KeepsItalic()
    {
        Assert.Equal("<i>Hi</i>", RemoveFormattingUtil.Remove("{\\an8}<i>Hi</i>", RemoveFormattingType.Alignment));
    }

    [Fact]
    public void All_IsWholesale_AlsoRemovesPositionTags()
    {
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("{\\pos(10,20)}<i>Hi</i>", RemoveFormattingType.All));
    }

    [Fact]
    public void AllNamedCategories_LeavePositionTagsAlone()
    {
        // The union of every named category is narrower than All: tags no category
        // covers (like positioning) must survive.
        Assert.Equal("{\\pos(10,20)}Hi", RemoveFormattingUtil.Remove("{\\pos(10,20)}<i>Hi</i>", AllNamed));
    }

    [Fact]
    public void All_WinsOverNamedCategories()
    {
        var both = RemoveFormattingType.All | RemoveFormattingType.Italic;
        Assert.Equal("Hi", RemoveFormattingUtil.Remove("{\\pos(10,20)}<i>Hi</i>", both));
    }

    [Fact]
    public void CombinedCategories_ApplyTogether()
    {
        Assert.Equal(
            "Hi there",
            RemoveFormattingUtil.Remove("<i>Hi</i> <b>there</b>", RemoveFormattingType.Italic | RemoveFormattingType.Bold));
    }

    [Fact]
    public void EmptyText_ReturnsUnchanged()
    {
        Assert.Equal(string.Empty, RemoveFormattingUtil.Remove(string.Empty, RemoveFormattingType.All));
    }
}
