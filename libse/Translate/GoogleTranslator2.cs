using System;
using System.Collections.Generic;
using System.Text;

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
            throw new NotImplementedException();
        }
    }
}
