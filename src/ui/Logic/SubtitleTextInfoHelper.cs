using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic;

internal static class SubtitleTextInfoHelper
{
    private static IBrush _errorBrush = MakeErrorBrush();
    private static readonly IBrush _transparentBrush = Brushes.Transparent;

    internal static void RefreshErrorBrush()
        => _errorBrush = MakeErrorBrush();

    private static IBrush MakeErrorBrush()
        => new SolidColorBrush(Se.Settings.General.ErrorColor.FromHexToColor());

    /// <summary>
    /// Populates <paramref name="panel"/> with per-line character count labels and
    /// writes formatted CPS / total-length strings with their error backgrounds to
    /// the four out parameters.
    /// </summary>
    /// <param name="text">Raw subtitle text (HTML tags are stripped internally).</param>
    /// <param name="item">Provides timing for the CPS calculation.</param>
    internal static void Populate(
        string text,
        SubtitleLineViewModel item,
        StackPanel panel,
        out string cpsText,
        out IBrush cpsBackground,
        out string totalText,
        out IBrush totalBackground)
    {
        // This runs on every keystroke in the edit box, right after the row view model's own
        // bindings (text error tint, pixel width) already stripped and split the same text.
        // Reuse that memo instead of stripping/splitting a second time; the guard keeps the
        // fallback for callers passing a different string than the row's current text.
        TextTotalInfo info;
        if (ReferenceEquals(text, item.Text))
        {
            info = PopulateLineLengthsAndTotalCore(item.GetStrippedText(), item.GetStrippedLines(), panel);
        }
        else
        {
            info = PopulateLineLengthsAndTotal(text, panel);
        }

        var maxCps = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;

        var cps = GetCharactersPerSecond(text, item.StartTime, item.EndTime);
        cpsText = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        cpsBackground = Se.Settings.General.ColorCharactersPerSecond && cps > maxCps ? _errorBrush : _transparentBrush;
        totalText = info.TotalText;
        totalBackground = info.TotalBackground;
    }

    internal static TextTotalInfo PopulateLineLengthsAndTotal(string text, StackPanel panel)
    {
        var cleanText = StripHtml(text);
        return PopulateLineLengthsAndTotalCore(cleanText, cleanText.SplitToLines(), panel);
    }

    private static TextTotalInfo PopulateLineLengthsAndTotalCore(string cleanText, List<string> lines, StackPanel panel)
    {
        var colorTextTooLong = Se.Settings.General.ColorTextTooLong;
        var maxLineLength = Se.Settings.General.SubtitleLineMaximumLength;

        var totalLength = GetTotalLength(cleanText);
        var lineCount = lines.Count;

        FillLineLengthPanel(panel, lines, colorTextTooLong, maxLineLength);

        var totalBackground = colorTextTooLong && totalLength > maxLineLength * lineCount
            ? _errorBrush
            : _transparentBrush;
        return new TextTotalInfo(
            totalLength,
            string.Format(Se.Language.Main.TotalCharacters, totalLength),
            totalBackground);
    }

    /// <summary>
    /// Fills <paramref name="panel"/> with the "Single line length: 12 / 30" labels, reusing the
    /// text blocks that are already there.
    /// </summary>
    /// <remarks>
    /// This runs on every keystroke in the edit box (and on every selection change), so the old
    /// Clear + rebuild threw away a handful of TextBlocks and re-added them to the logical and
    /// visual tree each time. Rewriting Text/Background in place keeps the tree untouched, so
    /// typing only costs a measure of the labels that actually changed.
    /// </remarks>
    internal static void FillLineLengthPanel(StackPanel panel, List<string> lines, bool colorTextTooLong, int maxLineLength)
    {
        var children = panel.Children;
        var needed = 1 + lines.Count + Math.Max(0, lines.Count - 1);
        while (children.Count > needed)
        {
            children.RemoveAt(children.Count - 1);
        }

        var index = 0;
        SetLabel(children, ref index, Se.Language.Main.SingleLineLength, null);
        for (var i = 0; i < lines.Count; i++)
        {
            if (i > 0)
            {
                SetLabel(children, ref index, "/", null);
            }

            var lineLength = GetLineLength(lines[i]);
            SetLabel(children,
                ref index,
                lineLength.ToString(CultureInfo.InvariantCulture),
                colorTextTooLong && lineLength > maxLineLength ? _errorBrush : null);
        }
    }

    private const double LabelFontSize = 12;
    private static readonly Thickness LabelPadding = new Thickness(2);

    private static void SetLabel(Avalonia.Controls.Controls children, ref int index, string text, IBrush? background)
    {
        if (index < children.Count && children[index] is TextBlock existing)
        {
            if (existing.Text != text)
            {
                existing.Text = text;
            }

            if (!ReferenceEquals(existing.Background, background))
            {
                existing.Background = background;
            }

            // The panel may have been built with differently styled text blocks (e.g. a bold
            // header seeded by the layout code); reused blocks must still look like our own.
            if (existing.FontWeight != FontWeight.Normal)
            {
                existing.FontWeight = FontWeight.Normal;
            }

            if (Math.Abs(existing.FontSize - LabelFontSize) > 0.001)
            {
                existing.FontSize = LabelFontSize;
            }

            if (existing.Padding != LabelPadding)
            {
                existing.Padding = LabelPadding;
            }

            if (existing.Margin != default)
            {
                existing.Margin = default;
            }
        }
        else
        {
            var tb = UiUtil.MakeTextBlock(text).WithFontSize(LabelFontSize).WithPadding(2);
            tb.Background = background;
            if (index < children.Count)
            {
                children[index] = tb; // a foreign control ended up in the panel - replace it
            }
            else
            {
                children.Add(tb);
            }
        }

        index++;
    }

    internal static void UpdateGaps(IList<SubtitleLineViewModel> items)
    {
        if (items.Count == 0) return;
        items[0].PreviousGap = double.MaxValue;
        for (var i = 0; i < items.Count - 1; i++)
        {
            var p = items[i];
            var next = items[i + 1];
            p.Gap = next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds;
            next.PreviousGap = p.Gap;
        }
        items[items.Count - 1].Gap = double.MaxValue;
    }

    internal static string StripHtml(string text)
        => HtmlUtil.RemoveHtmlTags(text, true);

    internal static double GetTotalLength(string text)
        => (double)text.CountCharacters(false);

    internal static int GetLineLength(string line)
        => (int)line.CountCharacters(false);

    internal static double GetCharactersPerSecond(string text, TimeSpan startTime, TimeSpan endTime)
    {
        var duration = endTime.TotalMilliseconds - startTime.TotalMilliseconds;
        if (duration <= 0.001)
        {
            return 0.0;
        }

        // No StripHtml here: every ICalcLength implementation strips html/ssa tags
        // itself, so pre-stripping would run RemoveHtmlTags twice per call.
        return (double)text.CountCharacters(forCps: true) / (duration / 1000.0);
    }

    internal sealed record TextTotalInfo(double TotalLength, string TotalText, IBrush TotalBackground);
}
