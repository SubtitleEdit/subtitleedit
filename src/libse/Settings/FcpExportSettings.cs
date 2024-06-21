using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class FcpExportSettings
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public string Alignment { get; set; }
        public int Baseline { get; set; }
        public Color Color { get; set; }

        public FcpExportSettings()
        {
            FontName = "Lucida Sans";
            FontSize = 36;
            Alignment = "center";
            Baseline = 29;
            Color = Color.WhiteSmoke;
        }
    }
}