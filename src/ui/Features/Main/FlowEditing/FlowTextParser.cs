using System;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Nikse.SubtitleEdit.Features.Main.FlowEditing;

public static partial class FlowTextParser
{
    private static readonly Regex FontColorRegex = new(
        "<font\\s+[^>]*color\\s*=\\s*[\\\"']?(?<color>#[0-9a-fA-F]{6}|#[0-9a-fA-F]{8}|[a-zA-Z]+)[\\\"']?[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex FontTagRegex = new(
        "</?font\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex BoxTagRegex = new(
        "</?box\\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AlignmentTagRegex = new(
        "\\{\\\\an[1-9]\\}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static FlowTextInfo Parse(string? text)
    {
        var source =
            text ?? string.Empty;

        var colorMatch =
            FontColorRegex.Match(
                source);

        var colorToken =
            colorMatch.Success
                ? colorMatch.Groups["color"].Value
                : null;

        var alignmentMatch =
            AlignmentTagRegex.Match(
                source);

        var alignmentToken =
            alignmentMatch.Success
                ? alignmentMatch.Value
                : null;

        var cleanText =
            FontTagRegex.Replace(
                source,
                string.Empty);

        cleanText = BoxTagRegex.Replace(cleanText, string.Empty);

        cleanText =
            AlignmentTagRegex.Replace(
                cleanText,
                string.Empty);

        return new FlowTextInfo(
            cleanText,
            colorToken,
            ToBrush(colorToken),
            alignmentToken);
    }

    public static string ApplyEditedText(
        string? originalText,
        string editedText)
    {
        var parsed =
            Parse(
                originalText);

        var result =
            editedText;

        // "No color" is a valid EBU/Teletext state. Do not invent a font tag:
        // keeping ColorToken null is what makes the normal 37-character rule
        // apply instead of the 36-character colour-code rule.
        if (!string.IsNullOrWhiteSpace(
                parsed.ColorToken))
        {
            result =
                $"<font color=\"{parsed.ColorToken}\">{result}</font>";
        }

        if (!string.IsNullOrWhiteSpace(
                parsed.AlignmentToken))
        {
            result =
                parsed.AlignmentToken +
                result;
        }

        return result;
    }

    private static IBrush ToBrush(
        string? colorToken)
    {
        // "No color" must still be visible in Flow. This display fallback is
        // intentionally NOT written back into the subtitle, so the underlying
        // subtitle remains colorless and continues to use the 37-char rule.
        if (string.IsNullOrWhiteSpace(
                colorToken))
        {
            return GetDefaultFlowForeground();
        }

        try
        {
            return new SolidColorBrush(
                Color.Parse(
                    colorToken));
        }
        catch
        {
            return colorToken.ToLowerInvariant() switch
            {
                "white" => Brushes.White,
                "yellow" => Brushes.Yellow,
                "cyan" => Brushes.Cyan,
                "red" => Brushes.Red,
                "green" => Brushes.Green,
                "blue" => Brushes.Blue,
                "magenta" => Brushes.Magenta,
                "black" => Brushes.Black,
                _ => GetDefaultFlowForeground(),
            };
        }
    }

    private static IBrush GetDefaultFlowForeground()
    {
        var isDark =
            Application.Current?.ActualThemeVariant ==
            ThemeVariant.Dark;

        return isDark
            ? Brushes.White
            : Brushes.Black;
    }
}

public sealed record FlowTextInfo(
    string Text,
    string? ColorToken,
    IBrush? Foreground,
    string? AlignmentToken);
