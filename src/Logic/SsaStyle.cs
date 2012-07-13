using System.Drawing;

namespace Nikse.SubtitleEdit.Logic
{
    internal class SsaStyle
    {
        public string Name { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }
        public Color Primary { get; set; }
        public Color Secondary { get; set; }
        public Color Tertiary { get; set; }
        public Color Outline { get; set; }
        public Color Background { get; set; }
        public int ShadowWidth { get; set; }
        public int OutlineWidth { get; set; }
        public string Alignment { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public int MarginVertical { get; set; }
        public string BorderStyle { get; set; }
        public string RawLine { get; set; }
        public bool LoadedFromHeader { get; set; }

        public SsaStyle()
        {
            FontName = Configuration.Settings.SubtitleSettings.SsaFontName;
            FontSize = (int)Configuration.Settings.SubtitleSettings.SsaFontSize;
            Primary = Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb);
            Secondary = Color.Yellow;
            Outline = Color.Black;
            Background = Color.Black;
            Alignment = "2";
            OutlineWidth = 2;
            ShadowWidth = 2;
            MarginLeft = 10;
            MarginRight = 10;
            MarginVertical = 10;
            BorderStyle = "1";
            RawLine = string.Empty;
            LoadedFromHeader = false;
        }
    }
}
