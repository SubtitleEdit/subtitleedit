using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// Guard tests for defects found on three axes (2026-08-27 bug hunt):
/// format DETECTION (which format claims a file, and does IsMine ever throw),
/// reader ROBUSTNESS against truncated/damaged input, and writer/reader agreement.
/// IsMine runs for every known format when a file is opened, so a throw there used to take
/// down the whole open.
/// </summary>
public class FormatRobustnessBugsTest
{
    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world.", 2000, 4000));
        subtitle.Paragraphs.Add(new Paragraph("Second line here," + Environment.NewLine + "with a line break.", 6000, 9000));
        subtitle.Paragraphs.Add(new Paragraph("Third line of the test.", 12000, 15000));
        subtitle.Paragraphs.Add(new Paragraph("Fourth and last.", 18000, 21000));
        return subtitle;
    }

    private static Subtitle RoundTrip(SubtitleFormat format, Subtitle subtitle)
    {
        var text = format.ToText(subtitle, "title");
        var target = new Subtitle();
        format.LoadSubtitle(target, text.SplitToLines(), "test" + format.Extension);
        return target;
    }

    [Fact]
    public void IsMine_NeverThrows_ForAnyFormatOnAnyOtherFormatsOutput()
    {
        // The detection loop calls IsMine on every format; a json root that is an array made
        // GooglePlayJson throw InvalidCastException, and a file name that is not on disk made
        // Footage throw FileNotFoundException - both crashed opening the file.
        var subtitle = MakeSubtitle();
        var formats = SubtitleFormat.AllSubtitleFormats.Where(f => f.IsTextBased).ToList();

        foreach (var writer in formats)
        {
            string text;
            try { text = writer.ToText(subtitle, "title"); }
            catch (NotImplementedException) { continue; }
            if (string.IsNullOrWhiteSpace(text)) continue;

            var lines = text.SplitToLines();
            foreach (var reader in formats)
            {
                var exception = Record.Exception(() => reader.IsMine(lines, "probe" + writer.Extension));
                Assert.True(exception == null,
                    $"{reader.Name}.IsMine threw on {writer.Name} output: {exception}");
            }
        }
    }

    [Fact]
    public void Readers_DoNotThrow_OnTruncatedInput()
    {
        // A damaged or half-written file must read as "not mine", never throw: unterminated json
        // strings gave a negative Substring length, a json value whose opening quote was the
        // first character indexed content[-1], a file ending in a number indexed past the end,
        // and several xml readers called LoadXml (or their fallback LoadXml) unguarded.
        var subtitle = MakeSubtitle();

        foreach (var format in SubtitleFormat.AllSubtitleFormats.Where(f => f.IsTextBased))
        {
            string text;
            try { text = format.ToText(subtitle, "title"); }
            catch (NotImplementedException) { continue; }
            if (string.IsNullOrWhiteSpace(text)) continue;

            foreach (var cut in new[] { text.Length / 2, text.Length * 9 / 10 })
            {
                var damaged = text.Substring(0, Math.Max(1, cut));
                var lines = damaged.SplitToLines();

                var exception = Record.Exception(() =>
                {
                    var target = new Subtitle();
                    if (format.IsMine(lines, "probe" + format.Extension))
                    {
                        format.LoadSubtitle(target, lines, "probe" + format.Extension);
                    }
                });

                Assert.True(exception == null,
                    $"{format.Name} threw on input truncated to {cut} chars: {exception}");
            }
        }
    }

    [Fact]
    public void SeJsonParser_UnterminatedString_DoesNotThrow()
    {
        var parser = new SeJsonParser();

        Assert.Null(Record.Exception(() => parser.GetArrayElements("[{\"text\": \"unterminated")));
        Assert.Null(Record.Exception(() => parser.GetArrayElementsByName("{\"body\":[{\"content\": \"oops", "body")));
        Assert.Null(Record.Exception(() => parser.GetFirstObject("{\"start\": 1234", "start")));
    }

    [Fact]
    public void SeJsonParser_ContentEndingInNumber_DoesNotThrow()
    {
        // "while (IsNumberChar(content[i]) && i < max)" indexed before checking the bound.
        var parser = new SeJsonParser();

        Assert.Null(Record.Exception(() => parser.GetArrayElements("[{\"start\": 1234")));
    }

    [Fact]
    public void GooglePlayJson_ArrayRoot_IsNotMineInsteadOfThrowing()
    {
        var lines = "[ {\"start\": 1000, \"end\": 2000, \"text\": \"Hi\"} ]".SplitToLines();

        var format = new GooglePlayJson();

        Assert.Null(Record.Exception(() => format.IsMine(lines, "probe.json")));
    }

    [Fact]
    public void CsvDaVinci_DoesNotClaimOtherCsvDialects()
    {
        // It used to claim any csv with time columns and import every cue with blank text
        // (the text column index simply did not exist, leaving Paragraph.Text null).
        var nuendo = new CsvNuendo();
        var text = nuendo.ToText(MakeSubtitle(), "title");
        var lines = text.SplitToLines();

        Assert.False(new CsvDaVinci().IsMine(lines, "probe.csv"));
    }

    [Fact]
    public void CsvDaVinci_NeverProducesNullText()
    {
        var text = new CsvNuendo().ToText(MakeSubtitle(), "title");
        var target = new Subtitle();

        new CsvDaVinci().LoadSubtitle(target, text.SplitToLines(), "probe.csv");

        Assert.All(target.Paragraphs, p => Assert.NotNull(p.Text));
    }

    [Fact]
    public void YouTubeChapters_WritesAListItCanReadBack()
    {
        // YouTube only accepts a chapter list starting at 0:00 and LoadSubtitle enforces that,
        // but ToText exported the subtitle's own first time - a file neither YouTube nor SE
        // would accept.
        var format = new YouTubeChapters();
        var text = format.ToText(MakeSubtitle(), "title");

        Assert.True(format.IsMine(text.SplitToLines(), "probe.txt"));

        var target = RoundTrip(format, MakeSubtitle());
        Assert.NotEmpty(target.Paragraphs);
        Assert.Equal(0, target.Paragraphs[0].StartTime.TotalMilliseconds, 0);
    }

    [Fact]
    public void UnknownSubtitle61_CanReadItsOwnOutput()
    {
        // ToText wrote a second time code per cue that LoadSubtitle read as the NEXT cue's
        // start, so every exported file came back shifted - and undetectable.
        var format = new UnknownSubtitle61();
        var text = format.ToText(MakeSubtitle(), "title");

        Assert.True(format.IsMine(text.SplitToLines(), "probe.txt"));

        var target = RoundTrip(format, MakeSubtitle());
        Assert.Equal(4, target.Paragraphs.Count);
        Assert.Equal(2000, target.Paragraphs[0].StartTime.TotalMilliseconds, 0);
        Assert.Equal("Hello world.", target.Paragraphs[0].Text);
    }

    [Fact]
    public void UnknownSubtitle44_LongLine_DoesNotSwallowTheNextCue()
    {
        // Text longer than the fixed text column ran into the time code with no separator, so
        // the reader lost that cue entirely.
        var target = RoundTrip(new UnknownSubtitle44(), MakeSubtitle());

        Assert.Equal(4, target.Paragraphs.Count);
        Assert.Contains("with a line break.", target.Paragraphs[1].Text, StringComparison.Ordinal);
    }

    [Fact]
    public void MsOfficeWorkbook_KeepsTheActor()
    {
        // ToText writes the actor as the ninth column (the header row calls it "Actors"), but
        // LoadSubtitle only ever read eight columns, so the actor was lost on every load.
        var subtitle = MakeSubtitle();
        subtitle.Paragraphs[0].Actor = "NARRATOR";
        subtitle.Paragraphs[2].Actor = "Anna";

        var target = RoundTrip(new MsOfficeWorkbook(), subtitle);

        Assert.Equal(4, target.Paragraphs.Count);
        Assert.Equal("NARRATOR", target.Paragraphs[0].Actor);
        Assert.Equal("Anna", target.Paragraphs[2].Actor);
        Assert.True(string.IsNullOrEmpty(target.Paragraphs[1].Actor));
    }
}
