using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// The subtitle preview for the ffmpeg player, as plain lines with times. mpv and VLC render
/// the preview themselves from an ASS file; this player draws it in
/// <see cref="FfmpegSoftwareControl"/>, so the snapshot only keeps what that overlay can show:
/// text with line breaks, italic/bold runs, and whether a line belongs to the secondary
/// subtitle (drawn at the top, the primary at the configured alignment).
/// </summary>
public sealed class FfmpegPreviewSubtitle
{
    /// <summary>A part of <see cref="Line.Text"/> shown italic and/or bold.</summary>
    public readonly record struct StyleRun(int Start, int Length, bool Italic, bool Bold);

    public sealed record Line(double StartSeconds, double EndSeconds, string Text, StyleRun[] Runs, bool Secondary);

    // From a \p1 (or higher) block to the \p0 block that ends the drawing, or else the end of the text.
    private static readonly Regex DrawingRegex = new(@"\{[^{}]*\\p[1-9][^{}]*\}.*?(?=\{[^{}]*\\p0[^{}]*\}|\z)", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex StyleTagRegex = new(@"</?[ibIB]>|\{\\[^{}]*\}", RegexOptions.Compiled);
    private static readonly Regex AssStyleOverrideRegex = new(@"\\([ib])(\d*)(?=[\\}])", RegexOptions.Compiled);

    public static readonly FfmpegPreviewSubtitle Empty = new(Array.Empty<Line>());

    private readonly Line[] _lines; // sorted by start
    private readonly double[] _starts;
    private readonly double[] _maxEndSoFar; // _maxEndSoFar[i] = latest end among _lines[0..i]

    private FfmpegPreviewSubtitle(Line[] lines)
    {
        _lines = lines;
        _starts = new double[lines.Length];
        _maxEndSoFar = new double[lines.Length];
        var maxEnd = double.MinValue;
        for (var i = 0; i < lines.Length; i++)
        {
            _starts[i] = lines[i].StartSeconds;
            maxEnd = Math.Max(maxEnd, lines[i].EndSeconds);
            _maxEndSoFar[i] = maxEnd;
        }
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
            var text = ToPlainText(p.Text, out var runs);
            if (string.IsNullOrWhiteSpace(text) || p.EndTime.TotalSeconds <= p.StartTime.TotalSeconds)
            {
                continue;
            }

            lines.Add(new Line(p.StartTime.TotalSeconds, p.EndTime.TotalSeconds, text, runs, secondary));
        }
    }

    /// <summary>
    /// Strips HTML and ASS tags and ASS drawings. Italic and bold survive as runs, because a
    /// tag rarely wraps the whole text: "&lt;i&gt;one&lt;/i&gt; two", or one line of two in italic.
    /// </summary>
    internal static string ToPlainText(string text, out StyleRun[] runs)
    {
        var source = text.Contains("\\p", StringComparison.Ordinal) ? DrawingRegex.Replace(text, string.Empty) : text;

        var plain = new StringBuilder(source.Length);
        var styled = new List<StyleRun>();
        var italic = false;
        var bold = false;
        var position = 0;
        foreach (Match tag in StyleTagRegex.Matches(source))
        {
            AppendPlain(source.Substring(position, tag.Index - position));
            position = tag.Index + tag.Length;
            ApplyStyleTag(tag.Value, ref italic, ref bold);
        }

        AppendPlain(source.Substring(position));

        var result = plain.ToString();
        var trimmed = result.Trim();
        var leading = result.Length - result.TrimStart().Length;
        runs = styled
            .Select(r => ClipRun(r, leading, trimmed.Length))
            .Where(r => r.Length > 0)
            .ToArray();
        return trimmed;

        void AppendPlain(string part)
        {
            var value = HtmlUtil.RemoveHtmlTags(part, true)
                .Replace("\\N", Environment.NewLine)
                .Replace("\\n", Environment.NewLine)
                .Replace("\\h", " ");
            if (value.Length == 0)
            {
                return;
            }

            if (italic || bold)
            {
                var last = styled.Count - 1;
                if (last >= 0 && styled[last].Italic == italic && styled[last].Bold == bold && styled[last].Start + styled[last].Length == plain.Length)
                {
                    styled[last] = styled[last] with { Length = styled[last].Length + value.Length };
                }
                else
                {
                    styled.Add(new StyleRun(plain.Length, value.Length, italic, bold));
                }
            }

            plain.Append(value);
        }
    }

    private static StyleRun ClipRun(StyleRun run, int leading, int textLength)
    {
        var start = Math.Max(0, run.Start - leading);
        var end = Math.Min(textLength, run.Start + run.Length - leading);
        return run with { Start = start, Length = end - start };
    }

    private static void ApplyStyleTag(string tag, ref bool italic, ref bool bold)
    {
        if (tag[0] == '<')
        {
            var on = tag[1] != '/';
            if (char.ToLowerInvariant(tag[^2]) == 'i')
            {
                italic = on;
            }
            else
            {
                bold = on;
            }

            return;
        }

        foreach (Match match in AssStyleOverrideRegex.Matches(tag))
        {
            // \b700 is a weight; \i and \b without a value go back to the style, which is regular here.
            var value = match.Groups[2].Value;
            var on = value.Length > 0 && value != "0";
            if (match.Groups[1].Value == "i")
            {
                italic = on;
            }
            else
            {
                bold = on;
            }
        }
    }

    /// <summary>Lines showing at <paramref name="seconds"/>, in start order.</summary>
    /// <remarks>
    /// Called for every video frame, so it must not walk the whole subtitle: lines starting
    /// after the position are skipped by binary search, and the backward walk stops at the
    /// first line before which nothing ends after the position.
    /// </remarks>
    public List<Line> GetActive(double seconds)
    {
        var result = new List<Line>();

        // First index whose start is after the position.
        var end = Array.BinarySearch(_starts, seconds);
        if (end < 0)
        {
            end = ~end;
        }
        else
        {
            while (end < _starts.Length && _starts[end] <= seconds)
            {
                end++;
            }
        }

        for (var i = end - 1; i >= 0 && _maxEndSoFar[i] > seconds; i--)
        {
            if (seconds < _lines[i].EndSeconds)
            {
                result.Add(_lines[i]);
            }
        }

        result.Reverse();
        return result;
    }
}
