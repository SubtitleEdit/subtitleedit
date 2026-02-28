using Nikse.SubtitleEdit.Core.Forms;

namespace LibSETests.Forms;

public class MoveWordUpDownTest
{
    [Fact]
    public void MoveWordUp_SimpleText()
    {
        var mover = new MoveWordUpDown("Hello", "world today");
        mover.MoveWordUp();

        Assert.Equal("Hello world", mover.S1);
        Assert.Equal("today", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithMultipleWords()
    {
        var mover = new MoveWordUpDown("First line", "second line here");
        mover.MoveWordUp();

        Assert.Equal("First line second", mover.S1);
        Assert.Equal("line here", mover.S2);
    }

    [Fact]
    public void MoveWordUp_EmptyS2()
    {
        var mover = new MoveWordUpDown("Hello", "");
        mover.MoveWordUp();

        Assert.Equal("Hello", mover.S1);
        Assert.Equal("", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithItalicTag()
    {
        var mover = new MoveWordUpDown("Hello", "<i>world today</i>");
        mover.MoveWordUp();

        Assert.Equal("Hello <i>world</i>", mover.S1);
        Assert.Equal("<i>today</i>", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithItalicTag2()
    {
        var mover = new MoveWordUpDown("Hello <i>world</i>", "<i>today</i>");
        mover.MoveWordUp();

        Assert.Equal("Hello <i>world today</i>", mover.S1);
        Assert.Equal("", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithItalicTag3()
    {
        var mover = new MoveWordUpDown("Hello, how are <i>the world</i>", "today");
        mover.MoveWordDown();

        Assert.Equal("Hello, how are <i>the</i>", mover.S1);
        Assert.Equal("<i>world</i> today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithItalicTag_Merge_Tags()
    {
        var mover = new MoveWordUpDown("Hello, how are <i>the</i>", "<i>world</i> today");
        mover.MoveWordDown();

        Assert.Equal("Hello, how are", mover.S1);
        Assert.Equal("<i>the world</i> today", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithBoldTag()
    {
        var mover = new MoveWordUpDown("Hello", "<b>world today</b>");
        mover.MoveWordUp();

        Assert.Equal("Hello <b>world</b>", mover.S1);
        Assert.Equal("<b>today</b>", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithUnderlineTag()
    {
        var mover = new MoveWordUpDown("Hello", "<u>world today</u>");
        mover.MoveWordUp();

        Assert.Equal("Hello <u>world</u>", mover.S1);
        Assert.Equal("<u>today</u>", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithFontTag()
    {
        var mover = new MoveWordUpDown("Hello", "<font color='red'>world today</font>");
        mover.MoveWordUp();

        Assert.Equal("Hello <font color='red'>world</font>", mover.S1);
        Assert.Equal("<font color='red'>today</font>", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithAssTag()
    {
        var mover = new MoveWordUpDown("Hello", "{\\i1}world today{\\i0}");
        mover.MoveWordUp();

        Assert.Equal("Hello {\\i1}world{\\i0}", mover.S1);
        Assert.Equal("{\\i1}today{\\i0}", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithAssTagAtStart()
    {
        var mover = new MoveWordUpDown("First", "{\\an8}Second word");
        mover.MoveWordUp();

        Assert.Equal("First {\\an8}Second", mover.S1);
        Assert.Equal("{\\an8}word", mover.S2);
    }

    [Fact]
    public void MoveWordUp_SingleWordInS2()
    {
        var mover = new MoveWordUpDown("Hello", "world");
        mover.MoveWordUp();

        Assert.Equal("Hello world", mover.S1);
        Assert.Equal("", mover.S2);
    }

    [Fact]
    public void MoveWordUp_RemovesEmptyTags()
    {
        var mover = new MoveWordUpDown("Hello", "<i>world</i>");
        mover.MoveWordUp();

        Assert.Equal("Hello <i>world</i>", mover.S1);
        Assert.Equal("", mover.S2);
    }

    [Fact]
    public void MoveWordDown_SimpleText()
    {
        var mover = new MoveWordUpDown("Hello world", "today");
        mover.MoveWordDown();

        Assert.Equal("Hello", mover.S1);
        Assert.Equal("world today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithMultipleWords()
    {
        var mover = new MoveWordUpDown("First line here", "second line");
        mover.MoveWordDown();

        Assert.Equal("First line", mover.S1);
        Assert.Equal("here second line", mover.S2);
    }

    [Fact]
    public void MoveWordDown_EmptyS1()
    {
        var mover = new MoveWordUpDown("", "Hello");
        mover.MoveWordDown();

        Assert.Equal("", mover.S1);
        Assert.Equal("Hello", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithItalicTag()
    {
        var mover = new MoveWordUpDown("<i>Hello world</i>", "today");
        mover.MoveWordDown();

        Assert.Equal("<i>Hello</i>", mover.S1);
        Assert.Equal("<i>world</i> today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithBoldTag()
    {
        var mover = new MoveWordUpDown("<b>Hello world</b>", "today");
        mover.MoveWordDown();

        Assert.Equal("<b>Hello</b>", mover.S1);
        Assert.Equal("<b>world</b> today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithUnderlineTag()
    {
        var mover = new MoveWordUpDown("<u>Hello world</u>", "today");
        mover.MoveWordDown();

        Assert.Equal("<u>Hello</u>", mover.S1);
        Assert.Equal("<u>world</u> today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithFontTag()
    {
        var mover = new MoveWordUpDown("<font color='red'>Hello world</font>", "today");
        mover.MoveWordDown();

        Assert.Equal("<font color='red'>Hello</font>", mover.S1);
        Assert.Equal("<font color='red'>world</font> today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithAssTag()
    {
        var mover = new MoveWordUpDown("{\\i1}Hello world{\\i0}", "today");
        mover.MoveWordDown();

        Assert.Equal("{\\i1}Hello{\\i0}", mover.S1);
        Assert.Equal("{\\i1}world{\\i0} today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithAssTagAtStart()
    {
        var mover = new MoveWordUpDown("{\\an8}Hello world", "today");
        mover.MoveWordDown();

        Assert.Equal("{\\an8}Hello", mover.S1);
        Assert.Equal("{\\an8}world today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_SingleWordInS1()
    {
        var mover = new MoveWordUpDown("Hello", "world");
        mover.MoveWordDown();

        Assert.Equal("", mover.S1);
        Assert.Equal("Hello world", mover.S2);
    }

    [Fact]
    public void MoveWordDown_RemovesEmptyTags()
    {
        var mover = new MoveWordUpDown("<i>Hello</i>", "world");
        mover.MoveWordDown();

        Assert.Equal("", mover.S1);
        Assert.Equal("<i>Hello</i> world", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithSpaces()
    {
        var mover = new MoveWordUpDown("Hello", "  world  today  ");
        mover.MoveWordUp();

        Assert.Equal("Hello world", mover.S1);
        Assert.Equal("today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithSpaces()
    {
        var mover = new MoveWordUpDown("  Hello  world  ", "today");
        mover.MoveWordDown();

        Assert.Equal("Hello", mover.S1);
        Assert.Equal("world today", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithNestedTags()
    {
        var mover = new MoveWordUpDown("Hello", "<i><b>world today</b></i>");
        mover.MoveWordUp();

        Assert.Equal("Hello <i><b>world</i></b>", mover.S1);
        Assert.Equal("<i><b>today</b></i>", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithNestedTags()
    {
        var mover = new MoveWordUpDown("<i><b>Hello world</b></i>", "today");
        mover.MoveWordDown();

        Assert.Equal("<i><b>Hello</b></i>", mover.S1);
        Assert.Equal("<i><b>world</b></i> today", mover.S2);
    }

    [Fact]
    public void MoveWordUp_PreservesAssTagStructure()
    {
        var mover = new MoveWordUpDown("Test", "{\\an8}{\\i1}word one{\\i0}");
        mover.MoveWordUp();

        Assert.Equal("Test {\\an8}{\\i1}word{\\i0}", mover.S1);
        Assert.Equal("{\\an8}{\\i1}one{\\i0}", mover.S2);
    }

    [Fact]
    public void MoveWordDown_PreservesAssTagStructure()
    {
        var mover = new MoveWordUpDown("{\\an8}{\\i1}word one{\\i0}", "Test");
        mover.MoveWordDown();

        Assert.Equal("{\\an8}{\\i1}word{\\i0}", mover.S1);
        Assert.Equal("{\\an8}{\\i1}one{\\i0} Test", mover.S2);
    }

    [Fact]
    public void MoveWordUp_MixedHtmlAndAssTag()
    {
        var mover = new MoveWordUpDown("Hello", "{\\an8}<i>world today</i>");
        mover.MoveWordUp();

        Assert.Equal("Hello {\\an8}<i>world</i>", mover.S1);
        Assert.Equal("{\\an8}<i>today</i>", mover.S2);
    }

    [Fact]
    public void MoveWordDown_MixedHtmlAndAssTag()
    {
        var mover = new MoveWordUpDown("{\\an8}<i>Hello world</i>", "today");
        mover.MoveWordDown();

        Assert.Equal("{\\an8}<i>Hello</i>", mover.S1);
        Assert.Equal("{\\an8}<i>world</i> today", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithLineBreaks()
    {
        var mover = new MoveWordUpDown("First\nline", "second\npart");
        mover.MoveWordUp();

        Assert.Equal("First\nline second", mover.S1);
        Assert.Equal("part", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithLineBreaks()
    {
        var mover = new MoveWordUpDown("First\nline", "second\npart");
        mover.MoveWordDown();

        Assert.Equal("First", mover.S1);
        Assert.Equal("line second\npart", mover.S2);
    }

    [Fact]
    public void MoveWordUp_NullS1()
    {
        var mover = new MoveWordUpDown(null, "world today");
        mover.MoveWordUp();

        Assert.Equal("world", mover.S1);
        Assert.Equal("today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_NullS1()
    {
        var mover = new MoveWordUpDown(null, "world");
        mover.MoveWordDown();

        Assert.Equal("", mover.S1);
        Assert.Equal("world", mover.S2);
    }

    [Fact]
    public void MoveWordUp_OnlyTagsInS2()
    {
        var mover = new MoveWordUpDown("Hello", "<i></i>");
        mover.MoveWordUp();

        Assert.Equal("Hello", mover.S1);
        Assert.Equal("<i></i>", mover.S2);
    }

    [Fact]
    public void MoveWordDown_OnlyTagsInS1()
    {
        var mover = new MoveWordUpDown("<i></i>", "world");
        mover.MoveWordDown();

        Assert.Equal("<i></i>", mover.S1);
        Assert.Equal("world", mover.S2);
    }

    [Fact]
    public void MoveWordUp_ComplexAssTag()
    {
        var mover = new MoveWordUpDown("Test", "{\\pos(100,200)\\c&H00FFFF&}Hello world");
        mover.MoveWordUp();

        Assert.Equal("Test {\\pos(100,200)\\c&H00FFFF&}Hello", mover.S1);
        Assert.Equal("{\\pos(100,200)\\c&H00FFFF&}world", mover.S2);
    }

    [Fact]
    public void MoveWordDown_ComplexAssTag()
    {
        var mover = new MoveWordUpDown("{\\pos(100,200)\\c&H00FFFF&}Hello world", "Test");
        mover.MoveWordDown();

        Assert.Equal("{\\pos(100,200)\\c&H00FFFF&}Hello", mover.S1);
        Assert.Equal("{\\pos(100,200)\\c&H00FFFF&}world Test", mover.S2);
    }

    [Fact]
    public void MoveWordUp_WithClosingItalicInS1()
    {
        var mover = new MoveWordUpDown("Hello <i>test</i>", "world today");
        mover.MoveWordUp();

        Assert.Equal("Hello <i>test</i> world", mover.S1);
        Assert.Equal("today", mover.S2);
    }

    [Fact]
    public void MoveWordDown_WithOpeningItalicInS2()
    {
        var mover = new MoveWordUpDown("Hello world", "<i>today test</i>");
        mover.MoveWordDown();

        Assert.Equal("Hello", mover.S1);
        Assert.Equal("world <i>today test</i>", mover.S2);
    }

    [Fact]
    public void MoveWordUp_MultiLineInS1()
    {
        var mover = new MoveWordUpDown("<i>First</i>\n<i>Second</i>", "third word");
        mover.MoveWordUp();

        Assert.Equal("<i>First</i>\n<i>Second</i> third", mover.S1);
        Assert.Equal("word", mover.S2);
    }

    //TODO: fix
    //[Fact]
    //public void MoveWordDown_MultiLineInS1()
    //{
    //    var mover = new MoveWordUpDown("<i>First</i>\n<i>Second word</i>", "third");
    //    mover.MoveWordDown();

    //    Assert.Equal("<i>First</i> <i>Second</i>", mover.S1);
    //    Assert.Equal("<i>word</i> third", mover.S2);
    //}

    [Fact]
    public void MoveWordUp_PartiallyTaggedContent()
    {
        var mover = new MoveWordUpDown("He knows much", "<i>of anything</i> anymore.");
        mover.MoveWordUp();

        Assert.Equal("He knows much <i>of</i>", mover.S1);
        Assert.Equal("<i>anything</i> anymore.", mover.S2);
    }
}
