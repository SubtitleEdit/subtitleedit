using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.Common
{
    /// <summary>
    /// Turns the vertical/horizontal position a source format carries - TTML regions, PAC vertical
    /// alignment and EBU STL teletext rows - into ASSA alignment tags and per line margins.
    /// </summary>
    /// <remarks>
    /// Used for the video preview (mpv/VLC), where every line used to end up at the one fixed
    /// alignment from the preview settings no matter what the file said (discussion #13857).
    /// The paragraphs are expected to be a throw-away preview copy - the text and the margins are
    /// rewritten in place.
    /// </remarks>
    public static class SubtitlePositionToAssa
    {
        /// <summary>
        /// libass' script size when the ASSA header has no PlayResX/PlayResY - which is the case for
        /// the preview header, so every margin written here is in this space.
        /// </summary>
        public const int DefaultAssaPlayResX = 384;

        public const int DefaultAssaPlayResY = 288;

        private static readonly char[] SpaceSeparators = { ' ', '\t', '\r', '\n' };

        // Memo for the parsed TTML layout: the header holds the whole source XML, and a preview
        // refresh happens on every edit - re-parsing a broadcast TTML file per keystroke is not free.
        private static readonly object RegionCacheLock = new object();
        private static string _regionCacheHeader;
        private static Dictionary<string, TtmlRegion> _regionCacheRegions;

        /// <summary>
        /// Applies the position info of <paramref name="sourceHeader"/>'s format to the paragraphs.
        /// </summary>
        /// <param name="subtitle">Preview copy of the subtitle - modified in place.</param>
        /// <param name="sourceHeader">Header as read from the source file (TTML xml, EBU STL GSI block...).</param>
        /// <param name="usePositions">
        /// False only maps the format specific margins away: MarginV holds a teletext row or a percentage
        /// for those formats, and neither means anything as an ASSA pixel margin.
        /// </param>
        /// <returns>True if any paragraph was positioned.</returns>
        public static bool ApplyPositions(Subtitle subtitle, string sourceHeader, bool usePositions = true)
        {
            return ApplyPositions(subtitle, sourceHeader, DefaultAssaPlayResX, DefaultAssaPlayResY, usePositions);
        }

        public static bool ApplyPositions(Subtitle subtitle, string sourceHeader, int playResX, int playResY, bool usePositions = true)
        {
            if (subtitle == null || subtitle.Paragraphs.Count == 0 || playResX <= 0 || playResY <= 0)
            {
                return false;
            }

            // ASSA/SSA margins are already in script units - they are the one thing that must be left alone.
            if (!string.IsNullOrEmpty(sourceHeader) &&
                (sourceHeader.IndexOf("[V4+ Styles]", StringComparison.Ordinal) >= 0 ||
                 sourceHeader.IndexOf("[V4 Styles]", StringComparison.Ordinal) >= 0))
            {
                return false;
            }

            if (IsEbuStlHeader(sourceHeader))
            {
                return ApplyEbuTeletextRows(subtitle, sourceHeader, playResY, usePositions);
            }

            var regions = GetTtmlRegions(sourceHeader);
            if (regions != null)
            {
                return usePositions && ApplyTtmlRegions(subtitle, regions, playResX, playResY);
            }

            return ApplyPercentageMargins(subtitle, playResY, usePositions);
        }

        /// <summary>
        /// PAC stores the vertical position as a percentage in MarginV - measured from the top for the
        /// top alignments and from the bottom for the rest, which is exactly what an ASSA margin means.
        /// </summary>
        private static bool ApplyPercentageMargins(Subtitle subtitle, int playResY, bool usePositions)
        {
            var applied = false;
            foreach (var p in subtitle.Paragraphs)
            {
                var marginV = p.MarginV;
                if (string.IsNullOrEmpty(marginV) || marginV[marginV.Length - 1] != '%')
                {
                    continue;
                }

                if (!usePositions ||
                    !double.TryParse(marginV.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var percentage))
                {
                    p.MarginV = null;
                    continue;
                }

                p.MarginV = ToMargin(percentage, playResY);
                applied = true;
            }

            return applied;
        }

        private static bool IsEbuStlHeader(string header)
        {
            // The verbatim 1024 byte GSI block Ebu.LoadSubtitle keeps: 3 chars code page number,
            // then the disk format code ("STL25.01").
            return header != null &&
                   header.Length == 1024 &&
                   header[3] == 'S' && header[4] == 'T' && header[5] == 'L';
        }

        /// <summary>
        /// EBU STL stores the teletext row the first line is displayed on in MarginV (1 based).
        /// </summary>
        private static bool ApplyEbuTeletextRows(Subtitle subtitle, string header, int playResY, bool usePositions)
        {
            var rows = GetEbuRowCount(header);
            var newLineRows = Math.Max(1, Configuration.Settings.SubtitleSettings.EbuStlNewLineRows);
            var applied = false;

            foreach (var p in subtitle.Paragraphs)
            {
                if (string.IsNullOrEmpty(p.MarginV))
                {
                    continue;
                }

                if (!usePositions ||
                    !int.TryParse(p.MarginV, NumberStyles.Integer, CultureInfo.InvariantCulture, out var row) ||
                    row < 1 || row > rows)
                {
                    // A row number is not a pixel margin - leaving it would nudge every line
                    // by a near random amount.
                    p.MarginV = null;
                    continue;
                }

                var lastRow = row + (Math.Max(1, Utilities.GetNumberOfLines(p.Text)) - 1) * newLineRows;
                var alignment = GetLeadingAlignment(p.Text);
                if (alignment == 0)
                {
                    if (lastRow <= rows / 3)
                    {
                        alignment = 8;
                    }
                    else if (row > rows * 2 / 3)
                    {
                        alignment = 2;
                    }
                    else
                    {
                        alignment = 5;
                    }

                    // The row says how far down the line sits, the justification says which way it
                    // is aligned - without this every line stays centered no matter what the EBU
                    // options dialog is set to.
                    alignment += GetEbuJustificationOffset();

                    p.Text = "{\\an" + alignment.ToString(CultureInfo.InvariantCulture) + "}" + p.Text;
                }

                if (IsTopAlignment(alignment))
                {
                    p.MarginV = ToMargin((row - 1) * 100.0 / rows, playResY);
                }
                else if (IsBottomAlignment(alignment))
                {
                    p.MarginV = ToMargin(Math.Max(0, rows - lastRow) * 100.0 / rows, playResY);
                }
                else
                {
                    p.MarginV = null; // libass centers the line, the margin says nothing
                }

                applied = true;
            }

            return applied;
        }

        /// <summary>
        /// Turns the EBU justification code into a shift of the ASSA alignment column: the codes
        /// are 0 = unchanged, 1 = left, 2 = centered and 3 = right, and each centered alignment
        /// (2, 5 and 8) has its left neighbour one below it and its right one above.
        /// </summary>
        private static int GetEbuJustificationOffset()
        {
            switch (Ebu.EbuUiHelper?.JustificationCode)
            {
                case 1: return -1;
                case 3: return 1;
                default: return 0;
            }
        }

        private static int GetEbuRowCount(string header)
        {
            try
            {
                var ebuHeader = Ebu.ReadHeader(Ebu.GetEncoding(header.Substring(0, 3)).GetBytes(header));
                if (ebuHeader.DisplayStandardCode == "1" || ebuHeader.DisplayStandardCode == "2") // teletext
                {
                    return 23;
                }

                if (int.TryParse(ebuHeader.MaximumNumberOfDisplayableRows, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rows) &&
                    rows > 1)
                {
                    return rows;
                }
            }
            catch
            {
                // ignore - fall back to the teletext row count
            }

            return 23;
        }

        private static bool ApplyTtmlRegions(Subtitle subtitle, Dictionary<string, TtmlRegion> regions, int playResX, int playResY)
        {
            var applied = false;
            foreach (var p in subtitle.Paragraphs)
            {
                var regionId = GetRegionId(p);
                if (string.IsNullOrEmpty(regionId) || !regions.TryGetValue(regionId, out var region))
                {
                    continue;
                }

                // A paragraph may carry its own box (TimedText10 keeps those in the effect).
                var left = region.Left;
                var top = region.Top;
                var width = region.Width;
                var height = region.Height;
                if (TryGetPercentagePair(GetEffect(p, "tts:origin"), region.Scale, out var originX, out var originY))
                {
                    left = originX;
                    top = originY;
                }

                if (TryGetPercentagePair(GetEffect(p, "tts:extent"), region.Scale, out var extentX, out var extentY))
                {
                    width = extentX;
                    height = extentY;
                }

                var alignment = GetLeadingAlignment(p.Text);
                if (alignment == 0)
                {
                    alignment = GetAlignment(region, top, height);
                    p.Text = "{\\an" + alignment.ToString(CultureInfo.InvariantCulture) + "}" + p.Text;
                }

                if (IsTopAlignment(alignment) && top.HasValue)
                {
                    p.MarginV = ToMargin(top.Value, playResY);
                }
                else if (IsBottomAlignment(alignment) && top.HasValue && height.HasValue)
                {
                    p.MarginV = ToMargin(100.0 - (top.Value + height.Value), playResY);
                }
                else
                {
                    p.MarginV = null;
                }

                if (left.HasValue)
                {
                    p.MarginL = ToMargin(left.Value, playResX);
                    if (width.HasValue)
                    {
                        p.MarginR = ToMargin(100.0 - (left.Value + width.Value), playResX);
                    }
                }

                applied = true;
            }

            return applied;
        }

        private static int GetAlignment(TtmlRegion region, double? top, double? height)
        {
            var vertical = region.DisplayAlign;
            if (string.IsNullOrEmpty(vertical))
            {
                // No displayAlign: let the box itself decide, like TimedTextImsc11 does when reading.
                var anchor = (top ?? 90) + (height ?? 0);
                vertical = anchor <= 33 ? "before" : anchor >= 66 ? "after" : "center";
            }

            var horizontal = 2; // center
            switch (region.TextAlign)
            {
                case "left":
                case "start":
                    horizontal = 1;
                    break;
                case "right":
                case "end":
                    horizontal = 3;
                    break;
            }

            switch (vertical)
            {
                case "before":
                    return 6 + horizontal;
                case "center":
                    return 3 + horizontal;
                default: // after
                    return horizontal;
            }
        }

        private static string GetRegionId(Paragraph p)
        {
            if (!string.IsNullOrEmpty(p.Region))
            {
                return p.Region;
            }

            return GetEffect(p, "region");
        }

        private static string GetEffect(Paragraph p, string tag)
        {
            if (string.IsNullOrEmpty(p.Effect))
            {
                return null;
            }

            foreach (var part in p.Effect.Split('|'))
            {
                var index = part.IndexOf('=');
                if (index > 0 && string.CompareOrdinal(part.Substring(0, index), tag) == 0)
                {
                    return part.Substring(index + 1);
                }
            }

            return null;
        }

        private static Dictionary<string, TtmlRegion> GetTtmlRegions(string header)
        {
            if (string.IsNullOrEmpty(header) || header.IndexOf("<region", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return null;
            }

            lock (RegionCacheLock)
            {
                if (ReferenceEquals(header, _regionCacheHeader))
                {
                    return _regionCacheRegions;
                }
            }

            var regions = ParseTtmlRegions(header);

            lock (RegionCacheLock)
            {
                _regionCacheHeader = header;
                _regionCacheRegions = regions;
            }

            return regions;
        }

        private static Dictionary<string, TtmlRegion> ParseTtmlRegions(string header)
        {
            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(header);
                var root = xml.DocumentElement;
                if (root == null)
                {
                    return null;
                }

                var scale = new TtmlScale(GetAttribute(root, "extent"), GetAttribute(root, "cellResolution"));

                var styles = new Dictionary<string, XmlNode>();
                foreach (XmlNode styleNode in xml.GetElementsByTagName("style", "*"))
                {
                    var styleId = GetXmlId(styleNode);
                    if (!string.IsNullOrEmpty(styleId) && !styles.ContainsKey(styleId))
                    {
                        styles.Add(styleId, styleNode);
                    }
                }

                var regions = new Dictionary<string, TtmlRegion>();
                foreach (XmlNode regionNode in xml.GetElementsByTagName("region", "*"))
                {
                    var id = GetXmlId(regionNode);
                    if (string.IsNullOrEmpty(id) || regions.ContainsKey(id))
                    {
                        continue;
                    }

                    var region = new TtmlRegion
                    {
                        Scale = scale,
                        DisplayAlign = GetStyleAttribute(regionNode, styles, "displayAlign"),
                        TextAlign = GetStyleAttribute(regionNode, styles, "textAlign"),
                    };

                    if (TryGetPercentagePair(GetStyleAttribute(regionNode, styles, "origin"), scale, out var left, out var top))
                    {
                        region.Left = left;
                        region.Top = top;
                    }

                    if (TryGetPercentagePair(GetStyleAttribute(regionNode, styles, "extent"), scale, out var width, out var height))
                    {
                        region.Width = width;
                        region.Height = height;
                    }

                    regions.Add(id, region);
                }

                return regions.Count == 0 ? null : regions;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a styling attribute of a region: set on the region itself, on a nested "style" element,
        /// or on a style the region refers to by id.
        /// </summary>
        private static string GetStyleAttribute(XmlNode regionNode, Dictionary<string, XmlNode> styles, string localName)
        {
            var value = GetAttribute(regionNode, localName);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            foreach (XmlNode child in regionNode.ChildNodes)
            {
                if (child.LocalName == "style")
                {
                    value = GetAttribute(child, localName);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            var styleReference = GetAttribute(regionNode, "style");
            if (string.IsNullOrEmpty(styleReference))
            {
                return null;
            }

            foreach (var styleId in styleReference.Split(SpaceSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (styles.TryGetValue(styleId, out var styleNode))
                {
                    value = GetAttribute(styleNode, localName);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Attribute lookup by local name - the namespace prefix in a TTML file is only a convention
        /// ("tts:origin", "origin" and "ttml_tts:origin" all show up in the wild).
        /// </summary>
        private static string GetAttribute(XmlNode node, string localName)
        {
            if (node?.Attributes == null)
            {
                return null;
            }

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.LocalName == localName)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        private static string GetXmlId(XmlNode node)
        {
            return GetAttribute(node, "id");
        }

        /// <summary>
        /// Reads a TTML length pair ("10% 80%", "192px 800px", "10c 2c") as percentages of the screen.
        /// </summary>
        private static bool TryGetPercentagePair(string value, TtmlScale scale, out double x, out double y)
        {
            x = 0;
            y = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var parts = value.Split(SpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 2 &&
                   TryGetPercentage(parts[0], scale.Width, scale.Columns, out x) &&
                   TryGetPercentage(parts[1], scale.Height, scale.Rows, out y);
        }

        private static bool TryGetPercentage(string value, double? pixels, double cells, out double percentage)
        {
            percentage = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var unit = 'p';
            var number = value;
            if (value.EndsWith("%", StringComparison.Ordinal))
            {
                unit = '%';
                number = value.Substring(0, value.Length - 1);
            }
            else if (value.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                number = value.Substring(0, value.Length - 2);
            }
            else if (value.EndsWith("c", StringComparison.OrdinalIgnoreCase))
            {
                unit = 'c';
                number = value.Substring(0, value.Length - 1);
            }

            if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return false;
            }

            switch (unit)
            {
                case '%':
                    percentage = result;
                    return true;
                case 'c':
                    if (cells <= 0)
                    {
                        return false;
                    }

                    percentage = result * 100.0 / cells;
                    return true;
                default: // pixels - only usable when the root element says how big the screen is
                    if (!pixels.HasValue || pixels.Value <= 0)
                    {
                        return false;
                    }

                    percentage = result * 100.0 / pixels.Value;
                    return true;
            }
        }

        /// <summary>
        /// Alignment of a leading "{\anX}" tag, 0 if the text has none.
        /// </summary>
        private static int GetLeadingAlignment(string text)
        {
            if (text == null || text.Length < 6 ||
                !text.StartsWith("{\\an", StringComparison.Ordinal) ||
                text[5] != '}' ||
                text[4] < '1' || text[4] > '9')
            {
                return 0;
            }

            return text[4] - '0';
        }

        private static bool IsTopAlignment(int alignment)
        {
            return alignment >= 7;
        }

        private static bool IsBottomAlignment(int alignment)
        {
            return alignment >= 1 && alignment <= 3;
        }

        private static string ToMargin(double percentage, int playRes)
        {
            var value = (int)Math.Round(percentage / 100.0 * playRes, MidpointRounding.AwayFromZero);
            if (value < 0)
            {
                value = 0;
            }
            else if (value > playRes)
            {
                value = playRes;
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }

        private class TtmlRegion
        {
            internal TtmlScale Scale { get; set; }
            internal double? Left { get; set; }
            internal double? Top { get; set; }
            internal double? Width { get; set; }
            internal double? Height { get; set; }
            internal string DisplayAlign { get; set; }
            internal string TextAlign { get; set; }
        }

        /// <summary>
        /// What the non-percentage units of a TTML document mean: the root extent for pixels and the
        /// cell resolution (32 x 15 per the spec) for cells.
        /// </summary>
        private class TtmlScale
        {
            internal double? Width { get; }
            internal double? Height { get; }
            internal double Columns { get; }
            internal double Rows { get; }

            internal TtmlScale(string rootExtent, string cellResolution)
            {
                Columns = 32;
                Rows = 15;

                if (!string.IsNullOrEmpty(rootExtent))
                {
                    var parts = rootExtent.Split(SpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0].TrimEnd('p', 'x', 'P', 'X'), NumberStyles.Float, CultureInfo.InvariantCulture, out var width) &&
                        double.TryParse(parts[1].TrimEnd('p', 'x', 'P', 'X'), NumberStyles.Float, CultureInfo.InvariantCulture, out var height) &&
                        width > 0 && height > 0)
                    {
                        Width = width;
                        Height = height;
                    }
                }

                if (!string.IsNullOrEmpty(cellResolution))
                {
                    var parts = cellResolution.Split(SpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var columns) &&
                        double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var rows) &&
                        columns > 0 && rows > 0)
                    {
                        Columns = columns;
                        Rows = rows;
                    }
                }
            }
        }
    }
}
