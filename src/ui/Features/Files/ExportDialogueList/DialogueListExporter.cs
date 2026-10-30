using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files.ExportDialogueList;

public enum DialogueListColumn
{
    Number,
    Start,
    End,
    Duration,
    Gap,
    Text,
    OriginalText,
    Actor,
    Style,
    Layer,
    Forced,
    Cps,
    Wpm,
    Characters,
}

public enum DialogueListTimeCodeFormat
{
    /// <summary>hh:mm:ss:ff - what dubbing and broadcast lists use.</summary>
    Frames,

    /// <summary>hh:mm:ss,zzz</summary>
    Milliseconds,

    /// <summary>A number of seconds, so Excel can calculate with it.</summary>
    Seconds,
}

public sealed record DialogueListRow(
    int Number,
    TimeSpan Start,
    TimeSpan End,
    string Text,
    string OriginalText,
    string Actor,
    string Style,
    int Layer,
    bool Forced);

public sealed class DialogueListOptions
{
    public List<DialogueListColumn> Columns { get; set; } = new();
    public DialogueListTimeCodeFormat TimeCodeFormat { get; set; } = DialogueListTimeCodeFormat.Frames;

    /// <summary>True keeps line breaks inside the cell, false joins the lines with " | ".</summary>
    public bool LineBreaksInCell { get; set; } = true;

    public bool IncludeHeader { get; set; } = true;
    public bool RemoveFormatting { get; set; } = true;
}

/// <summary>
/// The "dialogue list" spreadsheet: one row per subtitle with the chosen columns. The header
/// names are the English ones the CSV/XLSX import recognizes (Start, End, Duration, Text,
/// Actor), so an edited list can be imported again.
/// </summary>
public static class DialogueListExporter
{
    public const string LineSeparator = " | ";

    public static string GetHeader(DialogueListColumn column) => column switch
    {
        DialogueListColumn.Number => "#",
        DialogueListColumn.Start => "Start",
        DialogueListColumn.End => "End",
        DialogueListColumn.Duration => "Duration",
        DialogueListColumn.Gap => "Gap",
        DialogueListColumn.Text => "Text",
        DialogueListColumn.OriginalText => "Original text",
        DialogueListColumn.Actor => "Actor",
        DialogueListColumn.Style => "Style",
        DialogueListColumn.Layer => "Layer",
        DialogueListColumn.Forced => "Forced",
        DialogueListColumn.Cps => "CPS",
        DialogueListColumn.Wpm => "WPM",
        DialogueListColumn.Characters => "Characters",
        _ => column.ToString(),
    };

    /// <summary>
    /// True when the column has something to show for these rows - an Actor column of a file
    /// without actors is not ticked by default.
    /// </summary>
    public static bool HasData(DialogueListColumn column, IReadOnlyList<DialogueListRow> rows) => column switch
    {
        DialogueListColumn.OriginalText => rows.Any(r => !string.IsNullOrWhiteSpace(r.OriginalText)),
        DialogueListColumn.Actor => rows.Any(r => !string.IsNullOrWhiteSpace(r.Actor)),
        DialogueListColumn.Style => rows.Any(r => !string.IsNullOrWhiteSpace(r.Style)),
        DialogueListColumn.Layer => rows.Any(r => r.Layer != 0),
        DialogueListColumn.Forced => rows.Any(r => r.Forced),
        _ => true,
    };

    public static List<IReadOnlyList<object?>> MakeCells(IReadOnlyList<DialogueListRow> rows, DialogueListOptions options, double frameRate)
    {
        var result = new List<IReadOnlyList<object?>>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var next = i + 1 < rows.Count ? rows[i + 1] : null;
            result.Add(options.Columns.Select(c => GetCell(row, next, c, options, frameRate)).ToList());
        }

        return result;
    }

    public static byte[] CreateXlsx(IReadOnlyList<DialogueListRow> rows, DialogueListOptions options, double frameRate)
    {
        var headers = options.Columns.Select(GetHeader).ToList();
        var widths = options.Columns.Select(GetWidth).ToList();
        return XlsxWriter.Create("Dialogue list", headers, MakeCells(rows, options, frameRate), widths, options.IncludeHeader, options.LineBreaksInCell);
    }

    private static object? GetCell(DialogueListRow row, DialogueListRow? next, DialogueListColumn column, DialogueListOptions options, double frameRate)
    {
        return column switch
        {
            DialogueListColumn.Number => row.Number,
            DialogueListColumn.Start => FormatTime(row.Start, options.TimeCodeFormat, frameRate),
            DialogueListColumn.End => FormatTime(row.End, options.TimeCodeFormat, frameRate),
            DialogueListColumn.Duration => FormatTime(row.End - row.Start, options.TimeCodeFormat, frameRate),
            DialogueListColumn.Gap => next == null ? null : FormatTime(next.Start - row.End, options.TimeCodeFormat, frameRate),
            DialogueListColumn.Text => FormatText(row.Text, options),
            DialogueListColumn.OriginalText => FormatText(row.OriginalText, options),
            DialogueListColumn.Actor => row.Actor,
            DialogueListColumn.Style => row.Style,
            DialogueListColumn.Layer => row.Layer,
            DialogueListColumn.Forced => row.Forced ? "Yes" : string.Empty,
            DialogueListColumn.Cps => Math.Round(SubtitleTextInfoHelper.GetCharactersPerSecond(row.Text, row.Start, row.End), 1),
            DialogueListColumn.Wpm => GetWordsPerMinute(row),
            DialogueListColumn.Characters => (int)SubtitleTextInfoHelper.GetTotalLength(SubtitleTextInfoHelper.StripHtml(row.Text)),
            _ => null,
        };
    }

    private static double GetWordsPerMinute(DialogueListRow row)
    {
        var seconds = (row.End - row.Start).TotalSeconds;
        return seconds <= 0.001 || string.IsNullOrEmpty(row.Text)
            ? 0
            : Math.Round(60.0 / seconds * HtmlUtil.RemoveHtmlTags(row.Text, true).CountWords());
    }

    public static object FormatTime(TimeSpan time, DialogueListTimeCodeFormat format, double frameRate)
    {
        var sign = time < TimeSpan.Zero ? "-" : string.Empty;
        var t = time.Duration();
        switch (format)
        {
            case DialogueListTimeCodeFormat.Seconds:
                return Math.Round(time.TotalSeconds, 3);
            case DialogueListTimeCodeFormat.Milliseconds:
                return string.Format(CultureInfo.InvariantCulture, "{0}{1:00}:{2:00}:{3:00},{4:000}", sign, (int)t.TotalHours, t.Minutes, t.Seconds, t.Milliseconds);
            default:
                // Frames from the whole milliseconds like TimeCode.ToHHMMSSFF, so the list shows
                // the time codes SE shows (and SMPTE time code rates are honoured the same way)
                var rate = frameRate > 0 ? frameRate : 25;
                var frameCount = (int)Math.Round(SubtitleFormat.GetTimeCodeFrameRate(rate), MidpointRounding.AwayFromZero);
                var totalSeconds = (long)Math.Floor(t.TotalSeconds);
                var frames = SubtitleFormat.MillisecondsToFrames(t.Milliseconds, rate);
                if (frames >= frameCount)
                {
                    frames = 0;
                    totalSeconds++;
                }

                return string.Format(CultureInfo.InvariantCulture, "{0}{1:00}:{2:00}:{3:00}:{4:00}", sign, totalSeconds / 3600, totalSeconds / 60 % 60, totalSeconds % 60, frames);
        }
    }

    private static string FormatText(string text, DialogueListOptions options)
    {
        var s = text ?? string.Empty;
        if (options.RemoveFormatting)
        {
            s = HtmlUtil.RemoveHtmlTags(s, true);
        }

        var lines = s.SplitToLines();
        return string.Join(options.LineBreaksInCell ? "\n" : LineSeparator, lines);
    }

    private static double GetWidth(DialogueListColumn column) => column switch
    {
        DialogueListColumn.Number or DialogueListColumn.Layer or DialogueListColumn.Forced => 7,
        DialogueListColumn.Cps or DialogueListColumn.Wpm => 8,
        DialogueListColumn.Characters => 11,
        DialogueListColumn.Start or DialogueListColumn.End or DialogueListColumn.Duration or DialogueListColumn.Gap => 13,
        DialogueListColumn.Actor or DialogueListColumn.Style => 18,
        _ => 50,
    };
}
