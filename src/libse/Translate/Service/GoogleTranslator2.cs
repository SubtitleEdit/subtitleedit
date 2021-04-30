using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    /// <summary>
    /// Google translate via Google Cloud V2 API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslator2 : ITranslationStrategy
    {
        private readonly string _apiKey;

        public string GetName()
        {
            return "Google Translate Cloud V2 API";
        }

        public int GetMaxTextSize()
        {
            return 1000; //brummochse: this value was the old translation method call. idk if this is really the correct value
        }

        public int GetMaximumRequestArraySize()
        {
            return 100; //brummochse: this value was the old default value for the old translation method. idk if this is really the correct value
        }

        public GoogleTranslator2(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> sourceParagraphs)
        {
            var baseUrl = "https://translation.googleapis.com/language/translate/v2";
            var format = "text";
            var input = new StringBuilder();
            var formatList = new List<Formatting>();
            for (var index = 0; index < sourceParagraphs.Count; index++)
            {

                var p = sourceParagraphs[index];
                var f = new Formatting();
                formatList.Add(f);
                if (input.Length > 0)
                {
                    input.Append("&");
                }

                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                text = f.UnBreak(text, p.Text);

                input.Append("q=" + Utilities.UrlEncode(text));
            }

            string uri = $"{baseUrl}/?{input}&target={targetLanguage}&source={sourceLanguage}&format={format}&key={_apiKey}";
            string content;
            try
            {
                var request = WebRequest.Create(uri);
                request.Proxy = Utilities.GetProxy();
                request.ContentType = "application/json";
                request.ContentLength = 0;
                request.Method = "POST";
                var response = request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                content = reader.ReadToEnd();
            }
            catch (WebException webException)
            {
                var message = string.Empty;
                if (webException.Message.Contains("(400) Bad Request"))
                {
                    message = "API key invalid (or perhaps billing is not enabled)?";
                }
                else if (webException.Message.Contains("(403) Forbidden."))
                {
                    message = "Perhaps billing is not enabled (or API key is invalid)?";
                }
                throw new TranslationException(message, webException);
            }

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
                                            translatedText = Regex.Unescape(translatedText);
                                            translatedText = string.Join(Environment.NewLine, translatedText.SplitToLines());
                                            translatedText = TranslationHelper.PostTranslate(translatedText, targetLanguage);
                                            if (resultList.Count - skipCount < formatList.Count)
                                            {
                                                translatedText = formatList[resultList.Count - skipCount].ReAddFormatting(translatedText);
                                                translatedText = formatList[resultList.Count - skipCount].ReBreak(translatedText, targetLanguage);
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
