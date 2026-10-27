using Nikse.SubtitleEdit.Core.DetectEncoding;
using System;
#if NET8_0_OR_GREATER
using System.Buffers;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class LanguageAutoDetect
    {

        // Every word list below is registered in the index, which counts all of them in one
        // tokenizing pass over the text (see WordListIndex). It used to be one compiled
        // \b(word|word|...)\b regex per list, i.e. 40+ full scans of the text per detection
        // plus ~1.7 ms of regex compilation per list on first use.
        private static readonly Lazy<WordListIndex> WordIndex = new Lazy<WordListIndex>(BuildWordIndex);

        private static WordListIndex BuildWordIndex()
        {
            return new WordListIndex(new[]
            {
                AutoDetectWordsNorwegianOnly,
                AutoDetectWordsDanishOnly,
                AutoDetectWordsFrenchOnly,
                AutoDetectWordsPortugueseOnly,
                AutoDetectWordsRomanianOnly,
                AutoDetectWordsSpanishOnly,
                AutoDetectWordsEnglishUs,
                AutoDetectWordsEnglishGb,
                AutoDetectWordsRussianShort,
                AutoDetectWordsBulgarianShort,
                AutoDetectWordsRomanianShort,
                AutoDetectWordsEnglish,
                AutoDetectWordsDanish,
                AutoDetectWordsNorwegian,
                AutoDetectWordsSwedish,
                AutoDetectWordsSpanish,
                AutoDetectWordsItalian,
                AutoDetectWordsFrench,
                AutoDetectWordsPortuguese,
                AutoDetectWordsGerman,
                AutoDetectWordsDutch,
                AutoDetectWordsPolish,
                AutoDetectWordsGreek,
                AutoDetectWordsRussian,
                AutoDetectWordsBulgarian,
                AutoDetectWordsUkrainian,
                AutoDetectWordsAlbanian,
                AutoDetectWordsArabic,
                AutoDetectWordsFarsi,
                AutoDetectWordsHebrew,
                AutoDetectWordsVietnamese,
                AutoDetectWordsHungarian,
                AutoDetectWordsTurkish,
                AutoDetectWordsCroatianAndSerbian,
                AutoDetectWordsCroatian,
                AutoDetectWordsSerbian,
                AutoDetectWordsSerbianCyrillic,
                AutoDetectWordsSerbianCyrillicOnly,
                AutoDetectWordsIndonesian,
                AutoDetectWordsCatalan,
                AutoDetectWordsTagalog,
                AutoDetectWordsAfrikaans,
                AutoDetectWordsThai,
                AutoDetectWordsKorean,
                AutoDetectWordsMacedonian,
                AutoDetectWordsFinnish,
                AutoDetectWordsRomanian,
                AutoDetectWordsCzechAndSlovak,
                AutoDetectWordsCzech,
                AutoDetectWordsSlovak,
                AutoDetectWordsSlovenian,
                AutoDetectWordsIcelandic,
                AutoDetectWordsLatvian,
                AutoDetectWordsLithuanian,
                AutoDetectWordsEstonian,
                AutoDetectWordsHindi,
                AutoDetectWordsUrdu,
                AutoDetectWordsSinhalese,
            });
        }

        /// <summary>
        /// Occurrence counts of every registered word list in one text.
        /// </summary>
        private readonly struct WordListCounts
        {
            private readonly WordListIndex _index;
            private readonly int[] _counts;

            public WordListCounts(WordListIndex index, string text)
            {
                _index = index;
                _counts = index.Count(text);
            }

            /// <summary>
            /// How many whole words (case-insensitive) from <paramref name="words"/> the text
            /// contains. The list must be one of the static AutoDetectWords* fields.
            /// </summary>
            public int Get(string[] words)
            {
                return _counts[_index.IdOf(words)];
            }
        }

        private static WordListCounts CountWords(string text)
        {
            return new WordListCounts(WordIndex.Value, text);
        }

        private static int GetCount(string text, string[] words)
        {
            return CountWords(text).Get(words);
        }

        /// <summary>
        /// Tells the Cyrillic languages apart by spelling, which the shared short words
        /// (не, да, за, нас, може, така, беше, ...) cannot: ј/љ/њ/џ are written only in
        /// Macedonian and Serbian (ќ/ѓ/ѕ only in Macedonian, ћ/ђ only in Serbian), і/ї/є/ґ only
        /// in Ukrainian, ы/э/ё only in Russian, and ъ is everywhere in Bulgarian but rare in
        /// Russian and absent from the rest.
        /// </summary>
        /// <param name="text">The text to look at.</param>
        /// <param name="counts">Word counts of the same text, for a Macedonian/Serbian tie; null to count on demand.</param>
        /// <returns>"mk", "sr", "uk", "bg", "ru", or null when the letters do not say.</returns>
        private static string GetCyrillicLanguageByLetters(string text, WordListCounts? counts)
        {
            var macedonianOrSerbian = 0; // ј љ њ џ ќ ѓ ѕ ћ ђ
            var macedonianOnly = 0;      // ќ ѓ ѕ
            var serbianOnly = 0;         // ћ ђ
            var ukrainianOnly = 0;       // і ї є ґ
            var russianOnly = 0;         // ы э ё
            var hardSign = 0;            // ъ, not before е/ё/ю/я
            var eastSlavic = 0;          // ъ ы э ё й щ ь ю я - none of them in Macedonian or Serbian
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                switch (c)
                {
                    case 'ј': case 'Ј': case 'љ': case 'Љ': case 'њ': case 'Њ': case 'џ': case 'Џ':
                        macedonianOrSerbian++;
                        break;
                    case 'ќ': case 'Ќ': case 'ѓ': case 'Ѓ': case 'ѕ': case 'Ѕ':
                        macedonianOrSerbian++;
                        macedonianOnly++;
                        break;
                    case 'ћ': case 'Ћ': case 'ђ': case 'Ђ':
                        macedonianOrSerbian++;
                        serbianOnly++;
                        break;
                    case 'і': case 'І': case 'ї': case 'Ї': case 'є': case 'Є': case 'ґ': case 'Ґ':
                        ukrainianOnly++;
                        break;
                    case 'ы': case 'Ы': case 'э': case 'Э': case 'ё': case 'Ё':
                        russianOnly++;
                        eastSlavic++;
                        break;
                    case 'ъ': case 'Ъ':
                        // Russian writes ъ only before е/ё/ю/я (съел, подъезд, объявление),
                        // Bulgarian mostly before consonants (дъжд, къде, със) - only the latter
                        // says Bulgarian.
                        if (i + 1 >= text.Length || "еёюяЕЁЮЯ".IndexOf(text[i + 1]) < 0)
                        {
                            hardSign++;
                        }

                        eastSlavic++;
                        break;
                    case 'й': case 'Й': case 'щ': case 'Щ': case 'ь': case 'Ь': case 'ю': case 'Ю': case 'я': case 'Я':
                        eastSlavic++;
                        break;
                }
            }

            // Strictly more, so that a few misread letters in an OCR'ed file cannot flip a
            // Bulgarian or Russian text, where ъ/й/я/ю are everywhere.
            if (macedonianOrSerbian > 0 && macedonianOrSerbian > eastSlavic)
            {
                if (macedonianOnly != serbianOnly)
                {
                    return macedonianOnly > serbianOnly ? "mk" : "sr";
                }

                // Neither ќ/ѓ/ѕ nor ћ/ђ, or as many of each (an OCR tie): the word lists decide.
                var c2 = counts ?? CountWords(text);
                var macedonianCount = c2.Get(AutoDetectWordsMacedonian);
                var serbianCount = c2.Get(AutoDetectWordsSerbianCyrillic) + c2.Get(AutoDetectWordsSerbianCyrillicOnly);
                return macedonianCount > serbianCount ? "mk" : "sr";
            }

            if (ukrainianOnly > 0 && ukrainianOnly > russianOnly + hardSign)
            {
                return "uk";
            }

            if (russianOnly != hardSign)
            {
                return russianOnly > hardSign ? "ru" : "bg";
            }

            return null;
        }

        /// <summary>
        /// Lithuanian or Latvian by their own letters: ą/ę/ė/į/ų are Lithuanian, ā/ē/ī/ģ/ķ/ļ/ņ/ō
        /// Latvian (č/š/ž/ū are both). The word lists overlap (kas, kur, kad, ir, jau, ...).
        /// </summary>
        /// <param name="text">The text to look at.</param>
        /// <returns>"lt", "lv", or null when neither letter set is in the text.</returns>
        private static string GetBalticLanguageByLetters(string text)
        {
            var lithuanian = 0;
            var latvian = 0;
            foreach (var c in text)
            {
                switch (c)
                {
                    case 'ą': case 'Ą': case 'ę': case 'Ę': case 'ė': case 'Ė': case 'į': case 'Į': case 'ų': case 'Ų':
                        lithuanian++;
                        break;
                    case 'ā': case 'Ā': case 'ē': case 'Ē': case 'ī': case 'Ī': case 'ģ': case 'Ģ': case 'ķ': case 'Ķ':
                    case 'ļ': case 'Ļ': case 'ņ': case 'Ņ': case 'ō': case 'Ō':
                        latvian++;
                        break;
                }
            }

            if (lithuanian == latvian)
            {
                return null;
            }

            return lithuanian > latvian ? "lt" : "lv";
        }

        /// <summary>
        /// Afrikaans or Dutch by spelling: Dutch writes z and ij all the time (zijn, zo, mij,
        /// hij, zij), Afrikaans has no z and no ij but y instead (sy, hy, my, jy, altyd).
        /// </summary>
        /// <param name="text">The text to look at.</param>
        /// <returns>"af", "nl", or null when the text has neither.</returns>
        private static string GetAfrikaansOrDutchByLetters(string text)
        {
            var dutch = 0;
            var afrikaans = 0;
            var previous = '\0';
            foreach (var c in text)
            {
                if (c == 'z' || c == 'Z')
                {
                    dutch++;
                }
                else if ((c == 'j' || c == 'J') && (previous == 'i' || previous == 'I'))
                {
                    dutch++;
                }
                else if (c == 'y' || c == 'Y')
                {
                    afrikaans++;
                }

                previous = c;
            }

            if (dutch == afrikaans)
            {
                return null;
            }

            return afrikaans > dutch ? "af" : "nl";
        }

        /// <summary>
        /// Arabic, Persian or Urdu by the letters each script adds: Persian and Urdu write
        /// پ/چ/ژ/گ and the Persian ye/kaf (ی U+06CC, ک U+06A9), Arabic has ة; Urdu alone has
        /// ٹ/ڈ/ڑ/ں/ے/ھ/ہ. The Arabic ye/kaf (ي U+064A, ك U+0643) and ذ/ض/ظ/ء say nothing: older
        /// Persian files (Windows-1256) are typed with that ye/kaf, and ذ/ض/ظ/ء are common in
        /// Persian loanwords.
        /// </summary>
        /// <param name="text">The text to look at.</param>
        /// <returns>"ur", "fa", "ar", or null when the letters do not say.</returns>
        private static string GetArabicScriptLanguageByLetters(string text)
        {
            var arabic = 0;
            var persian = 0;
            var urdu = 0;
            foreach (var c in text)
            {
                switch (c)
                {
                    case 'ة':
                        arabic++;
                        break;
                    case 'ی': case 'ک': case 'پ': case 'چ': case 'ژ': case 'گ':
                        persian++;
                        break;
                    case 'ٹ': case 'ڈ': case 'ڑ': case 'ں': case 'ے': case 'ھ': case 'ہ': case 'ۓ':
                        urdu++;
                        break;
                }
            }

            if (urdu > 0 && urdu * 4 >= persian)
            {
                return "ur";
            }

            if (persian == arabic)
            {
                return null;
            }

            return persian > arabic ? "fa" : "ar";
        }

        /// <summary>
        /// Latin-script languages with letters of their own, for the letter fallback when no
        /// word list matched (a line or two). Shared letters are left out: ç (fr/pt/tr/ca/sq),
        /// ş (ro/tr), ä/ö/ü (de/sv/fi/et), å (sv/da/no), æ/ø (da/no), ć/đ (hr/sr/bs),
        /// č/š/ž (cs/sk/sl/hr/sr/lt/lv), õ (pt/et), ô (fr/sk), ą/ę (pl/lt).
        /// </summary>
        private static readonly KeyValuePair<string, string>[] LatinUniqueLetters =
        {
            new KeyValuePair<string, string>("pl", "łŁżŻźŹśŚńŃ"),
            new KeyValuePair<string, string>("cs", "řŘěĚůŮ"),
            new KeyValuePair<string, string>("sk", "ľĽŕŔ"),
            new KeyValuePair<string, string>("hu", "őŐűŰ"),
            new KeyValuePair<string, string>("ro", "ăĂșȘțȚţŢ"),
            new KeyValuePair<string, string>("tr", "ığĞİ"),
            new KeyValuePair<string, string>("lt", "ėĖįĮųŲ"),
            new KeyValuePair<string, string>("lv", "āĀēĒīĪģĢķĶļĻņŅ"),
            new KeyValuePair<string, string>("is", "þÞðÐ"),
            new KeyValuePair<string, string>("vi", "ơƠưƯạẠảẢấẤầẦẩẨẫẪậẬắẮằẰẳẲẵẴặẶẹẸẻẺẽẼếẾềỀểỂễỄệỆỉỈịỊọỌỏỎốỐồỒổỔỗỖộỘớỚờỜởỞỡỠợỢụỤủỦứỨừỪửỬữỮựỰỳỲỵỴỷỶỹỸ"),
            new KeyValuePair<string, string>("pt", "ãÃ"),
            new KeyValuePair<string, string>("es", "ñÑ¿¡"),
        };

        /// <summary>
        /// The Latin-script language whose own letters occur most in the text; Albanian when
        /// ë (which French has only in a few words) outnumbers the French accents.
        /// </summary>
        /// <param name="text">The text to look at.</param>
        /// <returns>A language code, or null when no language-specific letter is in the text.</returns>
        private static string GetLatinLanguageByUniqueLetters(string text)
        {
            string best = null;
            var bestCount = 0;
            foreach (var pair in LatinUniqueLetters)
            {
                var count = 0;
                foreach (var c in text)
                {
                    if (pair.Value.IndexOf(c) >= 0)
                    {
                        count++;
                    }
                }

                if (count > bestCount)
                {
                    best = pair.Key;
                    bestCount = count;
                }
            }

            if (best == null)
            {
                var albanian = 0;
                var french = 0;
                foreach (var c in text)
                {
                    switch (c)
                    {
                        case 'ë': case 'Ë':
                            albanian++;
                            break;
                        case 'é': case 'É': case 'è': case 'È': case 'ê': case 'Ê': case 'à': case 'À': case 'ù': case 'Ù':
                            french++;
                            break;
                    }
                }

                if (albanian >= 2 && albanian > french)
                {
                    return "sq";
                }
            }

            return best;
        }

        private static int GetCountContains(string text, params string[] words)
        {
            var count = 0;
            foreach (var w in words)
            {
                var index = text.IndexOf(w, StringComparison.Ordinal);
                while (index >= 0)
                {
                    count++;
                    index = text.IndexOf(w, index + w.Length, StringComparison.Ordinal);
                }
            }

            return count;
        }

        public static string AutoDetectGoogleLanguage(Encoding encoding)
        {
            switch (encoding.CodePage)
            {
                case 860:
                    return "pt"; // Portuguese
                case 28599:
                case 1254:
                    return "tr"; // Turkish
                case 28598:
                case 1255:
                    return "he"; // Hebrew
                case 28596:
                case 1256:
                    return "ar"; // Arabic
                case 1258:
                    return "vi"; // Vietnamese
                case 949:
                case 1361:
                case 20949:
                case 51949:
                case 50225:
                    return "ko"; // Korean
                case 1253:
                case 28597:
                    return "el"; // Greek
                case 50220:
                case 50221:
                case 50222:
                case 51932:
                case 20932:
                case 10001:
                    return "ja"; // Japanese
                case 20000:
                case 20002:
                case 20936:
                case 950:
                case 52936:
                case 54936:
                case 51936:
                    return "zh"; // Chinese
                default:
                    return null;
            }
        }

        // Short disambiguation lists: words that belong to one language but not its close
        // neighbour, probed only when the neighbour's main list already scored.
        private static readonly string[] AutoDetectWordsNorwegianOnly = { "ut", "deg", "meg", "merkelig", "mye", "spørre",
            "vært", "sa", "noe", "noen", "hva", "etter", "uten", "mellom", "ble", "blitt", "vet", "dere", "seg", "oss", "henne", "nei", "takk", "unnskyld",
            "liker", "trenger", "sier", "fjor", "sjanse", "aldri", "siste", "neste", "annen", "sånn", "enn", "å", "igjen", "gjøre", "gjør", "kanskje", "bra",
            "veldig", "gutt", "jente", "elva", "vann", "jobbet", "jobb", "lei", "fikk", "sett", "ditt", "mitt", "sitt", "vårt", "kveld",
            "natt", "uke", "penger", "redd", "kjære", "kjenner", "tilbake", "gjennom", "nesten", "nettopp", "dessverre", "viktig", "riktig", "ferdig", "trøtt", "liten", "litt", "hjelp", "hjelper", "begynner", "finner", "tar", "gir", "spør", "forteller", "arbeidet", };
        private static readonly string[] AutoDetectWordsDanishOnly = { "siger", "dig", "mig", "mærkelig", "tilbage", "spørge",
            "hvad", "noget", "nogen", "nu", "meget", "efter", "uden", "mellem", "gøre", "gør", "blev", "blevet", "sig", "os", "hende", "nej", "undskyld", "lide",
            "behøver", "sidste", "næste", "anden", "sådan", "jer", "jeres", "dit", "mit", "sit", "vores", "ind", "ud", "sagde", "sige", "kender",
            "kendt", "kære", "arbejde", "arbejder", "arbejdede", "åbner", "begynder", "finder", "tager", "giver", "lægger", "sætter", "sidder", "løber", "kører",
            "hjælper", "hjælp", "bruger", "spørger", "fortæller", "svær", "lille", "pæn", "syg", "træt", "færdig", "rigtig", "fejl", "vigtig", "kedelig",
            "pludselig", "desværre", "netop", "næsten", "gennem", "inden", "penge", "aften", "nat", "uge", "dreng", "pige", "vand", "ventede", "fik", "ked",
            "mere", "bange", "vred", "håber", "væk", };
        private static readonly string[] AutoDetectWordsFrenchOnly = { "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque" };
        private static readonly string[] AutoDetectWordsPortugueseOnly =
        {
            "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
            "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios",
        };
        private static readonly string[] AutoDetectWordsRomanianOnly = { "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici" };
        private static readonly string[] AutoDetectWordsSpanishOnly = { "Hola", "nada", "Vamos", "pasa", "los", "como" };
        private static readonly string[] AutoDetectWordsEnglishUs = { "color", "flavor", "honor", "humor", "neighbor" };
        private static readonly string[] AutoDetectWordsEnglishGb = { "colour", "flavour", "honour", "humour", "neighbour" };

        // Short probe lists used when guessing a code page from a raw byte buffer.
        private static readonly string[] AutoDetectWordsRussianShort = { "что", "быть", "весь", "этот", "один", "такой" };
        private static readonly string[] AutoDetectWordsBulgarianShort = { "Какво", "тук", "може", "Как", "Ваше" };
        private static readonly string[] AutoDetectWordsRomanianShort = { "să", "şi", "văzut", "regulă", "găsit", "viaţă" };

        private static readonly string[] AutoDetectWordsEnglish =
        {
            "we", "are", "and", "your", "what", "[TW]hat's", "You're", "(any|some|every)thing", "money", "because", "human", "abandon",
            "welcome", "really", "something", "confusing", "about", "know", "people", "other", "which", "would", "these", "could",
            "tutorial", "I'll", "show", "you", "about", "absolutely", "accept", "wrong", "thought", "training", "treatment", "successful",
            "strong", "simply", "situation","soldier", "somebody", "someone","something", "sometimes", "speak","statement", "perhaps",
            "piece", "picture", "police", "popular", "population", "suggest", "weapon", "water", "window", "woman", "talk", "security",
            "season", "remain", "relationship", "report" ,"user" ,"result", "this",
            "the", "that", "with", "have", "not", "were", "they", "there", "their", "them", "then", "than", "been", "from", "into", "will", "should", "did", "does",
            "has", "had", "he", "she", "him", "his", "where", "when", "why", "how", "who", "here", "just", "get", "got", "going", "need", "think", "right",
            "never", "always", "maybe", "please", "thank", "thanks", "sorry", "look", "tell", "told", "said", "make", "made", "take", "took", "give", "gave", "back",
            "time", "night", "life", "love", "good", "great", "little", "much", "many", "more", "only", "very", "but", "if", "I'm", "it's", "don't", "didn't", "doesn't",
            "can't", "isn't", "wasn't", "won't", "we're", "they're", "I've", "you've", "nobody", "before", "after", "years", "closed", "expected", "quiet",
        };

        private static readonly string[] AutoDetectWordsDanish =
        {
            "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej", "længere", "gjorde", "dig", "havde", "[Uu]ndskyld", "arbejder", "vidste", "troede", "stadigvæk", "[Mm]åske", "første", "gik",
            "fortælle", "begyndt", "spørgsmål", "pludselig", "være", "fordi", "været", "sidste","også", "løb", "sagde", "hjem",
            "ikke", "til", "hvor", "når", "hvis", "hvem", "hvorfor", "hvordan", "nok", "godt", "fra", "noget", "nogen", "hun", "mig", "deres",
            "uden", "mellem", "næste", "anden", "meget", "lidt", "hvad", "nej", "gøre", "gør", "siger", "kender", "kunne", "måtte", "selv", "mere", "andre", "dette", "hele", "sammen", "væk", "penge", "arbejde", "ked", "ærlig",
            "fik", "vand", "broen", "vejene", "husene", "senere", "komme", "kommet",
            "ventes", "kraftig", "regn", "morgen", "nordlige", "temperaturen", "frysepunktet", "bilister", "forsigtige",
        };

        private static readonly string[] AutoDetectWordsNorwegian =
        {
            "vi", "og", "jeg", "var", "men", "igjen", "Nei", "Hei", "noen", "gjøre", "kanskje", "[Tt]renger", "tenker", "skjer", "møte", "veldig", "takk", "penger", "konsept", "hjelp", "forsvunnet",
            "skutt", "sterkt", "minste", "første", "fortsette", "inneholder", "gikk", "fortelle", "begynt", "spørsmål", "plutselig",
            "ikke", "til", "hvor", "når", "hvis", "hvem", "hvorfor", "hvordan", "også", "nok", "godt", "fra", "noe", "hun", "meg", "deg", "seg", "oss", "deres",
            "uten", "mellom", "neste", "annen", "mye", "litt", "nå", "hva", "gjør", "ble", "sier", "kjenner", "kunne", "måtte",
            "siste", "selv", "andre", "dette", "hele", "sammen", "vekk", "hjem", "tilbake", "jobb", "jobbet", "lei", "sjanse",
            "ærlig", "etter", "aldri", "å", "være", "vært", "hadde", "dere", "fikk", "vann", "brua", "veiene", "husene", "elva", "ventet", "senere", "fjor",
            "ventes", "kraftig", "regn", "morgen", "temperaturen", "faller", "løpet", "natten", "bilister", "forsiktige",
        };

        private static readonly string[] AutoDetectWordsSwedish =
        {
            "vi", "är", "och", "Jag", "inte", "för", "måste", "Öppna", "Förlåt", "nånting", "ingenting", "jävla", "Varför", "[Ss]nälla", "fattar", "själv", "säger", "öppna", "jävligt", "dörren", "göra", "behöver", "tillbaka", "Varför",
            "träffa", "kanske", "säga", "hände", "honom", "hennes", "veckor", "tänker", "själv", "pratar", "mycket", "mamma", "dödade", "Ursäkta", "säger", "senaste", "håller", "förstår", "veckan", "varför", "tycker", "första", "gick",
            "berätta", "börjat", "människor", "frågor", "fråga", "plötsligt", "skämt",
            "hade", "varit", "att", "ska", "från", "också", "bara", "något", "någon", "någonting", "deras", "gång", "utan", "mellan", "nästa",
            "andra", "vad", "tack", "gör", "känner", "kunde", "många", "dessa", "detta", "alla", "allt", "tillsammans", "pengar", "arbete", "ledsen", "chans", "ärlig",
            "fick", "vatten", "bron", "vägarna", "husen", "komma", "kommit", "när", "hur", "vem", "vilken", "vilket", "väl", "redan", "ännu", "fortfarande", "sedan",
            "innan", "medan", "eftersom", "därför", "stiga", "snabbt", "staden", "stängde", "tyst", "flera", "kullen", "ljug", "prata", "finns", "hon", "henne",
            "våra", "vår", "okej", "visst", "precis", "dålig", "dåligt", "bättre", "bäst", "gamla", "unga", "människa", "kvinna", "flicka", "pojke",
            "kväll", "vecka", "älskar", "hjälp", "hjälpa", "hejdå", "nån", "nåt", "nånstans", "överallt", "upp", "ner", "kvar", "länge", "ibland", "tidigt", "genast", "verkligen", "nästan", "väldigt", "alldeles", "lika", "annan", "annat", "varje", "båda", "några", "inga", "allting", "vilka", "vart",
            "kände", "behövde", "såg", "hörde", "sade", "frågade", "svarade", "tänkte", "försökte", "började", "hjälpte",
            "arbetade", "jobbade", "levde", "väntade", "hoppades", "köpte", "betalade", "körde", "träffade", "ringde", "läste", "hittade", "öppnade",
            "väntas", "över", "norra", "morgon", "kraftigt", "delarna", "sjunker", "bör", "vara", "försiktiga", "noll", "bord", "två", "självklart", "följ", "menyn",
            "först", "utmärkt",
        };

        private static readonly string[] AutoDetectWordsSpanish =
        {
            "qué", "eso", "muy", "estoy?", "ahora", "hay", "tú", "así", "cuando", "cómo", "él", "sólo", "quiero", "gracias", "puedo", "bueno", "soy", "hacer", "fue", "eres", "usted", "tienes", "puede", "[Ss]eñor", "ese", "voy", "quién",
            "creo", "hola", "dónde", "sus", "verdad", "quieres", "mucho", "entonces", "estaba", "tiempo", "esa", "mejor", "hombre", "hace", "dios", "también", "están", "siempre", "hasta", "ahí", "siento",
            "puedes", "Dónde", "está", "estoy", "Vámonos", "vistiendo", "Caliente", "chimenea"
        };

        private static readonly string[] AutoDetectWordsItalian =
        {
            "Buongiorno", "Grazie", "Cosa", "quest[ao]", "quell[ao]", "tutt[io]", "[st]uo", "qualcosa", "ancora", "sono", "bene", "più", "andare", "essere", "venire", "abbiamo", "andiamo", "ragazzi", "signore", "numero", "giorno",
            "propriamente", "sensitivo", "negativo", "davvero", "faccio", "voglio", "vuole", "perché", "allora", "niente", "anche", "stai", "detto", "fatto", "hanno", "molto", "stato", "siamo", "così", "vuoi", "noi", "vero", "loro",
            "fare", "gli", "due",
            "è", "sei", "siete", "erano", "stata", "stati", "state", "ho", "hai", "avete", "avevo", "aveva", "avevano", "io", "lui", "lei", "voi", "che", "chi",
            "dove", "quanto", "quale", "quali", "qua", "lì", "adesso", "oggi", "domani", "ieri", "nulla", "nessuno", "qualcuno", "tutto", "tutti", "tutta", "tutte",
            "troppo", "soltanto", "già", "dopo", "mentre", "quindi", "sì", "prego", "scusa", "scusi", "dispiace", "ciao", "arrivederci", "signora", "signorina", "uomo",
            "donna", "ragazzo", "ragazza", "bambino", "bambini", "notte", "mattina", "sera", "anno", "anni", "volta", "amore", "soldi", "presto", "insieme", "peccato",
            "forse", "d'accordo", "puoi", "può", "possiamo", "devo", "devi", "deve", "dobbiamo", "vogliamo", "sai", "sappiamo", "pensi", "credo", "credi", "dico",
            "dici", "dice", "vedo", "vedi", "vede", "visto", "vieni", "vengo", "vai", "vado", "fai", "facciamo", "parlare", "parlo", "parli", "parla", "lavorare",
            "lavoro", "lavoravo", "città", "fiume", "ponte", "acqua", "strade", "collina", "tardi", "mentire", "mentirmi", "possibilità", "onesto", "scorso",
            "tranquilla", "rimasta", "fabbrica", "chiudesse", "aspettava", "salisse", "fretta", "sparito", "inondate", "guardava", "portava", "arrivare", "disse",
            "averlo", "senti", "dammi", "avuto",
        };

        private static readonly string[] AutoDetectWordsFrench =
        {
            "pas", "[vn]ous", "ça", "une", "pour", "[mt]oi", "dans", "elle", "tout", "plus", "[bmt]on", "suis", "avec", "oui", "fait", "ils", "être", "faire", "comme", "était", "quoi", "ici", "veux",
            "vouloir", "quelque", "pouvoir", "rien", "dit", "où", "votre", "pourquoi", "sont", "cette", "peux", "alors", "comment", "avez", "très", "même", "merci", "ont", "aussi", "chose", "voir",
            "allez", "tous", "ces", "deux", "avoir", " pouvoir", "même", "œil", "sœur",
        };

        private static readonly string[] AutoDetectWordsPortuguese =
        {
            "[Nn]ão", "[Ee]ntão", "uma", "ele", "bem", "isso", "você", "sim", "meu", "muito", "estou", "ela", "fazer", "tem", "já", "minha", "tudo", "só", "tenho", "agora", "vou", "seu", "quem", "há",
            "lhe", "quero", "nós", "são", "ter", "coisa", "dizer", "eles", "pode", "bom", "mesmo", "mim", "estava", "assim", "estão", "até", "quer", "temos", "acho", "obrigado", "também", "tens",
            "deus", "quê", "ainda", "noite", "conversar", "Desculpe", "verdade",
            "é", "foi", "tinha", "têm", "eu", "elas", "vocês", "teu", "tua", "sua", "nosso", "nossa", "isto", "aquilo", "aquele", "aquela", "aqui", "lá", "cá", "hoje",
            "amanhã", "ontem", "alguma", "muita", "tão", "apenas", "porquê", "pois", "obrigada", "desculpa", "adeus", "senhor", "senhora", "homem", "mulher",
            "rapaz", "menino", "menina", "criança", "crianças", "pessoas", "manhã", "anos", "dinheiro", "sozinho", "coisas", "faço", "fazes", "faz", "fiz", "fez",
            "feito", "disse", "diz", "vejo", "vês", "viu", "vem", "vai", "queres", "podes", "preciso", "precisas", "precisamos", "falar", "falo", "falas", "fala",
            "falei", "trabalhar", "trabalho", "cidade", "água", "estradas", "ninguém", "subisse", "fechar", "depressa", "devagar", "mas", "onde", "qual", "mentas",
            "está bem", "oportunidade", "tiveste", "estiveste", "disse-te", "dá-me", "esperava", "inundadas", "observava", "levava", "disseram", "tinham", "chegar",
        };

        private static readonly string[] AutoDetectWordsGerman =
        {
            "und", "auch", "sich", "bin", "hast", "möchte", "müssen", "weiß", "[Vv]ielleicht", "Warum", "jetzt", "Verdammt", "bist", "Darum", "sitzt", "Setz", "Das ist", "Du bist", "nicht", "Scheiße",
            "Nein", "für", "gesagt", "zwei", "richtig",
            "ich", "sie", "wir", "ihr", "mich", "dich", "mir", "dir", "uns", "euch", "ihn", "ihm", "ihnen", "mein", "dein", "unser", "dem", "ein", "eine", "einen",
            "einem", "einer", "kein", "keine", "nichts", "etwas", "jemand", "niemand", "immer", "niemals", "schon", "dort", "wo", "wohin", "woher", "wann", "wie", "wer",
            "weil", "wenn", "dass", "ob", "aber", "oder", "sondern", "denn", "doch", "gut", "schlecht", "sehr", "zu", "von", "aus", "bei", "nach", "über", "unter",
            "vor", "hinter", "zwischen", "ohne", "gegen", "durch", "um", "auf", "ist", "sind", "waren", "wird", "werden", "wurde", "habe", "haben", "hatte", "hatten",
            "kann", "kannst", "können", "konnte", "muss", "musst", "soll", "sollte", "willst", "wollen", "wollte", "darf", "gehe", "geht", "gehen", "ging", "komme",
            "kommt", "kommen", "kam", "mache", "macht", "machen", "gemacht", "sehe", "sieht", "sehen", "gesehen", "weißt", "wissen", "wusste", "denke", "denkst",
            "dachte", "sage", "sagt", "sagen", "sagte", "glaube", "lass", "komm", "geh", "schau", "hör", "warte", "frau", "junge", "mädchen", "kinder", "leute",
            "menschen", "abend", "zeit", "jahr", "jahre", "haus", "zuhause", "leben", "liebe", "schnell", "langsam", "zusammen", "allein", "schade", "danke", "bitte",
            "entschuldigung", "tschüss", "herr", "stadt", "fluss", "brücke", "wasser", "häuser", "straßen", "später", "fabrik", "lange", "gearbeitet", "lüg", "leid",
            "reden", "letztes", "ehrlich", "einmal", "ruhig", "jahrelang", "erwartet", "steigen", "hügel", "ganze", "nacht", "nochmal", "hattest", "schloss",
        };

        private static readonly string[] AutoDetectWordsDutch =
        {
            "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n", "hebben", "alleen", "Waarom", "stripboeken", "geweest",
            "niet", "ik", "jij", "wij", "ze", "zij", "hij", "deze", "zo", "zonder", "zijn", "waren", "bent", "wordt", "worden", "werd", "heb", "hebt", "heeft", "hadden", "kun",
            "kunnen", "moeten", "moest", "wilt", "willen", "wilde", "zal", "zullen", "zou", "zouden", "gaat", "ging", "komt", "komen",
            "kwam", "doe", "doet", "doen", "deed", "gedaan", "zie", "ziet", "zien", "zag", "gezien", "weten", "wist", "denk", "denkt", "dacht", "zeg", "zegt",
            "zeggen", "zei", "gezegd", "praten", "mij", "jouw", "hem", "onze", "hun", "niets", "altijd", "misschien", "echt", "veel", "weinig", "slecht", "nieuw", "nacht", "avond", "gisteren", "vandaag", "tijd", "jaren", "thuis", "leven", "mensen", "vrouw", "jongen", "kinderen", "wegen", "huizen", "stijgen", "verwacht", "jarenlang",
            "sloot", "liegen", "spijt", "eerlijk", "dank", "bedankt", "alsjeblieft", "tot ziens",
        };

        private static readonly string[] AutoDetectWordsPolish =
        {
            "Czy", "ale", "ty", "siê", "się", "jest", "mnie", "Proszę", "życie", "statku", "życia", "Czyli", "Wszystko",
            "Wiem", "Przepraszam", "dobrze", "chciałam", "Dziękuję", "Żołnierzyk", "Łowca", "został", "stało", "dolarów",
            "wiadomości", "Dobrze", "będzie", "Dzień", "przyszłość", "Uratowałaś", "Cześć", "Trzeba", "zginąć", "walczyć",
            "ludzkość", "maszyny", "Jeszcze", "okrążenie", "wyścigu", "porządku", "detektywie", "przebieralni", "który",
            "śmierci", "zabić", "wiedźminie", "przeznaczenie",
            "gdzie", "kiedy", "dlaczego", "kto", "wy", "mną", "ciebie", "cię", "tobie", "tobą", "jego", "jej", "wam", "są", "był", "była", "było", "byli", "byłeś",
            "będę", "będziesz", "jestem", "jesteś", "jesteśmy", "masz", "mamy", "mają", "miał", "miała", "mogę", "możesz", "możemy", "muszę", "musisz", "musimy", "chcę",
            "chcesz", "chciałem", "wiesz", "myślę", "myślisz", "mówię", "mówisz", "mówiłem", "mówiła", "mówili", "powiedz", "powiedział", "powiedziała", "widzę",
            "widzisz", "widziałem", "chodź", "idź", "idziemy", "zrobić", "zrobiłem", "robisz", "pani", "człowiek", "ludzie", "mężczyzna", "kobieta", "chłopak",
            "dziewczyna", "dziecko", "dzieci", "nocy", "rano", "czas", "czasu", "lata", "kocham", "pieniądze", "szybko", "razem", "źle", "bardzo", "tylko", "już",
            "teraz", "tutaj", "zawsze", "nigdy", "nikt", "ktoś", "coś", "wszyscy", "ponieważ", "jeśli", "jeżeli", "żeby", "więc", "też", "także", "która", "które",
            "całą", "późna", "pracowałem", "okłamuj", "słuchaj", "porozmawiać", "powiedzenia", "szansę", "zeszłym", "roku", "czego", "szczery", "miasto", "spokojne",
            "zamknięto", "fabrykę", "rzeka", "mostu", "drogi", "wodą", "patrzyła", "zabiera", "domy", "później", "przewidzieli", "spodziewał", "podniesie",
        };

        private static readonly string[] AutoDetectWordsGreek =
        {
            "μου", "[Εε]ίναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις",
            "και", "να", "το", "τα", "την", "τον", "της", "του", "τους", "τις", "με", "σε", "για", "από", "δεν", "μη", "μην", "θα", "ότι", "που", "πως", "πού", "τι",
            "γιατί", "πότε", "ποιος", "ποια", "ποιο", "εγώ", "εσύ", "αυτός", "αυτή", "εμείς", "εσείς", "αυτοί", "μας", "σας", "εδώ", "εκεί", "τώρα", "σήμερα", "αύριο",
            "χθες", "πάντα", "ποτέ", "κανείς", "κάποιος", "τίποτα", "κάτι", "όλα", "όλοι", "πολύ", "μόνο", "ακόμα", "ήδη", "μετά", "πριν", "επειδή", "αν", "αλλά", "ή",
            "ναι", "όχι", "εντάξει", "ευχαριστώ", "παρακαλώ", "συγνώμη", "συγγνώμη", "γεια", "κύριε", "κυρία", "άνθρωπος", "άνθρωποι", "άντρας", "γυναίκα", "αγόρι",
            "κορίτσι", "παιδί", "παιδιά", "μέρα", "νύχτα", "πρωί", "χρόνος", "χρόνια", "φορά", "σπίτι", "ζωή", "αγάπη", "λεφτά", "χρήματα", "νεκρός", "γρήγορα", "μαζί",
            "μόνος", "είμαι", "είσαι", "είμαστε", "είστε", "ήταν", "ήμουν", "έχω", "έχεις", "έχει", "έχουμε", "είχα", "είχε", "μπορώ", "μπορείς", "μπορεί", "πρέπει",
            "θέλω", "θέλεις", "ξέρω", "ξέρεις", "νομίζω", "λέω", "λες", "είπα", "είπε", "βλέπω", "είδα", "έλα", "πάμε", "κάνω", "κάνεις", "κάνει", "έκανα", "δουλειά",
            "πόλη", "ποτάμι", "γέφυρα", "νερό", "σπίτια", "αργότερα", "εργοστάσιο", "ήσυχη", "περίμενε", "ανέβαινε", "δρόμοι", "λόφο", "έπαιρνε", "έλεγαν", "δουλεύω",
            "ψέματα", "ευκαιρία", "ειλικρινής", "μιλήσουμε", "λυπάμαι", "κοίτα",
        };

        private static readonly string[] AutoDetectWordsRussian =
        {
            "[Ээч]?то", "[Нн]е", "[ТтМмбв]ы", "Да", "[Нн]ет", "Он", "его", "тебя", "как", "меня", "Но", "всё", "мне", "вас", "знаю", "ещё", "за", "нас", "чтобы", "был"
        };

        private static readonly string[] AutoDetectWordsBulgarian =
        {
            "беше", "[Бб]лагодаря", "бързо", "вас", "[Вв]аше", "[Вв]ече", "[Вв]иждам", "време", "[Вв]сичк[ио]", "години",
            "Да", "[Дд]обре", "дяволите", "за", "Защо", "защото", "[Зз]начи", "иска[мнш]", "[Кк]ак", "[Кк]акво", "като",
            "ко[еий]то", "малко", "много", "[Мм]оже[хш]?", "място", "нас", "[Нн]е", "н[еи]що", "н[ия]кой", "[Нн]яма",
            "преди", "повече", "става", "така", "[Тт]ова", "[Тт]олкова", "[Тт]рябва", "тук",
            "Хайде", "човек"
        };

        private static readonly string[] AutoDetectWordsUkrainian =
        {
            "Вона", "мене", "[Нн]і", "[Пп]ривіт", "[Цц]е", "[Щщ]о", "[Йй]ого", "[Вв]ін", "[Яя]к", "[Гг]аразд", "[Яя]кщо", "[Мм]ені", "[Тт]вій", "[Її]х", "[Вв]ітаю", "[Дд]якую", "вже", "було", "був", "цього",
            "нічого", "немає", "може", "знову", "бо", "щось", "щоб", "цим", "тобі", "хотів", "твоїх", "мої", "мій", "має", "їм", "йому", "дуже",
            "Привіт", "побачення", "тобою", "побачення", "Гаразд", "повинні",
            "ти", "ми", "ви", "вони", "він", "зараз", "сьогодні", "вчора", "завжди", "ніколи", "ніхто", "хтось", "усе", "всі", "тільки",
            "лише", "ні", "погано", "будь ласка", "вибач", "вибачте", "пробач", "пан", "пані", "людина", "чоловік", "жінка", "хлопець",
            "дівчина", "дитина", "діти", "ніч", "ранок", "рік", "роки", "дім", "додому", "життя", "кохаю", "гроші", "швидко", "повільно",
            "разом", "шкода", "маю", "маєш", "можу", "можеш", "мусимо", "хочеш", "хоче", "хочемо", "знаєш", "кажу",
            "кажеш", "сказав", "бачу", "бачиш", "бачив", "іди", "ходімо", "зробити", "зробив", "робиш", "місто", "роками", "тихим", "доки",
            "закрився", "очікував", "річка", "підніметься", "ранку", "міст", "зник", "водою", "дивилася", "забирає", "будинки", "пізніше", "казали",
            "передбачали", "усю", "казав", "працював", "пізна", "бреши", "сказати", "мав", "свій", "минулого", "чого", "чесним",
        };

        private static readonly string[] AutoDetectWordsAlbanian =
        {
            "është", "të", "një", "për", "nuk", "më", "që", "dhe", "në", "ku", "kush", "pse", "çfarë", "shumë", "mirë", "keq", "faleminderit", "zotëri", "zonjë", "këtu", "atje", "tani", "nesër", "dje", "gjithë", "gjithmonë", "kurrë", "asgjë", "dikush", "diçka", "ishte", "isha", "jemi", "janë", "kemi", "kanë", "kisha", "kishte", "duhet", "dua", "doja", "tha", "thuaj", "thashë", "shiko", "eja", "shko", "bëj", "bëri", "njeri", "njerëz", "burrë", "grua", "djalë", "vajzë", "fëmijë", "ditë", "natë", "natën", "kohë", "vitin", "herë", "shtëpi", "jetë", "dashuri", "edhe", "ose", "nga", "sepse", "nëse", "kështu", "ashtu", "vetëm", "gjë", "gjëra", "lutem", "falni", "kuptoj", "mendoj", "besoj", "shoh", "vij", "unë", "ata", "ato", "ajo", "këtë", "mbrapa", "vërtet", "gjithashtu", "tënde", "çmendur", "vullnetin", "vdekur", "qenë", "punoja", "deri", "vonë", "gënje", "vjen", "rregull", "flasim", "thënë", "jep", "shans", "pate", "kaluar", "sinqertë", "qetë", "lumi", "ura", "rrugët", "uji", "shtëpitë", "vonë", "qyteti",
        };

        private static readonly string[] AutoDetectWordsArabic =
        {
            "من", "هل", "لا", "في", "لقد", "ما", "ماذا", "يا", "هذا", "إلى", "على", "أنا", "أنت", "حسناً", "أيها", "كان", "كيف", "يكون", "هذه", "هذان", "الذي", "التي", "الذين", "هناك", "هنالك"
        };

        private static readonly string[] AutoDetectWordsFarsi =
        {
            "این", "برای", "اون", "سیب", "کال", "رو", "خيلي", "آره", "بود", "اون", "نيست", "اينجا", "باشه", "سلام", "ميکني", "داري", "چيزي", "چرا", "خوبه",
            "که", "را", "به", "از", "با", "در", "است", "هست", "نیست", "نبود", "باشد", "تو", "او", "شما", "آنها", "اونها", "اینجا", "آنجا", "اونجا", "حالا", "الان",
            "امروز", "فردا", "دیروز", "همیشه", "هیچوقت", "هیچ", "چیزی", "کسی", "همه", "خیلی", "هنوز", "دیگه", "دیگر", "چی", "چه", "کجا", "کی", "چطور", "چقدر",
            "کدام", "بله", "نه", "خوب", "بد", "ممنون", "مرسی", "متشکرم", "لطفا", "لطفاً", "ببخشید", "متأسفم", "متاسفم", "خداحافظ", "آقا", "خانم", "مرد", "زن", "پسر",
            "دختر", "بچه", "مردم", "روز", "شب", "سال", "بار", "خانه", "خونه", "زندگی", "پول", "مرده", "زود", "دیر", "تنها", "درست", "البته", "واقعا", "واقعاً", "باید",
            "نباید", "میدونم", "می‌دونم", "نمیدونم", "نمی‌دونم", "می‌دانم", "نمی‌دانم", "فکر", "می‌کنم", "میکنم", "می‌کنی", "میکنی", "می‌کردم", "می‌خواهم", "میخوام",
            "می‌خوام", "می‌خواهی", "می‌خواهد", "می‌تونم", "می‌توانم", "نمی‌تونم", "گفتم", "گفت", "گفتی", "بگو", "ببین", "بیا", "برو", "کرد", "کردم", "کردی", "کار",
            "شهر", "تا", "یک", "یه", "دو", "داری", "دارم", "داره", "می‌کنیم", "بودی", "بودم", "بودند", "دروغ", "نگو", "حرف", "بزنیم", "فرصت", "بده", "پارسال", "صادق",
            "آرام", "کارخانه", "بسته", "رودخانه", "بالا", "بیاید", "جاده‌ها", "آب", "خانه‌ها", "تماشا", "می‌گفتند", "دیده", "آمدنش",
        };

        private static readonly string[] AutoDetectWordsHebrew =
        {
            "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב", "אֶת", "שֶׁל", "עַל", "הוּא",
            "אֲשֶׁר", "הִיא", "הַמְלֶט", "נָסִיךְ", "הֲלֹא", "עוֹד", "אֵין", "אֲנִי", "זֹאת", "וְלֹא",
            "כָּךְ", "הִנֵּה", "מְאֹד", "אֲדוֹנִי",
            "אני", "את", "היא", "אנחנו", "אתם", "הם", "הן", "זה", "זאת", "של", "על", "עם", "לא", "כן", "מה", "מי", "איפה", "למה", "מתי", "איך", "כמה", "יש", "אין",
            "היה", "הייתי", "היית", "יהיה", "רוצה", "יכול", "צריך", "חושב", "אומר", "אמרתי", "אמרת", "אמר", "רואה", "ראיתי", "בוא", "לך", "תן", "עשה", "עשיתי", "לעשות",
            "לדבר", "עכשיו", "היום", "מחר", "אתמול", "תמיד", "אף פעם", "כלום", "משהו", "מישהו", "הכל", "כל", "רק", "גם", "עוד", "כבר", "מאוד", "רע", "תודה", "בבקשה",
            "סליחה", "שלום", "אדוני", "גבירתי", "איש", "אנשים", "גבר", "אישה", "ילד", "ילדה", "ילדים", "יום", "לילה", "בוקר", "זמן", "שנה", "שנים", "פעם", "בית", "חיים",
            "אהבה", "כסף", "מת", "מהר", "ביחד", "לבד", "חבל", "אבל", "כי", "אם", "או", "אז", "שם", "פה", "כאן", "הזדמנות", "לשקר", "תשקר", "עבדתי", "מאוחר", "לומר",
            "מצטער", "להיות", "העיר", "שקטה", "המפעל", "נסגר", "הנהר", "יעלה", "הגשר", "נעלם", "הכבישים", "הוצפו", "הבתים", "המים", "הגבעה", "צפתה", "אמרו", "ראו",
            "מגיע", "ממני", "ממך", "לי", "לו", "לה", "לנו", "לכם", "להם", "אותי", "אותך", "אותו", "אותה", "אותנו", "שלי", "שלך", "שלו", "שלה", "שלנו", "הלילה", "תראה",
            "צריכים", "שעברה", "פעם אחת", "איש לא", "ציפה", "הייתה",
        };

        private static readonly string[] AutoDetectWordsVietnamese =
        {
            "không", "[Tt]ôi", "anh", "đó", "ông", "tôi", "phải", "người", "được", "Cậu", "chúng", "chuyện", "muốn", "những", "nhiều",
            "của", "và", "có", "một", "này", "cho", "với", "để", "cô", "bà", "chị", "nó", "họ", "mình", "gì", "sao", "đâu", "nào", "bao", "khi", "lúc", "giờ", "luôn",
            "chưa", "đã", "sẽ", "đang", "rồi", "nữa", "cũng", "chỉ", "rất", "quá", "lắm", "vâng", "dạ", "đúng", "sai", "tốt", "xấu", "chào", "đi", "đến", "về", "làm",
            "nói", "biết", "nghĩ", "thấy", "nhìn", "nghe", "cần", "thích", "yêu", "sống", "chết", "tiền", "nhà", "năm", "ngày", "đêm", "sáng", "tối", "lần", "nhanh",
            "cùng", "nhau", "thật", "chắc", "thế", "vậy", "nhưng", "vì", "nếu", "hoặc", "đây", "đấy", "kia", "tại", "trong", "ngoài", "trên", "dưới", "trước", "giữa",
            "sông", "cầu", "đường", "nước", "đồi", "muộn", "dối", "cảm ơn", "xin lỗi", "làm ơn", "có thể", "bây giờ", "hôm nay", "ngày mai", "hôm qua", "thị trấn",
            "nhà máy", "đóng cửa", "yên tĩnh", "cơ hội", "thành thật", "chuyện này", "ở đâu", "nghe này", "còn", "ở", "ai đó", "chúng ta", "mọi người", "nhiều năm",
            "trước khi", "sau đó",
        };

        private static readonly string[] AutoDetectWordsHungarian =
        {
            "hogy", "lesz", "tudom", "vagy", "mondtam", "még", "vagyok", "csak", "Hát", "felesége", "Csak", "utána", "jött",
            "Miért", "Akkor", "magát", "holnap", "Tudja", "Köszönöm", "élet", "Örvendek", "vissza", "hogy", "tudom", "Rendben",
            "Istenem", "Gyerünk", "értem", "vagyok", "hiszem", "történt", "rendben", "olyan", "őket", "vannak", "mindig", "Kérlek",
            "Gyere", "kicsim", "vagyunk",
            "nem", "és", "egy", "ez", "az", "volt", "nincs", "már", "itt", "ott", "mit", "hol", "ki", "én", "ő", "ők", "nekem", "neked", "neki", "engem", "velem",
            "veled", "tudod", "akarom", "akarok", "kell", "lehet", "jó", "rossz", "kérem", "bocsánat", "sajnálom", "szia", "uram", "asszonyom", "ember", "emberek",
            "férfi", "nő", "fiú", "lány", "gyerek", "éjjel", "éjszaka", "reggel", "idő", "év", "ház", "haza", "szeretlek", "pénz", "gyorsan", "lassan", "együtt",
            "egyedül", "kár", "soha", "senki", "valaki", "semmi", "valami", "minden", "mindenki", "nagyon", "talán", "persze", "tényleg", "igazán", "biztos", "azt",
            "ezt", "ezek", "azok", "sincs", "mert", "aki", "ami", "amit", "ide", "oda", "innen", "onnan", "után", "előtt", "alatt", "között", "nélkül", "miatt",
            "szerint", "mondta", "mondd", "látom", "látod", "láttam", "menj", "menjünk", "csinálni", "csináltam", "csinálsz", "beszélnünk", "beszélni", "hazudj",
            "esélyt", "esélyed", "tavaly", "őszinte", "város", "évekig", "csendes", "mielőtt", "gyár", "bezárt", "számított", "folyó", "megárad", "reggelre", "híd",
            "eltűnt", "utak", "víz", "dombról", "nézte", "ahogy", "elviszi", "házakat", "később", "mondták", "látták", "előre", "voltál", "egész", "sokáig", "dolgoztam",
            "nézd", "tőlem", "legyél", "egyszer",
            "van", "estét", "asztaluk", "két", "természetesen", "kövessenek", "először", "étlapot", "hal", "ma", "este", "kiváló", "északon", "heves", "esőzés",
            "várható", "hőmérséklet", "fagypont", "alá", "süllyed", "autósok", "legyenek", "különösen", "óvatosak", "utakon",
        };

        private static readonly string[] AutoDetectWordsTurkish =
        {
            "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "çalýþýyor", "Aldırma",
            "burada", "efendim", "şey", "çok", "Merhaba", "Evet", "kötü", "musun",
            "güzel", "çünkü", "büyük", "Bebeğim", "olduğunu", "istiyorum", "değilsin", "bilmiyorum",
            "otursana", "Selam", "Tabii", "konuda", "istiyor", "Teşekkürler", "istemiyorum", "Gerçekte",
            "değil", "değildi", "değilim", "değiller", "mı", "mısın", "mıyım", "mıydı", "mıyız",
            "şu", "şimdi", "şeyi", "şeyler", "işte", "eğer", "artık", "yardım", "yalnız", "yalnızca",
            "nasıl", "başka", "doğru", "karşı", "dışarı", "sanırım", "yarın", "kızım", "oğlum", "canım",
            "tanrım", "hayatım", "aşkım", "yapıyorsun", "yapacağım", "lütfen", "kadın", "adamı", "bakalım",
            "bırak", "dışında", "hiçbir", "ışık", "kız", "sığ", "şans", "şöyle", "yaşam", "başladı",
            "olmalı", "bunları", "onları", "kızı", "sana", "seni", "beni", "bana", "hadi", "olur",
            "gerçekten", "kesinlikle", "biliyorum", "olduğum", "ölmüş", "öldü", "gidiyorum", "kalsın"
        };

        private static readonly string[] AutoDetectWordsCroatianAndSerbian =
        {
            "Hajde", "znam", "ćemo", "šta", "sam", "ali", "nije", "Nije", "samo", "ovo", "kako", "dobro", "Dobro", "sve", "tako", "će", "mogu", "ću", "zašto", "nešto", "za", "misliš", "možeš", "možemo", "ništa", "znaš", "ćemo", "znam",
            "li", "ga", "mu", "joj", "vam", "ih", "ili", "kad", "koliko", "kao", "još", "već", "sada", "tamo", "danas", "sutra", "nikad", "nikada", "svi", "svaki",
            "vrlo", "puno", "mnogo", "više", "manje", "loše", "hvala", "molim", "izvoli", "gospodin", "gospođa", "ljudi", "muškarac", "noć", "godina", "godine", "kuća",
            "kući", "ljubav", "novac", "mrtav", "zajedno", "šteta", "imam", "imaš", "ima", "imamo", "imaju", "može", "moraš", "moramo", "hoću", "hoćeš", "hoće", "želim",
            "želiš", "zna", "kažem", "kažeš", "rekao", "rekla", "reci", "vidim", "vidiš", "dođi", "idi", "idemo", "raditi", "radiš", "večeras", "jelovnik", "prvo",
            "naravno", "očekuje", "jaka", "kiša", "noći", "ispod", "nule", "vozači", "trebali", "posebno", "oprezni", "pasti", "dvoje", "temperature",
        };

        private static readonly string[] AutoDetectWordsCroatian =
        {
            "što", "ovdje", "gdje", "kamo", "tko", "prije", "uvijek", "vrijeme", "vidjeti", "netko", "vidio", "nitko", "bok", "lijepo", "oprosti", "htio", "mjesto", "oprostite", "čovjek", "dolje", "čovječe", "dvije", "dijete", "dio",
            "poslije", "događa", "vjerovati", "vjerojatno", "vjerujem", "točno", "razumijem", "vidjela", "cijeli", "svijet", "obitelj", "volio", "sretan", "dovraga", "svijetu", "htjela", "vidjeli", "negdje", "želio", "ponovno",
            "djevojka", "umrijeti", "čovjeka", "mjesta", "djeca", "osjećam", "uopće", "djecu", "naprijed", "obitelji", "doista", "mjestu", "lijepa", "također", "riječ", "tijelo",
            "vrijeme", "mlijeko", "lijepo",
            "stol", "slijedite", "izvrsna", "sjeveru", "tijekom", "cestama", "jučer", "tisuću", "tjedan", "mjesec", "svjetlo", "glazba", "zrak", "kruh", "tvrtka",
            "siječanj", "vjetar", "snijeg", "rijeka", "cijena", "vrijedi", "pješice", "sljedeći",
        };

        private static readonly string[] AutoDetectWordsSerbian =
        {
            "šta", "ovde", "gde", "ko", "pre", "uvek", "vreme", "videti", "neko", "video", "niko", "ćao", "lepo", "izvini", "hteo", "mesto", "izvinite", "čovek", "dole", "čoveče", "dve", "dete", "deo", "posle", "dešava", "verovati",
            "verovatno", "verujem", "tačno", "razumem", "videla", "ceo", "svet", "porodica", "voleo", "srećan", "dođavola", "svetu", "htela", "videli", "negde", "želeo", "ponovo", "devojka", "umreti", "čoveka", "mesta", "deca", "osećam",
            "uopšte", "decu", "napred", "porodicu", "zaista", "mestu", "lepa", "takođe", "reč", "telo",
            "sto", "veče", "pratite", "odlična", "severu", "tokom", "putevima", "juče", "hiljadu", "nedelja", "mesec", "svetlo", "muzika", "vazduh", "hleb", "firma",
            "januar", "trebalo", "budu", "vetar", "sneg", "reka", "cena", "vredi", "peške", "sledeći",
        };

        private static readonly string[] AutoDetectWordsSerbianCyrillic =
        {
            "ће", "ћемо", "Хоћу", "сам", "али", "није", "само", "ово", "како", "добро", "све", "тако", "ће", "могу", "ћу", "зашто", "нешто", "за", "шта", "овде", "бити", "чини", "учениче", "побегне", "остати", "Један", "Назад", "Молим",
            "сутра", "северу", "очекује", "јака", "киша", "температуре", "ноћи", "испод", "нуле", "возачи", "требало", "посебно", "опрезни", "путевима", "вече", "двоје",
            "наравно", "пратите", "можемо", "прво", "јеловник", "вечерас", "одлична", "сада", "данас", "јуче", "увек", "никад", "нико", "неко", "ништа", "сви", "више",
            "хвала", "људи", "дан", "ноћ", "кућа", "љубав", "новац", "заједно", "хоћеш", "хоће", "желим", "кажем", "рекао", "рекла", "дођи", "идемо", "хајде", "радити",
            "радио", "лажеш", "жао", "реду", "морамо", "разговарамо", "каже", "прилику", "своју", "прошле", "једном", "будеш", "искрен", "годинама", "миран",
            "затворила", "нарасти", "јутра", "нестао", "поплављени", "брда", "односи", "куће", "касније", "ја", "је", "су", "смо", "сте", "они", "она", "мене", "тебе",
            "њега", "њу", "нас", "вас", "их", "ми", "ти", "ви", "ко", "где", "кад", "јер", "ако", "или", "па", "још", "већ", "много", "мало", "лоше",
        };

        private static readonly string[] AutoDetectWordsSerbianCyrillicOnly =
        {
            "дођавола", "Дођи", "пронађемо", "ће", "ћемо", "Хоћу", "чекић", "неће",
        };

        private static readonly string[] AutoDetectWordsIndonesian =
        {
            "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu",
            "aku", "kamu", "kita", "mereka", "saya", "anda", "ini", "itu", "dari", "pada", "oleh", "atau", "tetapi", "karena", "kalau", "jika", "supaya", "agar", "apa",
            "siapa", "mana", "kapan", "mengapa", "kenapa", "bagaimana", "berapa", "tidak", "bukan", "belum", "jangan", "ada", "adalah", "harus", "mau", "ingin", "boleh",
            "dapat", "sedang", "masih", "lagi", "juga", "saja", "hanya", "sangat", "sekali", "sekarang", "nanti", "tadi", "kemarin", "besok", "hari", "malam", "pagi",
            "waktu", "kali", "orang", "pria", "wanita", "anak", "ibu", "ayah", "teman", "hidup", "cinta", "uang", "mati", "cepat", "pelan", "bersama", "sendiri",
            "sayang", "terima kasih", "maaf", "tolong", "selamat", "pak", "bu", "tuan", "nyonya", "semalaman", "kerja", "larut", "bohong", "dengar", "bicara", "soal",
            "perlu", "dikatakan", "beri", "kesempatan", "lalu", "jujur", "kota", "sunyi", "pabrik", "ditutup", "menyangka", "sungai", "naik", "begitu", "menjelang",
            "jembatan", "hilang", "jalan", "terendam", "menyaksikan", "bukit", "menyapu", "belakangan", "berkata", "menduganya", "kubilang", "padaku", "dariku", "oke",
            "hujan", "turun", "utara", "diperkirakan", "suhu", "bawah", "nol", "pengemudi", "hati-hati", "ekstra", "meja", "tentu", "silakan", "ikuti", "lihat",
            "menunya", "dulu", "ikannya", "enak", "deras",
        };

        // Catalan — distinctive words that don't collide with Spanish/French/Italian/Portuguese
        // (accents matter: "què"/"gràcies"/"això" won't match the Spanish "qué"/"gracias").
        private static readonly string[] AutoDetectWordsCatalan =
        {
            "què", "això", "amb", "però", "molt", "moltes", "molts", "gràcies", "aquest", "aquesta", "aquestes",
            "nosaltres", "vosaltres", "vull", "puc", "pots", "sóc", "ets", "són", "fer", "fas", "fem", "tinc",
            "tens", "seva", "teu", "meu", "ara", "res", "també", "perquè", "sisplau", "endavant", "ningú",
            "sempre", "doncs", "gairebé", "cadascú",
            "estat", "estic", "estàs", "està", "estem", "estan", "sou", "és", "eren", "heu", "havia", "té", "tenim", "tenen", "tenia", "vols", "volem",
            "podem", "cal", "saps", "sap", "crec", "creus", "dius", "diu", "vaig", "vam", "anar", "feu", "fet", "veig", "veus", "veure", "vine", "vinc",
            "noia", "nen", "nit", "matí", "anys", "vegada", "ràpid", "perdó", "d'acord", "adéu", "senyor",
            "senyora", "meva", "teva", "nostre", "vostre", "ells", "ell", "vostè", "ens", "allà", "allò", "més", "tampoc", "després",
            "abans", "dins", "algú", "treballar", "treballava", "treballo", "mentis", "greu", "parlar", "oportunitat", "sincer", "poble",
            "fàbrica", "tanqués", "riu", "carreteres", "turó", "aigua", "vist", "tothom", "encara", "només", "potser", "si us plau", "fer-ho",
            "demà", "dels", "pels", "pel", "s'esperen", "baixaran", "d'anar", "pluges", "bona", "teniu", "taula", "seguiu-me", "podríem", "primer", "peix", "excel·lent",
            "temperatures", "conductors", };

        // Tagalog / Filipino — distinctive Austronesian function words; low overlap with European
        // languages. Spanish loanwords ("para", "pero") are intentionally left out to avoid collisions.
        private static readonly string[] AutoDetectWordsTagalog =
        {
            "ang", "ng", "mga", "ako", "ikaw", "siya", "kami", "tayo", "kayo", "sila", "ito", "iyan", "iyon",
            "dito", "diyan", "doon", "hindi", "wala", "mayroon", "meron", "salamat", "kumusta", "bakit", "ano",
            "sino", "saan", "kailan", "paano", "gusto", "alam", "ginagawa", "talaga", "naman", "kasi", "namin",
            "natin", "ninyo", "nila", "niya", "kaya", "dahil", "ganito", "ganyan", "kung", "kapag", "mahal",
            "ka", "rin", "daw", "nga", "pala", "yung", "yan", "yun", "oo", "opo", "sige", "pwede", "puwede", "huwag", "wag", "walang", "maraming", "marami", "konti",
            "lahat", "kahit", "tungkol", "ngayon", "bukas", "kahapon", "gabi", "umaga", "araw", "oras", "taon", "beses", "bahay", "buhay", "patay", "mabilis", "mabagal",
            "magkasama", "mag-isa", "totoo", "sigurado", "siguro", "mabuti", "masama", "maganda", "malaki", "maliit", "matanda", "bata", "tao", "lalaki", "babae",
            "kaibigan", "sabi", "sinabi", "sabihin", "gawin", "ginawa", "ayaw", "akala", "tingin", "nakita", "halika", "tara", "pumunta", "nagpunta", "umuwi", "trabaho",
            "pasensya", "pakiusap", "bigyan", "pagkakataon", "nagkaroon", "nakaraang", "maging", "tapat", "minsan", "tahimik", "bayan", "loob", "bago", "magsara",
            "pabrika", "tataas", "ilog", "ganoon", "kabilis", "pagsapit", "tulay", "binaha", "kalsada", "pinagmasdan", "burol", "tinangay", "tubig", "kalaunan",
            "darating", "buong", "kailangan", "sa'yo", "akong", "nating", "masasabi", "pag-usapan", "pagsinungalingan", "nag-overtime", "tingnan", "okay lang", "noong",
            "mula",
        };

        // Afrikaans — only words that are NOT also valid Dutch, so a Dutch clip is never mistaken
        // for Afrikaans (the Dutch list is small; shared words like "maak"/"goed"/"weet" would leak).
        private static readonly string[] AutoDetectWordsAfrikaans =
        {
            "nie", "baie", "jy", "jou", "ek", "moenie", "dankie", "asseblief", "hoekom", "hierdie", "daardie",
            "sal", "vir", "hulle", "saam", "gesels", "meisie", "altyd", "regtig", "aandag", "vinnig", "seblief",
            "nogal", "sommer", "elkeen",
            "julle", "sy", "hy", "'n", "oukei", "verlede", "gesê", "sê", "gewerk", "gesluit", "gekyk", "dink", "glo", "gepraat", "gemaak", "gedoen", "gekom", "gegaan",
            "gewees", "geword", "gevind", "gekry", "gegee", "gesien", "geneem", "verwag", "stadig", "almal", "seker", "miskien", "sou", "moes", "wou", "jare", "nag",
            "môre", "vandag", "gister", "tyd", "lewe", "mense", "vrou", "seun", "kinders", "huise", "mekaar", "styg", "oggend", "paaie", "lank", "terwyl", "sodat",
            "eers", "laaste", "homself", "hul", "sonder", "binne", "buite", "hieroor", "wegneem", "eerlik", };

        private static readonly string[] AutoDetectWordsThai =
        {
            "ผู้กอง", "โรเบิร์ต", "วิตตอเรีย", "เข้าใจมั้ย", "คุณตำรวจ", "ราเชล", "เพื่อน", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์", "ขอโทษครับ", "ขอบคุณ", "วาร์กัส", "ทุกคน",
            "ไม่เอาน่า", "อิซานะ", "มจริง", "รับทราบ", "พอคะ", "ครับ", "อยาตขาป", "ยินดีทีดรูจักคะ", "ปลอดภัยดีนะ", "ทุกคน", "ตอนที", "ขอบคุณครับ", "ขอทษนะคะ", "ขอทษคะ"
        };

        private static readonly string[] AutoDetectWordsKorean =
        {
            "사루", "거야", "엄마", "그리고", "아니야", "하지만", "말이야", "그들은", "우리가", "엄마가", "괜찮아", 
            "일어나", "잘했어", "뭐라고", "있어요", "있어요", "보여줘", "괜찮아요", "목소릴", "기억이", "저주를", 
            "좋아요", "조사관", "회장님", "새끼야", "사모님", "가정부", "숨소리", "잠수부", "어떻게", "의미심장한",
            "양장점", "긴장감", "거예요", "그래요", "없어요", "고마워요", "알았어요", "안녕하세요", "모르겠어요"
        };

        // Everyday Macedonian words, preferring the ones Bulgarian and Serbian spell
        // differently (ќе/ще/ће, јас/аз/ја, тоа/това/то, зошто/защо/зашто, ...). The old list
        // was mostly words from one space film plus OCR misreads with Latin letters in them.
        private static readonly string[] AutoDetectWordsMacedonian =
        {
            "ќе", "што", "тоа", "дека", "дали", "јас", "тој", "таа", "тие", "сум", "треба", "требаат", "нема", "може", "можеби", "мора",
            "многу", "нешто", "ништо", "некој", "никој", "сите", "колку", "каде", "зошто", "затоа", "тука", "таму", "уште", "веќе",
            "сега", "денес", "секогаш", "никогаш", "сакам", "мислам", "знаеме", "одиме", "правиш", "знаеш", "имаш", "беше", "време",
            "работа", "луѓе", "жени", "добро", "добра", "благодарам", "фала", "господине", "тоалет", "инженер", "вселената", "орбитата",
            "очекуваат", "дождови", "температурите", "паднат", "нулата", "текот", "ноќта", "возачите", "бидат", "двајца", "секако", "следете", "видиме", "менито",
            "вечерва", "повеќе", "молам", "госпоѓо", "ноќ", "куќа", "љубов", "заедно", "можам", "сакаш", "велам", "дојди", "ајде", "работев", "зборуваме", "својата",
            "минатата", "шанса", "еднаш", "бидеш", "градот", "мирен", "фабриката", "реката", "издигне", "утрото", "мостот", "патиштата", "поплавени", "ридот", "водата",
            "куќите", "подоцна", "велеа", "доаѓа", "утре", "дожд", "ветер", "снег", "цена", "пријател", "пријатели", "следниот", "недела", "месец", "илјада", "јануари",
            "музика", "воздух", "леб", "фирма", "пешки", "вреди", "светло",
        };

        private static readonly string[] AutoDetectWordsFinnish =
        {
            "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä", "sulkeutuu", "Soitetaan", "Soita", "Onnistui", "Mitä", "Etuovi", "tippiä", "antaa", "Onko", "Hidasta", "tuntia",
            "tilata", "päästä", "palolaitokselle", "hätänumeroon", "aikaa", "Tämä", "Sinulla", "Palauttaa", "Kiitos", "Arvostele", "Älä", "toimi", "televisiota", "takaisin", "reitin", "pitäisi", "palauttaa", "nopeamman", "mitään",
            "meidät", "maksaa", "kuullut", "kaikki", "jälkeen", "ihmiset", "hätäkeskukseen", "hiljaa", "haluat", "enää", "enemmän", "auttaa", "Tunkeilijahälytys", "Pysähdy", "Princen", "Käänny", "Kyllä", "Kiva", "Haluatko", "Haluan",
            "ei", "ole", "olen", "olet", "oli", "ollut", "olemme", "ovat", "olla", "olisi", "ennen", "jos", "vaan", "vai", "tai", "myös", "vain", "nyt", "sitten",
            "tuolla", "siellä", "sinne", "tänne", "hän", "hänen", "häntä", "minun", "minua", "minulle", "sinun", "sinua", "sinulle", "sinä", "meidän", "meille",
            "teidän", "heidän", "joku", "jotain", "mikään", "kukaan", "kaikkea", "paljon", "hyvin", "hyvä", "hyvää", "huomenta", "anteeksi", "totta", "ehkä", "varmasti",
            "tietenkin", "koskaan", "aina", "ikinä", "juuri", "ihan", "niin", "näin", "sillä", "siksi", "koska", "vaikka", "kuitenkin", "eikä", "eivät", "emme", "voi",
            "voin", "voit", "voimme", "voisi", "täytyy", "pitää", "tiedän", "tiedät", "tiedä", "luulen", "luulin", "tarkoitan", "ymmärrän", "tule", "tulee", "menee", "tehdä", "tein", "teit", "tehnyt", "sanoi", "sanoin", "sanoit", "sanoa", "nähdä", "näit", "nähnyt", "katso", "odota", "ota", "mies",
            "nainen", "tyttö", "lapsi", "ihminen", "päivä", "päivää", "yö", "yöllä", "aamu", "aamulla", "aika", "vuosi", "vuotta", "vuoden", "kerran", "kertaa", "koko",
            "kotiin", "koti", "elämä", "rakastan", "kuollut", "tapahtui", "tapahtunut", "tarvitsen", "apua", "rahaa", "hetki", "hetkinen", "nopeasti", "yhdessä",
            "yksin", "miksi", "missä", "miten", "milloin", "kuka", "joka", "jotka", "mihin", "silta", "joen", "kaupunki", "vesi", "talot", "tiet", "myöhemmin", "töissä",
            "myöhään", "valehtele", "mahdollisuus", "rehellinen",
            "huomenna", "eilen", "tänään", "odotetaan", "pohjoisessa", "pohjoiseen", "etelässä", "sataa", "sadetta", "rankkasadetta", "lämpötila", "laskee", "pakkasen",
            "puolelle", "autoilijoiden", "kannattaa", "erityisen", "varovaisia", "iltaa", "teillä", "pöytää", "kahdelle", "tietysti", "seuratkaa", "voisimmeko",
            "ruokalistan", "ensin", "kala", "erinomaista",
        };

        private static readonly string[] AutoDetectWordsRomanian =
        {
            "pentru", "oamenii", "decât", "[Vv]reau", "[Ss]înt", "Asteaptã", "Fãrã", "aici", "domnule", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "fãcut", "spune", "făcut", "când", "aici", "Asta", "văzut", "dacă", "câteva",
            "amândoi", "Când", "totuși", "mașină", "aceeași", "întâmplat", "niște", "ziua", "noastră", "cunoscut", "decat", "[Tt]rebuie", "[Aa]cum", "Poate", "vrea", "soare", "nevoie", "daca", "echilibrul", "vorbesti", "zeului",
            "atunci", "memoria", "soarele",
            "ești", "sunt", "suntem", "sunteți", "erau", "fost", "fie", "și", "sau", "dar", "că", "în", "despre", "până", "după", "înainte", "fără", "între", "peste",
            "unde", "cine", "cât", "câte", "acolo", "azi", "astăzi", "mâine", "mereu", "niciodată", "nimic", "nimeni", "cineva", "ceva", "toți", "toate", "toată",
            "foarte", "mult", "prea", "doar", "numai", "încă", "bine", "rău", "mulțumesc", "mersi", "scuze", "doamnă", "domnișoară", "ea", "mie", "ție", "tine", "nouă",
            "lor", "mea", "tău", "său", "nostru", "vostru", "vrei", "vrem", "poți", "putem", "știu", "știi", "știe", "cred", "crezi", "spun", "spui", "spus", "zic",
            "zis", "văd", "vezi", "haide", "fac", "faci", "oameni", "bărbat", "femeie", "băiat", "fată", "copil", "copii", "zi", "noapte", "noaptea", "dimineață",
            "timp", "ani", "anul", "acasă", "casă", "viață", "viața", "dragoste", "bani", "repede", "încet", "împreună", "singur", "păcat", "sigur", "desigur", "chiar",
            "deci", "așa", "astfel", "iar", "lucru", "lucrat", "lucrez", "târziu", "devreme", "orașul", "fabrica", "râul", "podul", "drumurile", "inundate", "privea",
            "apa", "casele", "văzuseră", "venind", "minți", "vorbim", "șansă", "trecut", "măcar", "dată", "ai fost",
        };

        // Czech and Slovak languages have many common words (especially when non flexed)
        private static readonly string[] AutoDetectWordsCzechAndSlovak =
        {
            "[Oo]n[ao]?", "[Jj]?si", "[Aa]le", "[Tt]en(to)?", "[Rr]ok", "[Tt]ak", "[Aa]by", "[Tt]am", "[Jj]ed(en|na|no)", "[Nn]ež", "[Aa]ni", "[Bb]ez", "[Dd]obr[ýáé]", "[Vv]šak", "[Cc]el[ýáé]", "[Nn]ov[ýáé]", "[Dd]ruh[ýáé]", "jsem",
            "poøádku", "Pojïme", "háje", "není", "Jdeme", "všecko", "jsme", "Prosím", "Vezmi", "když", "Takže", "Dìkuji", "prechádzku", "všetko", "Poïme", "potom", "Takže", "Neviem", "budúcnosti", "trochu"
        };
        // differences between Czech and Slovak languages / Czech words / please keep the words aligned between these languages for better comparison
        private static readonly string[] AutoDetectWordsCzech =
        {
            ".*[Řř].*", ".*[ůě].*", "[Bb]ýt", "[Jj]sem", "[Jj]si", "[Jj]á", "[Mm]ít", "[Aa]no", "[Nn]e",  "[Nn]ic", "[Dd]en", "[Jj]en", "[Cc]o", "[Jj]ak[o]?", "[Nn]ebo",  "[Pp]ři", "[Pp]ro", "[Pp]řed.*", "[Jj](ít|du|de|deme|dou)",
            "[Mm]ezi",  "[Jj]eště", "[Čč]lověk", "[Pp]odle", "[Dd]alší", "přejděte", "používat", "obrázky", "obrázek", "vašem", "vygenerovat",
            "nikdo", "někdo", "něco", "všechno", "teď", "proč", "kdo", "kdy", "včera", "zítra", "děkuji", "promiň", "paní", "lidé", "peníze", "rychle", "pomalu", "řekl",
            "řekla", "viděl", "udělat", "město", "řeka", "později", "viděli", "chci", "vím", "víš", "může", "můžu", "můžeš", "jsou", "byl", "byla", "bylo", "jsme", "jste", "nejsem", "jenom", "velmi", "moc", "dobře", "špatně", "pán", "dítě", "děti", "dívka", "holka", "kluk", "ráno", "domů", "miluji", "miluju",
            "mají", "měl", "měla", "říkám", "říkal", "říkali", "řekni", "pojď", "jdi", "jdeme", "dělat", "dělám", "děláš", "udělal", "nevím", "nechci", "nemůžu",
            "nemám", "potřebuju", "potřebuji", "jasně", "samozřejmě", "opravdu", "vážně", "tady", "pak", "protože", "jestli", "pokud", "nelži", "promluvit", "šanci",
            "loni", "upřímný", "nečekal", "stoupne", "dívala", "kopce", "bere", "domy", "přicházet",
            "očekává", "silný", "déšť", "teploty", "klesnou", "nulu", "řidiči", "měli", "silnicích", "zvlášť", "opatrní", "stůl", "pojďte",
            "mnou", "bychom", "nejdřív", "jídelní", "lístek", "vynikající", };
        // differences between Czech and Slovak languages / Slovak words / please keep the words aligned between these languages for better comparison
        private static readonly string[] AutoDetectWordsSlovak =
        {
            ".*[Ôô].*", ".*[ä].*",  "[Bb]yť", "[Ss]om",  "[Ss]i",  "[Jj]a", "[Mm]ať", "[Áá]no", "[Nn]ie", "[Nn]ič", "[Dd]eň", "[Ll]en", "[Čč]o", "[Aa]ko", "[Aa]?[Ll]ebo", "[Pp]ri", "[Pp]re", "[Pp]red.*", "([Íí]sť|[Ii](dem|de|deme|dú))",
            "[Mm]edzi", "[Ee]šte",  "[Čč]lovek", "[Pp]odľa", "[Ďď]alš(í|ia|ie)",
            "bolo", "bola", "bol", "boli", "sme", "ste", "sú", "nikto", "niečo", "niekto", "všetko", "všetci", "kým", "keď", "ak", "pretože", "kto", "kedy", "prečo",
            "koľko", "ktorý", "ktorá", "ktoré", "teraz", "zajtra", "iba", "veľmi", "dobre", "zle", "ďakujem", "prepáč", "prepáčte", "pani", "ľudia", "dievča", "dieťa",
            "deti", "roky", "milujem", "peniaze", "rýchlo", "pomaly", "majú", "mal", "môžem", "môžeš", "môže", "môžeme", "chcem", "viem", "vieš", "hovorím", "hovoríš", "hovoril", "hovorili", "povedz", "povedal", "povedala", "videl", "videli", "poď", "choď", "ideme", "urobiť", "urobil", "robíš",
            "mesto", "nečakal", "rieka", "stúpne", "pozerala", "neskôr", "prichádzať", "celú", "pracoval", "dlho", "neklam", "pozri", "ľúto", "porozprávať", "povedať",
            "šancu", "vlani", "odo", "mňa", "úprimný", "pokojné", "továreň", "nezavreli", "cesty", "vodou", "berie", "domy", "kopca",
            "sa", "stôl", "dvoch", "samozrejme", "poďte", "najprv", "vidieť", "jedálny", "lístok", "vynikajúca", "severe", "očakáva", "dážď", "klesnú", "vodiči",
            "cestách", "obzvlášť", "dobrý", "mohli", "jedlo", "práve", "aj", };

        private static readonly string[] AutoDetectWordsSlovenian =
        {
            "sem", "bom", "Zakaj", "lahko", "Kje", "Lahko", "Hvala", "Ampak", "nekaj", "Prosim", "prišla", "nisem", "denar", "ampak",
            "všeč", "videti", "stanovanje", "službo", "prosim", "naredila", "moram", "domov", "Vrzi", "Povej", "številko", "zaporu",
            "ugrabila", "ubila", "tvoja", "tudi", "tečna", "stvari", "rusko", "povedala", "obraz", "nalogo", "mislim", "govoriš",
            "Seveda", "Razmišljam", "Potem", "Nič", "Nisem", "Mogoče", "žensko", "žalostna", "človek", "Čisto", "znaš",
            "zlomi", "zgodilo", "zdaj", "zajtrk", "utrujena","ustrelila", "srečna", "čarovnik", "zaživeti",
            "Torej", "plasti","kanjon", "več", "prišlo", "presenečeni", "največje",
            "bo", "bil", "niso", "sva", "boš", "bomo", "boste", "bodo", "ki", "če", "ker", "kjer", "kaj", "kdo", "kdaj", "kateri", "katera", "tisti", "tista", "tukaj",
            "danes", "jutri", "včeraj", "vedno", "nikoli", "nihče", "nekdo", "vse", "vsi", "vsak", "zelo", "še", "prav", "slabo", "živjo", "adijo", "gospod", "gospa",
            "ljudje", "moški", "ženska", "fant", "punca", "dekle", "otrok", "otroci", "noč", "zjutraj", "enkrat", "življenje", "ljubim", "hitro", "počasi", "skupaj",
            "imajo", "imel", "imela", "morem", "hočem", "hočeš", "hoče", "hočemo", "nočem", "vem", "veš", "vemo", "rekel", "videl", "pridi", "pojdi", "gremo",
            "narediti", "naredil", "delaš", "preden", "pričakoval", "narasla", "pozneje", "prihajati", "hiše", "vodo", "tovarna", "zaprla", "celo", "delal", "pozno",
            "poslušaj", "žal", "morava", "pogovoriti", "reči", "priložnost", "lani", "odnaša", "mostu",
            "pričakujemo", "močan", "dež", "ponoči", "padle", "ničlo", "vozniki", "naj", "posebej", "previdni", "dober", "mizo", "sledite", "jedilnik", "nocoj", "odlična", "najprej", "videli",
        };

        private static readonly string[] AutoDetectWordsIcelandic =
        {
            "við",
            "Það",
            "með",
            "Hvað",
            "fyrir",
            "hvað",
            "verður",
            "segja",
            "þessu",
            "hérna",
            "maður",
            "Mér",
            "Guð",
            "þið",
            "svona",
            "okkur",
            "þau",
            "sé",
            "núna",
            "hefur",
            "hafa",
            "eða",
            "þegar",
            "þarna",
            "verð",
            "veist",
            "ætla",
            "okkar",
            "barnið",
            "eitthvað",
            "ólétt",
            "væri",
            "sagði",
            "Því",
            "ykkur",
            "annað",
            "Frábært",
            "Þakka",
            "góður",
            "Gerðu",
            "Fyrirgefðu",
            "verða",
            "kvöld",
            "alvöru",
            "Veistu",
            "Kannski",
            "Einmitt",
            "verðum",
            "líður",
            "höfum",
            "hugmynd",
            "Heyrðu",
            "skrýtið",
            "stefnumót",
            "skeið",
            "síðan",
            "hingað",
            "ótrúlegt",
            "Þekkirðu",
            "spýtustrákur",
            "miðskóla",
        };

        private static readonly string[] AutoDetectWordsLatvian =
        {
            "paldies", "neesmu", "nezinu", "viņš", "viņa", "viņu", "kungs",
            "esmu", "velns", "dievs", "pagaidi", "varoņi", "agrāk", "varbūt",
            "kas", "kur", "kad", "tāpēc", "laikam", "tikai", "ļoti",
            "jā", "lūdzu", "atvainojiet", "pierādi", "tiešām",
            "viņš", "viņa", "mēs", "jūs", "viņi", "viņas",
            "būt", "neesmu", "esmu", "varu", "nevaru", "gribu", "gaidi",
            "domāju", "zinu", "nezinu",
            "cilvēks", "pasaulē", "dzīve", "laiks",
            "bija", "pirms", "tika", "neviens", "ka", "tik", "ir", "nav", "būs", "būtu", "esi", "esam", "esat", "arī", "jau", "vēl", "tagad", "šeit", "tur",
            "mani", "tev", "tevi", "viņam", "viņai", "mums", "viņiem", "šis", "šī", "tas", "tā", "tie", "tās", "visi", "viss", "visu", "kaut", "kāds", "kāda",
            "neko", "nekas", "nekad", "vienmēr", "labi", "laba", "sveiki", "čau", "atvaino", "piedod", "vari", "varam", "varat", "jābūt", "vajag",
            "gribi", "zini", "domā", "saki", "teica", "teicu", "runā", "redzu", "redzi", "redzēju", "nāc", "nāk", "ej", "iet", "darīt", "izdarīju", "vīrs", "vīrietis",
            "sieviete", "puika", "meitene", "bērns", "cilvēki", "diena", "dienu", "nakts", "nakti", "rīts", "rītā", "laiku", "gads", "gadu", "gadiem", "reizi", "mājās",
            "mājas", "mīlu", "miris", "notika", "nauda", "naudu", "ātri", "lēnām", "kopā", "viens", "pati", "žēl", "nē", "pilsēta", "fabrika", "slēgta", "upe",
            "celsies", "strauji", "tilts", "ceļi", "ūdens", "vēlāk", "strādāju", "nemelo", "iespēja", "godīgs", "vairāk",
        };

        private static readonly string[] AutoDetectWordsLithuanian =
        {
            "ir", "yra", "nėra", "aš", "jis", "ji", "jūs", "jie", "jos", "mane", "mano", "tau", "tave", "tavo", "jam", "jai", "mums", "jums", "jiems", "čia", "dabar", "jau", "dar", "tik", "taip", "labai", "gerai", "gal", "galbūt", "nes", "jei", "jeigu", "kai", "kada", "kodėl", "kaip", "kas", "ką", "kur", "kuris", "kuri", "kiek", "visi", "viskas", "viską", "nieko", "niekas", "niekada", "visada", "kažkas", "kažką", "kiti", "kitas", "šis", "ši", "tas", "tai", "tie", "tos", "ačiū", "prašau", "atsiprašau", "labas", "sveiki", "tikrai", "tiesa", "žinau", "žinai", "nežinau", "manau", "galiu", "gali", "galime", "negaliu", "noriu", "nori", "norėjau", "reikia", "turiu", "turi", "turėjai", "turėjo", "esu", "esi", "esame", "buvo", "buvau", "buvai", "būti", "būsiu", "sakiau", "sakė", "sakyk", "pasakyti", "matau", "matai", "mačiau", "eik", "eina", "ateik", "daryti", "padariau", "vyras", "moteris", "berniukas", "mergaitė", "vaikas", "žmogus", "žmonės", "diena", "dieną", "naktis", "naktį", "rytas", "ryte", "laikas", "laiko", "metai", "metų", "kartą", "namo", "namuose", "gyvenimas", "myliu", "miręs", "atsitiko", "pinigai", "pinigų", "greitai", "lėtai", "kartu", "vienas", "viena", "gaila", "šūdas", "velnias", "apie", "tačiau", "mūsų", "jūsų", "tavęs", "manęs", "palauk", "supratau", "vyksta", "miestas", "ramus", "gamykla", "užsidarė", "upė", "pakils", "tiltas", "dingo", "keliai", "vandeniu", "žiūrėjo", "neša", "namus", "vėliau", "numatė", "buvai", "dirbau", "nemeluok", "šansą", "sąžiningas", "turėjai",
        };

        private static readonly string[] AutoDetectWordsEstonian =
        {
            "praegu", "kohe", "või", "kuni", "ainult", "kas", "väga", "selline",
            "palju", "selle", "nende", "üks", "ikkagi", "tegelikult", "metsad",
            "tere", "aitäh", "palun", "midagi", "mitte", "miks", "kuidas",
            "olen", "oota", "tahan", "mees", "naine", "poiss", "tüdruk",
            "ma", "sa", "tema", "meie", "teie", "nema", "nad",
            "see", "seda", "siin", "seal", "praegu", "veel", "päris",
            "tõesti", "muidugi", "vabandust", "hee", "kuule",
            "oli", "olnud", "olid", "oled", "oleme", "olete", "ole", "oleks", "enne", "kui", "keegi", "kedagi", "nii", "kes", "kus", "kuhu",
            "millal", "sest", "aga", "siis", "juba", "ikka", "nüüd", "sinna", "siia", "minu", "mulle", "sinu", "sulle", "teda", "talle", "meid",
            "meile", "teid", "teile", "neid", "neile", "kõik", "kõike", "hea", "võib", "võin", "võid", "võime", "saab", "saan", "saad", "saame", "peab",
            "pean", "pead", "peame", "tahad", "tahab", "tean", "tead", "teab", "arvan", "arvad", "mõtlen", "ütlesin", "ütles", "ütlesid", "räägi", "rääkima", "rääkisin",
            "näe", "näen", "näed", "nägin", "tule", "tuleb", "läheb", "teha", "tegin", "tegi", "tehtud", "inimene", "inimesed", "päev", "päeva", "öö",
            "öösel", "hommik", "hommikul", "aeg", "aega", "aasta", "aastat", "aastaid", "kord", "korda", "kodu", "koju", "kodus", "elu", "armastan", "surnud", "juhtus",
            "juhtunud", "raha", "hetk", "kiiresti", "aeglaselt", "koos", "üksi", "kahju", "olgu", "jah", "ei", "linn", "tehas", "suleti", "oodanud", "jõgi", "tõuseb",
            "sild", "teed", "vesi", "majad", "hiljem", "terve", "valeta", "võimalus",
            "homme", "oodatakse", "põhja", "poole", "tugevat", "vihma", "temperatuur", "langeb", "alla", "nulli", "autojuhid", "peaksid", "teedel", "eriti",
            "ettevaatlikud", "olema", "õhtul", "täna", "laud", "kahele", "tulge", "kaasa", "võiksime", "menüüd", "näha", "kala", "suurepärane", "õhtust",
        };

        private static readonly string[] AutoDetectWordsHindi =
        {
            "एक", "और", "को", "का", "यह", "सकते", "लिए", "करने", "भारतीय", "सकता", "भारत", "तकनीक", "कंप्यूटिंग", "उपकरण", "भाषाओं", "भाषा", "कंप्यूटर", "आप", "आपको", "अपने", "लेकिन", "करना", "सकता", "बहुत", "चाहते", "अच्छा", "वास्तव",
            "लगता", "इसलिए", "शेल्डन", "धन्यवाद।", "तरह", "करता", "चाहता", "कोशिश", "करते", "किया", "अजीब", "सिर्फ", "शुरू"
        };

        private static readonly string[] AutoDetectWordsUrdu =
        {
            "نہیں", "ایک", "ہیں", "کیا", "اور", "لئے", "ٹھیک", "ہوں", "مجھے", "تھا", "کرنے", "صرف", "ارے", "یہاں", "بہت", "لیکن", "ساتھ", "اپنے", "اچھا", "میرے", "چاہتا", "انہوں", "تمہیں"
        };

        private static readonly string[] AutoDetectWordsSinhalese =
        {
            "කරන්න", "නැහැ", "කියල", "මගේ", "එයා", "ඔයාට", "කැප්ටන්", "ඔයාගේ", "පුළුවන්", "හැම", "වගේ", "වෙන්න", "ඕනා", "නෙමේ", "තියෙන්නේ", "වගේම", "දන්නවා", "වෙනවා", "එහෙම", "හිතන්නේ", "කියලා", "කරනවා", "යන්න", "දෙයක්", "තියනවා",
            "හොදයි", "දෙන්න", "එකක්", "මොනාද", "කියන්න", "කරන්නේ", "වෙන්නේ", "රොජර්ස්", "දාන්න", "කරලා", "ඔයාව", "වෙලා", "කොහොමද", "කිවුවා", "ඔබට", "රාවුල්"
        };

        private static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            return AutoDetectGoogleLanguage(text, bestCount, out _);
        }

        /// <param name="text">The text to detect the language of.</param>
        /// <param name="bestCount">The minimum number of keyword hits for a match.</param>
        /// <param name="score">The number of keyword hits for the returned language.</param>
        /// <param name="detectChineseScript">Whether to detect Chinese sub (Simplified/Traditional) script.</param>
        private static string AutoDetectGoogleLanguage(string text, int bestCount, out int score, bool detectChineseScript = false)
        {
            // Score-based selection instead of first-match-wins: every language block below is
            // evaluated and records a candidate via Consider(); the candidate with the most
            // keyword hits wins (ties keep the earlier/higher-priority language via strict '>').
            // This fixes short-file mis-detection where an ambiguous token used to hand the clip
            // to whichever language happened to sit earliest in the fixed scan order (e.g. a short
            // Portuguese clip returning "es"). The per-language disambiguation guards (es vs pt/fr,
            // ru vs bg/uk/mk, hr vs sr/sl, cs vs sk, ...) are preserved exactly; only the outer
            // control flow changed from returning on the first hit to picking the strongest.
            var best = string.Empty;
            var bestScore = -1;
            var counts = CountWords(text);
            var cyrillicChecked = false;
            string cyrillicByLetters = null;

            void Consider(string languageCode, int score)
            {
                if (score > bestScore)
                {
                    best = languageCode;
                    bestScore = score;
                }
            }

            var count = counts.Get(AutoDetectWordsEnglish);
            if (count > bestCount)
            {
                var dutchCount = counts.Get(AutoDetectWordsDutch);
                if (dutchCount < count)
                {
                    Consider("en", count);
                }
            }

            var danishOnlyCount = counts.Get(AutoDetectWordsDanishOnly);
            var norwegianOnlyCount = counts.Get(AutoDetectWordsNorwegianOnly);
            count = counts.Get(AutoDetectWordsDanish);
            if (count > bestCount)
            {
                var dutchCount = counts.Get(AutoDetectWordsDutch);
                var swedishCount = counts.Get(AutoDetectWordsSwedish);
                var icelandicCount = counts.Get(AutoDetectWordsIcelandic);
                // Norwegian-only spellings outnumbering the Danish-only ones say Norwegian, unless
                // there are too few of them to mean anything (one odd word in a long Danish file).
                if ((norwegianOnlyCount < 2 || norwegianOnlyCount <= danishOnlyCount) && dutchCount < count && swedishCount < count)
                {
                    if (icelandicCount > count * 1.5)
                    {
                        Consider("is", count);
                    }
                    else
                    {
                        Consider("da", count);
                    }
                }
            }

            count = counts.Get(AutoDetectWordsNorwegian);
            if (count > bestCount)
            {
                var dutchCount = counts.Get(AutoDetectWordsDutch);
                var swedishCount = counts.Get(AutoDetectWordsSwedish);
                if ((danishOnlyCount < 2 || danishOnlyCount <= norwegianOnlyCount) && dutchCount < count && swedishCount < count)
                {
                    Consider("no", count);
                }
            }

            count = counts.Get(AutoDetectWordsSwedish);
            if (count > bestCount)
            {
                Consider("sv", count);
            }

            count = counts.Get(AutoDetectWordsSpanish);
            if (count > bestCount)
            {
                var frenchCount = counts.Get(AutoDetectWordsFrenchOnly); // not spanish words
                var portugueseCount = counts.Get(AutoDetectWordsPortugueseOnly); // not spanish words
                if (frenchCount < 2 && portugueseCount < 2)
                {
                    Consider("es", count);
                }
            }

            count = counts.Get(AutoDetectWordsItalian);
            if (count > bestCount)
            {
                var frenchCount = counts.Get(AutoDetectWordsFrenchOnly); // not italian words
                if (frenchCount < 2)
                {
                    Consider("it", count);
                }
            }

            count = counts.Get(AutoDetectWordsFrench);
            if (count > bestCount)
            {
                var romanianCount = counts.Get(AutoDetectWordsRomanianOnly);
                if (romanianCount < 5)
                {
                    Consider("fr", count);
                }
            }

            count = counts.Get(AutoDetectWordsPortuguese);
            if (count > bestCount)
            {
                var slovenianCount = counts.Get(AutoDetectWordsSlovenian);
                if (slovenianCount > count)
                {
                    Consider("sl", count);
                }
                else
                {
                    Consider("pt", count); // Portuguese
                }
            }

            count = counts.Get(AutoDetectWordsCatalan);
            if (count > bestCount)
            {
                Consider("ca", count); // Catalan
            }

            count = counts.Get(AutoDetectWordsGerman);
            if (count > bestCount)
            {
                Consider("de", count);
            }

            count = counts.Get(AutoDetectWordsDutch);
            var afrikaansCount = counts.Get(AutoDetectWordsAfrikaans);
            if (count > bestCount || afrikaansCount > bestCount)
            {
                // Afrikaans text hits the Dutch list too (het, van, een, ...); the spelling tells.
                var afrikaansOrDutch = GetAfrikaansOrDutchByLetters(text);
                if (count > bestCount)
                {
                    Consider(afrikaansOrDutch ?? "nl", count);
                }

                if (afrikaansCount > bestCount)
                {
                    Consider(afrikaansOrDutch ?? "af", afrikaansCount); // Afrikaans
                }
            }

            count = counts.Get(AutoDetectWordsPolish);
            if (count > bestCount)
            {
                var czechWordsCount = counts.Get(AutoDetectWordsCzech);
                if (czechWordsCount > count)
                {
                    Consider("cs", count);
                }
                else
                {
                    Consider("pl", count);
                }
            }

            count = counts.Get(AutoDetectWordsGreek);
            if (count > bestCount)
            {
                Consider("el", count); // Greek
            }

            count = counts.Get(AutoDetectWordsRussian);
            if (count > bestCount)
            {
                var bulgarianCount = counts.Get(AutoDetectWordsBulgarian);
                var ukrainianCount = counts.Get(AutoDetectWordsUkrainian);
                var macedonianCount = counts.Get(AutoDetectWordsMacedonian);
                var cyrillic = CyrillicByLetters();
                if (cyrillic != null)
                {
                    Consider(cyrillic, count);
                }
                else if (bulgarianCount > count)
                {
                    if (ukrainianCount > bulgarianCount && ukrainianCount > macedonianCount)
                    {
                        Consider("uk", count); // Ukrainian
                    }
                    else if (macedonianCount > bulgarianCount && macedonianCount > ukrainianCount)
                    {
                        Consider("mk", count); // Macedonian
                    }
                    else
                    {
                        Consider("bg", count); // Bulgarian
                    }
                }
                else if (ukrainianCount > count)
                {
                    Consider("uk", count); // Ukrainian
                }
                else
                {
                    var serbianCount = counts.Get(AutoDetectWordsSerbianCyrillic);
                    var serbianWordsOnlyCount = counts.Get(AutoDetectWordsSerbianCyrillicOnly);
                    if (serbianCount > count)
                    {
                        Consider("sr", count); // Serbian
                    }
                    else if (serbianWordsOnlyCount > 1)
                    {
                        Consider("sr", count); // Serbian
                    }
                    else
                    {
                        Consider("ru", count); // Russian
                    }
                }
            }

            count = counts.Get(AutoDetectWordsUkrainian);
            if (count > bestCount)
            {
                Consider(CyrillicByLetters() ?? "uk", count); // Ukrainian
            }

            count = counts.Get(AutoDetectWordsBulgarian);
            if (count > bestCount)
            {
                var macedonianCount = counts.Get(AutoDetectWordsMacedonian);
                var cyrillic = CyrillicByLetters();
                if (cyrillic != null)
                {
                    Consider(cyrillic, count);
                }
                else if (macedonianCount > count)
                {
                    Consider("mk", count);
                }
                else
                {
                    Consider("bg", count); // Bulgarian
                }
            }

            count = counts.Get(AutoDetectWordsAlbanian);
            if (count > bestCount)
            {
                Consider("sq", count); // Albanian
            }

            count = counts.Get(AutoDetectWordsArabic);
            if (count > bestCount)
            {
                var hebrewCount = counts.Get(AutoDetectWordsHebrew);
                var farsiCount = counts.Get(AutoDetectWordsFarsi);
                if (hebrewCount < count && farsiCount < count)
                {
                    Consider(GetArabicScriptLanguageByLetters(text) ?? "ar", count); // Arabic
                }
            }

            count = counts.Get(AutoDetectWordsHebrew);
            if (count > bestCount)
            {
                Consider("he", count); // Hebrew
            }

            count = counts.Get(AutoDetectWordsFarsi);
            if (count > bestCount)
            {
                Consider(GetArabicScriptLanguageByLetters(text) ?? "fa", count); // Farsi (Persian)
            }

            count = counts.Get(AutoDetectWordsCroatianAndSerbian);
            if (count > bestCount)
            {
                var croatianCount = counts.Get(AutoDetectWordsCroatian);
                var serbianCount = counts.Get(AutoDetectWordsSerbian);
                var slovenianCount = counts.Get(AutoDetectWordsSlovenian);
                if (croatianCount > serbianCount)
                {
                    if (slovenianCount > croatianCount)
                    {
                        Consider("sl", count);
                    }
                    else
                    {
                        Consider("hr", count); // Croatian
                    }
                }
                else if (slovenianCount > count)
                {
                    Consider("sl", count);
                }
                else
                {
                    Consider("sr", count); // Serbian
                }
            }

            count = counts.Get(AutoDetectWordsVietnamese);
            if (count > bestCount)
            {
                Consider("vi", count); // Vietnamese
            }

            count = counts.Get(AutoDetectWordsHungarian);
            if (count > bestCount)
            {
                Consider("hu", count); // Hungarian
            }

            count = counts.Get(AutoDetectWordsTurkish);
            if (count > bestCount)
            {
                Consider("tr", count); // Turkish
            }

            count = counts.Get(AutoDetectWordsIndonesian);
            if (count > bestCount)
            {
                Consider("id", count); // Indonesian
            }

            count = counts.Get(AutoDetectWordsTagalog);
            if (count > bestCount)
            {
                Consider("tl", count); // Tagalog / Filipino
            }

            count = counts.Get(AutoDetectWordsThai);
            if (count > 10 || count > bestCount)
            {
                Consider("th", count); // Thai
            }

            count = counts.Get(AutoDetectWordsKorean);
            if (count > 10 || count > bestCount)
            {
                Consider("ko", count); // Korean
            }

            count = counts.Get(AutoDetectWordsFinnish);
            if (count > bestCount)
            {
                Consider("fi", count); // Finnish
            }

            count = counts.Get(AutoDetectWordsRomanian);
            if (count > bestCount)
            {
                Consider("ro", count); // Romanian
            }

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
            {
                Consider("ja", count); // Japanese - not tested...
            }

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
            {
                var script = "zh";
                if (detectChineseScript)
                {
                    script = DetermineChineseScript(text);
                }
                
                Consider(script, count); // Chinese (simplified) - not tested...
            }

            count = counts.Get(AutoDetectWordsCzechAndSlovak);
            if (count > bestCount)
            {
                var lithuanianCount = counts.Get(AutoDetectWordsLithuanian);
                var finnishCount = counts.Get(AutoDetectWordsFinnish);
                var estonianCount = counts.Get(AutoDetectWordsEstonian);
                if (estonianCount > count && estonianCount > lithuanianCount && estonianCount > finnishCount)
                {
                    Consider("et", count);
                }
                else if (lithuanianCount <= count && finnishCount < count)
                {
                    int czechWordsCount = counts.Get(AutoDetectWordsCzech);
                    int slovakWordsCount = counts.Get(AutoDetectWordsSlovak);
                    if (czechWordsCount >= slovakWordsCount)
                    {
                        Consider("cs", Math.Max(count, czechWordsCount)); // Czech
                    }
                    else
                    {
                        Consider("sk", Math.Max(count, slovakWordsCount)); // Slovak
                    }
                }
            }

            // Czech and Slovak on their own lists too (the shared list above may miss a plain
            // line), but not on a single hit: "[Jj]a", "[Nn]e" and "[Dd]en" are everyday words
            // in German, Dutch and the Scandinavian languages as well.
            count = counts.Get(AutoDetectWordsCzech);
            if (count > bestCount && count > 1)
            {
                var slovakWordsCount = counts.Get(AutoDetectWordsSlovak);
                if (slovakWordsCount > count)
                {
                    Consider("sk", slovakWordsCount);
                }
                else
                {
                    Consider("cs", count);
                }
            }

            count = counts.Get(AutoDetectWordsSlovak);
            if (count > bestCount && count > 1)
            {
                var czechWordsCount = counts.Get(AutoDetectWordsCzech);
                if (czechWordsCount > count)
                {
                    Consider("cs", czechWordsCount);
                }
                else
                {
                    Consider("sk", count);
                }
            }

            count = counts.Get(AutoDetectWordsSlovenian);
            if (count > bestCount)
            {
                var estonianCount = counts.Get(AutoDetectWordsEstonian);
                if (estonianCount > count)
                {
                    Consider("et", count);
                }
                else
                {
                    Consider("sl", count);
                }
            }

            count = counts.Get(AutoDetectWordsEstonian);
            if (count > bestCount)
            {
                Consider("et", count);
            }

            count = counts.Get(AutoDetectWordsLatvian);
            var lithuanianWordCount = counts.Get(AutoDetectWordsLithuanian);
            if (count > bestCount * 1.2 || lithuanianWordCount > bestCount)
            {
                // kas, kur, kad, ir, jau, ... are both languages'; the letters are not.
                var baltic = GetBalticLanguageByLetters(text);
                if (count > bestCount * 1.2)
                {
                    Consider(baltic ?? "lv", count);
                }

                if (lithuanianWordCount > bestCount)
                {
                    Consider(baltic ?? "lt", lithuanianWordCount);
                }
            }

            count = counts.Get(AutoDetectWordsHindi);
            if (count > bestCount)
            {
                Consider("hi", count);
            }

            count = counts.Get(AutoDetectWordsUrdu);
            if (count > bestCount)
            {
                Consider(GetArabicScriptLanguageByLetters(text) ?? "ur", count);
            }

            count = counts.Get(AutoDetectWordsSinhalese);
            if (count > bestCount)
            {
                Consider("si", count);
            }

            count = counts.Get(AutoDetectWordsMacedonian);
            if (count > bestCount)
            {
                Consider(CyrillicByLetters() ?? "mk", count);
            }

            count = counts.Get(AutoDetectWordsIcelandic);
            if (count > bestCount)
            {
                Consider("is", count);
            }

            score = bestScore;
            return best;

            // "mk", "sr", "uk", "bg", "ru" or null, scanning the letters once no matter how many blocks ask.
            string CyrillicByLetters()
            {
                if (!cyrillicChecked)
                {
                    cyrillicByLetters = GetCyrillicLanguageByLetters(text, counts);
                    cyrillicChecked = true;
                }

                return cyrillicByLetters;
            }
        }

        private static string DetermineChineseScript(string text)
{
    if (string.IsNullOrEmpty(text))
    {
        return "zh"; // Default to Simplified/generic
    }

    // Comprehensive Traditional Chinese character markers
    int traditionalCount = GetCountContains(text, 
        // Pronouns, Demographics & Measure Words
        "們", "這", "個", "樣", "兩", "幾", "誰", "什", "傢", "伙",
        // Verbs & Actions
        "說", "聽", "看", "見", "會", "能", "開", "關", "讓", "給", 
        "過", "來", "進", "帶", "選", "問", "愛", "買", "賣", "辦",
        "寫", "讀", "講", "說", "試", "尋", "找", "換", "換", "搬",
        "跑", "飛", "走", "過", "進", "出", "回", "到", "起", "坐",
        // Adverbs, Prepositions & Particles
        "還", "與", "後", "點", "為", "著", "麼", "裡", "裏", "邊",
        "對", "從", "向", "往", "把", "被", "比", "象", "像", "經",
        // Everyday Nouns & Objects
        "國", "學", "書", "車", "門", "電", "機", "話", "語", "時",
        "廣", "體", "灣", "臺", "台", "視", "頻", "錢", "萬", "東",
        "風", "飯", "頭", "長", "遠", "親", "樂", "報", "隊", "師",
        "員", "網", "劃", "園", "難", "雙", "無", "義", "業", "廠",
        "產", "醫", "藥", "點", "實", "際", "關", "應", "該", "發",
        // Additional Media, Emotion & Abstract Concepts
        "歡", "迎", "愛", "情", "感", "覺", "意", "思", "想", "法",
        "聲", "音", "樂", "器", "戲", "劇", "影", "片", "圖", "畫",
        "燈", "鏡", "筆", "紙", "類", "別", "種", "類", "結", "構"
    );

    // Corresponding Simplified Chinese character markers for contrast
    int simplifiedCount = GetCountContains(text, 
        // Pronouns, Demographics & Measure Words
        "们", "这", "个", "样", "两", "几", "谁", "什", "家", "伙",
        // Verbs & Actions
        "说", "听", "看", "见", "会", "能", "开", "关", "让", "给", 
        "过", "来", "进", "带", "选", "问", "爱", "买", "卖", "办",
        "写", "读", "讲", "说", "试", "寻", "找", "换", "换", "搬",
        "跑", "飞", "走", "过", "进", "出", "回", "到", "起", "坐",
        // Adverbs, Prepositions & Particles
        "还", "与", "后", "点", "为", "着", "么", "里", "里", "边",
        "对", "从", "向", "往", "把", "被", "比", "象", "像", "经",
        // Everyday Nouns & Objects
        "国", "学", "书", "车", "门", "电", "机", "话", "语", "时",
        "广", "体", "湾", "台", "台", "视", "频", "钱", "万", "东",
        "风", "饭", "头", "长", "远", "亲", "乐", "报", "队", "师",
        "员", "网", "划", "园", "难", "双", "无", "义", "业", "厂",
        "产", "医", "药", "点", "实", "际", "关", "应", "该", "发",
        // Additional Media, Emotion & Abstract Concepts
        "欢", "迎", "爱", "情", "感", "觉", "意", "思", "想", "法",
        "声", "音", "乐", "器", "戏", "剧", "影", "片", "图", "画",
        "灯", "镜", "笔", "纸", "类", "别", "种", "类", "结", "构"
    );

    if (traditionalCount > simplifiedCount && traditionalCount >= 2)
    {
        return "zh-Hant";
    }

    if (simplifiedCount > traditionalCount && simplifiedCount >= 2)
    {
        return "zh-Hans";
    }

    return "zh"; // Default / Simplified Chinese
}
        
        public static string AutoDetectGoogleLanguage(Subtitle subtitle, bool detectChineseScript = false)
        {
            return AutoDetectGoogleLanguageOrNull(subtitle, detectChineseScript) ?? "en";
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle, int maxNumberOfSubtitlesToCheck)
        {
            const int averageLength = 40;
            var max = subtitle.Paragraphs.Count;
            var sb = new StringBuilder(max * averageLength);
            var index = 0;
            var emptyLines = 0;
            while (index < max && index < maxNumberOfSubtitlesToCheck + emptyLines)
            {
                var text = subtitle.Paragraphs[index].Text;
                if (string.IsNullOrEmpty(text))
                {
                    emptyLines++;
                }
                else
                {
                    sb.AppendLine(text);
                }

                index++;
            }

            var allText = sb.ToString();
            string languageId = AutoDetectGoogleLanguage(allText, maxNumberOfSubtitlesToCheck / 14);
            if (string.IsNullOrEmpty(languageId))
            {
                languageId = GetEncodingViaLetter(allText);
            }

            if (string.IsNullOrEmpty(languageId))
            {
                return "en";
            }

            return languageId;
        }

        public class LanguageForAutoDetect
        {
            public string LanguageCode { get; set; }
            public string[] Words { get; set; }
            public int WordCount { get; set; }
        }

        public static LanguageForAutoDetect[] GetLanguagesWithCount(string text)
        {
            var list = new LanguageForAutoDetect[]
            {
                new LanguageForAutoDetect
                {
                    LanguageCode = "en",
                    Words = AutoDetectWordsEnglish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "da",
                    Words = AutoDetectWordsDanish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "no",
                    Words = AutoDetectWordsNorwegian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sv",
                    Words = AutoDetectWordsSwedish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "es",
                    Words = AutoDetectWordsSpanish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "it",
                    Words = AutoDetectWordsItalian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "fr",
                    Words = AutoDetectWordsFrench,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "pt",
                    Words = AutoDetectWordsPortuguese,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "de",
                    Words = AutoDetectWordsGerman,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "nl",
                    Words = AutoDetectWordsDutch,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "pl",
                    Words = AutoDetectWordsPolish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "el",
                    Words = AutoDetectWordsGreek,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ru",
                    Words = AutoDetectWordsRussian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "bg",
                    Words = AutoDetectWordsBulgarian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "uk",
                    Words = AutoDetectWordsUkrainian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sq",
                    Words = AutoDetectWordsAlbanian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ar",
                    Words = AutoDetectWordsArabic,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "fa",
                    Words = AutoDetectWordsFarsi,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "he",
                    Words = AutoDetectWordsHebrew,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "vi",
                    Words = AutoDetectWordsVietnamese,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "hu",
                    Words = AutoDetectWordsHungarian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "tr",
                    Words = AutoDetectWordsTurkish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "-",
                    Words = AutoDetectWordsCroatianAndSerbian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "hr",
                    Words = AutoDetectWordsCroatian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sr",
                    Words = AutoDetectWordsSerbian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sr",
                    Words = AutoDetectWordsSerbianCyrillic,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sr-cy",
                    Words = AutoDetectWordsSerbianCyrillicOnly,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "id",
                    Words = AutoDetectWordsIndonesian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "tl",
                    Words = AutoDetectWordsTagalog,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ca",
                    Words = AutoDetectWordsCatalan,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "af",
                    Words = AutoDetectWordsAfrikaans,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "th",
                    Words = AutoDetectWordsThai,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ko",
                    Words = AutoDetectWordsKorean,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "mk",
                    Words = AutoDetectWordsMacedonian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "fi",
                    Words = AutoDetectWordsFinnish,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ro",
                    Words = AutoDetectWordsRomanian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "-",
                    Words = AutoDetectWordsCzechAndSlovak,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "cs",
                    Words = AutoDetectWordsCzech,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sk",
                    Words = AutoDetectWordsSlovak,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "sl",
                    Words = AutoDetectWordsSlovenian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "is",
                    Words = AutoDetectWordsIcelandic,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "lv",
                    Words = AutoDetectWordsLatvian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "lt",
                    Words = AutoDetectWordsLithuanian,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "hi",
                    Words = AutoDetectWordsHindi,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "ur",
                    Words = AutoDetectWordsUrdu,
                },
                new LanguageForAutoDetect
                {
                    LanguageCode = "si",
                    Words = AutoDetectWordsSinhalese,
                },
            };

            var counts = CountWords(text);
            foreach (var item in list)
            {
                item.WordCount = counts.Get(item.Words);
            }

            return list;
        }

        public static string AutoDetectGoogleLanguageOrNull(Subtitle subtitle, bool detectChineseScript = false)
        {
            var allText = GetNonEmptyTexts(subtitle, 500000, out var nonEmptyCount);
            var languageId = AutoDetectGoogleLanguage(allText, nonEmptyCount / 14, out var score, detectChineseScript);
            if (string.IsNullOrEmpty(languageId))
            {
                languageId = GetEncodingViaLetter(allText);
            }

            if (string.IsNullOrEmpty(languageId))
            {
                languageId = null;
            }

            return languageId;
        }

        /// <summary>
        /// Like <see cref="AutoDetectGoogleLanguageOrNull"/>, but only from dictionary words -
        /// without the letter-statistics fallback, which also "recognizes" text decoded with the
        /// wrong code page (Arabic read as Cyrillic still looks like Russian letters).
        /// </summary>
        /// <param name="subtitle">Subtitle to detect the language of.</param>
        /// <param name="score">Keyword hits for the returned language - higher is more certain.</param>
        public static string AutoDetectGoogleLanguageFromWordsOrNull(Subtitle subtitle, out int score)
        {
            var allText = GetNonEmptyTexts(subtitle, 500000, out var nonEmptyCount);
            var languageId = AutoDetectGoogleLanguage(allText, nonEmptyCount / 14, out score);
            return string.IsNullOrEmpty(languageId) ? null : languageId;
        }

        /// <summary>
        /// The texts of the non-empty lines, one per line, stopping once past
        /// <paramref name="stopAfterBytes"/> and trimmed at the end - what a copy of the subtitle
        /// with RemoveEmptyLines and GetAllTexts gave, without deep-copying every paragraph.
        /// </summary>
        /// <param name="subtitle">The subtitle to read.</param>
        /// <param name="stopAfterBytes">Stop collecting texts once past this many characters.</param>
        /// <param name="nonEmptyCount">All non-empty lines, also those past the stop.</param>
        private static string GetNonEmptyTexts(Subtitle subtitle, int stopAfterBytes, out int nonEmptyCount)
        {
            nonEmptyCount = 0;
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Text.IsOnlyControlCharactersOrWhiteSpace())
                {
                    continue;
                }

                nonEmptyCount++;
                if (sb.Length <= stopAfterBytes)
                {
                    sb.AppendLine(p.Text);
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Alias for AutoDetectGoogleLanguageOrNull for backward compatibility.
        /// </summary>
        public static string AutoDetectGoogleLanguageOrNull2(Subtitle subtitle)
        {
            return AutoDetectGoogleLanguageOrNull(subtitle);
        }

        public static string AutoDetectLanguageName(string languageName, Subtitle subtitle)
        {
            if (string.IsNullOrEmpty(languageName))
            {
                languageName = "en_US";
            }

            var bestCount = subtitle.Paragraphs.Count / 14;

            var text = subtitle.GetAllTexts();
            var counts = CountWords(text);
            var dictionaryNames = Utilities.GetDictionaryLanguages();

            var containsEnGb = false;
            var containsEnUs = false;
            var containsHrHr = false;
            var containsSrLatn = false;
            foreach (var name in dictionaryNames)
            {
                if (name.Contains("[en_GB]"))
                {
                    containsEnGb = true;
                }

                if (name.Contains("[en_US]"))
                {
                    containsEnUs = true;
                }

                if (name.Contains("[hr_HR]"))
                {
                    containsHrHr = true;
                }

                if (name.Contains("[sr_Latn]"))
                {
                    containsSrLatn = true;
                }
            }

            foreach (var name in dictionaryNames)
            {
                var shortName = string.Empty;
                var start = name.IndexOf('[');
                var end = name.IndexOf(']');
                if (start >= 0 && end > start)
                {
                    start++;
                    shortName = name.Substring(start, end - start);
                }

                int count;
                switch (shortName.Replace("-", "_").ToLowerInvariant())
                {
                    case "da_dk":
                        count = counts.Get(AutoDetectWordsDanish);
                        if (count > bestCount)
                        {
                            int norwegianCount = counts.Get(AutoDetectWordsNorwegianOnly);
                            int dutchCount = counts.Get(AutoDetectWordsDutch);
                            if (norwegianCount < 2 && dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "nb_no":
                        count = counts.Get(AutoDetectWordsNorwegian);
                        if (count > bestCount)
                        {
                            int danishCount = counts.Get(AutoDetectWordsDanishOnly);
                            int dutchCount = counts.Get(AutoDetectWordsDutch);
                            if (danishCount < 2 && dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "sv_se":
                        count = counts.Get(AutoDetectWordsSwedish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "en_us":
                        count = counts.Get(AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            int dutchCount = counts.Get(AutoDetectWordsDutch);
                            if (dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                                if (containsEnGb)
                                {
                                    int usCount = counts.Get(AutoDetectWordsEnglishUs);
                                    int gbCount = counts.Get(AutoDetectWordsEnglishGb);
                                    if (gbCount > usCount)
                                    {
                                        languageName = "en_GB";
                                    }
                                }
                            }
                        }
                        break;
                    case "en_gb":
                        count = counts.Get(AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            int dutchCount = counts.Get(AutoDetectWordsDutch);
                            if (dutchCount < count)
                            {
                                languageName = shortName;
                                bestCount = count;
                                if (containsEnUs)
                                {
                                    int usCount = counts.Get(AutoDetectWordsEnglishUs);
                                    int gbCount = counts.Get(AutoDetectWordsEnglishGb);
                                    if (gbCount < usCount)
                                    {
                                        languageName = "en_US";
                                    }
                                }
                            }
                        }
                        break;
                    case "es_any":
                    case "es_es":
                        count = counts.Get(AutoDetectWordsSpanish);
                        if (count > bestCount)
                        {
                            int frenchCount = counts.Get(AutoDetectWordsFrenchOnly); // not spanish words
                            int portugueseCount = counts.Get(AutoDetectWordsPortugueseOnly); // not spanish words
                            if (frenchCount < 2 && portugueseCount < 2)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "it_it":
                        count = counts.Get(AutoDetectWordsItalian);
                        if (count > bestCount)
                        {
                            int frenchCount = counts.Get(AutoDetectWordsFrenchOnly); // not italian words
                            int spanishCount = counts.Get(AutoDetectWordsSpanishOnly); // not italian words
                            if (frenchCount < 2 && spanishCount < 2)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "fr_fr":
                        count = counts.Get(AutoDetectWordsFrench);
                        if (count > bestCount)
                        {
                            int romanianCount = counts.Get(AutoDetectWordsRomanianOnly);
                            int spanishCount = counts.Get(AutoDetectWordsSpanishOnly); // not french words
                            int italianCount = counts.Get(AutoDetectWordsItalian);
                            if (romanianCount < 5 && spanishCount < 2 && italianCount < 2)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "de_de":
                        count = counts.Get(AutoDetectWordsGerman);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "nl_nl":
                        count = counts.Get(AutoDetectWordsDutch);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "pl_pl":
                        count = counts.Get(AutoDetectWordsPolish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "el_gr":
                        count = counts.Get(AutoDetectWordsGreek);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ru_ru":
                        count = counts.Get(AutoDetectWordsRussian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "uk_ua":
                        count = counts.Get(AutoDetectWordsUkrainian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ro_ro":
                        count = counts.Get(AutoDetectWordsRomanian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "hr_hr": // Croatian
                        count = counts.Get(AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            bestCount = count;
                            languageName = shortName;
                            if (containsSrLatn)
                            {
                                int croatianCount = counts.Get(AutoDetectWordsCroatian);
                                int serbianCount = counts.Get(AutoDetectWordsSerbian);
                                if (serbianCount > croatianCount)
                                {
                                    languageName = "sr-Latn";
                                }
                            }
                        }
                        break;
                    case "sr_latn": // Serbian (Latin)
                        count = counts.Get(AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                            if (containsHrHr)
                            {
                                int croatianCount = counts.Get(AutoDetectWordsCroatian);
                                int serbianCount = counts.Get(AutoDetectWordsSerbian);
                                if (serbianCount < croatianCount)
                                {
                                    languageName = "hr_HR";
                                }
                            }
                        }
                        break;
                    case "sr": // Serbian (Cyrillic)
                        count = counts.Get(AutoDetectWordsSerbianCyrillic);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "pt_pt": // Portuguese Portugal
                    case "pt_br": // Portuguese Brazil
                        count = counts.Get(AutoDetectWordsPortuguese);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "hu_hu": // Hungarian
                        count = counts.Get(AutoDetectWordsHungarian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "cs_cz": // Czech
                        count = counts.Get(AutoDetectWordsCzech);
                        if (count > bestCount)
                        {
                            var lithuanianCount = counts.Get(AutoDetectWordsLithuanian);
                            if (count > lithuanianCount)
                            {
                                languageName = shortName;
                                bestCount = count;
                            }
                        }
                        break;
                    case "sk_sk": // Slovak
                        count = counts.Get(AutoDetectWordsSlovak);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "lv_lv": // Latvian
                        count = counts.Get(AutoDetectWordsLatvian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "lt_lt": // Lithuanian
                    case "lt":    // Lithuanian (Neutral)
                        count = counts.Get(AutoDetectWordsLithuanian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "hi_in": // Hindi
                    case "hi":
                        count = counts.Get(AutoDetectWordsHindi);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ur_ur": // Urdu
                    case "ur":
                        count = counts.Get(AutoDetectWordsUrdu);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "si_si": // Sinhalese
                    case "si_lk": // Sinhala (Sri Lanka)
                    case "si":
                        count = counts.Get(AutoDetectWordsSinhalese);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "tr_tr": // Turkish
                        count = counts.Get(AutoDetectWordsTurkish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "he_il": // Hebrew
                        count = counts.Get(AutoDetectWordsHebrew);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "vi_vn": // Vietnamese
                        count = counts.Get(AutoDetectWordsVietnamese);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ar": // Arabic
                    case "ar_ar":
                        count = counts.Get(AutoDetectWordsArabic);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "sq_al": // Albanian
                        count = counts.Get(AutoDetectWordsAlbanian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "id_id": // Indonesian
                        count = counts.Get(AutoDetectWordsIndonesian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "ko_kr": // Korean
                        count = counts.Get(AutoDetectWordsKorean);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "mk_mk": // Macedonian
                        count = counts.Get(AutoDetectWordsMacedonian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "fa": // Farsi (Persian)
                    case "fa_ir":
                        count = counts.Get(AutoDetectWordsFarsi);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "sl": // Slovenian
                    case "sl_si":
                        count = counts.Get(AutoDetectWordsSlovenian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                    case "is_is": // Icelandic
                    case "is":    // Icelandic (Neutral)
                        count = counts.Get(AutoDetectWordsIcelandic);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            bestCount = count;
                        }
                        break;
                }
            }
            return languageName;
        }

        private static Encoding DetectJapaneseOrChineseEncoding(byte[] buffer)
        {
            var shiftJisText = Encoding.GetEncoding(932).GetString(buffer);
            if (CountCharsInRange(shiftJisText, '\u3040', '\u30FF') > 10 && CountChars(shiftJisText, "\uFFFD") < 3) // Japanese kana
            {
                return Encoding.GetEncoding(932);
            }

            var gbkText = Encoding.GetEncoding(936).GetString(buffer);
            var big5Text = Encoding.GetEncoding(950).GetString(buffer);
            var isGbk = IsCjkText(gbkText);
            var isBig5 = IsCjkText(big5Text);
            var simplifiedCount = CountChars(gbkText, "\u4EEC\u8FD9\u8BF4\u6765\u4E3A\u5BF9\u6CA1\u8FC7\u56FD\u65F6\u4E2A\u4F1A\u5B66\u8BF7");
            var traditionalCount = CountChars(big5Text, "\u5011\u9019\u8AAA\u4F86\u70BA\u5C0D\u6C92\u904E\u570B\u6642\u500B\u6703\u5B78\u8ACB");
            if (isGbk && (!isBig5 || simplifiedCount >= traditionalCount))
            {
                return Encoding.GetEncoding(936); // Simplified Chinese
            }

            if (isBig5)
            {
                return Encoding.GetEncoding(950); // Traditional Chinese
            }

            return null;
        }

        private static bool IsCjkText(string text)
        {
            return CountCharsInRange(text, '\u4E00', '\u9FFF') > 10 && CountChars(text, "\uFFFD") < 3;
        }

        private static int CountCharsInRange(string text, char from, char to)
        {
            var count = 0;
            foreach (var ch in text)
            {
                if (ch >= from && ch <= to)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountChars(string text, string chars)
        {
            var count = 0;
            foreach (var ch in text)
            {
                if (chars.IndexOf(ch) >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        public static Encoding DetectAnsiEncoding(byte[] buffer)
        {
            try
            {
                var russianEncoding = Encoding.GetEncoding(1251); // Cyrillic
                var textEnc1251 = russianEncoding.GetString(buffer);
                var counts1251 = CountWords(textEnc1251);
                var cyrillicCount = counts1251.Get(AutoDetectWordsRussianShort) + counts1251.Get(AutoDetectWordsSerbianCyrillic) +
                                    counts1251.Get(AutoDetectWordsBulgarianShort) + counts1251.Get(AutoDetectWordsMacedonian) +
                                    counts1251.Get(AutoDetectWordsUkrainian);
                var koreanCount = GetCount(Encoding.GetEncoding(949).GetString(buffer), AutoDetectWordsKorean);
                
                // Cyrillic bytes read as 1253 are Greek letters too, and two-letter Greek words
                // (να, το, με, ...) match such junk by chance - so Greek has to beat the Cyrillic count.
                var greekEncoding = Encoding.GetEncoding(1253); // Greek
                var greekCount = GetCount(greekEncoding.GetString(buffer), AutoDetectWordsGreek);
                if (greekCount > 5 && greekCount > cyrillicCount)
                {
                    if (koreanCount > greekCount)
                    {
                        return Encoding.GetEncoding(949); // Korean
                    }

                    return greekEncoding;
                }

                if (counts1251.Get(AutoDetectWordsRussianShort) > 5) // Russian
                {
                    return russianEncoding;
                }

                var wordMinCount = buffer.Length / 300;

                if (counts1251.Get(AutoDetectWordsSerbianCyrillic) > wordMinCount)
                {
                    return russianEncoding;
                }

                if (counts1251.Get(AutoDetectWordsBulgarianShort) > 5) // Bulgarian
                {
                    return russianEncoding;
                }


                if (counts1251.Get(AutoDetectWordsSerbianCyrillic) > wordMinCount) // Serbian
                {
                    return russianEncoding;
                }

                var encoding1250 = Encoding.GetEncoding(1250); // Central European/Eastern European: Polish, Czech, Slovak, Hungarian, Slovene, Bosnian, Croatian, Serbian (Latin script), Romanian (before 1993 spelling reform) and Albanian
                var textEnc1250 = encoding1250.GetString(buffer);
                var counts1250 = CountWords(textEnc1250);
                if (counts1250.Get(AutoDetectWordsCroatianAndSerbian) > wordMinCount)
                {
                    return encoding1250;
                }

                if (counts1250.Get(AutoDetectWordsCzechAndSlovak) > wordMinCount)
                {
                    return encoding1250;
                }

                if (counts1250.Get(AutoDetectWordsHungarian) > wordMinCount)
                {
                    return encoding1250;
                }

                var encoding1252 = Encoding.GetEncoding(1252); // Latin - English and some other Western languages
                var textEnc1252 = encoding1252.GetString(buffer);
                var counts1252 = CountWords(textEnc1252);

                // Turkish: the dotless ı, ş and ğ (bytes 0xFD, 0xFE, 0xF0 in 1254) decode as ý, þ, ð in 1252,
                // so a text whose Turkish word count grows when read as 1254 is Turkish. Checked before the
                // Western European 1252 languages so their shared short words cannot steal it.
                var encoding1254 = Encoding.GetEncoding(1254);
                var turkishText = encoding1254.GetString(buffer);
                var turkish1254Count = GetCount(turkishText, AutoDetectWordsTurkish);
                if (turkish1254Count > wordMinCount / 2 && turkish1254Count > counts1252.Get(AutoDetectWordsTurkish))
                {
                    return encoding1254;
                }
                var pol1252Count = counts1252.Get(AutoDetectWordsPolish);
                var pol1250Count = counts1250.Get(AutoDetectWordsPolish);
                var encoding28592 = Encoding.GetEncoding(28592);
                var pol28592Count = GetCount(encoding28592.GetString(buffer), AutoDetectWordsPolish);
                if (pol1252Count > wordMinCount || pol1250Count > wordMinCount)
                {
                    if (pol28592Count > pol1250Count && pol28592Count > pol1252Count)
                    {
                        return encoding28592;
                    }

                    return pol1252Count > pol1250Count ? encoding1252 : encoding1250;
                }

                var dutchCount1252 = counts1252.Get(AutoDetectWordsDutch);
                if (dutchCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var danishCount1252 = counts1252.Get(AutoDetectWordsDanish);
                if (danishCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var swedishCount1252 = counts1252.Get(AutoDetectWordsSwedish);
                if (swedishCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var germanCount1252 = counts1252.Get(AutoDetectWordsGerman);
                if (germanCount1252 > wordMinCount)
                {
                    return encoding1252;
                }

                var spanishCount1252 = counts1252.Get(AutoDetectWordsSpanish);
                if (textEnc1252.Contains('¡'))
                {
                    spanishCount1252 += 5;
                }
                if (textEnc1252.Contains('¿'))
                {
                    spanishCount1252 += 5;
                }
                if (spanishCount1252 > wordMinCount && (textEnc1252.Contains('ú') || textEnc1252.Contains('í') || textEnc1252.Contains('ú') || textEnc1252.Contains('ó') || textEnc1252.Contains('é') || textEnc1252.Contains('ñ')))
                {
                    var weirdChars = CountWeirdCharacters(textEnc1252);
                    if (weirdChars < 3)
                    {
                        return encoding1252;
                    }
                }

                var russianEncoding28595 = Encoding.GetEncoding(28595); // Russian
                if (GetCount(russianEncoding28595.GetString(buffer), AutoDetectWordsRussianShort) > 5) // Russian
                {
                    return russianEncoding28595;
                }

                var thaiEncoding = Encoding.GetEncoding(874); // Thai
                var thaiCount = GetCount(thaiEncoding.GetString(buffer), AutoDetectWordsThai);
                var koreanEncoding = Encoding.GetEncoding(949); // Korean
                if (thaiCount > 10 || koreanCount > 10)
                {
                    return thaiCount > koreanCount ? thaiEncoding : koreanEncoding;
                }

                var arabicEncoding = Encoding.GetEncoding(1256); // Arabic
                var hebrewEncoding = Encoding.GetEncoding(1255); // Hebrew
                if (GetCount(arabicEncoding.GetString(buffer), AutoDetectWordsArabic) > 5)
                {
                    if (GetCount(hebrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 10)
                    {
                        return hebrewEncoding;
                    }

                    return arabicEncoding;
                }
                if (GetCount(hebrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 5)
                {
                    return hebrewEncoding;
                }

                if (counts1250.Get(AutoDetectWordsRomanianShort) > 99)
                {
                    return encoding1250;
                }

                var encoding28591 = Encoding.GetEncoding(28591);
                var frenchCount1252 = counts1252.Get(AutoDetectWordsFrench);
                if (frenchCount1252 > wordMinCount)
                {
                    var frenchCount28591 = GetCount(encoding28591.GetString(buffer), AutoDetectWordsFrench);
                    return frenchCount28591 > frenchCount1252 ? encoding28591 : encoding1252;
                }

                var portugueseCount1252 = counts1252.Get(AutoDetectWordsPortuguese);
                if (portugueseCount1252 > wordMinCount)
                {
                    var portugueseCount28591 = GetCount(encoding28591.GetString(buffer), AutoDetectWordsPortuguese);
                    return portugueseCount28591 > portugueseCount1252 ? encoding28591 : encoding1252;
                }

                if (turkish1254Count > wordMinCount)
                {
                    return encoding1254;
                }

                if (DetectJapaneseOrChineseEncoding(buffer) is { } cjkEncoding)
                {
                    return cjkEncoding;
                }

                var encoding = EncodingTools.DetectInputCodepage(buffer);
                if (encoding.CodePage == 28599)
                {
                    return Encoding.GetEncoding(1254); // prefer 1254 over 28599 
                }

                if (!Configuration.Settings.General.DefaultEncoding.StartsWith("UTF-8", StringComparison.Ordinal))
                {
                    // Use default ANSI encoding
                    foreach (var enc in Encoding.GetEncodings())
                    {
                        if (Configuration.Settings.General.DefaultEncoding.StartsWith(enc.CodePage + ":", StringComparison.Ordinal))
                        {
                            return enc.GetEncoding();
                        }
                    }
                }

                return encoding;
            }
            catch
            {
                return Encoding.Default;
            }
        }

        private static int CountWeirdCharacters(string textEnc1252)
        {
            var count = 0;
            foreach (var c in textEnc1252)
            {
                if ("¤÷®¬¦¯".Contains(c))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// UTF-16 without a byte order mark, told by its zero bytes: text in the Latin range has a
        /// zero high byte in every other position (even positions for big endian, odd for little
        /// endian), which no 8-bit or UTF-8 text file has. Such a file was read as UTF-8 with a
        /// NUL between every character, and no format recognised it.
        /// </summary>
        /// <returns>The UTF-16 encoding, or null if the bytes do not look like UTF-16</returns>
        public static Encoding GetUtf16WithoutByteOrderMark(byte[] buffer)
        {
            var length = buffer.Length - buffer.Length % 2;
            if (length < 64)
            {
                return null;
            }

            var evenZeros = 0;
            var oddZeros = 0;
            for (var i = 0; i < length; i += 2)
            {
                if (buffer[i] == 0)
                {
                    evenZeros++;
                }

                if (buffer[i + 1] == 0)
                {
                    oddZeros++;
                }
            }

            var pairs = length / 2;
            if (evenZeros > pairs * 0.4 && oddZeros < pairs * 0.02)
            {
                return Encoding.BigEndianUnicode;
            }

            if (oddZeros > pairs * 0.4 && evenZeros < pairs * 0.02)
            {
                return Encoding.Unicode;
            }

            return null;
        }

        public static Encoding GetEncodingFromFile(string fileName, bool skipAnsiAuto = false)
        {
            var encoding = Encoding.Default;

            try
            {
                foreach (var e in Encoding.GetEncodings())
                {
                    var enc = e.GetEncoding();
                    if (enc.WebName == Configuration.Settings.General.DefaultEncoding && enc.WebName != Encoding.Unicode.WebName && enc.WebName != Encoding.UTF8.WebName)
                    {
                        encoding = enc;
                        break;
                    }
                }

                using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bom = new byte[12]; // Get the byte-order mark, if there is one
                    file.Position = 0;
                    var readCount = file.Read(bom, 0, bom.Length);
                    if (readCount < 3)
                    {
                        return encoding;
                    }

                    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                    {
                        encoding = Encoding.UTF8;
                    }
                    else if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
                    {
                        encoding = Encoding.GetEncoding(12000); // UTF-32 (LE)
                    }
                    else if (bom[0] == 0xff && bom[1] == 0xfe)
                    {
                        encoding = Encoding.Unicode;
                    }
                    else if (bom[0] == 0xfe && bom[1] == 0xff) // utf-16 and ucs-2
                    {
                        encoding = Encoding.BigEndianUnicode;
                    }
                    else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) // ucs-4
                    {
                        encoding = Encoding.GetEncoding(12001); // UTF-32 (BE)
                    }
                    else if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 && (bom[3] == 0x38 || bom[3] == 0x39 || bom[3] == 0x2b || bom[3] == 0x2f)) // utf-7
                    {
#pragma warning disable SYSLIB0001 // legacy subtitle files may still be UTF-7 encoded; only used when the file carries a UTF-7 BOM
                        encoding = Encoding.UTF7;
#pragma warning restore SYSLIB0001
                    }
                    else if (file.Length > bom.Length)
                    {
                        long length = file.Length;
                        if (length > 500000)
                        {
                            length = 500000;
                        }

                        file.Position = 0;
                        var buffer = new byte[length];
                        file.ReadFully(buffer, 0, buffer.Length);

                        var utf16 = GetUtf16WithoutByteOrderMark(buffer);
                        if (utf16 != null)
                        {
                            return utf16;
                        }

                        if (IsUtf8(buffer, out var couldBeUtf8))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && Configuration.Settings.General.DefaultEncoding.StartsWith("UTF-8", StringComparison.Ordinal))
                        { // keep utf-8 encoding if it's default
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && Encoding.Default.GetString(buffer).ToLowerInvariant().Replace('\'', '"').Contains("encoding=\"utf-8\""))
                        { // keep utf-8 encoding for xml files with utf-8 in header (without any utf-8 encoded characters, but with only allowed utf-8 characters)
                            encoding = Encoding.UTF8;
                        }
                        else if (Configuration.Settings.General.AutoGuessAnsiEncoding && !skipAnsiAuto)
                        {
                            var autoDetected = DetectAnsiEncoding(buffer);
                            if (autoDetected != null)
                            {
                                return autoDetected.CodePage < 874 ? Encoding.Default : autoDetected;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return encoding;
        }

        /// <summary>
        /// Will try to determine if buffer is utf-8 encoded or not.
        /// If any non-utf8 sequences are found then false is returned, if no utf8 multi bytes sequences are found then false is returned.
        /// </summary>
        private static bool IsUtf8(byte[] buffer, out bool couldBeUtf8)
        {
            couldBeUtf8 = false;
            var utf8Count = 0;
            var i = 0;
            var max = buffer.Length - 3;
            while (i < max)
            {
                byte b = buffer[i];
                if (b <= 127)
                {
                    // Called with up to 500 KB of a subtitle file, which is nearly all plain
                    // ASCII - those bytes only advance the cursor, so jump straight to the
                    // next byte that actually needs validating.
#if NET8_0_OR_GREATER
                    var next = buffer.AsSpan(i + 1).IndexOfAnyExceptInRange((byte)0, (byte)127);
                    if (next < 0)
                    {
                        break;
                    }

                    i += next + 1;
#else
                    i++;
#endif
                    continue;
                }

                if (b >= 194 && b <= 223 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191)
                { // 2-byte sequence
                    utf8Count++;
                    i++;
                }
                else if (b >= 224 && b <= 239 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191 &&
                                                 buffer[i + 2] >= 128 && buffer[i + 2] <= 191)
                { // 3-byte sequence
                    utf8Count++;
                    i += 2;
                }
                else if (b >= 240 && b <= 244 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191 &&
                                                 buffer[i + 2] >= 128 && buffer[i + 2] <= 191 &&
                                                 buffer[i + 3] >= 128 && buffer[i + 3] <= 191)
                { // 4-byte sequence
                    utf8Count++;
                    i += 3;
                }
                else
                {
                    return false;
                }

                i++;
            }

            couldBeUtf8 = true;
            if (utf8Count == 0)
            {
                return false; // not utf-8 (no characters utf-8 encoded...)
            }

            return true;
        }

        // Letters only: the lists also hold phrases (a space), joiners and vowel marks.
        private static readonly char[] RightToLeftLetters = string.Concat(AutoDetectWordsArabic.Concat(AutoDetectWordsHebrew).Concat(AutoDetectWordsFarsi).Concat(AutoDetectWordsUrdu)).Where(char.IsLetter).Distinct().ToArray();

#if NET8_0_OR_GREATER
        private static readonly SearchValues<char> RightToLeftLetterSearchValues = SearchValues.Create(RightToLeftLetters);
#endif

        public static bool CouldBeRightToLeftLanguage(Subtitle subtitle)
        {
            const int maxNumberOfLinesToCheck = 20;
            var max = Math.Min(maxNumberOfLinesToCheck, subtitle.Paragraphs.Count);
            for (int i = 0; i < max; i++)
            {
                if (ContainsRightToLeftLetter(subtitle.Paragraphs[i].Text))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsRightToLeftLetter(string text)
        {
#if NET8_0_OR_GREATER
            return text.AsSpan().ContainsAny(RightToLeftLetterSearchValues);
#else
            foreach (var letter in RightToLeftLetters)
            {
                if (text.Contains(letter))
                {
                    return true;
                }
            }
            return false;
#endif
        }

        // Score slots, in the order the language codes are reported. LetterSets is indexed in
        // lock step with this array, and ties are broken in this order (see below), so the two
        // must stay aligned.
        private static readonly string[] LanguageCodes =
        {
            "ar", "ko", "ja", "th", "si", "ur", "ru", "el", "he", "hi",
            "bn", "hy", "ka", "zh", "am", "km", "de", "sv", "fr", "es"
        };

        // Slots in LanguageCodes that get a unique-character bonus. Derived rather than
        // hard-coded so reordering LanguageCodes cannot silently misapply the bonuses.
        private static readonly int IndexUr = Array.IndexOf(LanguageCodes, "ur");
        private static readonly int IndexDe = Array.IndexOf(LanguageCodes, "de");
        private static readonly int IndexSv = Array.IndexOf(LanguageCodes, "sv");
        private static readonly int IndexFr = Array.IndexOf(LanguageCodes, "fr");
        private static readonly int IndexEs = Array.IndexOf(LanguageCodes, "es");

        private static readonly string[] PriorityOrder = { "zh", "ja", "ko", "th", "ur", "ar", "he", "ru", "el", "hi", "de", "fr", "es", "sv" };

        private const string GermanUniqueLetters = "üßÜ";
        private const string SwedishOnlyLetters = "åÅ";  // å is unique to Swedish among these languages
        private const string FrenchUniqueLetters = "çÇœŒâêîôûÂÊÎÔÛŸ";
        private const string SpanishUniqueLetters = "ñÑ¡¿";
        private const string UrduUniqueLetters = "ﭖﭘﭙﭗﭦﭨﭩﭧﮮﮯﮦﮨﮩﮧﯼﯾﯿﯽﮪﮬﮭﮫﹱﹷﹹپچگےھیںۓڑ";

#if NET8_0_OR_GREATER
        // SearchValues gives an O(1) probabilistic lookup instead of a linear scan over the
        // const string, which is what dominated this method - it ran one scan per language
        // per character.
        private static readonly SearchValues<char>[] LetterSets =
        {
            SearchValues.Create(Letters.Arabic), SearchValues.Create(Letters.Korean),
            SearchValues.Create(Letters.Japanese), SearchValues.Create(Letters.Thai),
            SearchValues.Create(Letters.Sinhalese), SearchValues.Create(Letters.Urdu),
            SearchValues.Create(Letters.Russian), SearchValues.Create(Letters.Greek),
            SearchValues.Create(Letters.Hebrew), SearchValues.Create(Letters.Devanagari),
            SearchValues.Create(Letters.Bengali), SearchValues.Create(Letters.Armenian),
            SearchValues.Create(Letters.Georgian), SearchValues.Create(Letters.Chinese),
            SearchValues.Create(Letters.Amharic), SearchValues.Create(Letters.Khmer),
            SearchValues.Create(Letters.Deutsch), SearchValues.Create(Letters.Swedish),
            SearchValues.Create(Letters.French), SearchValues.Create(Letters.Spanish)
        };

        private static readonly SearchValues<char> GermanUniqueSet = SearchValues.Create(GermanUniqueLetters);
        private static readonly SearchValues<char> SwedishOnlySet = SearchValues.Create(SwedishOnlyLetters);
        private static readonly SearchValues<char> FrenchUniqueSet = SearchValues.Create(FrenchUniqueLetters);
        private static readonly SearchValues<char> SpanishUniqueSet = SearchValues.Create(SpanishUniqueLetters);
        private static readonly SearchValues<char> UrduUniqueSet = SearchValues.Create(UrduUniqueLetters);

        private static bool ContainsAny(string text, SearchValues<char> set)
        {
            return text.AsSpan().IndexOfAny(set) >= 0;
        }
#else
        // netstandard2.1 has no SearchValues; a HashSet is still a big win over the linear scan.
        private static readonly HashSet<char>[] LetterSets =
        {
            new HashSet<char>(Letters.Arabic), new HashSet<char>(Letters.Korean),
            new HashSet<char>(Letters.Japanese), new HashSet<char>(Letters.Thai),
            new HashSet<char>(Letters.Sinhalese), new HashSet<char>(Letters.Urdu),
            new HashSet<char>(Letters.Russian), new HashSet<char>(Letters.Greek),
            new HashSet<char>(Letters.Hebrew), new HashSet<char>(Letters.Devanagari),
            new HashSet<char>(Letters.Bengali), new HashSet<char>(Letters.Armenian),
            new HashSet<char>(Letters.Georgian), new HashSet<char>(Letters.Chinese),
            new HashSet<char>(Letters.Amharic), new HashSet<char>(Letters.Khmer),
            new HashSet<char>(Letters.Deutsch), new HashSet<char>(Letters.Swedish),
            new HashSet<char>(Letters.French), new HashSet<char>(Letters.Spanish)
        };

        private static readonly HashSet<char> GermanUniqueSet = new HashSet<char>(GermanUniqueLetters);
        private static readonly HashSet<char> SwedishOnlySet = new HashSet<char>(SwedishOnlyLetters);
        private static readonly HashSet<char> FrenchUniqueSet = new HashSet<char>(FrenchUniqueLetters);
        private static readonly HashSet<char> SpanishUniqueSet = new HashSet<char>(SpanishUniqueLetters);
        private static readonly HashSet<char> UrduUniqueSet = new HashSet<char>(UrduUniqueLetters);

        private static bool ContainsAny(string text, HashSet<char> set)
        {
            foreach (var c in text)
            {
                if (set.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }
#endif

        public static string GetEncodingViaLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            // Letters a single Latin-script language owns settle it outright.
            var latin = GetLatinLanguageByUniqueLetters(text);
            if (latin != null)
            {
                return latin;
            }

            // Check for unique characters first
            var hasGermanUnique = ContainsAny(text, GermanUniqueSet);
            var hasSwedishUnique = ContainsAny(text, SwedishOnlySet);
            var hasFrenchUnique = ContainsAny(text, FrenchUniqueSet);
            var hasSpanishUnique = ContainsAny(text, SpanishUniqueSet);
            var hasUrduUnique = ContainsAny(text, UrduUniqueSet);

            // Count all special characters.
            var languageScores = new int[LanguageCodes.Length];
#if NET8_0_OR_GREATER
            // Almost every set is absent from any given file - a Latin-script subtitle hits
            // none of the 16 non-Latin ones - but the loop below used to ask all 20 sets about
            // every character, which is 20 non-inlinable Contains calls per character over the
            // whole file. One vectorized IndexOfAny per set answers "is this script here at
            // all" in a fraction of that, and only the sets that survive pay per character.
            var textSpan = text.AsSpan();
            Span<int> presentSets = stackalloc int[LetterSets.Length];
            var presentCount = 0;
            for (var i = 0; i < LetterSets.Length; i++)
            {
                if (textSpan.IndexOfAny(LetterSets[i]) >= 0)
                {
                    presentSets[presentCount++] = i;
                }
            }

            if (presentCount > 0)
            {
                foreach (var c in text)
                {
                    for (var i = 0; i < presentCount; i++)
                    {
                        var index = presentSets[i];
                        if (LetterSets[index].Contains(c))
                        {
                            languageScores[index]++;
                        }
                    }
                }
            }
#else
            foreach (var c in text)
            {
                for (var i = 0; i < LetterSets.Length; i++)
                {
                    if (LetterSets[i].Contains(c))
                    {
                        languageScores[i]++;
                    }
                }
            }
#endif

            // Apply unique character bonuses for disambiguation
            if (hasUrduUnique && languageScores[IndexUr] > 0)
            {
                languageScores[IndexUr] += 100;  // Strong boost for Urdu
            }
            if (hasGermanUnique && languageScores[IndexDe] > 0)
            {
                languageScores[IndexDe] += 50;
            }
            if (hasSwedishUnique && languageScores[IndexSv] > 0)
            {
                languageScores[IndexSv] += 50;
            }
            if (hasFrenchUnique && languageScores[IndexFr] > 0)
            {
                languageScores[IndexFr] += 50;
            }
            if (hasSpanishUnique && languageScores[IndexEs] > 0)
            {
                languageScores[IndexEs] += 50;
            }

            var maxScore = 0;
            for (var i = 0; i < languageScores.Length; i++)
            {
                if (languageScores[i] > maxScore)
                {
                    maxScore = languageScores[i];
                }
            }

            if (maxScore < 1)
            {
                return null;
            }

            var topCount = 0;
            var firstTop = -1;
            for (var i = 0; i < languageScores.Length; i++)
            {
                if (languageScores[i] == maxScore)
                {
                    topCount++;
                    if (firstTop < 0)
                    {
                        firstTop = i;
                    }
                }
            }

            var result = LanguageCodes[firstTop];
            for (var p = 0; topCount > 1 && p < PriorityOrder.Length; p++)
            {
                // Priority order for tie-breaking
                var i = Array.IndexOf(LanguageCodes, PriorityOrder[p]);
                if (i >= 0 && languageScores[i] == maxScore)
                {
                    result = PriorityOrder[p];
                    break;
                }
            }

            if (result == "ru")
            {
                // The Cyrillic letter set is Russian's, so every Cyrillic text lands here;
                // the other Cyrillic languages have letters of their own that say otherwise.
                return GetCyrillicLanguageByLetters(text, null) ?? "ru";
            }

            if (result == "ar")
            {
                // Same for the Arabic script: Persian and Urdu have letters Arabic lacks.
                return GetArabicScriptLanguageByLetters(text) ?? "ar";
            }

            return result;
        }

        public static bool IsLanguageWithoutPeriods(string language)
        {
            return language == "ko" || language == "zh" || language == "ja" || language == "th";
        }

        public static class Letters
        {
            public const string Arabic = "غظضذخثتشرقصفعسنملكيطحزوهدجبا";
            public const string Korean = "가나다라마바사아자차카타파하아야어여오요우유으이대한민국활화산동물들천국의섬유독춤을춥니다";
            public const string Japanese = "あいうえおかきくけこがぎぐげごさしすせそざじずぜぞたちつてとだぢづでどなにぬねのはひふへほばびぶべぼぱぴぷぺぽまみむめもやゆよらりるれろわをん";
            public const string Thai = "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรลวศษสหฬอฮ";
            public const string Urdu = "ﺍﺎﺁﺂﺏﺑﺒﺐﭖﭘﭙﭗﺕﺗﺘﺖﭦﭨﭩﭧﺙﺛﺜﺚﺝﺟﺠﺞﭺﭼﭽﭻﺡﺣﺤﺢﺥﺧﺨﺦﺩﺪﺫﺬﺭﺮﮌﮍﺯﺰﮊﮋﺱﺳﺴﺲﺵﺷﺸﺶﺹﺻﺼﺺﺽﺿﻀﺾﻁﻃﻄﻂﻅﻇﻈﻆﻉﻋﻌﻊﻍﻏﻐﻎﻑﻓﻔﻒﻕﻗﻘﻖﻙﻛﻜﻚﻻﻼﻝﻟﻠﻞﻡﻣﻤﻢﻥﻧﻨﻦﻭﻮﮮﮯﮦﮨﮩﮧﯼﯾﯿﯽﮪﮬﮭﮫﴽﴼﺀﺋﺌﹱﹷﹹ";
            public const string Sinhalese = "අආඇඈඉඊඋඌඍඎඏඐඑඒඓඔඕඖකඛගඝඞඟචඡජඣඤඥඦටඨඩඪණඬතථදධනඳපඵබභමඹයරලවශෂසහළෆ්ාැෑිීුූෘෙේෛොෝෞෟ෦෧෨෩෪෫෬෭෮෯";
            public const string Russian = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
            public const string Greek = "αβγδεζηθικλμνξοπρσςτυφχψωάέήίόύώ";
            public const string Hebrew = "אבגדהוזחטיכלמנסעפצקרשתךםןףץ";
            public const string Devanagari = "अआइईउऊऋएऐओऔकखगघङचछजझञटठडढणतथदधनपफबभमयरलवशषसह";
            public const string Bengali = "অআইঈউঊঋএঐওঔকখগঘঙচছজঝঞটঠডঢণতথদধনপফবভমযরলশষসহ";
            public const string Armenian = "աբգդեզէըթժիլխծկհձղճմյնշոչպջռսվտրցւփքօֆ";
            public const string Georgian = "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ";
            public const string Chinese = "的一是在不了有和人这中大为上个国我以要他时来用们生到作地于出就分对成会可主发年动同工也";
            public const string Amharic = "ሀሁሂሃሄህሆለሉሊላሌልሎመሙሚማሜምሞሰሱሲሳሴስሶ";
            public const string Khmer = "កខគឃងចឆជឈញដឋឌឍណតថទធនបផពភមយរលវសហឡអ";
            public const string Swedish = "äö";
            public const string Deutsch = "äöüß";
            public const string Spanish = "ñÑáéíóúÁÉÍÓÚ¡¿";
            public const string French = "çÇœŒàâæéèêëîïôùûüÿÀÂÆÉÈÊËÎÏÔÙÛÜŸ";
        }
    }
}
