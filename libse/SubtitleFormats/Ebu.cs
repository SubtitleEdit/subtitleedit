﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// EBU Subtitling data exchange format
    /// </summary>
    public class Ebu : SubtitleFormat, IBinaryPersistableSubtitle
    {
        private static readonly Regex FontTagsNoSpace1 = new Regex("[a-zA-z.!?]</font><font[a-zA-Z =\"']+>[a-zA-Z-]", RegexOptions.Compiled);
        private static readonly Regex FontTagsNoSpace2 = new Regex("[a-zA-z.!?]<font[a-zA-Z =\"']+>[a-zA-Z-]", RegexOptions.Compiled);

        private static readonly Regex FontTagsStartSpace = new Regex("^<font color=\"[A-Za-z]+\"> ", RegexOptions.Compiled); // "<font color=\"Black\"> "
        private static readonly Regex FontTagsNewLineSpace = new Regex("[\r\n]+<font color=\"[A-Za-z]+\"> ", RegexOptions.Compiled); // "\r\n<font color=\"Black\"> "

        private const string LanguageCodeChinese = "75";

        public interface IEbuUiHelper
        {
            void Initialize(EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle);
            bool ShowDialogOk();
            byte JustificationCode { get; set; }
        }

        public static IEbuUiHelper EbuUiHelper { get; set; }

        private static readonly Regex RegExprColor = new Regex(@"^[a-f0-9]{6}$", RegexOptions.Compiled);

        public List<int> VerticalPositions = new List<int>();
        public List<int> JustificationCodes = new List<int>();

        public EbuGeneralSubtitleInformation Header;

        /// <summary>
        /// GSI block (1024 bytes)
        /// </summary>
        public class EbuGeneralSubtitleInformation
        {
            public string CodePageNumber { get; set; } // 0..2
            public string DiskFormatCode { get; set; } // 3..10
            public double FrameRateFromSaveDialog { get; set; }
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
                    if (FrameRateFromSaveDialog > 20)
                        return FrameRateFromSaveDialog;
                    if (DiskFormatCode.StartsWith("STL23", StringComparison.Ordinal))
                        return 23.0;
                    if (DiskFormatCode.StartsWith("STL24", StringComparison.Ordinal))
                        return 24.0;
                    if (DiskFormatCode.StartsWith("STL25", StringComparison.Ordinal))
                        return 25.0;
                    if (DiskFormatCode.StartsWith("STL29", StringComparison.Ordinal))
                        return 29.0;
                    if (DiskFormatCode.StartsWith("STL35", StringComparison.Ordinal))
                        return 35.0;
                    if (DiskFormatCode.StartsWith("STL48", StringComparison.Ordinal))
                        return 48.0;
                    if (DiskFormatCode.StartsWith("STL50", StringComparison.Ordinal))
                        return 50.0;
                    if (DiskFormatCode.StartsWith("STL60", StringComparison.Ordinal))
                        return 60.0;
                    return 30.0; // should be DiskFormatcode STL30.01
                }
            }

            public EbuGeneralSubtitleInformation()
            {
                CodePageNumber = "437";
                DiskFormatCode = "STL25.01";
                DisplayStandardCode = "0"; // 0=Open subtitling
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
                return "Length must be 1024 but is " + result.Length;
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
                JustificationCode = 2;
                CommentFlag = 0;
            }

            public byte[] GetBytes(EbuGeneralSubtitleInformation header)
            {
                var buffer = new byte[128]; // Text and Timing Information (TTI) block consists of 128 bytes

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

                var encoding = Encoding.Default;
                if (header.LanguageCode == LanguageCodeChinese)
                {
                    var lines = HtmlUtil.RemoveHtmlTags(TextField, true).SplitToLines();
                    var byteList = new List<byte>();
                    encoding = Encoding.GetEncoding(1200); // 16-bit Unicode
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var l = lines[i];
                        if (i > 0)
                        { // new line
                            byteList.Add(0);
                            byteList.Add(138);
                        }
                        byteList.AddRange(encoding.GetBytes(l).ToArray());
                    }
                    for (int i = 0; i < 112; i++)
                    {
                        if (i < byteList.Count)
                            buffer[16 + i] = byteList[i];
                        else
                            buffer[16 + i] = 0x8f;
                    }
                    return buffer;
                }
                if (header.CharacterCodeTableNumber == "00")
                {
                    encoding = Encoding.GetEncoding(20269);
                    // 0xC1—0xCF combines characters - http://en.wikipedia.org/wiki/ISO/IEC_6937

                    var sbTwoChar = new StringBuilder();
                    bool skipNext = false;
                    for (var index = 0; index < TextField.Length; index++)
                    {
                        var ch = TextField[index];
                        if (skipNext)
                        {
                            skipNext = false;
                        }
                        else if (ch == 'ı' && TextField.Substring(index).StartsWith("ı̂")) // extended unicode char - rewritten as simple 'î' - looks the same as "î" but it's not...)
                        {
                            sbTwoChar.Append(encoding.GetString(new byte[] { 0xc3, 0x69 })); // Ãi - simple î
                                                                                             //                          sbTwoChar.Append(encoding.GetString(new byte[] { 0xc3, 0xf5 }));
                            skipNext = true;
                        }
                        else if ("ÀÈÌÒÙàèìòù".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc1 }), "ÀÈÌÒÙàèìòù", "AEIOUaeiou"));
                        }
                        else if ("ÁĆÉÍĹŃÓŔŚÚÝŹáćéģíĺńóŕśúýź".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc2 }), "ÁĆÉÍĹŃÓŔŚÚÝŹáćéģíĺńóŕśúýź", "ACEILNORSUYZacegilnorsuyz"));
                        }
                        else if ("ÂĈÊĜĤÎĴÔŜÛŴŶâĉêĝĥĵôŝûŵŷîı̂".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc3 }), "ÂĈÊĜĤÎĴÔŜÛŴŶâĉêĝĥîĵôŝûŵŷ", "ACEGHIJOSUWYaceghijosuwy"));
                        }
                        else if ("ÃĨÑÕŨãĩñõũ".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc4 }), "ÃĨÑÕŨãĩñõũ", "AINOUainou"));
                        }
                        else if ("ĀĒĪŌŪāēīōū".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc5 }), "ĀĒĪŌŪāēīōū", "AEIOUaeiou"));
                        }
                        else if ("ĂĞŬăğŭ".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc6 }), "ĂĞŬăğŭ", "AGUagu"));
                        }
                        else if ("ĂĞŬăğŭ".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc6 }), "ĂĞŬăğŭ", "AGUagu"));
                        }
                        else if ("ĊĖĠİŻċėġıż".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc7 }), "ĊĖĠİŻċėġıż", "CEGIZcegiz"));
                        }
                        else if ("ÄËÏÖÜŸäëïöüÿ".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xc8 }), "ÄËÏÖÜŸäëïöüÿ", "AEIOUYaeiouy"));
                        }
                        else if ("ÅŮåů".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xca }), "ÅŮåů", "AUau"));
                        }
                        else if ("ÇĢĶĻŅŖŞŢçķļņŗşţ".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xcb }), "ÇĢĶĻŅŖŞŢçķļņŗşţ", "CGKLNRSTcklnrst"));
                        }
                        else if ("ŐŰőű".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xcd }), "ŐŰőű", "OUou"));
                        }
                        else if ("ĄĘĮŲąęįų".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xce }), "ĄĘĮŲąęįų", "AEIUaeiu"));
                        }
                        else if ("ČĎĚĽŇŘŠŤŽčďěľňřšťž".Contains(ch))
                        {
                            sbTwoChar.Append(ReplaceSpecialCharactersWithTwoByteEncoding(ch, encoding.GetString(new byte[] { 0xcf }), "ČĎĚĽŇŘŠŤŽčďěľňřšťž", "CDELNRSTZcdelnrstz"));
                        }
                        else
                        {
                            sbTwoChar.Append(ch);
                        }
                    }

                    TextField = sbTwoChar.ToString();
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
                if (Utilities.CountTagInText(TextField, "<i>") == 1 && TextField.StartsWith("<i>") && TextField.EndsWith("</i>")) // italic on all lines
                    TextField = TextField.Replace(Environment.NewLine, Environment.NewLine + "<i>");
                TextField = TextField.Replace("<i>", italicsOn);
                TextField = TextField.Replace("<I>", italicsOn);
                TextField = TextField.Replace("</i>", italicsOff);
                TextField = TextField.Replace("</I>", italicsOff);
                TextField = TextField.Replace("<u>", underlineOn);
                TextField = TextField.Replace("<U>", underlineOn);
                TextField = TextField.Replace("</u>", underlineOff);
                TextField = TextField.Replace("</U>", underlineOff);
                if (header.CharacterCodeTableNumber == "00")
                {
                    TextField = TextField.Replace("©", encoding.GetString(new byte[] { 0xd3 }));
                    TextField = TextField.Replace("™", encoding.GetString(new byte[] { 0xd4 }));
                    TextField = TextField.Replace("♪", encoding.GetString(new byte[] { 0xd5 }));
                }

                TextField = EncodeText(TextField, encoding, header.DisplayStandardCode);
                TextField = HtmlUtil.RemoveHtmlTags(TextField, true);

                // save em-dash indexes (–)
                var indexOfEmdash = new List<int>();
                for (int j = 0; j < TextField.Length; j++)
                {
                    if (TextField[j] == '–')
                        indexOfEmdash.Add(j);
                }

                if (header.DisplayStandardCode != "0") // 0=Open subtitling
                {
                    if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                    {
                        TextField = encoding.GetString(new byte[] { 0x0d, 0x0b, 0x0b }) + TextField; // d=double height, b=start box
                    }
                    else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                    {
                        TextField = encoding.GetString(new byte[] { 0x0b, 0x0b }) + TextField; // b=start box
                    }
                    else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                    {
                        TextField = encoding.GetString(new byte[] { 0x0d }) + TextField; // d=double height
                    }
                }

                // convert text to bytes
                byte[] bytes = encoding.GetBytes(TextField);

                // restore em-dashes (–)
                foreach (int index in indexOfEmdash)
                {
                    bytes[index] = 0xd0;
                }

                for (int i = 0; i < 112; i++)
                {
                    if (i < bytes.Length)
                        buffer[16 + i] = bytes[i];
                    else
                        buffer[16 + i] = 0x8f;
                }
                return buffer;
            }

            private static string EncodeText(string text, Encoding encoding, string displayStandardCode)
            {
                // newline
                string newline = encoding.GetString(new byte[] { 0x8a, 0x8a });
                if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                {
                    newline = encoding.GetString(new byte[] { 0x0a, 0x0a, 0x8a, 0x8a, 0x0d, 0x0b, 0x0b }); // 0a==end box, 0d==double height, 0b==start box
                }
                else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                {
                    newline = encoding.GetString(new byte[] { 0x0a, 0x0a, 0x8a, 0x8a, 0x0b, 0x0b }); // 0a==end box, 0b==start box
                }
                else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                {
                    newline = encoding.GetString(new byte[] { 0x8a, 0x8a, 0x0d, 0x0d }); // 0d==double height
                }
                if (displayStandardCode == "0") // 0=Open subtitling
                {
                    newline = encoding.GetString(new byte[] { 0x8A }); //8Ah=CR/LF
                }


                var sb = new StringBuilder();
                var list = text.SplitToLines();
                for (var index = 0; index < list.Count; index++)
                {
                    if (index > 0)
                        sb.Append(newline);

                    string line = list[index];
                    int i = 0;
                    while (i < line.Length)
                    {
                        var newStart = line.Substring(i);
                        if (newStart.StartsWith("<font ", StringComparison.Ordinal))
                        {
                            int end = line.IndexOf('>', i);
                            if (end > 0)
                            {
                                if (displayStandardCode != "0")
                                    sb.Append(GetColor(encoding, line, i));
                                i = end + 1;
                            }
                        }
                        else if (newStart == "</font>")
                        {
                            i += "</font>".Length;
                        }
                        else if (newStart.StartsWith("</font>", StringComparison.Ordinal))
                        {
                            if (displayStandardCode != "0")
                                sb.Append(encoding.GetString(new byte[] { 0x07 })); // white
                            i += "</font>".Length;
                        }
                        else
                        {
                            sb.Append(line.Substring(i, 1));
                            i++;
                        }
                    }
                }

                if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && displayStandardCode != "0")
                {
                    sb.Append(encoding.GetString(new byte[] { 0x0a, 0x0a })); //a=end box
                }

                return sb.ToString();
            }

            private static string GetColor(Encoding encoding, string line, int i)
            {
                int end = line.IndexOf('>', i);
                if (end > 0)
                {
                    string f = line.Substring(i, end - i);
                    if (f.Contains(" color="))
                    {
                        var colorStart = f.IndexOf(" color=", StringComparison.Ordinal);
                        if (line.IndexOf('"', colorStart + " color=".Length + 1) > 0)
                        {
                            int colorEnd = f.IndexOf('"', colorStart + " color=".Length + 1);
                            if (colorStart > 1)
                            {
                                string color = f.Substring(colorStart + 7, colorEnd - (colorStart + 7));
                                color = color.Trim('\'');
                                color = color.Trim('\"');
                                color = color.Trim('#');
                                return GetNearestEbuColorCode(color, encoding);
                            }
                        }
                    }
                }
                return string.Empty;
            }

            private static string GetNearestEbuColorCode(string color, Encoding encoding)
            {
                color = color.ToLower();
                if (color == "black" || color == "000000")
                    return encoding.GetString(new byte[] { 0x00 }); // black
                if (color == "red" || color == "ff0000")
                    return encoding.GetString(new byte[] { 0x01 }); // red
                if (color == "green" || color == "00ff00")
                    return encoding.GetString(new byte[] { 0x02 }); // green
                if (color == "yellow" || color == "ffff00")
                    return encoding.GetString(new byte[] { 0x03 }); // yellow
                if (color == "blue" || color == "0000ff")
                    return encoding.GetString(new byte[] { 0x04 }); // blue
                if (color == "magenta" || color == "ff00ff")
                    return encoding.GetString(new byte[] { 0x05 }); // magenta
                if (color == "cyan" || color == "00ffff")
                    return encoding.GetString(new byte[] { 0x06 }); // cyan
                if (color == "white" || color == "ffffff")
                    return encoding.GetString(new byte[] { 0x07 }); // white
                if (color.Length == 6)
                {
                    if (RegExprColor.IsMatch(color))
                    {
                        const int maxDiff = 130;
                        int r = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                        int g = int.Parse(color.Substring(2, 2), NumberStyles.HexNumber);
                        int b = int.Parse(color.Substring(4, 2), NumberStyles.HexNumber);
                        if (r < maxDiff && g < maxDiff && b < maxDiff)
                            return encoding.GetString(new byte[] { 0x00 }); // black
                        if (r > 255 - maxDiff && g < maxDiff && b < maxDiff)
                            return encoding.GetString(new byte[] { 0x01 }); // red
                        if (r < maxDiff && g > 255 - maxDiff && b < maxDiff)
                            return encoding.GetString(new byte[] { 0x02 }); // green
                        if (r > 255 - maxDiff && g > 255 - maxDiff && b < maxDiff)
                            return encoding.GetString(new byte[] { 0x03 }); // yellow
                        if (r < maxDiff && g < maxDiff && b > 255 - maxDiff)
                            return encoding.GetString(new byte[] { 0x04 }); // blue
                        if (r > 255 - maxDiff && g < maxDiff && b > 255 - maxDiff)
                            return encoding.GetString(new byte[] { 0x05 }); // magenta
                        if (r < maxDiff && g > 255 - maxDiff && b > 255 - maxDiff)
                            return encoding.GetString(new byte[] { 0x06 }); // cyan
                        if (r > 255 - maxDiff && g > 255 - maxDiff && b > 255 - maxDiff)
                            return encoding.GetString(new byte[] { 0x07 }); // white
                    }
                }
                return string.Empty;
            }

            private static string ReplaceSpecialCharactersWithTwoByteEncoding(char ch, string specialCharacter, string originalCharacters, string newCharacters)
            {
                if (originalCharacters.Length != newCharacters.Length)
                    throw new ArgumentException("originalCharacters and newCharacters must have equal length");

                for (int i = 0; i < newCharacters.Length; i++)
                {
                    if (originalCharacters[i] == ch)
                        return specialCharacter + newCharacters[i];

                }
                return ch.ToString();
            }

            public static byte GetFrameFromMilliseconds(int milliseconds, double frameRate)
            {
                return (byte)Math.Round(milliseconds / (TimeCode.BaseUnit / frameRate));
            }
        }

        public override string Extension => ".stl";

        public const string NameOfFormat = "EBU STL";

        public override string Name => NameOfFormat;

        public bool Save(string fileName, Subtitle subtitle)
        {
            return Save(fileName, subtitle, false);
        }

        public bool Save(string fileName, Subtitle subtitle, bool batchMode, EbuGeneralSubtitleInformation header = null)
        {
            using (var ms = new MemoryStream())
            {
                var ok = Save(fileName, ms, subtitle, batchMode, header);
                if (ok)
                {
                    ms.Position = 0;
                    using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        ms.CopyTo(fs);
                    }
                }
                return ok;
            }
        }

        public bool Save(string fileName, Stream stream, Subtitle subtitle, bool batchMode, EbuGeneralSubtitleInformation header)
        {
            if (header == null)
                header = new EbuGeneralSubtitleInformation();

            if (EbuUiHelper == null)
                return false;

            if (subtitle.Header != null && subtitle.Header.Length == 1024 && (subtitle.Header.Contains("STL24") || subtitle.Header.Contains("STL25") || subtitle.Header.Contains("STL29") || subtitle.Header.Contains("STL30")))
            {
                header = ReadHeader(Encoding.UTF8.GetBytes(subtitle.Header));
                EbuUiHelper.Initialize(header, EbuUiHelper.JustificationCode, null, subtitle);
            }
            else
            {
                EbuUiHelper.Initialize(header, EbuUiHelper.JustificationCode, fileName, subtitle);
            }

            if (!batchMode && !EbuUiHelper.ShowDialogOk())
                return false;

            header.TotalNumberOfSubtitles = subtitle.Paragraphs.Count.ToString("D5"); // seems to be 1 higher than actual number of subtitles
            header.TotalNumberOfTextAndTimingInformationBlocks = header.TotalNumberOfSubtitles;

            var today = $"{DateTime.Now:yyMMdd}";
            if (today.Length == 6)
            {
                header.CreationDate = today;
                header.RevisionDate = today;
            }

            var firstParagraph = subtitle.GetParagraphOrDefault(0);
            if (firstParagraph != null)
            {
                var tc = firstParagraph.StartTime;
                string firstTimeCode = $"{tc.Hours:00}{tc.Minutes:00}{tc.Seconds:00}{EbuTextTimingInformation.GetFrameFromMilliseconds(tc.Milliseconds, header.FrameRate):00}";
                if (firstTimeCode.Length == 8)
                    header.TimeCodeFirstInCue = firstTimeCode;
            }

            byte[] buffer = Encoding.Default.GetBytes(header.ToString());
            stream.Write(buffer, 0, buffer.Length);

            int subtitleNumber = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                var tti = new EbuTextTimingInformation();

                int rows;
                if (!int.TryParse(header.MaximumNumberOfDisplayableRows, out rows))
                    rows = 23;

                if (header.DisplayStandardCode == "1" || header.DisplayStandardCode == "2") // teletext
                    rows = 23;
                else if (header.DisplayStandardCode == "0" && header.MaximumNumberOfDisplayableRows == "02") // open subtitling
                    rows = 15;

                var text = p.Text.Trim(Utilities.NewLineChars);
                if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    tti.VerticalPosition = (byte)(1 + Configuration.Settings.SubtitleSettings.EbuStlMarginTop); // top (vertical)
                }
                else if (text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal))
                {
                    tti.VerticalPosition = (byte)(rows / 2); // middle (vertical)
                }
                else
                {
                    int startRow = rows - Configuration.Settings.SubtitleSettings.EbuStlMarginBottom -
                                          (Utilities.GetNumberOfLines(text) - 1) * Configuration.Settings.SubtitleSettings.EbuStlNewLineRows;
                    if (startRow < 0)
                        startRow = 0;
                    tti.VerticalPosition = (byte)startRow; // bottom (vertical)
                }

                tti.JustificationCode = EbuUiHelper.JustificationCode; // use default justification
                if (text.StartsWith("{\\an1}", StringComparison.Ordinal) || text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an7}", StringComparison.Ordinal))
                {
                    tti.JustificationCode = 1; // 01h=left-justified text
                }
                else if (text.StartsWith("{\\an3}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    tti.JustificationCode = 3; // 03h=right-justified
                }
                else if (text.StartsWith("{\\an2}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal))
                {
                    tti.JustificationCode = 2; // 02h=centred text
                }

                // replace some unsupported characters
                text = text.Replace("„", "\""); // lower quote
                text = text.Replace("‚", "’"); // lower apostrophe
                text = text.Replace("♫", "♪"); // only music single note supported

                tti.SubtitleNumber = (ushort)subtitleNumber;
                tti.TextField = text;
                int startTag = tti.TextField.IndexOf('}');
                if (tti.TextField.StartsWith("{\\", StringComparison.Ordinal) && startTag > 0 && startTag < 10)
                {
                    tti.TextField = tti.TextField.Remove(0, startTag + 1);
                }

                tti.TimeCodeInHours = p.StartTime.Hours;
                tti.TimeCodeInMinutes = p.StartTime.Minutes;
                tti.TimeCodeInSeconds = p.StartTime.Seconds;
                tti.TimeCodeInMilliseconds = p.StartTime.Milliseconds;
                tti.TimeCodeOutHours = p.EndTime.Hours;
                tti.TimeCodeOutMinutes = p.EndTime.Minutes;
                tti.TimeCodeOutSeconds = p.EndTime.Seconds;
                tti.TimeCodeOutMilliseconds = p.EndTime.Milliseconds;
                buffer = tti.GetBytes(header);
                stream.Write(buffer, 0, buffer.Length);
                subtitleNumber++;
            }
            return true;
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 1024 + 128 && fi.Length < 1024000) // not too small or too big
                {
                    try
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);
                        EbuGeneralSubtitleInformation header = ReadHeader(buffer);
                        if (header.DiskFormatCode.StartsWith("STL23", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL24", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL25", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL29", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL30", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL35", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL48", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL50", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL60", StringComparison.Ordinal) ||
                            ("012 ".Contains(header.DisplayStandardCode) && "437|850|860|863|865".Contains(header.CodePageNumber)))
                        {
                            return Utilities.IsInteger(header.CodePageNumber) || fileName.EndsWith(".stl", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        public void LoadSubtitle(Subtitle subtitle, byte[] buffer)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            EbuGeneralSubtitleInformation header = ReadHeader(buffer);
            subtitle.Header = Encoding.UTF8.GetString(buffer);
            Paragraph last = null;
            byte lastExtensionBlockNumber = 0xff;
            JustificationCodes = new List<int>();
            VerticalPositions = new List<int>();
            Configuration.Settings.General.CurrentFrameRate = header.FrameRate;
            foreach (EbuTextTimingInformation tti in ReadTextAndTiming(buffer, header))
            {
                if (tti.ExtensionBlockNumber != 0xfe) // FEh : Reserved for User Data
                {
                    var p = new Paragraph
                    {
                        Text = tti.TextField,
                        StartTime = new TimeCode(tti.TimeCodeInHours, tti.TimeCodeInMinutes, tti.TimeCodeInSeconds, tti.TimeCodeInMilliseconds),
                        EndTime = new TimeCode(tti.TimeCodeOutHours, tti.TimeCodeOutMinutes, tti.TimeCodeOutSeconds, tti.TimeCodeOutMilliseconds),
                        MarginV = tti.VerticalPosition.ToString(CultureInfo.InvariantCulture)
                    };

                    if (lastExtensionBlockNumber != 0xff && last != null)
                    {
                        last.Text += p.Text; // merge text
                    }
                    else
                    {
                        subtitle.Paragraphs.Add(p);
                        last = p;
                    }
                    p.Text = HtmlUtil.FixInvalidItalicTags(p.Text);
                    lastExtensionBlockNumber = tti.ExtensionBlockNumber;
                }
            }
            subtitle.Renumber();
            Header = header;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            LoadSubtitle(subtitle, FileUtil.ReadAllBytesShared(fileName));
        }

        public static EbuGeneralSubtitleInformation ReadHeader(byte[] buffer)
        {
            var header = new EbuGeneralSubtitleInformation
            {
                CodePageNumber = Encoding.Default.GetString(buffer, 0, 3),
                DiskFormatCode = Encoding.Default.GetString(buffer, 3, 8),
                DisplayStandardCode = Encoding.Default.GetString(buffer, 11, 1),
                CharacterCodeTableNumber = Encoding.Default.GetString(buffer, 12, 2),
                LanguageCode = Encoding.Default.GetString(buffer, 14, 2),
                OriginalProgrammeTitle = Encoding.Default.GetString(buffer, 16, 32),
                OriginalEpisodeTitle = Encoding.Default.GetString(buffer, 48, 32),
                TranslatedProgrammeTitle = Encoding.Default.GetString(buffer, 80, 32),
                TranslatedEpisodeTitle = Encoding.Default.GetString(buffer, 112, 32),
                TranslatorsName = Encoding.Default.GetString(buffer, 144, 32),
                TranslatorsContactDetails = Encoding.Default.GetString(buffer, 176, 32),
                SubtitleListReferenceCode = Encoding.Default.GetString(buffer, 208, 16),
                CreationDate = Encoding.Default.GetString(buffer, 224, 6),
                RevisionDate = Encoding.Default.GetString(buffer, 230, 6),
                RevisionNumber = Encoding.Default.GetString(buffer, 236, 2),
                TotalNumberOfTextAndTimingInformationBlocks = Encoding.Default.GetString(buffer, 238, 5),
                TotalNumberOfSubtitles = Encoding.Default.GetString(buffer, 243, 5),
                TotalNumberOfSubtitleGroups = Encoding.Default.GetString(buffer, 248, 3),
                MaximumNumberOfDisplayableCharactersInAnyTextRow = Encoding.Default.GetString(buffer, 251, 2),
                MaximumNumberOfDisplayableRows = Encoding.Default.GetString(buffer, 253, 2),
                TimeCodeStatus = Encoding.Default.GetString(buffer, 255, 1),
                TimeCodeStartOfProgramme = Encoding.Default.GetString(buffer, 256, 8),
                CountryOfOrigin = Encoding.Default.GetString(buffer, 274, 3),
                SpareBytes = Encoding.Default.GetString(buffer, 373, 75),
                UserDefinedArea = Encoding.Default.GetString(buffer, 448, 576)
            };
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
        private static string GetCharacter(out bool skipNext, EbuGeneralSubtitleInformation header, byte[] buffer, int index)
        {
            skipNext = false;

            if (header.LanguageCode == LanguageCodeChinese)
            {
                skipNext = true;
                return Encoding.GetEncoding(1200).GetString(buffer, index, 2); // 16-bit Unicode
            }

            if (header.CharacterCodeTableNumber == "00")
            {
                var b = buffer[index];
                if (b == 0xd3)
                    return "©";
                if (b == 0xd4)
                    return "™";
                if (b == 0xd5)
                    return "♪";


                //note that 0xC1—0xCF combines characters - http://en.wikipedia.org/wiki/ISO/IEC_6937
                var encoding = Encoding.GetEncoding(20269);
                if (index + 2 > buffer.Length)
                    return string.Empty;
                string next = encoding.GetString(buffer, index + 1, 1);
                switch (b)
                {
                    case 0xc1: // Grave
                        skipNext = @"AEIOUaeiou".Contains(next);
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
                        skipNext = @"ACEILNORSUYZacegilnorsuyz".Contains(next);
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
                        skipNext = @"ACEGHIJOSUWYaceghjosuwyıi".Contains(next);
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
                            case "j": return "ĵ";
                            case "o": return "ô";
                            case "s": return "ŝ";
                            case "u": return "û";
                            case "w": return "ŵ";
                            case "y": return "ŷ";
                            case "ı": return "ı̂";
                            case "i": return "î";
                        }
                        return string.Empty;
                    case 0xc4: // Tilde
                        skipNext = @"AINOUainou".Contains(next);
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
                        skipNext = @"AEIOUaeiou".Contains(next);
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
                        skipNext = @"AGUagu".Contains(next);
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
                        skipNext = @"CEGIZcegiz".Contains(next);
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
                        skipNext = @"AEIOUYaeiouy".Contains(next);
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
                        skipNext = @"AUau".Contains(next);
                        switch (next)
                        {
                            case "A": return "Å";
                            case "U": return "Ů";
                            case "a": return "å";
                            case "u": return "ů";
                        }
                        return string.Empty;
                    case 0xcb: // Cedilla
                        skipNext = @"CGKLNRSTcklnrst".Contains(next);
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
                        skipNext = @"OUou".Contains(next);
                        switch (next)
                        {
                            case "O": return "Ő";
                            case "U": return "Ű";
                            case "o": return "ő";
                            case "u": return "ű";
                        }
                        return string.Empty;
                    case 0xce: // Ogonek
                        skipNext = @"AEIUaeiu".Contains(next);
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
                        skipNext = @"CDELNRSTZcdelnrstz".Contains(next);
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
            if (header.CharacterCodeTableNumber == "01") // Latin/Cyrillic alphabet - from ISO 8859/5-1988
            {
                return Encoding.GetEncoding("ISO-8859-5").GetString(buffer, index, 1);
            }
            if (header.CharacterCodeTableNumber == "02") // Latin/Arabic alphabet - from ISO 8859/6-1987
            {
                return Encoding.GetEncoding("ISO-8859-6").GetString(buffer, index, 1);
            }
            if (header.CharacterCodeTableNumber == "03") // Latin/Greek alphabet - from ISO 8859/7-1987
            {
                return Encoding.GetEncoding("ISO-8859-7").GetString(buffer, index, 1); // or ISO-8859-1 ?
            }
            if (header.CharacterCodeTableNumber == "04") // Latin/Hebrew alphabet - from ISO 8859/8-1988
            {
                return Encoding.GetEncoding("ISO-8859-8").GetString(buffer, index, 1);
            }

            return string.Empty;
        }

        /// <summary>
        /// Read Text and Timing Information (TTI) block.
        /// Each Text and Timing Information (TTI) block consists of 128 bytes. 
        /// </summary>
        private IEnumerable<EbuTextTimingInformation> ReadTextAndTiming(byte[] buffer, EbuGeneralSubtitleInformation header)
        {
            const int startOfTextAndTimingBlock = 1024;
            const int ttiSize = 128;
            const byte italicsOn = 0x80;
            const byte italicsOff = 0x81;
            const byte underlineOn = 0x82;
            const byte underlineOff = 0x83;

            var list = new List<EbuTextTimingInformation>();
            int index = startOfTextAndTimingBlock;
            while (index + ttiSize <= buffer.Length)
            {
                var tti = new EbuTextTimingInformation
                {
                    SubtitleGroupNumber = buffer[index],
                    SubtitleNumber = (ushort)(buffer[index + 2] * 256 + buffer[index + 1]),
                    ExtensionBlockNumber = buffer[index + 3],
                    CumulativeStatus = buffer[index + 4],
                    TimeCodeInHours = buffer[index + 5 + 0],
                    TimeCodeInMinutes = buffer[index + 5 + 1],
                    TimeCodeInSeconds = buffer[index + 5 + 2],
                    TimeCodeInMilliseconds = FramesToMillisecondsMax999(buffer[index + 5 + 3]),
                    TimeCodeOutHours = buffer[index + 9 + 0],
                    TimeCodeOutMinutes = buffer[index + 9 + 1],
                    TimeCodeOutSeconds = buffer[index + 9 + 2],
                    TimeCodeOutMilliseconds = FramesToMillisecondsMax999(buffer[index + 9 + 3]),
                    VerticalPosition = buffer[index + 13],
                    JustificationCode = buffer[index + 14],
                    CommentFlag = buffer[index + 15]
                };
                VerticalPositions.Add(tti.VerticalPosition);
                JustificationCodes.Add(tti.JustificationCode);

                // Text block
                // - has a fixed length of 112 byte
                // - 8Ah = new line
                // - unused space = 8Fh
                int i = index + 16; // text block start at 17th byte (index 16)
                var open = header.DisplayStandardCode != "1" && header.DisplayStandardCode != "2";
                var closed = header.DisplayStandardCode != "0";
                int max = index + 112;
                var sb = new StringBuilder();
                while (i < max)
                {
                    var b = buffer[i];
                    if (b <= 0x1f) // Closed - Teletext control codes
                    {
                        if (closed)
                        {
                            var tag = GetColorOrTag(b);
                            if (!string.IsNullOrEmpty(tag))
                                sb.Append(tag);
                        }
                    }
                    else if (b >= 0x20 && b <= 0x7f) // Both - Character codes
                    {
                        bool skipNext;
                        var ch = GetCharacter(out skipNext, header, buffer, i);
                        sb.Append(ch);
                        if (skipNext)
                            i++;
                    }
                    else if (b >= 0x80 && b <= 0x85) // Open - italic/underline/boxing
                    {
                        if (open)
                        {
                            if (b == italicsOn && header.LanguageCode != LanguageCodeChinese)
                                sb.Append("<i>");
                            else if (b == italicsOff && header.LanguageCode != LanguageCodeChinese)
                                sb.Append("</i>");
                            else if (b == underlineOn && header.LanguageCode != LanguageCodeChinese)
                                sb.Append("<u>");
                            else if (b == underlineOff && header.LanguageCode != LanguageCodeChinese)
                                sb.Append("</u>");
                        }
                    }
                    else if (b >= 0x86 && b <= 0x89) // Both - Reserved for future use
                    {
                    }
                    else if (b == 0x8a) // Both - CR/LF
                    {
                        sb.AppendLine();
                    }
                    else if (b >= 0x8b && b <= 0x8e) // Both - Reserved for future use
                    {
                    }
                    else if (b == 0x8f) // Both - unused space
                    {
                    }
                    else if (b >= 0x90 && b <= 0x9f) // Both - Reserved for future use
                    {
                    }
                    else if (b >= 0xa1 && b <= 0xff) // Both - Character codes
                    {
                        bool skipNext;
                        var ch = GetCharacter(out skipNext, header, buffer, i);
                        sb.Append(ch);
                        if (skipNext)
                            i++;
                    }
                    i++;
                }
                tti.TextField = FixSpacesAndTags(sb.ToString());

                int rows;
                if (!int.TryParse(header.MaximumNumberOfDisplayableRows, out rows))
                    rows = 23;
                if (tti.VerticalPosition < 3)
                {
                    if (tti.JustificationCode == 1) // left
                        tti.TextField = "{\\an7}" + tti.TextField;
                    else if (tti.JustificationCode == 3) // right
                        tti.TextField = "{\\an9}" + tti.TextField;
                    else
                        tti.TextField = "{\\an8}" + tti.TextField;
                }
                else if (tti.VerticalPosition <= rows / 2 + 1)
                {
                    if (tti.JustificationCode == 1) // left
                        tti.TextField = "{\\an4}" + tti.TextField;
                    else if (tti.JustificationCode == 3) // right
                        tti.TextField = "{\\an6}" + tti.TextField;
                    else
                        tti.TextField = "{\\an5}" + tti.TextField;
                }
                else
                {
                    if (tti.JustificationCode == 1) // left
                        tti.TextField = "{\\an1}" + tti.TextField;
                    else if (tti.JustificationCode == 3) // right
                        tti.TextField = "{\\an3}" + tti.TextField;
                }
                index += ttiSize;
                list.Add(tti);
            }
            return list;
        }

        private string GetColorOrTag(byte b)
        {
            switch (b)
            {
                case 0x00:
                    return "<font color=\"Black\">";
                case 0x01:
                    return "<font color=\"Red\">";
                case 0x02:
                    return "<font color=\"Green\">";
                case 0x03:
                    return "<font color=\"Yellow\">";
                case 0x04:
                    return "<font color=\"Blue\">";
                case 0x05:
                    return "<font color=\"Magenta\">";
                case 0x06:
                    return "<font color=\"Cyan\">";
                case 0x07:
                    return "<font color=\"White\">";
                    //case 0x0a:
                    //    return "</box>";
                    //case 0x0b:
                    //    return "<box>";
            }
            return null;
        }

        private static string FixSpacesAndTags(string text)
        {
            text = text.Trim();
            while (text.Contains("  </font>"))
                text = text.Replace("  </font>", " </font>");

            var match = FontTagsNoSpace1.Match(text);
            while (match.Success)
            {
                text = text.Remove(match.Index, match.Length).Insert(match.Index, match.Value.Replace("</font><font", "</font> <font"));
                match = FontTagsNoSpace1.Match(text);
            }

            match = FontTagsNoSpace2.Match(text);
            while (match.Success)
            {
                text = text.Remove(match.Index, match.Length).Insert(match.Index, match.Value.Replace("<font", " <font"));
                match = FontTagsNoSpace2.Match(text);
            }

            if (!text.Replace("<font color=\"White\">", string.Empty).Contains("<font "))
            {
                text = text.Replace("<font color=\"White\">", string.Empty);
            }

            while (text.Contains(Environment.NewLine + Environment.NewLine))
                text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            var lines = text.SplitToLines();

            // fix multi font tags, e.g. a color in the middle of a line
            for (var index = 0; index < lines.Count; index++)
            {
                var whiteTag = "<font color=\"White\">";
                var line = lines[index];
                bool changed = false;
                var count = Utilities.CountTagInText(line, "<font ");
                if (count > 1)
                {
                    count = 0;
                    int endTags = 0;
                    var idx = line.IndexOf("<font ", StringComparison.Ordinal);
                    while (idx > 0)
                    {
                        count++;
                        var start = line.Substring(idx);
                        if (count == 1 && start.StartsWith(whiteTag))
                        {
                            line = line.Remove(idx, whiteTag.Length);
                            idx--;
                            changed = true;
                            lines[index] = line;
                        }
                        else if (count > 1 && start.StartsWith(whiteTag))
                        {
                            line = line.Remove(idx, whiteTag.Length).Insert(idx, "</font>");
                            changed = true;
                            lines[index] = line;
                            endTags++;
                            count--;
                        }
                        else if (count > 1 && count > endTags + 1 && !start.StartsWith(whiteTag))
                        {
                            line = line.Insert(idx, "</font>");
                            changed = true;
                            lines[index] = line;
                            idx += "</font>".Length;
                            endTags++;
                        }
                        idx = line.IndexOf("<font ", idx + 1, StringComparison.Ordinal);
                    }
                    if (changed)
                    {
                        text = string.Join(Environment.NewLine, lines);
                        lines = text.SplitToLines();
                    }
                }
            }

            if (lines.Count == 1)
            {
                var count = Utilities.CountTagInText(text, "<font ");
                if (count == 1 && !text.Contains("</font>"))
                    text += "</font>";
            }
            else if (lines.Count == 2)
            {
                var count1 = Utilities.CountTagInText(lines[0], "<font ");
                var count2 = Utilities.CountTagInText(lines[1], "<font ");
                if (count1 == 1 && !lines[0].Contains("</font>"))
                    lines[0] += "</font>";
                if (count2 == 1 && !lines[1].Contains("</font>"))
                    lines[1] += "</font>";
                text = lines[0] + Environment.NewLine + lines[1];
            }

            while (text.Contains(Environment.NewLine + " "))
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

            // remove starting white spaces
            match = FontTagsStartSpace.Match(text);
            while (match.Success)
            {
                text = text.Remove(match.Index + match.Length - 1, 1);
                match = FontTagsStartSpace.Match(text);
            }

            // remove starting white spaces on 2+ line
            match = FontTagsNewLineSpace.Match(text);
            while (match.Success)
            {
                text = text.Remove(match.Index + match.Length - 1, 1);
                match = FontTagsNewLineSpace.Match(text);
            }

            text = text.Replace(" </font>", "</font> ");

            text = HtmlUtil.FixInvalidItalicTags(text);

            return text;
        }

        public override bool IsTextBased => false;

        public bool Save(string fileName, Stream stream, Subtitle subtitle, bool batchMode)
        {
            return Save(fileName, stream, subtitle, batchMode, null);
        }

    }
}
