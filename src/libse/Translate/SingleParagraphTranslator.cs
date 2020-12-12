using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class IndexedParagraph : ITranslationUnit
    {
        private readonly Paragraph _sourceParagraph;
        public int Index { get;  }

        public IndexedParagraph(Paragraph sourceParagraph, int index)
        {
            this._sourceParagraph = sourceParagraph;
            this.Index = index;
        }

        public string GetText()
        {
            return _sourceParagraph.Text;
        }
    }

    public class SingleParagraphTranslator : Translator<IndexedParagraph>
    {
        public override string ToString()
        {
            return "Single Paragraph";
        }

        protected override IEnumerable<IndexedParagraph> ConstructTranslationUnits(List<Paragraph> sourceParagraphs)
        {
            for (int i = 0; i < sourceParagraphs.Count; i++)
            {
                yield return new IndexedParagraph(sourceParagraphs[i], i);
            }

        }

        protected override Dictionary<int, string> GetTargetParagraphs(List<IndexedParagraph> sourceTranslationUnits, List<string> targetTexts)
        {
            Dictionary<int, string> targetParagraphs = new Dictionary<int, string>();
            for (int i = 0; i < sourceTranslationUnits.Count; i++)
            {
                targetParagraphs.Add(sourceTranslationUnits[i].Index, targetTexts[i]);
            }
            return targetParagraphs;
        }
    }
}
