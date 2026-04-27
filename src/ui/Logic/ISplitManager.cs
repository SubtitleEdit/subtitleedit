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
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 2.0;

        var dividePositionMs = subtitle.StartTime.TotalMilliseconds + subtitle.Duration.TotalMilliseconds / 2.0 + gap;
        if (videoPositionSeconds > 0 && videoPositionSeconds > subtitle.StartTime.TotalSeconds && videoPositionSeconds < subtitle.EndTime.TotalSeconds)
        {
            dividePositionMs = videoPositionSeconds * 1000.0;
        }

        newSubtitle.SetStartTimeOnly(TimeSpan.FromMilliseconds(dividePositionMs));
        subtitle.EndTime = TimeSpan.FromMilliseconds(newSubtitle.StartTime.TotalMilliseconds - gap);

        var text = subtitle.Text;
        var lines = text.SplitToLines();
        if (textIndex > 0 && textIndex <= subtitle.Text.Length)
        {
            subtitle.Text = text.Substring(0, textIndex).Trim();
            newSubtitle.Text = text.Substring(textIndex).Trim();
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

        subtitles.Insert(idx + 1, newSubtitle);
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
            if (end < 0) break;
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

        if (lastIdx < 0) return null;

        var blockStart = text.LastIndexOf('{', lastIdx);
        var blockEnd = text.IndexOf('}', lastIdx);
        if (blockStart < 0 || blockEnd < 0) return null;

        // Extract only the specific tag from within the block
        var tagStart = lastIdx;
        var tagEnd = text.IndexOf('}', tagStart);
        if (tagEnd < 0) return null;

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