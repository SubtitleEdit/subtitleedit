using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public interface ISplitManager
{
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, string languageCode);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, string languageCode);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex, string languageCode);
    void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, int textIndex, string languageCode);
}

public class SplitManager : ISplitManager
{
    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, int textIndex, string languageCode)
    {
        var idx = subtitles.IndexOf(subtitle);
        if (idx < 0 || idx >= subtitles.Count)
        {
            return; // Subtitle not found in the collection
        }

        var newSubtitle = new SubtitleLineViewModel(subtitle, true);
        var originalStartMs = subtitle.StartTime.TotalMilliseconds;
        var originalDurationMs = subtitle.Duration.TotalMilliseconds;
        var originalEndMs = subtitle.EndTime.TotalMilliseconds;

        var hasUserVideoPosition = videoPositionSeconds > 0
            && videoPositionSeconds > subtitle.StartTime.TotalSeconds
            && videoPositionSeconds < subtitle.EndTime.TotalSeconds;

        var text = subtitle.Text;
        var lines = text.SplitToLines();
        if (textIndex > 0 && textIndex <= subtitle.Text.Length)
        {
            subtitle.Text = text.Substring(0, textIndex).Trim();
            newSubtitle.Text = text.Substring(textIndex).Trim();

            if (lines.Count == 2)
            {
                var dialogHelper = new DialogSplitMerge { DialogStyle = Configuration.Settings.General.DialogStyle, TwoLetterLanguageCode = languageCode };
                if (dialogHelper.IsDialog(lines))
                {
                    subtitle.Text = DialogSplitMerge.RemoveStartDash(subtitle.Text);
                    newSubtitle.Text = DialogSplitMerge.RemoveStartDash(newSubtitle.Text);
                }
            }

            // SE 4 parity (#11245 follow-up): if either half ends up with a single line
            // longer than the configured max, auto-break it. Cursor-split at the middle
            // of a long single-line subtitle would otherwise leave a too-wide line.
            subtitle.Text = AutoBreakIfTooLong(subtitle.Text, languageCode);
            newSubtitle.Text = AutoBreakIfTooLong(newSubtitle.Text, languageCode);
        }
        else if (lines.Count == 2)
        {
            var dialogHelper = new DialogSplitMerge { DialogStyle = Configuration.Settings.General.DialogStyle, TwoLetterLanguageCode = languageCode };
            if (dialogHelper.IsDialog(lines))
            {
                newSubtitle.Text = lines[1].TrimStart(' ', DialogSplitMerge.GetDashChar(), DialogSplitMerge.GetAlternateDashChar()).Trim();
                subtitle.Text = lines[0].TrimStart(' ', DialogSplitMerge.GetDashChar(), DialogSplitMerge.GetAlternateDashChar()).Trim();
            }
            else
            {
                newSubtitle.Text = lines[1].Trim();
                subtitle.Text = lines[0].Trim();
            }
        }
        else if (lines.Count > 2)
        {
            var splitIndex = lines.Count / 2;

            if (lines.Count % 2 == 1) // odd number of lines
            {
                if (Se.Settings.Tools.SplitOddLinesAction == nameof(SplitOddLinesActionType.WeightTop))
                {
                    splitIndex = splitIndex + 1;
                }
                else if (Se.Settings.Tools.SplitOddLinesAction == nameof(SplitOddLinesActionType.WeightBottom))
                {
                    // no changes
                }
                else // SplitUnevenLineActionType.Smart
                {
                    var try1First = string.Join(Environment.NewLine, lines.GetRange(0, splitIndex + 1)).Trim();
                    var try1Second = string.Join(Environment.NewLine, lines.GetRange(splitIndex + 1, lines.Count - (splitIndex + 1))).Trim();

                    var try2First = string.Join(Environment.NewLine, lines.GetRange(0, splitIndex)).Trim();
                    var try2Second = string.Join(Environment.NewLine, lines.GetRange(splitIndex, lines.Count - splitIndex)).Trim();

                    if (try1First.EndsWith('.') && !try2First.EndsWith('.'))
                    {
                        splitIndex = splitIndex + 1;
                    }
                    else if (!try1First.EndsWith(".") && try2First.EndsWith('.'))
                    {
                        // no changes
                    }
                    else if (Math.Abs(try1First.Length - try1Second.Length) < Math.Abs(try2First.Length - try2Second.Length))
                    {
                        splitIndex = splitIndex + 1;
                    }
                }
            }

            subtitle.Text = string.Join(Environment.NewLine, lines.GetRange(0, splitIndex)).Trim();
            newSubtitle.Text = string.Join(Environment.NewLine, lines.GetRange(splitIndex, lines.Count - splitIndex)).Trim();
        }
        else
        {
            var brokenLines = Utilities.AutoBreakLine(text, Se.Settings.General.SubtitleLineMaximumLength, 0, languageCode).SplitToLines();
            if (brokenLines.Count == 2)
            {
                subtitle.Text = brokenLines[0].Trim();
                newSubtitle.Text = brokenLines[1].Trim();
            }
            else
            {
                subtitle.Text = text;
                newSubtitle.Text = string.Empty;
            }
        }

        var (s1, s2) = FixTags(subtitle.Text, newSubtitle.Text);
        subtitle.Text = s1;
        newSubtitle.Text = s2;

        // Time split — done AFTER the text split so we can weight the divide point
        // by the resulting text-length ratio when no user video position was given.
        //
        // The gap between the two halves is the full MinimumBetweenLines (SE 4 parity).
        // Before #11247 follow-up, SE 5 inserted only half the configured gap because
        // `gap` was divided by 2 and applied only on the leading edge.
        var minGapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        var halfGapMs = minGapMs / 2.0;

        double subtitleEndMs;
        double newSubtitleStartMs;

        if (hasUserVideoPosition)
        {
            // User picked a video frame — that frame becomes the start of the new
            // line; the old line ends one full MinGap earlier. Mirrors SE 4's
            // SplitBehavior == 0 path in SetSplitTime.
            var divideMs = videoPositionSeconds * 1000.0;
            newSubtitleStartMs = divideMs;
            subtitleEndMs = divideMs - minGapMs;
        }
        else
        {
            // Auto-split — divide at the midpoint, weighted by the text-length
            // ratio of the two halves when they differ by more than a couple of
            // characters. Clamp to [0.25, 0.75] so a very lopsided split still
            // produces two viable durations. SE 4 Main.cs:12808 SetSplitTime.
            var firstTextLen = StripForLengthMeasure(subtitle.Text).Length;
            var secondTextLen = StripForLengthMeasure(newSubtitle.Text).Length;

            var startFactor = 0.5;
            if (Math.Abs(firstTextLen - secondTextLen) > 2)
            {
                var total = firstTextLen + secondTextLen;
                if (total > 0)
                {
                    startFactor = (double)firstTextLen / total;
                    if (startFactor < 0.25)
                    {
                        startFactor = 0.25;
                    }
                    if (startFactor > 0.75)
                    {
                        startFactor = 0.75;
                    }
                }
            }

            var middleMs = originalStartMs + originalDurationMs * startFactor;
            subtitleEndMs = middleMs - halfGapMs;
            newSubtitleStartMs = middleMs + halfGapMs;
        }

        // Don't let either half collapse to zero / negative duration when MinGap is
        // larger than the available headroom on a short subtitle. Each half needs
        // strictly positive duration; we give up the configured gap before we give
        // up a positive duration on either side.
        if (subtitleEndMs < originalStartMs + 1)
        {
            subtitleEndMs = originalStartMs + 1;
        }
        if (newSubtitleStartMs > originalEndMs - 1)
        {
            newSubtitleStartMs = originalEndMs - 1;
        }
        if (subtitleEndMs > newSubtitleStartMs)
        {
            // No room left for the requested gap — collapse to the midpoint of the
            // available window so both halves still end up with positive duration
            // (originalEnd > newSubtitleStart > originalStart, and
            //  originalStart < subtitleEnd < originalEnd).
            var midpoint = (subtitleEndMs + newSubtitleStartMs) / 2.0;
            subtitleEndMs = midpoint;
            newSubtitleStartMs = midpoint;
        }

        subtitle.EndTime = TimeSpan.FromMilliseconds(subtitleEndMs);
        newSubtitle.SetStartTimeOnly(TimeSpan.FromMilliseconds(newSubtitleStartMs));

        subtitles.Insert(idx + 1, newSubtitle);
    }

    // Strip HTML/ASSA tags and ALL line-break variants for length measurement.
    // Subtitle text may contain CRLF, LF, CR, or Unicode line separator (U+2028)
    // regardless of platform — these are all recognised by SplitToLines — and
    // counting them as characters would skew the proportional-duration weighting.
    private static string StripForLengthMeasure(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var stripped = HtmlUtil.RemoveHtmlTags(text, true);
        var sb = new System.Text.StringBuilder(stripped.Length);
        foreach (var c in stripped)
        {
            // CR, LF, NEL (U+0085), line separator (U+2028), paragraph separator (U+2029).
            if (c == '\r' || c == '\n' || c == '\u0085' || c == '\u2028' || c == '\u2029')
            {
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    // Mirrors SE 4's per-half guard in SplitSelectedParagraph: if any line in `text`
    // (after stripping HTML/ASSA tags for measurement) is over the configured
    // single-line max length, fold it via Utilities.AutoBreakLine. Otherwise the user
    // keeps the line breaks they intentionally chose.
    private static string AutoBreakIfTooLong(string text, string languageCode)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var maxLen = Configuration.Settings.General.SubtitleLineMaximumLength;
        if (maxLen <= 0)
        {
            return text;
        }

        var anyLineTooLong = HtmlUtil.RemoveHtmlTags(text, true)
            .SplitToLines()
            .Any(line => line.Length > maxLen);

        return anyLineTooLong ? Utilities.AutoBreakLine(text, languageCode) : text;
    }

    private static readonly (string Open, string Close)[] SimpleHtmlTags =
    [
        ("<b>", "</b>"),
        ("<i>", "</i>"),
        ("<u>", "</u>"),
    ];

    private static readonly string[] CompoundHtmlOpenPrefixes = ["<font", "<color"];
    private static readonly string[] CompoundHtmlCloseTags = ["</font>", "</color>"];

    private static (string, string) FixTags(string text1, string text2)
    {
        if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
        {
            return (text1, text2);
        }

        var closingToAppend = new List<string>();
        var openingToPrepend = new List<string>();

        // Handle simple HTML tags: <b>, <i>, <u>
        foreach (var (open, close) in SimpleHtmlTags)
        {
            var openCount = CountOccurrences(text1, open);
            var closeCount = CountOccurrences(text1, close);
            var unclosed = openCount - closeCount;
            if (unclosed > 0)
            {
                for (var i = 0; i < unclosed; i++)
                {
                    closingToAppend.Add(close);
                }
                if (!string.IsNullOrEmpty(text2))
                {
                    for (var i = 0; i < unclosed; i++)
                    {
                        openingToPrepend.Add(open);
                    }
                }
            }
        }

        // Handle compound HTML tags: <font ...>, <color ...>
        foreach (var (prefix, closeTag) in CompoundHtmlOpenPrefixes.Zip(CompoundHtmlCloseTags))
        {
            var openCount = CountCompoundOpenTags(text1, prefix);
            var closeCount = CountOccurrences(text1, closeTag);
            var unclosed = openCount - closeCount;
            if (unclosed > 0)
            {
                for (var i = 0; i < unclosed; i++)
                {
                    closingToAppend.Add(closeTag);
                }
                if (!string.IsNullOrEmpty(text2))
                {
                    // Re-open with the last unclosed opening tag
                    var lastOpenTag = GetLastUnclosedCompoundTag(text1, prefix, closeTag);
                    if (lastOpenTag != null)
                    {
                        for (var i = 0; i < unclosed; i++)
                        {
                            openingToPrepend.Add(lastOpenTag);
                        }
                    }
                }
            }
        }

        if (closingToAppend.Count > 0)
        {
            text1 = text1 + string.Concat(closingToAppend);
        }

        if (openingToPrepend.Count > 0)
        {
            text2 = string.Concat(openingToPrepend) + text2;
        }

        // Handle ASSA tags: propagate active state tags to text2
        if (!string.IsNullOrEmpty(text2))
        {
            text2 = PropagateAssaTags(text1, text2);
        }

        return (text1, text2);
    }

    private static int CountOccurrences(string text, string tag)
    {
        var count = 0;
        var idx = 0;
        while ((idx = text.IndexOf(tag, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            idx += tag.Length;
        }
        return count;
    }

    private static int CountCompoundOpenTags(string text, string prefix)
    {
        var count = 0;
        var idx = 0;
        while ((idx = text.IndexOf(prefix, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            idx += prefix.Length;
        }
        return count;
    }

    private static string? GetLastUnclosedCompoundTag(string text, string prefix, string closeTag)
    {
        // Collect all opening tags in order, then simulate close tags consuming the last opened
        var openTags = new List<(int pos, string tag)>();
        var idx = 0;
        while ((idx = text.IndexOf(prefix, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            var end = text.IndexOf('>', idx);
            if (end < 0)
            {
                break;
            }
            openTags.Add((idx, text.Substring(idx, end - idx + 1)));
            idx = end + 1;
        }

        // Remove one open tag per close tag found (last-in-first-out)
        idx = 0;
        while ((idx = text.IndexOf(closeTag, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            if (openTags.Count > 0)
            {
                openTags.RemoveAt(openTags.Count - 1);
            }
            idx += closeTag.Length;
        }

        return openTags.Count > 0 ? openTags[^1].tag : null;
    }

    private static string PropagateAssaTags(string text1, string text2)
    {
        // For each ASSA toggle tag {\b1}, {\i1}, {\u1} active in text1 (opened but not closed with {\b0} etc.),
        // prepend the corresponding tag to text2.
        var assaToggles = new[] { ("\\b1}", "\\b0}"), ("\\i1}", "\\i0}"), ("\\u1}", "\\u0}") };
        var tagsToAdd = new List<string>();
        foreach (var (onSuffix, offSuffix) in assaToggles)
        {
            var onCount = CountOccurrences(text1, onSuffix);
            var offCount = CountOccurrences(text1, offSuffix);
            if (onCount > offCount)
            {
                tagsToAdd.Add("{" + onSuffix);
            }
        }

        // Propagate last active \fn and \c tags
        var fnTag = GetLastAssaTag(text1, "\\fn");
        if (fnTag != null)
        {
            tagsToAdd.Add(fnTag);
        }

        var colorTag = GetLastAssaTag(text1, "\\c&H");
        if (colorTag != null)
        {
            tagsToAdd.Add(colorTag);
        }

        if (tagsToAdd.Count == 0)
        {
            return text2;
        }

        return "{" + string.Join("", tagsToAdd.Select(t => t.TrimStart('{'))) + text2;
    }

    private static string? GetLastAssaTag(string text, string prefix)
    {
        var lastIdx = -1;
        var idx = 0;
        while ((idx = text.IndexOf(prefix, idx, StringComparison.Ordinal)) >= 0)
        {
            lastIdx = idx;
            idx += prefix.Length;
        }

        if (lastIdx < 0)
        {
            return null;
        }

        var blockStart = text.LastIndexOf('{', lastIdx);
        var blockEnd = text.IndexOf('}', lastIdx);
        if (blockStart < 0 || blockEnd < 0)
        {
            return null;
        }

        // Extract only the specific tag from within the block
        var tagStart = lastIdx;
        var tagEnd = text.IndexOf('}', tagStart);
        if (tagEnd < 0)
        {
            return null;
        }

        return "{" + text.Substring(tagStart, tagEnd - tagStart + 1);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, string languageCode)
    {
        Split(subtitles, subtitle, -1, -1, languageCode);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, double videoPositionSeconds, string languageCode)
    {
        Split(subtitles, subtitle, videoPositionSeconds, -1, languageCode);
    }

    public void Split(ObservableCollection<SubtitleLineViewModel> subtitles, SubtitleLineViewModel subtitle, int textIndex, string languageCode)
    {
        Split(subtitles, subtitle, -1, textIndex, languageCode);
    }
}