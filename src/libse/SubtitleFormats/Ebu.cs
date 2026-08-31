using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// EBU Subtitling data exchange format.
    /// </summary>
    public class Ebu : SubtitleFormat, IBinaryPersistableSubtitle
    {
        public static double OverrideReadFrameRate { get; set; }

        private static readonly Regex FontTagsNoSpace1 = new Regex("[a-zA-z.!?]</font><font[a-zA-Z =\"']+>[a-zA-Z-]", RegexOptions.Compiled);
        private static readonly Regex FontTagsNoSpace2 = new Regex("[a-zA-z.!?]<font[a-zA-Z =\"']+>[a-zA-Z-]", RegexOptions.Compiled);

        // "<font color=\"Blue\"></font>" - two teletext color codes in a row, e.g. a background
        // color set right before the text color. Only the last one has any text, and SE has no
        // background color for STL, so the empty tag is noise that shifts the text by a space.
        private static readonly Regex EmptyFontTag = new Regex("<font color=\"[A-Za-z]+\"></font>", RegexOptions.Compiled);

        private static readonly Regex FontTagsStartSpace = new Regex("^<font color=\"[A-Za-z]+\"> ", RegexOptions.Compiled); // "<font color=\"Black\"> "
        private static readonly Regex FontTagsNewLineSpace = new Regex("[\r\n]+<font color=\"[A-Za-z]+\"> ", RegexOptions.Compiled); // "\r\n<font color=\"Black\"> "

        private const string LanguageCodeChinese = "75";

        private static readonly Dictionary<int, string> SpecialAsciiCodes = new Dictionary<int, string>
        {
            { 0xd3, "©" },
            { 0xd4, "™" },
            { 0xd5, "♪" },

            { 0xe0, "Ω" },
            { 0xe1, "Æ" },
            { 0xe2, "Ð" },
            { 0xe3, "ª" },
            { 0xe4, "Ħ" },

            { 0xe6, "Ĳ" },
            { 0xe7, "Ŀ" },
            { 0xe8, "Ł" },
            { 0xe9, "Ø" },
            { 0xea, "Œ" },
            { 0xeb, "º" },
            { 0xec, "Þ" },
            { 0xed, "Ŧ" },
            { 0xee, "Ŋ" },
            { 0xef, "ŉ" },

            { 0xf0, "ĸ" },
            { 0xf1, "æ" },
            { 0xf2, "đ" },
            { 0xf3, "ð" },
            { 0xf4, "ħ" },
            { 0xf5, "ı" },
            { 0xf6, "ĳ" },
            { 0xf7, "ŀ" },
            { 0xf8, "ł" },
            { 0xf9, "ø" },
            { 0xfa, "œ" },
            { 0xfb, "ß" },
            { 0xfc, "þ" },
            { 0xfd, "ŧ" },
            { 0xfe, "ŋ" },
        };

        public interface IEbuUiHelper
        {
            void Initialize(EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle);
            bool ShowDialogOk();
            byte JustificationCode { get; set; }
        }

        public static IEbuUiHelper EbuUiHelper { get; set; }

        private static readonly Regex RegExprColor = new Regex(@"^[a-f0-9]{6}$", RegexOptions.Compiled);

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
                    {
                        return FrameRateFromSaveDialog;
                    }

                    if (DiskFormatCode.StartsWith("STL23", StringComparison.Ordinal))
                    {
                        return 23.0;
                    }

                    if (DiskFormatCode.StartsWith("STL24", StringComparison.Ordinal))
                    {
                        return 24.0;
                    }

                    if (DiskFormatCode.StartsWith("STL25", StringComparison.Ordinal))
                    {
                        return 25.0;
                    }

                    if (DiskFormatCode.StartsWith("STL29", StringComparison.Ordinal))
                    {
                        return 29.0;
                    }

                    if (DiskFormatCode.StartsWith("STL35", StringComparison.Ordinal))
                    {
                        return 35.0;
                    }

                    if (DiskFormatCode.StartsWith("STL48", StringComparison.Ordinal))
                    {
                        return 48.0;
                    }

                    if (DiskFormatCode.StartsWith("STL50", StringComparison.Ordinal))
                    {
                        return 50.0;
                    }

                    if (DiskFormatCode.StartsWith("STL60", StringComparison.Ordinal))
                    {
                        return 60.0;
                    }

                    return 30.0; // should be DiskFormatCode STL30.01
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
                var result = CodePageNumber +
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
                {
                    return result;
                }

                return "Length must be 1024 but is " + result.Length;
            }
        }

        /// <summary>
        /// TTI block 128 bytes
        /// </summary>
        private class EbuTextTimingInformation
        {
            public byte SubtitleGroupNumber { get; set; }
            public ushort SubtitleNumber { get; set; }
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

            /// <summary>
            /// One extension block carrying the 112 text bytes starting at <paramref name="offset"/>.
            /// Callers emit as many as the overflow needs - a single block capped the whole
            /// subtitle at 224 bytes and silently discarded the rest.
            /// </summary>
            public byte[] GetBytesExtra(EbuGeneralSubtitleInformation header, byte[] extraBytes, int offset)
            {
                var buffer = SaveHeader(header);
                for (var i = 0; i < 112; i++)
                {
                    var index = offset + i;
                    buffer[16 + i] = index < extraBytes.Length ? extraBytes[index] : (byte)0x8f;
                }

                return buffer;
            }

            public byte[] GetBytes(EbuGeneralSubtitleInformation header, MemoryStream extra)
            {
                var buffer = SaveHeader(header);

                var encoding = GetEncoding(header.CodePageNumber);
                if (header.LanguageCode == LanguageCodeChinese)
                {
                    var lines = HtmlUtil.RemoveHtmlTags(TextField, true).SplitToLines();
                    var byteList = new List<byte>();
                    encoding = Encoding.GetEncoding(1200); // 16-bit Unicode
                    for (var i = 0; i < lines.Count; i++)
                    {
                        var l = lines[i];
                        if (i > 0)
                        { // new line
                            byteList.Add(0);
                            byteList.Add(138);
                        }
                        byteList.AddRange(encoding.GetBytes(l).ToArray());
                    }

                    for (var i = 0; i < 112; i++)
                    {
                        if (i < byteList.Count)
                        {
                            buffer[16 + i] = byteList[i];
                        }
                        else
                        {
                            buffer[16 + i] = 0x8f;
                        }
                    }

                    if (byteList.Count > 112)
                    {
                        extra.Write(byteList.ToArray(), 112, byteList.Count - 112);
                    }

                    return buffer;
                }

                if (header.CharacterCodeTableNumber == "00")
                {
                    // 0xC1—0xCF combines characters - http://en.wikipedia.org/wiki/ISO/IEC_6937
                    try
                    {
                        encoding = Encoding.GetEncoding(20269);
                    }
                    catch
                    {
                        encoding = Encoding.ASCII;
                    }
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

                var textBytes = new List<byte>();
                if (header.DisplayStandardCode != "0") // 0=Open subtitling
                {
                    if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                    {
                        textBytes.AddRange(new byte[] { 0x0d, 0x0b, 0x0b }); // d=double height, b=start box
                    }
                    else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                    {
                        textBytes.AddRange(new byte[] { 0x0b, 0x0b }); // b=start box
                    }
                    else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                    {
                        textBytes.AddRange(new byte[] { 0x0d }); // d=double height
                    }
                }
                EncodeText(textBytes, TextField, encoding, header.DisplayStandardCode, header.CharacterCodeTableNumber);

                var bytes = textBytes.ToArray();

                for (var i = 0; i < 112; i++)
                {
                    if (i < bytes.Length)
                    {
                        buffer[16 + i] = bytes[i];
                    }
                    else
                    {
                        buffer[16 + i] = 0x8f;
                    }
                }

                if (bytes.Length > 112)
                {
                    extra.Write(bytes, 112, bytes.Length - 112);
                }

                return buffer;
            }

            private byte[] SaveHeader(EbuGeneralSubtitleInformation header)
            {
                var buffer = new byte[128]; // Text and Timing Information (TTI) block consists of 128 bytes

                buffer[0] = SubtitleGroupNumber;
                var temp = BitConverter.GetBytes(SubtitleNumber);
                buffer[1] = temp[0];
                buffer[2] = temp[1];
                buffer[3] = ExtensionBlockNumber;
                buffer[4] = CumulativeStatus;

                var frames = GetFrameFromMilliseconds(TimeCodeInMilliseconds, header.FrameRate, out var extraSeconds);
                var tc = new TimeCode(TimeCodeInHours, TimeCodeInMinutes, TimeCodeInSeconds + extraSeconds, 0);
                buffer[5] = (byte)tc.Hours;
                buffer[6] = (byte)tc.Minutes;
                buffer[7] = (byte)tc.Seconds;
                buffer[8] = frames;

                frames = GetFrameFromMilliseconds(TimeCodeOutMilliseconds, header.FrameRate, out extraSeconds);
                tc = new TimeCode(TimeCodeOutHours, TimeCodeOutMinutes, TimeCodeOutSeconds + extraSeconds, 0);
                buffer[9] = (byte)tc.Hours;
                buffer[10] = (byte)tc.Minutes;
                buffer[11] = (byte)tc.Seconds;
                buffer[12] = frames;

                buffer[13] = VerticalPosition;
                buffer[14] = JustificationCode;
                buffer[15] = CommentFlag;
                return buffer;
            }

            private static string FixItalics(string text)
            {
                var italicOn = false;
                var sb = new StringBuilder();
                foreach (var line in HtmlUtil.FixInvalidItalicTags(text).SplitToLines())
                {
                    var s = line;
                    if (italicOn && !s.TrimStart().StartsWith("<i>", StringComparison.Ordinal))
                    {
                        s = "<i>" + s;
                    }

                    var endTagIndex = s.LastIndexOf("</i>", StringComparison.Ordinal);
                    if (s.LastIndexOf("<i>", StringComparison.Ordinal) > endTagIndex)
                    {
                        italicOn = true;
                    }
                    else if (endTagIndex >= 0)
                    {
                        italicOn = false;
                    }

                    if (italicOn)
                    {
                        sb.AppendLine(s + "</i>");
                    }
                    else
                    {
                        sb.AppendLine(s);
                    }
                }

                return sb.ToString().TrimEnd();
            }

            //TODO: Use bytes directly and not encoding
            private static void EncodeText(List<byte> textBytes, string input, Encoding encoding, string displayStandardCode, string characterCodeTableNumber)
            {
                // italic/underline
                var italicOn = (byte)0x80;
                var italicOff = (byte)0x81;
                var underlineOn = (byte)0x82;
                var underlineOff = (byte)0x83;
                var boxingOn = (byte)0x84;
                var boxingOff = (byte)0x85;

                // newline
                var newline = new byte[] { 0x8a, 0x8a };
                if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                {
                    newline = new byte[] { 0x0a, 0x0a, 0x8a, 0x8a, 0x0d, 0x0b, 0x0b }; // 0a==end box, 0d==double height, 0b==start box
                }
                else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox)
                {
                    var temp = new List<byte>
                    {
                        0x0a, // 0a==end box, 
                        0x0a
                    };
                    for (var i = 0; i < Configuration.Settings.SubtitleSettings.EbuStlNewLineRows; i++)
                    {
                        temp.Add(0x8a);
                    }
                    temp.Add(0x0b); // 0b==start box
                    temp.Add(0x0b);
                    newline = temp.ToArray();
                }
                else if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight)
                {
                    newline = new byte[] { 0x8a, 0x8a, 0x0d, 0x0d }; // 0d==double height
                }

                if (displayStandardCode == "0") // 0=Open subtitling
                {
                    newline = new byte[] { 0x8A }; //8Ah=CR/LF
                }

                byte? lastColor = null;
                var sb = new StringBuilder();

                // remove tags except "font", "italic", "underline" and "box"
                var startFont = Guid.NewGuid().ToString();
                var endFont = Guid.NewGuid().ToString();
                var startItalic = Guid.NewGuid().ToString();
                var endItalic = Guid.NewGuid().ToString();
                var startUnderline = Guid.NewGuid().ToString();
                var endUnderline = Guid.NewGuid().ToString();
                var startBox = Guid.NewGuid().ToString();
                var endBox = Guid.NewGuid().ToString();
                var text = FixItalics(input);
                text = text.Replace("<font", startFont);
                text = text.Replace("</font>", endFont);
                text = text.Replace("<i>", startItalic);
                text = text.Replace("</i>", endItalic);
                text = text.Replace("<I>", startItalic);
                text = text.Replace("</I>", endItalic);
                text = text.Replace("<u>", startUnderline);
                text = text.Replace("</u>", endUnderline);
                text = text.Replace("<U>", startUnderline);
                text = text.Replace("</U>", endUnderline);
                text = text.Replace("<box>", startBox);
                text = text.Replace("</box>", endBox);
                text = text.Replace("<BOX>", startBox);
                text = text.Replace("</BOX>", endBox);
                text = HtmlUtil.RemoveHtmlTags(text, true);
                text = text.Replace(startFont, "<font");
                text = text.Replace(endFont, "</font>");
                text = text.Replace(startItalic, "<i>");
                text = text.Replace(endItalic, "</i>");
                text = text.Replace(startUnderline, "<u>");
                text = text.Replace(endUnderline, "</u>");
                text = text.Replace(startBox, "<box>");
                text = text.Replace(endBox, "</box>");

                text = text.Replace(" </font>", "</font> ");
                var lastWasEndColor = false;
                var lastWasStartColor = false;
                var list = text.SplitToLines();
                for (var index = 0; index < list.Count; index++)
                {
                    if (index > 0)
                    {
                        sb.Append(newline);
                        textBytes.AddRange(newline);
                        if (displayStandardCode != "0" && lastColor != null)
                        {
                            sb.Append(lastColor);
                            textBytes.Add(lastColor.Value);
                        }
                    }

                    var line = list[index];
                    var i = 0;
                    while (i < line.Length)
                    {
                        var newStart = line.Substring(i);
                        if (newStart.StartsWith("<font ", StringComparison.OrdinalIgnoreCase))
                        {
                            lastWasStartColor = true;
                            var end = line.IndexOf('>', i);
                            if (end > 0)
                            {
                                if (displayStandardCode != "0")
                                {
                                    lastColor = GetColorByte(encoding, line, i);
                                    if (sb.EndsWith(' '))
                                    {
                                        sb = new StringBuilder(sb.ToString().TrimEnd(' '));
                                        if (textBytes.Count > 0 && textBytes[textBytes.Count - 1] == 32)
                                        {
                                            textBytes.RemoveAt(textBytes.Count - 1);
                                        }
                                    }

                                    if (lastColor != null)
                                    {
                                        sb.Append(lastColor);
                                        textBytes.Add(lastColor.Value);
                                    }
                                }

                                i = end + 1;
                            }
                        }
                        else if (newStart == "</font>")
                        {
                            i += "</font>".Length;
                            lastColor = null;
                            lastWasEndColor = true;
                        }
                        else if (newStart.StartsWith("</font>", StringComparison.OrdinalIgnoreCase))
                        {
                            i += "</font>".Length;

                            if (displayStandardCode != "0" && line.Length > i + 1)
                            {
                                var part = line.Substring(i);
                                if (part.StartsWith(" <font "))
                                {
                                    i++;
                                }
                                else if (part.StartsWith("<font "))
                                {
                                    // do nothing
                                }
                                else
                                {
                                    sb.Append(encoding.GetString(new byte[] { 0x07 })); // white
                                    textBytes.Add(0x07); // white
                                }
                            }

                            lastWasEndColor = true;
                            lastColor = null;
                        }
                        else if (newStart.StartsWith("<i>", StringComparison.Ordinal))
                        {
                            i += "<i>".Length;
                            textBytes.Add(italicOn);
                        }
                        else if (newStart.StartsWith("</i>", StringComparison.Ordinal))
                        {
                            i += "</i>".Length;
                            textBytes.Add(italicOff);
                        }
                        else if (newStart.StartsWith("<u>", StringComparison.Ordinal))
                        {
                            i += "<u>".Length;
                            textBytes.Add(underlineOn);
                        }
                        else if (newStart.StartsWith("</u>", StringComparison.Ordinal))
                        {
                            i += "</u>".Length;
                            textBytes.Add(underlineOff);
                        }
                        else if (newStart.StartsWith("<box>", StringComparison.Ordinal))
                        {
                            i += "<box>".Length;
                            textBytes.Add(boxingOn);
                        }
                        else if (newStart.StartsWith("</box>", StringComparison.Ordinal))
                        {
                            i += "</box>".Length;
                            textBytes.Add(boxingOff);
                        }
                        else
                        {
                            var ch = line[i];

                            var nextCh = line.Substring(i, 1);
                            if (nextCh == " " && lastWasEndColor)
                            {
                            }
                            else if (nextCh == " " && lastWasStartColor)
                            {
                            }
                            else
                            {
                                if (nextCh == "#")
                                {
                                    sb.Append(nextCh);
                                    textBytes.Add(0x23);
                                }
                                else if (nextCh == "Đ")
                                {
                                    sb.Append(nextCh);
                                    textBytes.Add(0xe2);
                                }
                                else if (nextCh == "–") // em dash
                                {
                                    sb.Append(nextCh);
                                    textBytes.Add(0xd0);
                                }
                                else
                                {
                                    if (characterCodeTableNumber == "00")
                                    {
                                        if (newStart.Length > 1 && line[i + 1] == 'ı' && newStart.StartsWith("ı̂")) // extended unicode char - rewritten as simple 'î' - looks the same as "î" but it's not...)
                                        {
                                            textBytes.AddRange(new byte[] { 0xc3, 0x69 }); // Ãi - simple î
                                            i++;
                                        }
                                        else if ("ÀÈÌÒÙàèìòù".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc1, "ÀÈÌÒÙàèìòù", "AEIOUaeiou"));
                                        }
                                        else if ("ÁĆÉÍĹŃÓŔŚÚÝŹáćéģíĺńóŕśúýź".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc2, "ÁĆÉÍĹŃÓŔŚÚÝŹáćéģíĺńóŕśúýź", "ACEILNORSUYZacegilnorsuyz"));
                                        }
                                        else if ("ÂĈÊĜĤÎĴÔŜÛŴŶâĉêĝĥĵôŝûŵŷîı̂".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc3, "ÂĈÊĜĤÎĴÔŜÛŴŶâĉêĝĥîĵôŝûŵŷ", "ACEGHIJOSUWYaceghijosuwy"));
                                        }
                                        else if ("ÃĨÑÕŨãĩñõũ".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc4, "ÃĨÑÕŨãĩñõũ", "AINOUainou"));
                                        }
                                        else if ("ĀĒĪŌŪāēīōū".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc5, "ĀĒĪŌŪāēīōū", "AEIOUaeiou"));
                                        }
                                        else if ("ĂĞŬăğŭ".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc6, "ĂĞŬăğŭ", "AGUagu"));
                                        }
                                        else if ("ĊĖĠİŻċėġıż".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc7, "ĊĖĠİŻċėġıż", "CEGIZcegiz"));
                                        }
                                        else if ("ÄËÏÖÜŸäëïöüÿ".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xc8, "ÄËÏÖÜŸäëïöüÿ", "AEIOUYaeiouy"));
                                        }
                                        else if ("ÅŮåů".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xca, "ÅŮåů", "AUau"));
                                        }
                                        else if ("ÇĢĶĻŅŖŞŢçķļņŗşţ".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xcb, "ÇĢĶĻŅŖŞŢçķļņŗşţ", "CGKLNRSTcklnrst"));
                                        }
                                        else if ("ŐŰőű".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xcd, "ŐŰőű", "OUou"));
                                        }
                                        else if ("ĄĘĮŲąęįų".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xce, "ĄĘĮŲąęįų", "AEIUaeiu"));
                                        }
                                        else if ("ČĎĚĽŇŘŠŤŽčďěľňřšťž".Contains(ch))
                                        {
                                            textBytes.AddRange(ReplaceSpecialCharactersWithTwoByteEncoding(encoding, ch, 0xcf, "ČĎĚĽŇŘŠŤŽčďěľňřšťž", "CDELNRSTZcdelnrstz"));
                                        }
                                        else if (SpecialAsciiCodes.ContainsValue(nextCh))
                                        {
                                            textBytes.Add((byte)SpecialAsciiCodes.First(p => p.Value == nextCh).Key);
                                        }
                                        else
                                        {
                                            sb.Append(nextCh);
                                            textBytes.AddRange(encoding.GetBytes(nextCh));
                                        }
                                    }
                                    else
                                    {
                                        sb.Append(nextCh);
                                        textBytes.AddRange(encoding.GetBytes(nextCh));
                                    }
                                }
                            }

                            i++;
                            lastWasEndColor = false;
                            lastWasStartColor = false;
                        }
                    }
                }

                if (Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox && displayStandardCode != "0")
                {
                    textBytes.AddRange(new byte[] { 0x0a, 0x0a }); //a=end box
                }
            }


            private static byte? GetColorByte(Encoding encoding, string line, int i)
            {
                var end = line.IndexOf('>', i);
                if (end > 0)
                {
                    var f = line.Substring(i, end - i);
                    var colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                    if (colorStart > 1)
                    {
                        // The attribute value may be double-quoted, single-quoted or bare
                        // ("<font color=#ffff00>" is common in SubRip files). The old code
                        // assumed a closing double quote and crashed the whole save on a
                        // negative Substring length when there was none.
                        var color = f.Substring(colorStart + " color=".Length).TrimStart();
                        if (color.Length > 0 && (color[0] == '"' || color[0] == '\''))
                        {
                            var quote = color[0];
                            var closingQuote = color.IndexOf(quote, 1);
                            color = closingQuote > 0 ? color.Substring(1, closingQuote - 1) : color.Substring(1);
                        }
                        else
                        {
                            var space = color.IndexOf(' ');
                            if (space > 0)
                            {
                                color = color.Substring(0, space);
                            }
                        }

                        color = color.Trim().Trim('#');
                        if (color.Length > 0)
                        {
                            return GetNearestColorCode(color);
                        }
                    }
                }

                return null;
            }




            private static byte[] ReplaceSpecialCharactersWithTwoByteEncoding(Encoding encoding, char ch, byte specialCharacter, string originalCharacters, string newCharacters)
            {
                if (originalCharacters.Length != newCharacters.Length)
                {
                    throw new ArgumentException("originalCharacters and newCharacters must have equal length");
                }

                for (var i = 0; i < newCharacters.Length; i++)
                {
                    if (originalCharacters[i] == ch)
                    {
                        var byteArr = new List<byte> { specialCharacter };
                        byteArr.AddRange(encoding.GetBytes(newCharacters[i].ToString()));
                        return byteArr.ToArray();
                    }
                }

                return encoding.GetBytes(ch.ToString());
            }

            public static byte GetFrameFromMilliseconds(int milliseconds, double frameRate, out byte extraSeconds)
            {
                extraSeconds = 0;
                var fr = Math.Round(milliseconds / (TimeCode.BaseUnit / frameRate));
                if (fr >= frameRate)
                {
                    fr = 0;
                    extraSeconds = 1;
                }

                return (byte)fr;
            }
        }

        public override string Extension => ".stl";

        public const string NameOfFormat = "EBU STL";

        public override string Name => NameOfFormat;

        // Carries the teletext row the line starts on (MarginV) and the row count in the GSI block.
        public override bool HasPositionSupport => true;

        internal struct SpecialCharacter
        {
            internal SpecialCharacter(string character, bool switchOrder = false, int priority = 2)
            {
                Character = character;
                SwitchOrder = switchOrder;
                Priority = priority;
            }

            internal string Character { get; set; }
            internal bool SwitchOrder { get; set; }
            internal int Priority { get; set; }
        }

        /// <summary>
        /// True when <paramref name="header"/> is a full 1024-character EBU STL header - present after
        /// loading an STL file or after the EBU save options dialog has stored one on the subtitle.
        /// The disk format code sits at 3..10 ("STLnn.mm") and is matched there rather than by
        /// listing frame rates, so every rate the save options dialog offers is recognized (STL23
        /// used to fall through, which threw the whole header away on save).
        /// </summary>
        public static bool IsStlHeader(string header)
        {
            return header != null &&
                   header.Length == 1024 &&
                   header.Substring(3, 3) == "STL";
        }

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
            {
                header = new EbuGeneralSubtitleInformation { LanguageCode = AutoDetectLanguageCode(subtitle) };

                // In an STL colours only exist as teletext control codes - open subtitling (the
                // default) has no colour mechanism at all. A coloured subtitle saved with an
                // invented header must be teletext level 1, or every colour would be dropped.
                if (subtitle.Paragraphs.Any(p => p.Text != null && p.Text.Contains("<font color", StringComparison.OrdinalIgnoreCase)))
                {
                    header.DisplayStandardCode = "1";
                }
            }

            if (EbuUiHelper == null)
            {
                return false;
            }

            // Paragraph.MarginV only holds a teletext row when the subtitle was read from an EBU
            // STL file (see LoadSubtitle). Every other format writes its own meaning into it -
            // ASSA dialogue lines always carry one - so the vertical position below may only be
            // taken from MarginV when this is true.
            var isEbuSource = IsStlHeader(subtitle.Header);

            // EBU-TT and DVB teletext write the same teletext row into MarginV as the STL reader
            // does, so a subtitle exchanged through them keeps its exact rows too.
            var isTeletextRowSource = isEbuSource ||
                                      EbuTt.IsEbuTtHeader(subtitle.Header) ||
                                      DvbTeletext.IsDvbTeletextHeader(subtitle.Header);
            if (isEbuSource)
            {
                header = ReadHeader(GetEncoding(subtitle.Header.Substring(0, 3)).GetBytes(subtitle.Header));
                EbuUiHelper.Initialize(header, EbuUiHelper.JustificationCode, null, subtitle);
            }
            else
            {
                EbuUiHelper.Initialize(header, EbuUiHelper.JustificationCode, fileName, subtitle);
            }

            if (!batchMode && !EbuUiHelper.ShowDialogOk())
            {
                return false;
            }

            header.TotalNumberOfSubtitles = subtitle.Paragraphs.Count.ToString("D5"); // seems to be 1 higher than actual number of subtitles
            header.TotalNumberOfTextAndTimingInformationBlocks = header.TotalNumberOfSubtitles;
            header.TotalNumberOfSubtitleGroups = "001";

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
                var frames = EbuTextTimingInformation.GetFrameFromMilliseconds(tc.Milliseconds, header.FrameRate, out var extraSeconds);
                tc = new TimeCode(tc.Hours, tc.Minutes, tc.Seconds + extraSeconds, 0);
                var firstTimeCode = $"{tc.Hours:00}{tc.Minutes:00}{tc.Seconds:00}{frames:00}";
                if (firstTimeCode.Length == 8)
                {
                    header.TimeCodeFirstInCue = firstTimeCode;
                }
            }

            var buffer = GetEncoding(header.CodePageNumber).GetBytes(header.ToString());
            stream.Write(buffer, 0, buffer.Length);

            var subtitleNumber = 0;
            // Counted as blocks are written: a paragraph whose encoded text needs an
            // extension block emits two, so TNB is not the subtitle count. It is patched
            // into the already-written header below.
            var numberOfTtiBlocks = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                var tti = new EbuTextTimingInformation();

                if (!int.TryParse(header.MaximumNumberOfDisplayableRows, out var rows))
                {
                    rows = 23;
                }

                if (header.DisplayStandardCode == "1" || header.DisplayStandardCode == "2") // teletext
                {
                    rows = 23;
                }
                else if (header.DisplayStandardCode == "0" && header.MaximumNumberOfDisplayableRows == "02") // open subtitling
                {
                    rows = 15;
                }

                var text = p.Text.Trim(Utilities.NewLineChars);

                var teletextPosition = 0;
                var isTeletext = header.DisplayStandardCode == "1" || header.DisplayStandardCode == "2";
                var hasTeletextPosition =
                    isTeletextRowSource &&
                    isTeletext &&
                    int.TryParse(p.MarginV, out teletextPosition) &&
                    teletextPosition >= 1 &&
                    teletextPosition <= rows;

                if (hasTeletextPosition)
                {
                    // The text may have been re-wrapped since it was read, so a position that no
                    // longer leaves room for every row would push the tail off the page.
                    var extraRows = Math.Max(0, Utilities.GetNumberOfLines(text) - 1) * Configuration.Settings.SubtitleSettings.EbuStlNewLineRows;
                    if (teletextPosition + extraRows > rows)
                    {
                        teletextPosition = Math.Max(1, rows - extraRows);
                    }

                    tti.VerticalPosition = (byte)teletextPosition;
                }
                else if (text.StartsWith("{\\an7}", StringComparison.Ordinal) || text.StartsWith("{\\an8}", StringComparison.Ordinal) || text.StartsWith("{\\an9}", StringComparison.Ordinal))
                {
                    tti.VerticalPosition = (byte)Configuration.Settings.SubtitleSettings.EbuStlMarginTop; // top (vertical)
                    if (header.DisplayStandardCode == "1" || header.DisplayStandardCode == "2") // teletext
                    {
                        tti.VerticalPosition++;
                    }
                }
                else if (text.StartsWith("{\\an4}", StringComparison.Ordinal) || text.StartsWith("{\\an5}", StringComparison.Ordinal) || text.StartsWith("{\\an6}", StringComparison.Ordinal))
                {
                    tti.VerticalPosition = (byte)(rows / 2); // middle (vertical)
                }
                else
                {
                    var numberOfLineBreaks = Math.Max(0, Utilities.GetNumberOfLines(text) - 1);
                    var startRow = rows - Configuration.Settings.SubtitleSettings.EbuStlMarginBottom -
                                          numberOfLineBreaks * Configuration.Settings.SubtitleSettings.EbuStlNewLineRows;
                    if (startRow < 0)
                    {
                        startRow = 0;
                    }

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
                text = text.Replace("‚", "'"); // lower apostrophe
                text = text.Replace("’", "'"); // right single quotation mark
                text = text.Replace("♫", "♪"); // only music single note supported
                text = text.Replace("…", "..."); // fix Unicode ellipsis

                tti.SubtitleNumber = (ushort)subtitleNumber;
                tti.TextField = text;
                int startTag = tti.TextField.IndexOf('}');
                if (tti.TextField.StartsWith("{\\", StringComparison.Ordinal) && startTag > 0 && startTag < 10)
                {
                    tti.TextField = tti.TextField.Remove(0, startTag + 1);
                }

                if (!p.StartTime.IsMaxTime)
                {
                    tti.TimeCodeInHours = p.StartTime.Hours;
                    tti.TimeCodeInMinutes = p.StartTime.Minutes;
                    tti.TimeCodeInSeconds = p.StartTime.Seconds;
                    tti.TimeCodeInMilliseconds = p.StartTime.Milliseconds;
                }

                if (!p.EndTime.IsMaxTime)
                {
                    tti.TimeCodeOutHours = p.EndTime.Hours;
                    tti.TimeCodeOutMinutes = p.EndTime.Minutes;
                    tti.TimeCodeOutSeconds = p.EndTime.Seconds;
                    tti.TimeCodeOutMilliseconds = p.EndTime.Milliseconds;
                }

                var extra = new MemoryStream();
                buffer = tti.GetBytes(header, extra);
                if (extra.Length > 0)
                {
                    // As many extension blocks as the overflow needs (112 text bytes each).
                    // EBN 0xFF marks the last block of the subtitle; earlier blocks are numbered,
                    // which is what makes LoadSubtitle merge them back together.
                    var extraBytes = extra.ToArray();
                    var extraBlockCount = (extraBytes.Length + 111) / 112;

                    buffer[3] = 0; // ExtensionBlockNumber - more blocks follow
                    stream.Write(buffer, 0, buffer.Length);
                    numberOfTtiBlocks++;

                    for (var extraBlock = 0; extraBlock < extraBlockCount; extraBlock++)
                    {
                        var extraBuffer = tti.GetBytesExtra(header, extraBytes, extraBlock * 112);
                        extraBuffer[3] = extraBlock == extraBlockCount - 1
                            ? (byte)0xff
                            : (byte)Math.Min(extraBlock + 1, 0xef);
                        stream.Write(extraBuffer, 0, extraBuffer.Length);
                        numberOfTtiBlocks++;
                    }
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                    numberOfTtiBlocks++;
                }
                subtitleNumber++;
            }
            // Rewrite GSI "Total Number of TTI blocks" (offset 238, 5 ASCII digits) now that the
            // real count is known - it was written as the subtitle count, so any file containing
            // an extension block under-reported it and tools walking the file by TNB lost the tail.
            if (stream.CanSeek && numberOfTtiBlocks > 0)
            {
                var position = stream.Position;
                var tnb = Encoding.ASCII.GetBytes(numberOfTtiBlocks.ToString("D5", CultureInfo.InvariantCulture));
                stream.Seek(238, SeekOrigin.Begin);
                stream.Write(tnb, 0, tnb.Length);
                stream.Seek(position, SeekOrigin.Begin);
            }

            return true;
        }

        private static string AutoDetectLanguageCode(Subtitle subtitle)
        {
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                return "00"; // Unknown/not applicable
            }

            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle);
            switch (languageCode)
            {
                case "sq": return "01"; // Albanian
                case "br": return "02"; // Breton
                case "ca": return "03"; // Catalan
                case "hr": return "04"; // Croatian
                case "cy": return "05"; // Welsh
                case "cs": return "06"; // Czech
                case "da": return "07"; // Danish
                case "de": return "08"; // German
                case "en": return "09"; // English
                case "es": return "0A"; // Spanish
                case "eo": return "0B"; // Esperanto
                case "et": return "0C"; // Estonian
                case "eu": return "0D"; // Basque
                case "fo": return "0E"; // Faroese
                case "fr": return "0F"; // French
                case "fy": return "10"; // Frisian
                case "ga": return "11"; // Irish
                case "gd": return "12"; // Gaelic
                case "gl": return "13"; // Galician
                case "is": return "14"; // Icelandic
                case "it": return "15"; // Italian
                case "Lappish": return "16"; // Lappish
                case "la": return "17"; // Latin
                case "lv": return "18"; // Latvian":
                case "lb": return "19"; // Luxembourgi
                case "lt": return "1A"; // Lithuanian
                case "hu": return "1B"; // Hungarian
                case "mt": return "1C"; // Maltese
                case "nl": return "1D"; // Dutch
                case "nb": return "1E"; // Norwegian
                case "oc": return "1F"; // Occitan
                case "pl": return "20"; // Polish
                case "pt": return "21"; // Portuguese
                case "ro": return "22"; // Romanian
                case "rm": return "23"; // Romansh
                case "sr": return "24"; // Serbian
                case "sk": return "25"; // Slovak
                case "sl": return "26"; // Slovenian
                case "fi": return "27"; // Finnish
                case "sv": return "28"; // Swedish
                case "tr": return "29"; // Turkish
                case "Flemish": return "2A"; // Flemish
                case "Wallon": return "2B"; // Wallon
            }

            return "09"; // English - default
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 1024 + 128 && fi.Length < 2048000) // not too small or too big
                {
                    try
                    {
                        var buffer = FileUtil.ReadBytesShared(fileName, 1024);
                        var header = ReadHeader(buffer);
                        if (header.DiskFormatCode.StartsWith("STL23", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL24", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL25", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL29", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL30", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL35", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL48", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL50", StringComparison.Ordinal) ||
                            header.DiskFormatCode.StartsWith("STL60", StringComparison.Ordinal) ||
                            "012 ".Contains(header.DisplayStandardCode) && "437|850|860|863|865".Contains(header.CodePageNumber))
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

        /// <summary>
        /// Drops what only an STL can carry: the teletext box tags and the teletext row in MarginV.
        /// </summary>
        /// <remarks>
        /// No other format knows either of them - the box tags used to end up as visible text in the
        /// video preview and in the saved file, and a row number ("20") counts as an ASSA pixel
        /// margin, which moved every line by a near random amount.
        /// </remarks>
        public override void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
            // The other teletext capable formats understand both: EBU-TT maps the box to a black
            // span background and the row to a region, DVB teletext boxes everything and places
            // lines by row - exchanging a subtitle between them keeps the teletext look.
            if (newFormat is EbuTt)
            {
                return;
            }

            var keepRows = newFormat is DvbTeletext;
            foreach (var p in subtitle.Paragraphs)
            {
                if (p.Text != null && p.Text.Contains("<box>", StringComparison.Ordinal))
                {
                    p.Text = p.Text.Replace("<box>", string.Empty).Replace("</box>", string.Empty);
                }

                if (!keepRows)
                {
                    p.MarginV = null;
                }
            }
        }

        public void LoadSubtitle(Subtitle subtitle, byte[] buffer)
        {
            subtitle.Paragraphs.Clear();
            var header = ReadHeader(buffer);
            subtitle.Header = header.ToString();
            if (header.DisplayStandardCode == "1" || header.DisplayStandardCode == "2" || HasTeletextColorCodes(buffer))
            {
                SeedTeletextBoxAndDoubleHeightSettings(buffer);
            }
            Paragraph last = null;
            byte lastExtensionBlockNumber = 0xff;
            JustificationCodes = new List<int>();
            Configuration.Settings.General.CurrentFrameRate = header.FrameRate;
            if (OverrideReadFrameRate > 20)
            {
                Configuration.Settings.General.CurrentFrameRate = OverrideReadFrameRate;
            }

            foreach (var tti in ReadTextAndTiming(buffer, header))
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

                    if (Math.Abs(p.StartTime.TotalMilliseconds) < 0.01 && Math.Abs(p.EndTime.TotalMilliseconds) < 0.01)
                    {
                        p.StartTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                        p.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                    }

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

        /// <summary>
        /// The teletext box/double-height write settings are global, but whether a file actually
        /// uses boxes or double height is a property of the file: seed the settings from the
        /// loaded bytes so a plain load-save round trip does not add control codes that were not
        /// there, and so the export dialog shows the file's actual state.
        /// </summary>
        private static void SeedTeletextBoxAndDoubleHeightSettings(byte[] buffer)
        {
            const int startOfTextAndTimingBlock = 1024;
            const int ttiSize = 128;
            const byte startBox = 0x0b;
            const byte doubleHeight = 0x0d;

            var useBox = false;
            var useDoubleHeight = false;
            var index = startOfTextAndTimingBlock;
            while (index + ttiSize <= buffer.Length && !(useBox && useDoubleHeight))
            {
                if (buffer[index + 3] != 0xfe && buffer[index + 15] == 0) // skip user data and comment blocks
                {
                    for (var i = index + 16; i < index + ttiSize; i++)
                    {
                        var b = buffer[i];
                        if (b == startBox)
                        {
                            useBox = true;
                        }
                        else if (b == doubleHeight)
                        {
                            useDoubleHeight = true;
                        }
                    }
                }

                index += ttiSize;
            }

            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = useBox;
            Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = useDoubleHeight;
        }

        /// <summary>
        /// Colors only exist as teletext control codes, so a file that declares open subtitling
        /// (DSC=0) is read without them. Some tools - Adobe Premiere among them - write the
        /// teletext codes anyway and still stamp the header with 0, and reading those strictly
        /// throws every color away. Nothing else uses 00h-1Fh in an open subtitling text field,
        /// so their presence is taken as proof the file really is teletext coded.
        /// </summary>
        private static bool HasTeletextColorCodes(byte[] buffer)
        {
            const int startOfTextAndTimingBlock = 1024;
            const int ttiSize = 128;

            var index = startOfTextAndTimingBlock;
            while (index + ttiSize <= buffer.Length)
            {
                if (buffer[index + 3] != 0xfe && buffer[index + 15] == 0) // skip user data and comment blocks
                {
                    for (var i = index + 16; i < index + ttiSize; i++)
                    {
                        // 00h (black) is left out on purpose: a writer that pads the text field with
                        // zero bytes instead of 8Fh would otherwise look like a colored file.
                        if (buffer[i] >= 0x01 && buffer[i] <= 0x07)
                        {
                            return true;
                        }
                    }
                }

                index += ttiSize;
            }

            return false;
        }

        public static EbuGeneralSubtitleInformation ReadHeader(byte[] buffer)
        {
            var enc = GetEncoding(Encoding.ASCII.GetString(buffer, 0, 3));
            var header = new EbuGeneralSubtitleInformation
            {
                CodePageNumber = enc.GetString(buffer, 0, 3),
                DiskFormatCode = enc.GetString(buffer, 3, 8),
                DisplayStandardCode = enc.GetString(buffer, 11, 1),
                CharacterCodeTableNumber = enc.GetString(buffer, 12, 2),
                LanguageCode = enc.GetString(buffer, 14, 2),
                OriginalProgrammeTitle = enc.GetString(buffer, 16, 32),
                OriginalEpisodeTitle = enc.GetString(buffer, 48, 32),
                TranslatedProgrammeTitle = enc.GetString(buffer, 80, 32),
                TranslatedEpisodeTitle = enc.GetString(buffer, 112, 32),
                TranslatorsName = enc.GetString(buffer, 144, 32),
                TranslatorsContactDetails = enc.GetString(buffer, 176, 32),
                SubtitleListReferenceCode = enc.GetString(buffer, 208, 16),
                CreationDate = enc.GetString(buffer, 224, 6),
                RevisionDate = enc.GetString(buffer, 230, 6),
                RevisionNumber = enc.GetString(buffer, 236, 2),
                TotalNumberOfTextAndTimingInformationBlocks = enc.GetString(buffer, 238, 5),
                TotalNumberOfSubtitles = enc.GetString(buffer, 243, 5),
                TotalNumberOfSubtitleGroups = enc.GetString(buffer, 248, 3),
                MaximumNumberOfDisplayableCharactersInAnyTextRow = enc.GetString(buffer, 251, 2),
                MaximumNumberOfDisplayableRows = enc.GetString(buffer, 253, 2),
                TimeCodeStatus = enc.GetString(buffer, 255, 1),
                TimeCodeStartOfProgramme = enc.GetString(buffer, 256, 8),
                TimeCodeFirstInCue = enc.GetString(buffer, 264, 8),
                TotalNumberOfDisks = enc.GetString(buffer, 272, 1),
                DiskSequenceNumber = enc.GetString(buffer, 273, 1),
                CountryOfOrigin = enc.GetString(buffer, 274, 3),
                Publisher = enc.GetString(buffer, 277, 32),
                EditorsName = enc.GetString(buffer, 309, 32),
                EditorsContactDetails = enc.GetString(buffer, 341, 32),
                SpareBytes = enc.GetString(buffer, 373, 75),
                UserDefinedArea = enc.GetString(buffer, 448, 576)
            };
            return header;
        }

        public static Encoding GetEncoding(string codePageNumber)
        {
            try
            {
                return Encoding.GetEncoding(int.TryParse(codePageNumber, out int cp) ? cp : 437);
            }
            catch (NotSupportedException)
            {
                return Encoding.GetEncoding(437);
            }
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
                if (SpecialAsciiCodes.TryGetValue(b, out var s))
                {
                    return s;
                }

                Encoding encoding;
                //note that 0xC1—0xCF combines characters - http://en.wikipedia.org/wiki/ISO/IEC_6937
                try
                {
                    encoding = Encoding.GetEncoding(20269);
                }
                catch
                {
                    encoding = Encoding.ASCII;
                }

                if (index + 2 > buffer.Length)
                {
                    return string.Empty;
                }

                var next = encoding.GetString(buffer, index + 1, 1);
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
            const byte boxingOn = 0x84;
            const byte boxingOff = 0x85;

            var list = new List<EbuTextTimingInformation>();
            var hasTeletextColorCodes = header.DisplayStandardCode == "0" && HasTeletextColorCodes(buffer);
            var index = startOfTextAndTimingBlock;
            var sb = new StringBuilder();
            // EBN 0xFF marks the LAST block of a subtitle, so a block continues the previous one
            // when the previous block's EBN was not 0xFF - the same rule LoadSubtitle merges on.
            byte previousExtensionBlockNumber = 0xff;
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
                JustificationCodes.Add(tti.JustificationCode);

                // Text block
                // - has a fixed length of 112 byte
                // - 8Ah = new line
                // - unused space = 8Fh
                var i = index + 16; // text block start at 17th byte (index 16)
                var open = header.DisplayStandardCode != "1" && header.DisplayStandardCode != "2";
                var closed = header.DisplayStandardCode != "0" || hasTeletextColorCodes;
                var max = i + 112;
                sb.Clear();
                var lastWasNewLine = false;
                while (i < max)
                {
                    var b = buffer[i];
                    if (b <= 0x1f) // Closed - Teletext control codes
                    {
                        if (closed)
                        {
                            var tag = GetColorOrTag(b);
                            if (!string.IsNullOrEmpty(tag))
                            {
                                CloseFontTagIfNewColor(sb, tag);
                            }
                        }
                    }
                    else if (b >= 0x20 && b <= 0x7f) // Both - Character codes
                    {
                        var ch = GetCharacter(out var skipNext, header, buffer, i);
                        sb.Append(ch);
                        if (skipNext)
                        {
                            i++;
                        }
                    }
                    else if (b >= 0x80 && b <= 0x85) // Open - italic/underline/boxing
                    {
                        if (open)
                        {
                            if (b == italicsOn && header.LanguageCode != LanguageCodeChinese)
                            {
                                sb.Append("<i>");
                            }
                            else if (b == italicsOff && header.LanguageCode != LanguageCodeChinese)
                            {
                                sb.Append("</i>");
                            }
                            else if (b == underlineOn && header.LanguageCode != LanguageCodeChinese)
                            {
                                sb.Append("<u>");
                            }
                            else if (b == underlineOff && header.LanguageCode != LanguageCodeChinese)
                            {
                                sb.Append("</u>");
                            }
                            else if (b == boxingOn && header.LanguageCode != LanguageCodeChinese)
                            {
                                sb.Append("<box>");
                            }
                            else if (b == boxingOff && header.LanguageCode != LanguageCodeChinese)
                            {
                                sb.Append("</box>");
                            }
                        }
                    }
                    else if (b >= 0x86 && b <= 0x89) // Both - Reserved for future use
                    {
                    }
                    else if (b == 0x8a) // Both - CR/LF
                    {
                        if (!lastWasNewLine)
                        {
                            AddMissingClosingTag(sb);
                            sb.AppendLine();
                            lastWasNewLine = true;
                            i++;
                            continue;
                        }
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
                        var ch = GetCharacter(out var skipNext, header, buffer, i);
                        if (sb.EndsWith('>') && sb.ToString().EndsWith("</font>"))
                        {
                            if (ch != " ")
                            {
                                sb.Append(' ');
                            }
                        }

                        sb.Append(ch);
                        if (skipNext)
                        {
                            i++;
                        }
                    }

                    lastWasNewLine = false;
                    i++;
                }

                AddMissingClosingTag(sb);
                tti.TextField = FixSpacesAndTags(sb.ToString());

                if (!int.TryParse(header.MaximumNumberOfDisplayableRows, out var rows))
                {
                    rows = 23;
                }

                // Only the first block of a subtitle carries the alignment tag. A continuation
                // block (EBN != 0xFF) is appended to the previous paragraph's text by
                // LoadSubtitle, so tagging it too spliced an "{\anN}" into mid-sentence.
                var isContinuationBlock = previousExtensionBlockNumber != 0xff;
                if (tti.ExtensionBlockNumber != 0xfe) // FEh is user data, not part of the chain
                {
                    previousExtensionBlockNumber = tti.ExtensionBlockNumber;
                }

                if (!isContinuationBlock)
                {
                    if (tti.VerticalPosition < 3)
                    {
                        if (tti.JustificationCode == 1) // left
                        {
                            tti.TextField = "{\\an7}" + tti.TextField;
                        }
                        else if (tti.JustificationCode == 3) // right
                        {
                            tti.TextField = "{\\an9}" + tti.TextField;
                        }
                        else
                        {
                            tti.TextField = "{\\an8}" + tti.TextField;
                        }
                    }
                    else if (tti.VerticalPosition <= rows / 2 + 1)
                    {
                        if (tti.JustificationCode == 1) // left
                        {
                            tti.TextField = "{\\an4}" + tti.TextField;
                        }
                        else if (tti.JustificationCode == 3) // right
                        {
                            tti.TextField = "{\\an6}" + tti.TextField;
                        }
                        else
                        {
                            tti.TextField = "{\\an5}" + tti.TextField;
                        }
                    }
                    else
                    {
                        if (tti.JustificationCode == 1) // left
                        {
                            tti.TextField = "{\\an1}" + tti.TextField;
                        }
                        else if (tti.JustificationCode == 3) // right
                        {
                            tti.TextField = "{\\an3}" + tti.TextField;
                        }
                    }
                }
                index += ttiSize;
                list.Add(tti);
            }

            return list;
        }

        private static void AddMissingClosingTag(StringBuilder sb)
        {
            var s = sb.ToString();
            var startTags = Utilities.CountTagInText(s, "<font ");
            var endTags = Utilities.CountTagInText(s, "</font>");
            if (startTags > endTags)
            {
                sb.Append("</font>");
            }
        }

        private static void CloseFontTagIfNewColor(StringBuilder sb, string tag)
        {
            var previousText = sb.ToString();
            if (string.IsNullOrEmpty(previousText))
            {
                if (!string.IsNullOrEmpty(tag) && !tag.Contains("\"White\""))
                {
                    if (sb.Length > 0 && !sb.EndsWith(' '))
                    {
                        sb.Append(' ');
                    }

                    sb.Append(tag);
                }

                return;
            }

            var lastFontStartTag = previousText.LastIndexOf("<font color", StringComparison.OrdinalIgnoreCase);
            if (lastFontStartTag < 0)
            {
                if (!string.IsNullOrEmpty(tag) && !tag.Contains("\"White\""))
                {
                    if (sb.Length > 0 && !sb.EndsWith(' '))
                    {
                        sb.Append(' ');
                    }

                    sb.Append(tag);
                }

                return;
            }

            var lastFontEndTag = previousText.LastIndexOf("</font>", StringComparison.OrdinalIgnoreCase);
            if (lastFontEndTag > lastFontStartTag)
            {
                if (!string.IsNullOrEmpty(tag) && !tag.Contains("\"White\""))
                {
                    if (sb.Length > 0 && !sb.EndsWith(' '))
                    {
                        sb.Append(' ');
                    }

                    sb.Append(tag);
                }

                return;
            }

            if (previousText.TrimEnd(' ').EndsWith(Environment.NewLine))
            {
                var text = sb.ToString();
                sb.Clear();
                sb.Append(text.TrimEnd());
                sb.Append("</font>" + Environment.NewLine);
            }
            else if (previousText.EndsWith(' '))
            {
                var text = sb.ToString();
                sb.Clear();
                sb.Append(text.TrimEnd(' '));
                sb.Append("</font> ");
            }
            else
            {
                sb.Append("</font> ");
            }

            if (!string.IsNullOrEmpty(tag) && !tag.Contains("\"White\""))
            {
                if (sb.Length > 0 && !sb.EndsWith(' '))
                {
                    sb.Append(' ');
                }

                sb.Append(tag);
            }
        }

        /// <summary>
        /// The teletext colour code an STL file would carry for <paramref name="color"/> - a colour
        /// name ("Red") or six hex digits, with or without a leading '#' - or null when the value is
        /// not a colour at all. The eight teletext colours are the corners of the RGB cube, so
        /// anything else is snapped to the nearest one; Save writes what this returns, and the UI
        /// asks it what a colour will become before it writes a tag (via GetNearestColorName).
        /// </summary>
        internal static byte? GetNearestColorCode(string color)
        {
            color = color.Trim().TrimStart('#').ToLowerInvariant();
            if (color == "black" || color == "000000")
            {
                return 0x00; // black
            }

            if (color == "red" || color == "ff0000")
            {
                return 0x01; // red
            }

            if (color == "green" || color == "00ff00")
            {
                return 0x02; // green
            }

            if (color == "yellow" || color == "ffff00")
            {
                return 0x03; // yellow
            }

            if (color == "blue" || color == "0000ff")
            {
                return 0x04; // blue
            }

            if (color == "magenta" || color == "ff00ff")
            {
                return 0x05; // magenta
            }

            if (color == "cyan" || color == "00ffff")
            {
                return 0x06; // cyan
            }

            if (color == "white" || color == "ffffff")
            {
                return 0x07; // white
            }

            if (color.Length == 6)
            {
                if (RegExprColor.IsMatch(color))
                {
                    const int maxDiff = 130;
                    var r = int.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                    var g = int.Parse(color.Substring(2, 2), NumberStyles.HexNumber);
                    var b = int.Parse(color.Substring(4, 2), NumberStyles.HexNumber);
                    if (r < maxDiff && g < maxDiff && b < maxDiff)
                    {
                        return 0x00; // black
                    }

                    if (r > 255 - maxDiff && g < maxDiff && b < maxDiff)
                    {
                        return 0x01; // red
                    }

                    if (r < maxDiff && g > 255 - maxDiff && b < maxDiff)
                    {
                        return 0x02; // green
                    }

                    if (r > 255 - maxDiff && g > 255 - maxDiff && b < maxDiff)
                    {
                        return 0x03; // yellow
                    }

                    if (r < maxDiff && g < maxDiff && b > 255 - maxDiff)
                    {
                        return 0x04; // blue
                    }

                    if (r > 255 - maxDiff && g < maxDiff && b > 255 - maxDiff)
                    {
                        return 0x05; // magenta
                    }

                    if (r < maxDiff && g > 255 - maxDiff && b > 255 - maxDiff)
                    {
                        return 0x06; // cyan
                    }

                    if (r > 255 - maxDiff && g > 255 - maxDiff && b > 255 - maxDiff)
                    {
                        return 0x07; // white
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The name of the teletext colour <paramref name="color"/> is nearest to - the same name
        /// the STL reader writes into the text - or null when it is not a colour at all.
        /// </summary>
        public static string GetNearestColorName(string color)
        {
            var code = GetNearestColorCode(color);
            return code == null ? null : GetColorName(code.Value);
        }

        private static string GetColorName(byte b)
        {
            switch (b)
            {
                case 0x00:
                    return "Black";
                case 0x01:
                    return "Red";
                case 0x02:
                    return "Green";
                case 0x03:
                    return "Yellow";
                case 0x04:
                    return "Blue";
                case 0x05:
                    return "Magenta";
                case 0x06:
                    return "Cyan";
                case 0x07:
                    return "White";
            }

            return null;
        }

        private static string GetColorOrTag(byte b)
        {
            //case 0x0a:
            //    return "</box>";
            //case 0x0b:
            //    return "<box>";
            var name = GetColorName(b);
            return name == null ? null : "<font color=\"" + name + "\">";
        }

        private static string FixSpacesAndTags(string text)
        {
            text = EmptyFontTag.Replace(text, string.Empty).Trim();
            while (text.Contains("  </font>"))
            {
                text = text.Replace("  </font>", " </font>");
            }

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

            var lines = text.SplitToLines();
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line);
                var count = Utilities.CountTagInText(line, "<font ");
                if (count == 1 && !line.Contains("</font>"))
                {
                    sb.Append("</font>");
                }

                if (Configuration.Settings.SubtitleSettings.EbuStlRemoveEmptyLines &&
                    HtmlUtil.RemoveHtmlTags(line).Length == 0)
                {
                    continue;
                }

                sb.AppendLine();
            }

            text = sb.ToString().TrimEnd();

            while (text.Contains(Environment.NewLine + " "))
            {
                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
            }

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
