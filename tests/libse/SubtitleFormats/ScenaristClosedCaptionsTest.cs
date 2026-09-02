using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibSETests.SubtitleFormats;

public class ScenaristClosedCaptionsTest
{
    private static Subtitle LoadScc(params string[] timedRows)
    {
        var lines = new List<string> { "Scenarist_SCC V1.0", "" };
        foreach (var row in timedRows)
        {
            lines.Add(row);
            lines.Add(string.Empty);
        }

        var subtitle = new Subtitle();
        new ScenaristClosedCaptions().LoadSubtitle(subtitle, lines, "test.scc");
        return subtitle;
    }

    [Fact]
    public void ImportValidCaption()
    {
        // "OK" encoded with CEA-608 odd parity: O = 4f, K = cb (0x4b with the parity bit set).
        var subtitle = LoadScc(
            "00:00:00:00\t94ae 94ae 9420 9420 9470 9470 4fcb 942f 942f",
            "00:00:04:00\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("OK", HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true));
    }

    [Fact]
    public void DoesNotEmitRawCea608BytesAsText()
    {
        // Synthetic repro from issue #11341: a row of raw CEA-608 data words (no proper
        // positioning codes) used to be decoded byte-pair-by-byte-pair into one long line of
        // mojibake and emitted as a subtitle cue. It must be rejected instead.
        var subtitle = LoadScc(
            "00:00:00:00\t94ae 94ae 9420 9420 9470 9470 4fcb 942f 942f",
            "00:00:04:00\t942c 942c",
            "00:00:25:00\t94ae 94ae 9420 9420 e640 e640 f06d 4250 20f0 6e52 f0f0 f750 2050 d061 5050 e53e f0c0 e550 e2f2 4f20 e262 5256 e0e5 f0e6 e2c0 402c 48e9 3068 c076 52d6 f0e0 e320 fe60 5020 de50 e020 f6e0 5020 5e60 e0e0 d061 de50 5252 ff70 4050 61e6 40c0 e942 e640 5ec0 52e6 4040 af20 9137 9137 e0e0 50f2 ff70 4050 61e6 40c0 e942 e640 ff70 4050 61e6 40c0 e942 e640 942f 942f",
            "00:45:36:04\t942c 942c");

        // Only the single valid caption survives; the bogus 608-data row is dropped.
        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("OK", HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true));
        Assert.DoesNotContain(subtitle.Paragraphs, p => p.Text.Contains("f@"));
    }

    [Fact]
    public void KeepsLongCaptionWithPositioningCode()
    {
        // A genuine two-row caption whose decoded text happens to exceed 32 characters (because
        // the decoder does not always re-insert the line break) must still be kept: it has a
        // Preamble Address Code (1340 / 13e0), so it is real caption text, not raw 608 data.
        var subtitle = LoadScc(
            "00:00:25:00\t94ae 94ae 9420 9420 1340 1340 9723 9723 cec1 5252 c154 4f52 ba20 616e 6420 68e9 7320 e6f2 e9e5 6e64 732c 13e0 13e0 9723 9723 2073 f4ef 7020 e9ec ece5 6794 2c94 2c61 ec20 e9e3 e520 e6e9 7368 e5f2 6de5 6e80 94d6 94d6 20e3 efec 64ae aeae 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        var text = HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true);
        Assert.Contains("NARRATOR", text);
        Assert.Contains("illegal ice fishermen", text);
    }

    [Fact]
    public void PreambleAddressCodesProduceLineBreaks()
    {
        // Issue #9803: row-positioning Preamble Address Codes (916e, 92ce, ...) were decoded as
        // stray letters ("n"/"N") instead of line breaks, e.g. "NARRADOR:nAsh y sus amigosN...".
        var subtitle = LoadScc(
            "00:00:25:00\t9420 9420 91d0 91d0 cec1 5252 c1c4 4f52 ba80 916e 916e c173 6820 7920 7375 7320 616d e967 ef73 92ce 92ce e3ef 6ef4 e96e e061 6e20 7375 2076 e961 eae5 942c 942c 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        var lines = HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true).SplitToLines();
        Assert.Equal(new[] { "NARRADOR:", "Ash y sus amigos", "continúan su viaje" }, lines);
        Assert.DoesNotContain("amigosN", subtitle.Paragraphs[0].Text);
        Assert.DoesNotContain(":n", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void KeepsLongCaptionWithParityStrippedPositioningCode()
    {
        // PAC row codes also appear parity-stripped (11/12/14/17, e.g. 1140) in some files. Such a
        // row must be recognized as positioned caption text and kept even when its single decoded
        // line exceeds 32 characters - not mistaken for a raw 608 data row and dropped.
        var aaaa = string.Concat(Enumerable.Repeat("c1c1 ", 20)); // 40 x 'A'
        var subtitle = LoadScc(
            $"00:00:25:00\t94ae 94ae 9420 9420 1140 1140 {aaaa}942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal(new string('A', 40), HtmlUtil.RemoveHtmlTags(subtitle.Paragraphs[0].Text, true));
    }

    [Fact]
    public void ItalicPreambleAddressCodeProducesItalic()
    {
        // Issue #9803: italics are carried by italic-style PACs (916e, 92ce, ...), not by mid-row
        // codes. "NARRADOR:" (regular PAC 91d0) stays regular; the next rows (916e/92ce) are italic.
        var subtitle = LoadScc(
            "00:00:25:00\t9420 9420 91d0 91d0 cec1 5252 c1c4 4f52 ba80 916e 916e c173 6820 7920 7375 7320 616d e967 ef73 92ce 92ce e3ef 6ef4 e96e e061 6e20 7375 2076 e961 eae5 942c 942c 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("{\\an7}NARRADOR:" + Environment.NewLine + "<i>Ash y sus amigos" + Environment.NewLine + "continúan su viaje</i>",
            subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void DecodesMusicNoteSplitAcrossWordBoundary()
    {
        // Issue #9803: a trailing " ♪" can arrive byte-misaligned as "2091 3780" (space + the note
        // code 9137 split over two words + 80 padding). It must decode to ♪, not "7".
        var subtitle = LoadScc(
            "00:00:25:00\t9420 9420 946e 946e 9137 9137 2045 ec20 6d75 6e64 ef20 f175 e9e5 f2ef 2076 e5f2 2091 3780 942c 942c 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("<i>♪ El mundo quiero ver ♪</i>", subtitle.Paragraphs[0].Text);
        Assert.DoesNotContain("7", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void DecodesAlignedMusicNotes()
    {
        // The common case: both notes byte-aligned as full 9137 words must keep working.
        var subtitle = LoadScc(
            "00:00:25:00\t9420 9420 946e 946e 9137 9137 20c4 e520 d075 e562 ecef 20d0 61ec e5f4 6120 73ef 7920 9137 9137 942c 942c 942f 942f",
            "00:45:36:04\t942c 942c");

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("<i>♪ De Pueblo Paleta soy ♪</i>", subtitle.Paragraphs[0].Text);
    }

    private static string SaveScc(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000));
        var scc = new ScenaristClosedCaptions().ToText(subtitle, "test");
        return scc.SplitToLines().First(line => line.Contains("9420")).Trim();
    }

    private static string SaveAndReloadScc(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000));
        var format = new ScenaristClosedCaptions();
        var reloaded = new Subtitle();
        format.LoadSubtitle(reloaded, format.ToText(subtitle, "test").SplitToLines(), "test.scc");
        return reloaded.Paragraphs.Count == 0 ? string.Empty : reloaded.Paragraphs[0].Text;
    }

    [Fact]
    public void WritesColorAsMidRowCode()
    {
        // Issue #14239: the writer only knew "<i>", so a color tag was encoded letter by letter
        // and showed up on screen as "<font color=...". Yellow is the mid-row code 912a, and
        // going back to white after it is 9120 (control codes are always sent twice).
        var scc = SaveScc("-- <font color=\"yellow\">Captions by VITAC</font> --");

        Assert.Contains("912a 912a", scc);
        Assert.Contains("9120 9120", scc);
        Assert.Equal("-- <font color=\"Yellow\">Captions by VITAC</font> --", SaveAndReloadScc("-- <font color=\"yellow\">Captions by VITAC</font> --"));
    }

    [Fact]
    public void WritesWebVttColorClassAsMidRowCode()
    {
        // WebVTT color classes are kept as they are when a .vtt file is loaded, so they reach
        // the SCC writer as "<c.yellow>" instead of "<font color=...>".
        Assert.Contains("912a 912a", SaveScc("<c.yellow>Captions by VITAC</c>"));
        Assert.Equal("<font color=\"Yellow\">Captions by VITAC</font>", SaveAndReloadScc("<c.yellow>Captions by VITAC</c>"));
    }

    [Theory]
    [InlineData("white", "9120")]
    [InlineData("green", "91a2")]
    [InlineData("blue", "91a4")]
    [InlineData("cyan", "9126")]
    [InlineData("red", "91a8")]
    [InlineData("yellow", "912a")]
    [InlineData("magenta", "912c")]
    [InlineData("#00FF00", "91a2")]     // hex spelling
    [InlineData("#104010", "91a2")]     // a dark green is still green
    [InlineData("#808080", "9120")]     // gray is nearest to white
    [InlineData("black", "9120")]       // CEA-608 has no black foreground - stay white
    [InlineData("chucknorris", "9120")] // not a color at all
    public void WritesAllCea608Colors(string color, string expectedCode)
    {
        var scc = SaveScc("Hi <font color=\"" + color + "\">there</font>");

        if (expectedCode == "9120")
        {
            Assert.DoesNotContain("91a2", scc);
            Assert.DoesNotContain("912a", scc);
        }
        else
        {
            Assert.Contains(expectedCode + " " + expectedCode, scc);
        }
    }

    [Fact]
    public void DoesNotWriteTagsAsText()
    {
        // Any tag with no CEA-608 equivalent must be dropped, not encoded letter by letter.
        foreach (var text in new[] { "<b>Hello</b>", "<ruby>Hello</ruby>", "<v Fred>Hello</v>", "<font face=\"Arial\">Hello</font>" })
        {
            var scc = SaveScc(text);

            Assert.DoesNotContain("bc", scc); // "<" with odd parity
            Assert.DoesNotContain("3e", scc); // ">"
            Assert.Equal("Hello", SaveAndReloadScc(text));
        }
    }

    [Fact]
    public void KeepsLessThanSignInText()
    {
        // ...but a stray "<" is text, not a tag ("bc" is "<" with odd parity, "3e" is ">").
        var scc = SaveScc("a < b > c");

        Assert.Contains("bc", scc);
        Assert.Contains("3e", scc);
        Assert.Equal("a < b > c", SaveAndReloadScc("a < b > c"));
    }

    [Fact]
    public void ItalicsWinsOverColor()
    {
        // CEA-608 encodes italics in the same slot as the colors - colored italics do not exist,
        // so the color inside an italic run is dropped instead of ending the italics.
        var scc = SaveScc("<i>Italic <font color=\"yellow\">yellow</font></i>");

        Assert.Contains("91ae 91ae", scc);
        Assert.DoesNotContain("912a", scc);
        Assert.Equal("<i>Italic yellow</i>", SaveAndReloadScc("<i>Italic <font color=\"yellow\">yellow</font></i>"));
    }

    [Fact]
    public void ReopensColorOnEveryRow()
    {
        // A Preamble Address Code resets the row to white, so a color spanning two lines must be
        // written again on the second row.
        var scc = SaveScc("<font color=\"yellow\">Line one" + Environment.NewLine + "line two</font>");

        Assert.Equal(2, scc.Split(new[] { "912a 912a" }, StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void CenteringCountsMidRowCodeCells()
    {
        // A mid-row code takes a screen cell (it shows as a space), so a line with a color and a
        // reset in it is two cells wider than its text and must be indented two cells less:
        // 24 characters center on column 4 (the row 15 code 94f2), 24 + 2 on column 0 + tab 3.
        Assert.Contains("94f2 94f2", SaveScc("Twenty four characters!!"));
        Assert.Contains("9470 9470 9723 9723", SaveScc("<font color=\"yellow\">Twenty four</font> characters!!"));
    }

    [Fact]
    public void LongColoredLineStaysOnTheRow()
    {
        // The 32 characters plus the mid-row code do not fit the row - do not fall back to a
        // right-hand column (the old negative indent picked column 28).
        var scc = SaveScc("<font color=\"yellow\">12345678901234567890123456789012</font>");

        Assert.Contains("9470 9470", scc); // row 15, column 0
        Assert.DoesNotContain("94fe", scc); // row 15, column 28
    }
}

public class ScenaristClosedCaptionsFormatLimitsTest
{
    [Fact]
    public void FormatLimits_Are32CharsAnd4Lines()
    {
        var limits = new ScenaristClosedCaptions().FormatLimits;
        Assert.NotNull(limits);
        Assert.Equal(32, limits!.MaxCharactersPerLine);
        Assert.Equal(4, limits.MaxLines);
    }

    [Fact]
    public void GetViolatingParagraphNumbers_FlagsLongLinesAndTooManyLines_IgnoringTags()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Short line", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("This merged line is well over thirty-two chars", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("<i>Tags do not count toward 32</i>", 2000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("1\r\n2\r\n3\r\n4\r\n5", 3000, 4000));

        var violating = new ScenaristClosedCaptions().FormatLimits!.GetViolatingParagraphNumbers(subtitle);

        Assert.Equal(new List<int> { 2, 4 }, violating);
    }

    [Fact]
    public void FormatLimits_DefaultIsNull()
    {
        Assert.Null(new SubRip().FormatLimits);
    }
}
