using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class WebVttHelper
    {
        private static readonly Regex NameRegex = new Regex("\\([\\.\\p{L}\\d#_-]+\\)", RegexOptions.Compiled);
        private static readonly Regex PropertiesRegex = new Regex("{[ \\.\\p{L}\\d:#\\s,_;:\\-\\(\\)]+}", RegexOptions.Compiled);

        public static List<WebVttStyle> GetStyles(string header)
        {
            if (string.IsNullOrEmpty(header))
            {
                return new List<WebVttStyle>();
            }

            var cueOn = false;
            var styleOn = false;
            var result = new List<WebVttStyle>();
            var currentStyle = new StringBuilder();
            foreach (var line in header.SplitToLines())
            {
                var s = line.Trim();
                if (styleOn)
                {
                    if (s == string.Empty)
                    {
                        styleOn = false;
                        AddStyle(result, currentStyle);
                        currentStyle = new StringBuilder();
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
            // STYLE
            // ::cue {
            //   background-image: linear-gradient(to bottom, dimgray, lightgray);
            //   color: papayawhip;
            // }

            // STYLE
            // ::cue(b) {
            //   color: peachpuff;
            // }
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

            var name = match.Value.Trim('(', ')', ' ');

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
            else if (name == "text-decoration")
            {
                SetTextDecoration(webVttStyle, value);
            }
        }

        private static void SetColor(WebVttStyle webVttStyle, string value)
        {
            var color = GetColorFromString(value);
            if (!color.HasValue)
            {
                return;
            }

            webVttStyle.Color = color;
        }

        private static SKColor? GetColorFromString(string s)
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

                    return new SKColor(byte.Parse(arr[0]), byte.Parse(arr[1]), byte.Parse(arr[2]));
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

                    return new SKColor(byte.Parse(arr[0]), byte.Parse(arr[1]), byte.Parse(arr[2]), alpha);
                }

                if (s.Length == 9 && s.StartsWith("#"))
                {
                    if (!int.TryParse(s.Substring(7, 2), NumberStyles.HexNumber, null, out var alpha))
                    {
                        alpha = 255; // full solid color
                    }

                    s = s.Substring(1, 6);
                    var c = ColorTranslator.FromHtml("#" + s);
                    return new SKColor(c.Red, c.Green, c.Blue, (byte)alpha);
                }

                return ColorTranslator.FromHtml(s);
            }
            catch
            {
                return null;
            }
        }

        private static void SetBackgroundColor(WebVttStyle webVttStyle, string value)
        {
            var color = GetColorFromString(value);
            if (!color.HasValue)
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

        private static void SetTextDecoration(WebVttStyle webVttStyle, string value)
        {
            if (value == "underline")
            {
                webVttStyle.Underline = true;
            }
            else if (value == "line-through")
            {
                webVttStyle.StrikeThrough = true;
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

            var color = GetColorFromString(arr[0]);
            if (!color.HasValue)
            {
                return;
            }

            if (int.TryParse(arr[1].Replace("px", string.Empty), out var number))
            {
                webVttStyle.ShadowColor = color;
                webVttStyle.ShadowWidth = number;
            }
        }

        public static WebVttStyle AddStyleFromColor(SKColor color)
        {
            return new WebVttStyle
            {
                Name = "." + Utilities.ColorToHexWithTransparency(color).TrimStart('#'),
                Color = color,
            };
        }

        public static string AddStyleToHeader(string header, WebVttStyle style)
        {
            var rawStyle = "::cue(." + style.Name.RemoveChar('.') + ") { " + GetCssProperties(style) + " }";

            if (string.IsNullOrEmpty(header))
            {
                return "WEBVTT" + Environment.NewLine + Environment.NewLine + "STYLE" + Environment.NewLine + rawStyle;
            }

            if (header.Contains("::cue(." + style.Name.RemoveChar('.') + ")"))
            {
                return header;
            }

            var sb = new StringBuilder();
            var styleFound = false;
            foreach (var line in header.SplitToLines())
            {
                sb.AppendLine(line);
                if (line.Trim() == "STYLE" && !styleFound)
                {
                    sb.AppendLine(rawStyle);
                    styleFound = true;
                }
            }

            if (!styleFound)
            {
                sb.AppendLine();
                sb.AppendLine("STYLE");
                sb.AppendLine(rawStyle);
            }

            return sb.ToString();
        }

        public static string GetCssProperties(WebVttStyle style)
        {
            var sb = new StringBuilder();

            if (style.Color != null)
            {
                if (style.Color.Value.Alpha == byte.MaxValue)
                {
                    sb.Append($"color:rgb({style.Color.Value.Red},{style.Color.Value.Green},{style.Color.Value.Blue}); ");
                }
                else
                {
                    sb.Append($"color:rgba({style.Color.Value.Red},{style.Color.Value.Green},{style.Color.Value.Blue},{(style.Color.Value.Alpha / 255.0).ToString(CultureInfo.InvariantCulture)}); ");
                }
            }

            if (style.BackgroundColor != null)
            {
                if (style.BackgroundColor.Value.Alpha == byte.MaxValue)
                {
                    sb.Append($"background-color:rgb({style.BackgroundColor.Value.Red} , {style.BackgroundColor.Value.Green} , {style.BackgroundColor.Value.Blue}); ");
                }
                else
                {
                    sb.Append($"background-color:rgba({style.BackgroundColor.Value.Red} , {style.BackgroundColor.Value.Green} , {style.BackgroundColor.Value.Blue} , {(style.BackgroundColor.Value.Alpha / 255.0).ToString(CultureInfo.InvariantCulture)}); ");
                }
            }

            if (style.Italic != null && style.Italic.Value)
            {
                sb.Append("font-style:italic; ");
            }

            if (style.Bold != null && style.Bold.Value)
            {
                sb.Append("font-weight:bold; ");
            }

            if (style.Underline != null && style.Underline.Value)
            {
                sb.Append("text-decoration:underline; ");
            }

            if (style.StrikeThrough != null && style.StrikeThrough.Value)
            {
                sb.Append("text-decoration:line-through; ");
            }

            if (!string.IsNullOrEmpty(style.FontName))
            {
                sb.Append($"font-family:{style.FontName}; ");
            }

            if (style.FontSize.HasValue && style.FontSize > 0)
            {
                sb.Append($"font-size:{style.FontSize}px; ");
            }

            if (style.ShadowColor.HasValue && style.ShadowWidth.HasValue && style.ShadowWidth > 0)
            {
                var colorString = Utilities.ColorToHexWithTransparency(style.ShadowColor.Value);
                var widthString = "{style.ShadowWidth.Value.ToString(CultureInfo.InvariantCulture)} px";
                sb.Append($"text-shadow: {colorString} {widthString}");
            }

            return sb.ToString().TrimEnd(' ', ';');
        }

        public static string RemoveColorTag(string input, SKColor color, List<WebVttStyle> webVttStyles)
        {
            if (webVttStyles == null)
            {
                return input;
            }

            var style = webVttStyles.FirstOrDefault(p => p.Color == color &&
                                                         p.Italic == null &&
                                                         p.Bold == null);
            if (style == null)
            {
                return input;
            }

            var text = input;
            text = text.Replace("." + style.Name.TrimStart('.') + ".", ".");
            text = text.Replace("." + style.Name.TrimStart('.') + ">", ">");

            var idx = text.IndexOf("<c>", StringComparison.Ordinal);
            if (idx >= 0)
            {
                var endIdx = text.IndexOf("</c>", idx);
                if (endIdx > 0)
                {
                    text = text.Remove(endIdx, 4);
                    text = text.Remove(idx, 3);
                }
            }

            return text;
        }

        public static string AddStyleToText(string input, WebVttStyle style, List<WebVttStyle> webVttStyles)
        {
            if (Configuration.Settings.SubtitleSettings.WebVttDoNoMergeTags)
            {
                var text = "<c." + style.Name.TrimStart('.') + ">" + input + "</c>";
                return text;
            }
            else
            {
                var text = input;
                if (text.Contains("<c."))
                {
                    if (!text.Contains("." + style.Name.TrimStart('.') + ".") && !text.Contains("." + style.Name.TrimStart('.') + ">"))
                    {
                        var regex = new Regex(@"<c\.[\.a-zA-Z\d#_-]+>");
                        var match = regex.Match(text);
                        if (match.Success)
                        {
                            text = RemoveUnusedColorStylesFromText(text, webVttStyles);
                            text = text.Insert(match.Index + match.Length - 1, "." + style.Name.TrimStart('.'));
                        }
                    }
                }
                else
                {
                    text = "<c." + style.Name.TrimStart('.') + ">" + text + "</c>";
                }

                return text;
            }
        }

        public static List<string> GetParagraphStyles(Paragraph paragraph)
        {
            var list = new List<string>();
            if (paragraph == null || string.IsNullOrEmpty(paragraph.Text))
            {
                return list;
            }

            var regex = new Regex(@"<c\.[\.a-zA-Z\d#_-]+>");
            foreach (Match match in regex.Matches(paragraph.Text))
            {
                var styles = match.Value.Remove(0, 3).Trim('>', ' ').Split('.');
                foreach (var styleName in styles)
                {
                    if (!string.IsNullOrEmpty(styleName) && !list.Contains(styleName))
                    {
                        list.Add("." + styleName);
                    }
                }
            }

            return list;
        }

        public static string SetParagraphStyles(Paragraph p, List<WebVttStyle> styles)
        {
            if (string.IsNullOrEmpty(p.Text))
            {
                return p.Text;
            }

            var text = p.Text;
            var regex = new Regex(@"<c\.[\.a-zA-Z\d#_-]+>");
            var match = regex.Match(text);
            while (match.Success)
            {
                text = text.Remove(match.Index, match.Value.Length);
                match = regex.Match(text);
            }

            text = text.Replace("</c>", string.Empty);

            if (styles.Count == 0)
            {
                return text;
            }

            if (Configuration.Settings.SubtitleSettings.WebVttDoNoMergeTags)
            {
                foreach (var style in styles)
                {
                    text = "<c." + style.Name.TrimStart('.') + ">" + text + "</c>";
                }

                return text;
            }
            else
            {
                var prefix = "<c" + string.Join("", styles.Select(s => s.Name)) + ">";
                return prefix + text + "</c>";
            }
        }

        public static string RemoveUnusedColorStylesFromText(string input, string header)
        {
            if (string.IsNullOrEmpty(header) || !header.Contains("WEBVTT"))
            {
                return input;
            }

            var styles = GetStyles(header);
            if (styles.Count <= 1)
            {
                return input;
            }

            return RemoveUnusedColorStylesFromText(input, styles);
        }

        public static string RemoveUnusedColorStylesFromText(string input, List<WebVttStyle> styles)
        {
            var regex = new Regex(@"<c\.[\.a-zA-Z\d#_-]+>");
            var match = regex.Match(input);
            if (!match.Success)
            {
                return input;
            }

            var text = input;
            var styleNames = match.Value.Remove(0, 3).Trim('>').Split('.');
            var colorsOnly = new List<string>();
            foreach (var styleName in styleNames)
            {
                var style = styles.FirstOrDefault(p => p.Name == "." + styleName);
                if (style != null &&
                    style.Color.HasValue &&
                    style.Bold == null &&
                    style.Italic == null &&
                    style.FontName == null &&
                    style.FontSize == null &&
                    style.ShadowColor == null &&
                    style.BackgroundColor == null &&
                    style.Underline == null &&
                    style.StrikeThrough == null &&
                    style.StrikeThrough == null)
                {
                    colorsOnly.Add(styleName);
                }
            }

            while (colorsOnly.Count > 1)
            {
                var name = colorsOnly[0];
                text = text.Replace("." + name + ".", ".");
                text = text.Replace("." + name + ">", ">");
                colorsOnly.RemoveAt(0);
            }

            return text;
        }

        public static WebVttStyle GetOnlyColorStyle(SKColor color, string header)
        {
            if (string.IsNullOrEmpty(header) || !header.Contains("WEBVTT"))
            {
                return null;
            }

            var styles = GetStyles(header);
            if (styles.Count <= 1)
            {
                return null;
            }

            foreach (var style in styles)
            {
                if (style != null &&
                    style.Color.HasValue &&
                    style.Bold == null &&
                    style.Italic == null &&
                    style.FontName == null &&
                    style.FontSize == null &&
                    style.ShadowColor == null &&
                    style.BackgroundColor == null &&
                    style.Underline == null &&
                    style.StrikeThrough == null &&
                    style.StrikeThrough == null &&
                    style.Color == color)
                {
                    return style;
                }
            }

            return null;
        }
    }
}
