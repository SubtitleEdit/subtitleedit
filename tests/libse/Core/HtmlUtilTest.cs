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

}
