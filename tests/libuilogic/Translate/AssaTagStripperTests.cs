using Nikse.SubtitleEdit.UiLogic.Translate;

namespace LibUiLogicTests.Translate;

public class AssaTagStripperTests
{
    [Fact]
    public void Strip_LeadingBlock_RestoresVerbatim()
    {
        const string input = @"{\bord0\blur0.8\pos(946.5,250.8)\fs54\fax0.13\frz348\fry358\frx4\1c&HFDF9AA&\3c&HFDF9AA&}Overboard";

        var stripped = StrippedLine.Strip(input);

        Assert.Equal("Overboard", stripped.Text);
        Assert.Equal(input, stripped.Restore(stripped.Text));
        Assert.Equal(@"{\bord0\blur0.8\pos(946.5,250.8)\fs54\fax0.13\frz348\fry358\frx4\1c&HFDF9AA&\3c&HFDF9AA&}Overbored", stripped.Restore("Overbored"));
    }

    [Fact]
    public void Strip_LeadingAndTrailingBlocks_WithWhitespace()
    {
        const string input = @"{\an8}{\i1} Hello there {\i0} ";

        var stripped = StrippedLine.Strip(input);

        Assert.Equal("Hello there", stripped.Text);
        Assert.Equal(input, stripped.Restore(stripped.Text));
    }

    [Fact]
    public void Strip_InlineBlock_StaysInText()
    {
        var stripped = StrippedLine.Strip(@"{\an8}He {\i1}really{\i0} said so.");

        Assert.Equal(@"He {\i1}really{\i0} said so.", stripped.Text);
        Assert.Equal(@"{\an8}", stripped.Prefix);
    }

    [Fact]
    public void Strip_DrawingLine_IsEmpty()
    {
        Assert.Equal(string.Empty, StrippedLine.Strip(@"{\p1}m 0 0 l 100 0 100 100{\p0}").Text);
        Assert.Equal(string.Empty, StrippedLine.Strip(@"{\pos(10,10)\p1}m 0 0 l 100 0{\p0}").Text);
    }

    [Fact]
    public void Strip_NoTags_Unchanged()
    {
        var stripped = StrippedLine.Strip("<i>Plain</i> text");

        Assert.Equal("<i>Plain</i> text", stripped.Text);
        Assert.Equal(string.Empty, stripped.Prefix);
        Assert.Equal(string.Empty, stripped.Suffix);
    }

    [Fact]
    public void Strip_CurlyBracesWithoutBackslash_NotATag()
    {
        var stripped = StrippedLine.Strip("{laughs} Hello");

        Assert.Equal("{laughs} Hello", stripped.Text);
    }

    [Fact]
    public void RemoveAllBlocks_RemovesInlineToo()
    {
        Assert.Equal("He really said so.", StrippedLine.RemoveAllBlocks(@"{\an8}He {\i1}really{\i0} said so."));
    }
}
