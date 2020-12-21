using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{

    public class SentenceMergingTranslationProcessor : AbstractTranslationProcessor<SentenceMergingTranslationProcessor.Sentence>
    {

        private static readonly char[] SentenceDelimiterChars = { '.', '?', '!' };
        private readonly StringSplitEngine _stringSplitEngine;

        public SentenceMergingTranslationProcessor()
        {
            _stringSplitEngine = new StringSplitEngine(SentenceDelimiterChars);
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
                    text = text.Trim();
                    return text;
                }
            }


     

            /**
             * divides the full sentence into multiple chunks and assigns them to the source paragraphs.
             * It tries to split by percentage equivalent to fit the length of the source paragraph
             */
            public void SetTranslation(string targetText)
            {

                var delimiterChars = new char[] { ' ', ' ' };
                var stringSplitEngine = new StringSplitEngine(delimiterChars);
                var inseparablePositions = stringSplitEngine.EvaluateInseparablePositions(targetText);

                List<int> chunksTextLength = SentenceParagraphs.ConvertAll(x => x.Text.Length);
                int overallSourceLength = Enumerable.Sum(chunksTextLength);

                int currentSentenceParagraph = 0;
                SentenceParagraphs[0].Translation = "";
                int currentSourceChunkEndPosition = chunksTextLength[0];

                for (int i = 0; i < targetText.Length; i++)
                {
                    char charAt = targetText[i];
                    if (delimiterChars.Contains(charAt) && !inseparablePositions.Contains(i))
                    {
                        double currentTargetPositionPercentage = (double)i / targetText.Length;
                        double currentSourceChunkEndPositionPercentage = (double)currentSourceChunkEndPosition / overallSourceLength;
                        if (currentTargetPositionPercentage > currentSourceChunkEndPositionPercentage)
                        {
                            currentSentenceParagraph++;
                            SentenceParagraphs[currentSentenceParagraph].Translation = "";
                            currentSourceChunkEndPosition += chunksTextLength[currentSentenceParagraph];
                        }
                    }

                    SentenceParagraphs[currentSentenceParagraph].Translation += charAt;
                }
            }
            ///**
            // * divides the full sentence into multiple chunks and assigns them to the source paragraphs.
            // * It tries to split by percentage equivalent to fit the length of the source paragraph
            // */
            //public void SetTranslation(string targetText)
            //{
            //    var delimiterChars = new char[] { ' ', ' ' };
            //    List<int> chunksTextLength = SentenceParagraphs.ConvertAll(x => x.Text.Length);
            //    int overallSourceLength = Enumerable.Sum(chunksTextLength);

            //    int currentSentenceParagraph = 0;
            //    SentenceParagraphs[0].Translation = "";
            //    int currentSourceChunkEndPosition = chunksTextLength[0];

            //    for (int i = 0; i < targetText.Length; i++)
            //    {
            //        char charAt = targetText[i];
            //        if (delimiterChars.Contains(charAt))
            //        {
            //            double currentTargetPositionPercentage = (double)i / targetText.Length;
            //            double currentSourceChunkEndPositionPercentage = (double)currentSourceChunkEndPosition / overallSourceLength;
            //            if (currentTargetPositionPercentage > currentSourceChunkEndPositionPercentage)
            //            {
            //                currentSentenceParagraph++;
            //                SentenceParagraphs[currentSentenceParagraph].Translation = "";
            //                currentSourceChunkEndPosition += chunksTextLength[currentSentenceParagraph];
            //            }
            //        }

            //        SentenceParagraphs[currentSentenceParagraph].Translation += charAt;
            //    }
            //}
        }

        public override string ToString()
        {
            return "Sentence Merging";
        }

        private IEnumerable<Sentence> ConcatSentences(List<Paragraph> paragraphs)
        {
            Sentence currentSentence = new Sentence();
            int lastParagraphNumber = Int16.MinValue;

            foreach (var paragraph in paragraphs)
            {
                if (lastParagraphNumber + 1 != paragraph.Number) //check if paragraphs belong sequentially together 
                {
                    if (currentSentence.Text.Trim().Length > 0) //this check avoid to add empty Sentence
                    {
                        yield return currentSentence;
                        currentSentence = new Sentence();
                    }
                }

                lastParagraphNumber = paragraph.Number;
                var paragraphWrapper = new ParagraphWrapper(paragraph);
                var text = paragraph.Text;

                List<string> sentenceChunks = _stringSplitEngine.split(text);
                foreach (var sentenceChunk in sentenceChunks)
                {
                    currentSentence.SentenceParagraphs.Add(new SentenceParagraphRelation(sentenceChunk, paragraphWrapper));
                    if (sentenceChunk.IndexOfAny(SentenceDelimiterChars) >= 0)
                    {
                        yield return currentSentence;
                        currentSentence = new Sentence();
                    }
                }
            }
            if (currentSentence.Text.Trim().Length > 0) //this check avoid a empty last Sentence (could happen when the last chunk ends with a delimiter)
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
