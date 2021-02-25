using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate.Service;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    /// <summary>
    /// handles complex translation tasks and cares about:
    /// - input text chunking (complying translation service specific constraints)
    /// - status report & cancellation option
    /// - how to pass the paragraphs into the translator (paragraph line handling)
    /// </summary>
    public interface ITranslationProcessor
    {
        List<string> Translate(ITranslationService translationService,
            string sourceLanguageIsoCode,
            string targetLanguageIsoCode,
            List<Paragraph> sourceParagraphs,
            TranslationProcessCancelStatus processCancelStatus = null);

        /// <summary>
        /// return the languages (as iso code) this Processor is suitable.
        /// </summary>
        /// <returns>null = no language restriction, empty = no language</returns>
        List<string> GetSupportedLanguages();
    }

    public interface ITranslationBaseUnit
    {
        string Text { get; }
    }

    /// <summary>
    /// callback for the translation progress. gets called every time when a translation chunk was processed
    /// </summary>
    /// <param name="targetParagraphs">recently translated paragraphs (key: number of the original source paragraph, value: translated text)</param>
    /// <returns></returns>
    public delegate bool TranslationProcessCancelStatus(Dictionary<int, string> targetParagraphs);

    public abstract class AbstractTranslationProcessor<TTranslationBaseUnit> : ITranslationProcessor where TTranslationBaseUnit : ITranslationBaseUnit
    {
        /**
         * due to translation service constraints not all paragraphs can't submitted at once. Therefore the paragraphs must be split in multiple Chunks
         */
        private class TranslationChunk
        {
            public readonly List<TTranslationBaseUnit> TranslationUnits = new List<TTranslationBaseUnit>();

            public int TextSize => TranslationUnits.ConvertAll(e => Utilities.UrlEncode(e.Text).Length).Sum();

            public int ArrayLength => TranslationUnits.Count;
        }

        protected abstract IEnumerable<TTranslationBaseUnit> ConstructTranslationBaseUnits(List<Paragraph> sourceParagraphs);

        //TODO: Should not use "Number"... should use "Index" or "Id" or similar instead
        protected abstract Dictionary<int, string> GetTargetParagraphs(List<TTranslationBaseUnit> sourceTranslationUnits, List<string> targetTexts);

        protected abstract string GetName();

        public string TranslatedName { get; set; }

        public override string ToString()
        {
            return TranslatedName ?? GetName();
        }

        public List<string> Translate(ITranslationService translationService, string sourceLanguageIsoCode, string targetLanguageIsoCode, List<Paragraph> sourceParagraphs, TranslationProcessCancelStatus processCancelStatus)
        {
            //remove empty paragraphs
            sourceParagraphs = sourceParagraphs.Where(p => !string.IsNullOrWhiteSpace(p.Text)).ToList();

            var translationBaseUnits = ConstructTranslationBaseUnits(sourceParagraphs);
            var translationChunks = BuildTranslationChunks(translationBaseUnits, translationService);

            var targetParagraphs = new Dictionary<int, string>();

            foreach (var translationChunk in translationChunks)
            {
                var result = translationService.Translate(sourceLanguageIsoCode, targetLanguageIsoCode, translationChunk.TranslationUnits.ConvertAll(x => new Paragraph() { Text = x.Text }));
                var newTargetParagraphs = GetTargetParagraphs(translationChunk.TranslationUnits, result);
                foreach (var newTargetParagraph in newTargetParagraphs)
                {
                    targetParagraphs[newTargetParagraph.Key] = newTargetParagraph.Value;
                }
                if (processCancelStatus != null && processCancelStatus(newTargetParagraphs)) //check if operation was canceled outside
                {
                    return targetParagraphs.Values.ToList();
                }
            }
            return targetParagraphs.Values.ToList();
        }

        public abstract List<string> GetSupportedLanguages();

        private IEnumerable<TranslationChunk> BuildTranslationChunks(IEnumerable<TTranslationBaseUnit> translationUnits, ITranslationService translationService)
        {
            int maxTextSize = translationService.GetMaxTextSize();
            int maximumRequestArrayLength = translationService.GetMaximumRequestArraySize();
            var currentChunk = new TranslationChunk();

            foreach (var translationUnit in translationUnits)
            {
                if (currentChunk.TextSize + Utilities.UrlEncode(translationUnit.Text).Length > maxTextSize
                    || currentChunk.ArrayLength + 1 > maximumRequestArrayLength)
                {
                    yield return currentChunk;
                    currentChunk = new TranslationChunk();
                }
                currentChunk.TranslationUnits.Add(translationUnit);
            }
            if (currentChunk.ArrayLength > 0)
            {
                yield return currentChunk;
            }
        }
    }
}
