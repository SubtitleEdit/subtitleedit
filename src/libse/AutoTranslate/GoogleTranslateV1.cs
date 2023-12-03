using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Core.Translate.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// Google translate via Google V1 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslateV1 : IAutoTranslator
    {
        private HttpClient _httpClient;

        public static string StaticName { get; set; } = "Google Translate V1 API";
        public string Name => StaticName;
        public string Url => "https://translate.google.com/";
        public string Error { get; set; }
        public int MaxCharacters => 1500;

        public void Initialize()
        {
            _httpClient?.Dispose();
            _httpClient = new HttpClient(); //DownloaderFactory.MakeHttpClient();
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=UTF-8");
            _httpClient.BaseAddress = new Uri("https://translate.googleapis.com/");
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return GoogleTranslationService.GetTranslationPairs();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return GoogleTranslationService.GetTranslationPairs();
        }

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode)
        {
            string jsonResultString;

            try
            {
                var url = $"translate_a/single?client=gtx&sl={sourceLanguageCode}&tl={targetLanguageCode}&dt=t&q={Utilities.UrlEncode(text)}";

                var result = _httpClient.GetAsync(url).Result;
                var bytes = result.Content.ReadAsByteArrayAsync().Result;
                jsonResultString = Encoding.UTF8.GetString(bytes).Trim();
            }
            catch (WebException webException)
            {
                throw new TranslationException("Free API quota exceeded?", webException);
            }

            var resultList = ConvertJsonObjectToStringLines(jsonResultString);
            return Task.FromResult(string.Join(Environment.NewLine, resultList));
        }

        private static List<string> ConvertJsonObjectToStringLines(string result)
        {
            var sbAll = new StringBuilder();
            var count = 0;
            var i = 1;
            var level = result.StartsWith('[') ? 1 : 0;
            while (i < result.Length - 1)
            {
                var sb = new StringBuilder();
                var start = false;
                for (; i < result.Length - 1; i++)
                {
                    var c = result[i];
                    if (start)
                    {
                        if (c == '\\' && result[i + 1] == '\\')
                        {
                            i++;
                        }
                        else if (c == '\\' && result[i + 1] == '"')
                        {
                            c = '"';
                            i++;
                        }
                        else if (c == '"')
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
            try
            {
                res = Regex.Unescape(res);
            }
            catch
            {
                res = res.Replace("\\n", "\n");
            }

            var lines = res.SplitToLines().ToList();
            return lines;
        }
    }
}
