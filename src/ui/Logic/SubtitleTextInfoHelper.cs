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
        var info = PopulateLineLengthsAndTotal(text, panel);
        var colorTextTooLong = Se.Settings.General.ColorTextTooLong;
        var maxCps = Se.Settings.General.SubtitleMaximumCharactersPerSeconds;

        var cps = GetCharactersPerSecond(text, item.StartTime, item.EndTime);
        cpsText = string.Format(Se.Language.Main.CharactersPerSecond, $"{cps:0.0}");
        cpsBackground = colorTextTooLong && cps > maxCps ? _errorBrush : _transparentBrush;
        totalText = info.TotalText;
        totalBackground = info.TotalBackground;
    }

    internal static TextTotalInfo PopulateLineLengthsAndTotal(string text, StackPanel panel)
    {
        var colorTextTooLong = Se.Settings.General.ColorTextTooLong;
        var maxLineLength = Se.Settings.General.SubtitleLineMaximumLength;

        var cleanText = StripHtml(text);
        var totalLength = GetTotalLength(cleanText);
        var lines = cleanText.SplitToLines();
        var lineCount = lines.Count;

        var children = new List<Control>(1 + lineCount + Math.Max(0, lineCount - 1));
        children.Add(UiUtil.MakeTextBlock(Se.Language.Main.SingleLineLength).WithFontSize(12).WithPadding(2));
        for (var i = 0; i < lineCount; i++)
        {
            if (i > 0)
            {
                children.Add(UiUtil.MakeTextBlock("/").WithFontSize(12).WithPadding(2));
            }

            var lineLength = GetLineLength(lines[i]);
            var tb = UiUtil.MakeTextBlock(lineLength.ToString(CultureInfo.InvariantCulture)).WithFontSize(12).WithPadding(2);
            if (colorTextTooLong && lineLength > maxLineLength)
            {
                tb.Background = _errorBrush;
            }

            children.Add(tb);
        }

        panel.Children.Clear();
        panel.Children.AddRange(children);

        var totalBackground = colorTextTooLong && totalLength > maxLineLength * lineCount
            ? _errorBrush
            : _transparentBrush;
        return new TextTotalInfo(
            totalLength,
            string.Format(Se.Language.Main.TotalCharacters, totalLength),
            totalBackground);
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

        return (double)StripHtml(text).CountCharacters(forCps: true) / (duration / 1000.0);
    }

    internal sealed record TextTotalInfo(double TotalLength, string TotalText, IBrush TotalBackground);
}
