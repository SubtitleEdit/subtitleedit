using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class WebVttToAssa
    {
        private static readonly Regex NameRegex = new Regex("\\([\\.a-zA-Z\\d#_-]+\\)", RegexOptions.Compiled);
        private static readonly Regex PropertiesRegex = new Regex("{[ \\.a-zA-Z\\d:#\\s,_;:\\-\\(\\)]+}", RegexOptions.Compiled);
        private static readonly Regex LineTagRegex = new Regex("<c\\.[a-z-_\\.A-Z#\\d]+>", RegexOptions.Compiled);

        public static Subtitle Convert(Subtitle webVttSubtitle, SsaStyle defaultStyle, int videoWidth, int videoHeight)
        {
            var styles = GetStyles(webVttSubtitle);
            var ssaStyles = ConvertStyles(styles, defaultStyle);
            var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, ssaStyles);
            var assaSubtitle = ConvertSubtitle(webVttSubtitle, header, ssaStyles);
            return assaSubtitle;
        }

        private static Subtitle ConvertSubtitle(Subtitle webVttSubtitle, string header, List<SsaStyle> ssaStyles)
        {
            var assaSubtitle = new Subtitle(webVttSubtitle) { Header = header };
            var layer = 0;
            foreach (var paragraph in assaSubtitle.Paragraphs)
            {
                paragraph.Layer = layer;
                layer++;

                paragraph.Text = paragraph.Text
                    .Replace("<i>", "{\\i1}")
                    .Replace("</i>", "{\\i0}")
                    .Replace("<b>", "{\\b1}")
                    .Replace("</b>", "{\\b0}")
                    .Replace("<u>", "{\\u1}")
                    .Replace("</u>", "{\\u0}").Trim();

                var matches = LineTagRegex.Matches(paragraph.Text);
                if (matches.Count == 1 && 
                    paragraph.Text.StartsWith("<c.", StringComparison.Ordinal) && 
                    paragraph.Text.EndsWith("</c>", StringComparison.Ordinal))
                {
                    var tag = matches[0].Value.Trim('<', '>', ' ');
                    if (ssaStyles.Any(p => p.Name == tag))
                    {
                        paragraph.Extra = tag;
                    }
                    else
                    {
                        paragraph.Text = SetInlineStyles(paragraph.Text, tag, ssaStyles);   
                    }
                }
            }

            return assaSubtitle;
        }

        private static string SetInlineStyles(string paragraphText, string tag, List<SsaStyle> ssaStyles)
        {
            throw new NotImplementedException();
        }

        private static List<SsaStyle> ConvertStyles(List<WebVttStyle> styles, SsaStyle defaultStyle)
        {
            var result = new List<SsaStyle>();
            foreach (var style in styles)
            {
                result.Add(new SsaStyle(new SsaStyle
                {
                    Name = style.Name,
                    FontName = style.FontName ?? defaultStyle.FontName,
                    FontSize = style.FontSize ?? defaultStyle.FontSize,
                    Primary = style.Color ?? defaultStyle.Primary,
                    Background = style.BackgroundColor ?? defaultStyle.Background,
                    Bold = style.Bold ?? defaultStyle.Bold,
                    Italic = style.Italic ?? defaultStyle.Italic,
                    ShadowWidth = style.ShadowWidth ?? defaultStyle.ShadowWidth,
                    OutlineWidth = style.ShadowWidth ?? defaultStyle.OutlineWidth,
                    Outline = style.ShadowColor ?? defaultStyle.Outline,
                }));
            }

            return result;
        }

        public class WebVttStyle
        {
            public string Name { get; set; }
            public string FontName { get; set; }
            public decimal? FontSize { get; set; }
            public Color? Color { get; set; }
            public Color? BackgroundColor { get; set; }
            public bool? Italic { get; set; }
            public bool? Bold { get; set; }
            public int? ShadowWidth { get; set; }
            public Color? ShadowColor { get; set; }
        }

        private static List<WebVttStyle> GetStyles(Subtitle webVttSubtitle)
        {
            if (string.IsNullOrEmpty(webVttSubtitle.Header))
            {
                return new List<WebVttStyle>();
            }

            var cueOn = false;
            var styleOn = false;
            var result = new List<WebVttStyle>();
            var currentStyle = new StringBuilder();
            foreach (var line in webVttSubtitle.Header.SplitToLines())
            {
                var s = line.Trim();
                if (styleOn)
                {
                    if (s == string.Empty)
                    {
                        styleOn = false;
                        AddStyle(result, currentStyle);
                    }
                    else
                    {
                        if (cueOn && s.StartsWith("::cue(", StringComparison.Ordinal))
                        {
                            AddStyle(result, currentStyle);
                            currentStyle = new StringBuilder();
                        }

                        if (s.StartsWith("::cue(", StringComparison.Ordinal))
                        {
                            currentStyle.AppendLine(s);
                            cueOn = true;
                        }
                        else if (cueOn)
                        {
                            currentStyle.AppendLine(s);
                        }
                    }
                }
                else if (s.Equals("STYLE", StringComparison.OrdinalIgnoreCase))
                {
                    styleOn = true;
                }
            }

            AddStyle(result, currentStyle);

            return result;

            // https://www.w3.org/TR/webvtt1/
            //STYLE
            //::cue {
            //                background-image: linear-gradient(to bottom, dimgray, lightgray);
            //            color: papayawhip;
            //            }
            //            /* Style blocks cannot use blank lines nor "dash dash greater than" */

            //            NOTE comment blocks can be used between style blocks.

            //            STYLE
            //            ::cue(b) {
            //            color: peachpuff;
            //            }

        }

        private static void AddStyle(List<WebVttStyle> result, StringBuilder currentStyle)
        {
            var text = currentStyle
                .ToString()
                .Replace(Environment.NewLine, " ");
            var match = NameRegex.Match(text);
            if (!match.Success)
            {
                return;
            }

            var name = match.Value.Trim('(',')',' ');

            match = PropertiesRegex.Match(text);
            if (!match.Success || string.IsNullOrWhiteSpace(match.Value))
            {
                return;
            }

            var properties = match.Value
                .Trim('{', '}', ' ')
                .RemoveChar('\r', '\n')
            .Split(';');

            var webVttStyle = new WebVttStyle { Name = name };
            foreach (var prop in properties)
            {
                SetProperty(webVttStyle, prop);
            }

            result.Add(webVttStyle);
        }

        private static void SetProperty(WebVttStyle webVttStyle, string prop)
        {
            var arr = prop.Split(':');
            if (arr.Length != 2)
            {
                return;
            }

            var name = arr[0].Trim();
            var value = arr[1].Trim();

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (name == "color")
            {
                SetColor(webVttStyle, value);
            }
            else if (name == "background-color")
            {
                SetBackgroundColor(webVttStyle, value);
            }
            else if (name == "font-family")
            {
                webVttStyle.FontName = value;
            }
            else if (name == "font-style")
            {
                SetFontStyle(webVttStyle, value);
            }
            else if (name == "font-weight")
            {
                SetFontWeight(webVttStyle, value);
            }
            else if (name == "text-shadow")
            {
                SetTextShadow(webVttStyle, value);
            }
        }

        private static void SetColor(WebVttStyle webVttStyle, string value)
        {
            var color = GetColorFromString(value, Color.Transparent);
            if (color == Color.Transparent)
            {
                return;
            }

            webVttStyle.Color = color;
        }

        private static Color GetColorFromString(string s, Color defaultColor)
        {
            try
            {
                if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                {
                    var arr = s
                        .RemoveChar(' ')
                        .Remove(0, 4)
                        .TrimEnd(')')
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                }
                
                if (s.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
                {
                    var arr = s
                        .RemoveChar(' ')
                        .Remove(0, 5)
                        .TrimEnd(')')
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var alpha = byte.MaxValue;
                    if (arr.Length == 4 && float.TryParse(arr[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var f))
                    {
                        if (f >= 0 && f < 1)
                        {
                            alpha = (byte)(f * byte.MaxValue);
                        }
                    }

                    return Color.FromArgb(alpha, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                }

                return ColorTranslator.FromHtml(s);
            }
            catch
            {
                return defaultColor;
            }
        }

        private static void SetBackgroundColor(WebVttStyle webVttStyle, string value)
        {
            var color = GetColorFromString(value, Color.Transparent);
            if (color == Color.Transparent)
            {
                return;
            }

            webVttStyle.BackgroundColor = color;
        }

        private static void SetFontWeight(WebVttStyle webVttStyle, string value)
        {
            if (value == "bold" || value == "bolder")
            {
                webVttStyle.Bold = true;
            }
            else if (value == "normal")
            {
                webVttStyle.Bold = false;
            }
        }

        private static void SetFontStyle(WebVttStyle webVttStyle, string value)
        {
            if (value == "italic" || value == "oblique")
            {
                webVttStyle.Italic = true;
            }
            else if (value == "normal")
            {
                webVttStyle.Italic = false;
            }
        }

        private static void SetTextShadow(WebVttStyle webVttStyle, string value)
        {
            //  text-shadow: #101010 3px;

            var arr = value.Split();
            if (arr.Length != 2)
            {
                return;
            }

            var color = GetColorFromString(arr[0], Color.Transparent);
            if (color == Color.Transparent)
            {
                return;
            }

            if (int.TryParse(arr[1].Replace("px", string.Empty),out var number))
            {
                webVttStyle.ShadowColor = color;
                webVttStyle.ShadowWidth = number;
            }
        }
    }
}
