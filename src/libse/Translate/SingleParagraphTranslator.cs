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
        public int Index { get; set; }

        public IndexedParagraph(Paragraph sourceParagraph)
        {
            this._sourceParagraph = sourceParagraph;
        }

        public string GetText()
        {
            return _sourceParagraph.Text;
        }
    }

    public class SingleParagraphTranslator : Translator<IndexedParagraph>
    {
        public SingleParagraphTranslator(ITranslationService translationService) : base(translationService)
        {
        }

        protected override IEnumerable<IndexedParagraph> ConstructTranslationUnits(List<Paragraph> sourceParagraphs)
        {
            return sourceParagraphs.ConvertAll(x => new IndexedParagraph(x));
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
