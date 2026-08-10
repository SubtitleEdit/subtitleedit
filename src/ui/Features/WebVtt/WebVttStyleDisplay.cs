using Avalonia.Media;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Globalization;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// Editable view of a <see cref="WebVttStyle"/>.
/// <para>
/// A WebVTT cue style is CSS, so every property is optional - a style that only sets
/// <c>color</c> must not gain a <c>font-family</c> just by being opened in the editor.
/// The nullable values of <see cref="WebVttStyle"/> are therefore split into a value plus
/// a "use it" flag, and only the enabled ones are written back in <see cref="ToWebVttStyle"/>.
/// </para>
/// <para>
/// <see cref="Name"/> is held without the leading dot that the cue selector uses
/// (<c>::cue(.red)</c>); the dot is added back when converting to a <see cref="WebVttStyle"/>.
/// </para>
/// </summary>
public partial class WebVttStyleDisplay : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private string _name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    [NotifyPropertyChangedFor(nameof(FontNameDisplay))]
    private string _fontName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    [NotifyPropertyChangedFor(nameof(FontSizeDisplay))]
    private decimal _fontSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private bool _bold;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    [NotifyPropertyChangedFor(nameof(ItalicDisplay))]
    private bool _italic;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private bool _underline;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private bool _strikeout;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private bool _useColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private Color _color;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private bool _useBackgroundColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private Color _backgroundColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private bool _useShadow;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private Color _shadowColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Css))]
    private decimal _shadowWidth;

    [ObservableProperty] private int _usageCount;
    [ObservableProperty] private bool _isSelected;

    public WebVttStyleDisplay()
    {
        _name = string.Empty;
        _fontName = string.Empty;
        _color = Colors.White;
        _backgroundColor = Colors.Black;
        _shadowColor = Colors.Black;
    }

    public WebVttStyleDisplay(WebVttStyle style) : this()
    {
        _name = (style.Name ?? string.Empty).TrimStart('.');
        _fontName = style.FontName ?? string.Empty;
        _fontSize = style.FontSize ?? 0;
        _bold = style.Bold == true;
        _italic = style.Italic == true;
        _underline = style.Underline == true;
        _strikeout = style.StrikeThrough == true;

        if (style.Color.HasValue)
        {
            _useColor = true;
            _color = style.Color.Value.ToAvaloniaColor();
        }

        if (style.BackgroundColor.HasValue)
        {
            _useBackgroundColor = true;
            _backgroundColor = style.BackgroundColor.Value.ToAvaloniaColor();
        }

        if (style.ShadowColor.HasValue)
        {
            _useShadow = true;
            _shadowColor = style.ShadowColor.Value.ToAvaloniaColor();
            _shadowWidth = style.ShadowWidth ?? 0;
        }
    }

    public WebVttStyleDisplay(WebVttStyleDisplay style) : this()
    {
        _name = style.Name;
        _fontName = style.FontName;
        _fontSize = style.FontSize;
        _bold = style.Bold;
        _italic = style.Italic;
        _underline = style.Underline;
        _strikeout = style.Strikeout;
        _useColor = style.UseColor;
        _color = style.Color;
        _useBackgroundColor = style.UseBackgroundColor;
        _backgroundColor = style.BackgroundColor;
        _useShadow = style.UseShadow;
        _shadowColor = style.ShadowColor;
        _shadowWidth = style.ShadowWidth;
    }

    public WebVttStyle ToWebVttStyle()
    {
        return new WebVttStyle
        {
            Name = "." + Name.TrimStart('.'),
            FontName = string.IsNullOrWhiteSpace(FontName) ? null : FontName,
            FontSize = FontSize > 0 ? FontSize : null,
            Bold = Bold ? true : null,
            Italic = Italic ? true : null,
            Underline = Underline ? true : null,
            StrikeThrough = Strikeout ? true : null,
            Color = UseColor ? Color.ToSKColor() : null,
            BackgroundColor = UseBackgroundColor ? BackgroundColor.ToSKColor() : null,
            ShadowColor = UseShadow ? ShadowColor.ToSKColor() : null,
            ShadowWidth = UseShadow ? ShadowWidth : null,
        };
    }

    /// <summary>The CSS this style is written as, shown in the editor so the raw result stays visible.</summary>
    public string Css => WebVttHelper.GetCssProperties(ToWebVttStyle());

    public string FontNameDisplay => string.IsNullOrWhiteSpace(FontName) ? "-" : FontName;

    public string FontSizeDisplay => FontSize > 0 ? FontSize.ToString(CultureInfo.CurrentCulture) : "-";

    public string ItalicDisplay => Italic ? Se.Language.General.Yes : "-";
}
