using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The colors a waveform is drawn with. Normally the user's waveform settings, but a theme with
/// a palette of its own (Midnight Cinema, Sakura) colors the waveform too - like the window
/// colors, the palette is the point of picking such a theme, so it wins over the settings.
/// </summary>
internal sealed record WaveformColors(
    string TextHex,
    Color Waveform,
    Color Background,
    Color Selected,
    Color Cursor,
    Color ShotChange,
    Color Grid,
    Color ParagraphLeft,
    Color ParagraphRight,
    Color FancyHigh,
    Color ParagraphBackground,
    Color ParagraphSelectedBackground)
{
    public Color Text => TextHex.FromHexToColor();

    /// <summary>
    /// Just the text color, for the per-frame paths: no parsing of the other settings.
    /// </summary>
    public static string CurrentTextHex => UiTheme.GetThemeWaveformColors()?.TextHex ?? Se.Settings.Waveform.WaveformTextColor;

    public static WaveformColors Current => UiTheme.GetThemeWaveformColors() ?? FromSettings(Se.Settings.Waveform);

    public static WaveformColors FromSettings(SeWaveform settings) => new(
        settings.WaveformTextColor,
        settings.WaveformColor.FromHexToColor(),
        settings.WaveformBackgroundColor.FromHexToColor(),
        settings.WaveformSelectedColor.FromHexToColor(),
        settings.WaveformCursorColor.FromHexToColor(),
        settings.WaveformShotChangeColor.FromHexToColor(),
        settings.WaveformGridColor.FromHexToColor(),
        settings.WaveformParagraphLeftColor.FromHexToColor(),
        settings.WaveformParagraphRightColor.FromHexToColor(),
        settings.WaveformFancyHighColor.FromHexToColor(),
        settings.ParagraphBackground.FromHexToColor(),
        settings.ParagraphSelectedBackground.FromHexToColor());

    public void ApplyTo(AudioVisualizer audioVisualizer)
    {
        audioVisualizer.WaveformColor = Waveform;
        audioVisualizer.WaveformBackgroundColor = Background;
        audioVisualizer.WaveformSelectedColor = Selected;
        audioVisualizer.WaveformCursorColor = Cursor;
        audioVisualizer.WaveformShotChangeColor = ShotChange;
        audioVisualizer.WaveformGridColor = Grid;
        audioVisualizer.WaveformParagraphLeftColor = ParagraphLeft;
        audioVisualizer.WaveformParagraphRightColor = ParagraphRight;
        audioVisualizer.WaveformFancyHighColor = FancyHigh;
        audioVisualizer.ParagraphBackground = ParagraphBackground;
        audioVisualizer.ParagraphSelectedBackground = ParagraphSelectedBackground;
    }
}
