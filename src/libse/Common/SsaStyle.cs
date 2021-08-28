using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class SsaStyle
    {
        public string Name { get; set; }
        public string FontName { get; set; }
        public float FontSize { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }
        public bool Strikeout { get; set; }
        public Color Primary { get; set; }
        public Color Secondary { get; set; }
        public Color Tertiary { get; set; }
        public Color Outline { get; set; }
        public Color Background { get; set; }
        public decimal ShadowWidth { get; set; }
        public decimal OutlineWidth { get; set; }
        public string Alignment { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public int MarginVertical { get; set; }
        public decimal ScaleX { get; set; }
        public decimal ScaleY { get; set; }
        public decimal Spacing { get; set; }
        public decimal Angle { get; set; }
        public string BorderStyle { get; set; }
        public string RawLine { get; set; }
        public bool LoadedFromHeader { get; set; }

        public const string DefaultAssStyleFormat = "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding";

        public SsaStyle()
        {
            Name = "Default";
            FontName = "Arial";
            FontSize = 20F;
            Primary = Color.White;
            Secondary = Color.Yellow;
            Outline = Color.Black;
            Background = Color.Black;
            Alignment = "2";
            OutlineWidth = 1M;
            ShadowWidth = 1M;
            MarginLeft = 10;
            MarginRight = 10;
            MarginVertical = 10;
            BorderStyle = "1";
            RawLine = string.Empty;
            LoadedFromHeader = false;
            ScaleX = 100;
            ScaleY = 100;
            Spacing = 0;
            Angle = 0;
        }

        public SsaStyle(SsaStyle ssaStyle)
        {
            Name = ssaStyle.Name;
            FontName = ssaStyle.FontName;
            FontSize = ssaStyle.FontSize;

            Italic = ssaStyle.Italic;
            Bold = ssaStyle.Bold;
            Underline = ssaStyle.Underline;
            Strikeout = ssaStyle.Strikeout;

            Primary = ssaStyle.Primary;
            Secondary = ssaStyle.Secondary;
            Tertiary = ssaStyle.Tertiary;
            Outline = ssaStyle.Outline;
            Background = ssaStyle.Background;

            ShadowWidth = ssaStyle.ShadowWidth;
            OutlineWidth = ssaStyle.OutlineWidth;

            Alignment = ssaStyle.Alignment;
            MarginLeft = ssaStyle.MarginLeft;
            MarginRight = ssaStyle.MarginRight;
            MarginVertical = ssaStyle.MarginVertical;
            ScaleX = ssaStyle.ScaleX;
            ScaleY = ssaStyle.ScaleY;
            Spacing = ssaStyle.Spacing;
            Angle = ssaStyle.Angle;

            BorderStyle = ssaStyle.BorderStyle;
            RawLine = ssaStyle.RawLine;
            LoadedFromHeader = ssaStyle.LoadedFromHeader;
        }

        private static string BoolToRawSsa(bool value) => value ? "-1" : "0";

        public string ToRawSsa(string styleFormat)
        {
            var sb = new StringBuilder();
            sb.Append("Style: ");
            var format = styleFormat.ToLowerInvariant().Substring(8).Split(',');
            for (int i = 0; i < format.Length; i++)
            {
                string f = format[i].Trim();
                if (f == "name")
                {
                    sb.Append(Name);
                }
                else if (f == "fontname")
                {
                    sb.Append(FontName);
                }
                else if (f == "fontsize")
                {
                    sb.Append(FontSize.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "primarycolour")
                {
                    sb.Append(ColorTranslator.ToWin32(Primary));
                }
                else if (f == "secondarycolour")
                {
                    sb.Append(ColorTranslator.ToWin32(Secondary));
                }
                else if (f == "tertiarycolour")
                {
                    sb.Append(ColorTranslator.ToWin32(Tertiary));
                }
                else if (f == "backcolour")
                {
                    sb.Append(ColorTranslator.ToWin32(Background));
                }
                else if (f == "bold")
                {
                    sb.Append(BoolToRawSsa(Bold));
                }
                else if (f == "italic")
                {
                    sb.Append(BoolToRawSsa(Italic));
                }
                else if (f == "underline")
                {
                    sb.Append(BoolToRawSsa(Underline));
                }
                else if (f == "outline")
                {
                    sb.Append(OutlineWidth.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "shadow")
                {
                    sb.Append(ShadowWidth.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "marginl")
                {
                    sb.Append(MarginLeft);
                }
                else if (f == "marginr")
                {
                    sb.Append(MarginRight);
                }
                else if (f == "marginv")
                {
                    sb.Append(MarginVertical);
                }
                else if (f == "borderstyle")
                {
                    sb.Append(BorderStyle);
                }
                else if (f == "encoding")
                {
                    sb.Append('1');
                }
                else if (f == "strikeout")
                {
                    sb.Append('0');
                }
                else if (f == "scalex")
                {
                    sb.Append(ScaleX.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "scaley")
                {
                    sb.Append(ScaleY.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "spacing")
                {
                    sb.Append(Spacing.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "angle")
                {
                    sb.Append(Angle.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "alignment")
                {
                    sb.Append(Alignment);
                }
                else if (f == "alphalevel")
                {
                    sb.Append("0");
                }

                sb.Append(',');
            }

            var s = sb.ToString().Trim();
            return s.Substring(0, s.Length - 1);
        }

        public string ToRawAss(string styleFormat = DefaultAssStyleFormat)
        {
            var sb = new StringBuilder();
            sb.Append("Style: ");
            var format = styleFormat.ToLowerInvariant().Substring(8).Split(',');
            for (int i = 0; i < format.Length; i++)
            {
                string f = format[i].Trim();
                if (f == "name")
                {
                    sb.Append(Name);
                }
                else if (f == "fontname")
                {
                    sb.Append(FontName);
                }
                else if (f == "fontsize")
                {
                    sb.Append(FontSize.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "primarycolour")
                {
                    sb.Append(AdvancedSubStationAlpha.GetSsaColorString(Primary));
                }
                else if (f == "secondarycolour")
                {
                    sb.Append(AdvancedSubStationAlpha.GetSsaColorString(Secondary));
                }
                else if (f == "outlinecolour")
                {
                    sb.Append(AdvancedSubStationAlpha.GetSsaColorString(Outline));
                }
                else if (f == "backcolour")
                {
                    sb.Append(AdvancedSubStationAlpha.GetSsaColorString(Background));
                }
                else if (f == "bold")
                {
                    sb.Append(BoolToRawSsa(Bold));
                }
                else if (f == "italic")
                {
                    sb.Append(BoolToRawSsa(Italic));
                }
                else if (f == "underline")
                {
                    sb.Append(BoolToRawSsa(Underline));
                }
                else if (f == "strikeout")
                {
                    sb.Append(BoolToRawSsa(Strikeout));
                }
                else if (f == "outline")
                {
                    sb.Append(OutlineWidth.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "shadow")
                {
                    sb.Append(ShadowWidth.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "alignment")
                {
                    sb.Append(Alignment);
                }
                else if (f == "marginl")
                {
                    sb.Append(MarginLeft);
                }
                else if (f == "marginr")
                {
                    sb.Append(MarginRight);
                }
                else if (f == "marginv")
                {
                    sb.Append(MarginVertical);
                }
                else if (f == "borderstyle")
                {
                    sb.Append(BorderStyle);
                }
                else if (f == "encoding")
                {
                    sb.Append('1');
                }
                else if (f == "strikeout")
                {
                    sb.Append('0');
                }
                else if (f == "scalex")
                {
                    sb.Append(ScaleX.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "scaley")
                {
                    sb.Append(ScaleY.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "spacing")
                {
                    sb.Append(Spacing.ToString(CultureInfo.InvariantCulture));
                }
                else if (f == "angle")
                {
                    sb.Append(Angle.ToString(CultureInfo.InvariantCulture));
                }

                sb.Append(',');
            }

            var s = sb.ToString().Trim();
            return s.Substring(0, s.Length - 1);
        }
    }
}
