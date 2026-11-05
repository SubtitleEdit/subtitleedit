using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Renders YouTube timed text (srv3/.ytt) as ASSA for the video preview, as close to YouTube's
    /// desktop player as libass allows.
    /// </summary>
    /// <remarks>
    /// Sizes and positions follow what the player really does (YTSubConverter): text is 32px at 720p,
    /// sz is scaled with 1 + (sz/100 - 1)/4, and coordinates are pulled in to coord * 0.96 + 2 percent.
    /// libass cannot draw a background box and an outline on the same text, so lines with a box keep
    /// only shadow edges, and lines without one get the full edge type.
    /// </remarks>
    public static class YttToAssa
    {
        private const int ReferenceHeight = 720;
        private const double ReferenceFontPixels = 32;
        private const string BoxStyleName = "YttBox";
        private const string NoBoxStyleName = "YttNoBox";

        // CSS font pixels -> ASSA font size (an ASSA size is a line height), YTSubConverter's FontSizeMapper
        private static readonly Dictionary<string, double> LineHeightFactors = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            { "Courier New", 36.0 / 32 },
            { "Times New Roman", 35.0 / 32 },
            { "Lucida Console", 32.0 / 32 },
            { "Roboto", 38.0 / 32 },
            { "Comic Sans MS", 45.0 / 32 },
            { "Monotype Corsiva", 35.0 / 32 },
            { "Arial", 37.0 / 32 },
        };

        public static Subtitle Convert(Subtitle ytt, int videoWidth, int videoHeight)
        {
            var width = videoWidth > 0 && videoHeight > 0 ? videoWidth : 1280;
            var height = videoWidth > 0 && videoHeight > 0 ? videoHeight : ReferenceHeight;
            var head = YttHead.Parse(ytt.Header);
            var baseFontPixels = ReferenceFontPixels * height / ReferenceHeight;

            var boxStyle = new SsaStyle
            {
                Name = BoxStyleName,
                FontName = "Roboto",
                FontSize = (decimal)Math.Round(GetAssaFontSize("Roboto", baseFontPixels), 1),
                Primary = SKColors.White,
                Outline = YttText.FromHtml(YttPen.DefaultBackColor, SKColors.Black).WithAlpha(YttPen.DefaultBackOpacity),
                Background = SKColors.Black.WithAlpha(0),
                BorderStyle = "3",
                OutlineWidth = (decimal)Math.Round(baseFontPixels / 8, 1),
                ShadowWidth = 0,
                Alignment = "2",
                MarginLeft = 0,
                MarginRight = 0,
                MarginVertical = 0,
            };
            var noBoxStyle = new SsaStyle(boxStyle)
            {
                Name = NoBoxStyleName,
                BorderStyle = "1",
                OutlineWidth = 0,
                Outline = SKColors.Black,
            };

            var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, new List<SsaStyle> { boxStyle, noBoxStyle });
            header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + width.ToString(CultureInfo.InvariantCulture), "[Script Info]", header);
            header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + height.ToString(CultureInfo.InvariantCulture), "[Script Info]", header);
            header = AdvancedSubStationAlpha.AddTagToHeader("WrapStyle", "WrapStyle: 0", "[Script Info]", header);

            var assa = new Subtitle(ytt) { Header = header };
            for (var i = 0; i < assa.Paragraphs.Count; i++)
            {
                var p = assa.Paragraphs[i];
                p.Layer = i;
                p.Text = ConvertLine(p, head, width, height, baseFontPixels, out var hasBox);
                p.Extra = hasBox ? BoxStyleName : NoBoxStyleName;
                p.Style = null;
                p.Region = null;
                p.Effect = null;
            }

            return assa;
        }

        private static double GetAssaFontSize(string fontName, double cssPixels)
        {
            return LineHeightFactors.TryGetValue(fontName, out var factor) ? cssPixels * factor : cssPixels * 38 / 32;
        }

        private static string ConvertLine(Paragraph p, YttHead head, int width, int height, double baseFontPixels, out bool hasBox)
        {
            var linePen = YttHead.ParseInt(p.Style);
            var runs = YttText.Parse(p.Text, out var assaAlignment);
            var pens = runs.Select(r => YttText.GetEffectivePen(head, r, linePen)).ToList();
            hasBox = pens.Count == 0 || pens.Any(pen => (pen.BackOpacity ?? YttPen.DefaultBackOpacity) > 0 && pen.Ruby != 2);

            var sb = new StringBuilder();
            sb.Append(GetPositionTags(head.GetWindowPosition(p.Region), assaAlignment, width, height));
            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                var pen = pens[i];
                if (pen.Ruby == 2)
                {
                    continue; // parenthesis around ruby text, for players without ruby
                }

                sb.Append('{').Append(GetPenTags(pen, baseFontPixels, hasBox, run.StartOffset)).Append('}');
                sb.Append(run.Text.Replace("{", "(").Replace("}", ")").Replace("\n", "\\N"));
            }

            return sb.ToString();
        }

        private static string GetPositionTags(YttWindowPosition wp, int? assaAlignment, int width, int height)
        {
            if (wp == null)
            {
                if (assaAlignment != null)
                {
                    var anchorPoint = YttWindowPosition.AssaAlignmentToAnchorPoint(assaAlignment.Value);
                    YttWindowPosition.GetDefaultCoordinates(anchorPoint, out var h, out var v);
                    wp = new YttWindowPosition { AnchorPoint = anchorPoint, Horizontal = h, Vertical = v };
                }
                else
                {
                    wp = new YttWindowPosition();
                }
            }

            var alignment = YttWindowPosition.AnchorPointToAssaAlignment(wp.AnchorPoint ?? YttWindowPosition.DefaultAnchorPoint);
            var x = YttWindowPosition.GetRealPercent(wp.Horizontal ?? YttWindowPosition.DefaultHorizontal) / 100 * width;
            var y = YttWindowPosition.GetRealPercent(wp.Vertical ?? YttWindowPosition.DefaultVertical) / 100 * height;
            return string.Format(CultureInfo.InvariantCulture, "{{\\an{0}\\pos({1:0.#},{2:0.#})}}", alignment, x, y);
        }

        private static string GetPenTags(YttPen pen, double baseFontPixels, bool hasBox, int? startOffset)
        {
            var sb = new StringBuilder();
            var fontName = YttPen.GetFontName(pen.FontStyle);
            var fontPixels = baseFontPixels * YttPen.GetRealScale(pen.Size);
            if (pen.Offset == 0 || pen.Offset == 2 || pen.Ruby == 4 || pen.Ruby == 5)
            {
                fontPixels /= 2;
            }

            sb.Append("\\fn").Append(fontName);
            sb.Append("\\fs").Append(Math.Round(GetAssaFontSize(fontName, fontPixels), 1).ToString(CultureInfo.InvariantCulture));
            sb.Append(pen.Bold == true ? "\\b1" : "\\b0");
            sb.Append(pen.Italic == true ? "\\i1" : "\\i0");
            sb.Append(pen.Underline == true ? "\\u1" : "\\u0");
            if (pen.FontStyle == 7)
            {
                sb.Append("\\fscx90"); // no small caps in ASSA - narrower plain caps is the nearest
            }

            var foreOpacity = pen.ForeOpacity ?? YttPen.DefaultForeOpacity;
            var fore = YttText.FromHtml(pen.ForeColor ?? YttPen.DefaultForeColor, SKColors.White).WithAlpha((byte)foreOpacity);
            var back = YttText.FromHtml(pen.BackColor ?? YttPen.DefaultBackColor, SKColors.Black).WithAlpha((byte)(pen.BackOpacity ?? YttPen.DefaultBackOpacity));
            var edgeOpacity = pen.EdgeColor == null ? foreOpacity : 255;
            var edge = YttText.FromHtml(pen.EdgeColor ?? YttPen.DefaultEdgeColor, SKColors.Black).WithAlpha((byte)edgeOpacity);

            var alphaTags = new StringBuilder();
            alphaTags.Append("\\1a").Append(AlphaHex(fore.Alpha));
            sb.Append("\\1c").Append(ColorHex(fore));

            var edgeSize = Math.Max(1, Math.Round(baseFontPixels / 16, 1)).ToString(CultureInfo.InvariantCulture);
            if (hasBox)
            {
                // the box is the outline of BorderStyle 3
                sb.Append("\\3c").Append(ColorHex(back));
                alphaTags.Append("\\3a").Append(AlphaHex(back.Alpha));
                if (pen.EdgeType == 1 || pen.EdgeType == 4)
                {
                    sb.Append("\\shad").Append(edgeSize).Append("\\4c").Append(ColorHex(edge));
                    alphaTags.Append("\\4a").Append(AlphaHex(edge.Alpha));
                }
                else
                {
                    sb.Append("\\shad0");
                }
            }
            else
            {
                sb.Append("\\bord0\\shad0\\blur0");
                switch (pen.EdgeType)
                {
                    case 1: // hard shadow
                        sb.Append("\\shad").Append(edgeSize).Append("\\4c").Append(ColorHex(edge));
                        alphaTags.Append("\\4a").Append(AlphaHex(edge.Alpha));
                        break;
                    case 2: // bevel
                        sb.Append("\\bord1\\shad1\\3c").Append(ColorHex(edge)).Append("\\4c").Append(ColorHex(edge));
                        alphaTags.Append("\\3a").Append(AlphaHex(edge.Alpha)).Append("\\4a").Append(AlphaHex(edge.Alpha));
                        break;
                    case 3: // glow / outline
                        sb.Append("\\bord").Append(edgeSize).Append("\\blur1\\3c").Append(ColorHex(edge));
                        alphaTags.Append("\\3a").Append(AlphaHex(edge.Alpha));
                        break;
                    case 4: // soft shadow
                        sb.Append("\\shad").Append(edgeSize).Append("\\blur2\\4c").Append(ColorHex(edge));
                        alphaTags.Append("\\4a").Append(AlphaHex(edge.Alpha));
                        break;
                }
            }

            if (startOffset != null && startOffset > 0)
            {
                // karaoke: a timed s appears at its offset
                sb.Append("\\alpha&HFF&\\t(")
                  .Append(startOffset.Value.ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append((startOffset.Value + 1).ToString(CultureInfo.InvariantCulture)).Append(',')
                  .Append(alphaTags).Append(')');
            }
            else
            {
                sb.Append(alphaTags);
            }

            return sb.ToString();
        }

        private static string ColorHex(SKColor c) => $"&H{c.Blue:X2}{c.Green:X2}{c.Red:X2}&";

        private static string AlphaHex(byte alpha) => $"&H{255 - alpha:X2}&";
    }
}
