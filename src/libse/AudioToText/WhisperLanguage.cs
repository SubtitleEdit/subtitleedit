using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class WhisperLanguage
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public WhisperLanguage(string code, string name)
        {
            Code = code;
            Name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name);
        }

        public override string ToString()
        {
            return Name;
        }

        public static WhisperLanguage[] Languages
        {
            get
            {
                if (Configuration.Settings.Tools.WhisperChoice == WhisperChoice.WhisperX)
                {
                    return new[]
                    {
                        new WhisperLanguage("zh", "chinese"),
                        new WhisperLanguage("nl", "dutch"),
                        new WhisperLanguage("en", "english"),
                        new WhisperLanguage("fr", "french"),
                        new WhisperLanguage("de", "german"),
                        new WhisperLanguage("it", "italian"),
                        new WhisperLanguage("ja", "japanese"),
                        new WhisperLanguage("pt", "portuguese"),
                        new WhisperLanguage("es", "spanish"),
                        new WhisperLanguage("uk", "ukrainian"),
                    };
                }

                return new[]
                {
                    new WhisperLanguage("en", "english"),
                    new WhisperLanguage("zh", "chinese"),
                    new WhisperLanguage("de", "german"),
                    new WhisperLanguage("es", "spanish"),
                    new WhisperLanguage("ru", "russian"),
                    new WhisperLanguage("ko", "korean"),
                    new WhisperLanguage("fr", "french"),
                    new WhisperLanguage("ja", "japanese"),
                    new WhisperLanguage("pt", "portuguese"),
                    new WhisperLanguage("tr", "turkish"),
                    new WhisperLanguage("pl", "polish"),
                    new WhisperLanguage("ca", "catalan"),
                    new WhisperLanguage("nl", "dutch"),
                    new WhisperLanguage("ar", "arabic"),
                    new WhisperLanguage("sv", "swedish"),
                    new WhisperLanguage("it", "italian"),
                    new WhisperLanguage("id", "indonesian"),
                    new WhisperLanguage("hi", "hindi"),
                    new WhisperLanguage("fi", "finnish"),
                    new WhisperLanguage("vi", "vietnamese"),
                    new WhisperLanguage("iw", "hebrew"),
                    new WhisperLanguage("uk", "ukrainian"),
                    new WhisperLanguage("el", "greek"),
                    new WhisperLanguage("ms", "malay"),
                    new WhisperLanguage("cs", "czech"),
                    new WhisperLanguage("ro", "romanian"),
                    new WhisperLanguage("da", "danish"),
                    new WhisperLanguage("hu", "hungarian"),
                    new WhisperLanguage("ta", "tamil"),
                    new WhisperLanguage("no", "norwegian"),
                    new WhisperLanguage("th", "thai"),
                    new WhisperLanguage("ur", "urdu"),
                    new WhisperLanguage("hr", "croatian"),
                    new WhisperLanguage("bg", "bulgarian"),
                    new WhisperLanguage("lt", "lithuanian"),
                    new WhisperLanguage("la", "latin"),
                    new WhisperLanguage("mi", "maori"),
                    new WhisperLanguage("ml", "malayalam"),
                    new WhisperLanguage("cy", "welsh"),
                    new WhisperLanguage("sk", "slovak"),
                    new WhisperLanguage("te", "telugu"),
                    new WhisperLanguage("fa", "persian"),
                    new WhisperLanguage("lv", "latvian"),
                    new WhisperLanguage("bn", "bengali"),
                    new WhisperLanguage("sr", "serbian"),
                    new WhisperLanguage("az", "azerbaijani"),
                    new WhisperLanguage("sl", "slovenian"),
                    new WhisperLanguage("kn", "kannada"),
                    new WhisperLanguage("et", "estonian"),
                    new WhisperLanguage("mk", "macedonian"),
                    new WhisperLanguage("br", "breton"),
                    new WhisperLanguage("eu", "basque"),
                    new WhisperLanguage("is", "icelandic"),
                    new WhisperLanguage("hy", "armenian"),
                    new WhisperLanguage("ne", "nepali"),
                    new WhisperLanguage("mn", "mongolian"),
                    new WhisperLanguage("bs", "bosnian"),
                    new WhisperLanguage("kk", "kazakh"),
                    new WhisperLanguage("sq", "albanian"),
                    new WhisperLanguage("sw", "swahili"),
                    new WhisperLanguage("gl", "galician"),
                    new WhisperLanguage("mr", "marathi"),
                    new WhisperLanguage("pa", "punjabi"),
                    new WhisperLanguage("si", "sinhala"),
                    new WhisperLanguage("km", "khmer"),
                    new WhisperLanguage("sn", "shona"),
                    new WhisperLanguage("yo", "yoruba"),
                    new WhisperLanguage("so", "somali"),
                    new WhisperLanguage("af", "afrikaans"),
                    new WhisperLanguage("oc", "occitan"),
                    new WhisperLanguage("ka", "georgian"),
                    new WhisperLanguage("be", "belarusian"),
                    new WhisperLanguage("tg", "tajik"),
                    new WhisperLanguage("sd", "sindhi"),
                    new WhisperLanguage("gu", "gujarati"),
                    new WhisperLanguage("am", "amharic"),
                    new WhisperLanguage("yi", "yiddish"),
                    new WhisperLanguage("lo", "lao"),
                    new WhisperLanguage("uz", "uzbek"),
                    new WhisperLanguage("fo", "faroese"),
                    new WhisperLanguage("ht", "haitian creole"),
                    new WhisperLanguage("ps", "pashto"),
                    new WhisperLanguage("tk", "turkmen"),
                    new WhisperLanguage("nn", "nynorsk"),
                    new WhisperLanguage("mt", "maltese"),
                    new WhisperLanguage("sa", "sanskrit"),
                    new WhisperLanguage("lb", "luxembourgish"),
                    new WhisperLanguage("my", "myanmar"),
                    new WhisperLanguage("bo", "tibetan"),
                    new WhisperLanguage("tl", "tagalog"),
                    new WhisperLanguage("mg", "malagasy"),
                    new WhisperLanguage("as", "assamese"),
                    new WhisperLanguage("tt", "tatar"),
                    new WhisperLanguage("haw", "hawaiian"),
                    new WhisperLanguage("ln", "lingala"),
                    new WhisperLanguage("ha", "hausa"),
                    new WhisperLanguage("ba", "bashkir"),
                    new WhisperLanguage("jw", "javanese"),
                    new WhisperLanguage("su", "sundanese"),
                };
            }
        }
    }
}
