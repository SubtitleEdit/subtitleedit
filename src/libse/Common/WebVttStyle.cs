using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class WebVttStyle
    {
        public string Name { get; set; }
        public string FontName { get; set; }
        public decimal? FontSize { get; set; }
        public Color? Color { get; set; }
        public Color? BackgroundColor { get; set; }
        public bool? Italic { get; set; }
        public bool? Bold { get; set; }
        public bool? Underline { get; set; }
        public bool? StrikeThrough { get; set; }
        public decimal? ShadowWidth { get; set; }
        public Color? ShadowColor { get; set; }

        public WebVttStyle()
        {
                
        }

        public WebVttStyle(WebVttStyle style)
        {
            Name = style.Name;
            FontName = style.FontName;
            FontSize = style.FontSize;
            Color = style.Color;
            BackgroundColor = style.BackgroundColor;
            Bold = style.Bold;
            Underline = style.Underline;
            StrikeThrough = style.StrikeThrough;
            ShadowWidth = style.ShadowWidth;
            ShadowColor = style.ShadowColor;
            Underline = style.Underline;
            ShadowWidth = style.ShadowWidth;
            StrikeThrough = style.StrikeThrough;
            ShadowWidth = style.ShadowWidth;
        }

        public override string ToString()
        {
            return WebVttHelper.GetCssProperties(this);
        }
    }
}