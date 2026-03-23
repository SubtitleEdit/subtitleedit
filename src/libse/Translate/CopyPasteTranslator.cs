using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate
{

    public class CopyPasteTranslator
    {
        private Formatting[] _formattings;
        private List<Paragraph> _paragraphs;
        private string _separator;

        public CopyPasteTranslator(List<Paragraph> paragraphs, string separator)
        {
            _paragraphs = paragraphs;
            _formattings = new Formatting[paragraphs.Count];
            _separator = separator;
        }

        public List<CopyPasteBlock> BuildBlocks(int maxBlockSize, string sourceLanguage, int startIndex)
        {
            var result = new List<CopyPasteBlock>();
            var input = new StringBuilder();
            var paragraphs = new List<Paragraph>();
            for (var index = startIndex; index < _paragraphs.Count; index++)
            {
                var p = _paragraphs[index];
                var f = new Formatting();
                _formattings[index - startIndex] = f;
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                if (input.Length + text.Length + 3 >= maxBlockSize)
                {
                    result.Add(new CopyPasteBlock { TargetText = input.ToString().Trim(), Paragraphs = paragraphs });
                    input.Clear();
                    paragraphs = new List<Paragraph>();
                }
                if (input.Length > 0)
                    input.Append(Environment.NewLine + _separator + Environment.NewLine);
                input.Append(text);
                paragraphs.Add(p);
            }
            if (input.Length > 0 && input.ToString() != Environment.NewLine + _separator + Environment.NewLine)
            {
                result.Add(new CopyPasteBlock { TargetText = input.ToString().Trim(), Paragraphs = paragraphs });
            }
            return result;
        }

        public static List<TranslationPair> GetTranslationPairs()
        {
            return new List<TranslationPair>
            {
                MakeLanguage("AFRIKAANS", "af"),
                MakeLanguage("ALBANIAN", "sq"),
                MakeLanguage("AMHARIC", "am"),
                MakeLanguage("ARABIC", "ar"),
                MakeLanguage("ARMENIAN", "hy"),
                MakeLanguage("AZERBAIJANI", "az"),
                MakeLanguage("BASQUE", "eu"),
                MakeLanguage("BELARUSIAN", "be"),
                MakeLanguage("BENGALI", "bn"),
                MakeLanguage("BOSNIAN", "bs"),
                MakeLanguage("BULGARIAN", "bg"),
                MakeLanguage("BURMESE", "my"),
                MakeLanguage("CATALAN", "ca"),
                MakeLanguage("CEBUANO", "ceb"),
                MakeLanguage("CHICHEWA", "ny"),
                MakeLanguage("CHINESE", "zh"),
                MakeLanguage("CHINESE_SIMPLIFIED", "zh-CN"),
                MakeLanguage("CHINESE_TRADITIONAL", "zh-TW"),
                MakeLanguage("CORSICAN", "co"),
                MakeLanguage("CROATIAN", "hr"),
                MakeLanguage("CZECH", "cs"),
                MakeLanguage("DANISH", "da"),
                MakeLanguage("DUTCH", "nl"),
                MakeLanguage("ENGLISH", "en"),
                MakeLanguage("ESPERANTO", "eo"),
                MakeLanguage("ESTONIAN", "et"),
                MakeLanguage("FILIPINO", "tl"),
                MakeLanguage("FINNISH", "fi"),
                MakeLanguage("FRENCH", "fr"),
                MakeLanguage("FRISIAN", "fy"),
                MakeLanguage("GALICIAN", "gl"),
                MakeLanguage("GEORGIAN", "ka"),
                MakeLanguage("GERMAN", "de"),
                MakeLanguage("GREEK", "el"),
                MakeLanguage("GUJARATI", "gu"),
                MakeLanguage("HAITIAN CREOLE", "ht"),
                MakeLanguage("HAUSA", "ha"),
                MakeLanguage("HAWAIIAN", "haw"),
                MakeLanguage("HEBREW", "he"),
                MakeLanguage("HINDI", "hi"),
                MakeLanguage("HMOUNG", "hmn"),
                MakeLanguage("HUNGARIAN", "hu"),
                MakeLanguage("ICELANDIC", "is"),
                MakeLanguage("IGBO", "ig"),
                MakeLanguage("INDONESIAN", "id"),
                MakeLanguage("IRISH", "ga"),
                MakeLanguage("ITALIAN", "it"),
                MakeLanguage("JAPANESE", "ja"),
                MakeLanguage("JAVANESE", "jw"),
                MakeLanguage("KANNADA", "kn"),
                MakeLanguage("KAZAKH", "kk"),
                MakeLanguage("KHMER", "km"),
                MakeLanguage("KOREAN", "ko"),
                MakeLanguage("KURDISH", "ku"),
                MakeLanguage("KYRGYZ", "ky"),
                MakeLanguage("LAO", "lo"),
                MakeLanguage("LATIN", "la"),
                MakeLanguage("LATVIAN", "lv"),
                MakeLanguage("LITHUANIAN", "lt"),
                MakeLanguage("LUXEMBOURGISH", "lb"),
                MakeLanguage("MACEDONIAN", "mk"),
                MakeLanguage("MALAY", "ms"),
                MakeLanguage("MALAGASY", "mg"),
                MakeLanguage("MALAYALAM", "ml"),
                MakeLanguage("MALTESE", "mt"),
                MakeLanguage("MAORI", "mi"),
                MakeLanguage("MARATHI", "mr"),
                MakeLanguage("MONGOLIAN", "mn"),
                MakeLanguage("MYANMAR", "my"),
                MakeLanguage("NEPALI", "ne"),
                MakeLanguage("NORWEGIAN", "no"),
                MakeLanguage("PASHTO", "ps"),
                MakeLanguage("PERSIAN", "fa"),
                MakeLanguage("POLISH", "pl"),
                MakeLanguage("PORTUGUESE", "pt"),
                MakeLanguage("PUNJABI", "pa"),
                MakeLanguage("ROMANIAN", "ro"),
                MakeLanguage("ROMANJI", "romanji"),
                MakeLanguage("RUSSIAN", "ru"),
                MakeLanguage("SAMOAN", "sm"),
                MakeLanguage("SCOTS GAELIC", "gd"),
                MakeLanguage("SERBIAN", "sr"),
                MakeLanguage("SESOTHO", "st"),
                MakeLanguage("SHONA", "sn"),
                MakeLanguage("SINDHI", "sd"),
                MakeLanguage("SINHALA", "si"),
                MakeLanguage("SLOVAK", "sk"),
                MakeLanguage("SLOVENIAN", "sl"),
                MakeLanguage("SOMALI", "so"),
                MakeLanguage("SPANISH", "es"),
                MakeLanguage("SUNDANESE", "su"),
                MakeLanguage("SWAHILI", "sw"),
                MakeLanguage("SWEDISH", "sv"),
                MakeLanguage("TAJIK", "tg"),
                MakeLanguage("TAMIL", "ta"),
                MakeLanguage("TELUGU", "te"),
                MakeLanguage("THAI", "th"),
                MakeLanguage("TURKISH", "tr"),
                MakeLanguage("UKRAINIAN", "uk"),
                MakeLanguage("URDU", "ur"),
                MakeLanguage("UZBEK", "uz"),
                MakeLanguage("VIETNAMESE", "vi"),
                MakeLanguage("WELSH", "cy"),
                MakeLanguage("XHOSA", "xh"),
                MakeLanguage("YIDDISH", "yi"),
                MakeLanguage("YORUBA", "yo"),
                MakeLanguage("ZULU", "zu"),
            };
        }

        private static TranslationPair MakeLanguage(string code, string name)
        {
            return new TranslationPair(name, code, code);
        }

        public void Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            var input = new StringBuilder();
            _formattings = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                _formattings[index] = f;
                if (input.Length > 0)
                    input.Append(Environment.NewLine + Environment.NewLine);
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                text = text.Replace(Environment.NewLine, "\n");
                input.Append(text);
            }
        }

        public List<string> GetTranslationResult(string targetLanguage, string target, CopyPasteBlock block)
        {
            var list = MakeList(target, targetLanguage, _formattings);
            if (list.Count > block.Paragraphs.Count)
            {
                return list.Where(p => !string.IsNullOrEmpty(p)).ToList();
            }

            if (list.Count < block.Paragraphs.Count)
            {
                var splitList = SplitMergedLines(list, block.Paragraphs);
                if (splitList.Count == block.Paragraphs.Count)
                    return splitList;
            }

            return list;
        }

        private List<string> MakeList(string res, string targetLanguage, Formatting[] formattings)
        {
            var lines = new List<string>();
            var sb = new StringBuilder();
            foreach (var line in res.SplitToLines())
            {
                if (line.Trim() == _separator)
                {
                    if (sb.Length > 0)
                        lines.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            if (sb.Length > 0)
                lines.Add(sb.ToString().Trim());

            var resultList = new List<string>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                var s = WebUtility.HtmlDecode(line);
                s = s.Replace("<I>", "<i>");
                s = s.Replace("<I >", "<i>");
                s = s.Replace("< I >", "<i>");
                s = s.Replace("< i >", "<i>");
                s = s.Replace("</I>", "</i>");
                s = s.Replace("</I >", "</i>");
                s = s.Replace("</ I>", "</i>");
                s = s.Replace("</ i>", "</i>");
                s = s.Replace("< / i>", "</i>");
                s = string.Join(Environment.NewLine, s.SplitToLines());
                s = TranslationHelper.PostTranslate(s, targetLanguage);
                s = s.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                if (formattings.Length > index)
                    s = formattings[index].ReAddFormatting(s);
                resultList.Add(s.Replace("  ", " "));
            }
            return resultList;
        }

        private static List<string> SplitMergedLines(List<string> input, List<Paragraph> paragraphs)
        {
            var hits = 0;
            var results = new List<string>();
            for (var index = 0; index < input.Count; index++)
            {
                var line = input[index];
                var text = paragraphs[index].Text;
                var badPoints = 0;
                if (text.StartsWith("[") && !line.StartsWith("["))
                    badPoints++;
                if (text.StartsWith("-") && !line.StartsWith("-"))
                    badPoints++;
                if (text.Length > 0 && char.IsUpper(text[0]) && line.Length > 0 && !char.IsUpper(line[0]))
                    badPoints++;
                if (text.EndsWith(".") && !line.EndsWith("."))
                    badPoints++;
                if (text.EndsWith("!") && !line.EndsWith("!"))
                    badPoints++;
                if (text.EndsWith("?") && !line.EndsWith("?"))
                    badPoints++;
                if (text.EndsWith(",") && !line.EndsWith(","))
                    badPoints++;
                if (text.EndsWith(":") && !line.EndsWith(":"))
                    badPoints++;
                var added = false;
                if (badPoints > 0 && hits + input.Count < paragraphs.Count)
                {
                    var percent = line.Length * 100.0 / text.Length;
                    if (percent > 150)
                    {
                        var temp = Utilities.AutoBreakLine(line).SplitToLines();
                        if (temp.Count == 2)
                        {
                            hits++;
                            results.Add(temp[0]);
                            results.Add(temp[1]);
                            added = true;
                        }
                    }
                }
                if (!added)
                {
                    results.Add(line);
                }
            }

            if (results.Count == paragraphs.Count)
                return results;

            return input;
        }
    }
}
