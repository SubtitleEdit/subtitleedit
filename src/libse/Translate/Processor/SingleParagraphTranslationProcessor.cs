using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    public class SingleParagraphTranslationProcessor : AbstractTranslationProcessor<Paragraph>
    {
        public override string ToString()
        {
            return "Single Paragraph";
        }

        protected override IEnumerable<Paragraph> ConstructTranslationBaseUnits(List<Paragraph> sourceParagraphs)
        {
            return sourceParagraphs;
        }

        protected override Dictionary<int, string> GetTargetParagraphs(List<Paragraph> sourceTranslationUnits, List<string> targetTexts)
        {
            Dictionary<int, string> targetParagraphs = new Dictionary<int, string>();
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
