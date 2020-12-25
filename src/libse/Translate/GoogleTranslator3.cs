using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Translate
{
    /// <summary>
    /// Google translate via Google Cloud V3 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslator3 : ITranslator
    {
        private readonly string _apiKey;
        private readonly string _projectNumberOrId;

        public GoogleTranslator3(string apiKey, string projectNumberOrId)
        {
            _apiKey = apiKey;
            _projectNumberOrId = projectNumberOrId;
        }

        public List<TranslationPair> GetTranslationPairs()
        {
            return new GoogleTranslator2(null).GetTranslationPairs();
        }

        public string GetName()
        {
            return "Google translate V3";
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log)
        {
            //TODO: Get access token...

            var input = new StringBuilder();
            var formatList = new List<Formatting>();
            bool skipNext = false;
            for (var index = 0; index < paragraphs.Count; index++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                var p = paragraphs[index];
                var f = new Formatting();
                formatList.Add(f);
                if (input.Length > 0)
                {
                    input.Append(",");
                }

                var nextText = string.Empty;
                if (index < paragraphs.Count - 1 && paragraphs[index + 1].StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 200)
                {
                    nextText = paragraphs[index + 1].Text;
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage, nextText);
                skipNext = f.SkipNext;
                if (!skipNext)
                {
                    text = f.UnBreak(text, p.Text);
                }

                input.Append("\"" + Json.EncodeJsonText(text) + "\"");
            }

            var request = (HttpWebRequest)WebRequest.Create($"https://translation.googleapis.com/v3/{_projectNumberOrId}:translateText");
            request.Proxy = Utilities.GetProxy();
            request.ContentType = "application/json";
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var requestJson = "{ \"sourceLanguageCode\": \"" + sourceLanguage + "\", \"targetLanguageCode\": \"" + targetLanguage + "\", " +
                                  "\"contents\": [" + input + "]}";
                streamWriter.Write(requestJson);
            }
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();
            var skipCount = 0;
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
                                            string nextText = null;
                                            translatedText = Regex.Unescape(translatedText);
                                            translatedText = string.Join(Environment.NewLine, translatedText.SplitToLines());
                                            translatedText = TranslationHelper.PostTranslate(translatedText, targetLanguage);
                                            if (resultList.Count - skipCount < formatList.Count)
                                            {
                                                translatedText = formatList[resultList.Count - skipCount].ReAddFormatting(translatedText, out nextText);
                                                if (nextText == null)
                                                {
                                                    translatedText = formatList[resultList.Count - skipCount].ReBreak(translatedText, targetLanguage);
                                                }
                                            }

                                            resultList.Add(translatedText);

                                            if (nextText != null)
                                            {
                                                resultList.Add(nextText);
                                                skipCount++;
                                            }
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
