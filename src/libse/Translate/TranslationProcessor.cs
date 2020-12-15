using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{

    public interface ITranslationProcessor
    {
        List<string> Translate(ITranslationService translationService, string sourceLanguageIsoCode, string targetLanguageIsoCode, List<Paragraph> paragraphs, TranslationProcessCancelStatus processCancelStatus);
    }

    /// <summary>
    /// callback for the translation progress. gets called every time when a translation chunk was processed
    /// </summary>
    /// <param name="targetParagraphs">recently translated paragraphs (key: index of the original source paragraph, value: translated text)</param>
    /// <returns></returns>
    public delegate bool TranslationProcessCancelStatus(Dictionary<int, string> targetParagraphs);

    public abstract class AbstractTranslationProcessor<T> : ITranslationProcessor where T : AbstractTranslationProcessor<T>.ITranslationUnit
    {
        public interface ITranslationUnit
        {
            string GetText();

        }
        /**
         * due to translation service constraints not all paragraphs can't submitted at once. Therefore the paragraphs must be split in multiple Chunks
         */
        private class TranslationChunk
        {

            public readonly List<T> TranslationUnits = new List<T>();

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

        public List<string> Translate(ITranslationService translationService, string sourceLanguageIsoCode, string targetLanguageIsoCode, List<Paragraph> paragraphs, TranslationProcessCancelStatus processCancelStatus )
        {
            IEnumerable<T> translationUnits =ConstructTranslationUnits(paragraphs);
            List<TranslationChunk> translationChunks = BuildTranslationChunks(translationUnits, translationService);
            var log = new StringBuilder();

            Dictionary<int,string> targetParagraphs=new Dictionary<int, string>();

            foreach (TranslationChunk translationChunk in translationChunks)
            {
                List<string> result = translationService.Translate(sourceLanguageIsoCode, targetLanguageIsoCode, translationChunk.TranslationUnits.ConvertAll(x=>new Paragraph() { Text = x.GetText() }), log);
                Dictionary<int, string> newTargetParagraphs=GetTargetParagraphs(translationChunk.TranslationUnits, result);
                foreach (KeyValuePair<int, string> newTargetParagraph in newTargetParagraphs)
                {
                    targetParagraphs[newTargetParagraph.Key] = newTargetParagraph.Value;
                }
                if (processCancelStatus(newTargetParagraphs)) //check if operation was canceled outside
                {
                    return targetParagraphs.Values.ToList();
                }
            }
            return targetParagraphs.Values.ToList();
        }

        private List<TranslationChunk> BuildTranslationChunks(IEnumerable<T> translationUnits, ITranslationService translationService)
        {
            List<TranslationChunk> chunks = new List<TranslationChunk>();

            int maxTextSize = translationService.GetMaxTextSize();
            int maximumRequestArrayLength = translationService.GetMaximumRequestArraySize();

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
