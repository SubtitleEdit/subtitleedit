using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using SkiaSharp;

namespace LibSETests.Common;

public class UtilitiesTest
{
    // SplitEndTags walks the line right-to-left, so each stripped tag must be prepended to
    // "post" to preserve the original suffix order - it used to append, so nested closing
    // tags came back reversed ("</font></i>") when callers rebuilt "pre + text + post".
    [Fact]
    public void SplitEndTagsNestedClosingTagsKeepOriginalOrder()
    {
        var post = string.Empty;
        var s = Utilities.SplitEndTags("<font color=\"white\"><i>Hello</i></font>", ref post);
        Assert.Equal("<font color=\"white\"><i>Hello", s);
        Assert.Equal("</i></font>", post);
    }

    [Fact]
    public void SplitEndTagsTagBeforeTrailingSpaceKeepsOriginalOrder()
    {
        var post = string.Empty;
        var s = Utilities.SplitEndTags("Hello</i> ", ref post);
        Assert.Equal("Hello", s);
        Assert.Equal("</i> ", post);
    }

    // WebVTT tracks in a Matroska container must be loaded as WebVTT, not SubRip.
    // MakeMKV (codec id "D_WEBVTT/*") prepends "<cue identifier>\n<cue settings>\n" to each
    // block, which previously leaked into the subtitle text (issue #11680).
    [Fact]
    public void LoadMatroskaTextSubtitle_WebVtt_DetectsFormatAndStripsCueHeader()
    {
        var track = new MatroskaTrackInfo { CodecId = "D_WEBVTT/SUBTITLES", IsSubtitle = true };
        var sub = new List<MatroskaSubtitle>
        {
            new MatroskaSubtitle(Encoding.UTF8.GetBytes("1\n\n[TENSE MUSIC]"), 160, 1000),
            new MatroskaSubtitle(Encoding.UTF8.GetBytes("\nalign:middle line:90%\nHello there.\nSecond line."), 3320, 1680),
        };
        var subtitle = new Subtitle();

        var format = Utilities.LoadMatroskaTextSubtitle(track, null, sub, subtitle);

        Assert.Equal("WebVTT", format.Name);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("[TENSE MUSIC]", subtitle.Paragraphs[0].Text);
        Assert.Equal("Hello there." + Environment.NewLine + "Second line.", subtitle.Paragraphs[1].Text);
    }

    // DisplayFileSizeToBytes used to cast each result to int, so any value >= 2 GB
    // (and large mb inputs) overflowed to a negative/garbage number. The method
    // returns long, so the result must survive past int.MaxValue.

    [Fact]
    public void DisplayFileSizeToBytes_Gigabytes_DoesNotOverflow()
    {
        Assert.Equal(3221225472L, Utilities.DisplayFileSizeToBytes("3 gb"));
    }

    [Fact]
    public void DisplayFileSizeToBytes_LargeMegabytes_DoesNotOverflow()
    {
        // 2048 mb = 2147483648 bytes, exactly one more than int.MaxValue.
        Assert.Equal(2147483648L, Utilities.DisplayFileSizeToBytes("2048 mb"));
    }

    [Fact]
    public void DisplayFileSizeToBytes_Kilobytes_RoundTrips()
    {
        Assert.Equal(2048L, Utilities.DisplayFileSizeToBytes("2 kb"));
    }

    // A second line fully wrapped in music symbols (♪ ... ♪) must not be auto-broken.
    // A copy/paste bug used to test line 0's ending instead of line 1's, so this case
    // slipped through and the line got merged/re-broken.
    [Fact]
    public void AutoBreakLine_KeepsSecondLineWrappedInMusicSymbols()
    {
        var input = "♪ La la la" + Environment.NewLine + "♪ da da ♪";

        var result = Utilities.AutoBreakLinePrivate(input, 43, 100, string.Empty, false);

        Assert.Equal(input, result);
    }

    // A multi-word "do not break after" entry (e.g. "SORT OF") must keep the whole phrase
    // together: the line may not be split between its words, even when a single word of the
    // phrase ("OF") is in the list too (issue #9631).
    [Theory]
    [InlineData("HE WAS SORT OF WEIRD", 10, new[] { "SORT OF", "OF" })]
    [InlineData("HE WAS SORT OF WEIRD", 10, new[] { "SORT OF" })]
    [InlineData("I AT LEAST KNOW HIM", 8, new[] { "AT LEAST", "LEAST" })]
    public void AutoBreakLine_NoBreakAfterMultiWordEntry_KeepsPhraseTogether(string input, int maxLength, string[] listItems)
    {
        var oldDataDirectory = Configuration.DataDirectory;
        var oldUseNoLineBreakAfter = Configuration.Settings.Tools.UseNoLineBreakAfter;
        var dictionaryFolder = Path.Combine(Path.GetTempPath(), "se9631_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(dictionaryFolder, "Dictionaries"));
            var xml = "<NoBreakAfterList>" + Environment.NewLine +
                      string.Join(Environment.NewLine, listItems.Select(p => "  <Item>" + p + "</Item>")) +
                      Environment.NewLine + "</NoBreakAfterList>";
            File.WriteAllText(Path.Combine(dictionaryFolder, "Dictionaries", "zz_NoBreakAfterList.xml"), xml);
            Configuration.DataDirectory = dictionaryFolder;
            Configuration.Settings.Tools.UseNoLineBreakAfter = true;
            Utilities.ResetNoBreakAfterList();

            var result = Utilities.AutoBreakLinePrivate(input, maxLength, 0, "zz", false);

            var lines = result.SplitToLines();
            Assert.True(lines.Count == 2, "Expected two lines, got: " + result);
            foreach (var item in listItems.Where(p => p.IndexOf(' ') >= 0))
            {
                Assert.True(lines[0].Contains(item) || lines[1].Contains(item),
                    "Phrase \"" + item + "\" was split: " + result);
            }
        }
        finally
        {
            Configuration.DataDirectory = oldDataDirectory;
            Configuration.Settings.Tools.UseNoLineBreakAfter = oldUseNoLineBreakAfter;
            Utilities.ResetNoBreakAfterList();
            try
            {
                Directory.Delete(dictionaryFolder, true);
            }
            catch (IOException)
            {
            }
        }
    }

    // A regex entry matches against the text before the break point. CanBreak matches the list
    // against a span and only materialises that text as a string when the list has a regex in it,
    // so a list with one still has to work - none of the shipped lists has one.
    [Fact]
    public void AutoBreakLine_NoBreakAfterRegexEntry_IsHonored()
    {
        var oldDataDirectory = Configuration.DataDirectory;
        var oldUseNoLineBreakAfter = Configuration.Settings.Tools.UseNoLineBreakAfter;
        var dictionaryFolder = Path.Combine(Path.GetTempPath(), "seRegexNoBreak_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(dictionaryFolder, "Dictionaries"));
            File.WriteAllText(Path.Combine(dictionaryFolder, "Dictionaries", "zz_NoBreakAfterList.xml"),
                "<NoBreakAfterList>" + Environment.NewLine +
                "  <Item RegEx=\"true\">\\bnumber$</Item>" + Environment.NewLine +
                "</NoBreakAfterList>");
            Configuration.DataDirectory = dictionaryFolder;
            Configuration.Settings.Tools.UseNoLineBreakAfter = true;
            Utilities.ResetNoBreakAfterList();

            var result = Utilities.AutoBreakLinePrivate("Call the number seven now", 14, 0, "zz", false);

            var lines = result.SplitToLines();
            Assert.True(lines.Count == 2, "Expected two lines, got: " + result);
            Assert.False(lines[0].TrimEnd().EndsWith("number"), "Broke after the regex entry: " + result);
        }
        finally
        {
            Configuration.DataDirectory = oldDataDirectory;
            Configuration.Settings.Tools.UseNoLineBreakAfter = oldUseNoLineBreakAfter;
            Utilities.ResetNoBreakAfterList();
            try
            {
                Directory.Delete(dictionaryFolder, true);
            }
            catch (IOException)
            {
            }
        }
    }

    // GetColorFromFontString measured colorStart relative to the extracted <font> tag
    // but indexed the full string, so a tag that wasn't at the start of the line read
    // the wrong region and returned the default color.
    [Fact]
    public void GetColorFromFontString_WithLeadingText_ReturnsTagColor()
    {
        var result = Utilities.GetColorFromFontString("Hi <font color=\"#FF0000\">x</font>", SKColors.Blue);

        Assert.Equal(new SKColor(255, 0, 0), result);
    }

    // French typography: a space before ! ? : ; — applied to French OCR output (issue #11702).
    [Theory]
    [InlineData("Quoi?", "Quoi ?")]
    [InlineData("Bonjour!", "Bonjour !")]
    [InlineData("Paul:", "Paul :")]
    [InlineData("fin;", "fin ;")]
    [InlineData("J'arrive. Tu viens?", "J'arrive. Tu viens ?")]
    [InlineData("Quoi? Vraiment?", "Quoi ? Vraiment ?")]
    public void AddSpaceBeforeFrenchPunctuation_InsertsSpace(string input, string expected)
    {
        Assert.Equal(expected, Utilities.AddSpaceBeforeFrenchPunctuation(input));
    }

    [Theory]
    [InlineData("Déjà vu ?")]        // already spaced
    [InlineData("12:30")]            // digit before colon (time code) — untouched
    [InlineData("Vraiment ?!")]      // mark after a mark, not a letter — untouched
    [InlineData("")]
    [InlineData("Hello")]
    public void AddSpaceBeforeFrenchPunctuation_LeavesOthersUnchanged(string input)
    {
        Assert.Equal(input, Utilities.AddSpaceBeforeFrenchPunctuation(input));
    }

    // The " 's " → "'s " merge is an English possessive OCR fix - it must not run for Dutch
    // (" 's avonds" is a separate genitive word, issue #12144) nor for the empty language code
    // the auto-trim path produces when language detection fails.
    [Theory]
    [InlineData("Ik hoorde ze 's avonds ruzie maken.", "nl", "Ik hoorde ze 's avonds ruzie maken.")]
    [InlineData("Ik hoorde ze 's avonds ruzie maken.", "", "Ik hoorde ze 's avonds ruzie maken.")]
    [InlineData("John 's car is red.", "en", "John's car is red.")]
    public void RemoveUnneededSpaces_ApostropheSMerge_EnglishOnly(string input, string language, string expected)
    {
        Assert.Equal(expected, Utilities.RemoveUnneededSpaces(input, language));
    }

    // ReverseNumbers exercises the internal ReverseString: digit groups of two or more must be
    // fully mirrored. Guards the span-based rewrite - reversing must read from the source
    // string, not in-place from the destination span (which starts zero-filled and would also
    // corrupt the second half after the midpoint).
    [Theory]
    [InlineData("Hello 123", "Hello 321")]
    [InlineData("1234", "4321")]
    [InlineData("12345", "54321")]
    [InlineData("25 mm or 7 cm", "52 mm or 7 cm")] // single digits stay untouched
    [InlineData("Hello", "Hello")]
    [InlineData("", "")]
    public void ReverseNumbers_MirrorsTwoOrMoreDigitGroups(string input, string expected)
    {
        Assert.Equal(expected, Utilities.ReverseNumbers(input));
    }

    // ReverseStartAndEndingForRightToLeft exercises the internal ReverseString and
    // ReverseParenthesis: leading/trailing punctuation swaps ends, brackets are mirrored,
    // and formatting tags stay in place.
    [Theory]
    [InlineData("- Hello.", ".Hello -")]
    [InlineData("(Hello.)", "(.Hello)")]
    [InlineData("!?Hello", "Hello?!")]
    [InlineData("<i>Hello.</i>", "<i>.Hello</i>")]
    [InlineData("Hello", "Hello")]
    public void ReverseStartAndEndingForRightToLeft_SwapsAndMirrorsEdges(string input, string expected)
    {
        Assert.Equal(expected, Utilities.ReverseStartAndEndingForRightToLeft(input));
    }

    [Fact]
    public void ReverseStartAndEndingForRightToLeft_MultipleLines()
    {
        var result = Utilities.ReverseStartAndEndingForRightToLeft("- Hello." + Environment.NewLine + "- Bye.");
        Assert.Equal(".Hello -" + Environment.NewLine + ".Bye -", result);
    }
}
