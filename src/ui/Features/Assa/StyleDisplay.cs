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
    [ObservableProperty] private string _fontName = string.Empty;
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
    [ObservableProperty] private bool _alignmentAn1;
    [ObservableProperty] private bool _alignmentAn2;
    [ObservableProperty] private bool _alignmentAn3;
    [ObservableProperty] private bool _alignmentAn4;
    [ObservableProperty] private bool _alignmentAn5;
    [ObservableProperty] private bool _alignmentAn6;
    [ObservableProperty] private bool _alignmentAn7;
    [ObservableProperty] private bool _alignmentAn8;
    [ObservableProperty] private bool _alignmentAn9;
    [ObservableProperty] private int _marginLeft;
    [ObservableProperty] private int _marginRight;
    [ObservableProperty] private int _marginVertical;
    [ObservableProperty] private BorderStyleItem _borderStyle;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isDefault;

    public string OriginalName { get; set; } = string.Empty;

    public StyleDisplay()
    {
        _name = string.Empty;
        OriginalName = string.Empty;
        AlignmentAn2 = true;
        BorderStyle = BorderStyleItem.List().First();
    }

    public void SetAlignment(string alignment)
    {
        AlignmentAn1 = alignment.EndsWith("1");
        AlignmentAn2 = alignment.EndsWith("2");
        AlignmentAn3 = alignment.EndsWith("3");
        AlignmentAn4 = alignment.EndsWith("4");
        AlignmentAn5 = alignment.EndsWith("5");
        AlignmentAn6 = alignment.EndsWith("6");
        AlignmentAn7 = alignment.EndsWith("7");
        AlignmentAn8 = alignment.EndsWith("8");
        AlignmentAn9 = alignment.EndsWith("9");

        UpdateAlignment();
    }

    public string GetAlignment()
    {
        if (AlignmentAn1)
        {
            return "1";
        }

        if (AlignmentAn2)
        {
            return "2";
        }

        if (AlignmentAn3)
        {
            return "3";
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
        OriginalName = style.Name;
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
        AlignmentAn1 = style.Alignment.EndsWith("1");
        AlignmentAn2 = style.Alignment.EndsWith("2");
        AlignmentAn3 = style.Alignment.EndsWith("3");
        AlignmentAn4 = style.Alignment.EndsWith("4");
        AlignmentAn5 = style.Alignment.EndsWith("5");
        AlignmentAn6 = style.Alignment.EndsWith("6");
        AlignmentAn7 = style.Alignment.EndsWith("7");
        AlignmentAn8 = style.Alignment.EndsWith("8");
        AlignmentAn9 = style.Alignment.EndsWith("9");
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

        UpdateAlignment();
    }

    public StyleDisplay(SeAssaStyle style)
    {
        Name = style.Name;
        OriginalName = style.Name;
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

    private void UpdateAlignment()
    {
        if (!AlignmentAn1 &&
            !AlignmentAn3 &&
            !AlignmentAn4 &&
            !AlignmentAn5 &&
            !AlignmentAn6 &&
            !AlignmentAn7 &&
            !AlignmentAn8 &&
            !AlignmentAn9)
        {
            AlignmentAn2 = true; // bottom center
        }
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
