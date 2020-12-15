using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public interface ITranslationStrategy
    {
        List<TranslationPair> GetTranslationPairs();
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> paragraphs, StringBuilder log);
        int GetMaxTextSize();
        int GetMaximumRequestArraySize();

    }
}
