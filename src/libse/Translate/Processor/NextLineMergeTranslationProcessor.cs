using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate.Processor
{
    /// <summary>
    ///  allows a maximum of 2 consecutive lines to be merged.
    /// </summary>
    public class NextLineMergeTranslationProcessor : AbstractTranslationProcessor<NextLineMergeTranslationProcessor.NextLineMerging>
    {
        const int TimeThresholdBetweenTwoParagraphs = 200;

        public class NextLineMerging : ITranslationBaseUnit
        {
            public string Text { get; }

            /// <summary>
            /// Number of the first (if two lines have been merged) or only paragraph
            /// </summary>
            public int OngoingSourceParagraphNumber { get; }

            /// <summary>
            /// true, if this translation unit merged 2 paragraph lines
            /// </summary>
            public bool SkipNext { get; }

            public NextLineMerging(string mergedText, int ongoingSourceParagraphNumber, bool skipNext)
            {
                Text = mergedText;
                OngoingSourceParagraphNumber = ongoingSourceParagraphNumber;
                SkipNext = skipNext;
            }
        }

        protected override IEnumerable<NextLineMerging> ConstructTranslationBaseUnits(List<Paragraph> sourceParagraphs)
        {
            bool skipNext = false;
            for (var index = 0; index < sourceParagraphs.Count; index++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                var currentParagraph = sourceParagraphs[index];
                var currentParagraphText = currentParagraph.Text;
                var nextParagraphText = string.Empty;
                if (index < sourceParagraphs.Count - 1)
                {
                    var nextParagraph = sourceParagraphs[index + 1];
                    if (IsDirectSuccessor(currentParagraph, nextParagraph) && IsChronologicallyClose(nextParagraph, currentParagraph))
                    {
                        nextParagraphText = nextParagraph.Text;
                    }
                }

                string baseUnitText = currentParagraphText;
                if (Configuration.Settings.Tools.TranslateAllowSplit &&
                    !string.IsNullOrEmpty(nextParagraphText) && !string.IsNullOrEmpty(currentParagraphText) &&
                    (char.IsLetterOrDigit(currentParagraphText[currentParagraphText.Length - 1]) || currentParagraphText[currentParagraphText.Length - 1] == ',' || currentParagraphText[currentParagraphText.Length - 1] == '\u060C') && //  \u060C = arabic comma
                    char.IsLower(nextParagraphText[0]) &&
                    !currentParagraphText.Contains('-') && !nextParagraphText.Contains('-'))
                {
                    baseUnitText = currentParagraphText + " " + nextParagraphText;
                    skipNext = true;
                }

                yield return new NextLineMerging(baseUnitText, currentParagraph.Number, skipNext);
            }
        }

        private static bool IsChronologicallyClose(Paragraph nextParagraph, Paragraph currentParagraph)
        {
            return nextParagraph.StartTime.TotalMilliseconds - currentParagraph.EndTime.TotalMilliseconds < TimeThresholdBetweenTwoParagraphs;
        }

        private static bool IsDirectSuccessor(Paragraph currentParagraph, Paragraph nextParagraph)
        {
            return currentParagraph.Number == nextParagraph.Number - 1;
        }

        protected override Dictionary<int, string> GetTargetParagraphs(List<NextLineMerging> sourceTranslationUnits, List<string> targetTexts)
        {
            Dictionary<int, string> targetParagraphs = new Dictionary<int, string>();

            for (int i = 0; i < sourceTranslationUnits.Count; i++)
            {
                var sourceTranslationUnit = sourceTranslationUnits[i];
                var targetText = targetTexts[i].Trim();

                string currentText = targetText;
                string nextText = null;
                if (sourceTranslationUnit.SkipNext)
                {
                    var lines = Utilities.AutoBreakLine(targetText).SplitToLines();
                    if (lines.Count == 1)
                    {
                        nextText = string.Empty;
                    }
                    else if (lines.Count == 2)
                    {
                        currentText = Utilities.AutoBreakLine(lines[0]);
                        nextText = Utilities.AutoBreakLine(lines[1]);
                    }
                    else
                    {
                        currentText = Utilities.AutoBreakLine(lines[0] + " " + lines[1]);
                        var sb = new StringBuilder();
                        for (int j = 2; j < lines.Count; j++)
                        {
                            sb.Append(lines[j]);
                            sb.Append(" ");
                        }
                        nextText = Utilities.AutoBreakLine(sb.ToString().TrimEnd());
                    }
                }

                targetParagraphs[sourceTranslationUnit.OngoingSourceParagraphNumber] = currentText;

                if (nextText != null)
                {
                    targetParagraphs[sourceTranslationUnit.OngoingSourceParagraphNumber + 1] = nextText;
                }
            }
            return targetParagraphs;
        }

        public override List<string> GetSupportedLanguages()
        {
            return null;
        }

        protected override string GetName()
        {
            return "Next Line Merging";
        }
    }
}
