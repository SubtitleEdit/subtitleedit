using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Translate
{
    /// <summary>
    /// Google translate via Google Cloud API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslator2 : ITranslator
    {
        public List<TranslationPair> GetTranslationPairs()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Google translate";
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<string> texts, StringBuilder log)
        {
            string result;
            using (var wc = new WebClient())
            {
                string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={Utilities.UrlEncode("input")}";
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

            result = sbAll.ToString().Trim();
            result = Regex.Unescape(result);
            result = Json.DecodeJsonText(result);

            string res = result;
            res = string.Join(Environment.NewLine, res.SplitToLines());
            res = res.Replace("NewlineString", Environment.NewLine);
            res = res.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
            res = res.Replace(Environment.NewLine + " ", Environment.NewLine);
            res = res.Replace(Environment.NewLine + " ", Environment.NewLine);
            res = res.Replace(" " + Environment.NewLine, Environment.NewLine);
            res = res.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();

            return null;
        }
    }
}
