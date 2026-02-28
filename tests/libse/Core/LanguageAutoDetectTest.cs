using Nikse.SubtitleEdit.Core.Common;
using System.Text;

namespace LibSETests.Core;

public class LanguageAutoDetectTest
{
    private static string GetLanguageCode(string fileName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
        var sub = new Subtitle();
        sub.LoadSubtitle(fileName, out _, null);
        return LanguageAutoDetect.AutoDetectGoogleLanguage(sub);
    }

    [Fact]
    
    public void AutoDetectRussian()
    {
        var languageCode = GetLanguageCode("auto_detect_Russian.srt");
        Assert.Equal("ru", languageCode);
    }

    [Fact]
    
    public void AutoDetectDanish()
    {
        var languageCode = GetLanguageCode("auto_detect_Danish.srt");
        Assert.Equal("da", languageCode);
    }

    private static Encoding DetectAnsiEncoding(string fileName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        fileName = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
        return LanguageAutoDetect.DetectAnsiEncoding(FileUtil.ReadAllBytesShared(fileName));
    }

    [Fact]
    public void AutoDetectCodePage1250()
    {
        var encoding = DetectAnsiEncoding("auto_detect_windows-1250.srt");
        Assert.Equal(1250, encoding.CodePage);
    }

    [Fact]
    public void AutoDetectCodePage1251()
    {
        var encoding = DetectAnsiEncoding("auto_detect_windows-1251.srt");
        Assert.Equal(1251, encoding.CodePage);
    }
}
