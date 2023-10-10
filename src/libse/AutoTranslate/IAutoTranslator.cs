using System.Collections.Generic;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Translate;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public interface IAutoTranslator
    {
        /// <summary>
        /// Name of auto translator (can be translated).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Url to homepage.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Initialization before calling translate.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Get languages that can be translated from.
        /// </summary>
        List<TranslationPair> GetSupportedSourceLanguages();

        /// <summary>
        /// Get languages that can be translated to.
        /// </summary>
        List<TranslationPair> GetSupportedTargetLanguages();

        /// <summary>
        /// Do translation.
        /// </summary>
        Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode);
    }
}