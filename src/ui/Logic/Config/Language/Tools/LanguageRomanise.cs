
namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRomanize
{
    public string Title { get; set; }
    public string TitleSettings { get; set; }
    public string TitleLanguages { get; set; }

    public string CyrillicOriginal { get; set; }
    public string CyrillicRomanized { get; set; }
    public string DevanagariOriginal { get; set; }
    public string DevanagariRomanized { get; set; }
    public string GeezOriginal { get; set; }
    public string GeezRomanized { get; set; }
    public string GreekOriginal { get; set; }
    public string GreekRomanized { get; set; }
    public string HangulOriginal { get; set; }
    public string HangulRomanized { get; set; }
    public string KanaOriginal { get; set; }
    public string KanaRomanized { get; set; }

    public LanguageRomanize()
    {
        Title = "Romanize";
        TitleSettings = "Toggle/select below to change settings for operation";
        TitleLanguages = "Toggle below to select languages";
        CyrillicOriginal = "Кириллица";
        CyrillicRomanized = "Cyrillic";
        DevanagariOriginal = "देवनागरी";
        DevanagariRomanized = "Devanagari";
        GeezOriginal = "የግዕዝ ፊደል";
        GeezRomanized = "Amharic Ge'ez";
        GreekOriginal = "Ελληνικά";
        GreekRomanized = "Greek";
        HangulOriginal = "한글";
        HangulRomanized = "Hangul/Korean";
        KanaOriginal = "カナ";
        KanaRomanized = "Japanese Kana";
    }
}
