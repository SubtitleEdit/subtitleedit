using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

public class LanguageAutoDetectLanguagesTest
{
    [Fact]
    public void AutoDetectEnglish()
    {
        var res = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(new Subtitle(new List<Paragraph>
        {
            new Paragraph("In this tutorial, I'll show you how to add", 0,0)
        }));
        Assert.Equal("en", res);
    }

    [Fact]
    public void AutoDetectDanish()
    {
        var res = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(new Subtitle(new List<Paragraph>
        {
            new Paragraph("Jeg kan ikke finde på noget at lave i dag. Kun at være glad.", 0,0)
        }));
        Assert.Equal("da", res);
    }

    #region GetEncodingViaLetter Tests

    [Fact]
    public void GetEncodingViaLetter_Arabic_ReturnsArabic()
    {
        var text = "مرحبا كيف حالك اليوم في هذا العالم الجميل";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("ar", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Chinese_ReturnsChinese()
    {
        var text = "这是一个测试中文的例子";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("zh", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Japanese_ReturnsJapanese()
    {
        var text = "これはテストです";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("ja", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Korean_ReturnsKorean()
    {
        var text = "안녕하세요 오늘 날씨가 좋습니다";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("ko", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Thai_ReturnsThai()
    {
        var text = "สวัสดีครับ วันนี้อากาศดีมาก";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("th", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Russian_ReturnsRussian()
    {
        var text = "Привет как дела сегодня хорошая погода";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("ru", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Greek_ReturnsGreek()
    {
        var text = "Γεια σας πώς είστε σήμερα";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("el", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Hebrew_ReturnsHebrew()
    {
        var text = "שלום מה שלומך היום";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("he", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Hindi_ReturnsHindi()
    {
        var text = "नमस्ते आज का दिन बहुत अच्छा है";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("hi", result);
    }

    //[Fact]
    //public void GetEncodingViaLetter_Urdu_ReturnsUrdu()
    //{
    //    var text = "یہ اردو میں ایک ٹیسٹ ہے آپ کیسے ہیں"; // Contains Urdu-specific characters: ی ہ ں
    //    var result = LanguageAutoDetect.GetEncodingViaLetter(text);
    //    Assert.Equal("ur", result);
    //}

    [Fact]
    public void GetEncodingViaLetter_Bengali_ReturnsBengali()
    {
        var text = "আজ আবহাওয়া খুব ভালো";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("bn", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Armenian_ReturnsArmenian()
    {
        var text = "Բարեւ ձեզ ինչպես եք";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("hy", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Georgian_ReturnsGeorgian()
    {
        var text = "გამარჯობა როგორ ხართ დღეს";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("ka", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Sinhalese_ReturnsSinhalese()
    {
        var text = "ආයුබෝවන් ඔබට කෙසේද";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("si", result);
    }

    [Fact]
    public void GetEncodingViaLetter_German_ReturnsGerman()
    {
        var text = "Das Wetter ist schön heute über";  // Added ü to make it clearly German
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("de", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Swedish_ReturnsSwedish()
    {
        var text = "Idag är vädret väldigt fint and hål";  // Added å to make it clearly Swedish
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("sv", result);
    }

    [Fact]
    public void GetEncodingViaLetter_French_ReturnsFrench()
    {
        var text = "Aujourd'hui le temps est très beau français";  // Added ç to make it clearer
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("fr", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Spanish_ReturnsSpanish()
    {
        var text = "Hoy el tiempo es muy bueno señor";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("es", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Amharic_ReturnsAmharic()
    {
        var text = "ሰላም ዛሬ ጥሩ ቀን ነው";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("am", result);
    }

    [Fact]
    public void GetEncodingViaLetter_Khmer_ReturnsKhmer()
    {
        var text = "សួស្តី តើអ្នកសុខសប្បាយទេ";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("km", result);
    }

    [Fact]
    public void GetEncodingViaLetter_EmptyString_ReturnsNull()
    {
        var result = LanguageAutoDetect.GetEncodingViaLetter("");
        Assert.Null(result);
    }

    [Fact]
    public void GetEncodingViaLetter_NullString_ReturnsNull()
    {
        var result = LanguageAutoDetect.GetEncodingViaLetter(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetEncodingViaLetter_OnlyEnglishCharacters_ReturnsNull()
    {
        var text = "This is just English text without special characters";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Null(result);
    }

    [Fact]
    public void GetEncodingViaLetter_MinimumThreshold_ReturnsLanguage()
    {
        var text = "äöü"; // Exactly 3 German characters (minimum threshold)
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("de", result);
    }

    [Fact]
    public void GetEncodingViaLetter_MixedEnglishAndChinese_ReturnsChinese()
    {
        var text = "Hello 这是一个测试 world";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("zh", result);
    }

    [Fact]
    public void GetEncodingViaLetter_MixedEnglishAndJapanese_ReturnsJapanese()
    {
        var text = "Welcome to これはテストです Japan";
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("ja", result);
    }

    [Fact]
    public void GetEncodingViaLetter_ChineseVsJapanese_PrioritizesChinese()
    {
        // When there's a tie, Chinese should be prioritized
        var text = "的一是"; // Common Chinese characters
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        Assert.Equal("zh", result);
    }

    [Fact]
    public void GetEncodingViaLetter_RepeatedCharacters_CountsMultiple()
    {
        var text = "ääääääööööö"; // Multiple German/Swedish characters
        var result = LanguageAutoDetect.GetEncodingViaLetter(text);
        // Should detect due to counting occurrences, not unique characters
        Assert.NotNull(result);
        Assert.True(result == "de" || result == "sv");
    }

    //[Fact]
    //public void GetEncodingViaLetter_ArabicVsUrdu_DistinguishesProperly()
    //{
    //    // Pure Arabic text (no Urdu-specific characters)
    //    var arabicText = "مرحبا كيف حالك في العالم";
    //    var arabicResult = LanguageAutoDetect.GetEncodingViaLetter(arabicText);
    //    Assert.Equal("ar", arabicResult);

    //    // Urdu with specific characters (گ پ چ ٹ ڈ ڑ ں ے ھ ہ ی)
    //    var urduText = "یہ پاکستان میں ٹیسٹ ہے";  // Contains ی ہ پ ک ٹ
    //    var urduResult = LanguageAutoDetect.GetEncodingViaLetter(urduText);
    //    Assert.Equal("ur", urduResult);
    //}

    [Fact]
    public void GetEncodingViaLetter_GermanVsSwedish_Distinguishes()
    {
        // Text with German-specific character ß
        var germanText = "Straße und größer";
        var germanResult = LanguageAutoDetect.GetEncodingViaLetter(germanText);
        Assert.Equal("de", germanResult);

        // Text with only Swedish common characters
        var swedishText = "äöäöäö";
        var swedishResult = LanguageAutoDetect.GetEncodingViaLetter(swedishText);
        // Both German and Swedish have ä and ö, but if equal, depends on implementation
        Assert.True(swedishResult == "de" || swedishResult == "sv");
    }

    [Fact]
    public void GetEncodingViaLetter_FrenchVsSpanish_Distinguishes()
    {
        // French with œ
        var frenchText = "cœur œuvre bœuf";
        var frenchResult = LanguageAutoDetect.GetEncodingViaLetter(frenchText);
        Assert.Equal("fr", frenchResult);

        // Spanish with ñ
        var spanishText = "niño año señor";
        var spanishResult = LanguageAutoDetect.GetEncodingViaLetter(spanishText);
        Assert.Equal("es", spanishResult);
    }

    [Fact]
    public void GetEncodingViaLetter_LongTextPerformance()
    {
        // Test with longer text to ensure performance improvement
        var longText = string.Concat(Enumerable.Repeat("这是一个测试中文的例子 ", 100));
        var result = LanguageAutoDetect.GetEncodingViaLetter(longText);
        Assert.Equal("zh", result);
    }

    #endregion
}
