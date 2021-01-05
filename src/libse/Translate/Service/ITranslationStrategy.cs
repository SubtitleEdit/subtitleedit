using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Translate.Service
{
    public interface ITranslationStrategy
    {
        string GetName();
        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> sourceParagraphs);
        int GetMaxTextSize();
        int GetMaximumRequestArraySize();
    }
}
