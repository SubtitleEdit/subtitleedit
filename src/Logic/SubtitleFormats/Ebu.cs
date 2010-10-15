using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// EBU Subtitling data exchange format
    /// </summary>
    public class Ebu : SubtitleFormat
    {

        /// <summary>
        /// GSI block (1024 bytes)
        /// </summary>
        private class EbuGeneralSubtitleInformation
        {
            public string CodePageNumber { get; set; } // 0..2
            public string DiskFormatCode { get; set; } // 3..10
            public string DisplayStandardCode { get; set; } // 11
            public string CharacterCodeTableNumber { get; set; } // 12..13
            public string LanguageCode { get; set; } // 14..15
            public string OriginalProgrammeTitle { get; set; } // 16..47
            public string OriginalEpisodeTitle { get; set; }
            public string TranslatedProgrammeTitle { get; set; }
            public string TranslatedEpisodeTitle { get; set; }
            public string TranslatorsName { get; set; }
            public string TranslatorsContactDetails { get; set; }
            public string SubtitleListReferenceCode { get; set; }
            public string CreationDate { get; set; }
            public string RevisionDate { get; set; }
            public string RevisionNumber { get; set; }
            public string TotalNumberOfTextAndTimingInformationBlocks { get; set; }
            public string TotalNumberOfSubtitles { get; set; }
            public string TotalNumberOfSubtitleGroups { get; set; }
            public string MaximumNumberOfDisplayableCharactersInAnyTextRow { get; set; }
            public string MaximumNumberOfDisplayableRows { get; set; }
            public string TimeCodeStatus { get; set; }
            public string TimeCodeStartOfProgramme { get; set; }
            public string TimeCodeFirstInCue { get; set; }
            public string TotalNumberOfDisks { get; set; }
            public string DiskSequenceNumber { get; set; }
            public string CountryOfOrigin { get; set; }
            public string Publisher { get; set; }
            public string EditorsName { get; set; }
            public string EditorsContactDetails { get; set; }
            public string SpareBytes { get; set; }
            public string UserDefinedArea { get; set; }

            public double FrameRate
            {
                get
                {
                    if (DiskFormatCode.StartsWith("STL25"))
                        return 25.0;
                    else
                        return 30.0; // should be DiskFormatcode STL30.01
                }
            }
        }

        /// <summary>
        /// TTI block 128 bytes 
        /// </summary>
        private class EbuTextTimingInformation
        {
            public string SubtitleGroupNumber { get; set; }
            public string SubtitleNumber { get; set; }
            public string ExtensionBlockNumber { get; set; }
            public string CumulativeStatus { get; set; }
            public int TimeCodeInHours { get; set; }
            public int TimeCodeInMinutes { get; set; }
            public int TimeCodeInSeconds { get; set; }
            public int TimeCodeInMilliseconds { get; set; }
            public int TimeCodeOutHours { get; set; }
            public int TimeCodeOutMinutes { get; set; }
            public int TimeCodeOutSeconds { get; set; }
            public int TimeCodeOutMilliseconds { get; set; }
            public string VerticalPosition { get; set; }
            public string JustificationCode { get; set; }
            public byte CommentFlag { get; set; }
            public string TextField { get; set; }
        }

        public override string Extension
        {
            get { return ".stl"; }
        }

        public override string Name
        {
            get { return "EBU stl"; }
        }

        public override bool HasLineNumber
        {
            get { return true; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length > 1024 + 128 && fi.Length < 1024000) // not too small or too big
                {
                    byte[] buffer = File.ReadAllBytes(fileName);
                    EbuGeneralSubtitleInformation header = ReadHeader(buffer);
                    if (header.DiskFormatCode.StartsWith("STL25") ||
                        header.DiskFormatCode.StartsWith("STL30"))                    
                    {
                        return Utilities.IsInteger(header.CodePageNumber) &&
                               Utilities.IsInteger(header.TotalNumberOfSubtitles);
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            byte[] buffer = File.ReadAllBytes(fileName);
            EbuGeneralSubtitleInformation header = ReadHeader(buffer);
            foreach (EbuTextTimingInformation tti in ReadTTI(buffer, header))
            {
                Paragraph p = new Paragraph();
                p.Text = tti.TextField;
                p.StartTime = new TimeCode(tti.TimeCodeInHours, tti.TimeCodeInMinutes, tti.TimeCodeInSeconds, tti.TimeCodeInMilliseconds);
                p.EndTime = new TimeCode(tti.TimeCodeOutHours, tti.TimeCodeOutMinutes, tti.TimeCodeOutSeconds, tti.TimeCodeOutMilliseconds);
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
        }

        private EbuGeneralSubtitleInformation ReadHeader(byte[] buffer)
        {
            EbuGeneralSubtitleInformation header = new EbuGeneralSubtitleInformation();
            header.CodePageNumber = Encoding.ASCII.GetString(buffer, 0, 3);
            header.DiskFormatCode = Encoding.ASCII.GetString(buffer, 3, 8);
            header.DisplayStandardCode = Encoding.ASCII.GetString(buffer, 11, 1);
            header.CharacterCodeTableNumber = Encoding.ASCII.GetString(buffer, 12, 2);
            header.LanguageCode = Encoding.ASCII.GetString(buffer, 14, 2);
            header.OriginalProgrammeTitle = Encoding.ASCII.GetString(buffer, 16, 32);
            header.OriginalEpisodeTitle = Encoding.ASCII.GetString(buffer, 48, 32);
            header.TranslatedProgrammeTitle = Encoding.ASCII.GetString(buffer, 80, 32);
            header.TranslatedEpisodeTitle = Encoding.ASCII.GetString(buffer, 112, 32);
            header.TranslatorsName = Encoding.ASCII.GetString(buffer, 144, 32);
            header.TranslatorsContactDetails = Encoding.ASCII.GetString(buffer, 176, 32);
            header.SubtitleListReferenceCode = Encoding.ASCII.GetString(buffer, 208, 16);
            header.CreationDate = Encoding.ASCII.GetString(buffer, 224, 6);
            header.RevisionDate = Encoding.ASCII.GetString(buffer, 230, 6);
            header.RevisionNumber = Encoding.ASCII.GetString(buffer, 236, 2);
            header.TotalNumberOfTextAndTimingInformationBlocks = Encoding.ASCII.GetString(buffer, 238, 5);
            header.TotalNumberOfSubtitles = Encoding.ASCII.GetString(buffer, 243, 5);
            header.TotalNumberOfSubtitleGroups = Encoding.ASCII.GetString(buffer, 248, 3);
            header.MaximumNumberOfDisplayableCharactersInAnyTextRow = Encoding.ASCII.GetString(buffer, 251, 2);
            header.MaximumNumberOfDisplayableRows = Encoding.ASCII.GetString(buffer, 253, 2);
            header.TimeCodeStatus = Encoding.ASCII.GetString(buffer, 255, 1);
            header.TimeCodeStartOfProgramme = Encoding.ASCII.GetString(buffer, 256, 8);

            return header;
        }

        /// <summary>
        /// Get text with regard code page from header
        /// </summary>
        /// <param name="skipNext">Skip next character</param>
        /// <param name="header">EBU header</param>
        /// <param name="buffer">data buffer</param>
        /// <param name="index">index to current byte in buffer</param>
        /// <returns>Character at index</returns>
        private string GetCharacter(out bool skipNext, EbuGeneralSubtitleInformation header, byte[] buffer, int index)
        {
            skipNext = false;
            if (header.CharacterCodeTableNumber == "00")
            {
                //note that 0xC1—0xCF combines characters - http://en.wikipedia.org/wiki/ISO/IEC_6937
                Encoding encoding = Encoding.GetEncoding(20269);
                string next = encoding.GetString(buffer, index + 1, 1);
                switch (buffer[index])
                {
                    case 0xc1:
                        skipNext = "aeiou".Contains(next.ToLower());
                        switch (next)
                        {
                            case "a": return "à";
                            case "e": return "è";
                            case "i": return "ì";
                            case "o": return "ò";
                            case "u": return "ù";
                            case "A": return "À";
                            case "E": return "È";
                            case "I": return "Ì";
                            case "O": return "Ò";
                            case "U": return "Ù";
                        }
                        return string.Empty;
                    case 0xc2:
                        skipNext = "aeiou".Contains(next.ToLower());
                        switch (next)
                        {
                            case "a": return "á";
                            case "e": return "é";
                            case "i": return "í";
                            case "o": return "ó";
                            case "u": return "ú";
                            case "A": return "Á";
                            case "E": return "É";
                            case "I": return "Í";
                            case "O": return "Ó";
                            case "U": return "Ú";
                        }
                        return string.Empty;
                    case 0xc3:
                        skipNext = "aeiou".Contains(next.ToLower());
                        switch (next)
                        {
                            case "a": return "â";
                            case "e": return "ê";
                            case "i": return "î";
                            case "o": return "ô";
                            case "u": return "û";
                            case "A": return "Â";
                            case "E": return "Ê";
                            case "I": return "Î";
                            case "O": return "Ô";
                            case "U": return "Û";
                        }
                        return string.Empty;
                    case 0xc4:
                        skipNext = "ao".Contains(next.ToLower());
                        switch (next)
                        {
                            case "a": return "ã";
                            case "o": return "õ";
                            case "A": return "Ã";
                            case "E": return "Õ";
                        }
                        return string.Empty;
                    case 0xc8:
                        skipNext = "aeiou".Contains(next.ToLower());
                        switch (next)
                        {
                            case "a": return "ä";
                            case "e": return "ë";
                            case "i": return "ï";
                            case "o": return "ö";
                            case "u": return "ü";
                            case "A": return "Ä";
                            case "E": return "Ë";
                            case "I": return "Ï";
                            case "O": return "Ö";
                            case "U": return "Ü";
                        }
                        return string.Empty;
                    default:
                        return encoding.GetString(buffer, index, 1);
                }
            }
            else if (header.CharacterCodeTableNumber == "01") // Latin/Cyrillic alphabet - from ISO 8859/5-1988
            {
                Encoding encoding = Encoding.GetEncoding("ISO-8859-5");
                return encoding.GetString(buffer, index, 1);
            }
            else if (header.CharacterCodeTableNumber == "02") // Latin/Arabic alphabet - from ISO 8859/6-1987
            {
                Encoding encoding = Encoding.GetEncoding("ISO-8859-6");
                return encoding.GetString(buffer, index, 1);
            }
            else if (header.CharacterCodeTableNumber == "03") // Latin/Greek alphabet - from ISO 8859/7-1987
            {
                Encoding encoding = Encoding.GetEncoding("ISO-8859-7"); // or ISO-8859-1 ?
                return encoding.GetString(buffer, index, 1);
            }
            else if (header.CharacterCodeTableNumber == "04") // Latin/Hebrew alphabet - from ISO 8859/8-1988
            {
                Encoding encoding = Encoding.GetEncoding("ISO-8859-8");
                return encoding.GetString(buffer, index, 1);
            }

            return string.Empty;
        }

        private IEnumerable<EbuTextTimingInformation> ReadTTI(byte[] buffer, EbuGeneralSubtitleInformation header)
        {
            const int StartOfTTI = 1024;
            const int TTISize = 128;
            const byte TextFieldCRLF = 0x8A;
            const byte TextFieldTerminator = 0x8F;
            const byte ItalicsOn = 0x80;
            const byte ItalicsOff = 0x80;
            const byte UnderlineOn = 0x82;
            const byte UnderlineOff = 0x83;

            List<EbuTextTimingInformation> list = new List<EbuTextTimingInformation>();
            int index = StartOfTTI;
            while (index + TTISize < buffer.Length)
            {
                var tti = new EbuTextTimingInformation();

                tti.TimeCodeInHours = buffer[index + 5 + 0];
                tti.TimeCodeInMinutes = buffer[index + 5 + 1];
                tti.TimeCodeInSeconds = buffer[index + 5 + 2];
                tti.TimeCodeInMilliseconds = (int)(1000.0 / (header.FrameRate / buffer[index + 5 + 3]));

                tti.TimeCodeOutHours = buffer[index + 9 + 0];
                tti.TimeCodeOutMinutes = buffer[index + 9 + 1];
                tti.TimeCodeOutSeconds = buffer[index + 9 + 2];
                tti.TimeCodeOutMilliseconds = (int)(1000 / (header.FrameRate / buffer[index + 9 + 3]));
                tti.CommentFlag = buffer[index + 15];

                // build text
                bool skipNext = false;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 112; i++)
                {
                    if (skipNext)
                    {
                        skipNext = false;
                    }
                    else
                    {
                        if (buffer[index + 16 + i] == TextFieldCRLF)
                            sb.AppendLine();
                        else if (buffer[index + 16 + i] == ItalicsOn)
                            sb.Append("<i>");
                        else if (buffer[index + 16 + i] == ItalicsOff)
                            sb.Append("</i>");
                        else if (buffer[index + 16 + i] == UnderlineOn)
                            sb.Append("<u>");
                        else if (buffer[index + 16 + i] == UnderlineOff)
                            sb.Append("</u>");
                        else if (buffer[index + 16 + i] == TextFieldTerminator)
                            break;
                        else if ((buffer[index + 16 + i] >= 0x20 && buffer[index + 16 + i] <= 0x7F) || buffer[index + 16 + i] >= 0xA1)
                            sb.Append(GetCharacter(out skipNext, header, buffer, index + 16 + i));
                    }
                }
                tti.TextField = sb.ToString();


                index += TTISize;
                list.Add(tti);
            }
            return list;
        }

    }
}