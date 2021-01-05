using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{

    public class SentenceMergingTranslationProcessor : AbstractTranslationProcessor<SentenceMergingTranslationProcessor.Sentence>
    {
        private static readonly char[] SentenceDelimiterAfterChars = { '.', '?', '!', ')' };
        private static readonly char[] SentenceDelimiterBeforeChars = { '(' };

        private static readonly char[] WordDelimiterAfterChars = { ' ', ' ' };

        private static readonly StringSplitEngine SentenceSplitEngine = new StringSplitEngine(SentenceDelimiterAfterChars, SentenceDelimiterBeforeChars);
        private static readonly List<AbstractStringSplitsChunkAssigner> StringSplitsChunkAssigners = new List<AbstractStringSplitsChunkAssigner>();

        static SentenceMergingTranslationProcessor()
        {
            var wordSplitEngine = new StringSplitEngine(WordDelimiterAfterChars);
            StringSplitsChunkAssigners.Add(new GreedyStringSplitsChunkAssigner(wordSplitEngine));
            StringSplitsChunkAssigners.Add(new RandomizedStringSplitsChunkAssigner(wordSplitEngine));
        }

        public class ParagraphWrapper
        {
            public Paragraph Paragraph { get; }
            public List<SentenceParagraphRelation> SentenceParagraphRelations { get; } = new List<SentenceParagraphRelation>();

            public ParagraphWrapper(Paragraph paragraph)
            {
                Paragraph = paragraph;
            }

            public string GenerateTargetText()
            {
                return string.Join(" ", SentenceParagraphRelations.ConvertAll(x => x.Translation));
            }
        }

        public class SentenceParagraphRelation
        {
            public string Text { get; }
            public string Translation { get; set; } = "";
            public ParagraphWrapper ParagraphWrapper { get; }

            public SentenceParagraphRelation(string text, ParagraphWrapper paragraphWrapper)
            {
                paragraphWrapper.SentenceParagraphRelations.Add(this);
                ParagraphWrapper = paragraphWrapper;
                Text = text;
            }
        }

        public class Sentence : ITranslationBaseUnit
        {
            public List<SentenceParagraphRelation> SentenceParagraphs = new List<SentenceParagraphRelation>();

            public string Text
            {
                get
                {
                    string text = string.Join(" ", SentenceParagraphs.ConvertAll(e => e.Text));
                    text = Regex.Replace(text, @"\s+", " "); //replace new lines and multiple spaces to a single space
                    return text.Trim();
                }
            }

            /**
             * divides the full sentence into multiple chunks and assigns them to the source paragraphs.
             * It tries to split by percentage equivalent to fit the length of the source paragraph
             */
            public void SetTranslation(string targetText)
            {
                List<int> sourceChunksTextLength = SentenceParagraphs.ConvertAll(x => x.Text.Length);
                var breakPositions = EvaluateBreakPositions(targetText, sourceChunksTextLength);

                string[] targetChunks = StringSplitEngine.SplitAt(targetText, breakPositions);
                for (int i = 0; i < targetChunks.Length; i++)
                {
                    SentenceParagraphs[i].Translation = targetChunks[i];
                }
            }

            private int[] EvaluateBreakPositions(string targetText, List<int> sourceChunksTextLength)
            {
                if (sourceChunksTextLength.Count == 1) //only 1 source chunk, no breakup required
                {
                    return new int[0];
                }
                List<AbstractStringSplitsChunkAssigner.RateResult> rateResults = new List<AbstractStringSplitsChunkAssigner.RateResult>();
                foreach (var stringSplitsChunkAssigner in StringSplitsChunkAssigners)
                {
                    var potentialBreakPositions = stringSplitsChunkAssigner.GetSplitPositions(sourceChunksTextLength, targetText);
                    var rateResult = AbstractStringSplitsChunkAssigner.CalculateRateResult(sourceChunksTextLength, targetText.Length, potentialBreakPositions);
                    rateResults.Add(rateResult);
                }
                rateResults = rateResults.OrderBy(x => x.Fitness).ToList();
                return rateResults.First().BreakPositions;
            }
        }

        protected override string GetName()
        {
            return "Sentence Merging";
        }

        private IEnumerable<Sentence> ConcatSentences(List<Paragraph> paragraphs)
        {
            Sentence currentSentence = new Sentence();
            int lastParagraphNumber = int.MinValue;

            foreach (var paragraph in paragraphs)
            {
                if (lastParagraphNumber + 1 != paragraph.Number) //check if paragraphs belong sequentially together 
                {
                    if (currentSentence.Text.Length > 0) //this check avoids to add empty Sentence
                    {
                        yield return currentSentence;
                        currentSentence = new Sentence();
                    }
                }

                lastParagraphNumber = paragraph.Number;
                var paragraphWrapper = new ParagraphWrapper(paragraph);

                var sentenceChunks = SentenceSplitEngine.Split(paragraph.Text);
                foreach (var sentenceChunk in sentenceChunks)
                {
                    if (currentSentence.Text.Length > 0 && SentenceDelimiterBeforeChars.Contains(sentenceChunk[0]))
                    {
                        yield return currentSentence;
                        currentSentence = new Sentence();
                    }
                    currentSentence.SentenceParagraphs.Add(new SentenceParagraphRelation(sentenceChunk, paragraphWrapper));
                    if (SentenceDelimiterAfterChars.Contains(sentenceChunk[sentenceChunk.Length - 1]))
                    {
                        yield return currentSentence;
                        currentSentence = new Sentence();
                    }
                }
            }
            if (currentSentence.Text.Length > 0) //this check avoid a empty last Sentence (could happen when the last chunk ends with a delimiter)
            {
                yield return currentSentence;
            }
        }

        protected override IEnumerable<Sentence> ConstructTranslationBaseUnits(List<Paragraph> sourceParagraphs)
        {
            return ConcatSentences(sourceParagraphs).ToList();
        }

        protected override Dictionary<int, string> GetTargetParagraphs(List<Sentence> sourceTranslationUnits, List<string> targetSentenceTexts)
        {
            for (int i = 0; i < sourceTranslationUnits.Count; i++)
            {
                var targetUnitText = targetSentenceTexts[i];
                sourceTranslationUnits[i].SetTranslation(targetUnitText);
            }

            Dictionary<int, string> targetParagraphs = new Dictionary<int, string>();
            foreach (Sentence sourceSentence in sourceTranslationUnits)
            {
                foreach (var sentenceParagraphRelation in sourceSentence.SentenceParagraphs)
                {
                    var paragraphWrapper = sentenceParagraphRelation.ParagraphWrapper;
                    targetParagraphs[paragraphWrapper.Paragraph.Number] = paragraphWrapper.GenerateTargetText();
                }
            }

            return targetParagraphs;
        }

        public override List<string> GetSupportedLanguages()
        {
            return Formatting.LanguagesAllowingLineMerging;
        }
    }
}
