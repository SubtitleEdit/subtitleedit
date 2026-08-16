using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;

namespace LibSETests.Core.ContainerFormats.Mp4;

/// <summary>
/// A text track's edit list (edts/elst) maps its media timeline onto the presentation
/// timeline, so a track written with e.g. "MP4Box -add subs.srt:delay=5000" carries an
/// empty edit that every player honours. Cue times have to move with it.
/// </summary>
public class Mp4EditListTest
{
    private const uint MovieTimeScale = 600;
    private const uint MediaTimeScale = 1000;

    [Fact]
    public void GetParagraphs_EmptyEditEntry_DelaysCues()
    {
        // 3000 movie ticks at 600/s = 5 seconds of empty edit
        var paragraphs = ParseWithEditList(EmptyEdit(3000), NormalEdit(9000, 0));

        Assert.Equal(3, paragraphs.Count);
        Assert.Equal(5000, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(6000, paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal(7000, paragraphs[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(9000, paragraphs[2].StartTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void GetParagraphs_MediaStartTime_MovesCuesEarlier()
    {
        // media_time 2000 at 1000/s = skip the first 2 seconds of media, which is
        // exactly the first cue - it is never presented, so it is dropped
        var paragraphs = ParseWithEditList(NormalEdit(4800, 2000));

        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("Second", paragraphs[0].Text);
        Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(1000, paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal("Third", paragraphs[1].Text);
        Assert.Equal(2000, paragraphs[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(3000, paragraphs[1].EndTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void GetParagraphs_MediaStartTimePastACue_DropsThatCue()
    {
        // Skipping 3 seconds of media leaves only the last of the three cues - the
        // second one ends exactly at the edit start, so it never shows either
        var paragraphs = ParseWithEditList(NormalEdit(1800, 3000));

        Assert.Single(paragraphs);
        Assert.Equal("Third", paragraphs[0].Text);
        Assert.Equal(1000, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(2000, paragraphs[0].EndTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void GetParagraphs_IdentityEditList_LeavesCuesAlone()
    {
        var paragraphs = ParseWithEditList(NormalEdit(3600, 0));

        Assert.Equal(3, paragraphs.Count);
        Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(2000, paragraphs[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(4000, paragraphs[2].StartTime.TotalMilliseconds, 1);
    }

    [Fact]
    public void GetParagraphs_NoEditList_LeavesCuesAlone()
    {
        var paragraphs = ParseWithEditList();

        Assert.Equal(3, paragraphs.Count);
        Assert.Equal(0, paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(2000, paragraphs[1].StartTime.TotalMilliseconds, 1);
        Assert.Equal(4000, paragraphs[2].StartTime.TotalMilliseconds, 1);
    }

    /// <summary>
    /// MP4Box writes whatever "lang=" it was given, so a French track is as likely to be
    /// tagged with the ISO 639-2/B code "fre" as with the 639-2/T code "fra".
    /// </summary>
    [Theory]
    [InlineData("fra", "French")]
    [InlineData("fre", "French")]
    [InlineData("deu", "German")]
    [InlineData("ger", "German")]
    [InlineData("nld", "Dutch")]
    [InlineData("dut", "Dutch")]
    [InlineData("eng", "English")]
    [InlineData("zzz", "Any")]
    public void LanguageString_BibliographicOrTerminologyCode_ResolvesToEnglishName(string code, string expected)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildTx3gMp4(PackLanguage(code)));
            var parser = new MP4Parser(tempFile);
            Assert.Equal(expected, parser.GetSubtitleTracks()[0].Mdia.Mdhd.LanguageString);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// QuickTime's "unspecified language" is 0x7FFF, which ffmpeg writes on every track of a
    /// .mov. Unpacked as a language code it is three DEL characters, which used to reach the
    /// track picker and (via seconv) output file names.
    /// </summary>
    [Theory]
    [InlineData(0x7FFF)] // QuickTime "unspecified"
    [InlineData(0x0000)] // all-zero packing unpacks to three backticks
    public void Iso639ThreeLetterCode_UnspecifiedLanguage_IsEmptyNotControlCharacters(int packedLanguage)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildTx3gMp4((ushort)packedLanguage));
            var parser = new MP4Parser(tempFile);
            var mdhd = parser.GetSubtitleTracks()[0].Mdia.Mdhd;
            Assert.Equal(string.Empty, mdhd.Iso639ThreeLetterCode);
            Assert.Equal("Any", mdhd.LanguageString);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static List<Paragraph> ParseWithEditList(params byte[][] editEntries)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, BuildTx3gMp4(0x55C4, editEntries));
            var parser = new MP4Parser(tempFile);
            return parser.GetSubtitleTracks()[0].Mdia.Minf.Stbl.GetParagraphs();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static byte[] EmptyEdit(uint segmentDurationMovieTicks)
        => Concat(UInt32Be(segmentDurationMovieTicks), UInt32Be(0xFFFFFFFF), UInt32Be(0x00010000));

    private static byte[] NormalEdit(uint segmentDurationMovieTicks, uint mediaTimeMediaTicks)
        => Concat(UInt32Be(segmentDurationMovieTicks), UInt32Be(mediaTimeMediaTicks), UInt32Be(0x00010000));

    /// <summary>Three 1-second cues at 0, 2 and 4 seconds, optionally behind an edit list.</summary>
    private static byte[] BuildTx3gMp4(ushort packedLanguage, params byte[][] editEntries)
    {
        var samples = new[] { "First", "", "Second", "", "Third", "" };
        var sampleBytes = samples.Select(s =>
        {
            var text = Encoding.UTF8.GetBytes(s);
            var buf = new byte[2 + text.Length];
            buf[0] = (byte)(text.Length >> 8);
            buf[1] = (byte)text.Length;
            System.Buffer.BlockCopy(text, 0, buf, 2, text.Length);
            return buf;
        }).ToArray();

        var ftyp = Box("ftyp", Ascii("isom"), UInt32Be(512), Ascii("isom"), Ascii("iso2"), Ascii("mp41"));
        var mdat = Box("mdat", Concat(sampleBytes));
        var sampleDataOffset = (uint)(ftyp.Length + 8);

        var stsd = Box("stsd", new byte[4], UInt32Be(1), Box("tx3g"));

        // 1 s on, 1 s off, repeated - the "off" samples are empty and get dropped
        var stts = Box("stts",
            new byte[4],
            UInt32Be(1),
            UInt32Be((uint)samples.Length),
            UInt32Be(1000));

        var stsc = Box("stsc", new byte[4], UInt32Be(1), UInt32Be(1), UInt32Be((uint)samples.Length), UInt32Be(1));
        var stsz = Box("stsz", new byte[4], UInt32Be(0), UInt32Be((uint)samples.Length),
            Concat(sampleBytes.Select(b => UInt32Be((uint)b.Length)).ToArray()));
        var stco = Box("stco", new byte[4], UInt32Be(1), UInt32Be(sampleDataOffset));

        var minf = Box("minf", Box("stbl", stsd, stts, stsc, stsz, stco));
        var hdlr = Box("hdlr", new byte[4], new byte[4], Ascii("sbtl"), new byte[12], new byte[] { 0 });
        var mdhd = Box("mdhd",
            new byte[4],
            new byte[4],
            new byte[4],
            UInt32Be(MediaTimeScale),
            UInt32Be(1000 * (uint)samples.Length),
            UInt16Be(packedLanguage),
            UInt16Be(0));

        var mdia = Box("mdia", hdlr, mdhd, minf);
        var trak = editEntries.Length == 0
            ? Box("trak", mdia)
            : Box("trak", Box("edts", Box("elst", new byte[4], UInt32Be((uint)editEntries.Length), Concat(editEntries))), mdia);

        var mvhd = Box("mvhd",
            new byte[4],
            new byte[4],
            new byte[4],
            UInt32Be(MovieTimeScale),
            UInt32Be(MovieTimeScale * 6),
            new byte[80]);

        return Concat(ftyp, mdat, Box("moov", mvhd, trak));
    }

    /// <summary>Packs a three letter code the way mdhd does - 5 bits per letter, offset from 0x60.</summary>
    private static ushort PackLanguage(string code)
        => (ushort)(((code[0] - 0x60) << 10) | ((code[1] - 0x60) << 5) | (code[2] - 0x60));

    private static byte[] Box(string name, params byte[][] parts)
    {
        var total = 8;
        foreach (var p in parts) total += p.Length;
        var box = new byte[total];
        WriteUInt32Be(box, 0, (uint)total);
        Encoding.ASCII.GetBytes(name, 0, 4, box, 4);
        var off = 8;
        foreach (var p in parts)
        {
            System.Buffer.BlockCopy(p, 0, box, off, p.Length);
            off += p.Length;
        }
        return box;
    }

    private static byte[] Concat(params byte[][] parts)
    {
        var total = 0;
        foreach (var p in parts) total += p.Length;
        var result = new byte[total];
        var off = 0;
        foreach (var p in parts)
        {
            System.Buffer.BlockCopy(p, 0, result, off, p.Length);
            off += p.Length;
        }
        return result;
    }

    private static byte[] Ascii(string s) => Encoding.ASCII.GetBytes(s);

    private static byte[] UInt32Be(uint v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };

    private static byte[] UInt16Be(ushort v) => new[] { (byte)(v >> 8), (byte)v };

    private static void WriteUInt32Be(byte[] dst, int off, uint v)
    {
        dst[off] = (byte)(v >> 24);
        dst[off + 1] = (byte)(v >> 16);
        dst[off + 2] = (byte)(v >> 8);
        dst[off + 3] = (byte)v;
    }
}
