using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// EBU-TT Part 1 (EBU Tech 3350) - the XML based successor of the EBU STL file for subtitle
    /// production and exchange. Unlike EBU-TT-D (the distribution profile, <see cref="EbuTtD"/>),
    /// a Part 1 document carries the full teletext feature set of an STL, so STL work survives the
    /// trip: the mapping follows EBU Tech 3360 - colours as referential span styles, the boxed
    /// look as a black span background, double height as a "1c 2c" cell font size, and the
    /// teletext row a subtitle starts on as a region per row on the 40x24 teletext cell grid.
    /// Styling is referential only (tts:* attributes live on tt:style elements, never inline).
    /// Spec: https://tech.ebu.ch/publications/tech3350
    /// </summary>
    public class EbuTt : SubtitleFormat
    {
        public override string Name => "EBU-TT";

        public override string Extension => ".xml";

        // The reader stores the teletext row a subtitle starts on in MarginV and the region id in
        // Paragraph.Region - the video preview positions from them like it does for an STL.
        public override bool HasPositionSupport => true;

        private const string TtmlNamespace = "http://www.w3.org/ns/ttml";
        private const string TtmlStylingNamespace = "http://www.w3.org/ns/ttml#styling";
        private const string EbuttmNamespace = "urn:ebu:tt:metadata";

        // Carries what EBU-TT has no element for - the DVB teletext page and language of a
        // .dvbttx source. A foreign namespace inside tt:metadata is valid TTML; other tools
        // ignore it.
        private const string SeMetadataNamespace = "urn:subtitleedit:metadata";

        // The teletext cell grid of EBU Tech 3360 - 40 columns, 24 rows (row 0 is the header row,
        // subtitles use rows 1-23 like the VerticalPosition byte of an STL TTI block).
        private const int CellRows = 24;

        private static string GetXmlStructure()
        {
            return @"<?xml version='1.0' encoding='UTF-8'?>
<tt xmlns='http://www.w3.org/ns/ttml' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:ebuttdt='urn:ebu:tt:datatypes' xmlns:ebutts='urn:ebu:tt:style' xmlns:ebuttm='urn:ebu:tt:metadata' ttp:timeBase='media' ttp:cellResolution='40 24' xml:lang='en'>
  <head>
    <metadata>
      <ebuttm:documentMetadata>
        <ebuttm:documentEbuttVersion>v1.0</ebuttm:documentEbuttVersion>
      </ebuttm:documentMetadata>
    </metadata>
    <styling>
      <style xml:id='defaultStyle' tts:fontFamily='monospaceSansSerif' tts:fontSize='1c 1c' tts:lineHeight='normal' tts:textAlign='center' tts:color='#ffffff' tts:backgroundColor='transparent' tts:fontStyle='normal' tts:fontWeight='normal'/>
    </styling>
    <layout>
      <region xml:id='bottom' tts:origin='10% 10%' tts:extent='80% 80%' tts:displayAlign='after' tts:overflow='visible'/>
    </layout>
  </head>
  <body style='defaultStyle'>
    <div>
    </div>
  </body>
</tt>
".Replace('\'', '"');
        }

        /// <summary>
        /// True when <paramref name="header"/> is an EBU-TT Part 1 document, as kept on the
        /// subtitle by <see cref="LoadSubtitle"/> - the marker element ebuttm:documentEbuttVersion
        /// is required in Part 1 and absent from EBU-TT-D documents.
        /// </summary>
        public static bool IsEbuTtHeader(string header)
        {
            return header != null &&
                   header.Contains("documentEbuttVersion", StringComparison.Ordinal) &&
                   header.Contains("www.w3.org/ns/ttml", StringComparison.Ordinal);
        }

        /// <summary>
        /// Drops what only the teletext formats understand when converting away: the box tags, the
        /// teletext row in MarginV and the region id. EBU STL and DVB teletext keep all of it -
        /// exchanging a subtitle between the three teletext capable formats is the point of EBU-TT.
        /// </summary>
        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            if (newFormat is Ebu || newFormat is DvbTeletext || newFormat is EbuTt)
            {
                return;
            }

            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Text != null && p.Text.Contains("<box>", StringComparison.Ordinal))
                {
                    p.Text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
                }

                p.MarginV = null;
                p.Region = null;
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !(fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ||
                                      fileName.EndsWith(".ttml", StringComparison.OrdinalIgnoreCase) ||
                                      fileName.EndsWith(".dfxp", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var text = JoinLines(lines);
            if (!text.Contains("documentEbuttVersion", StringComparison.Ordinal))
            {
                return false;
            }

            if (text.Contains("urn:ebu:tt:distribution", StringComparison.Ordinal))
            {
                return false; // conforms to the distribution profile - EBU-TT-D reads it
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var xml = new XmlDocument { XmlResolver = null };
            var xmlStructure = GetXmlStructure();

            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            xmlStructure = xmlStructure.Replace("xml:lang=\"en\"", $"xml:lang=\"{language}\"");
            xml.LoadXml(xmlStructure);

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", TtmlNamespace);
            namespaceManager.AddNamespace("ebuttm", EbuttmNamespace);

            // GSI titles, translator, publisher etc. of an STL source (or the metadata of an
            // EBU-TT source, or the page/language of a .dvbttx source) travel in the document
            // metadata.
            var metadataNode = xml.DocumentElement.SelectSingleNode("ttml:head/ttml:metadata", namespaceManager);
            var documentMetadataNode = metadataNode.SelectSingleNode("ebuttm:documentMetadata", namespaceManager);
            DocumentMetadata.FromHeader(subtitle.Header).WriteTo(xml, metadataNode, documentMetadataNode);

            var styling = xml.DocumentElement.SelectSingleNode("ttml:head/ttml:styling", namespaceManager);
            var layout = xml.DocumentElement.SelectSingleNode("ttml:head/ttml:layout", namespaceManager);
            var div = xml.DocumentElement.SelectSingleNode("ttml:body/ttml:div", namespaceManager);

            // The teletext look (all lines boxed, double height) is a property of the source file,
            // seeded into the settings when an STL, a .dvbttx or an EBU-TT document is loaded - it
            // must not leak onto a subtitle that never was teletext.
            var teletextSource = IsTeletextRowSource(subtitle.Header);
            var boxAll = teletextSource && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox;
            if (teletextSource && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
            {
                var defaultStyle = styling.SelectSingleNode("ttml:style[@xml:id='defaultStyle']", namespaceManager);
                SetTtsAttribute(xml, defaultStyle, "fontSize", "1c 2c");
            }

            var context = new WriteContext(xml, styling, layout, teletextSource);
            var count = 1;
            foreach (var p in subtitle.Paragraphs)
            {
                div.AppendChild(MakeParagraph(context, p, count, boxAll));
                count++;
            }

            return ToUtf8XmlString(xml).Replace(" xmlns=\"\"", string.Empty);
        }

        /// <summary>
        /// The document level metadata that travels with the subtitle - the GSI block fields of
        /// an STL mapped to their ebuttm:documentMetadata elements (Tech 3350 models them on the
        /// GSI block), plus the DVB teletext page/language of a .dvbttx source.
        /// </summary>
        private sealed class DocumentMetadata
        {
            public string OriginalProgrammeTitle;
            public string OriginalEpisodeTitle;
            public string TranslatedProgrammeTitle;
            public string TranslatedEpisodeTitle;
            public string TranslatorsName;
            public string TranslatorsContactDetails;
            public string SubtitleListReferenceCode;
            public string CreationDate;      // ISO yyyy-MM-dd (xs:date)
            public string RevisionDate;      // ISO yyyy-MM-dd (xs:date)
            public string RevisionNumber;    // integer as string
            public string StartOfProgramme;  // SMPTE HH:MM:SS:FF
            public string CountryOfOrigin;
            public string Publisher;
            public string EditorsName;
            public string EditorsContactDetails;
            public int? TeletextPage;
            public string TeletextLanguage;
            public bool TeletextHearingImpaired;

            public static DocumentMetadata FromHeader(string header)
            {
                if (Ebu.IsStlHeader(header))
                {
                    return FromStlHeader(header);
                }

                if (IsEbuTtHeader(header))
                {
                    return FromEbuTtDocument(header);
                }

                var metadata = new DocumentMetadata();
                if (DvbTeletext.TryParseHeader(header, out var page, out var language, out var hearingImpaired))
                {
                    metadata.TeletextPage = page;
                    metadata.TeletextLanguage = language;
                    metadata.TeletextHearingImpaired = hearingImpaired;
                }

                return metadata;
            }

            private static DocumentMetadata FromStlHeader(string header)
            {
                var metadata = new DocumentMetadata();
                try
                {
                    var gsi = Ebu.ReadHeader(Ebu.GetEncoding(header.Substring(0, 3)).GetBytes(header));
                    metadata.OriginalProgrammeTitle = Clean(gsi.OriginalProgrammeTitle);
                    metadata.OriginalEpisodeTitle = Clean(gsi.OriginalEpisodeTitle);
                    metadata.TranslatedProgrammeTitle = Clean(gsi.TranslatedProgrammeTitle);
                    metadata.TranslatedEpisodeTitle = Clean(gsi.TranslatedEpisodeTitle);
                    metadata.TranslatorsName = Clean(gsi.TranslatorsName);
                    metadata.TranslatorsContactDetails = Clean(gsi.TranslatorsContactDetails);
                    metadata.SubtitleListReferenceCode = Clean(gsi.SubtitleListReferenceCode);
                    metadata.CreationDate = GsiDateToIso(gsi.CreationDate);
                    metadata.RevisionDate = GsiDateToIso(gsi.RevisionDate);
                    if (int.TryParse((gsi.RevisionNumber ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var revision) && revision > 0)
                    {
                        metadata.RevisionNumber = revision.ToString(CultureInfo.InvariantCulture);
                    }

                    metadata.StartOfProgramme = GsiTimeCodeToSmpte(gsi.TimeCodeStartOfProgramme);
                    metadata.CountryOfOrigin = Clean(gsi.CountryOfOrigin);
                    metadata.Publisher = Clean(gsi.Publisher);
                    metadata.EditorsName = Clean(gsi.EditorsName);
                    metadata.EditorsContactDetails = Clean(gsi.EditorsContactDetails);
                }
                catch
                {
                    // an unreadable GSI block only costs the metadata, never the save
                }

                return metadata;
            }

            public static DocumentMetadata FromEbuTtDocument(string header)
            {
                var metadata = new DocumentMetadata();
                try
                {
                    var xml = new XmlDocument { XmlResolver = null };
                    xml.LoadXml(header);

                    foreach (XmlNode node in xml.GetElementsByTagName("documentMetadata", EbuttmNamespace))
                    {
                        foreach (XmlNode child in node.ChildNodes)
                        {
                            var value = child.InnerText?.Trim();
                            if (string.IsNullOrEmpty(value))
                            {
                                continue;
                            }

                            switch (child.LocalName)
                            {
                                case "documentOriginalProgrammeTitle": metadata.OriginalProgrammeTitle = value; break;
                                case "documentOriginalEpisodeTitle": metadata.OriginalEpisodeTitle = value; break;
                                case "documentTranslatedProgrammeTitle": metadata.TranslatedProgrammeTitle = value; break;
                                case "documentTranslatedEpisodeTitle": metadata.TranslatedEpisodeTitle = value; break;
                                case "documentTranslatorsName": metadata.TranslatorsName = value; break;
                                case "documentTranslatorsContactDetails": metadata.TranslatorsContactDetails = value; break;
                                case "documentSubtitleListReferenceCode": metadata.SubtitleListReferenceCode = value; break;
                                case "documentCreationDate": metadata.CreationDate = value; break;
                                case "documentRevisionDate": metadata.RevisionDate = value; break;
                                case "documentRevisionNumber": metadata.RevisionNumber = value; break;
                                case "documentStartOfProgramme": metadata.StartOfProgramme = value; break;
                                case "documentCountryOfOrigin": metadata.CountryOfOrigin = value; break;
                                case "documentPublisher": metadata.Publisher = value; break;
                                case "documentEditorsName": metadata.EditorsName = value; break;
                                case "documentEditorsContactDetails": metadata.EditorsContactDetails = value; break;
                            }
                        }
                    }

                    foreach (XmlNode node in xml.GetElementsByTagName("teletext", SeMetadataNamespace))
                    {
                        if (int.TryParse(node.Attributes?["page"]?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var page) &&
                            page >= 100 && page <= 899)
                        {
                            metadata.TeletextPage = page;
                            metadata.TeletextLanguage = node.Attributes?["language"]?.Value;
                            metadata.TeletextHearingImpaired = string.Equals(node.Attributes?["type"]?.Value, DvbTeletext.HeaderTypeHearingImpaired, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
                catch
                {
                    // a damaged source document only costs the metadata
                }

                return metadata;
            }

            /// <summary>
            /// Appends the elements to ebuttm:documentMetadata in the schema order of Tech 3350,
            /// and the teletext page/language as a namespaced sibling of documentMetadata.
            /// </summary>
            public void WriteTo(XmlDocument xml, XmlNode metadataNode, XmlNode documentMetadataNode)
            {
                void Append(string name, string value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return;
                    }

                    var element = xml.CreateElement("ebuttm", name, EbuttmNamespace);
                    element.InnerText = value;
                    documentMetadataNode.AppendChild(element);
                }

                Append("documentOriginatingSystem", "Subtitle Edit");
                Append("documentOriginalProgrammeTitle", OriginalProgrammeTitle);
                Append("documentOriginalEpisodeTitle", OriginalEpisodeTitle);
                Append("documentTranslatedProgrammeTitle", TranslatedProgrammeTitle);
                Append("documentTranslatedEpisodeTitle", TranslatedEpisodeTitle);
                Append("documentTranslatorsName", TranslatorsName);
                Append("documentTranslatorsContactDetails", TranslatorsContactDetails);
                Append("documentSubtitleListReferenceCode", SubtitleListReferenceCode);
                Append("documentCreationDate", CreationDate);
                Append("documentRevisionDate", RevisionDate);
                Append("documentRevisionNumber", RevisionNumber);
                Append("documentStartOfProgramme", StartOfProgramme);
                Append("documentCountryOfOrigin", CountryOfOrigin);
                Append("documentPublisher", Publisher);
                Append("documentEditorsName", EditorsName);
                Append("documentEditorsContactDetails", EditorsContactDetails);

                if (TeletextPage.HasValue)
                {
                    var teletext = xml.CreateElement("sem", "teletext", SeMetadataNamespace);
                    var pageAttribute = xml.CreateAttribute("page");
                    pageAttribute.InnerText = TeletextPage.Value.ToString(CultureInfo.InvariantCulture);
                    teletext.Attributes.Append(pageAttribute);
                    if (!string.IsNullOrEmpty(TeletextLanguage))
                    {
                        var languageAttribute = xml.CreateAttribute("language");
                        languageAttribute.InnerText = TeletextLanguage;
                        teletext.Attributes.Append(languageAttribute);
                    }

                    if (TeletextHearingImpaired)
                    {
                        var typeAttribute = xml.CreateAttribute("type");
                        typeAttribute.InnerText = DvbTeletext.HeaderTypeHearingImpaired;
                        teletext.Attributes.Append(typeAttribute);
                    }

                    metadataNode.AppendChild(teletext);
                }
            }

            private static string Clean(string value)
            {
                value = value?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            /// <summary>
            /// A GSI YYMMDD date as the ISO date xs:date wants - STL predates 2000, so 70-99 read
            /// as 19xx.
            /// </summary>
            private static string GsiDateToIso(string gsiDate)
            {
                gsiDate = gsiDate?.Trim();
                if (gsiDate == null || gsiDate.Length != 6 ||
                    !int.TryParse(gsiDate.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) ||
                    !int.TryParse(gsiDate.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var month) ||
                    !int.TryParse(gsiDate.Substring(4, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var day) ||
                    month < 1 || month > 12 || day < 1 || day > 31)
                {
                    return null;
                }

                year += year >= 70 ? 1900 : 2000;
                return $"{year:0000}-{month:00}-{day:00}";
            }

            private static string GsiTimeCodeToSmpte(string timeCode)
            {
                timeCode = timeCode?.Trim();
                if (timeCode == null || timeCode.Length != 8 || !long.TryParse(timeCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric) || numeric == 0)
                {
                    return null;
                }

                return $"{timeCode.Substring(0, 2)}:{timeCode.Substring(2, 2)}:{timeCode.Substring(4, 2)}:{timeCode.Substring(6, 2)}";
            }
        }

        /// <summary>
        /// The DVB teletext page and language a document carries when it came from a .dvbttx
        /// dump - stored by <see cref="ToText"/> so the trip back to DVB teletext keeps them.
        /// </summary>
        public static bool TryGetTeletextPageAndLanguage(string header, out int pageNumber, out string languageCode)
        {
            return TryGetTeletextPageAndLanguage(header, out pageNumber, out languageCode, out _);
        }

        /// <summary>
        /// Same as <see cref="TryGetTeletextPageAndLanguage(string, out int, out string)"/>, also
        /// reporting whether the page was announced as subtitles for the hearing impaired.
        /// </summary>
        public static bool TryGetTeletextPageAndLanguage(string header, out int pageNumber, out string languageCode, out bool hearingImpaired)
        {
            pageNumber = 0;
            languageCode = string.Empty;
            hearingImpaired = false;
            if (!IsEbuTtHeader(header))
            {
                return false;
            }

            var metadata = DocumentMetadata.FromEbuTtDocument(header);
            if (!metadata.TeletextPage.HasValue)
            {
                return false;
            }

            pageNumber = metadata.TeletextPage.Value;
            languageCode = metadata.TeletextLanguage ?? string.Empty;
            hearingImpaired = metadata.TeletextHearingImpaired;
            return true;
        }

        /// <summary>
        /// Fills the GSI block fields of an invented STL header from the document metadata of an
        /// EBU-TT source, so titles, translator, publisher etc. survive the trip back to STL.
        /// The GSI block is fixed width - every value is padded/truncated to its field.
        /// </summary>
        public static void ApplyDocumentMetadata(Ebu.EbuGeneralSubtitleInformation stlHeader, string ebuTtHeader)
        {
            if (stlHeader == null || !IsEbuTtHeader(ebuTtHeader))
            {
                return;
            }

            var metadata = DocumentMetadata.FromEbuTtDocument(ebuTtHeader);

            string Fixed(string value, string current, int width)
            {
                return value == null ? current : value.PadRight(width).Substring(0, width);
            }

            stlHeader.OriginalProgrammeTitle = Fixed(metadata.OriginalProgrammeTitle, stlHeader.OriginalProgrammeTitle, 32);
            stlHeader.OriginalEpisodeTitle = Fixed(metadata.OriginalEpisodeTitle, stlHeader.OriginalEpisodeTitle, 32);
            stlHeader.TranslatedProgrammeTitle = Fixed(metadata.TranslatedProgrammeTitle, stlHeader.TranslatedProgrammeTitle, 32);
            stlHeader.TranslatedEpisodeTitle = Fixed(metadata.TranslatedEpisodeTitle, stlHeader.TranslatedEpisodeTitle, 32);
            stlHeader.TranslatorsName = Fixed(metadata.TranslatorsName, stlHeader.TranslatorsName, 32);
            stlHeader.TranslatorsContactDetails = Fixed(metadata.TranslatorsContactDetails, stlHeader.TranslatorsContactDetails, 32);
            stlHeader.SubtitleListReferenceCode = Fixed(metadata.SubtitleListReferenceCode, stlHeader.SubtitleListReferenceCode, 16);
            stlHeader.CountryOfOrigin = Fixed(metadata.CountryOfOrigin, stlHeader.CountryOfOrigin, 3);
            stlHeader.Publisher = Fixed(metadata.Publisher, stlHeader.Publisher, 32);
            stlHeader.EditorsName = Fixed(metadata.EditorsName, stlHeader.EditorsName, 32);
            stlHeader.EditorsContactDetails = Fixed(metadata.EditorsContactDetails, stlHeader.EditorsContactDetails, 32);

            if (int.TryParse(metadata.RevisionNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var revision) &&
                revision >= 0 && revision <= 99)
            {
                stlHeader.RevisionNumber = revision.ToString("00", CultureInfo.InvariantCulture);
            }

            var startOfProgramme = metadata.StartOfProgramme?.Replace(":", string.Empty);
            if (startOfProgramme != null && startOfProgramme.Length == 8 &&
                long.TryParse(startOfProgramme, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                stlHeader.TimeCodeStartOfProgramme = startOfProgramme;
            }
        }

        /// <summary>
        /// True when the subtitle came from a format whose MarginV is a teletext row and whose
        /// box/double height flags were seeded into the settings: an EBU STL saved for teletext,
        /// a DVB teletext dump, or an EBU-TT document.
        /// </summary>
        private static bool IsTeletextRowSource(string header)
        {
            if (Ebu.IsStlHeader(header))
            {
                return header[11] == '1' || header[11] == '2'; // GSI display standard code - teletext only
            }

            return DvbTeletext.IsDvbTeletextHeader(header) || IsEbuTtHeader(header);
        }

        private sealed class WriteContext
        {
            public WriteContext(XmlDocument xml, XmlNode styling, XmlNode layout, bool teletextSource)
            {
                Xml = xml;
                Styling = styling;
                Layout = layout;
                TeletextSource = teletextSource;
            }

            public XmlDocument Xml { get; }
            public XmlNode Styling { get; }
            public XmlNode Layout { get; }
            public bool TeletextSource { get; }
            public HashSet<string> StyleIds { get; } = new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> RegionIds { get; } = new HashSet<string>(StringComparer.Ordinal) { "bottom" };
        }

        private XmlNode MakeParagraph(WriteContext context, Paragraph p, int count, bool boxAll)
        {
            var xml = context.Xml;
            XmlNode paragraph = xml.CreateElement("p", TtmlNamespace);

            var idAttribute = xml.CreateAttribute("xml:id");
            idAttribute.InnerText = $"sub{count}";
            paragraph.Attributes.Append(idAttribute);

            var start = xml.CreateAttribute("begin");
            start.InnerText = ToTimeCode(p.StartTime);
            paragraph.Attributes.Append(start);

            var end = xml.CreateAttribute("end");
            end.InnerText = ToTimeCode(p.EndTime);
            paragraph.Attributes.Append(end);

            var raw = (p.Text ?? string.Empty).RemoveControlCharactersButWhiteSpace();
            var alignment = GetAlignment(raw);
            var text = Utilities.RemoveSsaTags(raw);

            var region = xml.CreateAttribute("region");
            region.InnerText = GetRegion(context, p, alignment);
            paragraph.Attributes.Append(region);

            if (alignment.Horizontal != 0)
            {
                var styleAttribute = xml.CreateAttribute("style");
                styleAttribute.InnerText = alignment.Horizontal < 0
                    ? EnsureStyle(context, "textLeft", "textAlign", "left")
                    : EnsureStyle(context, "textRight", "textAlign", "right");
                paragraph.Attributes.Append(styleAttribute);
            }

            var first = true;
            var state = new TagState();
            foreach (var line in text.SplitToLines())
            {
                if (!first)
                {
                    paragraph.AppendChild(xml.CreateElement("br", TtmlNamespace));
                }

                foreach (var segment in SplitToSegments(line, state))
                {
                    var span = xml.CreateElement("span", TtmlNamespace);
                    var styles = new List<string>();
                    if (segment.Color != null)
                    {
                        styles.Add(EnsureColorStyle(context, segment.Color));
                    }

                    if (segment.Box || boxAll)
                    {
                        styles.Add(EnsureStyle(context, "boxStyle", "backgroundColor", "#000000"));
                    }

                    if (segment.Italic)
                    {
                        styles.Add(EnsureStyle(context, "italicStyle", "fontStyle", "italic"));
                    }

                    if (segment.Bold)
                    {
                        styles.Add(EnsureStyle(context, "boldStyle", "fontWeight", "bold"));
                    }

                    if (segment.Underline)
                    {
                        styles.Add(EnsureStyle(context, "underlineStyle", "textDecoration", "underline"));
                    }

                    if (styles.Count > 0)
                    {
                        var styleAttribute = xml.CreateAttribute("style");
                        styleAttribute.InnerText = string.Join(" ", styles);
                        span.Attributes.Append(styleAttribute);
                    }

                    span.AppendChild(xml.CreateTextNode(segment.Text));
                    paragraph.AppendChild(span);
                }

                first = false;
            }

            return paragraph;
        }

        private readonly struct Alignment
        {
            public Alignment(int horizontal, int vertical)
            {
                Horizontal = horizontal;
                Vertical = vertical;
            }

            public int Horizontal { get; } // -1 left, 0 center, 1 right
            public int Vertical { get; }   // -1 top, 0 middle, 1 bottom
        }

        private static Alignment GetAlignment(string text)
        {
            if (text.StartsWith("{\\an", StringComparison.Ordinal) && text.Length > 5 && text[5] == '}')
            {
                switch (text[4])
                {
                    case '7': return new Alignment(-1, -1);
                    case '8': return new Alignment(0, -1);
                    case '9': return new Alignment(1, -1);
                    case '4': return new Alignment(-1, 0);
                    case '5': return new Alignment(0, 0);
                    case '6': return new Alignment(1, 0);
                    case '1': return new Alignment(-1, 1);
                    case '3': return new Alignment(1, 1);
                }
            }

            return new Alignment(0, 1);
        }

        private static string GetRegion(WriteContext context, Paragraph p, Alignment alignment)
        {
            // MarginV only holds a teletext row when the subtitle came from a teletext format -
            // every other source writes its own meaning into it (an ASSA pixel margin, a PAC
            // percentage), which must not become a row.
            if (context.TeletextSource &&
                int.TryParse(p.MarginV, NumberStyles.Integer, CultureInfo.InvariantCulture, out var row) &&
                row >= 1 && row < CellRows)
            {
                var id = $"rowRegion{row}";
                if (context.RegionIds.Add(id))
                {
                    var xml = context.Xml;
                    var region = xml.CreateElement("region", TtmlNamespace);
                    var idAttribute = xml.CreateAttribute("xml:id");
                    idAttribute.InnerText = id;
                    region.Attributes.Append(idAttribute);
                    var y = row * 100.0 / CellRows;
                    SetTtsAttribute(xml, region, "origin", $"0% {y.ToString("0.###", CultureInfo.InvariantCulture)}%");
                    SetTtsAttribute(xml, region, "extent", $"100% {(100.0 - y).ToString("0.###", CultureInfo.InvariantCulture)}%");
                    SetTtsAttribute(xml, region, "displayAlign", "before");
                    SetTtsAttribute(xml, region, "overflow", "visible");
                    context.Layout.AppendChild(region);
                }

                return id;
            }

            if (alignment.Vertical < 0)
            {
                return EnsureZoneRegion(context, "top", "before");
            }

            if (alignment.Vertical == 0)
            {
                return EnsureZoneRegion(context, "middle", "center");
            }

            return "bottom";
        }

        private static string EnsureZoneRegion(WriteContext context, string id, string displayAlign)
        {
            if (context.RegionIds.Add(id))
            {
                var xml = context.Xml;
                var region = xml.CreateElement("region", TtmlNamespace);
                var idAttribute = xml.CreateAttribute("xml:id");
                idAttribute.InnerText = id;
                region.Attributes.Append(idAttribute);
                SetTtsAttribute(xml, region, "origin", "10% 10%");
                SetTtsAttribute(xml, region, "extent", "80% 80%");
                SetTtsAttribute(xml, region, "displayAlign", displayAlign);
                SetTtsAttribute(xml, region, "overflow", "visible");
                context.Layout.AppendChild(region);
            }

            return id;
        }

        private static string EnsureStyle(WriteContext context, string id, string ttsAttribute, string value)
        {
            if (context.StyleIds.Add(id))
            {
                var xml = context.Xml;
                var style = xml.CreateElement("style", TtmlNamespace);
                var idAttribute = xml.CreateAttribute("xml:id");
                idAttribute.InnerText = id;
                style.Attributes.Append(idAttribute);
                SetTtsAttribute(xml, style, ttsAttribute, value);
                context.Styling.AppendChild(style);
            }

            return id;
        }

        private static string EnsureColorStyle(WriteContext context, string color)
        {
            var hex = GetHexColor(color);
            var name = GetTeletextColorName(hex);
            var id = name != null ? "text" + name : "color" + hex.TrimStart('#');
            return EnsureStyle(context, id, "color", hex);
        }

        private static void SetTtsAttribute(XmlDocument xml, XmlNode node, string name, string value)
        {
            var attribute = xml.CreateAttribute("tts", name, TtmlStylingNamespace);
            attribute.InnerText = value;
            node.Attributes.Append(attribute);
        }

        // The eight teletext colours - the RGB cube corners, the same values the STL reader's
        // colour names stand for.
        private static readonly (string Name, string Hex)[] TeletextColors =
        {
            ("Black", "#000000"),
            ("Red", "#ff0000"),
            ("Green", "#00ff00"),
            ("Yellow", "#ffff00"),
            ("Blue", "#0000ff"),
            ("Magenta", "#ff00ff"),
            ("Cyan", "#00ffff"),
            ("White", "#ffffff"),
        };

        private static string GetTeletextColorName(string hex)
        {
            foreach (var color in TeletextColors)
            {
                if (string.Equals(color.Hex, hex, StringComparison.OrdinalIgnoreCase))
                {
                    return color.Name;
                }
            }

            return null;
        }

        private static string GetHexColor(string color)
        {
            foreach (var teletextColor in TeletextColors)
            {
                if (string.Equals(teletextColor.Name, color, StringComparison.OrdinalIgnoreCase))
                {
                    return teletextColor.Hex;
                }
            }

            var skColor = HtmlUtil.GetColorFromString(color);
            return $"#{skColor.Red:x2}{skColor.Green:x2}{skColor.Blue:x2}";
        }

        private readonly struct Segment
        {
            public Segment(string text, string color, bool box, bool italic, bool bold, bool underline)
            {
                Text = text;
                Color = color;
                Box = box;
                Italic = italic;
                Bold = bold;
                Underline = underline;
            }

            public string Text { get; }
            public string Color { get; }
            public bool Box { get; }
            public bool Italic { get; }
            public bool Bold { get; }
            public bool Underline { get; }
        }

        private sealed class TagState
        {
            public readonly Stack<string> Colors = new Stack<string>();
            public int Box;
            public int Italic;
            public int Bold;
            public int Underline;
        }

        /// <summary>
        /// Splits one line into segments of equal styling. Tag state carries across lines, so a
        /// tag opened on line one still applies to line two. Unknown tags are dropped, keeping
        /// their inner text.
        /// </summary>
        private static List<Segment> SplitToSegments(string line, TagState state)
        {
            var segments = new List<Segment>();
            var sb = new StringBuilder();
            var i = 0;

            void Flush()
            {
                if (sb.Length > 0)
                {
                    segments.Add(new Segment(sb.ToString(),
                        state.Colors.Count > 0 ? state.Colors.Peek() : null,
                        state.Box > 0, state.Italic > 0, state.Bold > 0, state.Underline > 0));
                    sb.Clear();
                }
            }

            while (i < line.Length)
            {
                if (line[i] == '<')
                {
                    var endTag = line.IndexOf('>', i);
                    if (endTag > i)
                    {
                        var tag = line.Substring(i + 1, endTag - i - 1).Trim();
                        var lower = tag.ToLowerInvariant();
                        if (lower == "i" || lower == "/i" || lower == "b" || lower == "/b" ||
                            lower == "u" || lower == "/u" || lower == "box" || lower == "/box" || lower == "/font")
                        {
                            Flush();
                            switch (lower)
                            {
                                case "i": state.Italic++; break;
                                case "/i": state.Italic = Math.Max(0, state.Italic - 1); break;
                                case "b": state.Bold++; break;
                                case "/b": state.Bold = Math.Max(0, state.Bold - 1); break;
                                case "u": state.Underline++; break;
                                case "/u": state.Underline = Math.Max(0, state.Underline - 1); break;
                                case "box": state.Box++; break;
                                case "/box": state.Box = Math.Max(0, state.Box - 1); break;
                                case "/font":
                                    if (state.Colors.Count > 0)
                                    {
                                        state.Colors.Pop();
                                    }

                                    break;
                            }
                        }
                        else if (lower.StartsWith("font", StringComparison.Ordinal))
                        {
                            Flush();
                            // A font tag with no color attribute (or e.g. only a face) still nests
                            // with a closing tag - push whatever colour is current so "/font" pops
                            // symmetrically.
                            var color = GetFontColor(tag);
                            state.Colors.Push(color ?? (state.Colors.Count > 0 ? state.Colors.Peek() : null));
                        }
                        else
                        {
                            sb.Append(line, i, endTag - i + 1); // not a tag we know - keep as text
                        }

                        i = endTag + 1;
                        continue;
                    }
                }

                sb.Append(line[i]);
                i++;
            }

            Flush();
            if (segments.Count == 0)
            {
                segments.Add(new Segment(string.Empty,
                    state.Colors.Count > 0 ? state.Colors.Peek() : null,
                    state.Box > 0, state.Italic > 0, state.Bold > 0, state.Underline > 0));
            }

            return segments;
        }

        /// <summary>
        /// The color attribute value of a font tag - double-quoted, single-quoted or bare
        /// (all three occur in the wild, see the unquoted-value crash of PR #14130).
        /// </summary>
        private static string GetFontColor(string tagContent)
        {
            var index = tagContent.IndexOf("color", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return null;
            }

            index = tagContent.IndexOf('=', index);
            if (index < 0)
            {
                return null;
            }

            var value = tagContent.Substring(index + 1).TrimStart();
            if (value.Length == 0)
            {
                return null;
            }

            if (value[0] == '"' || value[0] == '\'')
            {
                var quote = value[0];
                var endQuote = value.IndexOf(quote, 1);
                value = endQuote > 0 ? value.Substring(1, endQuote - 1) : value.Substring(1);
            }
            else
            {
                var endIndex = value.IndexOf(' ');
                if (endIndex > 0)
                {
                    value = value.Substring(0, endIndex);
                }
            }

            return value.Length == 0 ? null : value;
        }

        private static string ToTimeCode(TimeCode time)
        {
            var ts = time.TimeSpan;
            return $"{ts.Days * 24 + ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private sealed class StyleInfo
        {
            public string Color;
            public bool Box;
            public bool Italic;
            public bool Bold;
            public bool Underline;
            public bool DoubleHeight;
            public string TextAlign;
        }

        private sealed class RegionInfo
        {
            public int Row = -1;
            public bool RowReachesBottom;
            public string DisplayAlign;
            public string TextAlign;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var xml = new XmlDocument { XmlResolver = null, PreserveWhitespace = true };
            try
            {
                xml.LoadXml(JoinLines(lines).RemoveControlCharactersButWhiteSpace().Trim());
            }
            catch
            {
                try
                {
                    xml.LoadXml(JoinLines(lines).Replace(" & ", " &amp; ").RemoveControlCharactersButWhiteSpace().Trim());
                }
                catch (Exception exception)
                {
                    // A truncated or damaged file must read as "not mine", not throw out of
                    // IsMine, which runs for every format when a file is opened.
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    _errorCount = 1;
                    return;
                }
            }

            subtitle.Header = JoinLines(lines);

            var namespaceManager = new XmlNamespaceManager(xml.NameTable);
            namespaceManager.AddNamespace("ttml", TtmlNamespace);

            // ttp:frameRate matters when the document uses the smpte timebase - the frame part of
            // its timecodes is only correct at the declared rate.
            var frameRateText = GetLocalAttribute(xml.DocumentElement, "frameRate");
            if (int.TryParse(frameRateText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frameRate) &&
                frameRate >= 15 && frameRate <= 120)
            {
                double rate = frameRate;
                var multiplier = GetLocalAttribute(xml.DocumentElement, "frameRateMultiplier");
                if (!string.IsNullOrEmpty(multiplier))
                {
                    var parts = multiplier.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var numerator) &&
                        int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var denominator) &&
                        numerator > 0 && denominator > 0)
                    {
                        rate = rate * numerator / denominator;
                    }
                }

                Configuration.Settings.General.CurrentFrameRate = rate;
            }

            var cellRows = CellRows;
            var cellResolution = GetLocalAttribute(xml.DocumentElement, "cellResolution");
            if (!string.IsNullOrEmpty(cellResolution))
            {
                var parts = cellResolution.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedRows) &&
                    parsedRows > 1)
                {
                    cellRows = parsedRows;
                }
            }

            var styles = new Dictionary<string, StyleInfo>(StringComparer.Ordinal);
            foreach (XmlNode styleNode in xml.DocumentElement.SelectNodes("//ttml:style", namespaceManager))
            {
                var id = styleNode.Attributes?["xml:id"]?.Value ?? styleNode.Attributes?["id"]?.Value;
                if (string.IsNullOrEmpty(id) || styles.ContainsKey(id))
                {
                    continue;
                }

                var info = new StyleInfo
                {
                    Italic = GetTtsAttribute(styleNode, "fontStyle") == "italic",
                    Bold = GetTtsAttribute(styleNode, "fontWeight") == "bold",
                    Underline = GetTtsAttribute(styleNode, "textDecoration") == "underline",
                    TextAlign = GetTtsAttribute(styleNode, "textAlign"),
                };

                var color = GetTtsAttribute(styleNode, "color");
                if (!string.IsNullOrEmpty(color))
                {
                    info.Color = NormalizeColor(color);
                }

                var backgroundColor = GetTtsAttribute(styleNode, "backgroundColor");
                if (!string.IsNullOrEmpty(backgroundColor) &&
                    !backgroundColor.Equals("transparent", StringComparison.OrdinalIgnoreCase) &&
                    !backgroundColor.Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    info.Box = true;
                }

                var fontSize = GetTtsAttribute(styleNode, "fontSize");
                if (fontSize == "1c 2c" || fontSize == "100% 200%")
                {
                    info.DoubleHeight = true;
                }

                styles[id] = info;
            }

            var regions = new Dictionary<string, RegionInfo>(StringComparer.Ordinal);
            foreach (XmlNode regionNode in xml.DocumentElement.SelectNodes("//ttml:region", namespaceManager))
            {
                var id = regionNode.Attributes?["xml:id"]?.Value ?? regionNode.Attributes?["id"]?.Value;
                if (string.IsNullOrEmpty(id) || regions.ContainsKey(id))
                {
                    continue;
                }

                var info = new RegionInfo
                {
                    DisplayAlign = GetTtsAttribute(regionNode, "displayAlign"),
                    TextAlign = GetTtsAttribute(regionNode, "textAlign"),
                };

                // With displayAlign "before" the text starts at the region origin, so the origin
                // row is the teletext row the subtitle starts on.
                if (info.DisplayAlign == "before" &&
                    TryGetPercent(GetTtsAttribute(regionNode, "origin"), cellRows, out _, out var top))
                {
                    var row = (int)Math.Round(top * cellRows / 100.0, MidpointRounding.AwayFromZero);
                    if (row >= 1 && row < cellRows)
                    {
                        info.Row = row;
                        if (TryGetPercent(GetTtsAttribute(regionNode, "extent"), cellRows, out _, out var height))
                        {
                            info.RowReachesBottom = top + height >= 97.0;
                        }
                    }
                }

                regions[id] = info;
            }

            var body = xml.DocumentElement.SelectSingleNode("ttml:body", namespaceManager);
            if (body == null)
            {
                _errorCount++;
                return;
            }

            var defaultTextAlign = GetStyleTextAlign(body.Attributes?["style"]?.Value, styles) ?? "center";

            var anyStrictRowRegion = false;
            var doubleHeightUsed = false;
            var anyBoxSegment = false;
            var allBoxed = true;
            var paragraphData = new List<(XmlNode Node, List<object> Parts, RegionInfo Region, string RegionId, string TextAlign)>();

            foreach (XmlNode node in body.SelectNodes("//ttml:p", namespaceManager))
            {
                var regionId = node.Attributes?["region"]?.Value;
                RegionInfo region = null;
                if (regionId != null)
                {
                    regions.TryGetValue(regionId, out region);
                }

                var textAlign = GetStyleTextAlign(node.Attributes?["style"]?.Value, styles) ??
                                region?.TextAlign ?? defaultTextAlign;

                var parts = new List<object>();
                ReadNode(node, parts, styles, new StyleInfo());
                foreach (var part in parts)
                {
                    if (part is Segment segment && segment.Text.Trim().Length > 0)
                    {
                        if (segment.Box)
                        {
                            anyBoxSegment = true;
                        }
                        else
                        {
                            allBoxed = false;
                        }
                    }
                }

                if (region != null && region.Row >= 1 && region.RowReachesBottom)
                {
                    anyStrictRowRegion = true;
                }

                paragraphData.Add((node, parts, region, regionId, textAlign));
            }

            foreach (var style in styles.Values)
            {
                if (style.DoubleHeight)
                {
                    doubleHeightUsed = true;
                }
            }

            // The boxed look and double height are file properties, mirrored into the same
            // settings an STL load seeds - but only when the document is teletext mapped at all,
            // so a plain distribution style document does not flip the STL save options.
            var teletextMapped = anyStrictRowRegion || doubleHeightUsed;
            if (teletextMapped)
            {
                Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = anyBoxSegment && allBoxed;
                Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = doubleHeightUsed;
            }

            // When every span is boxed the box is the global teletext look, carried by the seeded
            // setting - tags would double it. Mixed boxing keeps per-span tags.
            var skipBoxTags = teletextMapped && anyBoxSegment && allBoxed;

            foreach (var (node, parts, region, regionId, textAlign) in paragraphData)
            {
                TimedText10.ExtractTimeCodes(node, subtitle, out var begin, out var end);
                var text = BuildText(parts, skipBoxTags);

                var row = region?.Row ?? -1;
                var alignmentTag = GetAlignmentTag(row, cellRows, region?.DisplayAlign, textAlign);
                if (alignmentTag != null)
                {
                    text = alignmentTag + text;
                }

                var paragraph = new Paragraph(begin, end, text);
                if (row >= 1)
                {
                    paragraph.MarginV = row.ToString(CultureInfo.InvariantCulture);
                }

                if (regionId != null)
                {
                    paragraph.Region = regionId;
                }

                subtitle.Paragraphs.Add(paragraph);
            }

            subtitle.Renumber();
        }

        private static string GetStyleTextAlign(string styleRefs, Dictionary<string, StyleInfo> styles)
        {
            if (string.IsNullOrEmpty(styleRefs))
            {
                return null;
            }

            string result = null;
            foreach (var styleRef in styleRefs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (styles.TryGetValue(styleRef, out var style) && !string.IsNullOrEmpty(style.TextAlign))
                {
                    result = style.TextAlign;
                }
            }

            return result;
        }

        /// <summary>
        /// The {\anX} prefix matching what the STL reader would produce for the same row and
        /// justification - null for the default bottom/center.
        /// </summary>
        private static string GetAlignmentTag(int row, int cellRows, string displayAlign, string textAlign)
        {
            var left = textAlign == "left" || textAlign == "start";
            var right = textAlign == "right" || textAlign == "end";

            int verticalZone; // -1 top, 0 middle, 1 bottom
            if (row >= 1)
            {
                var visibleRows = cellRows - 1;
                if (row < 3)
                {
                    verticalZone = -1;
                }
                else if (row <= visibleRows / 2 + 1)
                {
                    verticalZone = 0;
                }
                else
                {
                    verticalZone = 1;
                }
            }
            else if (displayAlign == "before")
            {
                verticalZone = -1;
            }
            else if (displayAlign == "center")
            {
                verticalZone = 0;
            }
            else
            {
                verticalZone = 1;
            }

            switch (verticalZone)
            {
                case -1:
                    return left ? "{\\an7}" : right ? "{\\an9}" : "{\\an8}";
                case 0:
                    return left ? "{\\an4}" : right ? "{\\an6}" : "{\\an5}";
                default:
                    return left ? "{\\an1}" : right ? "{\\an3}" : null;
            }
        }

        private static string NormalizeColor(string color)
        {
            var skColor = HtmlUtil.GetColorFromString(color);
            var hex = $"#{skColor.Red:x2}{skColor.Green:x2}{skColor.Blue:x2}";
            return GetTeletextColorName(hex) ?? hex;
        }

        private sealed class LineBreak
        {
        }

        private static void ReadNode(XmlNode node, List<object> parts, Dictionary<string, StyleInfo> styles, StyleInfo state)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.SignificantWhitespace)
                {
                    var value = child.NodeType == XmlNodeType.SignificantWhitespace ? " " : child.Value;

                    // A pretty printed document may wrap span content across source lines - only
                    // tt:br is a line break, so source formatting whitespace collapses to one space.
                    if (value.IndexOfAny(new[] { '\r', '\n', '\t' }) >= 0)
                    {
                        value = value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
                        while (value.Contains("  ", StringComparison.Ordinal))
                        {
                            value = value.Replace("  ", " ");
                        }
                    }

                    parts.Add(new Segment(value, state.Color, state.Box, state.Italic, state.Bold, state.Underline));
                }
                else if (child.LocalName == "br")
                {
                    parts.Add(new LineBreak());
                }
                else if (child.LocalName == "span")
                {
                    var childState = new StyleInfo
                    {
                        Color = state.Color,
                        Box = state.Box,
                        Italic = state.Italic,
                        Bold = state.Bold,
                        Underline = state.Underline,
                    };

                    var styleRefs = child.Attributes?["style"]?.Value;
                    if (!string.IsNullOrEmpty(styleRefs))
                    {
                        foreach (var styleRef in styleRefs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!styles.TryGetValue(styleRef, out var style))
                            {
                                continue;
                            }

                            childState.Color = style.Color ?? childState.Color;
                            childState.Box = childState.Box || style.Box;
                            childState.Italic = childState.Italic || style.Italic;
                            childState.Bold = childState.Bold || style.Bold;
                            childState.Underline = childState.Underline || style.Underline;
                        }
                    }

                    // Inline styling is not valid EBU-TT, but tolerate it on read - files in the
                    // wild are not always conformant.
                    var inlineColor = GetTtsAttribute(child, "color");
                    if (!string.IsNullOrEmpty(inlineColor))
                    {
                        childState.Color = NormalizeColor(inlineColor);
                    }

                    if (GetTtsAttribute(child, "fontStyle") == "italic")
                    {
                        childState.Italic = true;
                    }

                    if (GetTtsAttribute(child, "fontWeight") == "bold")
                    {
                        childState.Bold = true;
                    }

                    ReadNode(child, parts, styles, childState);
                }
                else
                {
                    ReadNode(child, parts, styles, state);
                }
            }
        }

        private static string BuildText(List<object> parts, bool skipBoxTags)
        {
            var sb = new StringBuilder();
            var lineSegments = new List<Segment>();

            void FlushLine()
            {
                // Trim the line, then emit tags per run of equal styling - box outermost, then
                // font, then i/b/u, the same nesting the STL reader produces.
                while (lineSegments.Count > 0 && lineSegments[0].Text.Trim().Length == 0)
                {
                    lineSegments.RemoveAt(0);
                }

                while (lineSegments.Count > 0 && lineSegments[lineSegments.Count - 1].Text.Trim().Length == 0)
                {
                    lineSegments.RemoveAt(lineSegments.Count - 1);
                }

                for (var i = 0; i < lineSegments.Count; i++)
                {
                    var segment = lineSegments[i];
                    var text = segment.Text;
                    if (i == 0)
                    {
                        text = text.TrimStart();
                    }

                    if (i == lineSegments.Count - 1)
                    {
                        text = text.TrimEnd();
                    }

                    var box = segment.Box && !skipBoxTags;
                    if (box)
                    {
                        sb.Append("<box>");
                    }

                    if (segment.Color != null)
                    {
                        sb.Append("<font color=\"").Append(segment.Color).Append("\">");
                    }

                    if (segment.Italic)
                    {
                        sb.Append("<i>");
                    }

                    if (segment.Bold)
                    {
                        sb.Append("<b>");
                    }

                    if (segment.Underline)
                    {
                        sb.Append("<u>");
                    }

                    sb.Append(text);

                    if (segment.Underline)
                    {
                        sb.Append("</u>");
                    }

                    if (segment.Bold)
                    {
                        sb.Append("</b>");
                    }

                    if (segment.Italic)
                    {
                        sb.Append("</i>");
                    }

                    if (segment.Color != null)
                    {
                        sb.Append("</font>");
                    }

                    if (box)
                    {
                        sb.Append("</box>");
                    }
                }

                lineSegments.Clear();
            }

            foreach (var part in parts)
            {
                if (part is LineBreak)
                {
                    FlushLine();
                    sb.AppendLine();
                }
                else if (part is Segment segment)
                {
                    // Merge with the previous segment when the styling is identical - separate
                    // text nodes of one span must not produce repeated tags.
                    if (lineSegments.Count > 0)
                    {
                        var previous = lineSegments[lineSegments.Count - 1];
                        if (previous.Color == segment.Color && previous.Box == segment.Box &&
                            previous.Italic == segment.Italic && previous.Bold == segment.Bold &&
                            previous.Underline == segment.Underline)
                        {
                            lineSegments[lineSegments.Count - 1] = new Segment(previous.Text + segment.Text,
                                previous.Color, previous.Box, previous.Italic, previous.Bold, previous.Underline);
                            continue;
                        }
                    }

                    lineSegments.Add(segment);
                }
            }

            FlushLine();

            var text = sb.ToString()
                .Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine)
                .Replace("</b>" + Environment.NewLine + "<b>", Environment.NewLine)
                .Replace("</u>" + Environment.NewLine + "<u>", Environment.NewLine)
                .Replace("</box>" + Environment.NewLine + "<box>", Environment.NewLine)
                .Trim();

            // All white text is the default, not a colour choice - same rule as the STL reader.
            if (!text.Replace("<font color=\"White\">", string.Empty).Contains("<font ", StringComparison.Ordinal))
            {
                text = text.Replace("<font color=\"White\">", string.Empty).Replace("</font>", string.Empty);
            }

            return text;
        }

        /// <summary>
        /// An attribute matched by local name so both "tts:origin" and any other prefix bound to
        /// the styling namespace resolve.
        /// </summary>
        private static string GetTtsAttribute(XmlNode node, string name)
        {
            if (node.Attributes == null)
            {
                return null;
            }

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.LocalName == name &&
                    (attribute.NamespaceURI == TtmlStylingNamespace || string.IsNullOrEmpty(attribute.NamespaceURI)))
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        private static string GetLocalAttribute(XmlNode node, string name)
        {
            if (node?.Attributes == null)
            {
                return null;
            }

            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (attribute.LocalName == name)
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses a "x y" pair into percentages - "0% 83.333%" directly, "0c 20c" via the cell
        /// grid.
        /// </summary>
        private static bool TryGetPercent(string pair, int cellRows, out double x, out double y)
        {
            x = 0;
            y = 0;
            if (string.IsNullOrEmpty(pair))
            {
                return false;
            }

            var parts = pair.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }

            return TryGetPercentValue(parts[0], 40, out x) && TryGetPercentValue(parts[1], cellRows, out y);
        }

        private static bool TryGetPercentValue(string value, int cells, out double result)
        {
            result = 0;
            if (value.EndsWith("%", StringComparison.Ordinal))
            {
                return double.TryParse(value.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }

            if (value.EndsWith("c", StringComparison.Ordinal))
            {
                if (double.TryParse(value.TrimEnd('c'), NumberStyles.Float, CultureInfo.InvariantCulture, out var cellValue))
                {
                    result = cellValue * 100.0 / cells;
                    return true;
                }
            }

            return false;
        }
    }
}
