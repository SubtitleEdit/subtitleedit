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

    public interface ITranslationUnit
    {
        string GetText();

    }

    public interface ILineHandler
    {
        IEnumerable<ITranslationUnit> wrap(List<Paragraph> sourceParagraphs);
        List<string> unwrap(List<ITranslationUnit> sourceUnits ,List<string> targetTexts);
    }

    public class ParagraphWrapper
    {
        private Paragraph _paragraph;
        public int Index { get;  }
        public List<SentenceParagraphRelation> SentenceParagraphRelations { get; } = new List<SentenceParagraphRelation>();

        public ParagraphWrapper(Paragraph paragraph, int index)
        {
            this._paragraph = paragraph;
            this.Index = index;
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

    public class Sentence : ITranslationUnit
    {
        public List<SentenceParagraphRelation> SentenceParagraphs = new List<SentenceParagraphRelation>();

        public string GetText()
        {
            string text = string.Join(" ", SentenceParagraphs.ConvertAll(e => e.Text));
            text = Regex.Replace(text, @"\s+", " "); //replace new lines and multiple spaces to a single space
            text = text.Trim();
            return text;
        }

        public void SetTranslation(string translatedText)
        {
            var delimiterChars = new char[] { ' ', ' ' };
            List<int> chunksTextLength = SentenceParagraphs.ConvertAll(x => x.Text.Length);
            int overallSourceLength = Enumerable.Sum(chunksTextLength);

            int currentSentenceParagraph = 0;
            SentenceParagraphs[0].Translation = "";
            int currentSourceChunkEndPosition = chunksTextLength[0];

            for (int i = 0; i < translatedText.Length; i++)
            {
                char charAt = translatedText[i];
                if (delimiterChars.Contains(charAt))
                {
                    double currentTargetPositionPercentage = (double)i / translatedText.Length;
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

    public class SentenceMergingTranslator : Translator<Sentence>
    {


        private static IEnumerable<Sentence> ConcatSentences(List<Paragraph> paragraphs)
        {
            var delimiterChars = new char[] { '.', '?', '!' };
            
            List<Sentence> sentences = new List<Sentence>();
            Sentence currentSentence = new Sentence();

            for (var index = 0; index < paragraphs.Count; index++)
            {
                Paragraph paragraph = paragraphs[index];

                var paragraphWrapper = new ParagraphWrapper(paragraph, index);
                var text = paragraph.Text;

                List<int> splitPositions = FindAllPositionsOf(text, delimiterChars);
                splitPositions = splitPositions.Select(i => i + 1).ToList(); //set the cut point after the delimiter to ensurer that the delimiter is always the end of a chunk
                List<string> sentenceChunks = SplitAt(text, splitPositions).ToList();
                sentenceChunks.RemoveAll(x => x.Trim().Length == 0); //remove empty chunks

                foreach (var sentenceChunk in sentenceChunks)
                {
                    currentSentence.SentenceParagraphs.Add(new SentenceParagraphRelation(sentenceChunk, paragraphWrapper));
                    if (sentenceChunk.IndexOfAny(delimiterChars) >=0)
                    {
                        sentences.Add(currentSentence);
                        currentSentence = new Sentence();
                    }
                }
            }
            if (currentSentence.GetText().Trim().Length > 0)
            {
                sentences.Add(currentSentence);
            }



            foreach (var sentence in sentences)
            {
                yield return sentence;
            }
        }

        private static List<int> FindAllPositionsOf(string text, char[] searchChars)
        {
            var foundIndexes = new List<int>();
            for (int i = 0; i < text.Length; i++)
            {
                if (searchChars.Contains(text[i]))
                {
                    foundIndexes.Add(i);
                }
            }
            return foundIndexes;
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

        protected override IEnumerable<Sentence> ConstructTranslationUnits(List<Paragraph> sourceParagraphs)
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
                    targetParagraphs.Remove(paragraphWrapper.Index);
                    targetParagraphs.Add(paragraphWrapper.Index, paragraphWrapper.GenerateTargetText());
                }
            }

            return targetParagraphs;
        }



        public SentenceMergingTranslator(ITranslationService translationService) : base(translationService)
        {
        }
    }
}
