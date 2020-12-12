using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public delegate bool TranslationCancelStatusCallback(Dictionary<int, string> targetParagraphs);

    public interface ITranslator
    {
        List<string> Translate(ITranslationService translationService, string source, string target, List<Paragraph> paragraphs, TranslationCancelStatusCallback cancelStatusCallback);
    }

    public abstract class Translator<T> : ITranslator where T : ITranslationUnit
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




        protected abstract IEnumerable<T> ConstructTranslationUnits(List<Paragraph> sourceParagraphs);
        protected abstract Dictionary<int,string> GetTargetParagraphs(List<T> sourceTranslationUnits, List<string> targetTexts);


        public List<string> Translate(ITranslationService translationService, string source, string target, List<Paragraph> paragraphs, TranslationCancelStatusCallback cancelStatusCallback )
        {
            var translationUnits =ConstructTranslationUnits(paragraphs);
            var translationChunks = BuildTranslationChunks(translationUnits, translationService);
            var log = new StringBuilder();

            Dictionary<int,string> targetParagraphs=new Dictionary<int, string>();

            foreach (var translationChunk in translationChunks)
            {
                List<string> result = translationService.Translate(source, target, translationChunk.TranslationUnits.ConvertAll(x=>new Paragraph() { Text = x.GetText() }), log);
                Dictionary<int, string> newTargetParagraphs=GetTargetParagraphs(translationChunk.TranslationUnits, result);
                foreach (KeyValuePair<int, string> newTargetParagraph in newTargetParagraphs)
                {
                    targetParagraphs[newTargetParagraph.Key] = newTargetParagraph.Value;
                }
                if (cancelStatusCallback(newTargetParagraphs)) //check if operation was canceled outside
                {
                    return targetParagraphs.Values.ToList();
                }
            }
            return targetParagraphs.Values.ToList();
        }

        private List<TranslationChunk> BuildTranslationChunks(IEnumerable<T> translationUnits, ITranslationService translationService)
        {

            List<TranslationChunk> chunks = new List<TranslationChunk>();

            int maxTextSize = translationService.MaxTextSize;
            int maximumRequestArrayLength = translationService.MaximumRequestArraySize;

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
