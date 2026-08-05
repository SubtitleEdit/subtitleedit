using Avalonia.Media;
using Avalonia.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class StyleDisplay : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private decimal _fontSize;
    [ObservableProperty] private int _usageCount;
    [ObservableProperty] private Color _colorPrimary;
    [ObservableProperty] private Color _colorSecondary;
    [ObservableProperty] private Color _colorOutline;
    [ObservableProperty] private Color _colorShadow;
    [ObservableProperty] private decimal _outlineWidth;
    [ObservableProperty] private decimal _shadowWidth;
    [ObservableProperty] private bool _bold;
    [ObservableProperty] private bool _italic;
    [ObservableProperty] private bool _underline;
    [ObservableProperty] private bool _strikeout;
    [ObservableProperty] private decimal _scaleX;
    [ObservableProperty] private decimal _scaleY;
    [ObservableProperty] private decimal _spacing;
    [ObservableProperty] private decimal _angle;
    [ObservableProperty] private int _marginLeft;
    [ObservableProperty] private int _marginRight;
    [ObservableProperty] private int _marginVertical;
    [ObservableProperty] private BorderStyleItem _borderStyle;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isDefault;
    [ObservableProperty] private string _category = string.Empty;

    private string _fontName = string.Empty;

    /// <summary>
    /// The font combo box binds SelectedItem two-way to this. When the style editor switches
    /// to a style whose font is not in the combo's item list, Avalonia clears SelectedItem and
    /// the binding writes null back - which must not erase the style's font (#13101).
    /// </summary>
    public string FontName
    {
        get => _fontName;
        set
        {
            if (value is null)
            {
                return;
            }

            SetProperty(ref _fontName, value);
        }
    }

    /// <summary>
    /// The name this style is currently known by in the subtitle's dialog lines. The styles
    /// dialog re-points lines on rename and then advances this to the new name (#13101).
    /// </summary>
    public string LastKnownName { get; set; } = string.Empty;

    // The nine alignment flags back one shared radio-button group, bound two-way through
    // "CurrentStyle.AlignmentAnX". When the current style changes, the group unchecks the
    // old radio while its binding still points at the previously shown style, which would
    // erase that style's alignment (#13101). Radio semantics only ever need "set true":
    // checking one clears the others, and false writes are ignored.
    private bool _alignmentAn1;
    private bool _alignmentAn2;
    private bool _alignmentAn3;
    private bool _alignmentAn4;
    private bool _alignmentAn5;
    private bool _alignmentAn6;
    private bool _alignmentAn7;
    private bool _alignmentAn8;
    private bool _alignmentAn9;

    public bool AlignmentAn1 { get => _alignmentAn1; set { if (value) { SetCheckedAlignment(1); } } }
    public bool AlignmentAn2 { get => _alignmentAn2; set { if (value) { SetCheckedAlignment(2); } } }
    public bool AlignmentAn3 { get => _alignmentAn3; set { if (value) { SetCheckedAlignment(3); } } }
    public bool AlignmentAn4 { get => _alignmentAn4; set { if (value) { SetCheckedAlignment(4); } } }
    public bool AlignmentAn5 { get => _alignmentAn5; set { if (value) { SetCheckedAlignment(5); } } }
    public bool AlignmentAn6 { get => _alignmentAn6; set { if (value) { SetCheckedAlignment(6); } } }
    public bool AlignmentAn7 { get => _alignmentAn7; set { if (value) { SetCheckedAlignment(7); } } }
    public bool AlignmentAn8 { get => _alignmentAn8; set { if (value) { SetCheckedAlignment(8); } } }
    public bool AlignmentAn9 { get => _alignmentAn9; set { if (value) { SetCheckedAlignment(9); } } }

    private void SetCheckedAlignment(int alignment)
    {
        SetAlignmentFlag(ref _alignmentAn1, alignment == 1, nameof(AlignmentAn1));
        SetAlignmentFlag(ref _alignmentAn2, alignment == 2, nameof(AlignmentAn2));
        SetAlignmentFlag(ref _alignmentAn3, alignment == 3, nameof(AlignmentAn3));
        SetAlignmentFlag(ref _alignmentAn4, alignment == 4, nameof(AlignmentAn4));
        SetAlignmentFlag(ref _alignmentAn5, alignment == 5, nameof(AlignmentAn5));
        SetAlignmentFlag(ref _alignmentAn6, alignment == 6, nameof(AlignmentAn6));
        SetAlignmentFlag(ref _alignmentAn7, alignment == 7, nameof(AlignmentAn7));
        SetAlignmentFlag(ref _alignmentAn8, alignment == 8, nameof(AlignmentAn8));
        SetAlignmentFlag(ref _alignmentAn9, alignment == 9, nameof(AlignmentAn9));
    }

    private void SetAlignmentFlag(ref bool field, bool value, string propertyName)
    {
        if (field == value)
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    public StyleDisplay()
    {
        _name = string.Empty;
        AlignmentAn2 = true;
        BorderStyle = BorderStyleItem.List().First();
    }

    public void SetAlignment(string alignment)
    {
        var number = alignment.Length > 0 && alignment[^1] >= '1' && alignment[^1] <= '9'
            ? alignment[^1] - '0'
            : 2; // bottom center

        SetCheckedAlignment(number);
    }

    public string GetAlignment()
    {
        if (AlignmentAn1)
        {
            return "1";
        }

        if (AlignmentAn3)
        {
            return "3";
        }

        if (AlignmentAn4)
        {
            return "4";
        }

        if (AlignmentAn5)
        {
            return "5";
        }

        if (AlignmentAn6)
        {
            return "6";
        }

        if (AlignmentAn7)
        {
            return "7";
        }

        if (AlignmentAn8)
        {
            return "8";
        }

        if (AlignmentAn9)
        {
            return "9";
        }

        return "2";
    }

    public StyleDisplay(SsaStyle style)
    {
        Name = style.Name;
        LastKnownName = style.Name;
        FontName = style.FontName;
        FontSize = style.FontSize;
        ColorPrimary = style.Primary.ToAvaloniaColor();
        ColorSecondary = style.Secondary.ToAvaloniaColor();
        ColorOutline = style.Outline.ToAvaloniaColor();
        ColorShadow = style.Background.ToAvaloniaColor();
        OutlineWidth = style.OutlineWidth;
        ShadowWidth = style.ShadowWidth;
        Bold = style.Bold;
        Italic = style.Italic;
        Underline = style.Underline;
        Strikeout = style.Strikeout;
        ScaleX = style.ScaleX;
        ScaleY = style.ScaleY;
        Spacing = style.Spacing;
        Angle = style.Angle;
        SetAlignment(style.Alignment);
        MarginLeft = style.MarginLeft;
        MarginRight = style.MarginRight;
        MarginVertical = style.MarginVertical;
        
        if (style.BorderStyle == "3")
        {
            BorderStyle = BorderStyleItem.List().First(p => p.Style == Assa.BorderStyleType.BoxPerLine);
        }
        else if (style.BorderStyle == "4")
        {
            BorderStyle = BorderStyleItem.List().First(p => p.Style == Assa.BorderStyleType.OneBox);
        }
        else
        {
            BorderStyle = BorderStyleItem.List().First(p => p.Style == Assa.BorderStyleType.Outline);
        }
    }

    public StyleDisplay(SeAssaStyle style)
    {
        Name = style.Name;
        LastKnownName = style.Name;
        FontName = style.FontName;
        FontSize = style.FontSize;
        ColorPrimary = style.ColorPrimary.FromHexToColor();
        ColorSecondary = style.ColorSecondary.FromHexToColor();
        ColorOutline = style.ColorOutline.FromHexToColor();
        ColorShadow = style.ColorShadow.FromHexToColor();
        OutlineWidth = style.OutlineWidth;
        ShadowWidth = style.ShadowWidth;
        Bold = style.Bold;
        Italic = style.Italic;
        Underline = style.Underline;
        Strikeout = style.Strikeout;
        ScaleX = style.ScaleX;
        ScaleY = style.ScaleY;
        Spacing = style.Spacing;
        Angle = style.Angle;
        MarginLeft = style.MarginLeft;
        MarginRight = style.MarginRight;
        MarginVertical = style.MarginVertical;
        IsDefault = style.IsDefault;
        Category = style.Category;

        if (style.UseOpaqueBox)
        {
            BorderStyle = BorderStyleItem.List().First(p => p.Style == Assa.BorderStyleType.OneBox);
        }
        else if (style.UseOpaqueBoxPerLine)
        {
            BorderStyle = BorderStyleItem.List().First(p => p.Style == Assa.BorderStyleType.BoxPerLine);
        }
        else
        {
            BorderStyle = BorderStyleItem.List().First(p => p.Style == Assa.BorderStyleType.Outline);
        }

        SetAlignment(style.Alignment);
    }

    internal SsaStyle ToSsaStyle()
    {
        return new SsaStyle
        {
            Name = Name,
            Alignment = GetAlignment(),
            Angle = Angle,
            Spacing = Spacing,
            ScaleX = ScaleX,
            ScaleY = ScaleY,
            MarginLeft = MarginLeft,
            MarginRight = MarginRight,
            MarginVertical = MarginVertical,
            FontName = FontName,
            FontSize = FontSize,
            Italic = Italic,
            Bold = Bold,
            Strikeout = Strikeout,
            Primary = ColorPrimary.ToSKColor(),
            Secondary = ColorSecondary.ToSKColor(),
            Tertiary = ColorShadow.ToSKColor(),
            Background = ColorShadow.ToSKColor(),
            Outline = ColorOutline.ToSKColor(),
            OutlineWidth = OutlineWidth,
            ShadowWidth = ShadowWidth,
            BorderStyle = ((int)BorderStyle.Style).ToString(),
        };
    }
}
