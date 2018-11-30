using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate
{
    /// <summary>
    /// Google translate via Google Cloud API - see https://cloud.google.com/translate/
    /// </summary>
    public class GoogleTranslator1 : ITranslator
    {
        public List<TranslationPair> GetTranslationPairs()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Google translate (old)";
        }

        public string GetUrl()
        {
            return "https://translate.google.com/";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<string> texts, StringBuilder log)
        {
            string baseUrl = "https://translation.googleapis.com/language/translate/v2";
            var format = "text";
            var input = new StringBuilder();
            foreach (var text in texts)
            {
                if (input.Length > 0)
                    input.Append("&");
                input.Append("q=" + Utilities.UrlEncode(text));
            }
            string uri = $"{baseUrl}/?{input}&target={targetLanguage}&source={sourceLanguage}&format={format}&key={Configuration.Settings.Tools.GoogleApiV2Key}";

            var request = WebRequest.Create(uri);
            request.Proxy = Utilities.GetProxy();
            request.ContentType = "application/json";
            request.ContentLength = 0;
            request.Method = "POST";
            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();

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
                                           // return PostTranslate(translatedText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
