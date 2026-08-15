using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Tests.Common;

/// <summary>
/// The fast paths added when these methods were made to skip work they cannot need must agree
/// with the full path in exactly the cases the guard decides. Each test below sits on one such
/// guard - a character class that has to be spotted, or one that must not trigger a rewrite.
/// </summary>
public class HotPathFastPathTest
{
    // --- CalcAll: the vectorized "is this simple text" probe and the control-character count ---

    [Theory]
    [InlineData("Hello.", 6)]
    [InlineData("", 0)]
    [InlineData("a\r\nb", 2)]                       // CR and LF are controls, not counted
    [InlineData("a\tb", 2)]                         // tab is a control too
    [InlineData("a\u0000b", 2)]                     // NUL, low end of the first control range
    [InlineData("a\u001Fb", 2)]                     // US, high end of the first control range
    [InlineData("a\u007Fb", 2)]                     // DEL, low end of the second control range
    [InlineData("a\u009Fb", 2)]                     // APC, high end of the second control range
    [InlineData("a\u0020b", 3)]                     // space is not a control
    [InlineData("a\u00A0b", 3)]                     // no-break space is not a control
    [InlineData("\u2010\u2027", 2)]                 // the punctuation window that stays "simple"
    [InlineData("\u200F\u202E", 0)]                 // BiDi marks are skipped
    [InlineData("\u200F\u2028", 1)]                 // ... but U+2028 is neither a control nor skipped
    [InlineData("e\u0301", 1)]                      // combining acute joins the previous letter
    [InlineData("\u0300", 1)]                       // a lone combining mark is one element
    [InlineData("\uD83D\uDE00", 1)]                 // surrogate pair is one element
    [InlineData("<i>ab</i>", 2)]                    // tags are stripped before counting
    public void CalcAllCountsTheSameOnBothPaths(string text, int expected)
    {
        Assert.Equal(expected, new CalcAll().CountCharacters(text, false));
    }

    // --- Json.EncodeJsonText: the "nothing to escape" fast path ---

    [Theory]
    [InlineData("Hello.")]
    [InlineData("")]
    [InlineData("no escapes here \u00E6\u00F8\u00E5 \u4F60\u597D")]
    [InlineData("quote \" inside")]
    [InlineData("backslash \\ inside")]
    [InlineData("line\nbreak")]
    [InlineData("carriage\rreturn")]
    [InlineData("windows\r\nbreak")]
    [InlineData("tab\there")]
    [InlineData("bell\u0007here")]
    [InlineData("formfeed\u000Chere")]
    [InlineData("backspace\u0008here")]
    [InlineData("unit\u001Fseparator")]
    public void EncodeJsonTextMatchesTheFullWalk(string text)
    {
        var expected = EncodeJsonTextReference(text);

        Assert.Equal(expected, Json.EncodeJsonText(text));
    }

    [Fact]
    public void EncodeJsonTextLeavesCleanTextAlone()
    {
        const string clean = "It was the best of times.";

        Assert.Same(clean, Json.EncodeJsonText(clean));
    }

    // The per-character walk Json.EncodeJsonText used to run unconditionally, kept here as the
    // oracle the fast path is checked against.
    private static string EncodeJsonTextReference(string text, string newLineCharacter = "<br />")
    {
        text = text.Replace("\r\n", "\n").Replace('\r', '\n');

        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            switch (c)
            {
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\n':
                    sb.Append(newLineCharacter);
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                default:
                    if (c < ' ')
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        return sb.ToString();
    }

    // --- RemoveUnneededSpaces: the three guards that skip a rewrite the line cannot need ---

    [Theory]
    [InlineData("tab\there", "tab here")]                       // tab becomes a space
    [InlineData("nbsp\u00A0here", "nbsp here")]                  // no-break space becomes a space
    [InlineData("zero\u200Bwidth", "zerowidth")]                 // zero-width space is dropped
    [InlineData("bom\uFEFFhere", "bomhere")]                     // zero-width no-break space is dropped
    [InlineData("osc\u009Dhere", "oschere")]                     // operating system command is dropped
    [InlineData("nothing to normalize", "nothing to normalize")]
    public void RemoveUnneededSpacesStillNormalizes(string input, string expected)
    {
        Assert.Equal(expected, Utilities.RemoveUnneededSpaces(input, "en"));
    }

    [Theory]
    [InlineData("Well. . .. what now", "Well... what now")]
    [InlineData("Well. ... what now", "Well... what now")]
    [InlineData("Well. .. . what now", "Well... what now")]
    [InlineData("Well. . . what now", "Well... what now")]
    [InlineData("Well. .. what now", "Well... what now")]
    [InlineData("Well.. . what now", "Well... what now")]
    [InlineData("Well.... what now", "Well... what now")]
    public void RemoveUnneededSpacesStillCollapsesSpacedEllipses(string input, string expected)
    {
        Assert.Equal(expected, Utilities.RemoveUnneededSpaces(input, "en"));
    }

    [Fact]
    public void RemoveUnneededSpacesStillFixesEllipsisAroundLineBreaks()
    {
        var input = "Hello ..." + System.Environment.NewLine + "... world";
        var expected = "Hello..." + System.Environment.NewLine + "...world";

        Assert.Equal(expected, Utilities.RemoveUnneededSpaces(input, "en"));
    }

    // --- StrippableText: the two extra characters that used to be concatenated onto the set ---

    [Theory]
    [InlineData("<i>Hello</i>", "<i>", "Hello", "</i>")]
    [InlineData("{" + "\\" + "an8}Hello", "{" + "\\" + "an8}", "Hello", "")]
    [InlineData("Hello", "", "Hello", "")]
    [InlineData("- Hello.", "- ", "Hello", ".")]
    public void StrippableTextStillStripsTags(string input, string pre, string stripped, string post)
    {
        var st = new StrippableText(input);

        Assert.Equal(pre, st.Pre);
        Assert.Equal(stripped, st.StrippedText);
        Assert.Equal(post, st.Post);
    }
}
