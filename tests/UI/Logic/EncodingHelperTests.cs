using Nikse.SubtitleEdit.Core.Common;
using System.Text;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

public class EncodingHelperTests
{
    private const string Text = "1\r\n00:00:01,000 --> 00:00:02,000\r\nHello there, how are you? Æblegrød\r\n";
    private const string AsciiText = "1\r\n00:00:01,000 --> 00:00:02,000\r\nHello there, how are you?\r\n";

    private static bool ResolvedSourceEncodingWritesBom(byte[] sourceBytes)
    {
        var sourceFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(sourceFile, sourceBytes);
            var encoding = EncodingHelper.ResolveEncoding(EncodingHelper.TryToUseSourceEncoding, sourceFile);
            Assert.Equal(Encoding.UTF8.CodePage, encoding.CodePage);
            return encoding.GetPreamble().Length > 0;
        }
        finally
        {
            File.Delete(sourceFile);
        }
    }

    [Fact]
    public void TryToUseSourceEncoding_Utf8WithoutBom_KeepsNoBom()
    {
        Assert.False(ResolvedSourceEncodingWritesBom(new UTF8Encoding(false).GetBytes(Text)));
    }

    [Fact]
    public void TryToUseSourceEncoding_Utf8WithBom_KeepsBom()
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Text)).ToArray();
        Assert.True(ResolvedSourceEncodingWritesBom(bytes));
    }

    [Fact]
    public void TryToUseSourceEncoding_Ascii_WritesNoBom()
    {
        Assert.False(ResolvedSourceEncodingWritesBom(Encoding.ASCII.GetBytes(AsciiText)));
    }

    [Fact]
    public void Utf8WithBom_WritesBom()
    {
        Assert.NotEmpty(EncodingHelper.ResolveEncoding(TextEncoding.Utf8WithBom, null).GetPreamble());
    }

    [Fact]
    public void Utf8WithoutBom_WritesNoBom()
    {
        Assert.Empty(EncodingHelper.ResolveEncoding(TextEncoding.Utf8WithoutBom, null).GetPreamble());
    }

    private static Encoding ResolveSourceEncoding(byte[] sourceBytes)
    {
        var sourceFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(sourceFile, sourceBytes);
            return EncodingHelper.ResolveEncoding(EncodingHelper.TryToUseSourceEncoding, sourceFile);
        }
        finally
        {
            File.Delete(sourceFile);
        }
    }

    [Fact]
    public void TryToUseSourceEncoding_BinarySource_UsesBinarySourceEncoding()
    {
        // e.g. a .sup/.mkv - an EBML-like header with NUL bytes, followed by bytes the ANSI guesser would like
        var bytes = new byte[] { 0x1a, 0x45, 0xdf, 0xa3, 0x00, 0x00, 0x00, 0x01 }
            .Concat(Encoding.Latin1.GetBytes(Text)).ToArray();

        var encoding = ResolveSourceEncoding(bytes);

        var expected = new UTF8Encoding(true);
        Assert.Equal(Encoding.UTF8.CodePage, encoding.CodePage);
        Assert.Equal(expected.GetPreamble(), encoding.GetPreamble());
    }

    [Fact]
    public void TryToUseSourceEncoding_Utf16Source_KeepsUtf16()
    {
        var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(Text)).ToArray();
        Assert.Equal(Encoding.Unicode.CodePage, ResolveSourceEncoding(bytes).CodePage);
    }

    [Theory]
    [InlineData("UTF-8 with BOM", true)]
    [InlineData("UTF-8 without BOM", false)]
    [InlineData("windows-1252", true)]
    [InlineData(null, true)]
    public void BinarySourceEncoding_IsDefaultWhenUtf8_ElseUtf8WithBom(string? defaultEncoding, bool expectBom)
    {
        var encoding = EncodingHelper.GetBinarySourceEncoding(defaultEncoding);
        Assert.Equal(Encoding.UTF8.CodePage, encoding.CodePage);
        Assert.Equal(expectBom, encoding.GetPreamble().Length > 0);
    }
}
