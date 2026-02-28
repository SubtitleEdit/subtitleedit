using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class SubtitleBeaming
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public SKColor FontColor { get; set; }
        public SKColor BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public SubtitleBeaming()
        {
            FontName = "Verdana";
            FontSize = 30;
            FontColor = SKColors.White;
            BorderColor = SKColors.DarkGray;
            BorderWidth = 2;
        }
    }
}