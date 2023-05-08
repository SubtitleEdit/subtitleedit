using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Http;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    /// <summary>
    /// Google translate via Google Cloud V3 API - see https://cloud.google.com/translate/
    ///
    /// Not used at the moment!
    /// </summary>
    public class GoogleTranslator3 : ITranslationStrategy
    {
        private readonly string _apiKey;
        private readonly string _projectNumberOrId;
        private readonly IDownloader _httpClient;

        public GoogleTranslator3(string apiKey, string projectNumberOrId)
        {
            _apiKey = apiKey;
            _projectNumberOrId = projectNumberOrId;
            _httpClient = DownloaderFactory.MakeHttpClient();
            _httpClient.BaseAddress = new Uri("https://translation.googleapis.com/v3/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetName()
        {
            return "Google Translate Cloud V3";
        }

        public int GetMaxTextSize()
        {
            return 1000;
        }


        public int GetMaximumRequestArraySize()
        {
            return 100; 
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> sourceParagraphs)
        {
            //TODO: Get access token...

            var input = new StringBuilder();
            var formatList = new List<Formatting>();
            for (var index = 0; index < sourceParagraphs.Count; index++)
            {

                var p = sourceParagraphs[index];
                var f = new Formatting();
                formatList.Add(f);
                if (input.Length > 0)
                {
                    input.Append(",");
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                text = f.UnBreak(text, p.Text);

                input.Append("\"" + Json.EncodeJsonText(text) + "\"");
            }

            var url = $"{_projectNumberOrId}:translateText";
            var result = _httpClient.PostAsync(url, new StringContent(string.Empty)).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            var resultList = new List<string>();
            var parser = new JsonParser();
            var x = (Dictionary<string, object>)parser.Parse(content);
            foreach (var k in x.Keys)
            {
                if (x[k] is Dictionary<string, object> v)
                {
                    foreach (var innerKey in v.Keys)
                    {
                        if (v[innerKey] is List<object> l)
                        {
                            foreach (var o2 in l)
                            {
                                if (o2 is Dictionary<string, object> v2)
                                {
                                    foreach (var innerKey2 in v2.Keys)
                                    {
                                        if (v2[innerKey2] is string translatedText)
                                        {

                                            translatedText = Regex.Unescape(translatedText);
                                            translatedText = string.Join(Environment.NewLine, translatedText.SplitToLines());
                                            translatedText = TranslationHelper.PostTranslate(translatedText, targetLanguage);
                                            if (resultList.Count < formatList.Count)
                                            {
                                                translatedText = formatList[resultList.Count].ReAddFormatting(translatedText);
                                                translatedText = formatList[resultList.Count].ReBreak(translatedText, targetLanguage);
                                            }

                                            resultList.Add(translatedText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return resultList;
        }
    }
}
