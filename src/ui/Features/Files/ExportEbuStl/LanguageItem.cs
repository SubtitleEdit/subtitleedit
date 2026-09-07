using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportEbuStl;

public class LanguageItem
{
    public string Code { get; set; }
    public string Language { get; set; }

    public override string ToString()
    {
        return $"{Code} - {Language}";
    }

    public LanguageItem(string code, string language)
    {
        Code = code;
        Language = language;
    }
    public static ObservableCollection<LanguageItem> CreateAll()
    {
        return new ObservableCollection<LanguageItem>
        {
            new("00", ""),
            new("01", "Albanian"),
            new("02", "Breton"),
            new("03", "Catalan"),
            new("04", "Croatian"),
            new("05", "Welsh"),
            new("06", "Czech"),
            new("07", "Danish"),
            new("08", "German"),
            new("09", "English"),
            new("0A", "Spanish"),
            new("0B", "Esperanto"),
            new("0C", "Estonian"),
            new("0D", "Basque"),
            new("0E", "Faroese"),
            new("0F", "French"),
            new("10", "Frisian"),
            new("11", "Irish"),
            new("12", "Gaelic"),
            new("13", "Galician"),
            new("14", "Icelandic"),
            new("15", "Italian"),
            new("16", "Lappish"),
            new("17", "Latin"),
            new("18", "Latvian"),
            new("19", "Luxembourgi"),
            new("1A", "Lithuanian"),
            new("1B", "Hungarian"),
            new("1C", "Maltese"),
            new("1D", "Dutch"),
            new("1E", "Norwegian"),
            new("1F", "Occitan"),
            new("20", "Polish"),
            new("21", "Portuguese"),
            new("22", "Romanian"),
            new("23", "Romansh"),
            new("24", "Serbian"),
            new("25", "Slovak"),
            new("26", "Slovenian"),
            new("27", "Finnish"),
            new("28", "Swedish"),
            new("29", "Turkish"),
            new("2A", "Flemish"),
            new("2B", "Wallon"),
            new("2D", "German - hearing impaired (VA-MAL)"),
            new("2F", "French - hearing impaired (VF-MAL)"),
            new("7F", "Amharic"),
            new("7E", "Arabic"),
            new("7D", "Armenian"),
            new("7C", "Assamese"),
            new("7B", "Azerbaijani"),
            new("7A", "Bambora"),
            new("79", "Bielorussian"),
            new("78", "Bengali"),
            new("77", "Bulgarian"),
            new("76", "Burmese"),
            new("75", "Chinese"),
            new("74", "Churash"),
            new("73", "Dari"),
            new("72", "Fulani"),
            new("71", "Georgian"),
            new("70", "Greek"),
            new("6F", "Gujurati"),
            new("6E", "Gurani"),
            new("6D", "Hausa"),
            new("6C", "Hebrew"),
            new("6B", "Hindi"),
            new("6A", "Indonesian"),
            new("69", "Japanese"),
            new("68", "Kannada"),
            new("67", "Kazakh"),
            new("66", "Khmer"),
            new("65", "Korean"),
            new("64", "Laotian"),
            new("63", "Macedonian"),
            new("62", "Malagasay"),
            new("61", "Malaysian"),
            new("60", "Moldavian"),
            new("5F", "Marathi"),
            new("5E", "Ndebele"),
            new("5D", "Nepali"),
            new("5C", "Oriya"),
            new("5B", "Papamiento"),
            new("5A", "Persian"),
            new("59", "Punjabi"),
            new("58", "Pushtu"),
            new("57", "Quechua"),
            new("56", "Russian"),
            new("55", "Ruthenian"),
            new("54", "Serbocroat"),
            new("53", "Shona"),
            new("52", "Sinhalese"),
            new("51", "Somali"),
            new("50", "Sranan Tongo"),
            new("4F", "Swahili"),
            new("4E", "Tadzhik"),
            new("4D", "Tamil"),
            new("4C", "Tatar"),
            new("4B", "Telugu"),
            new("4A", "Thai"),
            new("49", "Ukrainian"),
            new("48", "Urdu"),
            new("47", "Uzbek"),
            new("46", "Vietnamese"),
            new("45", "Zulu"),
        };
    }
}
