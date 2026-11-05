using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// The subtitle preview for the ffmpeg player, as plain lines with times. mpv and VLC render
/// the preview themselves from an ASS file; this player draws it in
/// <see cref="FfmpegSoftwareControl"/>, so the snapshot only keeps what that overlay can show:
/// text with line breaks, italic/bold/color runs, a line's own "\an" alignment, and whether a
/// line belongs to the secondary subtitle (drawn with that subtitle's own style).
/// </summary>
public sealed class FfmpegPreviewSubtitle
{
    /// <summary>A part of <see cref="Line.Text"/> shown italic, bold and/or in a color of its own.</summary>
    public readonly record struct StyleRun(int Start, int Length, bool Italic, bool Bold, Color? Color = null);

    /// <param name="Alignment">The line's own "\an" (1-9), null for the style's alignment.</param>
    public sealed record Line(double StartSeconds, double EndSeconds, string Text, StyleRun[] Runs, bool Secondary, int? Alignment = null);

    /// <summary>
    /// How a block of text is drawn. Sizes are in the units of <see cref="SeVideo.MpvPreviewFontSize"/>
    /// (<see cref="AdvancedSubStationAlpha.DefaultHeight"/> tall). <see cref="BorderStyle"/> is the
    /// ASSA value: 1 outline, 3 a box per line, 4 one box.
    /// </summary>
    public sealed record TextStyle(
        string FontName,
        decimal FontSize,
        bool Bold,
        decimal Margin,
        Color Primary,
        Color Outline,
        Color Shadow,
        decimal OutlineWidth,
        decimal ShadowWidth,
        int Alignment,
        int BorderStyle,
        string Justify)
    {
        public static TextStyle FromSettings(SeVideo settings) => new(
            settings.MpvPreviewFontName,
            settings.MpvPreviewFontSize,
            settings.MpvPreviewFontBold,
            settings.MpvPreviewMargin,
            settings.MpvPreviewColorPrimary.FromHexToColor(),
            settings.MpvPreviewColorOutline.FromHexToColor(),
            settings.MpvPreviewColorShadow.FromHexToColor(),
            settings.MpvPreviewOutlineWidth,
            settings.MpvPreviewShadowWidth,
            ParseAlignment(settings.MpvPreviewAlignment),
            settings.MpvPreviewBorderType,
            settings.MpvPreviewJustify);

        /// <summary>
        /// The style the second subtitle was built with, which sits in its own header at its own
        /// PlayRes - see <see cref="SecondarySubtitleMerger"/>. Null when it has no such style.
        /// </summary>
        /// <remarks>
        /// Justify is not in the header: mpv gets it as lines with a baked-in "\pos" (see
        /// <see cref="SecondarySubtitleJustifier"/>), so only the saved setting is known here.
        /// </remarks>
        internal static TextStyle? FromSecondary(Subtitle secondary, SeVideo settings)
        {
            var styleName = secondary.Paragraphs.FirstOrDefault()?.Extra;
            if (string.IsNullOrEmpty(styleName) || string.IsNullOrEmpty(secondary.Header) ||
                !secondary.Header.Contains(styleName, StringComparison.Ordinal))
            {
                return null;
            }

            var playResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", secondary.Header);
            if (!int.TryParse(playResY, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) || height <= 0)
            {
                height = AdvancedSubStationAlpha.DefaultHeight;
            }

            var unit = (decimal)AdvancedSubStationAlpha.DefaultHeight / height;
            var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, secondary.Header);
            return new TextStyle(
                style.FontName,
                style.FontSize * unit,
                style.Bold,
                style.MarginVertical * unit,
                style.Primary.ToAvaloniaColor(),
                style.Outline.ToAvaloniaColor(),
                style.Background.ToAvaloniaColor(),
                style.OutlineWidth * unit,
                style.ShadowWidth * unit,
                ParseAlignment(style.Alignment),
                int.TryParse(style.BorderStyle, NumberStyles.Integer, CultureInfo.InvariantCulture, out var borderStyle) ? borderStyle : 1,
                settings.SecondarySubtitleOverrideStyle ? settings.SecondarySubtitleJustify : "auto");
        }

        private static int ParseAlignment(string? alignment)
        {
            return int.TryParse(alignment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var a) && a is >= 1 and <= 9 ? a : 2;
        }
    }

    // From a \p1 (or higher) block to the \p0 block that ends the drawing, or else the end of the text.
    private static readonly Regex DrawingRegex = new(@"\{[^{}]*\\p[1-9][^{}]*\}.*?(?=\{[^{}]*\\p0[^{}]*\}|\z)", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex StyleTagRegex = new(@"</?[ibIB]>|<font\b[^>]*>|</font>|\{\\[^{}]*\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AssStyleOverrideRegex = new(@"\\(?:([ib])(\d*)|1?c(&H[0-9a-fA-F]+&?)?|an([1-9]))(?=[\\}])", RegexOptions.Compiled);
    private static readonly Regex FontColorRegex = new(@"color\s*=\s*[""']?([^""'\s>]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly FfmpegPreviewSubtitle Empty = new(Array.Empty<Line>(), null);

    private readonly Line[] _lines; // sorted by start
    private readonly double[] _starts;
    private readonly double[] _maxEndSoFar; // _maxEndSoFar[i] = latest end among _lines[0..i]

    private FfmpegPreviewSubtitle(Line[] lines, TextStyle? secondaryStyle)
    {
        _lines = lines;
        SecondaryStyle = secondaryStyle;
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

    /// <summary>The second subtitle's own style, null to draw it like the primary at the opposite edge.</summary>
    public TextStyle? SecondaryStyle { get; }

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

        var secondaryStyle = secondary != null ? TextStyle.FromSecondary(secondary, Se.Settings.Video) : null;
        return new FfmpegPreviewSubtitle(lines.OrderBy(l => l.StartSeconds).ToArray(), secondaryStyle);
    }

    private static void Add(List<Line> lines, IEnumerable<Paragraph> paragraphs, bool secondary, bool smpteMode)
    {
        foreach (var paragraph in paragraphs)
        {
            var p = smpteMode ? SmptePreviewStretch.Stretched(paragraph) : paragraph;
            var text = ToPlainText(p.Text, out var runs, out var alignment);
            if (string.IsNullOrWhiteSpace(text) || p.EndTime.TotalSeconds <= p.StartTime.TotalSeconds)
            {
                continue;
            }

            lines.Add(new Line(p.StartTime.TotalSeconds, p.EndTime.TotalSeconds, text, runs, secondary, alignment));
        }
    }

    /// <summary>
    /// Strips HTML and ASS tags and ASS drawings. Italic, bold and color survive as runs, because
    /// a tag rarely wraps the whole text: "&lt;i&gt;one&lt;/i&gt; two", or one line of two in italic.
    /// </summary>
    /// <param name="alignment">
    /// The "\an" that places the line. Null when it has none, and for a line with "\pos" or
    /// "\move": that "\an" only says how the line hangs from its point, which isn't drawn here
    /// (a justified second subtitle is "\an7" lines, which would end up in the top left corner).
    /// </param>
    internal static string ToPlainText(string text, out StyleRun[] runs, out int? alignment)
    {
        // Most lines have no markup, and the whole subtitle comes through here on the UI thread
        // after every settled edit.
        if (text.AsSpan().IndexOfAny('<', '{', '\\') < 0)
        {
            runs = [];
            alignment = null;
            return text.Trim();
        }

        var source = text.Contains("\\p", StringComparison.Ordinal) ? DrawingRegex.Replace(text, string.Empty) : text;

        var plain = new StringBuilder(source.Length);
        var styled = new List<StyleRun>();
        var state = new TagState();
        var position = 0;
        foreach (Match tag in StyleTagRegex.Matches(source))
        {
            AppendPlain(source.Substring(position, tag.Index - position));
            position = tag.Index + tag.Length;
            state.Apply(tag.Value);
        }

        AppendPlain(source.Substring(position));

        var result = plain.ToString();
        var trimmed = result.Trim();
        var leading = result.Length - result.TrimStart().Length;
        runs = styled
            .Select(r => ClipRun(r, leading, trimmed.Length))
            .Where(r => r.Length > 0)
            .ToArray();
        alignment = source.Contains("\\pos(", StringComparison.Ordinal) || source.Contains("\\move(", StringComparison.Ordinal)
            ? null
            : state.Alignment;
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

            if (state.Italic || state.Bold || state.Color.HasValue)
            {
                var last = styled.Count - 1;
                if (last >= 0 && styled[last].Italic == state.Italic && styled[last].Bold == state.Bold &&
                    styled[last].Color == state.Color && styled[last].Start + styled[last].Length == plain.Length)
                {
                    styled[last] = styled[last] with { Length = styled[last].Length + value.Length };
                }
                else
                {
                    styled.Add(new StyleRun(plain.Length, value.Length, state.Italic, state.Bold, state.Color));
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

    /// <summary>What the tags so far have switched on.</summary>
    private sealed class TagState
    {
        private readonly Stack<Color?> _fontColors = new();

        public bool Italic { get; private set; }
        public bool Bold { get; private set; }
        public Color? Color { get; private set; }
        public int? Alignment { get; private set; }

        public void Apply(string tag)
        {
            if (tag[0] == '{')
            {
                ApplyAssTags(tag);
            }
            else if (tag.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
            {
                _fontColors.Push(Color);
                var color = FontColorRegex.Match(tag);
                if (color.Success)
                {
                    Color = HtmlUtil.GetColorFromString(color.Groups[1].Value).ToAvaloniaColor();
                }
            }
            else if (tag.Equals("</font>", StringComparison.OrdinalIgnoreCase))
            {
                Color = _fontColors.Count > 0 ? _fontColors.Pop() : null;
            }
            else if (char.ToLowerInvariant(tag[^2]) == 'i')
            {
                Italic = tag[1] != '/';
            }
            else
            {
                Bold = tag[1] != '/';
            }
        }

        private void ApplyAssTags(string block)
        {
            foreach (Match match in AssStyleOverrideRegex.Matches(block))
            {
                if (match.Groups[1].Success)
                {
                    // \b700 is a weight; \i and \b without a value go back to the style, which is regular here.
                    var value = match.Groups[2].Value;
                    var on = value.Length > 0 && value != "0";
                    if (match.Groups[1].Value == "i")
                    {
                        Italic = on;
                    }
                    else
                    {
                        Bold = on;
                    }
                }
                else if (match.Groups[4].Success)
                {
                    // Like libass: the first one counts.
                    Alignment ??= match.Groups[4].Value[0] - '0';
                }
                else
                {
                    // \c without a value goes back to the style's color.
                    Color = match.Groups[3].Success
                        ? AdvancedSubStationAlpha.GetSsaColor(match.Groups[3].Value, SKColors.White).ToAvaloniaColor()
                        : null;
                }
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
