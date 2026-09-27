using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Main.StylePicker;

public partial class StylePickerItem : ObservableObject
{
    // Stand-in for the video behind an outline style, so white and yellow text stay readable in
    // both light and dark themes.
    private static readonly IBrush VideoBackgroundBrush = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40));

    /// <summary>Style name - for the "new style" row, the name that will be created.</summary>
    public string Name { get; }

    public string DisplayName { get; }
    public string LineCountText { get; }
    public bool IsCurrent { get; }
    public bool IsNew { get; }

    /// <summary>One-line summary under the name, e.g. "Arial 20 · Bold · Bottom-Center".</summary>
    public string Summary { get; }

    // Preview chip: "Aa" drawn with the style's font and colors.
    public FontFamily PreviewFontFamily { get; }
    public FontWeight PreviewFontWeight { get; }
    public FontStyle PreviewFontStyle { get; }
    public IBrush PreviewForeground { get; }
    public IBrush PreviewBackground { get; }
    public IBrush PreviewBorderBrush { get; }

    // Details panel for the highlighted style.
    public string FontText { get; }
    public string AlignmentText { get; }
    public string BorderText { get; }
    public string OutlineWidthText { get; }
    public string ShadowWidthText { get; }
    public string MarginsText { get; }
    public string UsageText { get; }
    public IBrush PrimaryBrush { get; }
    public IBrush SecondaryBrush { get; }
    public IBrush OutlineBrush { get; }
    public IBrush ShadowBrush { get; }
    public string PrimaryHex { get; }
    public string SecondaryHex { get; }
    public string OutlineHex { get; }
    public string ShadowHex { get; }

    // Number key follows the position; dimmed once the number keys type into the filter box.
    [ObservableProperty] private string _numberText;
    [ObservableProperty] private string _shortcutText;
    [ObservableProperty] private bool _isNumberKeyActive;

    public StylePickerItem(SsaStyle style, int lineCount, bool isCurrent, bool isNew, bool isSsa)
    {
        Name = style.Name;
        DisplayName = isNew ? string.Format(Se.Language.General.StylePickerNewStyleX, style.Name) : style.Name;
        LineCountText = lineCount > 0 ? "(" + lineCount + ")" : string.Empty;
        IsCurrent = isCurrent;
        IsNew = isNew;

        var fontSize = style.FontSize.ToString("0.##", CultureInfo.CurrentCulture);
        var alignment = GetAlignmentName(style.Alignment, isSsa);
        var flags = GetFontFlags(style);

        var summaryParts = new List<string> { style.FontName + " " + fontSize };
        if (flags.Count > 0)
        {
            summaryParts.Add(string.Join(", ", flags));
        }

        if (!string.IsNullOrEmpty(alignment))
        {
            summaryParts.Add(alignment);
        }

        Summary = string.Join(" · ", summaryParts);

        FontText = flags.Count > 0
            ? style.FontName + ", " + fontSize + " (" + string.Join(", ", flags) + ")"
            : style.FontName + ", " + fontSize;
        AlignmentText = string.IsNullOrEmpty(alignment) ? style.Alignment : alignment + " (" + style.Alignment + ")";
        MarginsText = string.Format(Se.Language.General.StylePickerMarginsXYZ, style.MarginLeft, style.MarginRight, style.MarginVertical);
        BorderText = GetBorderStyleName(style.BorderStyle);
        OutlineWidthText = style.OutlineWidth.ToString("0.##", CultureInfo.CurrentCulture);
        ShadowWidthText = style.ShadowWidth.ToString("0.##", CultureInfo.CurrentCulture);
        UsageText = isNew ? Se.Language.General.StylePickerNewStyleInfo : string.Format(Se.Language.General.StylePickerLinesUsingStyleX, lineCount);

        PrimaryBrush = MakeOpaqueBrush(style.Primary);
        SecondaryBrush = MakeOpaqueBrush(style.Secondary);
        OutlineBrush = MakeOpaqueBrush(style.Outline);
        ShadowBrush = MakeOpaqueBrush(style.Background);
        PrimaryHex = style.Primary.ToHex(false);
        SecondaryHex = style.Secondary.ToHex(false);
        OutlineHex = style.Outline.ToHex(false);
        ShadowHex = style.Background.ToHex(false);

        PreviewFontFamily = string.IsNullOrWhiteSpace(style.FontName) ? FontFamily.Default : new FontFamily(style.FontName);
        PreviewFontWeight = style.Bold ? FontWeight.Bold : FontWeight.Normal;
        PreviewFontStyle = style.Italic ? FontStyle.Italic : FontStyle.Normal;
        PreviewForeground = PrimaryBrush;
        PreviewBorderBrush = OutlineBrush;

        // Box styles draw the "shadow" color as the box behind the text.
        PreviewBackground = IsBoxBorderStyle(style.BorderStyle) ? ShadowBrush : VideoBackgroundBrush;

        _numberText = string.Empty;
        _shortcutText = string.Empty;
        _isNumberKeyActive = true;
    }

    private static IBrush MakeOpaqueBrush(SkiaSharp.SKColor color)
    {
        // Alpha is shown in the hex tooltip, a see-through swatch would just look like a theme color.
        return new SolidColorBrush(Color.FromRgb(color.Red, color.Green, color.Blue));
    }

    private static List<string> GetFontFlags(SsaStyle style)
    {
        var flags = new List<string>();
        if (style.Bold)
        {
            flags.Add(Se.Language.General.Bold);
        }

        if (style.Italic)
        {
            flags.Add(Se.Language.General.Italic);
        }

        if (style.Underline)
        {
            flags.Add(Se.Language.General.Underline);
        }

        if (style.Strikeout)
        {
            flags.Add(Se.Language.General.Strikeout);
        }

        return flags;
    }

    private static bool IsBoxBorderStyle(string? borderStyle)
    {
        return borderStyle == "3" || borderStyle == "4";
    }

    private static string GetBorderStyleName(string? borderStyle)
    {
        return borderStyle switch
        {
            "3" => Se.Language.General.BoxPerLine,
            "4" => Se.Language.General.Box,
            _ => Se.Language.General.Outline,
        };
    }

    /// <summary>
    /// Alignment as a position name. ASS uses numpad values (1-9); SSA uses the legacy values
    /// 1-3 bottom, 5-7 top and 9-11 middle.
    /// </summary>
    internal static string GetAlignmentName(string? alignment, bool isSsa)
    {
        var value = (alignment ?? string.Empty).Trim();
        if (isSsa)
        {
            value = value switch
            {
                "5" => "7",
                "6" => "8",
                "7" => "9",
                "9" => "4",
                "10" => "5",
                "11" => "6",
                _ => value,
            };
        }

        return value switch
        {
            "1" => Se.Language.General.BottomLeft,
            "2" => Se.Language.General.BottomCenter,
            "3" => Se.Language.General.BottomRight,
            "4" => Se.Language.General.MiddleLeft,
            "5" => Se.Language.General.MiddleCenter,
            "6" => Se.Language.General.MiddleRight,
            "7" => Se.Language.General.TopLeft,
            "8" => Se.Language.General.TopCenter,
            "9" => Se.Language.General.TopRight,
            _ => string.Empty,
        };
    }
}
