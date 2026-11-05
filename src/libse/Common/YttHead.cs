using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// A YouTube timed text (srv3/.ytt) pen - the text style a p or s element points to with its p attribute.
    /// </summary>
    /// <remarks>
    /// Attribute meanings follow YTSubConverter (MIT, github.com/arcusmaximus/YTSubConverter), which is the
    /// closest thing to a spec. Every value is nullable: an attribute that is not there means "player default",
    /// and the viewer can change the player defaults, so absent and default are not the same thing.
    /// </remarks>
    public class YttPen
    {
        public const string DefaultForeColor = "#FFFFFF";
        public const int DefaultForeOpacity = 254;
        public const string DefaultBackColor = "#080808";
        public const int DefaultBackOpacity = 192;

        /// <summary>Edge color when et is set without ec - this one follows the foreground opacity.</summary>
        public const string DefaultEdgeColor = "#222222";

        public int Id { get; set; }
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }

        /// <summary>fc - "#RRGGBB".</summary>
        public string ForeColor { get; set; }

        /// <summary>fo - 0-255.</summary>
        public int? ForeOpacity { get; set; }

        /// <summary>bc - "#RRGGBB".</summary>
        public string BackColor { get; set; }

        /// <summary>bo - 0-255.</summary>
        public int? BackOpacity { get; set; }

        /// <summary>et - 1 hard shadow, 2 bevel, 3 glow/outline, 4 soft shadow.</summary>
        public int? EdgeType { get; set; }

        /// <summary>ec - "#RRGGBB". Opaque when given, follows fo when not.</summary>
        public string EdgeColor { get; set; }

        /// <summary>fs - 0 default (same as 4), 1 mono serif, 2 serif, 3 mono sans, 4 sans, 5 casual, 6 cursive, 7 small caps.</summary>
        public int? FontStyle { get; set; }

        /// <summary>sz - virtual percentage, see <see cref="GetRealScale"/>.</summary>
        public int? Size { get; set; }

        /// <summary>of - 0 subscript, 1 normal, 2 superscript.</summary>
        public int? Offset { get; set; }

        /// <summary>rb - 1 ruby base, 2 parenthesis, 4 ruby over, 5 ruby under.</summary>
        public int? Ruby { get; set; }

        /// <summary>hg - 1 packs the text into one upright column cell (vertical text).</summary>
        public bool? Packed { get; set; }

        /// <summary>te - text emphasis dots. Rejected by YouTube's upload, only kept for round-trip.</summary>
        public int? TextEmphasis { get; set; }

        /// <summary>Attributes not modeled above, kept as read.</summary>
        public List<KeyValuePair<string, string>> OtherAttributes { get; set; } = new List<KeyValuePair<string, string>>();

        public static readonly string[] FontStyleNames =
        {
            "Default",
            "Courier New",
            "Times New Roman",
            "Lucida Console",
            "Roboto",
            "Comic Sans MS",
            "Monotype Corsiva",
            "Arial (small caps)",
        };

        /// <summary>Font family a renderer should use for <see cref="FontStyle"/>.</summary>
        public static string GetFontName(int? fontStyle)
        {
            switch (fontStyle ?? 0)
            {
                case 1: return "Courier New";
                case 2: return "Times New Roman";
                case 3: return "Lucida Console";
                case 5: return "Comic Sans MS";
                case 6: return "Monotype Corsiva";
                case 7: return "Arial";
                default: return "Roboto";
            }
        }

        /// <summary>
        /// The size the player really draws, as a factor of the default: 1 + (sz/100 - 1) / 4,
        /// so sz=200 is only 125% and the smallest possible (sz=0) is 75%.
        /// </summary>
        public static double GetRealScale(int? size)
        {
            var sz = Math.Max(0, size ?? 100);
            return 1 + (sz / 100.0 - 1) / 4;
        }

        public static int GetSizeFromRealScale(double realScale)
        {
            return (int)Math.Round(Math.Max(1 + (realScale - 1) * 4, 0) * 100);
        }

        public YttPen Clone()
        {
            var pen = (YttPen)MemberwiseClone();
            pen.OtherAttributes = new List<KeyValuePair<string, string>>(OtherAttributes);
            return pen;
        }

        /// <summary>Same look - everything but the id.</summary>
        public bool HasSameFormatting(YttPen other)
        {
            return other != null &&
                   Bold == other.Bold &&
                   Italic == other.Italic &&
                   Underline == other.Underline &&
                   string.Equals(ForeColor, other.ForeColor, StringComparison.OrdinalIgnoreCase) &&
                   ForeOpacity == other.ForeOpacity &&
                   string.Equals(BackColor, other.BackColor, StringComparison.OrdinalIgnoreCase) &&
                   BackOpacity == other.BackOpacity &&
                   EdgeType == other.EdgeType &&
                   string.Equals(EdgeColor, other.EdgeColor, StringComparison.OrdinalIgnoreCase) &&
                   FontStyle == other.FontStyle &&
                   Size == other.Size &&
                   Offset == other.Offset &&
                   Ruby == other.Ruby &&
                   Packed == other.Packed &&
                   TextEmphasis == other.TextEmphasis &&
                   OtherAttributes.SequenceEqual(other.OtherAttributes);
        }

        internal static YttPen Read(XmlElement element)
        {
            var pen = new YttPen { Id = YttHead.GetInt(element, "id") ?? 0 };
            foreach (XmlAttribute attribute in element.Attributes)
            {
                var v = attribute.Value;
                switch (attribute.Name)
                {
                    case "id": break;
                    case "b": pen.Bold = YttHead.ParseBool(v); break;
                    case "i": pen.Italic = YttHead.ParseBool(v); break;
                    case "u": pen.Underline = YttHead.ParseBool(v); break;
                    case "fc": pen.ForeColor = v; break;
                    case "fo": pen.ForeOpacity = YttHead.ParseInt(v); break;
                    case "bc": pen.BackColor = v; break;
                    case "bo": pen.BackOpacity = YttHead.ParseInt(v); break;
                    case "et": pen.EdgeType = YttHead.ParseInt(v); break;
                    case "ec": pen.EdgeColor = v; break;
                    case "fs": pen.FontStyle = YttHead.ParseInt(v); break;
                    case "sz": pen.Size = YttHead.ParseInt(v); break;
                    case "of": pen.Offset = YttHead.ParseInt(v); break;
                    case "rb": pen.Ruby = YttHead.ParseInt(v); break;
                    case "hg": pen.Packed = YttHead.ParseBool(v); break;
                    case "te": pen.TextEmphasis = YttHead.ParseInt(v); break;
                    default: pen.OtherAttributes.Add(new KeyValuePair<string, string>(attribute.Name, v)); break;
                }
            }

            return pen;
        }

        internal void Write(StringBuilder sb, bool uploadSafe)
        {
            var foreColor = ForeColor;
            var foreOpacity = ForeOpacity;
            var backOpacity = BackOpacity;
            if (uploadSafe)
            {
                // The upload drops fo/bo of 255 (and fc #FFFFFF with fo 255), and then the viewer's own
                // settings win. Android also ignores fc="#FFFFFF". See YTSubConverter's LimitColors.
                if (string.Equals(foreColor, "#FFFFFF", StringComparison.OrdinalIgnoreCase))
                {
                    foreColor = "#FEFEFE";
                }

                if (foreOpacity == 255)
                {
                    foreOpacity = 254;
                }

                if (backOpacity == 255)
                {
                    backOpacity = 254;
                }
            }

            sb.Append("    <pen id=\"").Append(Id.ToString(CultureInfo.InvariantCulture)).Append('"');
            YttHead.AppendAttribute(sb, "b", Bold);
            YttHead.AppendAttribute(sb, "i", Italic);
            YttHead.AppendAttribute(sb, "u", Underline);
            YttHead.AppendAttribute(sb, "fc", foreColor);
            YttHead.AppendAttribute(sb, "fo", foreOpacity);
            YttHead.AppendAttribute(sb, "bc", BackColor);
            YttHead.AppendAttribute(sb, "bo", backOpacity);
            YttHead.AppendAttribute(sb, "et", EdgeType);
            YttHead.AppendAttribute(sb, "ec", EdgeColor);
            YttHead.AppendAttribute(sb, "fs", FontStyle);
            YttHead.AppendAttribute(sb, "sz", Size);
            YttHead.AppendAttribute(sb, "of", Offset);
            YttHead.AppendAttribute(sb, "rb", Ruby);
            YttHead.AppendAttribute(sb, "hg", Packed);
            YttHead.AppendAttribute(sb, "te", TextEmphasis);
            foreach (var kvp in OtherAttributes)
            {
                YttHead.AppendAttribute(sb, kvp.Key, kvp.Value);
            }

            sb.Append(" />\n");
        }
    }

    /// <summary>
    /// A YouTube timed text window position (wp) - where a p element is placed.
    /// </summary>
    public class YttWindowPosition
    {
        public const int DefaultAnchorPoint = 7;
        public const int DefaultHorizontal = 50;
        public const int DefaultVertical = 100;

        public int Id { get; set; }

        /// <summary>ap - the point of the text box placed at ah/av: 0 1 2 / 3 4 5 / 6 7 8 from top left.</summary>
        public int? AnchorPoint { get; set; }

        /// <summary>ah - 0 (left) to 100 (right), integers only.</summary>
        public int? Horizontal { get; set; }

        /// <summary>av - 0 (top) to 100 (bottom), integers only.</summary>
        public int? Vertical { get; set; }

        public List<KeyValuePair<string, string>> OtherAttributes { get; set; } = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Where the player really draws a coordinate, in percent of the video: it pulls everything
        /// towards the center with coord * 0.96 + 2, so 0 is 2% and 100 is 98%.
        /// </summary>
        public static double GetRealPercent(int coordinate)
        {
            return Math.Max(0, Math.Min(100, coordinate)) * 0.96 + 2;
        }

        /// <summary>Inverse of <see cref="GetRealPercent"/>, rounded to what the upload accepts.</summary>
        public static int GetCoordinateFromRealPercent(double percent)
        {
            return (int)Math.Round(Math.Max(0, Math.Min(100, (percent - 2) / 0.96)));
        }

        /// <summary>ASSA \an value for an ap value.</summary>
        public static int AnchorPointToAssaAlignment(int anchorPoint)
        {
            switch (anchorPoint)
            {
                case 0: return 7;
                case 1: return 8;
                case 2: return 9;
                case 3: return 4;
                case 4: return 5;
                case 5: return 6;
                case 6: return 1;
                case 8: return 3;
                default: return 2;
            }
        }

        /// <summary>ap value for an ASSA \an value.</summary>
        public static int AssaAlignmentToAnchorPoint(int alignment)
        {
            switch (alignment)
            {
                case 7: return 0;
                case 8: return 1;
                case 9: return 2;
                case 4: return 3;
                case 5: return 4;
                case 6: return 5;
                case 1: return 6;
                case 3: return 8;
                default: return 7;
            }
        }

        /// <summary>The ah/av an anchor point sits at by default - its corner/edge of the safe area.</summary>
        public static void GetDefaultCoordinates(int anchorPoint, out int horizontal, out int vertical)
        {
            horizontal = anchorPoint % 3 == 0 ? 0 : anchorPoint % 3 == 1 ? 50 : 100;
            vertical = anchorPoint < 3 ? 0 : anchorPoint < 6 ? 50 : 100;
        }

        public YttWindowPosition Clone()
        {
            var wp = (YttWindowPosition)MemberwiseClone();
            wp.OtherAttributes = new List<KeyValuePair<string, string>>(OtherAttributes);
            return wp;
        }

        public bool HasSamePosition(YttWindowPosition other)
        {
            return other != null &&
                   (AnchorPoint ?? DefaultAnchorPoint) == (other.AnchorPoint ?? DefaultAnchorPoint) &&
                   (Horizontal ?? DefaultHorizontal) == (other.Horizontal ?? DefaultHorizontal) &&
                   (Vertical ?? DefaultVertical) == (other.Vertical ?? DefaultVertical) &&
                   OtherAttributes.SequenceEqual(other.OtherAttributes);
        }

        internal static YttWindowPosition Read(XmlElement element)
        {
            var wp = new YttWindowPosition { Id = YttHead.GetInt(element, "id") ?? 0 };
            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name)
                {
                    case "id": break;
                    case "ap": wp.AnchorPoint = YttHead.ParseInt(attribute.Value); break;
                    case "ah": wp.Horizontal = YttHead.ParseInt(attribute.Value); break;
                    case "av": wp.Vertical = YttHead.ParseInt(attribute.Value); break;
                    default: wp.OtherAttributes.Add(new KeyValuePair<string, string>(attribute.Name, attribute.Value)); break;
                }
            }

            return wp;
        }

        internal void Write(StringBuilder sb)
        {
            sb.Append("    <wp id=\"").Append(Id.ToString(CultureInfo.InvariantCulture)).Append('"');
            YttHead.AppendAttribute(sb, "ap", AnchorPoint);
            YttHead.AppendAttribute(sb, "ah", Horizontal);
            YttHead.AppendAttribute(sb, "av", Vertical);
            foreach (var kvp in OtherAttributes)
            {
                YttHead.AppendAttribute(sb, kvp.Key, kvp.Value);
            }

            sb.Append(" />\n");
        }
    }

    /// <summary>
    /// A YouTube timed text window style (ws) - justification and text direction of a p element.
    /// </summary>
    public class YttWindowStyle
    {
        public int Id { get; set; }

        /// <summary>ju - 0 left, 1 right, 2 center.</summary>
        public int? Justify { get; set; }

        /// <summary>pd - 0 left-to-right, 1 right-to-left, 2 vertical upright, 3 vertical rotated 90°.</summary>
        public int? PrintDirection { get; set; }

        /// <summary>sd - column order for vertical text (0/1).</summary>
        public int? ScrollDirection { get; set; }

        /// <summary>wfo - opacity of the secondary "window" box. 0 hides it whatever the viewer set.</summary>
        public int? WindowOpacity { get; set; }

        public List<KeyValuePair<string, string>> OtherAttributes { get; set; } = new List<KeyValuePair<string, string>>();

        public YttWindowStyle Clone()
        {
            var ws = (YttWindowStyle)MemberwiseClone();
            ws.OtherAttributes = new List<KeyValuePair<string, string>>(OtherAttributes);
            return ws;
        }

        public bool HasSameStyle(YttWindowStyle other)
        {
            return other != null &&
                   Justify == other.Justify &&
                   PrintDirection == other.PrintDirection &&
                   ScrollDirection == other.ScrollDirection &&
                   WindowOpacity == other.WindowOpacity &&
                   OtherAttributes.SequenceEqual(other.OtherAttributes);
        }

        internal static YttWindowStyle Read(XmlElement element)
        {
            var ws = new YttWindowStyle { Id = YttHead.GetInt(element, "id") ?? 0 };
            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name)
                {
                    case "id": break;
                    case "ju": ws.Justify = YttHead.ParseInt(attribute.Value); break;
                    case "pd": ws.PrintDirection = YttHead.ParseInt(attribute.Value); break;
                    case "sd": ws.ScrollDirection = YttHead.ParseInt(attribute.Value); break;
                    case "wfo": ws.WindowOpacity = YttHead.ParseInt(attribute.Value); break;
                    default: ws.OtherAttributes.Add(new KeyValuePair<string, string>(attribute.Name, attribute.Value)); break;
                }
            }

            return ws;
        }

        internal void Write(StringBuilder sb)
        {
            sb.Append("    <ws id=\"").Append(Id.ToString(CultureInfo.InvariantCulture)).Append('"');
            YttHead.AppendAttribute(sb, "ju", Justify);
            YttHead.AppendAttribute(sb, "pd", PrintDirection);
            YttHead.AppendAttribute(sb, "sd", ScrollDirection);
            YttHead.AppendAttribute(sb, "wfo", WindowOpacity);
            foreach (var kvp in OtherAttributes)
            {
                YttHead.AppendAttribute(sb, kvp.Key, kvp.Value);
            }

            sb.Append(" />\n");
        }
    }

    /// <summary>
    /// The head of a YouTube timed text file: pens, window positions and window styles.
    /// </summary>
    /// <remarks>
    /// Kept in <see cref="Subtitle.Header"/> as a body-less timedtext document. Each line points to its
    /// entries by id: <see cref="Paragraph.Style"/> is the line pen, <see cref="Paragraph.Region"/> the
    /// window position and <see cref="Paragraph.Effect"/> the window style. Pens of parts of a line are
    /// inline s tags in the text.
    /// </remarks>
    public class YttHead
    {
        public List<YttPen> Pens { get; } = new List<YttPen>();
        public List<YttWindowPosition> WindowPositions { get; } = new List<YttWindowPosition>();
        public List<YttWindowStyle> WindowStyles { get; } = new List<YttWindowStyle>();

        public static bool IsYttHeader(string header)
        {
            return !string.IsNullOrEmpty(header) && header.Contains("<timedtext");
        }

        /// <summary>Parses a header - an empty head for null or a header of another format.</summary>
        public static YttHead Parse(string header)
        {
            var head = new YttHead();
            if (!IsYttHeader(header))
            {
                return head;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(header);
                var headNode = xml.DocumentElement?.SelectSingleNode("head");
                if (headNode != null)
                {
                    head.ReadHeadElement((XmlElement)headNode);
                }
            }
            catch (XmlException)
            {
                // an unreadable header is the same as none
            }

            return head;
        }

        internal void ReadHeadElement(XmlElement headElement)
        {
            foreach (var element in headElement.ChildNodes.OfType<XmlElement>())
            {
                switch (element.LocalName)
                {
                    case "pen":
                        Replace(Pens, YttPen.Read(element), p => p.Id);
                        break;
                    case "wp":
                        Replace(WindowPositions, YttWindowPosition.Read(element), p => p.Id);
                        break;
                    case "ws":
                        Replace(WindowStyles, YttWindowStyle.Read(element), p => p.Id);
                        break;
                }
            }
        }

        private static void Replace<T>(List<T> list, T item, Func<T, int> getId)
        {
            var index = list.FindIndex(p => getId(p) == getId(item));
            if (index >= 0)
            {
                list[index] = item; // last one wins, like the player
            }
            else
            {
                list.Add(item);
            }
        }

        public YttHead Clone()
        {
            var head = new YttHead();
            head.Pens.AddRange(Pens.Select(p => p.Clone()));
            head.WindowPositions.AddRange(WindowPositions.Select(p => p.Clone()));
            head.WindowStyles.AddRange(WindowStyles.Select(p => p.Clone()));
            return head;
        }

        public YttPen GetPen(int? id) => id == null ? null : Pens.FirstOrDefault(p => p.Id == id.Value);
        public YttPen GetPen(string id) => GetPen(ParseInt(id));
        public YttWindowPosition GetWindowPosition(int? id) => id == null ? null : WindowPositions.FirstOrDefault(p => p.Id == id.Value);
        public YttWindowPosition GetWindowPosition(string id) => GetWindowPosition(ParseInt(id));
        public YttWindowStyle GetWindowStyle(int? id) => id == null ? null : WindowStyles.FirstOrDefault(p => p.Id == id.Value);
        public YttWindowStyle GetWindowStyle(string id) => GetWindowStyle(ParseInt(id));

        public int GetNextPenId() => Pens.Count == 0 ? 1 : Math.Max(1, Pens.Max(p => p.Id) + 1);
        public int GetNextWindowPositionId() => WindowPositions.Count == 0 ? 1 : Math.Max(1, WindowPositions.Max(p => p.Id) + 1);
        public int GetNextWindowStyleId() => WindowStyles.Count == 0 ? 1 : Math.Max(1, WindowStyles.Max(p => p.Id) + 1);

        /// <summary>
        /// Id of a pen formatted like <paramref name="template"/> - an existing one if any (id 0 is
        /// never reused, it is the placeholder some players ignore), else a new one.
        /// </summary>
        public int FindOrAddPen(YttPen template)
        {
            var existing = Pens.FirstOrDefault(p => p.Id != 0 && p.HasSameFormatting(template));
            if (existing != null)
            {
                return existing.Id;
            }

            var pen = template.Clone();
            pen.Id = GetNextPenId();
            Pens.Add(pen);
            return pen.Id;
        }

        public int FindOrAddWindowPosition(YttWindowPosition template)
        {
            var existing = WindowPositions.FirstOrDefault(p => p.Id != 0 && p.HasSamePosition(template));
            if (existing != null)
            {
                return existing.Id;
            }

            var wp = template.Clone();
            wp.Id = GetNextWindowPositionId();
            WindowPositions.Add(wp);
            return wp.Id;
        }

        public int FindOrAddWindowStyle(YttWindowStyle template)
        {
            var existing = WindowStyles.FirstOrDefault(p => p.Id != 0 && p.HasSameStyle(template));
            if (existing != null)
            {
                return existing.Id;
            }

            var ws = template.Clone();
            ws.Id = GetNextWindowStyleId();
            WindowStyles.Add(ws);
            return ws.Id;
        }

        /// <summary>The header string kept in <see cref="Subtitle.Header"/>.</summary>
        public string ToHeader()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<timedtext format=\"3\">\n");
            AppendHead(sb, false);
            sb.Append("</timedtext>\n");
            return sb.ToString();
        }

        /// <summary>
        /// Writes the head element. Upload-safe adds what YouTube's player and upload need (see
        /// YTSubConverter's WriteHead): a placeholder entry with id 0 in each list (the iOS app ignores
        /// the background of the first pen), and increasing ids (or the upload renumbers them).
        /// </summary>
        internal void AppendHead(StringBuilder sb, bool uploadSafe)
        {
            sb.Append("<head>\n");
            var positions = WindowPositions.ToList();
            var styles = WindowStyles.ToList();
            var pens = Pens.ToList();
            if (uploadSafe)
            {
                if (positions.All(p => p.Id != 0))
                {
                    positions.Add(new YttWindowPosition { Id = 0, AnchorPoint = YttWindowPosition.DefaultAnchorPoint, Horizontal = YttWindowPosition.DefaultHorizontal, Vertical = YttWindowPosition.DefaultVertical });
                }

                if (styles.All(p => p.Id != 0))
                {
                    styles.Add(new YttWindowStyle { Id = 0 });
                }

                if (pens.All(p => p.Id != 0))
                {
                    pens.Add(new YttPen { Id = 0 });
                }
            }

            foreach (var wp in positions.OrderBy(p => p.Id))
            {
                wp.Write(sb);
            }

            foreach (var ws in styles.OrderBy(p => p.Id))
            {
                ws.Write(sb);
            }

            foreach (var pen in pens.OrderBy(p => p.Id))
            {
                pen.Write(sb, uploadSafe);
            }

            sb.Append("</head>\n");
        }

        internal static int? GetInt(XmlElement element, string name)
        {
            var attribute = element.Attributes[name];
            return attribute == null ? (int?)null : ParseInt(attribute.Value);
        }

        public static int? ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            if (int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            {
                return i;
            }

            // the player accepts floating point coordinates, the upload does not
            if (double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                return (int)Math.Round(d);
            }

            return null;
        }

        internal static bool? ParseBool(string s)
        {
            var i = ParseInt(s);
            return i == null ? (bool?)null : i.Value != 0;
        }

        internal static void AppendAttribute(StringBuilder sb, string name, int? value)
        {
            if (value != null)
            {
                sb.Append(' ').Append(name).Append("=\"").Append(value.Value.ToString(CultureInfo.InvariantCulture)).Append('"');
            }
        }

        internal static void AppendAttribute(StringBuilder sb, string name, bool? value)
        {
            if (value != null)
            {
                sb.Append(' ').Append(name).Append("=\"").Append(value.Value ? '1' : '0').Append('"');
            }
        }

        internal static void AppendAttribute(StringBuilder sb, string name, string value)
        {
            if (value != null)
            {
                sb.Append(' ').Append(name).Append("=\"").Append(EncodeAttribute(value)).Append('"');
            }
        }

        internal static string EncodeAttribute(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace("\"", "&quot;");
        }

        internal static string EncodeText(string s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
