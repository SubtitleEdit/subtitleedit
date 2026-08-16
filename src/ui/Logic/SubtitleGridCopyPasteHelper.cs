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

    /// <summary>
    /// Parses clipboard text as a subtitle, or returns null when no format recognizes it - plain
    /// text lines (translations copied out of a text document) end up here. Both the paste that
    /// inserts lines and the paste that overwrites the selection have to tell those two apart, so
    /// they ask the same question here (#13682).
    /// </summary>
    internal static Subtitle? ParseClipboardSubtitle(string? text, SubtitleFormat subtitleFormat)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var lines = text.SplitToLines();
        var subtitle = Subtitle.Parse(lines, subtitleFormat.Extension);
        if (subtitle == null)
        {
            return null;
        }

        if (subtitle.Paragraphs.Count > 0)
        {
            return subtitle;
        }

        foreach (SubtitleFormat item in SubtitleFormat.AllSubtitleFormats)
        {
            if (item.IsMine(lines, string.Empty))
            {
                item.LoadSubtitle(subtitle, lines, string.Empty);
                return subtitle;
            }
        }

        return null;
    }

    /// <summary>
    /// Pastes <paramref name="text"/> into <paramref name="subtitles"/> at <paramref name="index"/>
    /// and returns the inserted lines in grid order (empty when nothing was pasted), so the caller
    /// can select and scroll to them like SE4 did (#13705).
    /// </summary>
    internal static List<SubtitleLineViewModel> PasteText(ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<SubtitleLineViewModel>();
        }

        var addTimeMilliseconds = (double)0;
        if (subtitles.Count > 0 && index >= 0 && index < subtitles.Count)
        {
            addTimeMilliseconds = subtitles[index].EndTime.TotalMilliseconds + Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
            index++;
        }
        else if (subtitles.Count > 0)
        {
             // If index is invalid (e.g. -1), append to end
             addTimeMilliseconds = subtitles[subtitles.Count - 1].EndTime.TotalMilliseconds + Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
             index = subtitles.Count;
        }


        var clipboardSubtitle = ParseClipboardSubtitle(text, subtitleFormat);
        if (clipboardSubtitle != null)
        {
            return LoadParagraphs(subtitles, index, subtitleFormat, clipboardSubtitle);
        }

        // fallback - plain text
        var lines = text.SplitToLines();
        var insertedLines = new List<SubtitleLineViewModel>();
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
                insertedLines.Add(p);
                index++;
                addTimeMilliseconds += Se.Settings.General.NewEmptyDefaultMs + Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
            }
        }

        return insertedLines;
    }

    private static List<SubtitleLineViewModel> LoadParagraphs(ObservableCollection<SubtitleLineViewModel> subtitles, int index, SubtitleFormat subtitleFormat, Subtitle subtitle)
    {
        var insertedLines = new List<SubtitleLineViewModel>(subtitle.Paragraphs.Count);
        foreach (var p in subtitle.Paragraphs)
        {
            var line = new SubtitleLineViewModel(p, subtitleFormat);
            subtitles.Insert(index, line);
            insertedLines.Add(line);
            index++;
        }

        return insertedLines;
    }
}
