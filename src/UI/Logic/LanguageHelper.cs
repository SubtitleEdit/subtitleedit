using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic;

public static class LanguageHelper
{
    public static Dictionary<string, string> CountryToLanguage = new Dictionary<string, string>
    {
        { "afghanistan", "pashto" },
        { "argentina", "spanish" },
        { "australia", "english" },
        { "austria", "german" },
        { "bangladesh", "bengali" },
        { "belgium", "dutch" }, // also french and german
        { "brazil", "portuguese" },
        { "canada", "english" }, // also french
        { "china", "mandarin chinese" },
        { "colombia", "spanish" },
        { "czech republic", "czech" },
        { "denmark", "danish" },
        { "egypt", "arabic" },
        { "finland", "finnish" },
        { "france", "french" },
        { "germany", "german" },
        { "greece", "greek" },
        { "hungary", "hungarian" },
        { "india", "hindi" }, // also english and many regional languages
        { "indonesia", "indonesian" },
        { "iran", "persian" },
        { "iraq", "arabic" },
        { "ireland", "english" }, // also irish
        { "israel", "hebrew" },
        { "italy", "italian" },
        { "japan", "japanese" },
        { "kenya", "swahili" },
        { "mexico", "spanish" },
        { "netherlands", "dutch" },
        { "new zealand", "english" },
        { "nigeria", "english" }, // also hausa, yoruba, igbo, etc.
        { "norway", "norwegian" },
        { "pakistan", "urdu" },
        { "peru", "spanish" },
        { "philippines", "filipino" }, // and english
        { "poland", "polish" },
        { "portugal", "portuguese" },
        { "romania", "romanian" },
        { "russia", "russian" },
        { "saudi arabia", "arabic" },
        { "serbia", "serbian" },
        { "south africa", "zulu" }, // 11 official languages, english widely used
        { "south korea", "korean" },
        { "spain", "spanish" },
        { "sweden", "swedish" },
        { "switzerland", "german" }, // also french, italian, romansh
        { "thailand", "thai" },
        { "turkey", "turkish" },
        { "ukraine", "ukrainian" },
        { "united arab emirates", "arabic" },
        { "united kingdom", "english" },
        { "united states", "english" },
        { "vietnam", "vietnamese" }
    };
}
