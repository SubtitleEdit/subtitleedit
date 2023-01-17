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
        public decimal FontSize { get; set; }
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
            FontSize = 20m;
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

        public static SsaStyle FromRawSsa(string header, string styleLine)
        {
            var result = new SsaStyle();
            var styleArray = styleLine.Split(',');
            var format = GetSsaFormatList(header);

            if (styleArray.Length != format.Length)
            {
                return result;
            }

            for (var i = 0; i < format.Length; i++)
            {
                var f = format[i].Trim();
                var v = styleArray[i].Trim();

                if (f == "name")
                {
                    result.Name = v;
                }
                else if (f == "fontname")
                {
                    result.FontName = v;
                }
                else if (f == "fontsize")
                {
                    if (decimal.TryParse(v, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
                    {
                        result.FontSize = d;
                    }
                }
                else if (f == "primarycolour")
                {
                    result.Primary = AdvancedSubStationAlpha.GetSsaColor(v, Color.White);
                }
                else if (f == "secondarycolour")
                {
                    result.Secondary = AdvancedSubStationAlpha.GetSsaColor(v, Color.Yellow);
                }
                else if (f == "tertiarycolour")
                {
                    result.Tertiary = AdvancedSubStationAlpha.GetSsaColor(v, Color.Yellow);
                }
                else if (f == "backcolour")
                {
                    result.Outline = AdvancedSubStationAlpha.GetSsaColor(v, Color.Black);
                }
                else if (f == "bold")
                {
                    result.Bold = v != "0";
                }
                else if (f == "italic")
                {
                    result.Italic = v != "0";
                }
                else if (f == "outline")
                {
                    if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                    {
                        result.OutlineWidth = number;
                    }
                }
                else if (f == "shadow")
                {
                    if (decimal.TryParse(f, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number))
                    {
                        result.ShadowWidth = number;
                    }
                }
                else if (f == "marginl")
                {
                    if (int.TryParse(f, out var number))
                    {
                        result.MarginLeft = number;
                    }
                }
                else if (f == "marginr")
                {
                    if (int.TryParse(f, out var number))
                    {
                        result.MarginRight = number;
                    }
                }
                else if (f == "marginv")
                {
                    if (int.TryParse(f, out var number))
                    {
                        result.MarginVertical = number;
                    }
                }
                else if (f == "borderstyle")
                {
                    result.BorderStyle = v;
                }
                else if (f == "alignment")
                {
                    switch (v)
                    {
                        case "1":
                            result.Alignment = "1"; // bottom left
                            break;
                        case "2":
                            result.Alignment = "2"; // bottom center
                            break;
                        case "3":
                            result.Alignment = "3"; // bottom right
                            break;
                        case "9":
                            result.Alignment = "4"; // middle left
                            break;
                        case "10":
                            result.Alignment = "5"; // middle center
                            break;
                        case "11":
                            result.Alignment = "6"; // middle right
                            break;
                        case "5":
                            result.Alignment = "7"; // top left
                            break;
                        case "6":
                            result.Alignment = "8"; // top center
                            break;
                        case "7":
                            result.Alignment = "9"; // top right
                            break;
                        default:
                            result.Alignment = "2";
                            break;
                    }
                }
            }

            return result;
        }

        private static string[] GetSsaFormatList(string header)
        {
            if (string.IsNullOrEmpty(header))
            {
                header = SubStationAlpha.DefaultHeader;
            }

            foreach (var line in header.SplitToLines())
            {
                var s = line.Trim().ToLowerInvariant();
                if (s.StartsWith("format:"))
                {
                    s = s.Remove(0, "format:".Length).TrimStart().Replace(" ", string.Empty);
                    return s.Split(',');
                }
            }

            return "Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding"
                .Replace(" ", string.Empty)
                .ToLowerInvariant()
                .Split();
        }
    }
}
