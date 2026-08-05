using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

internal static class SubtitleGridCopyPasteHelper
{
    internal static async Task Copy(Window window, List<SubtitleLineViewModel> selectedItems, SubtitleFormat subtitleFormat, Subtitle sourceSubtitle)
    {
        var subtitle = new Subtitle();
        subtitle.Header = sourceSubtitle.Header;
        subtitle.Footer = sourceSubtitle.Footer;
        foreach (var item in selectedItems)
        {
            subtitle.Paragraphs.Add(item.ToParagraph(subtitleFormat));
        }

        var text = GetClipboardText(subtitleFormat, subtitle);
        await ClipboardHelper.SetTextAsync(window, text);
    }

    internal static async Task Cut(Window window, ObservableCollection<SubtitleLineViewModel> subtitles, List<SubtitleLineViewModel> selectedItems, SubtitleFormat subtitleFormat, Subtitle sourceSubtitle)
    {
        var subtitle = new Subtitle();
        subtitle.Header = sourceSubtitle.Header;
        subtitle.Footer = sourceSubtitle.Footer;
        foreach (var item in selectedItems)
        {
            subtitle.Paragraphs.Add(item.ToParagraph(subtitleFormat));
        }

        var text = GetClipboardText(subtitleFormat, subtitle);
        await ClipboardHelper.SetTextAsync(window, text);

        foreach (var item in selectedItems)
        {
            subtitles.Remove(item);
        }
    }

    // When copying ASSA/SSA lines, only the event lines ("Dialogue:"/"Comment:") belong on the
    // clipboard: Aegisub's paste interprets every other clipboard line (the [Script Info] /
    // [V4+ Styles] file header) as a plain-text subtitle line, so the file headers would be
    // pasted as fake subtitle lines (issue #10476). Aegisub's own copy puts only the entry
    // data on the clipboard, so match that; SE's paste parses the bare event lines back into
    // paragraphs, so the SE-to-SE round-trip keeps working.
    internal static string GetClipboardText(SubtitleFormat subtitleFormat, Subtitle subtitle)
    {
        var text = subtitleFormat.ToText(subtitle, string.Empty);
        if (subtitleFormat is AdvancedSubStationAlpha or SubStationAlpha)
        {
            var lines = text.SplitToLines();
            var firstEventIndex = -1;
            for (var i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].TrimStart();
                if (trimmed.StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("Comment:", StringComparison.OrdinalIgnoreCase))
                {
                    firstEventIndex = i;
                    break;
                }
            }

            if (firstEventIndex > 0)
            {
                // Stop at the first section that follows the events: ToText appends the
                // subtitle footer ([Fonts] / [Graphics] / [Aegisub Extradata], including the
                // embedded font payload) after the event lines, and that would be pasted as
                // fake subtitle lines just like the header was (#10476).
                var endIndex = lines.Count;
                for (var i = firstEventIndex; i < lines.Count; i++)
                {
                    if (lines[i].TrimStart().StartsWith('['))
                    {
                        endIndex = i;
                        break;
                    }
                }

                var eventLines = lines.GetRange(firstEventIndex, endIndex - firstEventIndex);
                while (eventLines.Count > 0 && string.IsNullOrWhiteSpace(eventLines[eventLines.Count - 1]))
                {
                    eventLines.RemoveAt(eventLines.Count - 1);
                }

                text = string.Join(Environment.NewLine, eventLines);
            }
        }

        return text;
    }

    internal static async Task Paste(Window window, ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat)
    {
        var text = await ClipboardHelper.GetTextAsync(window);
        Paste(subtitles, index, subtitleFormat, text);
    }

    internal static void Paste(ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var lines = text.SplitToLines();
        var subtitle = Subtitle.Parse(lines, subtitleFormat.Extension);

        var beforeIndex = -1;
        if (subtitles.Count > 0 && index >= 0 && index < subtitles.Count)
        {
            beforeIndex = index;
            index++;
        }
        else
        {
            // If index is invalid (e.g. -1), append to end. This also clamps an empty
            // subtitle's insertion index to zero.
            beforeIndex = subtitles.Count - 1;
            index = subtitles.Count;
        }

        if (subtitle?.Paragraphs.Count > 0)
        {
            LoadParagraphs(subtitles, index, subtitleFormat, subtitle, GetOverlapShift(subtitles, beforeIndex, subtitle));
            return;
        }

        foreach (SubtitleFormat item in SubtitleFormat.AllSubtitleFormats)
        {
            if (item.IsMine(lines, string.Empty) && subtitle != null)
            {
                item.LoadSubtitle(subtitle, lines, string.Empty);
                LoadParagraphs(subtitles, index, subtitleFormat, subtitle, GetOverlapShift(subtitles, beforeIndex, subtitle));
                return;
            }
        }

        // fallback - plain text
        var addTimeMilliseconds = beforeIndex >= 0
            ? subtitles[beforeIndex].EndTime.TotalMilliseconds + Se.Settings.General.MinimumBetweenLines.GetMilliseconds()
            : 0;
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var p = new SubtitleLineViewModel
                {
                    StartTime = TimeSpan.FromMilliseconds(addTimeMilliseconds),
                    EndTime = TimeSpan.FromMilliseconds(addTimeMilliseconds + Se.Settings.General.NewEmptyDefaultMs),
                    Text = line.Trim()
                };
                subtitles.Insert(index, p);
                index++;
                addTimeMilliseconds += Se.Settings.General.NewEmptyDefaultMs + Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
            }
        }
    }

    private static double GetOverlapShift(ObservableCollection<SubtitleLineViewModel> subtitles, int beforeIndex, Subtitle subtitle)
    {
        if (subtitle.Paragraphs.Count == 0 || beforeIndex < 0 || beforeIndex >= subtitles.Count)
        {
            return 0;
        }

        var lastEnd = subtitles[beforeIndex].EndTime.TotalMilliseconds;
        var firstPastedStart = subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
        var startsAfterNextLine = beforeIndex + 1 < subtitles.Count &&
                                  subtitles[beforeIndex + 1].StartTime.TotalMilliseconds < firstPastedStart;
        return lastEnd > firstPastedStart || startsAfterNextLine
            ? lastEnd + Se.Settings.General.MinimumBetweenLines.GetMilliseconds() - firstPastedStart
            : 0;
    }

    private static void LoadParagraphs(ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat, Subtitle subtitle, double addTimeMilliseconds)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            if (addTimeMilliseconds != 0)
            {
                p.StartTime.TotalMilliseconds += addTimeMilliseconds;
                p.EndTime.TotalMilliseconds += addTimeMilliseconds;
            }

            subtitles.Insert(index, new SubtitleLineViewModel(p, subtitleFormat));
            index++;
        }
    }
}
