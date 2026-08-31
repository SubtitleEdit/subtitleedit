using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextColor;

public partial class PickTeletextColorViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>
    /// True when the user picked "No color": remove the explicit color code so the line
    /// renders in the teletext default white without spending a character cell on a
    /// color code (37 vs 36 usable characters).
    /// </summary>
    public bool NoColorPressed { get; private set; }

    public Color SelectedColor { get; private set; }

    /// <summary>
    /// The teletext spacing attribute colors 0x00..0x07 - the only colors an EBU STL
    /// open subtitle can carry (see Ebu.GetNearestEbuColorCode). The exact corner hex
    /// values round-trip to the teletext codes without snapping.
    /// </summary>
    public List<TeletextColorItem> TeletextColors { get; } =
    [
        new TeletextColorItem("Black", Se.Language.General.ColorBlack, Color.FromRgb(0x00, 0x00, 0x00)),
        new TeletextColorItem("Red", Se.Language.General.ColorRed, Color.FromRgb(0xFF, 0x00, 0x00)),
        new TeletextColorItem("Green", Se.Language.General.ColorGreen, Color.FromRgb(0x00, 0xFF, 0x00)),
        new TeletextColorItem("Yellow", Se.Language.General.ColorYellow, Color.FromRgb(0xFF, 0xFF, 0x00)),
        new TeletextColorItem("Blue", Se.Language.General.ColorBlue, Color.FromRgb(0x00, 0x00, 0xFF)),
        new TeletextColorItem("Magenta", Se.Language.General.ColorMagenta, Color.FromRgb(0xFF, 0x00, 0xFF)),
        new TeletextColorItem("Cyan", Se.Language.General.ColorCyan, Color.FromRgb(0x00, 0xFF, 0xFF)),
        new TeletextColorItem("White", Se.Language.General.ColorWhite, Color.FromRgb(0xFF, 0xFF, 0xFF)),
    ];

    /// <summary>
    /// The half intensity colors of teletext CLUT 1, plus gray - reachable in a full teletext
    /// stream (DVB teletext, Level 2.5) through an X/26 foreground color triplet, but not in an
    /// EBU STL file. The hex values are the map defaults widened the 0x11-step way, so they
    /// round-trip without touching a redefinable colour map entry.
    /// </summary>
    public List<TeletextColorItem> HalfIntensityColors { get; } =
    [
        new TeletextColorItem("HalfRed", Se.Language.General.ColorHalfRed, Color.FromRgb(0x77, 0x00, 0x00)),
        new TeletextColorItem("HalfGreen", Se.Language.General.ColorHalfGreen, Color.FromRgb(0x00, 0x77, 0x00)),
        new TeletextColorItem("HalfYellow", Se.Language.General.ColorHalfYellow, Color.FromRgb(0x77, 0x77, 0x00)),
        new TeletextColorItem("HalfBlue", Se.Language.General.ColorHalfBlue, Color.FromRgb(0x00, 0x00, 0x77)),
        new TeletextColorItem("HalfMagenta", Se.Language.General.ColorHalfMagenta, Color.FromRgb(0x77, 0x00, 0x77)),
        new TeletextColorItem("HalfCyan", Se.Language.General.ColorHalfCyan, Color.FromRgb(0x00, 0x77, 0x77)),
        new TeletextColorItem("Gray", Se.Language.General.ColorGray, Color.FromRgb(0x77, 0x77, 0x77)),
    ];

    /// <summary>
    /// Level 2.5 teletext (DVB teletext): also offer the CLUT 1 half intensity colors and a
    /// free color choice - the writer puts a free color in a redefinable colour map entry.
    /// </summary>
    public bool IsLevel25 { get; private set; }

    /// <summary>
    /// True when the user asked for the full color picker instead of a palette color.
    /// </summary>
    public bool CustomPressed { get; private set; }

    /// <summary>
    /// The teletext color the selected line currently uses, or null when it has no
    /// explicit color code ("no color" is the current state).
    /// </summary>
    public Color? CurrentColor { get; private set; }

    // EBU STL import writes named tags (<font color="Red">), SE's color service writes
    // hex tags (<font color="#ff0000">) - the current-color highlight must match both.
    private static readonly Regex FontColorTag = new Regex("<font\\s+color\\s*=\\s*\"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public void Initialize(string? currentText)
    {
        Initialize(currentText, level25: false);
    }

    public void Initialize(string? currentText, bool level25)
    {
        IsLevel25 = level25;
        CurrentColor = null;
        if (string.IsNullOrEmpty(currentText))
        {
            return;
        }

        var match = FontColorTag.Match(currentText);
        if (!match.Success)
        {
            return;
        }

        var value = match.Groups[1].Value.Trim().TrimStart('#');
        var items = level25 ? TeletextColors.Concat(HalfIntensityColors) : TeletextColors.AsEnumerable();
        foreach (var item in items)
        {
            var hex = $"{item.Color.R:X2}{item.Color.G:X2}{item.Color.B:X2}";
            if (value.Equals(item.EnglishName, StringComparison.OrdinalIgnoreCase) ||
                value.Equals(hex, StringComparison.OrdinalIgnoreCase))
            {
                CurrentColor = item.Color;
                return;
            }
        }
    }

    [RelayCommand]
    private void PickColor(TeletextColorItem item)
    {
        SelectedColor = item.Color;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void PickNoColor()
    {
        NoColorPressed = true;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void PickCustomColor()
    {
        CustomPressed = true;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
