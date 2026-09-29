using System.IO.Compression;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats;

public class EZTitlesBinaryTest
{
    // Builds a minimal .ezt file: "EZTZ" + zlib(header + records), using the record layout
    // observed in an EZTitles 5.3 project (see EZTitlesBinary).
    private static byte[] BuildFile(params (int startFrames, int endFrames, string[] rows)[] subtitles)
    {
        var body = new List<byte>();
        body.AddRange(new byte[64]); // fake header
        body.AddRange(BitConverter.GetBytes(subtitles.Length));

        var number = 1;
        foreach (var (startFrames, endFrames, rows) in subtitles)
        {
            var rec = new List<byte>();
            rec.AddRange(BitConverter.GetBytes((short)number++));
            rec.Add(0);
            rec.AddRange(TimeCodeBytes(startFrames));
            rec.AddRange(TimeCodeBytes(endFrames));
            rec.AddRange(new byte[] { 0x0a, 0x00, 0x80, 0x01, 0x00, 0x80, 0x00 });
            rec.Add((byte)rows.Length);
            foreach (var row in rows)
            {
                rec.AddRange(new byte[] { 0xaf, 0x01, 0, 0, 0, 0, 0, 0, 0x02, 0x03 });
                rec.AddRange(BitConverter.GetBytes(row.Length));
                rec.AddRange(Encoding.UTF32.GetBytes(row));
                rec.AddRange(BitConverter.GetBytes(row.Length));
                for (var i = 0; i < row.Length; i++)
                {
                    rec.AddRange(new byte[] { 0xff, 0xff, 0xff, 0x1f, 0xff, 0xff, 0xff, 0x1f, 0x09 }); // per-char attributes
                }
            }

            body.AddRange(BitConverter.GetBytes(rec.Count));
            body.AddRange(rec);
        }

        using var ms = new MemoryStream();
        ms.Write(Encoding.ASCII.GetBytes("EZTZ"));
        using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(body.ToArray());
        }

        return ms.ToArray();
    }

    private static byte[] TimeCodeBytes(int totalFrames)
    {
        var frames = totalFrames % 30;
        var seconds = totalFrames / 30;
        return new[] { (byte)frames, (byte)(seconds % 60), (byte)(seconds / 60 % 60), (byte)(seconds / 3600) };
    }

    private static Subtitle Load(byte[] bytes)
    {
        var path = Path.GetTempFileName() + ".ezt";
        try
        {
            File.WriteAllBytes(path, bytes);
            var format = new EZTitlesBinary();
            Assert.True(format.IsMine(null!, path));
            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, null!, path);
            return subtitle;
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadsTextAndTimeCodes()
    {
        var bytes = BuildFile(
            (0, 50, new[] { "My name is Carey Newman." }),
            (53, 149, new[] { "My traditional name", "is Hayalthkin’geme." })); // 00:00:04:29

        var subtitle = Load(bytes);

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("My name is Carey Newman.", subtitle.Paragraphs[0].Text);
        Assert.Equal("My traditional name" + Environment.NewLine + "is Hayalthkin’geme.", subtitle.Paragraphs[1].Text);
        // Frame numbers up to 29 -> 29.97 fps time codes
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(1000 + SubtitleFormat.FramesToMilliseconds(20, 29.97), subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(4000 + SubtitleFormat.FramesToMilliseconds(29, 29.97), subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    // Timing templates have subtitles without any text row; the reader locates the record
    // list from the first text row, so leading empty records must be chained back to.
    [Fact]
    public void KeepsEmptySubtitlesBeforeFirstText()
    {
        var bytes = BuildFile(
            (30, 60, new string[0]),
            (90, 120, new string[0]),
            (150, 180, new[] { "ON-SCREEN" }),
            (210, 240, new string[0]));

        var subtitle = Load(bytes);

        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(string.Empty, subtitle.Paragraphs[0].Text);
        Assert.Equal("ON-SCREEN", subtitle.Paragraphs[2].Text);
        Assert.Equal(string.Empty, subtitle.Paragraphs[3].Text);
    }

    // Per-character attributes as written by EZTitles: style flags (0x02 = italic), RGB colour
    // and 0x1f for "default colour", background, then 4 more bytes.
    private static byte[] Attributes(bool italic, byte r = 0xff, byte g = 0xff, byte b = 0xff, bool defaultColor = true) =>
        new byte[] { (byte)(italic ? 0x02 : 0x00), r, g, b, (byte)(defaultColor ? 0x1f : 0x00), 0xff, 0xff, 0xff, 0x1f, 0x09, 0x08, 0x00, 0x00 };

    private static byte[] BuildFormattedFile(params (byte verticalAlign, byte justification, string text, Func<int, byte[]> attributes)[] subtitles)
    {
        var body = new List<byte>();
        body.AddRange(new byte[64]); // fake header
        body.AddRange(BitConverter.GetBytes(subtitles.Length));

        foreach (var (verticalAlign, justification, text, attributes) in subtitles)
        {
            var rec = new List<byte>();
            rec.AddRange(BitConverter.GetBytes((short)1));
            rec.Add(0x61);
            rec.AddRange(new byte[] { 0x00, 0x00, 0xff, 0xff }); // unset time codes
            rec.AddRange(new byte[] { 0x00, 0x00, 0xff, 0xff });
            rec.AddRange(new byte[] { 0x0b, 0x00, 0x80, 0x01, 0x80, 0x80, verticalAlign });
            rec.Add(1);
            rec.AddRange(new byte[] { 0x5a, 0x01, 0, 0, 0, 0, 0, 0, justification, 0x03 });
            rec.AddRange(BitConverter.GetBytes(text.Length));
            rec.AddRange(Encoding.UTF32.GetBytes(text));
            rec.AddRange(BitConverter.GetBytes(text.Length));
            rec.AddRange(BitConverter.GetBytes(13));
            for (var i = 0; i < text.Length; i++)
            {
                rec.AddRange(attributes(i));
            }

            rec.AddRange(new byte[40]); // trailing record data
            body.AddRange(BitConverter.GetBytes(rec.Count));
            body.AddRange(rec);
        }

        using var ms = new MemoryStream();
        ms.Write(Encoding.ASCII.GetBytes("EZTZ"));
        using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(body.ToArray());
        }

        return ms.ToArray();
    }

    [Fact]
    public void LoadsItalicColorAndAlignment()
    {
        var bytes = BuildFormattedFile(
            (0, 2, "Red italic", _ => Attributes(true, 0xff, 0x00, 0x00, false)),
            (2, 2, "Top", _ => Attributes(false, defaultColor: false)), // explicit white = no tag
            (2, 0, "Top left", _ => Attributes(false)),
            (0, 1, "Bottom right", _ => Attributes(false, 0xff, 0xff, 0x00, false)),
            (0, 2, "Only THIS in italic", i => Attributes(i >= 5 && i < 9)));

        var subtitle = Load(bytes);

        Assert.Equal(5, subtitle.Paragraphs.Count);
        Assert.Equal("<font color=\"#ff0000\"><i>Red italic</i></font>", subtitle.Paragraphs[0].Text);
        Assert.Equal("{\\an8}Top", subtitle.Paragraphs[1].Text);
        Assert.Equal("{\\an7}Top left", subtitle.Paragraphs[2].Text);
        Assert.Equal("{\\an3}<font color=\"#ffff00\">Bottom right</font>", subtitle.Paragraphs[3].Text);
        Assert.Equal("Only <i>THIS</i> in italic", subtitle.Paragraphs[4].Text);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(0, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void IsMineRejectsOtherFiles()
    {
        var path = Path.GetTempFileName() + ".ezt";
        try
        {
            File.WriteAllText(path, "1\n00:00:00,000 --> 00:00:01,000\nHello\n");
            Assert.False(new EZTitlesBinary().IsMine(null!, path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
