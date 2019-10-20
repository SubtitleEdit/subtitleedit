using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class SsaStyle
    {
        public string Name { get; set; }
        public string FontName { get; set; }
        public float FontSize { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }
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
        public string BorderStyle { get; set; }
        public string RawLine { get; set; }
        public bool LoadedFromHeader { get; set; }

        public SsaStyle()
        {
            FontName = Configuration.Settings.SubtitleSettings.SsaFontName;
            FontSize = (float)Configuration.Settings.SubtitleSettings.SsaFontSize;
            Primary = Color.FromArgb(Configuration.Settings.SubtitleSettings.SsaFontColorArgb);
            Secondary = Color.Yellow;
            Outline = Color.Black;
            Background = Color.Black;
            Alignment = "2";
            OutlineWidth = Configuration.Settings.SubtitleSettings.SsaOutline;
            ShadowWidth = Configuration.Settings.SubtitleSettings.SsaShadow;
            MarginLeft = 10;
            MarginRight = 10;
            MarginVertical = 10;
            BorderStyle = "1";
            if (Configuration.Settings.SubtitleSettings.SsaOpaqueBox)
            {
                BorderStyle = "3";
            }

            RawLine = string.Empty;
            LoadedFromHeader = false;
        }

        public SsaStyle(SsaStyle ssaStyle)
        {
            Name = ssaStyle.Name;
            FontName = ssaStyle.FontName;
            FontSize = ssaStyle.FontSize;

            Italic = ssaStyle.Italic;
            Bold = ssaStyle.Bold;
            Underline = ssaStyle.Underline;

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

            BorderStyle = ssaStyle.BorderStyle;
            RawLine = ssaStyle.RawLine;
            LoadedFromHeader = ssaStyle.LoadedFromHeader;
        }

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
                else if (f == "outlinecolour")
                {
                    sb.Append(ColorTranslator.ToWin32(Outline));
                }
                else if (f == "backcolour")
                {
                    sb.Append(ColorTranslator.ToWin32(Background));
                }
                else if (f == "bold")
                {
                    sb.Append(Convert.ToInt32(Bold));
                }
                else if (f == "italic")
                {
                    sb.Append(Convert.ToInt32(Italic));
                }
                else if (f == "underline")
                {
                    sb.Append(Convert.ToInt32(Underline));
                }
                else if (f == "outline")
                {
                    sb.Append(Outline);
                }
                else if (f == "shadow")
                {
                    sb.Append(OutlineWidth);
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
                    sb.Append("100");
                }
                else if (f == "scaley")
                {
                    sb.Append("100");
                }
                else if (f == "spacing")
                {
                    sb.Append('0');
                }
                else if (f == "angle")
                {
                    sb.Append('0');
                }

                sb.Append(',');
            }
            string s = sb.ToString().Trim();
            return s.Substring(0, s.Length - 1);
        }

        public string ToRawAss(string styleFormat)
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
                else if (f == "tertiarycolour")
                {
                    sb.Append(AdvancedSubStationAlpha.GetSsaColorString(Tertiary));
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
                    sb.Append(Convert.ToInt32(Bold));
                }
                else if (f == "italic")
                {
                    sb.Append(Convert.ToInt32(Italic));
                }
                else if (f == "underline")
                {
                    sb.Append(Convert.ToInt32(Underline));
                }
                else if (f == "outline")
                {
                    sb.Append(OutlineWidth);
                }
                else if (f == "shadow")
                {
                    sb.Append(ShadowWidth);
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
                    sb.Append("100");
                }
                else if (f == "scaley")
                {
                    sb.Append("100");
                }
                else if (f == "spacing")
                {
                    sb.Append('0');
                }
                else if (f == "angle")
                {
                    sb.Append('0');
                }

                sb.Append(',');
            }
            string s = sb.ToString().Trim();
            return s.Substring(0, s.Length - 1);
        }
    }
}
