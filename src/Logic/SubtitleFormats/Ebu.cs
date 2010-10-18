using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

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

            public EbuGeneralSubtitleInformation()
            {
                CodePageNumber = "437";
                DiskFormatCode = "STL25.01";
            }

            public override string  ToString()
            {
                string result = 
                          CodePageNumber + 
                          DiskFormatCode + 
                          DisplayStandardCode  + 
                          CharacterCodeTableNumber +
                          LanguageCode +
                          OriginalProgrammeTitle +
                          OriginalEpisodeTitle +
                          TranslatedProgrammeTitle +
                          TranslatedEpisodeTitle + 
                          TranslatorsName +
                          TranslatorsContactDetails +
                          SubtitleListReferenceCode +
                          CreationDate +
                          RevisionDate +
                          RevisionNumber +
                          TotalNumberOfTextAndTimingInformationBlocks  +
                          TotalNumberOfSubtitles  +
                          TotalNumberOfSubtitleGroups +
                          MaximumNumberOfDisplayableCharactersInAnyTextRow +
                          MaximumNumberOfDisplayableRows +
                          TimeCodeStatus +
                          TimeCodeStartOfProgramme +
                          TimeCodeFirstInCue +
                          TotalNumberOfDisks +
                          DiskSequenceNumber +
                          CountryOfOrigin +
                          Publisher +
                          EditorsName +
                          EditorsContactDetails  +
                          SpareBytes +
                          UserDefinedArea;
                return result;
            }
        }

        /// <summary>
        /// TTI block 128 bytes 
        /// </summary>
        private class EbuTextTimingInformation
        {
            public byte SubtitleGroupNumber { get; set; }
            public UInt16 SubtitleNumber { get; set; }
            public byte ExtensionBlockNumber { get; set; }
            public byte CumulativeStatus { get; set; }
            public int TimeCodeInHours { get; set; }
            public int TimeCodeInMinutes { get; set; }
            public int TimeCodeInSeconds { get; set; }
            public int TimeCodeInMilliseconds { get; set; }
            public int TimeCodeOutHours { get; set; }
            public int TimeCodeOutMinutes { get; set; }
            public int TimeCodeOutSeconds { get; set; }
            public int TimeCodeOutMilliseconds { get; set; }
            public byte VerticalPosition { get; set; }
            public byte JustificationCode { get; set; }
            public byte CommentFlag { get; set; }
            public string TextField { get; set; }

            internal byte[] GetBytes()
            {
                byte[] buffer = new byte[128]; // Text and Timing Information (TTI) block consists of 128 bytes

                buffer[0] = SubtitleGroupNumber;
                byte[] temp = BitConverter.GetBytes(SubtitleNumber);
                buffer[1] = temp[0];
                buffer[2] = temp[1];
                buffer[3] = ExtensionBlockNumber;
                buffer[4] = CumulativeStatus;

                //TODO: time codes

                buffer[13] = VerticalPosition;
                buffer[14] = JustificationCode;
                buffer[15] = CommentFlag;

                //TODO: text field... 

                return buffer;
            }
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

        public void Save(string fileName, Subtitle subtitle)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            EbuGeneralSubtitleInformation header = new EbuGeneralSubtitleInformation();
            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(header.ToString());
            fs.Write(buffer, 0, buffer.Length);

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                EbuTextTimingInformation tti = new EbuTextTimingInformation();
                buffer = tti.GetBytes();
                fs.Write(buffer, 0, buffer.Length);
            }
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
                    case 0xc1: // Grave
                        skipNext = "AEIOUaeiou".Contains(next);
                        switch (next)
                        {
                            case "A": return "À";
                            case "E": return "È";
                            case "I": return "Ì";
                            case "O": return "Ò";
                            case "U": return "Ù";
                            case "a": return "à";
                            case "e": return "è";
                            case "i": return "ì";
                            case "o": return "ò";
                            case "u": return "ù";
                        }
                        return string.Empty;
                    case 0xc2: // Acute
                        skipNext = "ACEILNORSUYZacegilnorsuyz".Contains(next);
                        switch (next)
                        {
                            case "A": return "Á";
                            case "C": return "Ć";
                            case "E": return "É";
                            case "I": return "Í";
                            case "L": return "Ĺ";
                            case "N": return "Ń";
                            case "O": return "Ó";
                            case "R": return "Ŕ";
                            case "S": return "Ś";
                            case "U": return "Ú";
                            case "Y": return "Ý";
                            case "Z": return "Ź";
                            case "a": return "á";
                            case "c": return "ć";
                            case "e": return "é";
                            case "g": return "ģ";
                            case "i": return "í";
                            case "l": return "ĺ"; 
                            case "n": return "ń";
                            case "o": return "ó";
                            case "r": return "ŕ";
                            case "s": return "ś";
                            case "u": return "ú";
                            case "y": return "ý";
                            case "z": return "ź";
                        }
                        return string.Empty;
                    case 0xc3: // Circumflex
                        skipNext = "ACEGHIJOSUWYaceghijosuwy".Contains(next); 
                        switch (next)
                        {
                            case "A": return "Â";
                            case "C": return "Ĉ";
                            case "E": return "Ê";
                            case "G": return "Ĝ";
                            case "H": return "Ĥ";
                            case "I": return "Î";
                            case "J": return "Ĵ";
                            case "O": return "Ô";
                            case "S": return "Ŝ";
                            case "U": return "Û";
                            case "W": return "Ŵ";
                            case "Y": return "Ŷ";
                            case "a": return "â";
                            case "c": return "ĉ";
                            case "e": return "ê";
                            case "g": return "ĝ";
                            case "h": return "ĥ";
                            case "i": return "î";
                            case "j": return "ĵ";
                            case "o": return "ô";
                            case "s": return "ŝ";
                            case "u": return "û";
                            case "w": return "ŵ";
                            case "y": return "ŷ";
                        }
                        return string.Empty;
                    case 0xc4: // Tilde
                        skipNext = "AINOUainou".Contains(next);
                        switch (next)
                        {
                            case "A": return "Ã";
                            case "I": return "Ĩ";
                            case "N": return "Ñ";
                            case "O": return "Õ";
                            case "U": return "Ũ";
                            case "a": return "ã";
                            case "i": return "ĩ";
                            case "n": return "ñ";
                            case "o": return "õ";
                            case "u": return "ũ";
                        }
                        return string.Empty;
                    case 0xc5: // Macron
                        skipNext = "AEIOUaeiou".Contains(next);
                        switch (next) 
                        {
                            case "A": return "Ā";
                            case "E": return "Ē";
                            case "I": return "Ī";
                            case "O": return "Ō";
                            case "U": return "Ū";
                            case "a": return "ā";
                            case "e": return "ē";
                            case "i": return "ī";
                            case "o": return "ō";
                            case "u": return "ū";
                        }
                        return string.Empty;
                    case 0xc6: // Breve
                        skipNext = "AGUagu".Contains(next);
                        switch (next)
                        {
                            case "A": return "Ă";
                            case "G": return "Ğ";
                            case "U": return "Ŭ";
                            case "a": return "ă";
                            case "g": return "ğ";
                            case "u": return "ŭ";
                        }
                        return string.Empty;
                    case 0xc7: // Dot
                        skipNext = "CEGIZcegiz".Contains(next);
                        switch (next)
                        {
                            case "C": return "Ċ";
                            case "E": return "Ė";
                            case "G": return "Ġ";
                            case "I": return "İ";
                            case "Z": return "Ż";
                            case "c": return "ċ";
                            case "e": return "ė";
                            case "g": return "ġ";
                            case "i": return "ı";
                            case "z": return "ż";
                        }
                        return string.Empty;
                    case 0xc8: // Umlaut or diæresis
                        skipNext = "AEIOUYaeiouy".Contains(next);
                        switch (next)
                        {
                            case "A": return "Ä";
                            case "E": return "Ë";
                            case "I": return "Ï";
                            case "O": return "Ö";
                            case "U": return "Ü";
                            case "Y": return "Ÿ";
                            case "a": return "ä";
                            case "e": return "ë";
                            case "i": return "ï";
                            case "o": return "ö";
                            case "u": return "ü";
                            case "y": return "ÿ";
                        }
                        return string.Empty;
                    case 0xca: // Ring
                        skipNext = "AUau".Contains(next);
                        switch (next)
                        {
                            case "A": return "Å";
                            case "U": return "Ů";
                            case "a": return "å";
                            case "u": return "ů";
                        }
                        return string.Empty;
                    case 0xcb: // Cedilla
                        skipNext = "CGKLNRSTcklnrst".Contains(next);
                        switch (next)
                        {
                            case "C": return "Ç";
                            case "G": return "Ģ";
                            case "K": return "Ķ";
                            case "L": return "Ļ";
                            case "N": return "Ņ";
                            case "R": return "Ŗ";
                            case "S": return "Ş";
                            case "T": return "Ţ";
                            case "c": return "ç";
                            case "k": return "ķ";
                            case "l": return "ļ";
                            case "n": return "ņ";
                            case "r": return "ŗ";
                            case "s": return "ş";
                            case "t": return "ţ";
                        }
                        return string.Empty;
                    case 0xcd: // DoubleAcute
                        skipNext = "OUou".Contains(next);
                        switch (next)
                        {
                            case "O": return "Ő";
                            case "U": return "Ű";
                            case "o": return "ő";
                            case "u": return "ű";
                        }
                        return string.Empty;
                    case 0xce: // Ogonek
                        skipNext = "AEIUaeiu".Contains(next);
                        switch (next)
                        {
                            case "A": return "Ą";
                            case "E": return "Ę";
                            case "I": return "Į";
                            case "U": return "Ų";
                            case "a": return "ą";
                            case "e": return "ę";
                            case "i": return "į";
                            case "u": return "ų";
                        }
                        return string.Empty;
                    case 0xcf: // Caron
                        skipNext = "CDELNRSTZcdelnrstz".Contains(next);
                        switch (next)
                        {
                            case "C": return "Č";
                            case "D": return "Ď";
                            case "E": return "Ě";
                            case "L": return "Ľ";
                            case "N": return "Ň";
                            case "R": return "Ř";
                            case "S": return "Š";
                            case "T": return "Ť";
                            case "Z": return "Ž";
                            case "c": return "č";
                            case "d": return "ď";
                            case "e": return "ě";
                            case "l": return "ľ";
                            case "n": return "ň";
                            case "r": return "ř";
                            case "s": return "š";
                            case "t": return "ť";
                            case "z": return "ž";
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
                
                tti.SubtitleGroupNumber = buffer[index];
                tti.SubtitleNumber = (ushort)(buffer[index + 2] * 256+ buffer[index + 1]);
                tti.ExtensionBlockNumber = buffer[index + 3];
                tti.CumulativeStatus = buffer[index + 4];

                tti.TimeCodeInHours = buffer[index + 5 + 0];
                tti.TimeCodeInMinutes = buffer[index + 5 + 1];
                tti.TimeCodeInSeconds = buffer[index + 5 + 2];
                tti.TimeCodeInMilliseconds = (int)(1000.0 / (header.FrameRate / buffer[index + 5 + 3]));

                tti.TimeCodeOutHours = buffer[index + 9 + 0];
                tti.TimeCodeOutMinutes = buffer[index + 9 + 1];
                tti.TimeCodeOutSeconds = buffer[index + 9 + 2];
                tti.TimeCodeOutMilliseconds = (int)(1000 / (header.FrameRate / buffer[index + 9 + 3]));

                tti.VerticalPosition = buffer[index + 13];
                tti.JustificationCode = buffer[index + 14];
                tti.CommentFlag = buffer[index + 15];

                // build text
                bool skipNext = false;
                StringBuilder sb = new StringBuilder();
                string endTags = string.Empty;                
                string color = string.Empty;
                string lastColor = string.Empty;
                for (int i = 0; i < 112; i++)
                {
                    if (skipNext)
                    {
                        skipNext = false;
                    }
                    else
                    {
                        if (buffer[index + 16 + i] >= 0 && buffer[index + 16 + i] <= 0x17)
                        {
                            switch (buffer[index + 16 + i])
                            {
                                case 0x00:
                                case 0x10: color = "Black";
                                    break;
                                case 0x01:
                                case 0x11: color = "Red";
                                    break;
                                case 0x02:
                                case 0x12: color = "Green";
                                    break;
                                case 0x03:
                                case 0x13: color = "Yellow";
                                    break;
                                case 0x04:
                                case 0x14: color = "Blue";
                                    break;
                                case 0x05:
                                case 0x15: color = "Magenta";
                                    break;
                                case 0x06:
                                case 0x16: color = "Cyan";
                                    break;
                                case 0x07:
                                case 0x17: color = "White";
                                    break;
                            }
                        }
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
                        {
                            string ch = GetCharacter(out skipNext, header, buffer, index + 16 + i);
                            if (ch != " ")
                            {
                                if (color != lastColor && color.Length > 0)
                                {
                                    endTags = "</font>";
                                    if (lastColor.Length > 0)
                                        sb.Append("</font>");
                                    sb.Append("<font color=\"" + color + "\">");
                                }
                                lastColor = color;
                            }
                            sb.Append(ch);
                        }
                    }
                }
                tti.TextField = sb.ToString().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine) + endTags;
                tti.TextField = tti.TextField.TrimEnd();

                index += TTISize;
                list.Add(tti);
            }
            return list;
        }

    }
}