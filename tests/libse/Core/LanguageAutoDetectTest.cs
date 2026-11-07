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
    [InlineData(51932, 8, new[] { "ここで何をしているの？", "何と言えばいいか分からない。", "今すぐ行かなければ、時間がない。", "一人で来るなと言ったでしょう。" })] // EUC-JP, was GBK
    [InlineData(20866, 8, new[] { "Что ты здесь делаешь?", "Я не знаю, что сказать.", "Нам нужно идти прямо сейчас.", "Он был здесь вчера." })] // KOI8-R, was 1250/1252
    [InlineData(866, 8, new[] { "Что ты здесь делаешь?", "Я не знаю, что сказать.", "Нам нужно идти прямо сейчас.", "Он был здесь вчера." })] // DOS, was 1251 (Serbian letters)
    [InlineData(1256, 120, new[] { "اينجا چه کار مي‌کني؟", "نمي‌دانم چه بگويم.", "بايد همين حالا برويم، وقت نداريم.", "خيلي ممنونم، دوست من." })] // Persian, was Hebrew in long files
    [InlineData(1256, 3, new[] { "تم يہاں کيا کر رہے ہو؟", "مجھے نہيں معلوم کيا کہوں.", "بہت شکريہ، دوست." })] // Urdu, was 1252
    [InlineData(1252, 1, new[] { "- Sorry isn’t enough anymore.\r\n[sighs] I know, I know." })] // only a smart quote: was Mac Central European
    [InlineData(1252, 3, new[] { "Waar is jou broer?\r\nOns moet nou gaan, daar is nie tyd nie.", "Ek weet nie wat om te sê nie.", "Ons moet nou gaan, daar is nie tyd nie." })] // Afrikaans: was Mac Central European
    public void AutoDetectAnsiEncodingKeepsText(int codePage, int cueCount, string[] lines)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = BuildSrt(cueCount, lines);
        var bytes = Encoding.GetEncoding(codePage).GetBytes(text);
        var encoding = LanguageAutoDetect.DetectAnsiEncoding(bytes);
        Assert.Equal(text, encoding.GetString(bytes));
    }

    [Fact]
    public void AutoDetectCodePageShiftJisInAssa()
    {
        // The Dialogue line prefixes and the header add many ASCII letters - real CJK files have those too.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var sb = new StringBuilder("[Script Info]\r\nScriptType: v4.00+\r\nPlayResX: 1920\r\nPlayResY: 1080\r\n\r\n[Events]\r\nFormat: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\r\n");
        var lines = new[] { "ここで何をしているのか分かりません", "明日そのことについて話せますか？", "彼女はずっと前に家に帰った", "なぜか分からない" };
        for (var i = 0; i < 20; i++)
        {
            sb.Append($"Dialogue: 0,0:00:{i:00}.00,0:00:{i:00}.90,Default,,0,0,0,,{lines[i % lines.Length]}\r\n");
        }

        var bytes = Encoding.GetEncoding(932).GetBytes(sb.ToString());
        Assert.Equal(932, LanguageAutoDetect.DetectAnsiEncoding(bytes).CodePage);
    }

    [Fact]
    public void DosCodePageFileIsNotOpenedAsUtf8()
    {
        // GetEncodingFromFile turned every code page below 874 into Encoding.Default, which is UTF-8.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var text = BuildSrt(8, "Что ты здесь делаешь?", "Я не знаю, что сказать.", "Нам нужно идти прямо сейчас.", "Он был здесь вчера.");
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, Encoding.GetEncoding(866).GetBytes(text));
            Assert.Equal(866, LanguageAutoDetect.GetEncodingFromFile(path).CodePage);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("fi", new[] { "Mikä tahansa tyhjä merkki.", "Väli on enimmäisarvo.", "Merkkien määrä muodossa.", "Lähtövideotiedostoa ei luotu." })] // was sk: Slovak counted every line with ä
    [InlineData("hr", new[] { "Što je ovo za video?", "Video je dobro, ali nije gotov.", "Pokreni video sada.", "Video ne radi kako treba." })] // was sr: "video" was a Serbian word
    [InlineData("mk", new[] { "Отворете го видеото во новиот прозорец.", "Зачувај ги промените со оваа команда.", "Изберете кој формат сакате да го користите.", "Ова е текстот кој се прикажува на екранот." })] // was sr: за, само, како, ... are Macedonian too
    [InlineData("az", new[] { "Bu faylı açmaq mümkün deyil.", "Videonu seçin və subtitr əlavə edin.", "Dəyişiklikləri yadda saxlamaq istəyirsiniz?", "Xəta baş verdi, yenidən cəhd edin." })] // was tr
    [InlineData("pl", new[] { "To jest plik, który można otworzyć.", "Wybierz wideo lub plik audio.", "Napisy są już zapisane dla tego wideo.", "Kliknij tylko raz, aby przejść dalej." })] // Polish function words
    [InlineData("ko", new[] { "클립보드에서 줄 삽입", "보통 크기로 시작하여 자막이 재생되는 동안 서서히 커짐.", "'sweet dreams are' 대신 '♫ sweet dreams are'", "선택한 줄로 이동 (목록에서 보이도록 스크롤)" })] // was en: a few English words outscored the short Korean list
    [InlineData("th", new[] { "ไปยังตำแหน่งวิดีโอ - 0.5 วินาทีและหยุดชั่วคราว", "libVLC - การเรนเดอร์ Native Window ID", "ดาวน์โหลดโมเดลหรือไม่?", "คำที่ใช้แทนต้องแตกต่างจากคำเดิม" })] // was en
    [InlineData("ckb", new[] { "فایلێک بکەرەوە", "ڕێکخستنەکان پاشەکەوت بکە", "ڤیدیۆکە لێرە دابنێ", "دەقەکە بگۆڕە" })] // Central Kurdish, was fa
    [InlineData("es", new[] { "Vídeo: alternar brillo (solo mpv)", "Añada uno o más segmentos, p. ej. desde la forma de onda", "Ancho de línea única - máximo: píxeles", "No se pudieron restaurar los ajustes:" })] // was lv: few listed Spanish words
    [InlineData("gl", new[] { "Xa che dixen que non podo ir.", "A miña nai está na cociña.", "Hoxe hai moita xente na rúa.", "Imos tomar algo despois do traballo?" })] // was pt
    [InlineData("eu", new[] { "Ez dakit zer egin behar dudan.", "Orain ez, mesedez, bihar hitz egingo dugu.", "Zergatik zaude hemen?", "Eskerrik asko laguntzeagatik." })] // was hu/fr
    [InlineData("cy", new[] { "Dw i ddim yn gwybod beth i'w wneud.", "Mae hi wedi mynd adref yn barod.", "Pam wyt ti yma nawr?", "Diolch yn fawr am dy help." })] // was no/af/en
    [InlineData("pt", new[] { "Já te disse que não posso ir.", "A minha mãe está na cozinha.", "Hoje há muita gente na rua.", "Vamos tomar alguma coisa depois do trabalho?" })] // stays Portuguese next to Galician
    [InlineData("ru", new[] { "Выделять цвета VobSub для OCR", "Введите имя базы данных", "CEA-608 в видео (SEI)", "Добавить новую строку после номера строки", "Видео: переключить контраст (только mpv)", "Лицензия на модель IndexTTS 2.5" })] // was it: Latin words in Cyrillic text
    [InlineData("hr", new[] { "Postavi linije ranije ili kasnije...", "Skrati fontove na korištene znakove...", "Dovršavanje izlazne datoteke (premještanje indeksa na početak)...", "Nije moguće dohvatiti podatke o videu", "Karaoke - iskakanje aktivne riječi" })] // was sr on a tie
    [InlineData("pl", new[] { "Nagrywanie nie powiodło się.", "Długość pojedynczej linii.", "Maksymalna liczba klatek między liniami.", "Rozmiar czcionki tekstu na przebiegu fali." })] // was nl: Afrikaans "nie"
    public void AutoDetectLanguage(string expected, string[] lines)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < 12; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(lines[i % lines.Length], i * 1000, i * 1000 + 900));
        }

        Assert.Equal(expected, LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle));
    }

    [Theory]
    [InlineData("sv", new[] { "Vad sa han till dig?", "Vad sa han till dig?" })] // was da
    [InlineData("tr", new[] { "Onun adı ne?", "Ofiste kimse yok." })] // guard: veya/olarak/yok are Turkish-only
    [InlineData("fr", new[] { "Qu'est-ce que tu fais là?", "On se voit demain." })] // was fi
    public void AutoDetectLanguageShortText(string expected, string[] lines)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < lines.Length; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(lines[i], i * 1000, i * 1000 + 900));
        }

        Assert.Equal(expected, LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle));
    }
}
