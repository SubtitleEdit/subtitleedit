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
                    input.Append(" | ");
                }
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text.Replace("|", string.Empty) , sourceLanguage), sourceLanguage);
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
                            if (count % 2 == 1 && level > 2) // even numbers are original text, level > 3 is translation
                                sbAll.Append(" " + sb);
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
            List<string> lines = Split(res);
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
                    s = formattings[index].ReAddFormatting(s);
                resultList.Add(s);
            }

            return resultList;
        }

        private List<string> Split(string res)
        {
            return res.Split('|').ToList();
        }
    }
}
