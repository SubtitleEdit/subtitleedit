using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    public class SingleParagraphTranslationProcessor : AbstractTranslationProcessor<Paragraph>
    {
        protected override string GetName()
        {
            return "Single Paragraph";
        }

        protected override IEnumerable<Paragraph> ConstructTranslationBaseUnits(List<Paragraph> sourceParagraphs)
        {
            return sourceParagraphs;
        }

        protected override Dictionary<int, string> GetTargetParagraphs(List<Paragraph> sourceTranslationUnits, List<string> targetTexts)
        {
            var targetParagraphs = new Dictionary<int, string>();
            for (int i = 0; i < sourceTranslationUnits.Count; i++)
            {
                targetParagraphs.Add(sourceTranslationUnits[i].Number, targetTexts[i]);
            }
            return targetParagraphs;
        }

        public override List<string> GetSupportedLanguages()
        {
            return null;
        }
    }
}
