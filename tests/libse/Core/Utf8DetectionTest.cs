using Nikse.SubtitleEdit.Core.Common;
using System.Text;

namespace LibSETests.Core;

/// <summary>
/// The UTF-8 check skipped the last three bytes, so an 8-bit file whose only accented letter was
/// the last one before the final line break ("...café\r\n") was opened as UTF-8.
/// </summary>
public class Utf8DetectionTest
{
    private const string Srt = "1\r\n00:00:01,000 --> 00:00:02,000\r\nOn va au café";

    private static Encoding Detect(byte[] bytes)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, bytes);
            return LanguageAutoDetect.GetEncodingFromFile(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("\r\n")]
    [InlineData("\n")]
    [InlineData("")]
    public void AnsiFileWithAccentAtTheEndIsNotUtf8(string ending)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = Srt + ending;
        var bytes = Encoding.GetEncoding(1252).GetBytes(text);

        var encoding = Detect(bytes);

        Assert.NotEqual(Encoding.UTF8.CodePage, encoding.CodePage);
        Assert.Equal(text, encoding.GetString(bytes));
    }

    [Theory]
    [InlineData("On va au café")]
    [InlineData("日本語")]
    [InlineData("OK 😀")]
    public void Utf8FileWithMultiByteLetterAtTheEndIsUtf8(string lastLine)
    {
        var bytes = Encoding.UTF8.GetBytes("1\r\n00:00:01,000 --> 00:00:02,000\r\n" + lastLine);

        Assert.Equal(Encoding.UTF8.CodePage, Detect(bytes).CodePage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void LargeUtf8FileCutInsideALetterIsUtf8(int padding)
    {
        // Only the first 500,000 bytes are read, which may end inside a three-byte letter.
        var sb = new StringBuilder(new string('x', padding));
        for (var i = 1; sb.Length < 400_000; i++) // about 590 KB
        {
            sb.Append($"{i}\r\n00:00:01,000 --> 00:00:02,000\r\n日本語のテキストです。\r\n\r\n");
        }

        Assert.Equal(Encoding.UTF8.CodePage, Detect(Encoding.UTF8.GetBytes(sb.ToString())).CodePage);
    }
}
