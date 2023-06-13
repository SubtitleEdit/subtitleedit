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
        public decimal? ShadowWidth { get; set; }
        public Color? ShadowColor { get; set; }
    }
}