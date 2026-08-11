using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

// ConvertToText stopped materializing all six regex groups per match (the regex matches every
// single character of the document, so that was six substrings per character) and builds its
// output in a StringBuilder instead of a List<string> + Join. Pin the output against the
// implementation it replaces, over documents that exercise every branch.
public class RichTextToPlainTextEquivalenceTest
{
    public static TheoryData<string> Documents()
    {
        var data = new TheoryData<string>
        {
            // plain text, several paragraphs
            Wrap(Repeat(@"\pard\sa200\sl276\slmult1\f0\fs22\lang9 We should head back.\par ", 20)),

            // escaped braces, backslash and non-breaking space
            Wrap(@"\pard Braces \{ and \} and a backslash \\ and a hard space \~ done.\par "),

            // ignorable destination groups that must be skipped
            Wrap(@"{\*\generator Riched20 10.0.19041}{\info{\author Someone}{\company ACME}}\pard Visible text.\par "),

            // font/color tables (destinations) mixed with text
            Wrap(@"{\fonttbl{\f0\fnil\fcharset0 Calibri;}}{\colortbl ;\red255\green0\blue0;}\pard Colored text.\par "),

            // hex escapes and the \uc / \u unicode skip machinery
            Wrap(@"\pard Caf\'e9 and \u248?\u229?\u230? and \uc0\u9731 snow.\par "),

            // special characters (tabs, dashes, quotes, bullets)
            Wrap(@"\pard A\tab B\emdash C\endash D\lquote E\rquote F\ldblquote G\rdblquote H\bullet I\line J\par "),

            // nested groups
            Wrap(@"{\pard {\b Bold {\i and italic}} back to normal.\par }"),

            // empty body
            Wrap(string.Empty),
        };

        return data;
    }

    [Theory]
    [MemberData(nameof(Documents))]
    public void MatchesTheOriginalImplementation(string rtf)
    {
        Assert.Equal(Reference(rtf), RichTextToPlainText.ConvertToText(rtf));
    }

    [Fact]
    public void RoundTripsRtfWrittenBySubtitleEdit()
    {
        const string text = "Line one with \\ and {braces}\r\nLine two with Café and \u2013 dash.";
        var rtf = RichTextToPlainText.ConvertToRtf(text);
        Assert.Equal(Reference(rtf), RichTextToPlainText.ConvertToText(rtf));
    }

    private static string Repeat(string s, int count)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            sb.Append(s);
        }

        return sb.ToString();
    }

    private static string Wrap(string body) =>
        @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat" + body + "}";

    private static readonly Regex RtfRegex = new Regex(
        @"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)",
        RegexOptions.Singleline | RegexOptions.IgnoreCase);

    /// <summary>The implementation the current one replaces: all six groups read per match,
    /// output collected as a List&lt;string&gt; and joined.</summary>
    private static string Reference(string inputRtf)
    {
        var stack = new Stack<(int UcSkip, bool Ignorable)>();
        var ignorable = false;
        var ucskip = 1;
        var curskip = 0;
        var outList = new List<string>();

        foreach (Match match in RtfRegex.Matches(inputRtf))
        {
            var word = match.Groups[1].Value;
            var arg = match.Groups[2].Value;
            var hex = match.Groups[3].Value;
            var character = match.Groups[4].Value;
            var brace = match.Groups[5].Value;
            var tchar = match.Groups[6].Value;

            if (!string.IsNullOrEmpty(brace))
            {
                curskip = 0;
                if (brace == "{")
                {
                    stack.Push((ucskip, ignorable));
                }
                else if (brace == "}")
                {
                    var entry = stack.Pop();
                    ucskip = entry.UcSkip;
                    ignorable = entry.Ignorable;
                }
            }
            else if (!string.IsNullOrEmpty(character))
            {
                curskip = 0;
                if (character == "~")
                {
                    if (!ignorable)
                    {
                        outList.Add("\xA0");
                    }
                }
                else if ("{}\\".Contains(character))
                {
                    if (!ignorable)
                    {
                        outList.Add(character);
                    }
                }
                else if (character == "*")
                {
                    ignorable = true;
                }
            }
            else if (!string.IsNullOrEmpty(word))
            {
                curskip = 0;
                if (ReferenceDestinations.Contains(word))
                {
                    ignorable = true;
                }
                else if (ignorable)
                {
                }
                else if (ReferenceSpecialCharacters.ContainsKey(word))
                {
                    outList.Add(ReferenceSpecialCharacters[word]);
                }
                else if (word == "uc")
                {
                    ucskip = int.Parse(arg);
                }
                else if (word == "u")
                {
                    var c = int.Parse(arg);
                    if (c < 0)
                    {
                        c += 0x10000;
                    }

                    outList.Add(char.ConvertFromUtf32(c));
                    curskip = ucskip;
                }
            }
            else if (!string.IsNullOrEmpty(hex))
            {
                if (curskip > 0)
                {
                    curskip -= 1;
                }
                else if (!ignorable)
                {
                    var c = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                    outList.Add(char.ConvertFromUtf32(c));
                }
            }
            else if (!string.IsNullOrEmpty(tchar))
            {
                if (curskip > 0)
                {
                    curskip -= 1;
                }
                else if (!ignorable)
                {
                    outList.Add(tchar);
                }
            }
        }

        return string.Join(string.Empty, outList.ToArray());
    }

    // The production tables, read straight out of the class: the reference must differ from the
    // implementation only in the loop shape, never in what it considers a destination or a
    // special character.
    private static readonly HashSet<string> ReferenceDestinations =
        new HashSet<string>((IEnumerable<string>)PrivateStatic("Destinations"));

    private static readonly Dictionary<string, string> ReferenceSpecialCharacters =
        new Dictionary<string, string>((Dictionary<string, string>)PrivateStatic("SpecialCharacters"));

    private static object PrivateStatic(string fieldName)
    {
        var field = typeof(RichTextToPlainText).GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(field);
        var value = field!.GetValue(null);
        Assert.NotNull(value);
        return value!;
    }
}
