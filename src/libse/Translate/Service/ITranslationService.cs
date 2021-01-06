using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    public interface ITranslationService : ITranslationStrategy
    {
        List<TranslationPair> GetSupportedSourceLanguages();

        List<TranslationPair> GetSupportedTargetLanguages();
    }
}
