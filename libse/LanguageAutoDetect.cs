using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core
{
    public static class LanguageAutoDetect
    {

        private static int GetCount(string text, params string[] words)
        {
            var options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
            var pattern = "\\b(" + string.Join("|", words) + ")\\b";
            return Regex.Matches(text, pattern, options).Count;
        }

        private static int GetCountContains(string text, params string[] words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var regEx = new Regex(words[i]);
                count += regEx.Matches(text).Count;
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

        private static readonly string[] AutoDetectWordsEnglish = { "we", "are", "and", "your?", "what" };
        private static readonly string[] AutoDetectWordsDanish = { "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej" };
        private static readonly string[] AutoDetectWordsNorwegian = { "vi", "er", "og", "jeg", "var", "men" };
        private static readonly string[] AutoDetectWordsSwedish = { "vi", "är", "och", "Jag", "inte", "för" };
        private static readonly string[] AutoDetectWordsSpanish = { "qué", "eso", "muy", "estoy?", "ahora", "hay", "tú", "así", "cuando", "cómo", "él", "sólo", "quiero", "gracias", "puedo", "bueno", "soy", "hacer", "fue", "eres", "usted", "tienes", "puede",
                                                                    "[Ss]eñor", "ese", "voy", "quién", "creo", "hola", "dónde", "sus", "verdad", "quieres", "mucho", "entonces", "estaba", "tiempo", "esa", "mejor", "hombre", "hace", "dios", "también", "están",
                                                                    "siempre", "hasta", "ahí", "siento", "puedes" };
        private static readonly string[] AutoDetectWordsItalian = { "Buongiorno", "Grazie", "Cosa", "quest[ao]", "quell[ao]", "tutt[io]", "[st]uo", "qualcosa", "ancora", "sono", "bene", "più", "andare", "essere", "venire", "abbiamo", "andiamo", "ragazzi",
                                                                    "signore", "numero", "giorno", "propriamente", "sensitivo", "negativo", "davvero", "faccio", "voglio", "vuole", "perché", "allora", "niente", "anche", "stai", "detto", "fatto", "hanno",
                                                                    "molto", "stato", "siamo", "così", "vuoi", "noi", "vero", "loro", "fare", "gli", "due" };
        private static readonly string[] AutoDetectWordsFrench = { "pas", "[vn]ous", "ça", "une", "pour", "[mt]oi", "dans", "elle", "tout", "plus", "[bmt]on", "suis", "avec", "oui", "fait", "ils", "être", "faire", "comme", "était", "quoi", "ici", "veux",
                                                                   "rien", "dit", "où", "votre", "pourquoi", "sont", "cette", "peux", "alors", "comment", "avez", "très", "même", "merci", "ont", "aussi", "chose", "voir", "allez", "tous", "ces", "deux" };
        private static readonly string[] AutoDetectWordsPortuguese = { "[Nn]ão", "[Ee]ntão", "uma", "ele", "bem", "isso", "você", "sim", "meu", "muito", "estou", "ela", "fazer", "tem", "já", "minha", "tudo", "só", "tenho", "agora", "vou", "seu", "quem",
                                                                       "há", "lhe", "quero", "nós", "coisa", "são", "ter", "dizer", "eles", "pode", "bom", "mesmo", "mim", "estava", "assim", "estão", "até", "quer", "temos", "acho", "obrigado", "também",
                                                                       "tens", "deus", "quê", "ainda", "noite" };
        private static readonly string[] AutoDetectWordsGerman = { "und", "auch", "sich", "bin", "hast", "möchte" };
        private static readonly string[] AutoDetectWordsDutch = { "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n" };
        private static readonly string[] AutoDetectWordsPolish = { "Czy", "ale", "ty", "siê", "jest", "mnie" };
        private static readonly string[] AutoDetectWordsGreek = { "μου", "[Εε]ίναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις" };
        private static readonly string[] AutoDetectWordsRussian = { "[Ээч]?то", "[Нн]е", "[ТтМмбв]ы", "Да", "[Нн]ет", "Он", "его", "тебя", "как", "меня", "Но", "всё", "мне", "вас", "знаю", "ещё", "за", "нас", "чтобы", "был" };
        private static readonly string[] AutoDetectWordsUkrainian = { "[Нн]і", "[Пп]ривіт", "[Цц]е", "[Щщ]о", "[Йй]ого", "[Вв]ін", "[Яя]к", "[Гг]аразд", "[Яя]кщо", "[Мм]ені", "[Тт]вій", "[Її]х", "[Вв]ітаю", "[Дд]якую", "вже", "було", "був", "цього",
                                                                      "нічого", "немає", "може", "знову", "бо", "щось", "щоб", "цим", "тобі", "хотів", "твоїх", "мої", "мій", "має", "їм", "йому", "дуже" };
        private static readonly string[] AutoDetectWordsBulgarian = { "[Кк]акво", "тук", "може", "Как", "Ваше" };
        private static readonly string[] AutoDetectWordsArabic = { "من", "هل", "لا", "فى", "لقد", "ما" };
        private static readonly string[] AutoDetectWordsHebrew = { "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב" };
        private static readonly string[] AutoDetectWordsVietnamese = { "không", "[Tt]ôi", "anh", "đó", "ông" };
        private static readonly string[] AutoDetectWordsHungarian = { "hogy", "lesz", "tudom", "vagy", "mondtam", "még" };
        private static readonly string[] AutoDetectWordsTurkish = { "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "çalýþýyor", "burada", "efendim" };
        private static readonly string[] AutoDetectWordsCroatianAndSerbian = { "sam", "ali", "nije", "samo", "ovo", "kako", "dobro", "sve", "tako", "će", "mogu", "ću", "zašto", "nešto", "za" };
        private static readonly string[] AutoDetectWordsCroatian = { "što", "ovdje", "gdje", "kamo", "tko", "prije", "uvijek", "vrijeme", "vidjeti", "netko",
                                                                     "vidio", "nitko", "bok", "lijepo", "oprosti", "htio", "mjesto", "oprostite", "čovjek", "dolje",
                                                                     "čovječe", "dvije", "dijete", "dio", "poslije", "događa", "vjerovati", "vjerojatno", "vjerujem", "točno",
                                                                     "razumijem", "vidjela", "cijeli", "svijet", "obitelj", "volio", "sretan", "dovraga", "svijetu", "htjela",
                                                                     "vidjeli", "negdje", "želio", "ponovno", "djevojka", "umrijeti", "čovjeka", "mjesta", "djeca", "osjećam",
                                                                     "uopće", "djecu", "naprijed", "obitelji", "doista", "mjestu", "lijepa", "također", "riječ", "tijelo" };
        private static readonly string[] AutoDetectWordsSerbian = { "šta", "ovde", "gde", "ko", "pre", "uvek", "vreme", "videti", "neko",
                                                                    "video", "niko", "ćao", "lepo", "izvini", "hteo", "mesto", "izvinite", "čovek", "dole",
                                                                    "čoveče", "dve", "dete", "deo", "posle", "dešava", "verovati", "verovatno", "verujem", "tačno",
                                                                    "razumem", "videla", "ceo", "svet", "porodica", "voleo", "srećan", "dođavola", "svetu", "htela",
                                                                    "videli", "negde", "želeo", "ponovo", "devojka", "umreti", "čoveka", "mesta", "deca", "osećam",
                                                                    "uopšte", "decu", "napred", "porodicu", "zaista", "mestu", "lepa", "takođe", "reč", "telo" };
        private static readonly string[] AutoDetectWordsSerbianCyrillic = { "сам", "али", "није", "само", "ово", "како", "добро", "све", "тако", "ће", "могу", "ћу", "зашто", "нешто", "за", "шта", "овде" };
        private static readonly string[] AutoDetectWordsIndonesian = { "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu" };
        private static readonly string[] AutoDetectWordsThai = { "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล", "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์" };
        private static readonly string[] AutoDetectWordsKorean = { "그리고", "아니야", "하지만", "말이야", "그들은", "우리가" };
        private static readonly string[] AutoDetectWordsFinnish = { "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä" };
        private static readonly string[] AutoDetectWordsRomanian1 = { "pentru", "oamenii", "decât", "[Vv]reau", "[Ss]înt", "Asteaptã", "Fãrã", "aici", "domnule", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "fãcut", "spune" };
        private static readonly string[] AutoDetectWordsRomanian2 = { "pentru", "oamenii", "decat", "[Tt]rebuie", "[Aa]cum", "Poate", "vrea", "soare", "nevoie", "daca", "echilibrul", "vorbesti", "zeului", "atunci", "memoria", "soarele" };

        private static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, AutoDetectWordsEnglish);
            if (count > bestCount)
                return "en";

            count = GetCount(text, AutoDetectWordsDanish);
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                int dutchCount = GetCount(text, AutoDetectWordsDutch);
                if (norwegianCount < 2 && dutchCount < count)
                    return "da";
            }

            count = GetCount(text, AutoDetectWordsNorwegian);
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                int dutchCount = GetCount(text, AutoDetectWordsDutch);
                if (danishCount < 2 && dutchCount < count)
                    return "no";
            }

            count = GetCount(text, AutoDetectWordsSwedish);
            if (count > bestCount)
                return "sv";

            count = GetCount(text, AutoDetectWordsSpanish);
            if (count > bestCount)
            {
                int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                int portugueseCount = GetCount(text, "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
                                                     "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios"); // not spanish words
                if (frenchCount < 2 && portugueseCount < 2)
                    return "es";
            }

            count = GetCount(text, AutoDetectWordsItalian);
            if (count > bestCount)
            {
                int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not italian words
                if (frenchCount < 2)
                    return "it";
            }

            count = GetCount(text, AutoDetectWordsFrench);
            if (count > bestCount)
            {
                int romanianCount = GetCount(text, "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici");
                if (romanianCount < 5)
                    return "fr";
            }

            count = GetCount(text, AutoDetectWordsPortuguese);
            if (count > bestCount)
                return "pt"; // Portuguese

            count = GetCount(text, AutoDetectWordsGerman);
            if (count > bestCount)
                return "de";

            count = GetCount(text, AutoDetectWordsDutch);
            if (count > bestCount)
                return "nl";

            count = GetCount(text, AutoDetectWordsPolish);
            if (count > bestCount)
                return "pl";

            count = GetCount(text, AutoDetectWordsGreek);
            if (count > bestCount)
                return "el"; // Greek

            count = GetCount(text, AutoDetectWordsRussian);
            if (count > bestCount)
                return "ru"; // Russian

            count = GetCount(text, AutoDetectWordsUkrainian);
            if (count > bestCount)
                return "uk"; // Ukrainian

            count = GetCount(text, AutoDetectWordsBulgarian);
            if (count > bestCount)
                return "bg"; // Bulgarian

            count = GetCount(text, AutoDetectWordsArabic);
            if (count > bestCount)
            {
                int hebrewCount = GetCount(text, AutoDetectWordsHebrew);
                if (hebrewCount < count)
                    return "ar"; // Arabic
            }

            count = GetCount(text, AutoDetectWordsHebrew);
            if (count > bestCount)
                return "he"; // Hebrew

            count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
            if (count > bestCount)
            {
                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                if (croatianCount > serbianCount)
                    return "hr"; // Croatian
                return "sr"; // Serbian
            }

            count = GetCount(text, AutoDetectWordsVietnamese);
            if (count > bestCount)
                return "vi"; // Vietnamese

            count = GetCount(text, AutoDetectWordsHungarian);
            if (count > bestCount)
                return "hu"; // Hungarian

            count = GetCount(text, AutoDetectWordsTurkish);
            if (count > bestCount)
                return "tr"; // Turkish

            count = GetCount(text, AutoDetectWordsIndonesian);
            if (count > bestCount)
                return "id"; // Indonesian

            count = GetCount(text, AutoDetectWordsThai);
            if (count > 10 || count > bestCount)
                return "th"; // Thai

            count = GetCount(text, AutoDetectWordsKorean);
            if (count > 10 || count > bestCount)
                return "ko"; // Korean

            count = GetCount(text, AutoDetectWordsFinnish);
            if (count > bestCount)
                return "fi"; // Finnish

            count = GetCount(text, AutoDetectWordsRomanian1);
            if (count <= bestCount)
                count = GetCount(text, AutoDetectWordsRomanian2);
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
                return "ja"; // Japanese - not tested...

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
                return "zh"; // Chinese (simplified) - not tested...

            return string.Empty;
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle)
        {
            string languageId = AutoDetectGoogleLanguageOrNull(subtitle);
            if (languageId == null)
                languageId = "en";

            return languageId;
        }

        public static string AutoDetectGoogleLanguageOrNull(Subtitle subtitle)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);

            string languageId = AutoDetectGoogleLanguage(sb.ToString(), subtitle.Paragraphs.Count / 14);
            if (string.IsNullOrEmpty(languageId))
                languageId = null;

            return languageId;
        }

        public static string AutoDetectLanguageName(string languageName, Subtitle subtitle)
        {
            if (string.IsNullOrEmpty(languageName))
                languageName = "en_US";
            int bestCount = subtitle.Paragraphs.Count / 14;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            List<string> dictionaryNames = Utilities.GetDictionaryLanguages();

            bool containsEnGb = false;
            bool containsEnUs = false;
            bool containsHrHr = false;
            bool containsSrLatn = false;
            foreach (string name in dictionaryNames)
            {
                if (name.Contains("[en_GB]"))
                    containsEnGb = true;
                if (name.Contains("[en_US]"))
                    containsEnUs = true;
                if (name.Contains("[hr_HR]"))
                    containsHrHr = true;
                if (name.Contains("[sr-Latn]"))
                    containsSrLatn = true;
            }

            foreach (string name in dictionaryNames)
            {
                string shortName = string.Empty;
                int start = name.IndexOf('[');
                int end = name.IndexOf(']');
                if (start > 0 && end > start)
                {
                    start++;
                    shortName = name.Substring(start, end - start);
                }

                int count;
                switch (shortName)
                {
                    case "da_DK":
                        count = GetCount(text, AutoDetectWordsDanish);
                        if (count > bestCount)
                        {
                            int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                            int dutchCount = GetCount(text, AutoDetectWordsDutch);
                            if (norwegianCount < 2 && dutchCount < count)
                                languageName = shortName;
                        }
                        break;
                    case "nb_NO":
                        count = GetCount(text, AutoDetectWordsNorwegian);
                        if (count > bestCount)
                        {
                            int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                            int dutchCount = GetCount(text, AutoDetectWordsDutch);
                            if (danishCount < 2 && dutchCount < count)
                                languageName = shortName;
                        }
                        break;
                    case "sv_SE":
                        count = GetCount(text, AutoDetectWordsSwedish);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "en_US":
                        count = GetCount(text, AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsEnGb)
                            {
                                int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                if (gbCount > usCount)
                                    languageName = "en_GB";
                            }
                        }
                        break;
                    case "en_GB":
                        count = GetCount(text, AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsEnUs)
                            {
                                int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                if (gbCount < usCount)
                                    languageName = "en_US";
                            }
                        }
                        break;
                    case "es_ES":
                        count = GetCount(text, AutoDetectWordsSpanish);
                        if (count > bestCount)
                        {
                            int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                            int portugueseCount = GetCount(text, "[NnCc]ão", "Então", "h?ouve", "pessoal", "rapariga", "tivesse", "fizeste",
                                                                 "jantar", "conheço", "atenção", "foste", "milhões", "devias", "ganhar", "raios"); // not spanish words
                            if (frenchCount < 2 && portugueseCount < 2)
                                languageName = shortName;
                        }
                        break;
                    case "it_IT":
                        count = GetCount(text, AutoDetectWordsItalian);
                        if (count > bestCount)
                        {
                            int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not italian words
                            int spanishCount = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not italian words
                            if (frenchCount < 2 && spanishCount < 2)
                                languageName = shortName;
                        }
                        break;
                    case "fr_FR":
                        count = GetCount(text, AutoDetectWordsFrench);
                        if (count > bestCount)
                        {
                            int romanianCount = GetCount(text, "[Vv]reau", "[Ss]înt", "[Aa]cum", "pentru", "domnule", "aici");
                            int spanishCount = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                            int italianCount = GetCount(text, AutoDetectWordsItalian);
                            if (romanianCount < 5 && spanishCount < 2 && italianCount < 2)
                                languageName = shortName;
                        }
                        break;
                    case "de_DE":
                        count = GetCount(text, AutoDetectWordsGerman);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "nl_NL":
                        count = GetCount(text, AutoDetectWordsDutch);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pl_PL":
                        count = GetCount(text, AutoDetectWordsPolish);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "el_GR":
                        count = GetCount(text, AutoDetectWordsGreek);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "ru_RU":
                        count = GetCount(text, AutoDetectWordsRussian);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "uk_UA":
                        count = GetCount(text, AutoDetectWordsUkrainian);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "ro_RO":
                        count = GetCount(text, AutoDetectWordsRomanian1);
                        if (count <= bestCount)
                            count = GetCount(text, AutoDetectWordsRomanian2);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "hr_HR": // Croatian
                        count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsSrLatn)
                            {
                                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                                if (serbianCount > croatianCount)
                                    languageName = "sr-Latn";
                            }
                        }
                        break;
                    case "sr-Latn": // Serbian (Latin)
                        count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsHrHr)
                            {
                                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                                if (serbianCount < croatianCount)
                                    languageName = "hr_HR";
                            }
                        }
                        break;
                    case "sr": // Serbian (Cyrillic)
                        count = GetCount(text, AutoDetectWordsSerbianCyrillic);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pt_PT": // Portuguese
                        count = GetCount(text, AutoDetectWordsPortuguese);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pt_BR": // Portuguese (Brasil)
                        count = GetCount(text, AutoDetectWordsPortuguese);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "hu_HU": // Hungarian
                        count = GetCount(text, AutoDetectWordsHungarian);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                }
            }
            return languageName;
        }

        public static Encoding DetectAnsiEncoding(byte[] buffer)
        {
            if (Utilities.IsRunningOnMono())
                return Encoding.Default;

            try
            {
                Encoding encoding = DetectEncoding.EncodingTools.DetectInputCodepage(buffer);

                Encoding greekEncoding = Encoding.GetEncoding(1253); // Greek
                if (GetCount(greekEncoding.GetString(buffer), AutoDetectWordsGreek) > 5)
                    return greekEncoding;

                Encoding russianEncoding = Encoding.GetEncoding(1251); // Cyrillic
                if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                    return russianEncoding;
                if (GetCount(russianEncoding.GetString(buffer), "Какво", "тук", "може", "Как", "Ваше", "какво") > 5) // Bulgarian
                    return russianEncoding;

                russianEncoding = Encoding.GetEncoding(28595); // Russian
                if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                    return russianEncoding;

                Encoding thaiEncoding = Encoding.GetEncoding(874); // Thai
                if (GetCount(thaiEncoding.GetString(buffer), "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล", "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์") > 5)
                    return thaiEncoding;

                Encoding arabicEncoding = Encoding.GetEncoding(28596); // Arabic
                Encoding hewbrewEncoding = Encoding.GetEncoding(28598); // Hebrew
                if (GetCount(arabicEncoding.GetString(buffer), AutoDetectWordsArabic) > 5)
                {
                    if (GetCount(hewbrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 10)
                        return hewbrewEncoding;
                    return arabicEncoding;
                }
                if (GetCount(hewbrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 5)
                    return hewbrewEncoding;

                return encoding;
            }
            catch
            {
                return Encoding.Default;
            }
        }

        public static Encoding GetEncodingFromFile(string fileName)
        {
            var encoding = Encoding.Default;

            try
            {
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    if (ei.CodePage + ": " + ei.DisplayName == Configuration.Settings.General.DefaultEncoding &&
                        ei.Name != Encoding.UTF8.BodyName &&
                        ei.Name != Encoding.Unicode.BodyName)
                    {
                        encoding = ei.GetEncoding();
                        break;
                    }
                }

                using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bom = new byte[12]; // Get the byte-order mark, if there is one
                    file.Position = 0;
                    file.Read(bom, 0, bom.Length);
                    if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                        encoding = Encoding.UTF8;
                    else if (bom[0] == 0xff && bom[1] == 0xfe)
                        encoding = Encoding.Unicode;
                    else if (bom[0] == 0xfe && bom[1] == 0xff) // utf-16 and ucs-2
                        encoding = Encoding.BigEndianUnicode;
                    else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) // ucs-4
                        encoding = Encoding.UTF32;
                    else if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 && (bom[3] == 0x38 || bom[3] == 0x39 || bom[3] == 0x2b || bom[3] == 0x2f)) // utf-7
                        encoding = Encoding.UTF7;
                    else if (file.Length > bom.Length)
                    {
                        long length = file.Length;
                        if (length > 500000)
                            length = 500000;

                        file.Position = 0;
                        var buffer = new byte[length];
                        file.Read(buffer, 0, buffer.Length);

                        bool couldBeUtf8;
                        if (IsUtf8(buffer, out couldBeUtf8))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && Configuration.Settings.General.DefaultEncoding == Encoding.UTF8.BodyName)
                        { // keep utf-8 encoding if it's default
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && Encoding.Default.GetString(buffer).ToLower().Replace('\'', '"').Contains("encoding=\"utf-8\""))
                        { // keep utf-8 encoding for xml files with utf-8 in header (without any utf-8 encoded characters, but with only allowed utf-8 characters)
                            encoding = Encoding.UTF8;
                        }
                        else if (Configuration.Settings.General.AutoGuessAnsiEncoding)
                        {
                            encoding = DetectAnsiEncoding(buffer);

                            Encoding greekEncoding = Encoding.GetEncoding(1253); // Greek
                            if (GetCount(greekEncoding.GetString(buffer), AutoDetectWordsGreek) > 5)
                                return greekEncoding;

                            Encoding russianEncoding = Encoding.GetEncoding(1251); // Cyrillic
                            if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                                return russianEncoding;
                            if (GetCount(russianEncoding.GetString(buffer), "Какво", "тук", "може", "Как", "Ваше", "какво") > 5) // Bulgarian
                                return russianEncoding;
                            russianEncoding = Encoding.GetEncoding(28595); // Russian
                            if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5)
                                return russianEncoding;

                            Encoding thaiEncoding = Encoding.GetEncoding(874); // Thai
                            if (GetCount(thaiEncoding.GetString(buffer), "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล", "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์") > 5)
                                return thaiEncoding;

                            Encoding arabicEncoding = Encoding.GetEncoding(28596); // Arabic
                            Encoding hewbrewEncoding = Encoding.GetEncoding(28598); // Hebrew
                            if (GetCount(arabicEncoding.GetString(buffer), AutoDetectWordsArabic) > 5)
                            {
                                if (GetCount(hewbrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 10)
                                    return hewbrewEncoding;
                                return arabicEncoding;
                            }
                            if (GetCount(hewbrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 5)
                                return hewbrewEncoding;

                            Encoding romanianEncoding = Encoding.GetEncoding(1250); // Romanian
                            if (GetCount(romanianEncoding.GetString(buffer), "să", "şi", "văzut", "regulă", "găsit", "viaţă") > 99)
                                return romanianEncoding;

                            Encoding koreanEncoding = Encoding.GetEncoding(949); // Korean
                            if (GetCount(koreanEncoding.GetString(buffer), "그리고", "아니야", "하지만", "말이야", "그들은", "우리가") > 5)
                                return koreanEncoding;
                        }
                    }
                }
            }
            catch
            {
            }
            return encoding;
        }

        /// <summary>
        /// Will try to determine if buffer is utf-8 encoded or not.
        /// If any non-utf8 sequences are found then false is returned, if no utf8 multibytes sequences are found then false is returned.
        /// </summary>
        private static bool IsUtf8(byte[] buffer, out bool couldBeUtf8)
        {
            couldBeUtf8 = false;
            int utf8Count = 0;
            int i = 0;
            while (i < buffer.Length - 3)
            {
                byte b = buffer[i];
                if (b > 127)
                {
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
                }
                i++;
            }
            couldBeUtf8 = true;
            if (utf8Count == 0)
                return false; // not utf-8 (no characters utf-8 encoded...)

            return true;
        }

    }
}
