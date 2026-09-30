using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace LibSETests.Core;

/// <summary>
/// UTF-16 text without a byte order mark was read as UTF-8 with a NUL between every character,
/// so no format recognised it ("romania sub v3.srt" was big endian with no BOM).
/// </summary>
public class Utf16WithoutByteOrderMarkTest
{
    private const string Srt = "1\r\n00:00:00,000 --> 00:00:03,000\r\nNoris: So this is a timer? No, it’s the date?\r\n\r\n" +
                               "2\r\n00:00:51,000 --> 00:00:56,000\r\nMax: Yes, I think it’s the amount of time.\r\n\r\n";

    [Fact]
    public void BigAndLittleEndianAreDetected()
    {
        Assert.Equal(Encoding.BigEndianUnicode, LanguageAutoDetect.GetUtf16WithoutByteOrderMark(Encoding.BigEndianUnicode.GetBytes(Srt)));
        Assert.Equal(Encoding.Unicode, LanguageAutoDetect.GetUtf16WithoutByteOrderMark(Encoding.Unicode.GetBytes(Srt)));
    }

    [Fact]
    public void EightBitAndUtf8TextIsNotUtf16()
    {
        Assert.Null(LanguageAutoDetect.GetUtf16WithoutByteOrderMark(Encoding.UTF8.GetBytes(Srt)));
        Assert.Null(LanguageAutoDetect.GetUtf16WithoutByteOrderMark(Encoding.Latin1.GetBytes(Srt.Replace('’', '\''))));
        Assert.Null(LanguageAutoDetect.GetUtf16WithoutByteOrderMark(new byte[10])); // too short to tell
    }

    [Fact]
    public void BigEndianSrtWithoutBomOpens()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, Encoding.BigEndianUnicode.GetBytes(Srt));

            var encoding = LanguageAutoDetect.GetEncodingFromFile(path);
            var subtitle = Subtitle.Parse(path, encoding);

            Assert.Equal(Encoding.BigEndianUnicode, encoding);
            Assert.NotNull(subtitle);
            Assert.IsType<SubRip>(subtitle.OriginalFormat);
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Max: Yes, I think it’s the amount of time.", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
