using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public static partial class MergeAndSplitHelper
    {
        public static bool MergeSplitProblems { get; set; }

        public static async Task<int> MergeAndTranslateIfPossible(Subtitle sourceSubtitle, Subtitle targetSubtitle, TranslationPair source, TranslationPair target, int index, IAutoTranslator autoTranslator, bool forceSingleLineMode, CancellationToken cancellationToken)
        {
            if (forceSingleLineMode)
            {
                return 0;
            }

            var noSentenceEndingSource = IsNonMergeLanguage(source);
            var noSentenceEndingTarget = IsNonMergeLanguage(target);

            // Try to handle (remove and save info for later restore) italics, bold, alignment, and more where possible
            var tempSubtitle = new Subtitle(sourceSubtitle);
            var formattingList = HandleFormatting(tempSubtitle, index, target.Code);

            if (Configuration.Settings.Tools.AutoTranslateMaxBytes <= 0)
            {
                Configuration.Settings.Tools.AutoTranslateMaxBytes = new ToolsSettings().AutoTranslateMaxBytes;
            }
            var maxChars = Math.Min(autoTranslator.MaxCharacters, Configuration.Settings.Tools.AutoTranslateMaxBytes);

            if (MergeSplitProblems)
            {
                MergeSplitProblems = false;
                if (maxChars > 500)
                {
                    maxChars /= 2;
                }
            }

            // Merge text for better translation and save info enough to split again later
            var mergeResult = MergeMultipleLines(tempSubtitle, index, maxChars, noSentenceEndingSource, noSentenceEndingTarget);
            var mergeCount = mergeResult.ParagraphCount;
            var text = mergeResult.Text;

            if (mergeResult.HasError)
            {
                MergeSplitProblems = true;

                if (!noSentenceEndingSource)
                {
                    noSentenceEndingSource = true;
                    mergeResult = MergeMultipleLines(tempSubtitle, index, maxChars, noSentenceEndingSource, noSentenceEndingTarget);
                    mergeCount = mergeResult.ParagraphCount;
                    text = mergeResult.Text;
                }

                if (mergeResult.HasError)
                {
                    return 0;
                }
            }

            var linesTranslate = 0;
            var mergedTranslation = await autoTranslator.Translate(text, source.Code, target.Code, cancellationToken);


            // Split by line ending chars where period count matches
            var splitResult = SplitMultipleLines(mergeResult, mergedTranslation, target.Code);
            if (splitResult.Count == mergeCount && HasSameEmptyLines(splitResult, tempSubtitle, index) &&
                Utilities.CountTagInText(text, '.') == Utilities.CountTagInText(mergedTranslation, '.'))
            {
                var idx = 0;
                foreach (var line in splitResult)
                {
                    var reformattedText = formattingList[idx].ReAddFormatting(line);
                    targetSubtitle.Paragraphs[index].Text = reformattedText;
                    index++;
                    linesTranslate++;
                    idx++;
                }

                return linesTranslate;
            }


            // Split per number of lines
            var translatedLines = mergedTranslation.SplitToLines();
            if (translatedLines.Count == mergeResult.Text.SplitToLines().Count)
            {
                var newSub = new Subtitle();
                for (var i = 0; i < mergeResult.ParagraphCount; i++)
                {
                    newSub.Paragraphs.Add(new Paragraph());
                }

                var translatedLinesIdx = 0;
                var paragraphIdx = 0;
                foreach (var mergeItem in mergeResult.MergeResultItems)
                {
                    var numberOfParagraphs = mergeItem.EndIndex - mergeItem.StartIndex + 1;
                    var numberOfLines = mergeItem.Text.SplitToLines().Count;

                    var sb = new StringBuilder();
                    for (var j = 0; j < numberOfLines && translatedLinesIdx < translatedLines.Count; j++)
                    {
                        sb.AppendLine(translatedLines[translatedLinesIdx]);
                        translatedLinesIdx++;
                    }

                    var translatedText = sb.ToString().Trim();

                    var arr = TextSplit.SplitMulti(translatedText, numberOfParagraphs, target.TwoLetterIsoLanguageName);
                    for (var i = 0; i < arr.Count && paragraphIdx < newSub.Paragraphs.Count; i++)
                    {
                        var res = arr[i];
                        if (res.Contains('\n') ||
                            res.TrimEnd('.').Contains('.') ||
                            res.Length >= Configuration.Settings.General.SubtitleLineMaximumLength)
                        {
                            res = Utilities.AutoBreakLine(arr[i], Configuration.Settings.General.SubtitleLineMaximumLength * 2, Configuration.Settings.General.MergeLinesShorterThan, target.TwoLetterIsoLanguageName);
                        }

                        newSub.Paragraphs[paragraphIdx].Text = res;
                        paragraphIdx++;
                    }
                }

                if (paragraphIdx == mergeCount && HasSameEmptyLines(newSub, sourceSubtitle, index))
                {
                    var idx = 0;
                    foreach (var p in newSub.Paragraphs)
                    {
                        var reformattedText = formattingList[idx].ReAddFormatting(p.Text);
                        targetSubtitle.Paragraphs[index].Text = reformattedText;
                        index++;
                        linesTranslate++;
                        idx++;
                    }

                    return linesTranslate;
                }
            }

            // Split by line ending chars - periods in numbers removed
            var noPeriodsInNumbersTranslation = FixPeriodInNumbers(mergedTranslation);
            splitResult = SplitMultipleLines(mergeResult, noPeriodsInNumbersTranslation, target.Code);
            if (splitResult.Count == mergeCount && HasSameEmptyLines(splitResult, tempSubtitle, index) &&
                Utilities.CountTagInText(text, '.') == Utilities.CountTagInText(noPeriodsInNumbersTranslation, '.'))
            {
                var idx = 0;
                foreach (var line in splitResult)
                {
                    var reformattedText = formattingList[idx].ReAddFormatting(line.Replace('¤', '.'));
                    targetSubtitle.Paragraphs[index].Text = reformattedText;
                    index++;
                    linesTranslate++;
                    idx++;
                }

                return linesTranslate;
            }


            // Split by line ending chars
            splitResult = SplitMultipleLines(mergeResult, mergedTranslation, target.Code);
            if (splitResult.Count == mergeCount && HasSameEmptyLines(splitResult, tempSubtitle, index))
            {
                var idx = 0;
                foreach (var line in splitResult)
                {
                    var reformattedText = formattingList[idx].ReAddFormatting(line);
                    targetSubtitle.Paragraphs[index].Text = reformattedText;
                    index++;
                    linesTranslate++;
                    idx++;
                }

                return linesTranslate;
            }


            MergeSplitProblems = true;

            return linesTranslate;
        }

        private static string FixPeriodInNumbers(string mergedTranslation)
        {
            var findCommaNumber = new Regex(@"\d\.\d");
            var s = mergedTranslation;
            while (true)
            {
                var match = findCommaNumber.Match(s);
                if (match.Success)
                {
                    s = s.Remove(match.Index + 1, 1);
                    s = s.Insert(match.Index + 1, "¤");
                }
                else
                {
                    return s;
                }
            }
        }

        private static bool HasSameEmptyLines(Subtitle newSub, Subtitle sourceSubtitle, int index)
        {
            for (var i = 0; i < newSub.Paragraphs.Count; i++)
            {
                var inputBlank = string.IsNullOrWhiteSpace(sourceSubtitle.Paragraphs[i + index].Text);
                var outputBlank = string.IsNullOrWhiteSpace(newSub.Paragraphs[i].Text);
                if (inputBlank || outputBlank && (inputBlank != outputBlank))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSameEmptyLines(List<string> splitResult, Subtitle tempSubtitle, int index)
        {
            for (var i = 0; i < splitResult.Count; i++)
            {
                var inputBlank = string.IsNullOrWhiteSpace(tempSubtitle.Paragraphs[i + index].Text);
                var outputBlank = string.IsNullOrWhiteSpace(splitResult[i]);
                if (inputBlank || outputBlank && (inputBlank != outputBlank))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<Formatting> HandleFormatting(Subtitle sourceSubtitle, int index, string sourceLanguage)
        {
            var formattingList = new List<Formatting>();

            for (var i = index; i < sourceSubtitle.Paragraphs.Count; i++)
            {
                var p = sourceSubtitle.Paragraphs[i];
                var f = new Formatting();
                var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
                p.Text = text;
                formattingList.Add(f);
            }

            return formattingList;
        }

        public static MergeResult MergeMultipleLines(Subtitle sourceSubtitle, int index, int maxTextSize, bool noSentenceEndingSource, bool noSentenceEndingTarget)
        {
            var result = new MergeResult
            {
                MergeResultItems = new List<MergeResultItem>(),
                NoSentenceEndingSource = noSentenceEndingSource,
                NoSentenceEndingTarget = noSentenceEndingTarget,
            };

            var item = new MergeResultItem
            {
                StartIndex = index,
                EndIndex = index,
                Text = string.Empty,
                Paragraphs = new List<Paragraph>(),
            };

            result.Text = sourceSubtitle.Paragraphs[index].Text;
            item.Paragraphs.Add(sourceSubtitle.Paragraphs[index]);
            item.Text = sourceSubtitle.Paragraphs[index].Text;
            if (string.IsNullOrWhiteSpace(result.Text))
            {
                item.IsEmpty = true;
                item.Text = string.Empty;
                result.MergeResultItems.Add(item);
                item = null;
                result.Text = string.Empty;
            }

            var textBuild = new StringBuilder(result.Text);
            var prev = sourceSubtitle.Paragraphs[index];

            for (var i = index + 1; i < sourceSubtitle.Paragraphs.Count; i++)
            {
                var p = sourceSubtitle.Paragraphs[i];

                if (item != null && Utilities.UrlEncodeLength(result.Text + Environment.NewLine + p.Text) > maxTextSize)
                {
                    break;
                }

                if (noSentenceEndingSource)
                {

                    if (item != null)
                    {
                        item.TextIndexEnd = result.Text.Length;
                    }

                    result.Text += Environment.NewLine + "." + Environment.NewLine + p.Text;

                    if (item != null)
                    {
                        item.Text += Environment.NewLine + "." + Environment.NewLine + p.Text;
                        item.Paragraphs.Add(p);

                        item.TextIndexStart = result.Text.Length;
                        item.StartIndex = i - 1;
                        item.EndIndex = i - 1;
                        result.MergeResultItems.Add(item);

                        textBuild = new StringBuilder();
                        item = new MergeResultItem { StartIndex = i, Text = string.Empty, Paragraphs = new List<Paragraph>() };
                    }
                }
                else if (string.IsNullOrWhiteSpace(p.Text))
                {
                    var endChar = result.Text[result.Text.Length - 1];
                    if (item != null)
                    {
                        item.TextIndexStart = result.Text.Length;
                        item.TextIndexEnd = result.Text.Length;
                        item.EndChar = endChar;
                        var endCharOccurrences = Utilities.CountTagInText(textBuild.ToString(), endChar);
                        item.EndCharOccurrences = endCharOccurrences;
                        result.MergeResultItems.Add(item);
                    }

                    result.MergeResultItems.Add(new MergeResultItem
                    {
                        StartIndex = index,
                        EndIndex = index,
                        IsEmpty = true,
                        TextIndexStart = result.Text.Length,
                        TextIndexEnd = result.Text.Length,
                        Text = string.Empty,
                        Paragraphs = new List<Paragraph>(),
                    });

                    item = null;
                    textBuild = new StringBuilder();

                }
                else if (result.Text.HasSentenceEnding() || string.IsNullOrWhiteSpace(result.Text))
                {

                    if (string.IsNullOrWhiteSpace(result.Text))
                    {
                        if (item != null)
                        {
                            item.TextIndexEnd = result.Text.Length;
                            result.MergeResultItems.Add(item);
                            textBuild = new StringBuilder();
                        }
                    }
                    else
                    {
                        var endChar = result.Text[result.Text.Length - 1];

                        if (item != null)
                        {
                            item.EndChar = endChar;
                            var endCharOccurrences = Utilities.CountTagInText(textBuild.ToString(), endChar);
                            item.EndCharOccurrences = endCharOccurrences;
                            item.TextIndexEnd = result.Text.Length;
                            result.MergeResultItems.Add(item);
                            textBuild = new StringBuilder();
                        }
                    }

                    textBuild.Append(p.Text);

                    result.Text += Environment.NewLine + p.Text;

                    item = new MergeResultItem { StartIndex = i, TextIndexStart = result.Text.Length, Text = p.Text, Paragraphs = new List<Paragraph>(), };
                }
                else if (item != null && (item.Continuous || item.StartIndex == item.EndIndex) && p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds < 1000)
                {
                    textBuild.Append(" ");
                    textBuild.Append(p.Text);
                    result.Text += " " + p.Text;
                    item.Text += " " + p.Text;
                    item.Continuous = true;
                    item.Paragraphs.Add(p);
                }
                else
                {
                    result.HasError = true;
                    break; // weird continuation
                }

                if (item != null)
                {
                    item.EndIndex = i;
                    item.TextIndexEnd = result.Text.Length;
                }

                prev = p;
            }

            if (item != null)
            {
                //TODO: skip last item if it does not has sentence ending (or skip early at sentence ending...)

                result.MergeResultItems.Add(item);

                if (result.Text.Length > 0 && result.Text.HasSentenceEnding())
                {
                    var endChar = result.Text[result.Text.Length - 1];
                    item.EndChar = endChar;
                    item.EndCharOccurrences = Utilities.CountTagInText(textBuild.ToString(), endChar);
                }
            }

            result.Text = result.Text.Trim();
            result.ParagraphCount = result.MergeResultItems.Sum(p => p.EndIndex - p.StartIndex + 1);

            foreach (var i in result.MergeResultItems)
            {
                i.Text = i.Text.Trim();
            }

            return result;
        }

        public static List<string> SplitMultipleLines(MergeResult mergeResult, string translatedText, string language)
        {
            var lines = new List<string>();
            var text = translatedText;

            if (mergeResult.NoSentenceEndingSource)
            {
                var sb = new StringBuilder();
                var translatedLines = text.SplitToLines();
                foreach (var translatedLine in translatedLines)
                {
                    var s = translatedLine.Trim();
                    if (s == ".")
                    {
                        lines.Add(sb.ToString().Trim());
                        sb.Clear();
                        continue;
                    }

                    sb.AppendLine(s);
                }

                if (sb.Length > 0)
                {
                    lines.Add(sb.ToString().Trim());
                }

                return lines;
            }

            foreach (var item in mergeResult.MergeResultItems)
            {
                if (item.IsEmpty)
                {
                    lines.Add(string.Empty);
                }
                else if (item.Continuous)
                {
                    var part = GetPartFromItem(text, item);
                    text = text.Remove(0, part.Length).Trim();
                    var lineRange = SplitContinuous(part, item, language);
                    lines.AddRange(lineRange);
                }
                else
                {
                    var part = GetPartFromItem(text, item);
                    text = text.Remove(0, part.Length).Trim();
                    lines.Add(Utilities.AutoBreakLine(part));
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                return new List<string>();
            }

            return lines;
        }

        private static string GetPartFromItem(string input, MergeResultItem item)
        {
            if (item.EndChar == '\0')
            {
                return input;
            }

            var idx = input.IndexOf(item.EndChar);
            if (idx < 0)
            {
                return string.Empty;
            }

            var count = 1;
            while (idx >= 0 && idx < input.Length - 1)
            {
                if (count == item.EndCharOccurrences)
                {
                    var s = input.Substring(0, idx + 1);
                    return s;
                }

                idx = input.IndexOf(item.EndChar, idx + 1);
                count++;
            }

            return input;
        }

        private static List<string> SplitContinuous(string text, MergeResultItem item, string language)
        {
            var count = item.EndIndex - item.StartIndex + 1;

            if (count == 2)
            {
                var arr = Utilities.AutoBreakLine(text, Configuration.Settings.General.SubtitleLineMaximumLength * 2, 0, language).SplitToLines();
                if (arr.Count == 2 && item.Paragraphs.Count == 2)
                {
                    // test using character count for percentage
                    var totalCharLength = item.Paragraphs[0].Text.Length + item.Paragraphs[1].Text.Length;
                    var pctCharLength1 = item.Paragraphs[0].Text.Length * 100.0 / totalCharLength;
                    var pctCharLength2 = item.Paragraphs[1].Text.Length * 100.0 / totalCharLength;
                    var pctCharArr = GetTwoPartsByPct(text, pctCharLength1, pctCharLength2);

                    // test using duration for percentage
                    var totalDurationLength = item.Paragraphs[0].DurationTotalMilliseconds + item.Paragraphs[1].DurationTotalMilliseconds + 1;
                    var pctDurationLength1 = item.Paragraphs[0].DurationTotalMilliseconds * 100.0 / totalDurationLength;
                    var pctDurationLength2 = item.Paragraphs[1].DurationTotalMilliseconds * 100.0 / totalDurationLength;
                    var pctDurationArr = GetTwoPartsByPct(text, pctDurationLength1, pctDurationLength2);


                    // use best match of the three arrays considering line separator, adherence to chars/sec

                    // same result for char split + duration split
                    if (pctCharArr[0].Length > 0 && pctCharArr[0] == pctDurationArr[0])
                    {
                        return pctCharArr.ToList();
                    }

                    // check max chars
                    var cps1 = Utilities.GetCharactersPerSecond(new Paragraph(arr[0], item.Paragraphs[0].StartTime.TotalMilliseconds, item.Paragraphs[0].EndTime.TotalMilliseconds));
                    var cps2 = Utilities.GetCharactersPerSecond(new Paragraph(arr[1], item.Paragraphs[1].StartTime.TotalMilliseconds, item.Paragraphs[1].EndTime.TotalMilliseconds));

                    var cpsChar1 = Utilities.GetCharactersPerSecond(new Paragraph(pctCharArr[0], item.Paragraphs[0].StartTime.TotalMilliseconds, item.Paragraphs[0].EndTime.TotalMilliseconds));
                    var cpsChar2 = Utilities.GetCharactersPerSecond(new Paragraph(pctCharArr[1], item.Paragraphs[1].StartTime.TotalMilliseconds, item.Paragraphs[1].EndTime.TotalMilliseconds));

                    var cpsDuration1 = Utilities.GetCharactersPerSecond(new Paragraph(pctDurationArr[0], item.Paragraphs[0].StartTime.TotalMilliseconds, item.Paragraphs[0].EndTime.TotalMilliseconds));
                    var cpsDuration2 = Utilities.GetCharactersPerSecond(new Paragraph(pctDurationArr[1], item.Paragraphs[1].StartTime.TotalMilliseconds, item.Paragraphs[1].EndTime.TotalMilliseconds));

                    if (pctCharArr[0].Length > 0 && pctCharArr[0].EndsWith(',') &&
                        cpsChar1 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds &&
                        cpsChar2 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        return pctCharArr.ToList();
                    }

                    if (pctDurationArr[0].Length > 0 && pctDurationArr[0].EndsWith(',') &&
                        cpsDuration1 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds &&
                        cpsDuration2 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        return pctDurationArr.ToList();
                    }

                    if (pctCharArr[0].Length > 0 &&
                        cpsChar1 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds &&
                        cpsChar2 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        return pctCharArr.ToList();
                    }

                    if (pctDurationArr[0].Length > 0 &&
                        cpsDuration1 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds &&
                        cpsDuration2 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        return pctDurationArr.ToList();
                    }

                    if (arr[0].Length > 0 &&
                        cps1 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds &&
                        cps2 < Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                    {
                        return arr.ToList();
                    }

                    if (pctDurationArr[0].Length > 0)
                    {
                        return pctDurationArr.ToList();
                    }
                }

                return arr;
            }

            return TextSplit.SplitMulti(text, count, language);
        }

        private static string[] GetTwoPartsByPct(string text, double pctCharLength1, double pctCharLength2)
        {
            var idx = (int)Math.Round(text.Length * pctCharLength1 / 100.0, MidpointRounding.AwayFromZero);
            for (var i = 0; i < idx; i++)
            {
                var j = idx - i;
                if (j > 1 && text[j] == ' ')
                {
                    return new[] { text.Substring(0, j).Trim(), text.Substring(j + 1).Trim() };
                }

                var k = idx + i;
                if (k < text.Length - 1 && text[k] == ' ')
                {
                    return new[] { text.Substring(0, k).Trim(), text.Substring(k + 1).Trim() };
                }
            }

            return new[] { string.Empty, string.Empty };
        }

        private static bool IsNonMergeLanguage(TranslationPair language)
        {
            var code = language.TwoLetterIsoLanguageName ?? language.Code;

            return code.ToLowerInvariant() == "zh" ||
                   code.ToLowerInvariant() == "zh-CN" ||
                   code.ToLowerInvariant() == "zh-TW" ||
                   code.ToLowerInvariant() == "yue_Hant" ||
                   code.ToLowerInvariant() == "zho_Hans" ||
                   code.ToLowerInvariant() == "zho_Hant" ||
                   code.ToLowerInvariant() == "jpn_Jpan" ||
                   code.ToLowerInvariant() == "ja";
        }
    }
}
