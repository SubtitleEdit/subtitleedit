using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.SubtitleFormats.Legacy;

namespace LibSETests.SubtitleFormats;

public class SubRipTest
{
    private static Subtitle Load(string input, string fileName = "test.srt")
    {
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, input.SplitToLines(), fileName);
        return subtitle;
    }

    private static void AssertParagraph(Paragraph p, string start, string end, string text)
    {
        Assert.Equal(start, p.StartTime.ToString());
        Assert.Equal(end, p.EndTime.ToString());
        Assert.Equal(text.Replace("\n", Environment.NewLine), p.Text);
    }

    [Fact]
    public void Canonical()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,500\nHello\nWorld\n\n2\n00:00:03,000 --> 00:00:04,000\n<i>Bye</i>\n\n");
        Assert.Equal(2, s.Paragraphs.Count);
        AssertParagraph(s.Paragraphs[0], "00:00:01,000", "00:00:02,500", "Hello\nWorld");
        AssertParagraph(s.Paragraphs[1], "00:00:03,000", "00:00:04,000", "<i>Bye</i>");
        Assert.Equal(1, s.Paragraphs[0].Number);
        Assert.Equal(2, s.Paragraphs[1].Number);
    }

    [Fact]
    public void KeepsOriginalNumbers()
    {
        var s = Load("5\n00:00:01,000 --> 00:00:02,000\nA\n\n9\n00:00:03,000 --> 00:00:04,000\nB");
        Assert.Equal(5, s.Paragraphs[0].Number);
        Assert.Equal(9, s.Paragraphs[1].Number);
    }

    [Fact]
    public void MissingNumbers()
    {
        var s = Load("00:00:01,000 --> 00:00:02,000\nA\n\n00:00:03,000 --> 00:00:04,000\nB\n\n3\n00:00:05,000 --> 00:00:06,000\nC");
        Assert.Equal(3, s.Paragraphs.Count);
        Assert.Equal(new[] { "A", "B", "C" }, s.Paragraphs.Select(p => p.Text));
        Assert.Equal(new[] { 1, 2, 3 }, s.Paragraphs.Select(p => p.Number));
    }

    [Fact]
    public void MissingBlankLines()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\nA\n2\n00:00:03,000 --> 00:00:04,000\nB\n00:00:05,000 --> 00:00:06,000\nC");
        Assert.Equal(new[] { "A", "B", "C" }, s.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void ExtraBlankLines()
    {
        var s = Load("\n\n\n1\n\n00:00:01,000 --> 00:00:02,000\n\n\nA\n\n\n\n2\n\n\n00:00:03,000 --> 00:00:04,000\nB\n\n\n\n");
        Assert.Equal(new[] { "A", "B" }, s.Paragraphs.Select(p => p.Text));
        Assert.Equal(new[] { 1, 2 }, s.Paragraphs.Select(p => p.Number));
    }

    [Fact]
    public void BlankLineInsideTextIsKept()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\nLine one\n\nLine two\n\n2\n00:00:03,000 --> 00:00:04,000\nB");
        AssertParagraph(s.Paragraphs[0], "00:00:01,000", "00:00:02,000", "Line one\n\nLine two");
    }

    [Fact]
    public void NumberAsTextIsKept()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\n42\n\n2\n00:00:03,000 --> 00:00:04,000\n1999\n43\n");
        Assert.Equal(new[] { "42", "1999\n43".Replace("\n", Environment.NewLine) }, s.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void DanglingNumberAtEndOfFileIsDropped()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\nA\n\n2\n");
        Assert.Equal("A", Assert.Single(s.Paragraphs).Text);
    }

    [Fact]
    public void EmptyParagraphs()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\n\n2\n00:00:03,000 --> 00:00:04,000\n\n3\n00:00:05,000 --> 00:00:06,000\nC");
        Assert.Equal(new[] { "", "", "C" }, s.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void NoTrailingNewLineAndCrLf()
    {
        var s = Load("1\r\n00:00:01,000 --> 00:00:02,000\r\nA  \r\n\r\n2\r\n00:00:03,000 --> 00:00:04,000\r\nB");
        Assert.Equal(new[] { "A", "B" }, s.Paragraphs.Select(p => p.Text));
    }

    [Theory]
    [InlineData("00:00:01,000 --> 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01.000 --> 00:00:02.000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01:000 --> 00:00:02:000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01;000 --> 00:00:02;000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00.00.01,000 --> 00.00.02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 -> 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 —> 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 – 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 - > 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 -- > 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 -->> 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 ---> 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 ==> 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 -- 00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000-->00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000   -->   00:00:02,000", "00:00:01,000", "00:00:02,000")]
    [InlineData("  00:00:01,000 --> 00:00:02,000  ", "00:00:01,000", "00:00:02,000")]
    [InlineData("00: 00:01 ,000 --> 00:00 :02, 000", "00:00:01,000", "00:00:02,000")]
    [InlineData("0:0:1,5 --> 0:0:2,25", "00:00:01,005", "00:00:02,025")]
    [InlineData("00:00:01,0001 --> 00:00:02,9999", "00:00:01,000", "00:00:02,999")]
    [InlineData("01:48,460 --> 01:52,364", "00:01:48,460", "00:01:52,364")]
    [InlineData("00:00:01 --> 00:00:02", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:O1,000 --> 00:00:02,O00", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01،000 --> 00:00:02،000", "00:00:01,000", "00:00:02,000")]
    [InlineData("​00:00:01,000 --> 00:00:02,000﻿", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 --> 00:00:02,000 X1:100 X2:200 Y1:10 Y2:20", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 --> 00:00:02,000  X1: 100 X2: 200 Y1: 10 Y2: 20", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 --> 00:00:02,000\tx1:100 x2:200 y1:10 y2:20", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 --> 00:00:02,000X1:100 X2:200 Y1:10 Y2:20", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01.000 -> 00:00:02.000 X1:100 X2:200 Y1:10 Y2:20", "00:00:01,000", "00:00:02,000")]
    [InlineData("00:00:01,000 --> 00:00:02,000 {X1:100 Y1:10}", "00:00:01,000", "00:00:02,000")]
    [InlineData("99:00:01,000 --> 99:00:02,000", "99:00:01,000", "99:00:02,000")]
    public void TolerantTimeCodes(string timeCodeLine, string start, string end)
    {
        var s = Load("1\n" + timeCodeLine + "\nText\n\n2\n00:01:00,000 --> 00:01:01,000\nNext");
        Assert.Equal(2, s.Paragraphs.Count);
        AssertParagraph(s.Paragraphs[0], start, end, "Text");
    }

    [Fact]
    public void NegativeTimeCodes()
    {
        var s = Load("1\n-00:00:02,000 --> -00:00:01,000\nText");
        Assert.Equal(-2000, s.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(-1000, s.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Theory]
    [InlineData("10:30 - 11:00 is the meeting.")]
    [InlineData("1999 was a long time ago.")]
    [InlineData("- Dialogue line")]
    [InlineData("2 -> 3")]
    [InlineData("12:30 -- the time")]
    public void TextLinesAreNotTimeCodes(string line)
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\nA\n" + line + "\n\n2\n00:00:03,000 --> 00:00:04,000\nB");
        Assert.Equal(2, s.Paragraphs.Count);
        Assert.Equal("A" + Environment.NewLine + line, s.Paragraphs[0].Text);
    }

    [Fact]
    public void TwoDigitMillisecondsAreFrames()
    {
        var s = Load("1\n00:00:01,12 --> 00:00:02,24\nA\n\n2\n00:00:03,00 --> 00:00:04,12\nB");
        var expected = SubtitleFormat.FramesToMillisecondsMax999(12);
        Assert.Equal(expected, s.Paragraphs[0].StartTime.Milliseconds);
    }

    [Fact]
    public void ControlCharacters()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\n\u007FHello\0there\u007F\nSecond\0\n");
        Assert.Equal("Hello there" + Environment.NewLine + "Second", s.Paragraphs[0].Text);
    }

    [Fact]
    public void WsrtItalic()
    {
        var s = Load("1\n00:00:01,000 --> 00:00:02,000\n<31>Hi</31>", "test.wsrt");
        Assert.Equal("<i>Hi</i>", s.Paragraphs[0].Text);
    }

    [Fact]
    public void SingleUntouchedLineIsNotCopied()
    {
        var lines = new List<string> { "1", "00:00:01,000 --> 00:00:02,000", "Hello" };
        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, lines, "a.srt");
        Assert.Same(lines[2], subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void BrokenTimeCodeDropsItsParagraph()
    {
        var format = new SubRip();
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, "1\n00:00:01,000 --> 00:00:02,000\nA\n\n2\n00:00:xx,000 --> 00:00:04,000\nB\n\n3\n00:00:05,000 --> 00:00:06,000\nC".SplitToLines(), "a.srt");
        Assert.Equal(new[] { "A", "C" }, subtitle.Paragraphs.Select(p => p.Text));
        Assert.Equal(2, format.ErrorCount);
        Assert.Contains("00:00:xx,000", format.Errors);
    }

    [Fact]
    public void IsMine()
    {
        Assert.True(new SubRip().IsMine("1\n00:00:01,000 --> 00:00:02,000\nA".SplitToLines(), "a.srt"));
        Assert.False(new SubRip().IsMine("WEBVTT\n\n00:00:01.000 --> 00:00:02.000\nA".SplitToLines(), "a.vtt"));
        Assert.False(new SubRip().IsMine("{1}{25}Hello|World\n{30}{50}Bye".SplitToLines(), "a.sub"));
        Assert.False(new SubRip().IsMine("Just\nsome\nplain\ntext".SplitToLines(), "a.txt"));
        Assert.False(new SubRip().IsMine(new List<string>(), "a.srt"));
    }

    [Fact]
    public void ToTextRoundTrip()
    {
        const string input = "1\r\n00:00:01,000 --> 00:00:02,500\r\nHello\r\nWorld\r\n\r\n2\r\n01:02:03,004 --> 99:59:59,999\r\n<i>Bye</i>\r\n\r\n";
        var s = Load(input);
        var text = new SubRip().ToText(s, "title");
        Assert.Equal(input.Replace("\r\n", Environment.NewLine), text);
        Assert.Equal(new SubRipLegacy().ToText(s, "title"), text);
    }

    [Fact]
    public void ToTextNegativeTimeCodeMatchesLegacy()
    {
        var s = new Subtitle();
        s.Paragraphs.Add(new Paragraph("A", -1500, 500) { Number = 1 });
        Assert.Equal(new SubRipLegacy().ToText(s, "t"), new SubRip().ToText(s, "t"));
    }

    /// <summary>
    /// Well-formed and lightly damaged files must load exactly like the old parser did.
    /// </summary>
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public void SameAsLegacy(bool messySeparators, bool crlfAndTrailingSpaces, bool noTrailingNewLine)
    {
        var texts = new[]
        {
            "It was the best of times.",
            "<i>Are you coming?</i>",
            "- No.\n- Then stay.",
            "1999 was a long time ago.",
            "Line one\nLine two\nLine three",
            "♪ Music ♪",
        };

        var sb = new StringBuilder();
        var nl = crlfAndTrailingSpaces ? "\r\n" : "\n";
        var ms = 1000;
        for (var i = 0; i < 200; i++)
        {
            var start = new TimeCode(ms).ToString();
            var end = new TimeCode(ms + 1234).ToString();
            sb.Append(i + 1).Append(nl);
            sb.Append(messySeparators ? start.Replace(',', '.') + " -> " + end.Replace(',', '.') : start + " --> " + end).Append(nl);
            foreach (var line in texts[i % texts.Length].Split('\n'))
            {
                sb.Append(line).Append(crlfAndTrailingSpaces ? "  " : "").Append(nl);
            }

            sb.Append(nl);
            ms += 2000;
        }

        var input = sb.ToString();
        if (noTrailingNewLine)
        {
            input = input.TrimEnd();
        }

        AssertSameAsLegacy(input.SplitToLines());
    }

    [Theory]
    [InlineData("auto_detect_Danish.srt")]
    [InlineData("auto_detect_Russian.srt")]
    [InlineData("auto_detect_windows-1250.srt")]
    [InlineData("auto_detect_windows-1251.srt")]
    [InlineData("auto_detect_windows-1254.srt")]
    public void SameAsLegacyRealFiles(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Files", fileName);
        if (!File.Exists(path))
        {
            path = Path.Combine("Files", fileName);
        }

        var lines = File.ReadAllText(path, Encoding.UTF8).SplitToLines();
        AssertSameAsLegacy(lines);
    }

    private static void AssertSameAsLegacy(List<string> lines)
    {
        var expected = new Subtitle();
        new SubRipLegacy().LoadSubtitle(expected, lines, "a.srt");
        var actual = new Subtitle();
        new SubRip().LoadSubtitle(actual, lines, "a.srt");

        Assert.True(expected.Paragraphs.Count > 0);
        Assert.Equal(expected.Paragraphs.Count, actual.Paragraphs.Count);
        for (var i = 0; i < expected.Paragraphs.Count; i++)
        {
            var e = expected.Paragraphs[i];
            var a = actual.Paragraphs[i];
            Assert.Equal(e.Number, a.Number);
            Assert.Equal(e.StartTime.TotalMilliseconds, a.StartTime.TotalMilliseconds);
            Assert.Equal(e.EndTime.TotalMilliseconds, a.EndTime.TotalMilliseconds);
            Assert.Equal(e.Text, a.Text);
        }
    }
}
