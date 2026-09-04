using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// DVB teletext subtitles in a Manzanita "private_stream_1" dump (.dvbttx) - a full teletext
    /// format: the eight Level 1 colours as spacing attributes, the rest of the colour map via
    /// packets X/28/0 and X/26 (Level 2.5), boxed double height rows, and teletext row
    /// positioning. Reading goes through <see cref="ManzanitaTransportStreamParser"/>, writing
    /// through <see cref="ManzanitaTeletextWriter"/>.
    /// </summary>
    public class DvbTeletext : SubtitleFormat, IBinaryPersistableSubtitle
    {
        public const string NameOfFormat = "DVB Teletext";

        public override string Extension => ".dvbttx";

        public override string Name => NameOfFormat;

        public override bool IsTextBased => false;

        // The alignment picker stores the teletext row a line starts on in MarginV (1-23), and
        // the writer places the line there - so the video preview should too.
        public override bool HasPositionSupport => true;

        /// <summary>
        /// The teletext page the subtitles ride on - kept from the loaded file (or the save
        /// options dialog) via the subtitle header, see <see cref="CreateHeader(int, string, bool)"/>.
        /// </summary>
        public int PageNumber { get; set; } = 888;

        /// <summary>
        /// Three letter ISO 639-2 language code for the teletext descriptor.
        /// </summary>
        public string LanguageCode { get; set; } = "eng";

        /// <summary>
        /// Announce the page as subtitles for the hearing impaired (DVB teletext_type 0x05)
        /// rather than plain subtitles (0x02) in the file's teletext descriptor.
        /// </summary>
        public bool HearingImpaired { get; set; }

        /// <summary>
        /// Drops what only teletext can carry when converting away: the teletext row in MarginV,
        /// which any other format would read as a pixel margin or a percentage. EBU STL and
        /// EBU-TT place lines by the same rows, so converting to them keeps the positions.
        /// </summary>
        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            if (newFormat is Ebu || newFormat is EbuTt || newFormat is DvbTeletext)
            {
                return;
            }

            foreach (var p in subtitle.Paragraphs)
            {
                p.MarginV = null;
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            return !string.IsNullOrEmpty(fileName) && File.Exists(fileName) && FileUtil.IsManzanita(fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            var parser = new ManzanitaTransportStreamParser();
            parser.Parse(fileName);
            var pages = parser.GetTeletext();
            if (pages.Count == 0)
            {
                return;
            }

            // Without a page picker (batch mode) the page with the most subtitles is the best
            // guess for the subtitle page of a multi page carousel.
            var page = pages.OrderByDescending(p => p.Value.Count).First();
            subtitle.Paragraphs.AddRange(page.Value);
            subtitle.Renumber();
            var descriptor = parser.GetTeletextDescriptor(page.Key);
            subtitle.Header = CreateHeader(page.Key, descriptor?.LanguageCode ?? string.Empty, descriptor?.IsHearingImpaired ?? false);
        }

        public bool Save(string fileName, Stream stream, Subtitle subtitle, bool batchMode)
        {
            var pageNumber = PageNumber;
            var languageCode = LanguageCode;
            var hearingImpaired = HearingImpaired;
            if (TryParseHeader(subtitle.Header, out var headerPage, out var headerLanguage, out var headerHearingImpaired))
            {
                pageNumber = headerPage;
                languageCode = headerLanguage;
                hearingImpaired = headerHearingImpaired;
            }
            else if (EbuTt.TryGetTeletextPageAndLanguage(subtitle.Header, out var ebuTtPage, out var ebuTtLanguage, out var ebuTtHearingImpaired))
            {
                // An EBU-TT document written from a .dvbttx source carries the page, language and
                // subtitle type in its metadata - the trip through EBU-TT must not reset them.
                pageNumber = ebuTtPage;
                hearingImpaired = ebuTtHearingImpaired;
                if (!string.IsNullOrEmpty(ebuTtLanguage))
                {
                    languageCode = ebuTtLanguage;
                }
            }

            var writer = new ManzanitaTeletextWriter
            {
                PageNumber = pageNumber,
                LanguageCode = languageCode,
                HearingImpaired = hearingImpaired,
                FrameRate = Configuration.Settings.General.CurrentFrameRate,
            };

            var bytes = writer.GetBytes(subtitle);
            stream.Write(bytes, 0, bytes.Length);
            return true;
        }

        /// <summary>
        /// The header stored on a subtitle loaded from (or destined for) a .dvbttx file, carrying
        /// the save options the writer needs - the counterpart of the 1024 byte GSI block an EBU
        /// STL subtitle keeps on its header.
        /// </summary>
        public static string CreateHeader(int pageNumber, string languageCode)
        {
            return CreateHeader(pageNumber, languageCode, hearingImpaired: false);
        }

        /// <summary>
        /// Same as <see cref="CreateHeader(int, string)"/>; the subtitle type attribute is only
        /// written for the hearing impaired, so a plain subtitle header keeps its old shape.
        /// </summary>
        public static string CreateHeader(int pageNumber, string languageCode, bool hearingImpaired)
        {
            var language = (languageCode ?? string.Empty).Trim().ToLowerInvariant();
            if (language.Length != 3)
            {
                language = "eng";
            }

            return "<dvbteletext page=\"" + pageNumber.ToString(CultureInfo.InvariantCulture) +
                   "\" language=\"" + language + "\"" +
                   (hearingImpaired ? " type=\"" + HeaderTypeHearingImpaired + "\"" : string.Empty) +
                   " />";
        }

        /// <summary>
        /// Value of the header's "type" attribute for a hearing impaired page; absent (or any
        /// other value) means a plain subtitle page.
        /// </summary>
        public const string HeaderTypeHearingImpaired = "hearing-impaired";

        public static bool IsDvbTeletextHeader(string header)
        {
            return TryParseHeader(header, out _, out _);
        }

        public static bool TryParseHeader(string header, out int pageNumber, out string languageCode)
        {
            return TryParseHeader(header, out pageNumber, out languageCode, out _);
        }

        public static bool TryParseHeader(string header, out int pageNumber, out string languageCode, out bool hearingImpaired)
        {
            pageNumber = 0;
            languageCode = string.Empty;
            hearingImpaired = false;
            if (header == null || !header.StartsWith("<dvbteletext ", StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                var xml = new XmlDocument { XmlResolver = null };
                xml.LoadXml(header);
                var page = xml.DocumentElement?.Attributes?["page"]?.Value;
                var language = xml.DocumentElement?.Attributes?["language"]?.Value;
                if (page == null || language == null ||
                    !int.TryParse(page, NumberStyles.Integer, CultureInfo.InvariantCulture, out pageNumber) ||
                    pageNumber < 100 || pageNumber > 899)
                {
                    return false;
                }

                languageCode = language;
                hearingImpaired = string.Equals(xml.DocumentElement?.Attributes?["type"]?.Value, HeaderTypeHearingImpaired, StringComparison.OrdinalIgnoreCase);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
