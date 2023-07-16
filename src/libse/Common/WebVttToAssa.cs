using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class WebVttToAssa
    {
        private static readonly Regex LineTagRegex = new Regex("<c\\.[a-z-_\\.A-Z#\\d]+>", RegexOptions.Compiled);
        private static readonly Regex LineTagRegexMore = new Regex(@"</?c[a-z-_\.A-Z#\d]*>", RegexOptions.Compiled);

        public static Subtitle Convert(Subtitle webVttSubtitle, SsaStyle defaultStyle, int videoWidth, int videoHeight)
        {
            var vttStyles = WebVttHelper.GetStyles(webVttSubtitle.Header);
            var ssaStyles = ConvertStyles(vttStyles, defaultStyle);
            var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, ssaStyles);
            var assaSubtitle = ConvertSubtitle(webVttSubtitle, header, ssaStyles, vttStyles, videoWidth, videoHeight);
            return assaSubtitle;
        }

        private static Subtitle ConvertSubtitle(Subtitle webVttSubtitle, string header, List<SsaStyle> ssaStyles, List<WebVttStyle> webVttStyles, int width, int height)
        {
            var assaSubtitle = new Subtitle(webVttSubtitle) { Header = header };

            if (width > 0 && height > 0)
            {
                assaSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + width.ToString(CultureInfo.InvariantCulture), "[Script Info]", assaSubtitle.Header);
                assaSubtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + height.ToString(CultureInfo.InvariantCulture), "[Script Info]", assaSubtitle.Header);
            }

            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(assaSubtitle.Header);
            foreach (var style in styles)
            {
                if (style.FontSize <= 25 && width > 0 && height > 0)
                {
                    const int defaultAssaHeight = 288;
                    style.FontSize = AssaResampler.Resample(defaultAssaHeight, height, style.FontSize);
                }
            }
            assaSubtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(assaSubtitle.Header, styles);

            var layer = 0;
            foreach (var paragraph in assaSubtitle.Paragraphs)
            {
                paragraph.Layer = layer;
                paragraph.Extra = "Default";
                layer++;

                if (!paragraph.Text.Contains('<'))
                {
                    paragraph.Text = GetAlignment(paragraph, width, height);
                    continue;
                }

                paragraph.Text = paragraph.Text
                    .Replace("<i>", "{\\i1}")
                    .Replace("</i>", "{\\i0}")
                    .Replace("<b>", "{\\b1}")
                    .Replace("</b>", "{\\b0}")
                    .Replace("<u>", "{\\u1}")
                    .Replace("</u>", "{\\u0}").Trim();


                if (!paragraph.Text.Contains('<'))
                {
                    paragraph.Text = GetAlignment(paragraph, width, height);
                    continue;
                }

                var matches = LineTagRegex.Matches(paragraph.Text);
                if (matches.Count == 1 &&
                    paragraph.Text.StartsWith("<c.", StringComparison.Ordinal) &&
                    paragraph.Text.EndsWith("</c>", StringComparison.Ordinal))
                {
                    var tag = matches[0].Value.TrimEnd('>', ' ').Remove(0, 2);
                    if (ssaStyles.Any(p => p.Name == tag))
                    {
                        paragraph.Extra = tag;
                        paragraph.Text = paragraph.Text.Remove(matches[0].Index, matches[0].Length);
                        paragraph.Text = paragraph.Text.Replace("</c>", string.Empty);
                        continue;
                    }
                }

                paragraph.Text = SetInlineStyles(paragraph.Text, ssaStyles, webVttStyles);

                paragraph.Text = GetAlignment(paragraph, width, height);
            }

            return assaSubtitle;
        }

        private static string GetAlignment(Paragraph paragraph, int width, int height)
        {
            if (string.IsNullOrEmpty(paragraph.Extra) || paragraph.Text.StartsWith("{\\an"))
            {
                return paragraph.Text;
            }

            return GetPositionInfo(paragraph.Style, width, height) + paragraph.Text;
        }

        internal static string GetPositionInfo(string s, int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return string.Empty;
            }

            //position: x --- 0% = left, 100% = right (horizontal)
            //line: x --- 0 or -16 or 0% = top, 16 or -1 or 100% = bottom (vertical)
            var x = 0;
            var y = 0;
            var pos = GetTag(s, "position:");
            var line = GetTag(s, "line:");
            var positionInfo = string.Empty;
            var hAlignLeft = false;
            var hAlignRight = false;
            var vAlignTop = false;
            var vAlignMiddle = false;
            double number;

            if (!string.IsNullOrEmpty(pos) && pos.EndsWith('%') && double.TryParse(pos.TrimEnd('%'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
            {
                x = (int)Math.Round(number * width / 100.0, MidpointRounding.AwayFromZero);
            }

            if (!string.IsNullOrEmpty(line))
            {
                line = line.Trim();
                if (line.EndsWith('%'))
                {
                    if (double.TryParse(line.TrimEnd('%'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
                    {
                        y = (int)Math.Round(number * height / 100.0, MidpointRounding.AwayFromZero);
                        if (number < 25)
                        {
                            vAlignTop = true;
                        }
                        else if (number < 75)
                        {
                            vAlignMiddle = true;
                        }
                    }
                }
                else
                {
                    if (double.TryParse(line, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
                    {
                        if (number >= 0 && number <= 7)
                        {
                            vAlignTop = true; // Positive numbers indicate top down
                        }
                        else if (number > 7 && number < 11)
                        {
                            vAlignMiddle = true;
                        }
                    }
                }
            }

            if (x > 0 && y > 0)
            {
                return "{\\pos(" + x + "," + y + ")}";
            }

            if (hAlignLeft)
            {
                if (vAlignTop)
                {
                    return "{\\an7}";
                }

                if (vAlignMiddle)
                {
                    return "{\\an4}";
                }

                return "{\\an1}";
            }

            if (hAlignRight)
            {
                if (vAlignTop)
                {
                    return "{\\an9}";
                }

                if (vAlignMiddle)
                {
                    return "{\\an6}";
                }

                return "{\\an3}";
            }

            if (vAlignTop)
            {
                return "{\\an8}";
            }

            if (vAlignMiddle)
            {
                return "{\\an5}";
            }

            return positionInfo;
        }

        private static string GetTag(string s, string tag)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(tag))
            {
                return null;
            }

            var pos = s.IndexOf(tag, StringComparison.Ordinal);
            if (pos >= 0)
            {
                var v = s.Substring(pos + tag.Length).Trim();
                var end = v.IndexOf("%,", StringComparison.Ordinal);
                if (end >= 0)
                {
                    v = v.Remove(end + 1);
                }

                end = v.IndexOf(' ');
                if (end >= 0)
                {
                    v = v.Remove(end);
                }

                return v;
            }

            return null;
        }

        private static string SetInlineStyles(string input, List<SsaStyle> ssaStyles, List<WebVttStyle> webVttStyles)
        {
            var allInlineStyles = new List<WebVttStyle>();
            var start = 0;
            var sb = new StringBuilder();
            var webVttStyle = new WebVttStyle();
            var text = input;
            var match = LineTagRegexMore.Match(text);
            while (match.Success)
            {
                if (match.Value == "</c>")
                {
                    if (match.Index > start)
                    {
                        var s = text.Substring(start, Math.Min(text.Length - start, match.Index));
                        sb.Append(s);
                        start = match.Index;

                        if (allInlineStyles.Count > 0)
                        {
                            allInlineStyles.RemoveAt(allInlineStyles.Count - 1);

                            webVttStyle = new WebVttStyle();
                            foreach (var style in allInlineStyles)
                            {
                                webVttStyle = ApplyStyle(style, webVttStyle);
                            }
                        }
                    }
                }
                else if (match.Value.StartsWith("<c.", StringComparison.Ordinal))
                {
                    var arr = match.Value.Remove(0, 3).TrimEnd('>').Split('.');
                    foreach (var styleName in arr)
                    {
                        var styleFound = webVttStyles.FirstOrDefault(p => p.Name == "." + styleName);
                        if (styleFound != null)
                        {
                            webVttStyle = ApplyStyle(styleFound, webVttStyle);
                        }
                        else if (styleName == "i")
                        {
                            webVttStyle.Italic = true;
                        }
                        else if (styleName == "b")
                        {
                            webVttStyle.Bold = true;
                        }
                        else if (styleName == "u")
                        {
                            webVttStyle.Underline = true;
                        }
                        else if (WebVTT.DefaultColorClasses.TryGetValue(styleName, out var c))
                        {
                            webVttStyle.Color = c;
                        }
                    }

                    allInlineStyles.Add(webVttStyle);
                    sb.Append(WebVttToAssaInline(webVttStyle));
                }

                text = text.Remove(match.Index, match.Length);
                match = LineTagRegexMore.Match(text);
            }

            if (text.Length > start)
            {
                sb.Append(text.Substring(start));
            }

            return sb.ToString();
        }

        private static string WebVttToAssaInline(WebVttStyle webVttStyle)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            if (webVttStyle.Color != null)
            {
                sb.Append("\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(webVttStyle.Color.Value));
            }

            if (webVttStyle.BackgroundColor != null)
            {
                sb.Append("\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(webVttStyle.BackgroundColor.Value, "3c"));
            }

            if (webVttStyle.ShadowColor != null)
            {
                sb.Append("\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(webVttStyle.ShadowColor.Value, "4c"));
            }

            if (webVttStyle.ShadowWidth != null)
            {
                sb.Append("\\shad" + webVttStyle.ShadowWidth.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (webVttStyle.FontName != null)
            {
                sb.Append($"\\fn{webVttStyle.FontName}");
            }

            if (webVttStyle.FontSize != null)
            {
                sb.Append($"\\fs{webVttStyle.FontSize}");
            }

            if (webVttStyle.Italic != null && webVttStyle.Italic == true)
            {
                sb.Append("\\i1");
            }

            if (webVttStyle.Italic != null && webVttStyle.Italic == false)
            {
                sb.Append("\\i0");
            }

            if (webVttStyle.Bold != null && webVttStyle.Bold == true)
            {
                sb.Append("\\b1");
            }

            if (webVttStyle.Bold != null && webVttStyle.Bold == false)
            {
                sb.Append("\\b0");
            }

            if (webVttStyle.Underline != null && webVttStyle.Underline == true)
            {
                sb.Append("\\u1");
            }

            if (webVttStyle.Underline != null && webVttStyle.Underline == false)
            {
                sb.Append("\\u0");
            }

            if (webVttStyle.StrikeThrough != null && webVttStyle.StrikeThrough == true)
            {
                sb.Append("\\s1");
            }

            if (webVttStyle.StrikeThrough != null && webVttStyle.StrikeThrough == false)
            {
                sb.Append("\\s0");
            }

            sb.Append("}");

            if (sb.Length > 2)
            {
                return sb.ToString();
            }

            return string.Empty;
        }

        private static WebVttStyle ApplyStyle(WebVttStyle style, WebVttStyle defaultStyle)
        {
            return new WebVttStyle
            {
                BackgroundColor = style.BackgroundColor ?? defaultStyle.BackgroundColor,
                Bold = style.Bold ?? defaultStyle.Bold,
                Italic = style.Italic ?? defaultStyle.Italic,
                Underline = style.Underline ?? defaultStyle.Underline,
                FontName = style.FontName ?? defaultStyle.FontName,
                FontSize = style.FontSize ?? defaultStyle.FontSize,
                Color = style.Color ?? defaultStyle.Color,
                ShadowColor = style.ShadowColor ?? defaultStyle.ShadowColor,
                ShadowWidth = style.ShadowWidth ?? defaultStyle.ShadowWidth,
            };
        }

        private static List<SsaStyle> ConvertStyles(List<WebVttStyle> styles, SsaStyle defaultStyle)
        {
            var result = new List<SsaStyle>();
            defaultStyle.Name = "Default";
            result.Add(defaultStyle);
            ;
            foreach (var style in styles)
            {
                var newStyle = new SsaStyle
                {
                    BorderStyle = "3", // box per line (background color is outline)
                    Name = style.Name,
                    FontName = style.FontName ?? defaultStyle.FontName,
                    FontSize = style.FontSize ?? defaultStyle.FontSize,
                    Primary = style.Color ?? defaultStyle.Primary,
                    Outline = style.BackgroundColor ?? defaultStyle.Outline,
                    Bold = style.Bold ?? defaultStyle.Bold,
                    Italic = style.Italic ?? defaultStyle.Italic,
                    Underline = style.Underline ?? defaultStyle.Underline,
                    Strikeout = style.StrikeThrough ?? defaultStyle.Strikeout,
                    ShadowWidth = style.ShadowWidth ?? defaultStyle.ShadowWidth,
                    Secondary = style.ShadowColor ?? defaultStyle.Secondary,
                };

                if (newStyle.Outline.A == 0 && style.BackgroundColor.HasValue)
                {
                    newStyle.Background = newStyle.Outline;
                }

                result.Add(newStyle);
            }

            return result;
        }
    }
}
