using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    /// <summary>
    /// Optional extension for <see cref="IAutoTranslator"/> implementations that can use the
    /// surrounding subtitle lines (previous/next) as extra context for the translation.
    /// </summary>
    public interface IAutoTranslatorWithContext
    {
        /// <summary>
        /// Translates the given text, providing the neighboring subtitle lines as extra context.
        /// </summary>
        /// <param name="text">The text to be translated.</param>
        /// <param name="sourceLanguageCode">The language code of the source language.</param>
        /// <param name="targetLanguageCode">The language code of the target language.</param>
        /// <param name="previousLineText">The subtitle line right before <paramref name="text"/>, or empty if none.</param>
        /// <param name="nextLineText">The subtitle line right after <paramref name="text"/>, or empty if none.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the translation process.</param>
        Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, string previousLineText, string nextLineText, CancellationToken cancellationToken);
    }
}
