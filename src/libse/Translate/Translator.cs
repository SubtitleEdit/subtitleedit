using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public abstract class Translator<T> where T : ITranslationUnit
    {
        /**
         * due to translation service constraints not all paragraphs can't submitted at once. Therefore the paragraphs must be split in multiple Chunks
         */
        private class TranslationChunk
        {

            public List<T> TranslationUnits = new List<T>();
            public int TextSize()
            {
                return Enumerable.Sum(TranslationUnits.ConvertAll(e => Utilities.UrlEncode(e.GetText()).Length));
            }

            public int ArrayLength()
            {
                return TranslationUnits.Count;
            }
        }


        private readonly ITranslationService _translationService;

        public delegate bool TranslationCancelStatusCallback(Dictionary<int, string> targetParagraphs);

        public Translator(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        protected abstract IEnumerable<T> ConstructTranslationUnits(List<Paragraph> sourceParagraphs);
        protected abstract Dictionary<int,string> GetTargetParagraphs(List<T> sourceTranslationUnits, List<string> targetTexts);


        public void Translate(string source, string target, List<Paragraph> paragraphs, TranslationCancelStatusCallback cancelStatusCallback)
        {
            var translationUnits =ConstructTranslationUnits(paragraphs);


            var translationChunks = BuildTranslationChunks(translationUnits);
            var log = new StringBuilder();

            foreach (var translationChunk in translationChunks)
            {
                List<string> result = _translationService.Translate(source, target, translationChunk.TranslationUnits.ConvertAll(x=>new Paragraph() { Text = x.GetText() }), log);

                Dictionary<int, string> targetParagraphs=GetTargetParagraphs(translationChunk.TranslationUnits, result);
           

                if (cancelStatusCallback(targetParagraphs))
                {
                    return;
                }
            }
        }

        private List<TranslationChunk> BuildTranslationChunks(IEnumerable<T> translationUnits)
        {

            List<TranslationChunk> chunks = new List<TranslationChunk>();

            int maxTextSize = _translationService.MaxTextSize;
            int maximumRequestArrayLength = _translationService.MaximumRequestArraySize;

            TranslationChunk currentChunk = new TranslationChunk();

            foreach (var translationUnit in translationUnits)
            {
                if (currentChunk.TextSize() + Utilities.UrlEncode(translationUnit.GetText()).Length > maxTextSize || currentChunk.ArrayLength() + 1 > maximumRequestArrayLength)
                {
                    chunks.Add(currentChunk);
                    currentChunk = new TranslationChunk();
                }
                currentChunk.TranslationUnits.Add(translationUnit);
            }

            if (currentChunk.ArrayLength() > 0)
            {
                chunks.Add(currentChunk);
            }

            return chunks;
        }
    }
}
