using System.Collections.Generic;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public interface ITranslationStrategy
    {

        string GetUrl();
        List<string> Translate(string sourceLanguage, string targetLanguage, List<Paragraph> sourceParagraphs);
        int GetMaxTextSize();
        int GetMaximumRequestArraySize();

    }
}
