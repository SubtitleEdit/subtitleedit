using Nikse.SubtitleEdit.Core.Common;
using System.Text;
using Nikse.SubtitleEdit.Logic;

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
}
