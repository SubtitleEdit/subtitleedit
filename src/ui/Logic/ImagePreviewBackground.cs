using Avalonia.Controls;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The backdrop behind subtitle bitmap thumbnails, shared by the OCR window and the binary
/// (Blu-ray sup / VobSub) edit window.
///
/// Subtitle bitmaps are light or dark text on a transparent background, so they need a backdrop
/// of their own or they vanish into the row. A checkerboard was tried and dropped for the OCR
/// grid (#12692): at thumbnail size the tiling competes with the glyphs. The binary edit window
/// kept it, and there the light half of the mid-gray checkerboard left light subtitles unreadable
/// (#14328). Both now use one flat colour, defaulting to the OCR window's near-black, and both
/// offer the same context-menu entry to change it - SE 4 had this configurable too
/// (Tools.BinEditImageBackgroundColor).
/// </summary>
public static class ImagePreviewBackground
{
    /// <summary>The OCR window's original hard-coded backdrop.</summary>
    public static Color DefaultColor { get; } = Color.FromRgb(0x2D, 0x2D, 0x30);

    /// <summary>
    /// A brush for one window's preview cells.
    ///
    /// Deliberately one instance per window rather than a shared static: a brush is an
    /// AvaloniaObject and carries dispatcher affinity, so a single static instance handed to
    /// every window is not safe to reuse across Avalonia application lifetimes. Every cell in a
    /// window shares that window's brush, which is what lets a colour pick repaint them all with
    /// no grid rebuild - and nothing is captured per cell, which a recycled FuncDataTemplate
    /// would drop anyway.
    /// </summary>
    public static SolidColorBrush CreateBrush()
    {
        return new SolidColorBrush(CurrentColor);
    }

    public static Color CurrentColor => ParseOrDefault(Se.Settings.Appearance.ImagePreviewBackgroundColor);

    /// <summary>
    /// The shared "Image background color..." entry. Built here so the two windows cannot drift
    /// into offering different wording or behaviour. <paramref name="brush"/> is the window's own
    /// preview brush, recoloured in place so the change shows immediately.
    /// </summary>
    public static MenuItem MakeMenuItem(IWindowService windowService, Func<Window?> getOwner, SolidColorBrush brush)
    {
        var menuItem = new MenuItem { Header = Se.Language.General.ImageBackgroundColorDotDotDot };
        menuItem.Click += async (_, _) => await PickAsync(windowService, getOwner(), brush);
        return menuItem;
    }

    private static async Task PickAsync(IWindowService windowService, Window? owner, SolidColorBrush brush)
    {
        if (owner == null)
        {
            return;
        }

        var result = await windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            owner, vm => vm.Initialize(brush.Color));

        if (!result.OkPressed)
        {
            return;
        }

        brush.Color = result.SelectedColor;
        Se.Settings.Appearance.ImagePreviewBackgroundColor = result.SelectedColor.FromColorToHex();
        Se.SaveSettings();
    }

    /// <summary>
    /// The saved hex, or the default when it is missing or unreadable - a hand-edited settings
    /// file should not take the previews down with it.
    /// </summary>
    internal static Color ParseOrDefault(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return DefaultColor;
        }

        try
        {
            return hex.FromHexToColor();
        }
        catch (Exception)
        {
            return DefaultColor;
        }
    }
}
