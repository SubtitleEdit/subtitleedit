using System.Collections.Generic;
using System.Threading;
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
        /// Represents an error message or description.
        /// </summary>
        string Error { get; set; }

        /// <summary>
        /// Gets the maximum number of characters that can be translated at once.
        /// </summary>
        int MaxCharacters { get; }

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
        /// Translates the given text from the source language to the target language.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="sourceLanguageCode">The language code of the source language.</param>
        /// <param name="targetLanguageCode">The language code of the target language.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the translation process.</param>
        /// <returns>A task that represents the asynchronous translation operation.
        /// The task result contains the translated text.</returns>
        Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken);
    }
}