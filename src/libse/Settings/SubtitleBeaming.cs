using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class SubtitleBeaming
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public SubtitleBeaming()
        {
            FontName = "Verdana";
            FontSize = 30;
            FontColor = Color.White;
            BorderColor = Color.DarkGray;
            BorderWidth = 2;
        }
    }
}