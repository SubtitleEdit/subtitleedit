using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
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

        public Subtitle MergeSelectedLines(Subtitle inputSubtitle, int[] selectedIndices, BreakMode breakMode = BreakMode.Normal)
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
                        var mergeResult = ContinuationUtilities.MergeHelper(subtitle.Paragraphs[index].Text, subtitle.Paragraphs[index + 1].Text, continuationProfile, LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle));
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

                endMilliseconds = subtitle.Paragraphs[index].EndTime.TotalMilliseconds;
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
                text = Utilities.AutoBreakLine(text, LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle));
            }

            currentParagraph.Text = text;

            //display time
            currentParagraph.EndTime.TotalMilliseconds = endMilliseconds;

            var nextParagraph = subtitle.GetParagraphOrDefault(next);
            if (nextParagraph != null && currentParagraph.EndTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && currentParagraph.StartTime.TotalMilliseconds < nextParagraph.StartTime.TotalMilliseconds)
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

        public void MergeSelectedLines(ObservableCollection<SubtitleLineViewModel> inputSubtitle, List<SubtitleLineViewModel> selectedItems, BreakMode breakMode = BreakMode.Normal)
        {
            if (inputSubtitle.Count <= 0 || selectedItems.Count <= 1)
            {
                return;
            }

           // var subtitle = new Subtitle(inputSubtitle, false);
            var sb = new StringBuilder();
            var deleteIndices = new List<int>();
            var first = true;
            var firstIndex = 0;
            double endMilliseconds = 0;
            var next = 0;
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
                        var mergeResult = ContinuationUtilities.MergeHelper(inputSubtitle[index].Text, inputSubtitle[index + 1].Text, continuationProfile, inputSubtitle.AutoDetectGoogleLanguage());
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
                }
                else
                {
                    sb.AppendLine(addText);
                }

                endMilliseconds = inputSubtitle[index].EndTime.TotalMilliseconds;
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
            else if (breakMode == BreakMode.KeepBreaks)
            {
                text = Utilities.AutoBreakLine(text, inputSubtitle.AutoDetectGoogleLanguage());
            }

            currentParagraph.Text = text;

            //display time
            currentParagraph.EndTime = TimeSpan.FromMilliseconds(endMilliseconds);

            var nextParagraph = inputSubtitle.GetOrNull(next);
            if (nextParagraph != null && currentParagraph.EndTime.TotalMilliseconds > nextParagraph.StartTime.TotalMilliseconds && currentParagraph.StartTime.TotalMilliseconds < nextParagraph.StartTime.TotalMilliseconds)
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

            var nextParagraph = selectedItems[1];
            var nextText = Utilities.UnbreakLine(nextParagraph.Text);

            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(subtitles.Select(p=>new Paragraph(p.Text, p.StartTime.TotalMilliseconds, p.EndTime.TotalMilliseconds)));
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var dialogHelper = new DialogSplitMerge { DialogStyle = Enum.Parse<DialogType>(Se.Settings.General.DialogStyle), TwoLetterLanguageCode = language };
            var dialogText = dialogHelper.FixDashes("- " + currentText.TrimStart(' ', '-') + Environment.NewLine + "- " + nextText.TrimStart(' ', '-'));
            currentParagraph.Text = dialogText;

            currentParagraph.EndTime = TimeSpan.FromMilliseconds(nextParagraph.EndTime.TotalMilliseconds);

            subtitles.Remove(nextParagraph);
            subtitles.Renumber();
        }
    }
}
