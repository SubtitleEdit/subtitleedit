using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// YouTube timed text, aka srv3 - what yt-dlp writes for --sub-format srv3 and what
    /// YTSubConverter produces as .ytt. Text can be split over several timed s runs inside
    /// each p element.
    /// </summary>
    /// <remarks>
    /// Pens, window positions and window styles live in the header (see <see cref="YttHead"/>),
    /// parts of a line in another pen are s tags in the text (see <see cref="YttText"/>).
    /// Saving applies the workarounds YTSubConverter found for YouTube's upload and players, so
    /// a file saved here looks the same once uploaded.
    /// </remarks>
    public class YouTubeTimedText : SubtitleFormat
    {
        public override string Extension => ".ytt";

        public override string Name => "YouTube timed text srv3";

        // No parenthesis in the name above - GetSubtitleFormatByFriendlyName truncates there.

        public override List<string> AlternateExtensions => new List<string> { ".srv3", ".xml" };

        public override List<string> AlternateNames => new List<string> { "Unknown 82" };

        public override string ToText(Subtitle subtitle, string title)
        {
            var head = YttHead.Parse(subtitle.Header);
            var body = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                AppendParagraph(body, p, head);
            }

            // LF only - CRLF reportedly breaks the iOS app (YTSubConverter).
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<timedtext format=\"3\">\n");
            head.AppendHead(sb, true);
            sb.Append("<body>\n");
            sb.Append(body);
            sb.Append("</body>\n</timedtext>\n");
            return sb.ToString();
        }

        private static void AppendParagraph(StringBuilder sb, Paragraph p, YttHead head)
        {
            var runs = YttText.Parse(p.Text, out var assaAlignment);

            var linePen = head.GetPen(p.Style)?.Id;
            var wp = head.GetWindowPosition(p.Region)?.Id;
            var ws = head.GetWindowStyle(p.Effect)?.Id;
            if (wp == null && assaAlignment != null && assaAlignment != 2)
            {
                var anchorPoint = YttWindowPosition.AssaAlignmentToAnchorPoint(assaAlignment.Value);
                YttWindowPosition.GetDefaultCoordinates(anchorPoint, out var ah, out var av);
                wp = head.FindOrAddWindowPosition(new YttWindowPosition { AnchorPoint = anchorPoint, Horizontal = ah, Vertical = av });
                if (ws == null)
                {
                    var justify = anchorPoint % 3 == 0 ? 0 : anchorPoint % 3 == 2 ? 1 : 2;
                    ws = head.FindOrAddWindowStyle(new YttWindowStyle { Justify = justify });
                }
            }

            // i/b/u/font become pens
            var pens = new List<int?>();
            foreach (var run in runs)
            {
                if (run.HasHtmlFormatting)
                {
                    pens.Add(head.FindOrAddPen(YttText.GetEffectivePen(head, run, linePen)));
                }
                else
                {
                    pens.Add(run.PenId != null && head.GetPen(run.PenId) != null ? run.PenId : null);
                }
            }

            // a single run in one pen is a line pen
            if (runs.Count == 1 && runs[0].StartOffset == null && runs[0].OtherAttributes.Count == 0 && pens[0] != null)
            {
                linePen = pens[0];
                pens[0] = null;
            }

            var start = (int)Math.Round(p.StartTime.TotalMilliseconds);
            var duration = (int)Math.Round(p.DurationTotalMilliseconds);

            // The Android app ignores the position of (and sometimes doesn't show) lines at 0 ms.
            if (start <= 0)
            {
                duration -= 1 - start;
                start = 1;
            }

            sb.Append("<p t=\"").Append(start.ToString(CultureInfo.InvariantCulture))
              .Append("\" d=\"").Append(Math.Max(1, duration).ToString(CultureInfo.InvariantCulture)).Append('"');
            YttHead.AppendAttribute(sb, "p", linePen);
            YttHead.AppendAttribute(sb, "wp", wp);
            YttHead.AppendAttribute(sb, "ws", ws);
            sb.Append('>');

            var spans = new List<(bool IsSpan, string Xml)>();
            for (var i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                var pen = pens[i];
                var text = YttHead.EncodeText(run.Text);
                if ((pen == null || pen == linePen) && run.StartOffset == null && run.OtherAttributes.Count == 0)
                {
                    spans.Add((false, text));
                    continue;
                }

                var span = new StringBuilder("<s");
                YttHead.AppendAttribute(span, "p", pen ?? linePen);
                YttHead.AppendAttribute(span, "t", run.StartOffset);
                foreach (var kvp in run.OtherAttributes)
                {
                    YttHead.AppendAttribute(span, kvp.Key, kvp.Value);
                }

                span.Append('>').Append(text).Append("</s>");
                spans.Add((true, span.ToString()));
            }

            // Unless a line has text outside any s, the upload removes the first s' pen.
            // A zero-width space after the first s is invisible (YTSubConverter).
            if (spans.Any(s => s.IsSpan) && spans.All(s => s.IsSpan || s.Xml.Length == 0))
            {
                var first = spans.FindIndex(s => s.IsSpan);
                spans.Insert(first + 1, (false, YttText.ZeroWidthSpace));
            }

            foreach (var span in spans)
            {
                sb.Append(span.Xml);
            }

            sb.Append("</p>\n");
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var xmlAsText = string.Join("\n", lines);
            if (!xmlAsText.Contains("</timedtext>") || !xmlAsText.Contains("<p "))
            {
                return;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
                xml.LoadXml(xmlAsText.Trim());
                var head = new YttHead();
                if (xml.DocumentElement.SelectSingleNode("head") is XmlElement headElement)
                {
                    head.ReadHeadElement(headElement);
                }

                foreach (XmlNode node in xml.DocumentElement.SelectNodes("body/p"))
                {
                    try
                    {
                        var element = (XmlElement)node;
                        var start = Convert.ToDouble(element.Attributes["t"].InnerText, CultureInfo.InvariantCulture);
                        var duration = element.Attributes["d"] != null
                            ? Convert.ToDouble(element.Attributes["d"].InnerText, CultureInfo.InvariantCulture)
                            : 5000;

                        // undo the 0 ms workaround (see ToText)
                        if (Math.Abs(start - 1) < 0.001)
                        {
                            start = 0;
                            duration++;
                        }

                        var linePen = YttHead.GetInt(element, "p");
                        var p = new Paragraph(YttText.FromXml(element, linePen), start, start + duration)
                        {
                            Style = linePen?.ToString(CultureInfo.InvariantCulture),
                            Region = YttHead.GetInt(element, "wp")?.ToString(CultureInfo.InvariantCulture),
                            Effect = YttHead.GetInt(element, "ws")?.ToString(CultureInfo.InvariantCulture),
                        };
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        _errorCount++;
                    }
                }

                subtitle.Header = head.ToHeader();
                subtitle.Renumber();
            }
            catch (Exception)
            {
                _errorCount++;
            }
        }

        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            if (newFormat is YouTubeTimedText)
            {
                return;
            }

            var head = YttHead.Parse(subtitle.Header);
            foreach (var p in subtitle.Paragraphs)
            {
                var text = YttText.ToHtml(p.Text, head, YttHead.ParseInt(p.Style));
                var wp = head.GetWindowPosition(p.Region);
                if (wp != null)
                {
                    var alignment = YttWindowPosition.AnchorPointToAssaAlignment(wp.AnchorPoint ?? YttWindowPosition.DefaultAnchorPoint);
                    var vertical = wp.Vertical ?? YttWindowPosition.DefaultVertical;

                    // the anchor point says which corner, the position decides the screen part
                    var row = vertical < 34 ? 7 : vertical < 67 ? 4 : 1;
                    var column = (alignment - 1) % 3;
                    alignment = row + column;
                    if (alignment != 2)
                    {
                        text = "{\\an" + alignment.ToString(CultureInfo.InvariantCulture) + "}" + text;
                    }
                }

                p.Text = text;
                p.Style = null;
                p.Region = null;
                p.Effect = null;
            }

            if (YttHead.IsYttHeader(subtitle.Header))
            {
                subtitle.Header = null;
            }
        }
    }
}
