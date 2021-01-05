using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    public interface ITranslationService : ITranslationStrategy
    {
        List<TranslationPair> GetSupportedSourceLanguages();

        List<TranslationPair> GetSupportedTargetLanguages();
    }
}
