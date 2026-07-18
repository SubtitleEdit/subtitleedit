using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class HtmlUtilTest
{
    [Fact]
    public void TestRemoveOpenCloseTagCyrillicI()
    {
        const string source = "<\u0456>SubtitleEdit</\u0456>";
        Assert.Equal("SubtitleEdit", HtmlUtil.RemoveOpenCloseTags(source, HtmlUtil.TagCyrillicI));
    }

    [Fact]
    public void TestRemoveOpenCloseTagFont()
    {
        const string source = "<font color=\"#000\">SubtitleEdit</font>";
        Assert.Equal("SubtitleEdit", HtmlUtil.RemoveOpenCloseTags(source, HtmlUtil.TagFont));
    }

    [Fact]
    public void RemoveHtmlTags1()
    {
        const string source = "<font color=\"#000\"><i>SubtitleEdit</i></font>";
        Assert.Equal("SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
    }

    [Fact]
    public void RemoveHtmlTags2()
    {
        const string source = "<font size=\"12\" color=\"#000\">Hi <font color=\"#000\"><i>SubtitleEdit</i></font></font>";
        Assert.Equal("Hi SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
    }

    [Fact]
    public void RemoveHtmlTags3()
    {
        const string source = "{\\an9}<i>How are you? Are you <font='arial'>happy</font> and 1 < 2</i>";
        Assert.Equal("{\\an9}How are you? Are you happy and 1 < 2", HtmlUtil.RemoveHtmlTags(source));
    }

    [Fact]
    public void RemoveHtmlTagsKeepAss()
    {
        const string source = "{\\an2}<i>SubtitleEdit</i>";
        Assert.Equal("{\\an2}SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
    }

    [Fact]
    public void RemoveHtmlTagsRemoveAss()
    {
        const string source = "{\\an2}<i>SubtitleEdit</i>";
        Assert.Equal("SubtitleEdit", HtmlUtil.RemoveHtmlTags(source, true));
    }

    [Fact]
    public void RemoveHtmlTagsBadItalic()
    {
        const string source = "<i>SubtitleEdit<i/>";
        Assert.Equal("SubtitleEdit", HtmlUtil.RemoveHtmlTags(source));
    }

    [Fact]
    public void RemoveHtmlTagsWebVttCMultiline()
    {
        var source = "<c.yellow>-Qu'est-ce qu'on a ?</c>" + Environment.NewLine + "<c.yellow>-Adrien Dorval, 65 ans.</c>";
        var expected = "-Qu'est-ce qu'on a ?" + Environment.NewLine + "-Adrien Dorval, 65 ans.";
        Assert.Equal(expected, HtmlUtil.RemoveHtmlTags(source, true));
    }

    [Fact]
    public void RemoveHtmlTagsWebVttVMultiline()
    {
        var source = "<v Roger>Hi</v>" + Environment.NewLine + "<v.loud Bob>Bye</v>";
        var expected = "Hi" + Environment.NewLine + "Bye";
        Assert.Equal(expected, HtmlUtil.RemoveHtmlTags(source, true));
    }

    [Fact]
    public void FixInvalidItalicTags1()
    {
        const string s = "<i>foobar<i>?";
        Assert.Equal("<i>foobar</i>?", HtmlUtil.FixInvalidItalicTags(s));
    }

    [Fact]
    public void FixInvalidItalicTags2()
    {
        string s = "<i>foobar?" + Environment.NewLine + "<i>foobar?";
        Assert.Equal("<i>foobar?</i>" + Environment.NewLine + "<i>foobar?</i>", HtmlUtil.FixInvalidItalicTags(s));
    }

    [Fact]
    public void FixInvalidItalicTagsDanglingStartTagAtEnd()
    {
        const string s = "<i>a</i><i>";
        Assert.Equal("<i>a</i>", HtmlUtil.FixInvalidItalicTags(s));
    }

    [Fact]
    public void FixInvalidItalicTagsDanglingStartTagKeepsCharBeforeTag()
    {
        // Used to become "<i>ab</i></i>" - the "c" was eaten and an extra end tag appended
        const string s = "<i>ab</i>c<i>";
        Assert.Equal("<i>ab</i>c", HtmlUtil.FixInvalidItalicTags(s));
    }

    [Fact]
    public void FixInvalidItalicTagsDanglingStartTagKeepsWordBeforeTag()
    {
        // Used to become "<i>Hello</i> </i>" - the "d" was eaten
        const string s = "<i>Hello</i> d<i>";
        Assert.Equal("<i>Hello</i> d", HtmlUtil.FixInvalidItalicTags(s));
    }

    // Two lines, four stray "</i>" tags, no "<i>" - each line with a tag becomes italic.
    // The first six cases pin the long-standing behavior for shapes the old condition matched;
    // the rest used to fall through and be returned with the invalid tags unchanged.
    [Theory]
    [InlineData("</i>Hello</i>|</i>World</i>", "<i>Hello</i>|<i>World</i>")]
    [InlineData("</i>Hello|</i>a</i>World</i>", "<i>Hello</i>|<i>aWorld</i>")]
    [InlineData("</i>Hello</i>x|</i>World</i>", "<i>Hellox</i>|<i>World</i>")]
    [InlineData("<i>Hello<i>|<i>World<i>", "<i>Hello</i>|<i>World</i>")]
    [InlineData("<i>Hello|<i>a<i>World<i>", "<i>Hello</i>|<i>aWorld</i>")]
    [InlineData("<i>Hello<i>x|<i>World<i>", "<i>Hellox</i>|<i>World</i>")]
    [InlineData("</i>Hello</i>a</i>|World</i>", "<i>Helloa</i>|<i>World</i>")]
    [InlineData("a</i>b</i>|c</i>d</i>", "<i>ab</i>|<i>cd</i>")]
    [InlineData("</i>Hello</i>b</i>c</i>|plain", "<i>Hellobc</i>|plain")]
    [InlineData("<i>Hello<i>a<i>|World<i>", "<i>Helloa</i>|<i>World</i>")]
    [InlineData("a<i>b<i>|c<i>d<i>", "<i>ab</i>|<i>cd</i>")]
    [InlineData("<i>Hello<i>b<i>c<i>|plain", "<i>Hellobc</i>|plain")]
    public void FixInvalidItalicTagsTwoLinesOnlyStrayTags(string input, string expected)
    {
        var s = input.Replace("|", Environment.NewLine);
        Assert.Equal(expected.Replace("|", Environment.NewLine), HtmlUtil.FixInvalidItalicTags(s));
    }

    [Fact]
    public void EncodeNumericEncodesNonAsciiBmpChar()
    {
        Assert.Equal("Hi&#229;", HtmlUtil.EncodeNumeric("Hiå")); // å
    }

    [Fact]
    public void EncodeNumericLeavesAsciiUntouched()
    {
        Assert.Equal("abc", HtmlUtil.EncodeNumeric("abc"));
    }

    [Fact]
    public void EncodeNumericEncodesSurrogatePairAsSingleCodePoint()
    {
        // U+1F600 (😀) is a surrogate pair in UTF-16; it must encode as one
        // code point reference, not two lone-surrogate references.
        Assert.Equal("&#128512;", HtmlUtil.EncodeNumeric("😀"));
    }

}
