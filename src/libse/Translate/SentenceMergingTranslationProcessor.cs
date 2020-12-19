using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.Translate
{

    public class SentenceMergingTranslationProcessor : AbstractTranslationProcessor<SentenceMergingTranslationProcessor.Sentence>
    {

        private static readonly char[] SentenceDelimiterChars = { '.', '?', '!' };

        public class ParagraphWrapper
        {
            public Paragraph Paragraph { get; }
            public List<SentenceParagraphRelation> SentenceParagraphRelations { get; } = new List<SentenceParagraphRelation>();

            public ParagraphWrapper(Paragraph paragraph)
            {
                this.Paragraph = paragraph;
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
                this.ParagraphWrapper = paragraphWrapper;
                this.Text = text;
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
                List<int> chunksTextLength = SentenceParagraphs.ConvertAll(x => x.Text.Length);
                int overallSourceLength = Enumerable.Sum(chunksTextLength);

                int currentSentenceParagraph = 0;
                SentenceParagraphs[0].Translation = "";
                int currentSourceChunkEndPosition = chunksTextLength[0];

                for (int i = 0; i < targetText.Length; i++)
                {
                    char charAt = targetText[i];
                    if (delimiterChars.Contains(charAt))
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
        }

        public override string ToString()
        {
            return "Sentence Merging";
        }

        private static IEnumerable<Sentence> ConcatSentences(List<Paragraph> paragraphs)
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

                var splitPositions = new List<int>(FindAllPositionsOf(text, SentenceDelimiterChars));
                splitPositions = splitPositions.Select(i => i + 1).ToList(); //set the cut point after the delimiter to ensurer that the delimiter is always the end of a chunk
                List<string> sentenceChunks = SplitAt(text, splitPositions).ToList();
                sentenceChunks.RemoveAll(x => x.Trim().Length == 0); //remove empty chunks

                foreach (var sentenceChunk in sentenceChunks)
                {
                    currentSentence.SentenceParagraphs.Add(new SentenceParagraphRelation(sentenceChunk, paragraphWrapper));
                    if (sentenceChunk.IndexOfAny(SentenceDelimiterChars) >=0)
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

        private static IEnumerable<int> FindAllPositionsOf(string text, char[] searchChars)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (searchChars.Contains(text[i]))
                {
                    yield return i;
                }
            }
        }

        public static string[] SplitAt(string source, List<int> positions)
        {

            string[] output = new string[positions.Count + 1];
            int pos = 0;

            for (int i = 0; i < positions.Count; pos = positions[i++])
                output[i] = source.Substring(pos, positions[i] - pos);

            output[positions.Count] = source.Substring(pos);
            return output;
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
                    targetParagraphs[paragraphWrapper.]= paragraphWrapper.GenerateTargetText();
                }
            }

            return targetParagraphs;
        }

    }
}
