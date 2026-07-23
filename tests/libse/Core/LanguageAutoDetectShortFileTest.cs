using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// Characterization tests for language auto-detection on *short* subtitles (a handful
/// of lines), focused on the most-used subtitle languages. Short files are the weak
/// spot: the detector's threshold is <c>paragraphs.Count / 14</c> (== 0 below 14 lines),
/// and it returns the first language whose keyword count clears that threshold — a fixed
/// priority order (English, Danish, Norwegian, Swedish, Spanish, ...), not an argmax.
/// So a couple of ambiguous tokens can hand a short clip to the wrong (earlier) language.
///
/// These tests assert the language a human would obviously pick. Any that fail mark a
/// real short-file detection gap to improve.
/// </summary>
public class LanguageAutoDetectShortFileTest
{
    private static string? Detect(params string[] lines)
    {
        var sub = new Subtitle();
        var start = 0;
        foreach (var line in lines)
        {
            sub.Paragraphs.Add(new Paragraph(line, start, start + 2000));
            start += 2500;
        }

        sub.Renumber();
        return LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(sub);
    }

    [Fact]
    public void ShortEnglish()
    {
        Assert.Equal("en", Detect(
            "What are you doing here?",
            "I thought you would know."));
    }

    [Fact]
    public void ShortSpanish()
    {
        Assert.Equal("es", Detect(
            "¿Qué estás haciendo aquí?",
            "No lo sé, muchas gracias."));
    }

    [Fact]
    public void ShortPortuguese()
    {
        Assert.Equal("pt", Detect(
            "O que você está fazendo?",
            "Não sei, muito obrigado."));
    }

    [Fact]
    public void ShortFrench()
    {
        Assert.Equal("fr", Detect(
            "Qu'est-ce que tu fais ici?",
            "Je ne sais pas, merci."));
    }

    [Fact]
    public void ShortGerman()
    {
        Assert.Equal("de", Detect(
            "Was machst du hier?",
            "Ich weiß es nicht, danke."));
    }

    [Fact]
    public void ShortItalian()
    {
        Assert.Equal("it", Detect(
            "Cosa stai facendo qui?",
            "Non lo so, grazie mille."));
    }

    [Fact]
    public void ShortDutch()
    {
        Assert.Equal("nl", Detect(
            "Wat ben je hier aan het doen?",
            "Ik weet het niet, alleen jij."));
    }

    [Fact]
    public void ShortRussian()
    {
        Assert.Equal("ru", Detect(
            "Что ты здесь делаешь?",
            "Я не знаю, спасибо."));
    }

    [Fact]
    public void ShortArabic()
    {
        Assert.Equal("ar", Detect(
            "ماذا تفعل هنا؟",
            "لا أعرف، شكرا لك."));
    }

    [Fact]
    public void ShortTurkish()
    {
        Assert.Equal("tr", Detect(
            "Burada ne yapıyorsun?",
            "Bilmiyorum, teşekkür ederim."));
    }

    [Fact]
    public void ShortPolish()
    {
        Assert.Equal("pl", Detect(
            "Co ty tu robisz?",
            "Nie wiem, dziękuję bardzo."));
    }

    // Even shorter — a single line. This is the hardest case and most likely to be
    // undetectable (null) or wrong.
    [Fact]
    public void SingleLineSpanish()
    {
        Assert.Equal("es", Detect("¿Dónde está mi dinero, señor?"));
    }

    [Fact]
    public void SingleLinePortuguese()
    {
        Assert.Equal("pt", Detect("Você não sabe o que está acontecendo."));
    }

    [Fact]
    public void SingleLineFrench()
    {
        Assert.Equal("fr", Detect("Pourquoi est-ce que vous êtes ici?"));
    }

    // Broader coverage of the most-subtitled languages, written from natural two-line
    // dialogue (not tailored to the detector's word lists). These are the languages a
    // short clip currently resolves correctly; they guard against regressions.
    [Theory]
    // Nordic
    [InlineData("da", "Hvad laver du her?", "Det ved jeg ikke, mange tak.")]
    [InlineData("sv", "Vad gör du här?", "Jag vet inte, tack så mycket.")]
    [InlineData("no", "Hva gjør du her?", "Jeg vet ikke, tusen takk.")]
    [InlineData("fi", "Mitä sinä teet täällä?", "En tiedä, kiitos paljon.")]
    // Central / Eastern Europe (Latin)
    [InlineData("sk", "Čo tu robíš?", "Neviem, ďakujem pekne.")]
    [InlineData("hu", "Mit csinálsz itt?", "Nem tudom, köszönöm szépen.")]
    [InlineData("ro", "Ce faci aici?", "Nu știu, mulțumesc mult.")]
    [InlineData("hr", "Što radiš ovdje?", "Ne znam, hvala lijepo.")]
    [InlineData("sl", "Kaj delaš tukaj?", "Ne vem, hvala lepa.")]
    [InlineData("et", "Mida sa siin teed?", "Ma ei tea, suur tänu.")]
    // Cyrillic
    [InlineData("bg", "Какво правиш тук?", "Не знам, благодаря.")]
    [InlineData("sr", "Шта радиш овде?", "Не знам, хвала.")]
    // Greek
    [InlineData("el", "Τι κάνεις εδώ;", "Δεν ξέρω, ευχαριστώ πολύ.")]
    // Middle East
    [InlineData("he", "מה אתה עושה כאן?", "אני לא יודע, תודה רבה.")]
    // Asia
    [InlineData("ja", "ここで何をしているの？", "わからない、ありがとう。")]
    [InlineData("zh", "你在这里做什么？", "我不知道，谢谢你。")]
    [InlineData("ko", "여기서 뭐 하고 있어요?", "몰라요, 감사합니다.")]
    [InlineData("vi", "Bạn đang làm gì ở đây?", "Tôi không biết, cảm ơn.")]
    [InlineData("id", "Apa yang kamu lakukan di sini?", "Aku tidak tahu, terima kasih.")]
    [InlineData("th", "คุณกำลังทำอะไรอยู่ที่นี่", "ฉันไม่รู้ ขอบคุณมาก")]
    [InlineData("hi", "तुम यहाँ क्या कर रहे हो?", "मुझे नहीं पता, धन्यवाद।")]
    // Newly added word-lists
    [InlineData("ca", "Què fas aquí?", "No ho sé, moltes gràcies.")]        // Catalan
    [InlineData("tl", "Ano ang ginagawa mo dito?", "Hindi ko alam, salamat.")] // Tagalog / Filipino
    [InlineData("af", "Wat maak jy hier?", "Ek weet nie, baie dankie.")]    // Afrikaans
    public void ShortClip(string expected, string line1, string line2)
    {
        Assert.Equal(expected, Detect(line1, line2));
    }
}
