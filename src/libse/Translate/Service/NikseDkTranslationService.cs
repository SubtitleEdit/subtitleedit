using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    public class NikseDkTranslationService : ITranslationService
    {

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("SWEDISH", "sv"),
            };
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return new List<TranslationPair>
            {
                new TranslationPair("DANISH", "da"),
            };
        }

        public string GetName()
        {
            return "nikse.dk Multi Translator";
        }

        public string GetUrl()
        {
            return "https://www.nikse.dk/MultiTranslator/online";
        }

        public List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> sourceParagraphs)
        {
            var targetTexts = new List<string>();
            var sb = new StringBuilder();
            foreach (var p in sourceParagraphs)
            {
                var s = p.Text.Replace(Environment.NewLine, "<br/>");
                s = "<p>" + s + "</p>";
                sb.Append(s);
            }

            string result = GetTranslateStringFromNikseDk(sb.ToString());
            foreach (var s in result.Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries))
            {
                targetTexts.Add(s.Trim());
            }

            if (sourceParagraphs.Count != targetTexts.Count)
            {
                throw new Exception("target paragraphs count (" + targetTexts + ") does not equal source paragraphs count (" + sourceParagraphs + ").");
            }
            return targetTexts;
        }

        private static string GetTranslateStringFromNikseDk(string input)
        {
            WebRequest.DefaultWebProxy = Utilities.GetProxy();
            //var request = WebRequest.Create("http://localhost:54942/MultiTranslator/TranslateForSubtitleEdit");
            var request = WebRequest.Create("https://www.nikse.dk/MultiTranslator/TranslateForSubtitleEdit");
            request.Method = "POST";
            var postData = String.Format("languagePair={1}&text={0}", Utilities.UrlEncode(input), "svda");
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public int GetMaxTextSize()
        {
            return 9000; //brummochse: found this value in the old source code.. is it correct?
        }

        public int GetMaximumRequestArraySize()
        {
            return 1000; //brummochse: actually, when I understand the service correctly, no limit is required because it is only limited by text size
        }

        public override string ToString()
        {
            return GetName();
        }
    }
}
