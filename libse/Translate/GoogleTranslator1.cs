using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Translate
{
    /// <summary>
    /// Google translate via Google V1 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslator1 : ITranslator
    {
        private const char SplitChar = '\n';

        public List<TranslationPair> GetTranslationPairs()
        {
            return new GoogleTranslator2(string.Empty).GetTranslationPairs();
        }

        public string GetName()
        {
            return "Google translate (old)";
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            string result;
            var input = new StringBuilder();
            var formattings = new Formatting[paragraphs.Count];
            for (var index = 0; index < paragraphs.Count; index++)
            {
                var p = paragraphs[index];
                var f = new Formatting();
                formattings[index] = f;
                if (input.Length > 0)
                {
                    input.Append(" " + SplitChar + " ");
                }
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text.Replace(SplitChar.ToString(), string.Empty), sourceLanguage), sourceLanguage);
                text = f.Unbreak(text, p.Text);
                input.Append(text);
            }

            using (var wc = new WebClient())
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={Utilities.UrlEncode(input.ToString())}";
                wc.Proxy = Utilities.GetProxy();
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
                result = wc.DownloadString(url).Trim();
            }

            var sbAll = new StringBuilder();
            int count = 0;
            int i = 1;
            int level = result.StartsWith('[') ? 1 : 0;
            while (i < result.Length - 1)
            {
                var sb = new StringBuilder();
                var start = false;
                for (; i < result.Length - 1; i++)
                {
                    var c = result[i];
                    if (start)
                    {
                        if (c == '"' && result[i - 1] != '\\')
                        {
                            count++;
                            if (count % 2 == 1 && level > 2 && level < 5) // even numbers are original text, level 3 is translation
                            {
                                sbAll.Append(" " + sb);
                            }

                            i++;
                            break;
                        }
                        sb.Append(c);
                    }
                    else if (c == '"')
                    {
                        start = true;
                    }
                    else if (c == '[')
                    {
                        level++;
                    }
                    else if (c == ']')
                    {
                        level--;
                    }
                }
            }

            var res = sbAll.ToString().Trim();
            res = Regex.Unescape(res);
            var lines = res.SplitToLines().ToList();
            var resultList = new List<string>();
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var s = Json.DecodeJsonText(line);
                s = string.Join(Environment.NewLine, s.SplitToLines());
                s = TranslationHelper.PostTranslate(s, targetLanguage);
                s = s.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                if (formattings.Length > index)
                {
                    s = formattings[index].ReAddFormatting(s);
                    s = formattings[index].Rebreak(s);
                }

                resultList.Add(s);
            }

            if (resultList.Count > paragraphs.Count)
            {
                var trimmedList = resultList.Where(p => !string.IsNullOrEmpty(p)).ToList();
                if (trimmedList.Count == paragraphs.Count)
                {
                    return trimmedList;
                }
            }

            if (resultList.Count < paragraphs.Count)
            {
                var splitList = SplitMergedLines(resultList, paragraphs);
                if (splitList.Count == paragraphs.Count)
                {
                    return splitList;
                }
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
                if (text.StartsWith('[') && !line.StartsWith('['))
                {
                    badPoints++;
                }

                if (text.StartsWith('-') && !line.StartsWith('-'))
                {
                    badPoints++;
                }

                if (text.Length > 0 && char.IsUpper(text[0]) && line.Length > 0 && !char.IsUpper(line[0]))
                {
                    badPoints++;
                }

                if (text.EndsWith('.') && !line.EndsWith('.'))
                {
                    badPoints++;
                }

                if (text.EndsWith('!') && !line.EndsWith('!'))
                {
                    badPoints++;
                }

                if (text.EndsWith('?') && !line.EndsWith('?'))
                {
                    badPoints++;
                }

                if (text.EndsWith(',') && !line.EndsWith(','))
                {
                    badPoints++;
                }

                if (text.EndsWith(':') && !line.EndsWith(':'))
                {
                    badPoints++;
                }

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
            {
                return results;
            }

            return input;
        }
    }
}
