using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// sample_SSTG1.sdb is a real SSTG1Pro project (Jet 4 database, 29.97 drop frame, two timecode
/// segments) whose subtitle rows were replaced by synthetic ones: plain and compressed Unicode memos,
/// single-page and multi-page long-value memos, a second track, an empty row, a row with only the
/// original-language text, and a deleted row. The Format table still carries the italic and ruby
/// entries for cue ids 402, 418, 198 and 210.
/// </summary>
public class CanvassSstg1Test
{
    private static string FixturePath => Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_SSTG1.sdb");

    private const long Segment1BaseFrames = 106848; // programme timecode at media frame 1
    private const long Segment2BaseFrames = 123034; // 01:08:25;08 drop frame, at media frame 16186

    private static Subtitle Load()
    {
        var subtitle = new Subtitle();
        new CanvassSstg1().LoadSubtitle(subtitle, new List<string>(), FixturePath);
        return subtitle;
    }

    [Fact]
    public void IsMineRecognizesSdb()
    {
        Assert.True(new CanvassSstg1().IsMine(new List<string>(), FixturePath));
        Assert.False(new CanvassSstg1().IsMine(new List<string>(), Path.Combine(Directory.GetCurrentDirectory(), "Files", "auto_detect_Danish.srt")));
    }

    [Fact]
    public void LoadsRowsInTimeOrderAndSkipsEmptyAndDeletedRows()
    {
        var subtitle = Load();

        Assert.Equal(7, subtitle.Paragraphs.Count);
        Assert.Equal("最後に追加", subtitle.Paragraphs[0].Text); // highest id, earliest frame
        Assert.DoesNotContain(subtitle.Paragraphs, p => p.Text.Contains("削除済み"));
        for (var i = 1; i < subtitle.Paragraphs.Count; i++)
        {
            Assert.True(subtitle.Paragraphs[i - 1].StartTime.TotalMilliseconds <= subtitle.Paragraphs[i].StartTime.TotalMilliseconds);
        }
    }

    [Fact]
    public void TimesComeFromTheTimecodeSegments()
    {
        var subtitle = Load();

        var first = subtitle.Paragraphs.Single(p => p.Text == "テスト　１行目"); // media frames 2438-2523
        Assert.Equal(SubtitleFormat.FramesToMilliseconds(Segment1BaseFrames + 2438 - 1, 29.97), first.StartTime.TotalMilliseconds);
        Assert.Equal(SubtitleFormat.FramesToMilliseconds(Segment1BaseFrames + 2523 - 1, 29.97), first.EndTime.TotalMilliseconds);

        var secondSegment = subtitle.Paragraphs.Single(p => p.Text == "オリジナルのみ"); // media frame 16186 = segment 2 start
        Assert.Equal(SubtitleFormat.FramesToMilliseconds(Segment2BaseFrames, 29.97), secondSegment.StartTime.TotalMilliseconds);
        Assert.Equal(4105234, (int)secondSegment.StartTime.TotalMilliseconds); // 123034 frames at 30000/1001
    }

    [Fact]
    public void FallsBackToOriginalTextWhenTranslationIsEmpty()
    {
        var subtitle = Load();

        Assert.Contains(subtitle.Paragraphs, p => p.Text == "オリジナルのみ");
    }

    [Fact]
    public void ItalicFormatEntriesBecomeItalicTags()
    {
        var subtitle = Load();

        Assert.Contains(subtitle.Paragraphs, p => p.Text == "<i>こんにちは！</i>");
        Assert.Contains(subtitle.Paragraphs, p => p.Text == "<i>あいうえおか" + Environment.NewLine + "きくけこさしすせそた</i>");
        Assert.Contains(subtitle.Paragraphs, p => p.Text == "<i>スタッフぼしゅう中</i>"); // track 1
    }

    [Fact]
    public void RubyFormatEntriesBecomeRubyContainers()
    {
        var subtitle = Load();

        var expected = "きょうは<ruby-container><ruby-base>訛</ruby-base><ruby-text>なま</ruby-text></ruby-container>りだ" + Environment.NewLine + "二行目";
        Assert.Contains(subtitle.Paragraphs, p => p.Text == expected);
    }

    [Fact]
    public void JetReaderReadsCatalogAndTypedColumns()
    {
        var db = new JetDatabaseReader(File.ReadAllBytes(FixturePath));

        Assert.False(db.IsEncrypted);
        var tables = db.GetTables();
        Assert.Contains("Track", tables.Keys);
        Assert.Contains("Format", tables.Keys);
        Assert.Contains("Globals", tables.Keys);
        Assert.Contains("Timecodes", tables.Keys);
        Assert.DoesNotContain("MSysObjects", tables.Keys);

        var globals = Assert.Single(db.ReadTable("Globals"));
        Assert.Equal("ＭＳ ゴシック", globals["strGlobFontName"]);
        Assert.Equal(true, globals["bDropFrame"]);
        Assert.Equal(0, globals["iFrameType"]);
        Assert.Equal(1920.0, globals["iSourceWidth"]);
        Assert.Equal(string.Empty, globals["strGlobPresetName"]); // present but zero-length text

        var version = Assert.Single(db.ReadTable("Version"));
        Assert.Equal(3003, version["Version"]);
        Assert.Equal(7, version["Revision"]);

        var timecodes = db.ReadTable("Timecodes");
        Assert.Equal(2, timecodes.Count);
        Assert.Equal(1, timecodes[0]["iStartFrame"]);
        Assert.Equal((int)Segment1BaseFrames, timecodes[0]["iBaseFrames"]);
        Assert.Equal(16186, timecodes[1]["iStartFrame"]);
        Assert.Equal((int)Segment2BaseFrames, timecodes[1]["iBaseFrames"]);

        Assert.Empty(db.ReadTable("NoSuchTable"));
    }

    [Fact]
    public void DecodesCompressedUnicode()
    {
        // FF FE prefix, "AB" in one-byte mode, 0x00 switches to two-byte mode, then U+3042 (あ)
        var bytes = new byte[] { 0xFF, 0xFE, 0x41, 0x42, 0x00, 0x42, 0x30 };
        Assert.Equal("ABあ", JetDatabaseReader.DecodeText(bytes, 0, bytes.Length));

        var plain = System.Text.Encoding.Unicode.GetBytes("テスト");
        Assert.Equal("テスト", JetDatabaseReader.DecodeText(plain, 0, plain.Length));
    }
}
