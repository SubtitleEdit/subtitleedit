using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using SkiaSharp;

namespace LibSETests.Common;

public class UtilitiesTest
{
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
}
