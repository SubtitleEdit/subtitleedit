using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

/// <summary>
/// Guard tests for the 2026-08-30 bug hunt: parsers that never terminated or threw on malformed
/// input, a CSV reader that lost the state a continuation line resumes in, and case conversion
/// that followed the machine's locale instead of the subtitle's language.
/// </summary>
public class BugHunt21Test
{
    private static Subtitle OneLine(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 1000));
        return subtitle;
    }

    private static bool Completes(string text)
    {
        // "index = IndexOf(...) + 1" is 0 when there is no match, which restarted the scan from
        // the beginning forever - the app froze on a single unclosed tag.
        var task = Task.Run(() =>
            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(OneLine(text), true, true, true, true, true, "en"));
        return task.Wait(5000);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("{\\i1 Hello there")]
    [InlineData("<font color=#ff0000 Hello")]
    [InlineData("<c.yellow Hello")]
    public void ConvertColorsToDialog_TerminatesOnUnclosedTag(string text)
    {
        Assert.True(Completes(text), "ConvertColorsToDialogInSubtitle did not terminate");
    }

    [Fact]
    public void ConvertColorsToDialog_StillConvertsTwoColors()
    {
        var subtitle = OneLine("<font color=\"#ff0000\">Hello</font> <font color=\"#00ff00\">there</font>");
        ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, true, true, false, false, "en");
        Assert.Contains("-", subtitle.Paragraphs[0].Text);
        Assert.Contains("Hello", subtitle.Paragraphs[0].Text);
        Assert.Contains("there", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void CsvSplit_KeepsTrailingEmptyField()
    {
        Assert.Equal(new[] { "a", "" }, CsvUtil.CsvSplit("a,", false, out _, ','));
        Assert.Equal(new[] { "a", "", "" }, CsvUtil.CsvSplit("a,,", false, out _, ','));
        Assert.Equal(new[] { "", "a" }, CsvUtil.CsvSplit(",a", false, out _, ','));
        Assert.Empty(CsvUtil.CsvSplit(string.Empty, false, out _, ','));
    }

    [Fact]
    public void CsvSplitLines_ClosingQuoteAtStartOfContinuationLine()
    {
        // The closing quote was appended as literal text and the field stayed open, so every
        // later line was swallowed into it.
        var rows = CsvUtil.CsvSplitLines(new List<string> { "1,\"Hello", "world", "\",x" }, ',');
        Assert.Single(rows);
        Assert.Equal(3, rows[0].Count);
        Assert.Equal("1", rows[0][0]);
        Assert.Equal("x", rows[0][2]);
    }

    [Fact]
    public void CsvSplitLines_SeparatorAtStartOfContinuationLine()
    {
        // A comma that is inside the quotes must not split the field.
        var rows = CsvUtil.CsvSplitLines(new List<string> { "\"Hello", ", world\",x" }, ',');
        Assert.Single(rows);
        Assert.Equal(2, rows[0].Count);
        Assert.Equal("x", rows[0][1]);
    }

    [Fact]
    public void CsvSplitLines_PlainRowsAreUnchanged()
    {
        var rows = CsvUtil.CsvSplitLines(new List<string> { "a,b,c", "1,\"two, still two\",3" }, ',');
        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "a", "b", "c" }, rows[0]);
        Assert.Equal(new[] { "1", "two, still two", "3" }, rows[1]);
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void FixCasing_UpperAndLowerFollowTheSubtitleLanguage(string machineCulture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(machineCulture);

            var upper = OneLine("I think it is fine.");
            new FixCasing("en") { FixMakeUppercase = true }.Fix(upper);
            Assert.Equal("I THINK IT IS FINE.", upper.Paragraphs[0].Text);

            var lower = OneLine("I THINK IT IS FINE.");
            new FixCasing("en") { FixMakeLowercase = true }.Fix(lower);
            Assert.Equal("i think it is fine.", lower.Paragraphs[0].Text);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void FixCasing_TurkishSubtitleStillGetsTurkishCasing()
    {
        var upper = OneLine("iyi");
        new FixCasing("tr") { FixMakeUppercase = true }.Fix(upper);
        Assert.Equal("İYİ", upper.Paragraphs[0].Text);
    }

    [Fact]
    public void RichTextToPlainText_UnbalancedBraceDoesNotThrow()
    {
        Assert.Equal("Hello", RichTextToPlainText.ConvertToText("{\\rtf1\\ansi Hello}}").Trim());
    }

    [Fact]
    public void SubtitleParse_MalformedRtfReportsUnknownFormatInsteadOfThrowing()
    {
        // ~20 readers run their input through FromRtf(), so one stray '}' used to abort format
        // detection for the whole file - whatever its extension.
        var lines = new List<string> { "{\\rtf1\\ansi", "00:00:01:00 00:00:02:00 Hello}", "}}" };
        foreach (var ext in new[] { ".rtf", ".srt", ".txt" })
        {
            var exception = Record.Exception(() => Subtitle.Parse(lines, ext));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void UuDecode_NonAsciiCharacterDoesNotThrow()
    {
        var exception = Record.Exception(() => UUEncoding.UUDecode("♪♪"));
        Assert.Null(exception);
    }

    [Fact]
    public void UuEncoding_RoundTripStillWorks()
    {
        var bytes = new byte[] { 1, 2, 3, 250, 128, 0, 77 };
        Assert.Equal(bytes, UUEncoding.UUDecode(UUEncoding.UUEncode(bytes)));
    }

    [Fact]
    public void DeleteShotChanges_RemovesTheFileFoundForARenamedVideo()
    {
        var root = Path.Combine(Path.GetTempPath(), "se-bughunt21-" + Path.GetRandomFileName());
        var shotChangeDirectory = Path.Combine(root, "shotchanges");
        Directory.CreateDirectory(shotChangeDirectory);
        try
        {
            // The hash comes from the file's content, so renaming the video keeps it - the stored
            // file still carries the OLD name, which is why the delete has to go through the same
            // wildcard the load uses.
            var videoFileName = Path.Combine(root, "movie-renamed.mkv");
            File.WriteAllBytes(videoFileName, new byte[70000]);
            var hash = MovieHasher.GenerateHash(videoFileName);
            var stored = Path.Combine(shotChangeDirectory, $"{hash}_movieoriginalname.shotchanges");
            File.WriteAllText(stored, "1.5");

            Assert.Single(ShotChangeHelper.FromDisk(videoFileName, shotChangeDirectory));

            ShotChangeHelper.DeleteShotChanges(videoFileName, shotChangeDirectory);

            Assert.False(File.Exists(stored));
            Assert.Empty(ShotChangeHelper.FromDisk(videoFileName, shotChangeDirectory));
        }
        finally
        {
            try
            {
                Directory.Delete(root, true);
            }
            catch
            {
                // ignore
            }
        }
    }
}
