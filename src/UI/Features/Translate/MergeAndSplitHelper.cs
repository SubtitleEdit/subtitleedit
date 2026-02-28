using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public static partial class MergeAndSplitHelper
{
    private const int MinCharsForHalving = 500;
    private const int MaxGapBetweenContinuousLinesMs = 1000;
    private const char PeriodPlaceholder = '¤';

    public static bool MergeSplitProblems { get; set; }

    public static async Task<int> MergeAndTranslateIfPossible(
        ObservableCollection<TranslateRow> rows,
        TranslationPair source,
        TranslationPair target,
        int index,
        IAutoTranslator autoTranslator,
        bool forceSingleLineMode,
        CancellationToken cancellationToken)
    {
        var noSentenceEndingSource = IsNonMergeLanguage(source);
        var noSentenceEndingTarget = IsNonMergeLanguage(target);

        var tempSubtitle = CreateTempSubtitle(rows);
        var formattingList = HandleFormatting(tempSubtitle, index, target.Code);
        var maxChars = CalculateMaxChars(autoTranslator, forceSingleLineMode);

        var mergeResult = TryMergeLines(tempSubtitle, index, maxChars, ref noSentenceEndingSource, noSentenceEndingTarget);
        if (mergeResult.HasError)
        {
            return 0;
        }

        var mergedTranslation = await autoTranslator.Translate(mergeResult.Text, source.Code, target.Code, cancellationToken);

        if (forceSingleLineMode || mergeResult.ParagraphCount == 1)
        {
            return ApplySingleLineTranslation(rows, index, formattingList, mergedTranslation);
        }

        return TrySplitStrategies(rows, target, index, tempSubtitle, formattingList, mergeResult, mergedTranslation);
    }

    private static TranslateRow[] CreateTempSubtitle(ObservableCollection<TranslateRow> rows)
    {
        return rows.Select(p => new TranslateRow
        {
            Number = p.Number,
            Show = p.Show,
            Duration = p.Duration,
            Text = p.Text
        }).ToArray();
    }

    private static int CalculateMaxChars(IAutoTranslator autoTranslator, bool forceSingleLineMode)
    {
        if (Configuration.Settings.Tools.AutoTranslateMaxBytes <= 0)
        {
            Configuration.Settings.Tools.AutoTranslateMaxBytes = new ToolsSettings().AutoTranslateMaxBytes;
        }

        var maxChars = Math.Min(autoTranslator.MaxCharacters, Configuration.Settings.Tools.AutoTranslateMaxBytes);

        if (forceSingleLineMode)
        {
            return 0;
        }

        if (MergeSplitProblems)
        {
            MergeSplitProblems = false;
            if (maxChars > MinCharsForHalving)
            {
                maxChars /= 2;
            }
        }

        return maxChars;
    }

    private static MergeResult TryMergeLines(
        TranslateRow[] tempSubtitle,
        int index,
        int maxChars,
        ref bool noSentenceEndingSource,
        bool noSentenceEndingTarget)
    {
        var mergeResult = MergeMultipleLines(tempSubtitle, index, maxChars, noSentenceEndingSource, noSentenceEndingTarget);

        if (mergeResult.HasError)
        {
            MergeSplitProblems = true;

            if (!noSentenceEndingSource)
            {
                noSentenceEndingSource = true;
                mergeResult = MergeMultipleLines(tempSubtitle, index, maxChars, noSentenceEndingSource, noSentenceEndingTarget);
            }
        }

        return mergeResult;
    }

    private static int ApplySingleLineTranslation(
        ObservableCollection<TranslateRow> rows,
        int index,
        List<Formatting> formattingList,
        string mergedTranslation)
    {
        if (index < rows.Count && formattingList.Count > 0)
        {
            rows[index].TranslatedText = formattingList[0].ReAddFormatting(mergedTranslation);
            return 1;
        }
        return 0;
    }

    private static int TrySplitStrategies(
        ObservableCollection<TranslateRow> rows,
        TranslationPair target,
        int index,
        TranslateRow[] tempSubtitle,
        List<Formatting> formattingList,
        MergeResult mergeResult,
        string mergedTranslation)
    {
        var sourceTexts = tempSubtitle.Select(p => p.Text).ToList();
        var mergeCount = mergeResult.ParagraphCount;

        // Strategy 1: Split by line ending chars where period count matches
        var splitResult = SplitMultipleLines(mergeResult, mergedTranslation, target.Code);
        if (IsSplitValid(splitResult, mergeCount, sourceTexts, index) &&
            HasMatchingPeriodCount(mergeResult.Text, mergedTranslation))
        {
            return ApplySplitResult(rows, index, formattingList, splitResult);
        }

        // Strategy 2: Split per number of lines
        var lineCountResult = TrySplitByLineCount(rows, target, index, formattingList, mergeResult, mergedTranslation, splitResult);
        if (lineCountResult > 0)
        {
            return lineCountResult;
        }

        // Strategy 3: Split with periods in numbers normalized
        var noPeriodsInNumbersTranslation = FixPeriodInNumbers(mergedTranslation);
        splitResult = SplitMultipleLines(mergeResult, noPeriodsInNumbersTranslation, target.Code);
        if (IsSplitValid(splitResult, mergeCount, sourceTexts, index) &&
            HasMatchingPeriodCount(mergeResult.Text, noPeriodsInNumbersTranslation))
        {
            return ApplySplitResult(rows, index, formattingList, splitResult, restorePeriodPlaceholder: true);
        }

        // Strategy 4: Split by line ending chars (relaxed - no period count check)
        splitResult = SplitMultipleLines(mergeResult, mergedTranslation, target.Code);
        if (IsSplitValid(splitResult, mergeCount, sourceTexts, index))
        {
            return ApplySplitResult(rows, index, formattingList, splitResult);
        }

        MergeSplitProblems = true;
        return 0;
    }

    private static bool IsSplitValid(List<string> splitResult, int expectedCount, List<string> sourceTexts, int index)
    {
        return splitResult.Count == expectedCount && HasSameEmptyLines(splitResult, sourceTexts, index);
    }

    private static bool HasMatchingPeriodCount(string source, string translation)
    {
        return Utilities.CountTagInText(source, '.') == Utilities.CountTagInText(translation, '.');
    }

    private static int ApplySplitResult(
        ObservableCollection<TranslateRow> rows,
        int index,
        List<Formatting> formattingList,
        List<string> splitResult,
        bool restorePeriodPlaceholder = false)
    {
        var linesTranslated = 0;
        var idx = 0;

        foreach (var line in splitResult)
        {
            if (index >= rows.Count || idx >= formattingList.Count)
            {
                break;
            }

            var text = restorePeriodPlaceholder ? line.Replace(PeriodPlaceholder, '.') : line;
            rows[index].TranslatedText = formattingList[idx].ReAddFormatting(text);
            index++;
            linesTranslated++;
            idx++;
        }

        return linesTranslated;
    }

    private static int TrySplitByLineCount(
        ObservableCollection<TranslateRow> rows,
        TranslationPair target,
        int index,
        List<Formatting> formattingList,
        MergeResult mergeResult,
        string mergedTranslation,
        List<string> splitResult)
    {
        var translatedLines = mergedTranslation.SplitToLines();
        if (translatedLines.Count != mergeResult.Text.SplitToLines().Count)
        {
            return 0;
        }

        var translatedLinesIdx = 0;
        var currentRowIndex = index;

        foreach (var mergeItem in mergeResult.MergeResultItems)
        {
            var numberOfParagraphs = mergeItem.EndIndex - mergeItem.StartIndex + 1;
            var numberOfLines = mergeItem.Text.SplitToLines().Count;

            var translatedText = BuildTranslatedText(translatedLines, ref translatedLinesIdx, numberOfLines);
            var splitParts = TextSplit.SplitMulti(translatedText, numberOfParagraphs, target.TwoLetterIsoLanguageName);

            for (var i = 0; i < splitParts.Count && currentRowIndex < rows.Count; i++)
            {
                rows[currentRowIndex].TranslatedText = AutoBreakIfNeeded(splitParts[i], target.TwoLetterIsoLanguageName);
                currentRowIndex++;
            }
        }

        var totalProcessed = currentRowIndex - index;
        if (totalProcessed == mergeResult.ParagraphCount &&
            HasSameEmptyLines(rows.Skip(index).Take(totalProcessed).Select(p => p.TranslatedText).ToList(),
                              rows.Skip(index).Take(totalProcessed).Select(p => p.Text).ToList(), 0))
        {
            return ApplyFormattingToExistingTranslations(rows, index, formattingList, mergeResult.ParagraphCount);
        }

        return 0;
    }

    private static string BuildTranslatedText(List<string> translatedLines, ref int translatedLinesIdx, int numberOfLines)
    {
        var sb = new StringBuilder();
        for (var j = 0; j < numberOfLines && translatedLinesIdx < translatedLines.Count; j++)
        {
            sb.AppendLine(translatedLines[translatedLinesIdx]);
            translatedLinesIdx++;
        }
        return sb.ToString().Trim();
    }

    private static string AutoBreakIfNeeded(string text, string language)
    {
        if (text.Contains('\n') ||
            text.TrimEnd('.').Contains('.') ||
            text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength)
        {
            return Utilities.AutoBreakLine(
                text,
                Configuration.Settings.General.SubtitleLineMaximumLength * 2,
                Configuration.Settings.General.MergeLinesShorterThan,
                language);
        }
        return text;
    }

    private static int ApplyFormattingToExistingTranslations(
        ObservableCollection<TranslateRow> rows,
        int index,
        List<Formatting> formattingList,
        int count)
    {
        var linesTranslated = 0;

        for (var i = 0; i < count; i++)
        {
            if (i >= formattingList.Count || index >= rows.Count)
            {
                break;
            }

            rows[index].TranslatedText = formattingList[i].ReAddFormatting(rows[index].TranslatedText);
            index++;
            linesTranslated++;
        }

        return linesTranslated;
    }

    private static string FixPeriodInNumbers(string mergedTranslation)
    {
        var findCommaNumber = FindPeriodInNumber();
        var s = mergedTranslation;
        while (true)
        {
            var match = findCommaNumber.Match(s);
            if (match.Success)
            {
                s = s.Remove(match.Index + 1, 1);
                s = s.Insert(match.Index + 1, PeriodPlaceholder.ToString());
            }
            else
            {
                return s;
            }
        }
    }

    [GeneratedRegex(@"\d\.\d")]
    private static partial Regex FindPeriodInNumber();

    private static bool HasSameEmptyLines(List<string> newTexts, List<string> sourceSubtitle, int index)
    {
        for (var i = 0; i < newTexts.Count && i + index < sourceSubtitle.Count; i++)
        {
            var inputBlank = string.IsNullOrWhiteSpace(sourceSubtitle[i + index]);
            var outputBlank = string.IsNullOrWhiteSpace(newTexts[i]);
            if (inputBlank != outputBlank)
            {
                return false;
            }
        }

        return true;
    }

    private static List<Formatting> HandleFormatting(TranslateRow[] rows, int index, string sourceLanguage)
    {
        var formattingList = new List<Formatting>();

        for (var i = index; i < rows.Length; i++)
        {
            var p = rows[i];
            var f = new Formatting();
            var text = f.SetTagsAndReturnTrimmed(TranslationHelper.PreTranslate(p.Text, sourceLanguage), sourceLanguage);
            p.Text = text;
            formattingList.Add(f);
        }

        return formattingList;
    }

    public static MergeResult MergeMultipleLines(TranslateRow[] sourceSubtitle, int index, int maxTextSize, bool noSentenceEndingSource, bool noSentenceEndingTarget)
    {
        var result = new MergeResult
        {
            MergeResultItems = new List<MergeResultItem>(),
            NoSentenceEndingSource = noSentenceEndingSource,
            NoSentenceEndingTarget = noSentenceEndingTarget,
        };

        var context = new MergeContext(sourceSubtitle, index);

        // Initialize with the first subtitle line
        InitializeFirstItem(result, context);

        // Process remaining lines
        for (var i = index + 1; i < sourceSubtitle.Length; i++)
        {
            var currentRow = sourceSubtitle[i];

            if (ExceedsMaxSize(result, context, currentRow, maxTextSize))
            {
                break;
            }

            var continueProcessing = ProcessRow(result, context, currentRow, i, noSentenceEndingSource);
            if (!continueProcessing)
            {
                break;
            }
        }

        FinalizeResult(result, context);
        return result;
    }

    private sealed class MergeContext
    {
        public MergeResultItem? CurrentItem { get; set; }
        public StringBuilder TextBuilder { get; set; }
        public TranslateRow PreviousRow { get; set; }
        public int StartIndex { get; }

        public MergeContext(TranslateRow[] sourceSubtitle, int index)
        {
            StartIndex = index;
            PreviousRow = sourceSubtitle[index];
            TextBuilder = new StringBuilder();
        }
    }

    private static void InitializeFirstItem(MergeResult result, MergeContext context)
    {
        var firstRow = context.PreviousRow;
        context.CurrentItem = new MergeResultItem
        {
            StartIndex = context.StartIndex,
            EndIndex = context.StartIndex,
            Text = string.Empty,
            Paragraphs = new List<TranslateRow> { firstRow },
        };

        result.Text = firstRow.Text;
        context.CurrentItem.Text = firstRow.Text;

        if (string.IsNullOrWhiteSpace(result.Text))
        {
            context.CurrentItem.IsEmpty = true;
            context.CurrentItem.Text = string.Empty;
            result.MergeResultItems.Add(context.CurrentItem);
            context.CurrentItem = null;
            result.Text = string.Empty;
        }
        else
        {
            context.TextBuilder = new StringBuilder(result.Text);
        }
    }

    private static bool ExceedsMaxSize(MergeResult result, MergeContext context, TranslateRow currentRow, int maxTextSize)
    {
        return context.CurrentItem != null &&
               Utilities.UrlEncodeLength(result.Text + Environment.NewLine + currentRow.Text) > maxTextSize;
    }

    private static bool ProcessRow(MergeResult result, MergeContext context, TranslateRow currentRow, int rowIndex, bool noSentenceEndingSource)
    {
        if (noSentenceEndingSource)
        {
            ProcessNoSentenceEndingRow(result, context, currentRow, rowIndex);
        }
        else if (string.IsNullOrWhiteSpace(currentRow.Text))
        {
            ProcessEmptyRow(result, context, rowIndex);
        }
        else if (result.Text.HasSentenceEnding() || string.IsNullOrWhiteSpace(result.Text))
        {
            ProcessSentenceEndingRow(result, context, currentRow, rowIndex);
        }
        else if (IsContinuousWithPrevious(context, currentRow))
        {
            ProcessContinuousRow(result, context, currentRow, rowIndex);
        }
        else
        {
            result.HasError = true;
            return false; // Cannot process - weird continuation
        }

        context.PreviousRow = currentRow;
        return true;
    }

    private static void ProcessNoSentenceEndingRow(MergeResult result, MergeContext context, TranslateRow currentRow, int rowIndex)
    {
        if (context.CurrentItem == null)
        {
            return;
        }

        context.CurrentItem.TextIndexEnd = result.Text.Length;
        result.Text += Environment.NewLine + "." + Environment.NewLine + currentRow.Text;
        context.CurrentItem.Text += Environment.NewLine + "." + Environment.NewLine + currentRow.Text;
        context.CurrentItem.Paragraphs.Add(currentRow);
        context.CurrentItem.TextIndexStart = result.Text.Length;
        context.CurrentItem.StartIndex = rowIndex - 1;
        context.CurrentItem.EndIndex = rowIndex - 1;

        result.MergeResultItems.Add(context.CurrentItem);

        context.TextBuilder = new StringBuilder(currentRow.Text);
        context.CurrentItem = new MergeResultItem
        {
            StartIndex = rowIndex,
            EndIndex = rowIndex,
            Text = currentRow.Text,
            Paragraphs = new List<TranslateRow> { currentRow }
        };
    }

    private static void ProcessEmptyRow(MergeResult result, MergeContext context, int rowIndex)
    {
        if (context.CurrentItem != null && result.Text.Length > 0)
        {
            var endChar = result.Text[^1];
            context.CurrentItem.TextIndexStart = result.Text.Length;
            context.CurrentItem.TextIndexEnd = result.Text.Length;
            context.CurrentItem.EndChar = endChar;
            context.CurrentItem.EndCharOccurrences = Utilities.CountTagInText(context.TextBuilder.ToString(), endChar);
            result.MergeResultItems.Add(context.CurrentItem);
        }

        result.MergeResultItems.Add(new MergeResultItem
        {
            StartIndex = context.StartIndex,
            EndIndex = context.StartIndex,
            IsEmpty = true,
            TextIndexStart = result.Text.Length,
            TextIndexEnd = result.Text.Length,
            Text = string.Empty,
            Paragraphs = new List<TranslateRow>(),
        });

        context.CurrentItem = null;
        context.TextBuilder = new StringBuilder();
    }

    private static void ProcessSentenceEndingRow(MergeResult result, MergeContext context, TranslateRow currentRow, int rowIndex)
    {
        if (context.CurrentItem != null)
        {
            if (string.IsNullOrWhiteSpace(result.Text))
            {
                context.CurrentItem.TextIndexEnd = result.Text.Length;
            }
            else
            {
                var endChar = result.Text[^1];
                context.CurrentItem.EndChar = endChar;
                context.CurrentItem.EndCharOccurrences = Utilities.CountTagInText(context.TextBuilder.ToString(), endChar);
                context.CurrentItem.TextIndexEnd = result.Text.Length;
            }

            result.MergeResultItems.Add(context.CurrentItem);
        }

        context.TextBuilder = new StringBuilder(currentRow.Text);
        result.Text += Environment.NewLine + currentRow.Text;

        context.CurrentItem = new MergeResultItem
        {
            StartIndex = rowIndex,
            EndIndex = rowIndex,
            TextIndexStart = result.Text.Length,
            Text = currentRow.Text,
            Paragraphs = new List<TranslateRow> { currentRow },
        };
    }

    private static bool IsContinuousWithPrevious(MergeContext context, TranslateRow currentRow)
    {
        if (context.CurrentItem == null)
        {
            return false;
        }

        var isFirstOrContinuous = context.CurrentItem.Continuous || context.CurrentItem.StartIndex == context.CurrentItem.EndIndex;
        var gapMs = currentRow.Show.TotalMilliseconds - context.PreviousRow.Hide.TotalMilliseconds;

        return isFirstOrContinuous && gapMs < MaxGapBetweenContinuousLinesMs;
    }

    private static void ProcessContinuousRow(MergeResult result, MergeContext context, TranslateRow currentRow, int rowIndex)
    {
        if (context.CurrentItem == null)
        {
            return;
        }

        context.TextBuilder.Append(' ');
        context.TextBuilder.Append(currentRow.Text);
        result.Text += " " + currentRow.Text;
        context.CurrentItem.Text += " " + currentRow.Text;
        context.CurrentItem.Continuous = true;
        context.CurrentItem.Paragraphs.Add(currentRow);
        context.CurrentItem.EndIndex = rowIndex;
        context.CurrentItem.TextIndexEnd = result.Text.Length;
    }

    private static void FinalizeResult(MergeResult result, MergeContext context)
    {
        if (context.CurrentItem != null)
        {
            result.MergeResultItems.Add(context.CurrentItem);

            if (result.Text.Length > 0 && result.Text.HasSentenceEnding())
            {
                var endChar = result.Text[^1];
                context.CurrentItem.EndChar = endChar;
                context.CurrentItem.EndCharOccurrences = Utilities.CountTagInText(context.TextBuilder.ToString(), endChar);
            }
        }

        result.Text = result.Text.Trim();
        result.ParagraphCount = result.MergeResultItems.Sum(p => p.EndIndex - p.StartIndex + 1);

        foreach (var item in result.MergeResultItems)
        {
            item.Text = item.Text.Trim();
        }
    }

    public static List<string> SplitMultipleLines(MergeResult mergeResult, string translatedText, string language)
    {
        var text = translatedText;

        if (mergeResult.NoSentenceEndingSource)
        {
            return SplitByPeriodMarkers(text);
        }

        var lines = new List<string>();
        foreach (var item in mergeResult.MergeResultItems)
        {
            if (item.IsEmpty)
            {
                lines.Add(string.Empty);
                continue;
            }

            var part = ExtractPartForItem(ref text, item);
            if (string.IsNullOrEmpty(part) && !item.IsEmpty)
            {
                return []; // Failed to extract part
            }

            if (item.Continuous)
            {
                lines.AddRange(SplitContinuousText(part, item, language));
            }
            else
            {
                lines.Add(AutoBreakIfTooLong(part));
            }
        }

        // If there's remaining text, the split failed
        return string.IsNullOrEmpty(text) ? lines : [];
    }

    private static List<string> SplitByPeriodMarkers(string text)
    {
        var lines = new List<string>();
        var sb = new StringBuilder();

        foreach (var translatedLine in text.SplitToLines())
        {
            var trimmed = translatedLine.Trim();
            if (trimmed == ".")
            {
                lines.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else
            {
                sb.AppendLine(trimmed);
            }
        }

        if (sb.Length > 0)
        {
            lines.Add(sb.ToString().Trim());
        }

        return lines;
    }

    private static string ExtractPartForItem(ref string text, MergeResultItem item)
    {
        if (item.EndChar == '\0')
        {
            var result = text;
            text = string.Empty;
            return result;
        }

        var part = FindPartByEndChar(text, item.EndChar, item.EndCharOccurrences);
        if (!string.IsNullOrEmpty(part))
        {
            text = text[part.Length..].Trim();
        }

        return part;
    }

    private static string FindPartByEndChar(string input, char endChar, int targetOccurrence)
    {
        var idx = input.IndexOf(endChar);
        if (idx < 0)
        {
            return string.Empty;
        }

        var count = 1;
        while (idx >= 0 && idx < input.Length - 1)
        {
            if (count == targetOccurrence)
            {
                return input[..(idx + 1)];
            }

            idx = input.IndexOf(endChar, idx + 1);
            count++;
        }

        return input;
    }

    private static string AutoBreakIfTooLong(string text)
    {
        return text.Length > Configuration.Settings.General.SubtitleLineMaximumLength
            ? Utilities.AutoBreakLine(text)
            : text;
    }

    private static List<string> SplitContinuousText(string text, MergeResultItem item, string language)
    {
        var paragraphCount = item.EndIndex - item.StartIndex + 1;

        if (paragraphCount == 2 && item.Paragraphs.Count == 2)
        {
            return SplitIntoTwoParts(text, item, language);
        }

        return TextSplit.SplitMulti(text, paragraphCount, language);
    }

    private static List<string> SplitIntoTwoParts(string text, MergeResultItem item, string language)
    {
        var candidates = GenerateSplitCandidates(text, item, language);

        if (candidates.Count == 0)
        {
            return Utilities.AutoBreakLine(text, Configuration.Settings.General.SubtitleLineMaximumLength * 2, 0, language)
                .SplitToLines()
                .ToList();
        }

        var bestCandidate = SelectBestCandidate(candidates, item);
        return [bestCandidate.Part1, bestCandidate.Part2];
    }

    private static List<SplitCandidate> GenerateSplitCandidates(string text, MergeResultItem item, string language)
    {
        var candidates = new List<SplitCandidate>();
        var maxCps = Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;

        // Strategy 1: Auto-break by language rules
        var autoBreak = Utilities.AutoBreakLine(text, Configuration.Settings.General.SubtitleLineMaximumLength * 2, 0, language)
            .SplitToLines();
        if (autoBreak.Count == 2)
        {
            candidates.Add(CreateCandidate(autoBreak[0], autoBreak[1], item, SplitStrategy.AutoBreak));
        }

        // Strategy 2: Split by character proportion (original text lengths)
        var charPctSplit = SplitByProportion(text, item.Paragraphs[0].Text.Length, item.Paragraphs[1].Text.Length);
        if (!string.IsNullOrEmpty(charPctSplit.Part1))
        {
            candidates.Add(CreateCandidate(charPctSplit.Part1, charPctSplit.Part2, item, SplitStrategy.CharacterProportion));
        }

        // Strategy 3: Split by duration proportion
        var durationPctSplit = SplitByProportion(text,
            item.Paragraphs[0].DurationTotalMilliseconds,
            item.Paragraphs[1].DurationTotalMilliseconds + 1); // +1 to avoid division by zero
        if (!string.IsNullOrEmpty(durationPctSplit.Part1))
        {
            candidates.Add(CreateCandidate(durationPctSplit.Part1, durationPctSplit.Part2, item, SplitStrategy.DurationProportion));
        }

        // Strategy 4: Split at punctuation boundaries
        var punctuationSplit = SplitAtBestPunctuation(text, item);
        if (!string.IsNullOrEmpty(punctuationSplit.Part1))
        {
            candidates.Add(CreateCandidate(punctuationSplit.Part1, punctuationSplit.Part2, item, SplitStrategy.Punctuation));
        }

        return candidates;
    }

    private static SplitCandidate CreateCandidate(string part1, string part2, MergeResultItem item, SplitStrategy strategy)
    {
        var cps1 = CalculateCps(part1, item.Paragraphs[0]);
        var cps2 = CalculateCps(part2, item.Paragraphs[1]);

        return new SplitCandidate
        {
            Part1 = part1,
            Part2 = part2,
            Cps1 = cps1,
            Cps2 = cps2,
            Strategy = strategy
        };
    }

    private static double CalculateCps(string text, TranslateRow paragraph)
    {
        return new Paragraph(text, paragraph.Show.TotalMilliseconds, paragraph.Hide.TotalMilliseconds)
            .GetCharactersPerSecond();
    }

    private static SplitCandidate SelectBestCandidate(List<SplitCandidate> candidates, MergeResultItem item)
    {
        var maxCps = Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;
        var maxLineLength = Configuration.Settings.General.SubtitleLineMaximumLength;

        // Score each candidate
        foreach (var candidate in candidates)
        {
            candidate.Score = CalculateCandidateScore(candidate, maxCps, maxLineLength);
        }

        // Return the candidate with the highest score
        return candidates.OrderByDescending(c => c.Score).First();
    }

    private static double CalculateCandidateScore(SplitCandidate candidate, double maxCps, int maxLineLength)
    {
        var score = 0.0;

        // Bonus for both parts being within CPS limits (most important)
        if (candidate.Cps1 < maxCps && candidate.Cps2 < maxCps)
        {
            score += 100;
        }
        else if (candidate.Cps1 < maxCps || candidate.Cps2 < maxCps)
        {
            score += 30; // Partial bonus if at least one is within limits
        }

        // Penalty for exceeding CPS (proportional to how much it exceeds)
        if (candidate.Cps1 > maxCps)
        {
            score -= (candidate.Cps1 - maxCps) * 2;
        }
        if (candidate.Cps2 > maxCps)
        {
            score -= (candidate.Cps2 - maxCps) * 2;
        }

        // Bonus for ending at natural break points
        if (EndsWithNaturalBreak(candidate.Part1))
        {
            score += 20;
        }

        // Bonus for balanced line lengths
        var lengthDiff = Math.Abs(candidate.Part1.Length - candidate.Part2.Length);
        var avgLength = (candidate.Part1.Length + candidate.Part2.Length) / 2.0;
        var balanceRatio = avgLength > 0 ? lengthDiff / avgLength : 0;
        score += (1 - balanceRatio) * 10; // Up to 10 points for perfect balance

        // Penalty for exceeding max line length
        if (candidate.Part1.Length > maxLineLength)
        {
            score -= 15;
        }
        if (candidate.Part2.Length > maxLineLength)
        {
            score -= 15;
        }

        // Small bonus based on strategy preference
        score += candidate.Strategy switch
        {
            SplitStrategy.Punctuation => 5,
            SplitStrategy.DurationProportion => 3,
            SplitStrategy.CharacterProportion => 2,
            SplitStrategy.AutoBreak => 1,
            _ => 0
        };

        return score;
    }

    private static bool EndsWithNaturalBreak(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var lastChar = text[^1];
        return lastChar is ',' or ';' or ':' or '.' or '!' or '?' or '—' or '-' or ')' or '"' or '\'';
    }

    private static (string Part1, string Part2) SplitByProportion(string text, double length1, double length2)
    {
        var totalLength = length1 + length2;
        if (totalLength <= 0)
        {
            return (string.Empty, string.Empty);
        }

        var targetIdx = (int)Math.Round(text.Length * length1 / totalLength, MidpointRounding.AwayFromZero);
        return FindNearestSpaceSplit(text, targetIdx);
    }

    private static (string Part1, string Part2) FindNearestSpaceSplit(string text, int targetIdx)
    {
        // Search outward from target index to find nearest space
        for (var offset = 0; offset < targetIdx && offset < text.Length - targetIdx; offset++)
        {
            var leftIdx = targetIdx - offset;
            if (leftIdx > 1 && text[leftIdx] == ' ')
            {
                return (text[..leftIdx].Trim(), text[(leftIdx + 1)..].Trim());
            }

            var rightIdx = targetIdx + offset;
            if (rightIdx < text.Length - 1 && text[rightIdx] == ' ')
            {
                return (text[..rightIdx].Trim(), text[(rightIdx + 1)..].Trim());
            }
        }

        return (string.Empty, string.Empty);
    }

    private static (string Part1, string Part2) SplitAtBestPunctuation(string text, MergeResultItem item)
    {
        // Priority order for punctuation-based splits
        char[] punctuationPriority = [',', ';', ':', '—', '-', ')'];

        var totalDuration = item.Paragraphs[0].DurationTotalMilliseconds + item.Paragraphs[1].DurationTotalMilliseconds;
        var targetRatio = totalDuration > 0
            ? item.Paragraphs[0].DurationTotalMilliseconds / totalDuration
            : 0.5;

        var targetIdx = (int)(text.Length * targetRatio);
        var searchRange = text.Length / 3; // Search within 33% of target

        foreach (var punct in punctuationPriority)
        {
            var bestIdx = FindBestPunctuationIndex(text, punct, targetIdx, searchRange);
            if (bestIdx > 0 && bestIdx < text.Length - 1)
            {
                // Split after the punctuation
                var splitIdx = bestIdx + 1;
                while (splitIdx < text.Length && text[splitIdx] == ' ')
                {
                    splitIdx++;
                }

                return (text[..splitIdx].Trim(), text[splitIdx..].Trim());
            }
        }

        return (string.Empty, string.Empty);
    }

    private static int FindBestPunctuationIndex(string text, char punct, int targetIdx, int searchRange)
    {
        var bestIdx = -1;
        var bestDistance = int.MaxValue;

        var startSearch = Math.Max(0, targetIdx - searchRange);
        var endSearch = Math.Min(text.Length, targetIdx + searchRange);

        for (var i = startSearch; i < endSearch; i++)
        {
            if (text[i] == punct)
            {
                var distance = Math.Abs(i - targetIdx);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIdx = i;
                }
            }
        }

        return bestIdx;
    }

    private enum SplitStrategy
    {
        AutoBreak,
        CharacterProportion,
        DurationProportion,
        Punctuation
    }

    private sealed class SplitCandidate
    {
        public required string Part1 { get; init; }
        public required string Part2 { get; init; }
        public double Cps1 { get; init; }
        public double Cps2 { get; init; }
        public SplitStrategy Strategy { get; init; }
        public double Score { get; set; }
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