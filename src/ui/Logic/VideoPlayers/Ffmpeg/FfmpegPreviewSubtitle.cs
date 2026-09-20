using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// The subtitle preview for the ffmpeg player, as plain lines with times. mpv and VLC render
/// the preview themselves from an ASS file; this player draws it in
/// <see cref="FfmpegSoftwareControl"/>, so the snapshot only keeps what that overlay can show:
/// text with line breaks, whole-line italic, and whether a line belongs to the secondary
/// subtitle (drawn at the top, the primary at the configured alignment).
/// </summary>
public sealed class FfmpegPreviewSubtitle
{
    public sealed record Line(double StartSeconds, double EndSeconds, string Text, bool Italic, bool Secondary);

    public static readonly FfmpegPreviewSubtitle Empty = new(Array.Empty<Line>());

    private readonly Line[] _lines; // sorted by start

    private FfmpegPreviewSubtitle(Line[] lines)
    {
        _lines = lines;
    }

    public int Count => _lines.Length;

    /// <summary>
    /// Snapshot of <paramref name="subtitle"/> (plus the secondary subtitle when given). The
    /// subtitle is copied, so the caller can keep editing it; in SMPTE mode the times get the
    /// same 1001/1000 stretch the mpv preview applies.
    /// </summary>
    public static FfmpegPreviewSubtitle Build(Subtitle subtitle, Subtitle? secondary, bool smpteMode)
    {
        var lines = new List<Line>(subtitle.Paragraphs.Count + (secondary?.Paragraphs.Count ?? 0));
        Add(lines, subtitle.Paragraphs, false, smpteMode);
        if (secondary != null)
        {
            Add(lines, secondary.Paragraphs, true, smpteMode);
        }

        return new FfmpegPreviewSubtitle(lines.OrderBy(l => l.StartSeconds).ToArray());
    }

    private static void Add(List<Line> lines, IEnumerable<Paragraph> paragraphs, bool secondary, bool smpteMode)
    {
        foreach (var paragraph in paragraphs)
        {
            var p = smpteMode ? SmptePreviewStretch.Stretched(paragraph) : paragraph;
            var text = ToPlainText(p.Text, out var italic);
            if (string.IsNullOrWhiteSpace(text) || p.EndTime.TotalSeconds <= p.StartTime.TotalSeconds)
            {
                continue;
            }

            lines.Add(new Line(p.StartTime.TotalSeconds, p.EndTime.TotalSeconds, text, italic, secondary));
        }
    }

    /// <summary>Strips HTML and ASS tags; italic survives only when it wraps the whole text.</summary>
    internal static string ToPlainText(string text, out bool italic)
    {
        var trimmed = text.Trim();
        italic = trimmed.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) &&
                 trimmed.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) &&
                 trimmed.IndexOf("</i>", StringComparison.OrdinalIgnoreCase) == trimmed.Length - 4;
        if (!italic)
        {
            italic = trimmed.StartsWith("{\\i1}", StringComparison.Ordinal) && !trimmed.Contains("{\\i0}", StringComparison.Ordinal);
        }

        var plain = HtmlUtil.RemoveHtmlTags(trimmed, true);
        plain = plain.Replace("\\N", Environment.NewLine).Replace("\\n", Environment.NewLine);
        return plain.Trim();
    }

    /// <summary>Lines showing at <paramref name="seconds"/>, in start order.</summary>
    public List<Line> GetActive(double seconds)
    {
        var result = new List<Line>();
        foreach (var line in _lines)
        {
            if (line.StartSeconds > seconds)
            {
                break;
            }

            if (seconds < line.EndSeconds)
            {
                result.Add(line);
            }
        }

        return result;
    }
}
