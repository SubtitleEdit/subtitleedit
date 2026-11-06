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
    public void AutoDetectCodePage1254()
    {
        // Issue #10095: Turkish subtitle saved as Windows-1254 was reopened as 1252.
        var encoding = DetectAnsiEncoding("auto_detect_windows-1254.srt");
        Assert.Equal(1254, encoding.CodePage);
    }

    [Fact]
    public void AutoDetectCodePage1251()
    {
        var encoding = DetectAnsiEncoding("auto_detect_windows-1251.srt");
        Assert.Equal(1251, encoding.CodePage);
    }

    [Fact]
    public void AutoDetectCodePageShiftJis()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(932).GetBytes("これは日本語の字幕です。よろしくお願いします。ありがとうございました。");
        Assert.Equal(932, LanguageAutoDetect.DetectAnsiEncoding(bytes).CodePage);
    }

    [Fact]
    public void AutoDetectCodePageGbk()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(936).GetBytes("我们这个国家的时间很好，请来学习中文，说明一下为什么。");
        Assert.Equal(936, LanguageAutoDetect.DetectAnsiEncoding(bytes).CodePage);
    }

    [Fact]
    public void AutoDetectCodePage1252ItalianNotShiftJis()
    {
        // Issue #16678: invalid Shift_JIS bytes decode as U+30FB (a kana), so Italian 1252 text was read as Japanese.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(1252).GetBytes(
            "1\r\n00:00:01,000 --> 00:00:03,000\r\nPerché sei qui? È così difficile.\r\n\r\n" +
            "2\r\n00:00:04,000 --> 00:00:06,000\r\nLì c'è già un uomo, lui è là.\r\n\r\n" +
            "3\r\n00:00:07,000 --> 00:00:09,000\r\nCosì è, perciò andrò lì e poi là.\r\n\r\n" +
            "4\r\n00:00:10,000 --> 00:00:12,000\r\nSì, c'è più gente qui. Cos'è? Lì!\r\n");
        var codePage = LanguageAutoDetect.DetectAnsiEncoding(bytes).CodePage;
        Assert.NotEqual(932, codePage);
        Assert.NotEqual(936, codePage);
        Assert.NotEqual(950, codePage);
    }

    [Fact]
    public void AutoDetectCodePageShiftJisWithMiddleDot()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(932).GetBytes("これは日本語の字幕です・よろしくお願いします・ありがとうございました。");
        Assert.Equal(932, LanguageAutoDetect.DetectAnsiEncoding(bytes).CodePage);
    }

    private static string BuildSrt(int cueCount, params string[] lines)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < cueCount; i++)
        {
            sb.Append($"{i + 1}\r\n00:00:{i % 60:00},000 --> 00:00:{i % 60:00},900\r\n{lines[i % lines.Length]}\r\n\r\n");
        }

        return sb.ToString();
    }

    [Theory]
    [InlineData(932, 40, new[] { "ここで何をしているのか分かりません", "明日そのことについて話せますか？", "彼女はずっと前に家に帰った", "なぜか分からない" })] // was Greek: short Greek words match Shift_JIS bytes read as 1253
    [InlineData(874, 8, new[] { "ฉันไม่รู้ว่าคุณทำอะไรที่นี่ เราคุยเรื่องนี้พรุ่งนี้ได้ไหม นี่ไม่ดีเลย เธอกลับบ้านไปนานแล้ว เราต้องไปเดี๋ยวนี้ เขาพูดอะไรกับคุณ ฉันไม่เข้าใจว่าทำไม ขอบคุณมาก" })] // was Greek
    [InlineData(949, 3, new[] { "여기서 뭐 하는지 모르겠어요.", "내일 다시 만나요.", "정말 미안해요." })] // was GBK
    [InlineData(950, 12, new[] { "你是誰？我只是路過的。", "老師，請幫幫我。", "沒關係，別擔心。" })] // Big5 must not become Korean
    [InlineData(1257, 12, new[] { "Kes sa oled? Ma lihtsalt möödusin siit.", "Õpetaja, palun aita mind.", "Ära muretse, kõik on korras." })] // Estonian, was 1250
    [InlineData(1257, 3, new[] { "Vai mēs varam par to runāt rīt?", "Mums tagad jāiet.", "Es tikai gāju garām." })] // Latvian, was 1252
    [InlineData(1252, 8, new[] { "No sé què estàs fent aquí.", "Això no és gaire bo.", "És veritat." })] // Catalan, was 1250
    [InlineData(1252, 3, new[] { "Chi sei tu? Stavo solo passando di qui.", "Non lo sapevo.", "La stazione è a dieci minuti a piedi." })] // small file: the fallback must not be UTF-8
    [InlineData(1252, 12, new[] { "Nuk e di çfarë po bën këtu.", "A mund të flasim nesër për këtë?", "Kjo nuk është mirë." })] // Albanian, was ISO-8859-13
    [InlineData(1252, 9, new[] { "Má ég fá meira kaffi?", "Þetta er ekki gott.", "Hvað sagði hann við þig?", "Getum við talað um þetta á morgun?", "Það er enginn á skrifstofunni." })] // Icelandic: the fallback detector says ISO-8859-13 for some such files
    [InlineData(1250, 12, new[] { "Nu ştiu ce faci aici.", "Putem vorbi despre asta mâine?", "Trebuie să plecăm acum.", "Mulţumesc frumos." })] // Romanian, was UTF-8 or Big5
    [InlineData(1256, 40, new[] { "لا أعرف ماذا تفعل هنا؟", "هل يمكننا التحدث عن هذا غدا؟", "ماذا قال لك؟", "شكرا جزيلا، يا صديقي." })] // Arabic ، and ؟ are ¡ and ¿ in 1252, was taken as Spanish
    public void AutoDetectAnsiEncodingKeepsText(int codePage, int cueCount, string[] lines)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = BuildSrt(cueCount, lines);
        var bytes = Encoding.GetEncoding(codePage).GetBytes(text);
        var encoding = LanguageAutoDetect.DetectAnsiEncoding(bytes);
        Assert.Equal(text, encoding.GetString(bytes));
    }
}
