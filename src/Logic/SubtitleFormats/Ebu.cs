using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// EBU Subtitling data exchange format
    /// </summary>
    public class Ebu : SubtitleFormat
    {

        internal EbuGeneralSubtitleInformation Header;

        /// <summary>
        /// GSI block (1024 bytes)
        /// </summary>
        internal class EbuGeneralSubtitleInformation
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
                DisplayStandardCode = "0";
                CharacterCodeTableNumber = "00";
                LanguageCode = "0A";
                OriginalProgrammeTitle = "No Title                        ";
                OriginalEpisodeTitle = "                                ";
                TranslatedProgrammeTitle = string.Empty.PadLeft(32, ' ');
                TranslatedEpisodeTitle = string.Empty.PadLeft(32, ' ');
                TranslatorsName = string.Empty.PadLeft(32, ' ');
                TranslatorsContactDetails = string.Empty.PadLeft(32, ' ');
                SubtitleListReferenceCode = "0               ";
                CreationDate = "101021";
                RevisionDate = "101021";
                RevisionNumber = "01";
                TotalNumberOfTextAndTimingInformationBlocks = "00725";
                TotalNumberOfSubtitles = "00725";
                TotalNumberOfSubtitleGroups = "001";
                MaximumNumberOfDisplayableCharactersInAnyTextRow = "40";
                MaximumNumberOfDisplayableRows = "23";
                TimeCodeStatus = "1";
                TimeCodeStartOfProgramme = "00000000";
                TimeCodeFirstInCue = "00000001";
                TotalNumberOfDisks = "1";
                DiskSequenceNumber = "1";
                CountryOfOrigin = "USA";
                Publisher = string.Empty.PadLeft(32, ' ');
                EditorsName = string.Empty.PadLeft(32, ' ');
                EditorsContactDetails = string.Empty.PadLeft(32, ' ');
                SpareBytes = string.Empty.PadLeft(75, ' ');
                UserDefinedArea = string.Empty.PadLeft(576, ' ');
            }

            public override string ToString()
            {
                string result =
                          CodePageNumber +
                          DiskFormatCode +
                          DisplayStandardCode +
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
                          TotalNumberOfTextAndTimingInformationBlocks +
                          TotalNumberOfSubtitles +
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
                          EditorsContactDetails +
                          SpareBytes +
                          UserDefinedArea;
                if (result.Length == 1024)
                    return result;
                throw new Exception("Length must be 1024");
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

            public EbuTextTimingInformation()
            {
                SubtitleGroupNumber = 0;
                ExtensionBlockNumber = 255;
                CumulativeStatus = 0;
                VerticalPosition = 0x16;
                JustificationCode = 0;
                CommentFlag = 0;
            }

            internal byte[] GetBytes(EbuGeneralSubtitleInformation header)
            {
                byte[] buffer = new byte[128]; // Text and Timing Information (TTI) block consists of 128 bytes

                buffer[0] = SubtitleGroupNumber;
                byte[] temp = BitConverter.GetBytes(SubtitleNumber);
                buffer[1] = temp[0];
                buffer[2] = temp[1];
                buffer[3] = ExtensionBlockNumber;
                buffer[4] = CumulativeStatus;

                buffer[5] = (byte)TimeCodeInHours;
                buffer[6] = (byte)TimeCodeInMinutes;
                buffer[7] = (byte)TimeCodeInSeconds;
                buffer[8] = GetFrameFromMilliseconds(TimeCodeInMilliseconds, header.FrameRate);

                buffer[9] = (byte)TimeCodeOutHours;
                buffer[10] = (byte)TimeCodeOutMinutes;
                buffer[11] = (byte)TimeCodeOutSeconds;
                buffer[12] = GetFrameFromMilliseconds(TimeCodeOutMilliseconds, header.FrameRate);

                buffer[13] = VerticalPosition;
                buffer[14] = JustificationCode;
                buffer[15] = CommentFlag;

                Encoding encoding = Encoding.Default;
                if (header.CharacterCodeTableNumber == "00")
                {
                    encoding = Encoding.GetEncoding(20269);
                    // 0xC1—0xCF combines characters - http://en.wikipedia.org/wiki/ISO/IEC_6937
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc1 }), "ÀÈÌÒÙàèìòù", "AEIOUaeiou");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc2 }), "ÁĆÉÍĹŃÓŔŚÚÝŹáćéģíĺńóŕśúýź", "ACEILNORSUYZacegilnorsuyz");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc3 }), "ÂĈÊĜĤÎĴÔŜÛŴŶâĉêĝĥîĵôŝûŵŷ", "ACEGHIJOSUWYaceghijosuwy");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc4 }), "ÃĨÑÕŨãĩñõũ", "AINOUainou");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc5 }), "ĀĒĪŌŪāēīōū", "AEIOUaeiou");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc6 }), "ĂĞŬăğŭ", "AGUagu");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc7 }), "ĊĖĠİŻċėġıż", "CEGIZcegiz");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xc8 }), "ÄËÏÖÜŸäëïöüÿ", "AEIOUYaeiouy");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xca }), "ÅŮåů", "AUau");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xcb }), "ÇĢĶĻŅŖŞŢçķļņŗşţ", "CGKLNRSTcklnrst");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xcd }), "ŐŰőű", "OUou");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xce }), "ĄĘĮŲąęįų", "AEIUaeiu");
                    TextField = ReplaceSpecialCharactersWithTwoByteEncoding(TextField, encoding.GetString(new byte[] { 0xcf }), "ČĎĚĽŇŘŠŤŽčďěľňřšťž", "CDELNRSTZcdelnrstz");
                }
                else if (header.CharacterCodeTableNumber == "01") // Latin/Cyrillic alphabet - from ISO 8859/5-1988
                {
                    encoding = Encoding.GetEncoding("ISO-8859-5");
                }
                else if (header.CharacterCodeTableNumber == "02") // Latin/Arabic alphabet - from ISO 8859/6-1987
                {
                    encoding = Encoding.GetEncoding("ISO-8859-6");
                }
                else if (header.CharacterCodeTableNumber == "03") // Latin/Greek alphabet - from ISO 8859/7-1987
                {
                    encoding = Encoding.GetEncoding("ISO-8859-7"); // or ISO-8859-1 ?
                }
                else if (header.CharacterCodeTableNumber == "04") // Latin/Hebrew alphabet - from ISO 8859/8-1988
                {
                    encoding = Encoding.GetEncoding("ISO-8859-8");
                }

                // italic/underline
                string italicsOn = encoding.GetString(new byte[] { 0x80 });
                string italicsOff = encoding.GetString(new byte[] { 0x81 });
                string underlineOn = encoding.GetString(new byte[] { 0x82 });
                string underlineOff = encoding.GetString(new byte[] { 0x83 });
                TextField = TextField.Replace("<i>", italicsOn);
                TextField = TextField.Replace("<I>", italicsOn);
                TextField = TextField.Replace("</i>", italicsOff);
                TextField = TextField.Replace("</I>", italicsOff);
                TextField = TextField.Replace("<u>", underlineOn);
                TextField = TextField.Replace("<U>", underlineOn);
                TextField = TextField.Replace("</u>", underlineOff);
                TextField = TextField.Replace("</U>", underlineOff);

                //font tags
                string[] lines = TextField.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                string veryFirstColor = null;
                foreach (string line in lines)
                {
                    string firstColor = null;
                    string s = line;
                    int start = s.IndexOf("<font ");
                    if (start >= 0)
                    {
                        int end = s.IndexOf(">", start);
                        if (end > 0)
                        {
                            string f = s.Substring(start, end - start);
                            if (f.Contains(" color="))
                            {
                                int colorStart = f.IndexOf(" color=");
                                if (s.IndexOf("\"", colorStart + " color=".Length + 1) > 0)
                                {
                                    int colorEnd = f.IndexOf("\"", colorStart + " color=".Length + 1);
                                    if (colorStart > 1)
                                    {
                                        string color = f.Substring(colorStart + 7, colorEnd - (colorStart + 7));
                                        color = color.Trim('\'');
                                        color = color.Trim('\"');
                                        color = color.Trim('#');

                                        s = s.Remove(start, end - start + 1);
                                        if (veryFirstColor == null)
                                            veryFirstColor = GetNearestEbuColorCode(color, encoding);
                                        if (firstColor == null)
                                            firstColor = GetNearestEbuColorCode(color, encoding);
                                        else
                                            s = s.Insert(start, GetNearestEbuColorCode(color, encoding));
                                    }
                                }
                            }
                        }
                    }
                    //byte colorByte = 0x07; // white
                    byte colorByte = 255;
                    if (!string.IsNullOrEmpty(veryFirstColor))
                        colorByte = encoding.GetBytes(veryFirstColor)[0];
                    if (!string.IsNullOrEmpty(firstColor))
                        colorByte = encoding.GetBytes(firstColor)[0];
                    string prefix = encoding.GetString(new byte[] { 0xd, colorByte, 0xb, 0xb });

                    if (colorByte != 255)
                        sb.Append(prefix);
                    sb.AppendLine(s);
                }
                TextField = Utilities.RemoveHtmlTags(sb.ToString()).TrimEnd();

                // newline
                string newline = encoding.GetString(new byte[] { 0x0a, 0x0a, 0x8a, 0x8a });
                TextField = TextField.Replace(Environment.NewLine, newline);

                string endOfLine = encoding.GetString(new byte[] { 0x0a, 0x0a, 0x8a });
                TextField += endOfLine;

                // convert text to bytes
                byte[] bytes = encoding.GetBytes(TextField);

                for (int i = 0; i < 112; i++)
                {
                    if (i < bytes.Length)
                        buffer[16 + i] = bytes[i];
                    //else if (i == bytes.Length)
                    //    buffer[16 + i] = 0x8f;
                    else
                        buffer[16 + i] = 0x8f;
                }
                return buffer;
            }

            private string GetNearestEbuColorCode(string color, Encoding encoding)
            {
                color = color.ToLower();
                if (color == "black" || color == "000000")
                    return encoding.GetString(new byte[] { 0x00 }); // black
                else if (color == "red" || color == "ff0000")
                    return encoding.GetString(new byte[] { 0x01 }); // red
                else if (color == "green" || color == "00ff00")
                    return encoding.GetString(new byte[] { 0x02 }); // green
                else if (color == "yellow" || color == "ffff00")
                    return encoding.GetString(new byte[] { 0x03 }); // yellow
                else if (color == "blue" || color == "0000ff")
                    return encoding.GetString(new byte[] { 0x04 }); // blue
                else if (color == "magenta" || color == "ff00ff")
                    return encoding.GetString(new byte[] { 0x05 }); // magenta
                else if (color == "cyan" || color == "00ffff")
                    return encoding.GetString(new byte[] { 0x06 }); // cyan
                else if (color == "white" || color == "ffffff")
                    return encoding.GetString(new byte[] { 0x07 }); // white
                else if (color.Length == 6)
                {
                    Regex regExpr = new Regex(@"^[a-f0-9]{6}$", RegexOptions.Compiled);
                    if (regExpr.IsMatch(color))
                    {
                        const int MaxDiff = 130;
                        int r = int.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        int g = int.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        int b = int.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                        if (r < MaxDiff && g < MaxDiff && b < MaxDiff)
                            return encoding.GetString(new byte[] { 0x00 }); // black
                        if (r > 255 - MaxDiff && g < MaxDiff && b < MaxDiff)
                            return encoding.GetString(new byte[] { 0x01 }); // red
                        if (r < MaxDiff && g > 255 - MaxDiff && b < MaxDiff)
                            return encoding.GetString(new byte[] { 0x02 }); // green
                        if (r > 255 - MaxDiff && g > 255 - MaxDiff && b < MaxDiff)
                            return encoding.GetString(new byte[] { 0x03 }); // yellow
                        if (r < MaxDiff && g < MaxDiff && b > 255 - MaxDiff)
                            return encoding.GetString(new byte[] { 0x04 }); // blue
                        if (r > 255 - MaxDiff && g < MaxDiff && b > 255 - MaxDiff)
                            return encoding.GetString(new byte[] { 0x05 }); // magenta
                        if (r < MaxDiff && g > 255 - MaxDiff && b > 255 - MaxDiff)
                            return encoding.GetString(new byte[] { 0x06 }); // cyan
                        if (r > 255-MaxDiff && g > 255-MaxDiff && b > 255-MaxDiff)
                            return encoding.GetString(new byte[] { 0x07 }); // white
                    }
                }
                return string.Empty;
            }

            private string ReplaceSpecialCharactersWithTwoByteEncoding(string text, string specialCharacter, string originalCharacters, string newCharacters)
            {
                if (originalCharacters.Length != newCharacters.Length)
                    throw new ArgumentException("originalCharacters and newCharacters must have equal length");

                for (int i = 0; i < newCharacters.Length; i++)
                    text = text.Replace(originalCharacters[i].ToString(), specialCharacter + newCharacters[i]);
                return text;
            }

            public static byte GetFrameFromMilliseconds(int milliseconds, double frameRate)
            {
                int frame = (int)(milliseconds / (1000.0 / frameRate));
                return (byte)(frame);
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
            EbuGeneralSubtitleInformation header = new EbuGeneralSubtitleInformation();
            EbuSaveOptions saveOptions = new EbuSaveOptions();
            if (subtitle.Header != null && subtitle.Header.Length > 1024 && (subtitle.Header.Contains("STL25") || subtitle.Header.Contains("STL30")))
            {
                header = ReadHeader(Encoding.UTF8.GetBytes(subtitle.Header));
                saveOptions.Initialize(header, 0, null, subtitle);
            }
            else
            {
                saveOptions.Initialize(header, 0, fileName, subtitle);
            }

            if (saveOptions.ShowDialog() != DialogResult.OK)
                return;

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            header.TotalNumberOfSubtitles = ((subtitle.Paragraphs.Count+1).ToString()).PadLeft(5, '0'); // seems to be 1 higher than actual number of subtitles
            header.TotalNumberOfTextAndTimingInformationBlocks = header.TotalNumberOfSubtitles;

            string today = string.Format("{0:00}{1:00}{2:00}", DateTime.Now.Year.ToString().Remove(0, 2), DateTime.Now.Month, DateTime.Now.Day);
            if (today.Length == 6)
            {
                header.CreationDate = today;
                header.RevisionDate = today;
            }

            Paragraph firstParagraph = subtitle.GetParagraphOrDefault(0);
            if (firstParagraph != null)
            {
                TimeCode tc = firstParagraph.StartTime;
                string firstTimeCode = string.Format("{0:00}{1:00}{2:00}{3:00}", tc.Hours, tc.Minutes, tc.Seconds, EbuTextTimingInformation.GetFrameFromMilliseconds(tc.Milliseconds, header.FrameRate));
                if (firstTimeCode.Length == 8)
                    header.TimeCodeFirstInCue = firstTimeCode;
            }

            byte[] buffer = ASCIIEncoding.ASCII.GetBytes(header.ToString());
            fs.Write(buffer, 0, buffer.Length);

            int subtitleNumber = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                EbuTextTimingInformation tti = new EbuTextTimingInformation();
                if (p.Text.Contains(Environment.NewLine))
                    tti.VerticalPosition = 0x14;
                else
                    tti.VerticalPosition = 0x16;
                tti.JustificationCode = saveOptions.JustificationCode;
                tti.SubtitleNumber = (ushort)subtitleNumber;
                tti.TextField = p.Text;
                tti.TimeCodeInHours = p.StartTime.Hours;
                tti.TimeCodeInMinutes = p.StartTime.Minutes;
                tti.TimeCodeInSeconds = p.StartTime.Seconds;
                tti.TimeCodeInMilliseconds = p.StartTime.Milliseconds;
                tti.TimeCodeOutHours = p.EndTime.Hours;
                tti.TimeCodeOutMinutes = p.EndTime.Minutes;
                tti.TimeCodeOutSeconds = p.EndTime.Seconds;
                tti.TimeCodeOutMilliseconds = p.EndTime.Milliseconds;
                buffer = tti.GetBytes(header);
                fs.Write(buffer, 0, buffer.Length);
                subtitleNumber++;
            }
            fs.Close();
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
                        return Utilities.IsInteger(header.CodePageNumber);
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
            subtitle.Header = Encoding.UTF8.GetString(buffer);
            foreach (EbuTextTimingInformation tti in ReadTTI(buffer, header))
            {
                Paragraph p = new Paragraph();
                p.Text = tti.TextField;
                p.StartTime = new TimeCode(tti.TimeCodeInHours, tti.TimeCodeInMinutes, tti.TimeCodeInSeconds, tti.TimeCodeInMilliseconds);
                p.EndTime = new TimeCode(tti.TimeCodeOutHours, tti.TimeCodeOutMinutes, tti.TimeCodeOutSeconds, tti.TimeCodeOutMilliseconds);
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
            Header = header;
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
            header.CountryOfOrigin = Encoding.ASCII.GetString(buffer, 274, 3);
            header.SpareBytes = Encoding.ASCII.GetString(buffer, 373, 75);
            header.UserDefinedArea = Encoding.ASCII.GetString(buffer, 448, 576);

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
            const byte ItalicsOff = 0x81;
            const byte UnderlineOn = 0x82;
            const byte UnderlineOff = 0x83;

            List<EbuTextTimingInformation> list = new List<EbuTextTimingInformation>();
            int index = StartOfTTI;
            while (index + TTISize <= buffer.Length)
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
                for (int i = 0; i < 112; i++) // skip fist byte (seems to be always 0xd/32/space - thx Iban)
                {
                    byte b = buffer[index + 16 + i];
                    if (b <= 0xf && (i == 0 || i == 2 || i == 3))
                    {
                        // not used, 0=0xd, 2=0xb, 3=0xb
                    }
                    else if (skipNext)
                    {
                        skipNext = false;
                    }
                    else
                    {
                        if (b >= 0 && b <= 0x17)
                        {
                            switch (b)
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
                        if (b == TextFieldCRLF)
                            sb.AppendLine();
                        else if (b == ItalicsOn)
                            sb.Append("<i>");
                        else if (b == ItalicsOff)
                            sb.Append("</i>");
                        else if (b == UnderlineOn)
                            sb.Append("<u>");
                        else if (b == UnderlineOff)
                            sb.Append("</u>");
                        else if (b == TextFieldTerminator)
                            break;
                        else if ((b >= 0x20 && b <= 0x7F) || b >= 0xA1)
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
                tti.TextField = sb.ToString().Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine).TrimEnd() + endTags;
                index += TTISize;
                list.Add(tti);
            }
            return list;
        }

    }
}