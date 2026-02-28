using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public static class BatchStatics
{
    public static string CalculateGeneralStatistics(List<BatchConvertItem> batchItems)
    {
        if (batchItems.Count == 0 || batchItems.All(p => p.Subtitle == null))
        {
            return Se.Language.File.Statistics.NothingFound;
        }

        var allText = new StringBuilder();
        int minimumLineLength = 99999999;
        int maximumLineLength = 0;
        long totalLineLength = 0;
        int minimumSingleLineLength = 99999999;
        int maximumSingleLineLength = 0;
        long totalSingleLineLength = 0;
        long totalSingleLines = 0;
        int minimumSingleLineWidth = 99999999;
        int maximumSingleLineWidth = 0;
        long totalSingleLineWidth = 0;
        double minimumDuration = 100000000;
        double maximumDuration = 0;
        double totalDuration = 0;
        double minimumCharsSec = 100000000;
        double maximumCharsSec = 0;
        double totalCharsSec = 0;
        double minimumWpm = 100000000;
        double maximumWpm = 0;
        double totalWpm = 0;
        var gapMinimum = double.MaxValue;
        var gapMaximum = 0d;
        var gapTotal = 0d;

        var aboveOptimalCpsCount = 0;
        var aboveMaximumCpsCount = 0;
        var aboveMaximumWpmCount = 0;
        var belowMinimumDurationCount = 0;
        var aboveMaximumDurationCount = 0;
        var aboveMaximumLineLengthCount = 0;
        var aboveMaximumLineWidthCount = 0;
        var belowMinimumGapCount = 0;
        var totalNumberOfLines = 0;

        foreach (var batchItem in batchItems)
        {
            if (batchItem.Subtitle == null || batchItem.Subtitle.Paragraphs.Count == 0)
            {
                continue;
            }

            var subtitle = batchItem.Subtitle;

            foreach (var p in subtitle.Paragraphs)
            {
                allText.Append(p.Text);

                var len = GetLineLength(p);
                minimumLineLength = Math.Min(minimumLineLength, len);
                maximumLineLength = Math.Max(len, maximumLineLength);
                totalLineLength += len;

                var duration = p.DurationTotalMilliseconds;
                minimumDuration = Math.Min(duration, minimumDuration);
                maximumDuration = Math.Max(duration, maximumDuration);
                totalDuration += duration;

                var charsSec = p.GetCharactersPerSecond();
                minimumCharsSec = Math.Min(charsSec, minimumCharsSec);
                maximumCharsSec = Math.Max(charsSec, maximumCharsSec);
                totalCharsSec += charsSec;

                var wpm = p.WordsPerMinute;
                minimumWpm = Math.Min(wpm, minimumWpm);
                maximumWpm = Math.Max(wpm, maximumWpm);
                totalWpm += wpm;

                var next = subtitle.GetParagraphOrDefault(subtitle.GetIndex(p) + 1);
                if (next != null)
                {
                    var gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
                    if (gap < gapMinimum)
                    {
                        gapMinimum = gap;
                    }

                    if (gap > gapMaximum)
                    {
                        gapMaximum = gap;
                    }

                    if (gap < Configuration.Settings.General.MinimumMillisecondsBetweenLines)
                    {
                        belowMinimumGapCount++;
                    }

                    gapTotal += gap;
                }

                foreach (var line in p.Text.SplitToLines())
                {
                    var l2 = GetSingleLineLength(line);
                    minimumSingleLineLength = Math.Min(l2, minimumSingleLineLength);
                    maximumSingleLineLength = Math.Max(l2, maximumSingleLineLength);
                    totalSingleLineLength += l2;

                    if (l2 > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        aboveMaximumLineLengthCount++;
                    }

                    if (Configuration.Settings.Tools.ListViewSyntaxColorWideLines)
                    {
                        var w = GetSingleLineWidth(line);
                        minimumSingleLineWidth = Math.Min(w, minimumSingleLineWidth);
                        maximumSingleLineWidth = Math.Max(w, maximumSingleLineWidth);
                        totalSingleLineWidth += w;

                        if (w > Configuration.Settings.General.SubtitleLineMaximumPixelWidth)
                        {
                            aboveMaximumLineWidthCount++;
                        }
                    }

                    totalSingleLines++;
                }

                var cps = p.GetCharactersPerSecond();
                if (cps > Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds)
                {
                    aboveOptimalCpsCount++;
                }

                if (cps > Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds)
                {
                    aboveMaximumCpsCount++;
                }

                if (p.WordsPerMinute > Configuration.Settings.General.SubtitleMaximumWordsPerMinute)
                {
                    aboveMaximumWpmCount++;
                }

                if (p.DurationTotalMilliseconds < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                {
                    belowMinimumDurationCount++;
                }

                if (p.DurationTotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                {
                    aboveMaximumDurationCount++;
                }
            }

            totalNumberOfLines += subtitle.Paragraphs.Count;
        }

        var allTextToLower = allText.ToString().ToLowerInvariant();

        var sb = new StringBuilder();
        var l = Se.Language.File.Statistics;
        sb.AppendLine(string.Format(l.NumberOfLinesX, totalNumberOfLines));
        sb.AppendLine(string.Format(l.NumberOfCharactersInTextOnly, allText.ToString().CountCharacters(false)));
        sb.AppendLine(string.Format(l.TotalDuration, new TimeCode(totalDuration).ToDisplayString()));
        sb.AppendLine(string.Format(l.TotalCharsPerSecond, (double)allText.ToString().CountCharacters(true) / (totalDuration / TimeCode.BaseUnit)));
        sb.AppendLine(string.Format(l.NumberOfItalicTags, Utilities.CountTagInText(allTextToLower, "<i>")));
        sb.AppendLine(string.Format(l.NumberOfBoldTags, Utilities.CountTagInText(allTextToLower, "<b>")));
        sb.AppendLine(string.Format(l.NumberOfUnderlineTags, Utilities.CountTagInText(allTextToLower, "<u>")));
        sb.AppendLine(string.Format(l.NumberOfFontTags, Utilities.CountTagInText(allTextToLower, "<font ")));
        sb.AppendLine(string.Format(l.NumberOfAlignmentTags, Utilities.CountTagInText(allTextToLower, "{\\an")));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.LineLengthMinimum, minimumLineLength));
        sb.AppendLine(string.Format(l.LineLengthMaximum, maximumLineLength));
        sb.AppendLine(string.Format(l.LineLengthAverage, (double)totalLineLength / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.LinesPerSubtitleAverage, (double)totalSingleLines / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.SingleLineLengthMinimum, minimumSingleLineLength));
        sb.AppendLine(string.Format(l.SingleLineLengthMaximum, maximumSingleLineLength));
        sb.AppendLine(string.Format(l.SingleLineLengthAverage, (double)totalSingleLineLength / totalSingleLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.SingleLineLengthExceedingMaximum, Configuration.Settings.General.SubtitleLineMaximumLength, aboveMaximumLineLengthCount,
            ((double)aboveMaximumLineLengthCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();

        if (Configuration.Settings.Tools.ListViewSyntaxColorWideLines)
        {
            sb.AppendLine(string.Format(l.SingleLineWidthMinimum, minimumSingleLineWidth));
            sb.AppendLine(string.Format(l.SingleLineWidthMaximum, maximumSingleLineWidth));
            sb.AppendLine(string.Format(l.SingleLineWidthAverage, (double)totalSingleLineWidth / totalSingleLines));
            sb.AppendLine();
            sb.AppendLine(string.Format(l.SingleLineWidthExceedingMaximum, Configuration.Settings.General.SubtitleLineMaximumPixelWidth, aboveMaximumLineWidthCount,
                ((double)aboveMaximumLineWidthCount / totalNumberOfLines) * 100.0));
            sb.AppendLine();
        }

        sb.AppendLine(string.Format(l.DurationMinimum, minimumDuration / TimeCode.BaseUnit));
        sb.AppendLine(string.Format(l.DurationMaximum, maximumDuration / TimeCode.BaseUnit));
        sb.AppendLine(string.Format(l.DurationAverage, totalDuration / totalNumberOfLines / TimeCode.BaseUnit));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.DurationExceedingMinimum, Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds / TimeCode.BaseUnit, belowMinimumDurationCount,
            ((double)belowMinimumDurationCount / totalNumberOfLines) * 100.0));
        sb.AppendLine(string.Format(l.DurationExceedingMaximum, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds / TimeCode.BaseUnit, aboveMaximumDurationCount,
            ((double)aboveMaximumDurationCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.CharactersPerSecondMinimum, minimumCharsSec));
        sb.AppendLine(string.Format(l.CharactersPerSecondMaximum, maximumCharsSec));
        sb.AppendLine(string.Format(l.CharactersPerSecondAverage, totalCharsSec / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.CharactersPerSecondExceedingOptimal, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds, aboveOptimalCpsCount,
            ((double)aboveOptimalCpsCount / totalNumberOfLines) * 100.0));
        sb.AppendLine(string.Format(l.CharactersPerSecondExceedingMaximum, Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds, aboveMaximumCpsCount,
            ((double)aboveMaximumCpsCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.WordsPerMinuteMinimum, minimumWpm));
        sb.AppendLine(string.Format(l.WordsPerMinuteMaximum, maximumWpm));
        sb.AppendLine(string.Format(l.WordsPerMinuteAverage, totalWpm / totalNumberOfLines));
        sb.AppendLine();
        sb.AppendLine(string.Format(l.WordsPerMinuteExceedingMaximum, Configuration.Settings.General.SubtitleMaximumWordsPerMinute, aboveMaximumWpmCount,
            ((double)aboveMaximumWpmCount / totalNumberOfLines) * 100.0));
        sb.AppendLine();

        if (totalNumberOfLines > 1)
        {
            sb.AppendLine(string.Format(l.GapMinimum, gapMinimum));
            sb.AppendLine(string.Format(l.GapMaximum, gapMaximum));
            sb.AppendLine(string.Format(l.GapAverage, gapTotal / totalNumberOfLines - 1));
            sb.AppendLine();
            sb.AppendLine(string.Format(l.GapExceedingMinimum, Configuration.Settings.General.MinimumMillisecondsBetweenLines, belowMinimumGapCount,
                ((double)belowMinimumGapCount / totalNumberOfLines) * 100.0));
            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    private static int GetLineLength(Paragraph p)
    {
        return p.Text.Replace(Environment.NewLine, string.Empty).CountCharacters(Configuration.Settings.General.CpsLineLengthStrategy, false);
    }

    private static int GetSingleLineLength(string s)
    {
        return s.CountCharacters(Configuration.Settings.General.CpsLineLengthStrategy, false);
    }

    private static int GetSingleLineWidth(string s)
    {
        var textBlock = new TextBlock();
        var x = TextMeasurer.MeasureString(HtmlUtil.RemoveHtmlTags(s, true), textBlock.FontFamily.Name, (float)textBlock.FontSize);
        return (int)x.Width;
    }
}