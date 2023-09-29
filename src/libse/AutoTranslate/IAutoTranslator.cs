using System.Collections.Generic;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public interface IAutoTranslator
    {
        void Initialize(string url);
        string Url { get; }
        List<TranslationPair> GetSupportedSourceLanguages();
        List<TranslationPair> GetSupportedTargetLanguages();
        Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode);
    }
}