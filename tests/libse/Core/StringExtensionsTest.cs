using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Core;

public class StringExtensionsTest
{

    [Fact]
    public void LineStartsWithHtmlTagEmpty()
    {
        var test = string.Empty;
        Assert.False(test.LineStartsWithHtmlTag(true));
    }

    [Fact]
    public void LineStartsWithHtmlTagNull()
    {
        string? test = null;
        Assert.False(test.LineStartsWithHtmlTag(true));
    }

    [Fact]
    public void LineStartsWithHtmlTagItalic()
    {
        const string test = "<i>";
        Assert.True(test.LineStartsWithHtmlTag(true));
    }

    [Fact]
    public void EndsWithEmpty()
    {
        var test = string.Empty;
        Assert.False(test.EndsWith('?'));
    }

    [Fact]
    public void EndsWithHtmlTagEmpty()
    {
        var test = string.Empty;
        Assert.False(test.LineEndsWithHtmlTag(true));
    }

    [Fact]
    public void EndsWithHtmlTagItalic()
    {
        const string test = "<i>Hej</i>";
        Assert.True(test.LineEndsWithHtmlTag(true));
    }

    [Fact]
    public void LineBreakStartsWithHtmlTagEmpty()
    {
        var test = string.Empty;
        Assert.False(test.LineBreakStartsWithHtmlTag(true));
    }

    [Fact]
    public void LineBreakStartsWithHtmlTagItalic()
    {
        var test = "<i>Hej</i>" + Environment.NewLine + "<i>Hej</i>";
        Assert.True(test.LineBreakStartsWithHtmlTag(true));
    }

    [Fact]
    public void LineBreakStartsWithHtmlTagFont()
    {
        var test = "Hej!" + Environment.NewLine + "<font color=FFFFFF>Hej!</font>";
        Assert.True(test.LineBreakStartsWithHtmlTag(true, true));
    }

    //QUESTION: fix three lines?
    //[Fact]
    //public void LineBreakStartsWithHtmlTagFontThreeLines()
    //{
    //    string test = "Hej!" + Environment.NewLine + "Hej!" + Environment.NewLine + "<font color=FFFFFF>Hej!</font>";
    //    Assert.True(test.LineBreakStartsWithHtmlTag(true, true));
    //}

    [Fact]
    public void LineBreakStartsWithHtmlTagFontFalse()
    {
        const string test = "<font color=FFFFFF>Hej!</font>";
        Assert.False(test.LineBreakStartsWithHtmlTag(true, true));
    }

    [Fact]
    public void SplitToLines1()
    {
        var input = "Line1" + Environment.NewLine + "Line2";
        Assert.Equal(2, input.SplitToLines().Count);
    }

    [Fact]
    public void SplitToLinesEmptyLines1()
    {
        var input = "\n\nLine3\r\n\r\nLine5\r";
        var res = input.SplitToLines();
        Assert.Equal(6, res.Count);
        Assert.Equal(string.Empty, res[0]);
        Assert.Equal(string.Empty, res[1]);
        Assert.Equal("Line3", res[2]);
        Assert.Equal(string.Empty, res[3]);
        Assert.Equal("Line5", res[4]);
        Assert.Equal(string.Empty, res[5]);
    }

    [Fact]
    public void SplitToLines0A0A0D()
    {
        var input = "a\n\n\rb";
        var res = input.SplitToLines();
        Assert.Equal(4, res.Count);
        Assert.Equal("a", res[0]);
        Assert.Equal(string.Empty, res[1]);
        Assert.Equal("b", res[3]);
    }

    [Fact]
    public void SplitToLines650D0D0A650A0A650A650D650D0A650A0D65()
    {
        var input = "e\r\r\ne\n\ne\ne\re\r\ne\n\re";
        var res = input.SplitToLines();
        Assert.Equal(10, res.Count);
        Assert.Equal("e", res[0]);
        Assert.Equal(string.Empty, res[1]);
        Assert.Equal("e", res[2]);
        Assert.Equal(string.Empty, res[3]);
        Assert.Equal("e", res[4]);
        Assert.Equal("e", res[5]);
        Assert.Equal("e", res[6]);
        Assert.Equal("e", res[7]);
        Assert.Equal(string.Empty, res[8]);
        Assert.Equal("e", res[9]);
    }

    [Fact]
    public void FixExtraSpaces()
    {
        var input = "Hallo  world!";
        var res = input.FixExtraSpaces();
        Assert.Equal("Hallo world!", res);
    }

    [Fact]
    public void FixExtraSpaces2()
    {
        var input = "Hallo   world!";
        var res = input.FixExtraSpaces();
        Assert.Equal("Hallo world!", res);
    }

    [Fact]
    public void FixExtraSpaces3()
    {
        var input = "Hallo world!  ";
        var res = input.FixExtraSpaces();
        Assert.Equal("Hallo world! ", res);
    }

    [Fact]
    public void FixExtraSpaces4()
    {
        var input = "Hallo " + Environment.NewLine + " world!";
        var res = input.FixExtraSpaces();
        Assert.Equal("Hallo" + Environment.NewLine + "world!", res);
    }


    [Fact]
    public void FixExtraSpaces5()
    {
        var input = "a  " + Environment.NewLine + "b";
        var res = input.FixExtraSpaces();
        Assert.Equal("a" + Environment.NewLine + "b", res);
    }

    [Fact]
    public void FixExtraSpaces6()
    {
        var input = "a" + Environment.NewLine + "   b";
        var res = input.FixExtraSpaces();
        Assert.Equal("a" + Environment.NewLine + "b", res);
    }

    [Fact]
    public void RemoveRecursiveLineBreakTest()
    {
        Assert.Equal("foo\r\nfoo", "foo\r\n\r\nfoo".RemoveRecursiveLineBreaks());
        Assert.Equal("foo\r\nfoo", "foo\r\nfoo".RemoveRecursiveLineBreaks());
        Assert.Equal("foo\r\nfoo", "foo\r\n\r\n\r\nfoo".RemoveRecursiveLineBreaks());
    }

    [Fact]
    public void RemoveRecursiveLineBreakNonWindowsStyleTest()
    {
        Assert.Equal("foo\nfoo", "foo\nfoo".RemoveRecursiveLineBreaks());
        Assert.Equal("foo\nfoo", "foo\n\n\nfoo".RemoveRecursiveLineBreaks());
        Assert.Equal("foo\n.\nfoo", "foo\n.\n\n\nfoo".RemoveRecursiveLineBreaks());
    }

    [Fact]
    public void RemoveChar1()
    {
        var input = "Hallo world!";
        var res = input.RemoveChar(' ');
        Assert.Equal("Halloworld!", res);
    }

    [Fact]
    public void RemoveChar2()
    {
        var input = " Hallo  world! ";
        var res = input.RemoveChar(' ');
        Assert.Equal("Halloworld!", res);
    }

    [Fact]
    public void RemoveChar3()
    {
        var input = " Hallo  world! ";
        var res = input.RemoveChar(' ', '!');
        Assert.Equal("Halloworld", res);
    }

    [Fact]
    public void RemoveChar4()
    {
        var input = " Hallo  world! ";
        var res = input.RemoveChar(' ', '!', 'H');
        Assert.Equal("alloworld", res);
    }

    [Fact]
    public void CountLetters1()
    {
        var input = " Hallo  world! ";
        var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
        Assert.Equal(" Hallo  world! ".Length, res);
    }

    [Fact]
    public void CountLetters2()
    {
        var input = " Hallo " + Environment.NewLine + " world! ";
        var res = CalcFactory.MakeCalculator(nameof(CalcNoSpace)).CountCharacters(input, false);
        Assert.Equal("Halloworld!".Length, res);
    }

    [Fact]
    public void CountLetters3()
    {
        var input = " Hallo" + Environment.NewLine + "world!";
        var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
        Assert.Equal(" Halloworld!".Length, res);
    }

    [Fact]
    public void CountLetters4Ssa()
    {
        var input = "{\\an1}Hallo";
        var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
        Assert.Equal("Hallo".Length, res);
    }

    [Fact]
    public void CountLetters4Html()
    {
        var input = "<i>Hallo</i>";
        var res = CalcFactory.MakeCalculator(nameof(CalcAll)).CountCharacters(input, false);
        Assert.Equal("Hallo".Length, res);
    }

    [Fact]
    public void CountLetters5HtmlFont()
    {
        var input = "<font color=\"red\"><i>Hal lo<i></font>";
        var res = CalcFactory.MakeCalculator(nameof(CalcNoSpace)).CountCharacters(input, false);
        Assert.Equal("Hallo".Length, res);
    }

    [Fact]
    public void CountLetters6HtmlFontMultiLine()
    {
        var input = "<font color=\"red\"><i>Hal lo<i></font>" + Environment.NewLine + "<i>Bye!</i>";
        var res = CalcFactory.MakeCalculator(nameof(CalcNoSpace)).CountCharacters(input, false);
        Assert.Equal("HalloBye!".Length, res);
    }

    [Fact]
    public void ToggleCasing1()
    {
        var input = "how are you";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("How Are You", res);
    }

    [Fact]
    public void ToggleCasing1WithItalic()
    {
        var input = "how <i>are</i> you";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("How <i>Are</i> You", res);
    }

    [Fact]
    public void ToggleCasing1WithItalicStart()
    {
        var input = "<i>how</i> are you";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("<i>How</i> Are You", res);
    }

    [Fact]
    public void ToggleCasing1WithItalicEnd()
    {
        var input = "how are <i>you</i>";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("How Are <i>You</i>", res);
    }

    [Fact]
    public void ToggleCasing1WithItalicEndAndBold()
    {
        var input = "how are <i><b>you</b></i>";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("How Are <i><b>You</b></i>", res);
    }

    [Fact]
    public void ToggleCasing2()
    {
        var input = "How Are You";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("HOW ARE YOU", res);
    }

    [Fact]
    public void ToggleCasing3()
    {
        var input = "HOW ARE YOU";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("how are you", res);
    }

    [Fact]
    public void ToggleCasingWithFont()
    {
        var input = "<font color=\"Red\">HOW ARE YOU</font>";
        var res = input.ToggleCasing(new SubRip());
        Assert.Equal("<font color=\"Red\">how are you</font>", res);
    }

    [Fact]
    public void ToggleCasingAssa()
    {
        var input = "{\\i1}This is an example…{\\i0}";
        var res = input.ToggleCasing(new AdvancedSubStationAlpha());
        Assert.Equal("{\\i1}THIS IS AN EXAMPLE…{\\i0}", res);
    }

    [Fact]
    public void ToggleCasingAssaSoftLineBreak()
    {
        var input = "HOW ARE\\nYOU?";
        var res = input.ToggleCasing(new AdvancedSubStationAlpha());
        Assert.Equal("how are\\nyou?", res);
    }

    [Fact]
    public void ToggleCasingVoiceTag()
    {
        var input = "<v Johnny>How are you?";
        var res = input.ToggleCasing(null);
        Assert.Equal("<v Johnny>HOW ARE YOU?", res);
    }

    [Fact]
    public void ToProperCaseFromUpper()
    {
        var input = "HOW ARE YOU?";
        var res = input.ToProperCase(null);
        Assert.Equal("How Are You?", res);
    }

    [Fact]
    public void ToProperCaseFromLower()
    {
        var input = "how are you?";
        var res = input.ToProperCase(null);
        Assert.Equal("How Are You?", res);
    }

    [Fact]
    public void ToProperCaseItalic()
    {
        var input = "<i>HOW ARE YOU?</i>";
        var res = input.ToProperCase(null);
        Assert.Equal("<i>How Are You?</i>", res);
    }

    [Fact]
    public void ToLowercaseButKeepTags1()
    {
        var input = "<i>HOW ARE YOU?</i>";
        var res = input.ToLowercaseButKeepTags();
        Assert.Equal("<i>how are you?</i>", res);
    }

    [Fact]
    public void ToLowercaseButKeepTags2()
    {
        var input = "{\\c&H0000FF&}Red";
        var res = input.ToLowercaseButKeepTags();
        Assert.Equal("{\\c&H0000FF&}red", res);
    }

    [Fact]
    public void HasSentenceEndingCultureNeutralTest()
    {
        // language two letter language set to null
        Assert.True("foobar.".HasSentenceEnding(null)); // this is supposed to use the culture neutral chars

        Assert.True("foobar.".HasSentenceEnding());
        Assert.True("foobar?</font>".HasSentenceEnding());
        Assert.True("foobar!</font>".HasSentenceEnding());
        Assert.True("foobar.</font>\"".HasSentenceEnding());
        Assert.True("{\\i1}How are you?{\\i0}".HasSentenceEnding());
        Assert.True("{\\i1}How are you?{\\i0}</font>".HasSentenceEnding());
        Assert.True("{\\i1}How are you?</font>{\\i0}".HasSentenceEnding());
        Assert.True("foobar.\"".HasSentenceEnding());
        Assert.True("foobar--".HasSentenceEnding());
        Assert.True("foobar--</i>".HasSentenceEnding());
        Assert.True("foobar—".HasSentenceEnding()); // em dash
        Assert.True("foobar—</i>".HasSentenceEnding()); // em dash

        Assert.False("\"".HasSentenceEnding());
        Assert.False("foobar>".HasSentenceEnding());
        Assert.False("How are you{\\i0}".HasSentenceEnding());
        Assert.False("".HasSentenceEnding());
    }

    [Fact]
    public void HasSentenceEndingGreekTest()
    {
        const string greekCultureTwoLetter = "el";
        Assert.True("foobar)".HasSentenceEnding(greekCultureTwoLetter));
        Assert.True("foobar\u037E</font>\"".HasSentenceEnding(greekCultureTwoLetter));
        Assert.True("foobar؟\"".HasSentenceEnding(greekCultureTwoLetter));
        Assert.True("foobar;".HasSentenceEnding(greekCultureTwoLetter));
    }

}
