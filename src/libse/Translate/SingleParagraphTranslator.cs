using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    

    public class SingleParagraphTranslationProcessor : AbstractTranslationProcessor<SingleParagraphTranslationProcessor.IndexedParagraph>
    {
        public class IndexedParagraph : ITranslationUnit
        {
            private readonly Paragraph _sourceParagraph;
            public int Index { get; }

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

        public override string ToString()
        {
            return "Single Paragraph";
        }

        protected override IEnumerable<IndexedParagraph> ConstructTranslationUnits(List<Paragraph> sourceParagraphs)
        {
            foreach (var sourceParagraph in sourceParagraphs)
            {
                yield return new IndexedParagraph(sourceParagraph, sourceParagraph.Number);
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
