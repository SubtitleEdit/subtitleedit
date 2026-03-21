using Nikse.SubtitleEdit.Features.Assa;

namespace Nikse.SubtitleEdit.Logic.Config;
public class SeAssaStyle
{
    public SeAssaStyle(StyleDisplay style)
    {
        Name = style.Name;
        FontName = style.FontName;
        FontSize = style.FontSize;
        ColorPrimary = style.ColorPrimary.FromColorToHex();
        ColorSecondary = style.ColorSecondary.FromColorToHex();
        ColorOutline = style.ColorOutline.FromColorToHex();
        ColorShadow = style.ColorShadow.FromColorToHex();
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
        Alignment = style.GetAlignment();
        MarginLeft = style.MarginLeft;
        MarginRight = style.MarginRight;
        MarginVertical = style.MarginVertical;
        UseOpaqueBox = style.BorderStyle.Style == BorderStyleType.OneBox;
        UseOpaqueBoxPerLine = style.BorderStyle.Style == BorderStyleType.BoxPerLine;
        IsDefault = style.IsDefault;
    }

    public string Name { get; set; } = string.Empty;
    public string FontName { get; set; } = string.Empty;
    public decimal FontSize { get; set; }
    public int UsageCount { get; set; }
    public string ColorPrimary { get; set; }
    public string ColorSecondary { get; set; }
    public string ColorOutline { get; set; }
    public string ColorShadow { get; set; }
    public decimal OutlineWidth { get; set; }
    public decimal ShadowWidth { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikeout { get; set; }
    public decimal ScaleX { get; set; }
    public decimal ScaleY { get; set; }
    public decimal Spacing { get; set; }
    public decimal Angle { get; set; }
    public string Alignment { get; set; } = string.Empty;
    public int MarginLeft { get; set; }
    public int MarginRight { get; set; }
    public int MarginVertical { get; set; }
    public bool UseOpaqueBox { get; set; }
    public bool UseOpaqueBoxPerLine { get; set; }
    public bool IsDefault { get; set; }

    public SeAssaStyle()
    {
        Alignment = string.Empty;
        ColorPrimary = string.Empty;
        ColorSecondary = string.Empty;
        ColorOutline = string.Empty;
        ColorShadow = string.Empty;
    }
}


