using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public class MergeManager : IMergeManager
    {
        public enum BreakMode
        {
            AutoBreak,
            Normal,
            Unbreak,
            UnbreakNoSpace,
            KeepBreaks
        }

        /// <summary>
        /// True when merged lines should keep the end time of the last merged line even if it
        /// overlaps the following subtitle, based on the merge settings and the current format.
        /// </summary>
        public static bool ShouldKeepEndTime(SubtitleFormat? currentFormat)
        {
            if (!Se.Settings.Tools.MergeKeepEndTime)
            {
                return false;
            }

            return !Se.Settings.Tools.MergeKeepEndTimeOnlyAssa || currentFormat is AdvancedSubStationAlpha;
        }

        public Subtitle MergeSelectedLines(Subtitle inputSubtitle, int[] selectedIndices, BreakMode breakMode = BreakMode.Normal, bool keepEndTime = false)
        {
            if (inputSubtitle.Paragraphs.Count <= 0 || selectedIndices.Length <= 1)
            {
                return inputSubtitle;
            }

            var subtitle = new Subtitle(inputSubtitle, false);
            var sb = new StringBuilder();
            var deleteIndices = new List<int>();
            var first = true;
            var firstIndex = 0;
            double endMilliseconds = 0;
            var next = 0;

            // Auto-detection reads the whole file (two subtitle copies, then ~30 word-count passes
            // over the joined text), so it must run once per merge - not once per merged line.
            // Merging the lines only edits trailing continuation marks, which cannot change a
            // whole-file language verdict.
            string? language = null;
            string DetectLanguage() => language ?? (language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle));

            foreach (var index in selectedIndices)
            {
                if (first)
                {
                    firstIndex = index;
                    next = index + 1;
                    first = !first;
                }
                else
                {
                    deleteIndices.Add(index);
                    if (next != index)
                    {
                        return subtitle; 
                    }

                    next++;
                }

                var continuationStyle = Configuration.Settings.General.ContinuationStyle;
                if (continuationStyle != ContinuationStyle.None)
                {
                    var continuationProfile = ContinuationUtilities.GetContinuationProfile(continuationStyle);
                    if (next < firstIndex + selectedIndices.Length)
                    {
                        var mergeResult = ContinuationUtilities.MergeHelper(subtitle.Paragraphs[index].Text, subtitle.Paragraphs[index + 1].Text, continuationProfile, DetectLanguage());
                        subtitle.Paragraphs[index].Text = mergeResult.Item1;
                        subtitle.Paragraphs[index + 1].Text = mergeResult.Item2;
                    }
                }
                var addText = subtitle.Paragraphs[index].Text;

                if (firstIndex != index)
                {
                    // addText = RemoveAssStartAlignmentTag(addText);
                }

                if (breakMode == BreakMode.UnbreakNoSpace)
                {
                    sb.Append(addText);
                }
                else
                {
                    sb.AppendLine(addText);
                }

                // Max, not last: with "keep end time" the merged line must span every merged
                // line, and selected lines are not necessarily ordered by end time (e.g. an
                // ASSA sign event that outlives the dialog line merged into it).
                endMilliseconds = Math.Max(endMilliseconds, subtitle.Paragraphs[index].EndTime.TotalMilliseconds);
            }

            var currentParagraph = subtitle.Paragraphs[firstIndex];
            var text = sb.ToString().TrimEnd();
            text = HtmlUtil.FixInvalidItalicTags(text);
            //text = FixAssaTagsAfterMerge(text);
            //text = ChangeAllLinesTagsToSingleTag(text, "i");
            //text = ChangeAllLinesTagsToSingleTag(text, "b");
            //text = ChangeAllLinesTagsToSingleTag(text, "u");
            if (breakMode == BreakMode.Unbreak)
            {
                text = Utilities.UnbreakLine(text);
            }
            else if (breakMode == BreakMode.UnbreakNoSpace)
            {
                text = text.Replace(" " + Environment.NewLine + " ", string.Empty)
                    .Replace(Environment.NewLine + " ", string.Empty)
                    .Replace(" " + Environment.NewLine, string.Empty)
                    .Replace(Environment.NewLine, string.Empty);
            }
            else
            {
                text = Utilities.AutoBreakLine(text, DetectLanguage());
            }

            currentParagraph.Text = text;

            //display time
            currentParagraph.EndTime.TotalMilliseconds = endMilliseconds;

            var nextParagraph = subtitle.GetParagraphOrDefault(next);
            if (!keepEndTime && nextParagraph != null && currentParagraph.EndTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && currentParagraph.StartTime.TotalMilliseconds < nextParagraph.StartTime.TotalMilliseconds)
            {
                currentParagraph.EndTime.TotalMilliseconds = nextParagraph.StartTime.TotalMilliseconds - 1;
            }

            for (var i = deleteIndices.Count - 1; i >= 0; i--)
            {
                subtitle.Paragraphs.RemoveAt(deleteIndices[i]);
            }

            subtitle.Renumber();
            return subtitle;
        }

        public void MergeSelectedLines(ObservableCollection<SubtitleLineViewModel> inputSubtitle, List<SubtitleLineViewModel> selectedItems, BreakMode breakMode = BreakMode.Normal, bool keepEndTime = false)
        {
            if (inputSubtitle.Count <= 0 || selectedItems.Count <= 1)
            {
                return;
            }

           // var subtitle = new Subtitle(inputSubtitle, false);
            var sb = new StringBuilder();
            var sbOriginal = new StringBuilder();
            var deleteIndices = new List<int>();
            var first = true;
            var firstIndex = 0;
            double endMilliseconds = 0;
            var next = 0;

            // Auto-detection copies the whole grid into a Subtitle and runs ~30 word-count passes
            // over the joined text, so it must run once per merge - not once per merged line (and
            // not three more times for the auto-break calls below). Merging only edits trailing
            // continuation marks, which cannot change a whole-file language verdict.
            string? language = null;
            string DetectLanguage() => language ?? (language = inputSubtitle.AutoDetectGoogleLanguage());

            foreach (var selectedItem in selectedItems)
            {
                var index = inputSubtitle.IndexOf(selectedItem);
                if (first)
                {
                    firstIndex = index;
                    next = firstIndex + 1;
                    first = !first;
                }
                else
                {
                    deleteIndices.Add(index);
                    if (next != index)
                    {
                        return;
                    }

                    next++;
                }

                var continuationStyle = Configuration.Settings.General.ContinuationStyle;
                if (continuationStyle != ContinuationStyle.None)
                {
                    var continuationProfile = ContinuationUtilities.GetContinuationProfile(continuationStyle);
                    if (next < firstIndex + selectedItems.Count)
                    {
                        var mergeResult = ContinuationUtilities.MergeHelper(inputSubtitle[index].Text, inputSubtitle[index + 1].Text, continuationProfile, DetectLanguage());
                        inputSubtitle[index].Text = mergeResult.Item1;
                        inputSubtitle[index + 1].Text = mergeResult.Item2;
                    }
                }
                var addText = inputSubtitle[index].Text;

                if (firstIndex != index)
                {
                    // addText = RemoveAssStartAlignmentTag(addText);
                }

                if (breakMode == BreakMode.UnbreakNoSpace)
                {
                    sb.Append(addText);
                    sbOriginal.Append(inputSubtitle[index].OriginalText);
                }
                else
                {
                    sb.AppendLine(addText);
                    sbOriginal.AppendLine(inputSubtitle[index].OriginalText);
                }

                // Max, not last: with "keep end time" the merged line must span every merged
                // line, and selected lines are not necessarily ordered by end time (e.g. an
                // ASSA sign event that outlives the dialog line merged into it).
                endMilliseconds = Math.Max(endMilliseconds, inputSubtitle[index].EndTime.TotalMilliseconds);
            }

            var currentParagraph = inputSubtitle[firstIndex];
            var text = sb.ToString().TrimEnd();
            text = HtmlUtil.FixInvalidItalicTags(text);
            //text = FixAssaTagsAfterMerge(text);
            //text = ChangeAllLinesTagsToSingleTag(text, "i");
            //text = ChangeAllLinesTagsToSingleTag(text, "b");
            //text = ChangeAllLinesTagsToSingleTag(text, "u");
            if (breakMode == BreakMode.Unbreak)
            {
                text = Utilities.UnbreakLine(text);
            }
            else if (breakMode == BreakMode.UnbreakNoSpace)
            {
                text = text.Replace(" " + Environment.NewLine + " ", string.Empty)
                    .Replace(Environment.NewLine + " ", string.Empty)
                    .Replace(" " + Environment.NewLine, string.Empty)
                    .Replace(Environment.NewLine, string.Empty);
            }
            else if (breakMode != BreakMode.KeepBreaks)
            {
                text = Utilities.AutoBreakLine(text, DetectLanguage());
            }

            currentParagraph.Text = text;

            var originalText = sbOriginal.ToString().TrimEnd();
            if (!string.IsNullOrEmpty(originalText))
            {
                originalText = HtmlUtil.FixInvalidItalicTags(originalText);
                if (breakMode == BreakMode.Unbreak)
                {
                    originalText = Utilities.UnbreakLine(originalText);
                }
                else if (breakMode == BreakMode.UnbreakNoSpace)
                {
                    originalText = originalText.Replace(" " + Environment.NewLine + " ", string.Empty)
                        .Replace(Environment.NewLine + " ", string.Empty)
                        .Replace(" " + Environment.NewLine, string.Empty)
                        .Replace(Environment.NewLine, string.Empty);
                }
                else if (breakMode != BreakMode.KeepBreaks)
                {
                    originalText = Utilities.AutoBreakLine(originalText, DetectLanguage());
                }

                currentParagraph.OriginalText = originalText;
            }

            //display time
            currentParagraph.EndTime = TimeSpan.FromMilliseconds(endMilliseconds);

            var nextParagraph = inputSubtitle.GetOrNull(next);
            if (!keepEndTime && nextParagraph != null && currentParagraph.EndTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && currentParagraph.StartTime.TotalMilliseconds < nextParagraph.StartTime.TotalMilliseconds)
            {
                currentParagraph.EndTime = TimeSpan.FromMilliseconds(nextParagraph.StartTime.TotalMilliseconds - 1);
            }

            for (var i = deleteIndices.Count - 1; i >= 0; i--)
            {
                inputSubtitle.RemoveAt(deleteIndices[i]);
            }

            inputSubtitle.Renumber();
        }

        public void MergeSelectedLinesAsDialog(ObservableCollection<SubtitleLineViewModel> subtitles, List<SubtitleLineViewModel> selectedItems)
        {
            if (selectedItems.Count != 2)
            {
                return;
            }

            var currentParagraph = selectedItems[0];
            var currentText = Utilities.UnbreakLine(currentParagraph.Text);
            var currentOriginalText = Utilities.UnbreakLine(currentParagraph.OriginalText);

            var nextParagraph = selectedItems[1];
            var nextText = Utilities.UnbreakLine(nextParagraph.Text);
            var nextOriginalText = Utilities.UnbreakLine(nextParagraph.OriginalText);

            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(subtitles.Select(p=>new Paragraph(p.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)));
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var dialogHelper = new DialogSplitMerge
            {
                DialogStyle = Enum.Parse<DialogType>(Se.Settings.General.DialogStyle),
                TwoLetterLanguageCode = language,
                SkipLineEndingCheck = true, // user explicitly asked for a dialog merge
            };
            var dialogText = dialogHelper.FixDashesAndSpaces("- " + currentText.TrimStart(' ', '-') + Environment.NewLine + "- " + nextText.TrimStart(' ', '-'));
            currentParagraph.Text = dialogText;

            if (!string.IsNullOrWhiteSpace(currentOriginalText) || !string.IsNullOrWhiteSpace(nextOriginalText))
            {
                var dialogOriginalText = dialogHelper.FixDashesAndSpaces("- " + currentOriginalText.TrimStart(' ', '-')
                                                                              + Environment.NewLine + "- "
                                                                              + nextOriginalText.TrimStart(' ', '-'));
                currentParagraph.OriginalText = dialogOriginalText;
            }

            currentParagraph.EndTime = TimeSpan.FromMilliseconds(nextParagraph.EndTime.TotalMilliseconds);

            subtitles.Remove(nextParagraph);
            subtitles.Renumber();
        }
    }
}
