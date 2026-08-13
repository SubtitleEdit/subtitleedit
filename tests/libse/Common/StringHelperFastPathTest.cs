using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Tests.Common;

/// <summary>
/// Each fast path added here replaced code that produced the same answer the slow way. The tests
/// below keep a copy of that slow way as the reference and fuzz the two against each other, so a
/// guard that is too eager (or an equivalence that only holds for the examples someone thought
/// of) fails here rather than in a subtitle.
/// </summary>
public class StringHelperFastPathTest
{
    private static readonly char[] Alphabet =
        " \t\r\n0123456789.:,-></abcXYZ\u00A0\u2028\u3000\u266A\u266B\u2014?!*$\u00A3'\"{}\\an1x".ToCharArray();

    private static IEnumerable<string> Fuzz(int count, int maxLength = 24, int seed = 20260813)
    {
        var random = new Random(seed);
        var buffer = new char[maxLength];
        for (var n = 0; n < count; n++)
        {
            var length = random.Next(0, maxLength + 1);
            for (var i = 0; i < length; i++)
            {
                buffer[i] = Alphabet[random.Next(Alphabet.Length)];
            }

            yield return new string(buffer, 0, length);
        }
    }

    // ---------------------------------------------------------------- IsOnlyChars(OrWhiteSpace)

    private static readonly char[] TimeCodeChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ':', ',' };
    private static readonly char[] MusicChars = { '\u266A', '\u266B' };
    private static readonly char[] InterjectionChars = { '.', '?', '!', '-', '\u2014' };

    private static readonly CharLookup TimeCodeLookup = CharLookup.Create(TimeCodeChars);
    private static readonly CharLookup MusicLookup = CharLookup.Create(MusicChars);
    private static readonly CharLookup InterjectionLookup = CharLookup.Create(InterjectionChars);

    [Fact]
    public void IsOnlyCharsOrWhiteSpaceMatchesRemoveCharThenIsNullOrWhiteSpace()
    {
        var sets = new[]
        {
            (TimeCodeChars, TimeCodeLookup),
            (MusicChars, MusicLookup),
            (InterjectionChars, InterjectionLookup),
        };

        foreach (var input in Fuzz(20000))
        {
            foreach (var (chars, lookup) in sets)
            {
                var expected = string.IsNullOrWhiteSpace(input.RemoveChar(chars));
                Assert.Equal(expected, input.IsOnlyCharsOrWhiteSpace(lookup));
            }
        }
    }

    [Fact]
    public void IsOnlyCharsMatchesRemoveCharThenIsNullOrEmpty()
    {
        var lookup = CharLookup.Create(StringExtensions.UnicodeControlChars);
        foreach (var input in Fuzz(20000))
        {
            var expected = string.IsNullOrEmpty(input.RemoveChar(StringExtensions.UnicodeControlChars));
            Assert.Equal(expected, input.IsOnlyChars(lookup));
        }
    }

    [Fact]
    public void IsOnlyCharsOrWhiteSpaceAcceptsWhiteSpaceBeyondTheAsciiOnes()
    {
        // string.IsNullOrWhiteSpace uses char.IsWhiteSpace, which is far more than ' ' and '\t'.
        Assert.True("12\u00A0:\u3000\u200034".IsOnlyCharsOrWhiteSpace(TimeCodeLookup));
        Assert.False("12a34".IsOnlyCharsOrWhiteSpace(TimeCodeLookup));
    }

    [Fact]
    public void IsOnlyCharsOrWhiteSpaceTreatsEmptyAsOnlyThose()
    {
        Assert.True(string.Empty.IsOnlyCharsOrWhiteSpace(TimeCodeLookup));
        Assert.True("   ".IsOnlyCharsOrWhiteSpace(TimeCodeLookup));
        Assert.True(string.Empty.IsOnlyChars(TimeCodeLookup));
    }

    // ---------------------------------------------------------------- Utilities.IsNumber

    private static readonly Regex ReferenceIsNumber = new Regex("^\\d+$", RegexOptions.Compiled);
    private static readonly Regex ReferenceIsEpisodeNumber = new Regex("^\\d+x\\d+$", RegexOptions.Compiled);

    private static bool ReferenceIsNumberOrEpisode(string s)
    {
        s = s.Trim('$', '\u00A3', '\u00A5', '%', '*');
        return ReferenceIsNumber.IsMatch(s) || ReferenceIsEpisodeNumber.IsMatch(s);
    }

    [Fact]
    public void IsNumberMatchesTheRegexesItReplaced()
    {
        foreach (var input in Fuzz(40000))
        {
            Assert.Equal(ReferenceIsNumberOrEpisode(input), Utilities.IsNumber(input));
        }
    }

    [Fact]
    public void IsNumberKeepsTheRegexQuirks()
    {
        // \d is \p{Nd}, so Arabic-Indic digits count just as they did through the regex.
        Assert.Equal(ReferenceIsNumberOrEpisode("\u0661\u0662"), Utilities.IsNumber("\u0661\u0662"));
        Assert.True(Utilities.IsNumber("\u0661\u0662"));

        // .NET's $ also matches right before a single trailing line feed - but not two.
        Assert.True(Utilities.IsNumber("42\n"));
        Assert.False(Utilities.IsNumber("42\n\n"));
        Assert.False(Utilities.IsNumber("42\r\n"));

        Assert.True(Utilities.IsNumber("$42*"));
        Assert.True(Utilities.IsNumber("1x02"));
        Assert.False(Utilities.IsNumber("1x"));
        Assert.False(Utilities.IsNumber("x02"));
        Assert.False(Utilities.IsNumber("1x2x3"));
        Assert.False(Utilities.IsNumber(string.Empty));
        Assert.False(Utilities.IsNumber("42 "));
    }

    // ---------------------------------------------------------------- HtmlUtil.RemoveColorTags

    private static readonly Regex ColorAttributeRegex =
        new Regex("[ ]*(COLOR|color|Color)=[\"']*[#\\dA-Za-z]*[\"']*[ ]*", RegexOptions.Compiled);

    private static string ReferenceRemoveColorTags(string input)
    {
        var r = ColorAttributeRegex;
        var s = input;
        var match = r.Match(s);
        while (match.Success)
        {
            s = s.Remove(match.Index, match.Value.Length).Insert(match.Index, " ");
            if (match.Index > 4)
            {
                if (string.Compare(s, match.Index - 5, "<font >", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    s = s.Remove(match.Index - 5, 7);
                    var endIndex = s.IndexOf("</font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        s = s.Remove(endIndex, 7);
                    }
                    else
                    {
                        endIndex = s.IndexOf("< /font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            s = s.Remove(endIndex, 7);
                        }
                        else
                        {
                            endIndex = s.IndexOf("</ font>", match.Index - 5, StringComparison.OrdinalIgnoreCase);
                            if (endIndex >= 0)
                            {
                                s = s.Remove(endIndex, 7);
                            }
                        }
                    }
                }
                else if (s.Length > match.Index + 1 && s[match.Index + 1] == '>')
                {
                    s = s.Remove(match.Index, 1);
                }
            }

            match = r.Match(s);
        }

        return s.Trim();
    }

    [Fact]
    public void RemoveColorTagsMatchesTheUnguardedLoop()
    {
        var cases = new List<string>
        {
            "<font color=\"#ff0000\">Hello there.</font>",
            "<font COLOR='red'>Hello there.</font>",
            "<font Color=red>Hello</font> and <font color=blue>there</font>",
            "Hello there.",
            " color=red without any tag ",
            "<i>Hello</i>",
            string.Empty,
            "   ",
        };
        cases.AddRange(Fuzz(5000, 32, seed: 7));

        foreach (var input in cases)
        {
            Assert.Equal(ReferenceRemoveColorTags(input), HtmlUtil.RemoveColorTags(input));
        }
    }

    // ------------------------------------------------------- HtmlUtil.RemoveAssAlignmentTags

    private static string ReferenceRemoveAssAlignmentTags(string s)
    {
        var text = s;
        for (var digit = '1'; digit <= '9'; digit++)
        {
            text = text.Replace("{\\an" + digit + "}", string.Empty);
        }

        for (var digit = '1'; digit <= '9'; digit++)
        {
            text = text.Replace("{an" + digit + "\\", "{");
        }

        for (var digit = '1'; digit <= '9'; digit++)
        {
            text = text.Replace("\\an" + digit + "\\", "\\");
        }

        for (var digit = '1'; digit <= '9'; digit++)
        {
            text = text.Replace("\\an" + digit + "}", "}");
        }

        for (var digit = '1'; digit <= '9'; digit++)
        {
            text = text.Replace("{\\a" + digit + "}", string.Empty);
        }

        return text;
    }

    [Fact]
    public void RemoveAssAlignmentTagsMatchesTheUnguardedReplaceChain()
    {
        var cases = new List<string>
        {
            "{\\an8}Hello there.",
            "{\\a5}Hello there.",
            "{an3\\pos(320,240)}Hello there.",
            "{\\pos(320,240)\\an1\\fad(200,200)}Hello",
            "{\\fad(200,200)\\an7}Hello",
            "{\\pos(320,240)}Hello there.",   // backslash, but no alignment tag
            "Hello there.",                    // no backslash at all
            "{\\an0}Hello",                    // 0 is not an alignment
            "\\a4}",
            "{\\a9}",
            string.Empty,
        };
        cases.AddRange(Fuzz(20000, 20, seed: 11));

        foreach (var input in cases)
        {
            Assert.Equal(ReferenceRemoveAssAlignmentTags(input), HtmlUtil.RemoveAssAlignmentTags(input));
        }
    }

    // ---------------------------------------------------------------- Json.ReadTag

    [Fact]
    public void JsonReadTagFindsQuotedTagsAndIgnoresBareOnes()
    {
        Assert.Equal("12340", Json.ReadTag("{\"tStartMs\":12340,\"x\":1}", "tStartMs"));
        Assert.Equal("12340", Json.ReadTag("{'tStartMs':12340,'x':1}", "tStartMs"));
        Assert.Null(Json.ReadTag("{\"other\":1}", "tStartMs"));

        // A bare, unquoted occurrence must not be taken for the tag.
        Assert.Equal("7", Json.ReadTag("{\"note\":\"see tStartMs below\",\"tStartMs\":7}", "tStartMs"));

        // Double quotes win over single quotes, whichever comes first in the text.
        Assert.Equal("2", Json.ReadTag("{'dur':1,\"dur\":2}", "dur"));
    }
}
