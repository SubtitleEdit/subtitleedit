using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// ARIB B-36 - see http://www.arib.or.jp/english/html/overview/doc/2-STD-B36v2_3.pdf (ARIB = Japan Association of Radio Industries and Businesses)
    /// For the text decodinfg see http://www.arib.or.jp/english/html/overview/doc/6-STD-B24v5_2-1p3-E1.pdf
    /// Also see https://github.com/johnoneil/arib
    /// </summary>
    public class AribB36 : SubtitleFormat
    {

        public class ProgramManagement
        {
            public string ProductionDepartmentDisplay { get; set; } // 6 bytes
            public string MaterialNumber { get; set; } // 27 bytes
            public string ProgramName { get; set; } // 40 bytes
            public string ProgramSubtitle { get; set; } // 40 bytes
            public string MaterialType { get; set; } // 1 byte
            public string RegistrationMode { get; set; } // 1 byte
            public string LanguageCode { get; set; } // 3 bytes - ISO639
            public string DmfReceptionDisplay { get; set; } // 2 bytes
            public string IndependenceCompletionSubtitles { get; set; } // 1 byte
            public string Sound { get; set; } // 1 byte
            public string TotalNumberOfPages { get; set; } // 4 bytes
            public string ProgramDataAmount { get; set; } // 8 bytes
            public string TimingPresent { get; set; } // 1 byte - Space: No, *: Yes
            public string TimingType { get; set; } // 2 bytes
            public string TimingUnit { get; set; } // 1 byte - T: Time UnitF: Frame

            public ProgramManagement(byte[] buffer, int index)
            {
                ProductionDepartmentDisplay = Encoding.ASCII.GetString(buffer, index, 6);
                MaterialNumber = Encoding.ASCII.GetString(buffer, index + 6, 27);
                ProgramName = Encoding.ASCII.GetString(buffer, index + 33, 40);
                ProgramSubtitle = Encoding.ASCII.GetString(buffer, index + 73, 40);
                MaterialType = Encoding.ASCII.GetString(buffer, index + 113, 1);
                RegistrationMode = Encoding.ASCII.GetString(buffer, index + 114, 1);
                LanguageCode = Encoding.ASCII.GetString(buffer, index + 115, 3);
                DmfReceptionDisplay = Encoding.ASCII.GetString(buffer, index + 118, 2);
                IndependenceCompletionSubtitles = Encoding.ASCII.GetString(buffer, index + 120, 1);
                Sound = Encoding.ASCII.GetString(buffer, index + 121, 1);
                TotalNumberOfPages = Encoding.ASCII.GetString(buffer, index + 122, 4);
                ProgramDataAmount = Encoding.ASCII.GetString(buffer, index + 126, 8);
                TimingPresent = Encoding.ASCII.GetString(buffer, index + 134, 1);
                TimingType = Encoding.ASCII.GetString(buffer, index + 135, 2);
                TimingUnit = Encoding.ASCII.GetString(buffer, index + 137, 1);
            }
        }

        public class PageManagement
        {
            public string PageNumber { get; set; } // 6 bytes
            public string PageMaterialType { get; set; } // 1 byte
            public string TransmissionTimingType { get; set; } // 2 bytes
            public string SpecifiedTimingUnit { get; set; } // 1 byte
            public string TransmissionTiming { get; set; } // 9 bytes
            public string EraseTiming { get; set; } // 9 byte
            public string TimeControlMode { get; set; } // 2 bytes
            public string DeleteScreen { get; set; } // 3 bytes
            public string PresentationFormat { get; set; } // 3 bytes
            public string DisplayVideoEffectiveRatio { get; set; } // 1 byte
            public string WindowDisplayArea { get; set; } // 16 bytes
            public string Scroll { get; set; } // 1 byte
            public string ScrollDirection { get; set; } // 1 byte
            public string Sound { get; set; } // 1 byte
            public string PageAmountOfData { get; set; } // 5 bytes
            public string PageDeleteSpecified { get; set; } // 3 bytes
            public string Memo { get; set; } // 20 bytes
            public string Reserve { get; set; } // 32 bytes
            public string PageCompletedMark { get; set; } // 3 bytes
            public string UserAreaIdentification { get; set; } // 1 bytes
            public string UserArea { get; set; } // ? bytes


            public PageManagement(byte[] buffer, int index)
            {
                PageNumber = Encoding.ASCII.GetString(buffer, index, 6);
                PageMaterialType = Encoding.ASCII.GetString(buffer, index + 6, 1);
                TransmissionTimingType = Encoding.ASCII.GetString(buffer, index + 7, 2);
                SpecifiedTimingUnit = Encoding.ASCII.GetString(buffer, index + 9, 1);
                TransmissionTiming = Encoding.ASCII.GetString(buffer, index + 10, 9);
                EraseTiming = Encoding.ASCII.GetString(buffer, index + 19, 9);
                TimeControlMode = Encoding.ASCII.GetString(buffer, index + 28, 2);
                DeleteScreen = Encoding.ASCII.GetString(buffer, index + 30, 3);
                PresentationFormat = Encoding.ASCII.GetString(buffer, index + 33, 3);
                DisplayVideoEffectiveRatio = Encoding.ASCII.GetString(buffer, index + 36, 1);
                WindowDisplayArea = Encoding.ASCII.GetString(buffer, index + 37, 16);
                Scroll = Encoding.ASCII.GetString(buffer, index + 53, 1);
                ScrollDirection = Encoding.ASCII.GetString(buffer, index + 54, 1);
                Sound = Encoding.ASCII.GetString(buffer, index + 55, 1);
                PageAmountOfData = Encoding.ASCII.GetString(buffer, index + 56, 5);
                PageDeleteSpecified = Encoding.ASCII.GetString(buffer, index + 61, 3);
                Memo = Encoding.ASCII.GetString(buffer, index + 64, 20);
                Reserve = Encoding.ASCII.GetString(buffer, index + 84, 32);
                PageCompletedMark = Encoding.ASCII.GetString(buffer, index + 116, 1);
                UserAreaIdentification = Encoding.ASCII.GetString(buffer, index + 117, 1);
            }
            private static TimeCode GetTime(string s, string timingUnit)
            {
                if (s.Length == 9)
                {
                    var hours = int.Parse(s.Substring(0, 2));
                    var minutes = int.Parse(s.Substring(2, 2));
                    var seconds = int.Parse(s.Substring(4, 2));
                    if (timingUnit == "T")
                    {
                        //HHMMSSXX0 (last '0' is hardcoded, last 3 bytes is milliseconds)
                        var milliseconds = int.Parse(s.Substring(6, 3));
                        return new TimeCode(hours, minutes, seconds, milliseconds);
                    }
                    else
                    {
                        //HHMMSSXXF (last 'F' is hardcoded, XX=frames)
                        var frames = int.Parse(s.Substring(6, 2));
                        return new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
                    }
                }
                return new TimeCode();
            }

            public TimeCode GetStartTime()
            {
                return GetTime(TransmissionTiming, SpecifiedTimingUnit);
            }

            public TimeCode GetEndTime()
            {
                string trimmed = EraseTiming.Trim();
                if (trimmed == "F" || trimmed == "0" || trimmed.Length == 0)
                {
                    return new TimeCode();
                }

                return GetTime(EraseTiming, SpecifiedTimingUnit);
            }
        }

        public class CaptionTextPageManagement
        {
            public byte DataGroupId { get; set; }
            public byte DataGroupVersion { get; set; }
            public byte DataGroupLinkNumber { get; set; }
            public byte LastDataGroupLinkNumber { get; set; }
            public int DataGroupSize { get; set; }
            public byte TimeControlMode { get; set; }
            public string OffsetTimeMode { get; set; }
            public byte NumberOfLanguages { get; set; }
            public byte LanguageTag { get; set; }
            public string DisplayMode { get; set; }
            public byte DisplayModeControl { get; set; }
            public string Iso639LanguageCode { get; set; }
            public byte Format { get; set; }
            public string CharacterEncoding { get; set; }
            public string RollupMode { get; set; }

            public CaptionTextPageManagement(byte[] buffer, int index)
            {
                DataGroupId = (byte)(buffer[index] >> 2);
                DataGroupVersion = (byte)(buffer[index] & 0b00000011);
                DataGroupLinkNumber = buffer[index + 1];
                LastDataGroupLinkNumber = buffer[index + 2];
                DataGroupSize = (buffer[index + 3] << 8) + buffer[index + 4];
                TimeControlMode = (byte)(buffer[index + 5] >> 6);
                NumberOfLanguages = buffer[index + 12];
                LanguageTag = (byte)(buffer[index + 13] >> 5);
                DisplayModeControl = buffer[index + 13];
                Iso639LanguageCode = Encoding.ASCII.GetString(buffer, index + 14, 3);
                Format = (byte)(buffer[index + 18] >> 4);
            }
        }

        public class CaptionTextUnit
        {
            public byte UnitSeparator { get; set; }
            public byte DataUnitParameter { get; set; }
            public int DataUnitSize { get; set; }
            public AribText AribText { get; set; }
        }

        public class AribTextATag
        {
            public Point Location { get; set; }
            public byte[] Data { get; set; }
            public string Text { get; set; }
        }

        public class AribText
        {
            private static readonly Regex ATag = new Regex(@"\d+;\d+ a", RegexOptions.Compiled);

            public double DurationInSeconds { get; set; }
            public Point AreaSize { get; set; }
            public Point AreaLocation { get; set; }
            public Point FontWidthAndHeight { get; set; }
            public int CharacterSpacing { get; set; }
            public int LineSpacing { get; set; }
            public List<AribTextATag> Texts { get; set; }

            public AribText(byte[] buffer, int index, string languageCode, int length)
            {
                Texts = new List<AribTextATag>();
                var sb = new StringBuilder();
                int startBuffer = index;
                for (int i = 0; i <= length; i++)
                {
                    var b = buffer[index + i];
                    if (b == 0x9b || i == length) // 0x9b = separator
                    {
                        var code = sb.ToString();
                        var match = ATag.Match(code);
                        if (match.Success)
                        {
                            code = match.Value;
                            var arr = code.TrimEnd('a').TrimEnd().Split(';');
                            int x, y;
                            if (arr.Length == 2 && int.TryParse(arr[0], out x) && int.TryParse(arr[1], out y))
                            {
                                int start = startBuffer + match.Length;
                                int len = index + i - start;
                                var decodedText = AribB24Decoder.AribToString(buffer, start, len);
                                Texts.Add(new AribTextATag
                                {
                                    Location = new Point(x, y),
                                    Text = decodedText
                                });
                            }
                        }
                        else if (code.EndsWith(" S"))
                        {
                            double d;
                            if (double.TryParse(code.TrimEnd('S'), out d))
                            {
                                DurationInSeconds = d;
                            }
                        }
                        else if (code.EndsWith(" V"))
                        {
                            var arr = code.TrimEnd('V').TrimEnd().Split(';');
                            int x, y;
                            if (arr.Length == 2 && int.TryParse(arr[0], out x) && int.TryParse(arr[1], out y))
                            {
                                AreaSize = new Point(x, y);
                            }
                        }
                        else if (code.EndsWith(" _"))
                        {
                            var arr = code.TrimEnd('_').TrimEnd().Split(';');
                            int x, y;
                            if (arr.Length == 2 && int.TryParse(arr[0], out x) && int.TryParse(arr[1], out y))
                            {
                                AreaLocation = new Point(x, y);
                            }
                        }
                        else if (code.EndsWith(" W"))
                        {
                            var arr = code.TrimEnd('W').TrimEnd().Split(';');
                            int x, y;
                            if (arr.Length == 2 && int.TryParse(arr[0], out x) && int.TryParse(arr[1], out y))
                            {
                                FontWidthAndHeight = new Point(x, y);
                            }
                        }
                        else if (code.EndsWith(" X"))
                        {
                            var s = code.TrimEnd('X').TrimEnd();
                            int x;
                            if (int.TryParse(s, out x))
                            {
                                CharacterSpacing = x;
                            }
                        }
                        else if (code.EndsWith(" Y"))
                        {
                            var s = code.TrimEnd('Y').TrimEnd();
                            int y;
                            if (int.TryParse(s, out y))
                            {
                                LineSpacing = y;
                            }
                        }
                        sb.Clear();
                        startBuffer = index + i + 1;
                    }
                    else
                    {
                        sb.Append(Encoding.ASCII.GetString(buffer, index + i, 1));
                    }
                }
            }
        }

        public class CaptionText
        {
            public byte DataGroupId { get; set; }
            public byte DataGroupVersion { get; set; }
            public byte DataGroupLinkNumber { get; set; }
            public byte LastDataGroupLinkNumber { get; set; }
            public int DataGroupSize { get; set; }
            public int DataUnitLoopLength { get; set; }

            public List<CaptionTextUnit> CaptionTextUnits { get; set; }

            public CaptionText(byte[] buffer, int index, string languageCode)
            {
                DataGroupId = (byte)(buffer[index] >> 2);
                DataGroupVersion = (byte)(buffer[index] & 0b00000011);
                DataGroupLinkNumber = buffer[index + 1];
                LastDataGroupLinkNumber = buffer[index + 2];
                DataGroupSize = (buffer[index + 3] << 8) + buffer[index + 4];
                DataUnitLoopLength = (buffer[index + 12] << 16) + (buffer[index + 13] << 8) + buffer[index + 14];
                CaptionTextUnits = new List<CaptionTextUnit>(1);
                var unitIndex = index + 15;
                int end = unitIndex + DataUnitLoopLength;
                while (unitIndex < end)
                {
                    var unit = new CaptionTextUnit();
                    unit.UnitSeparator = buffer[unitIndex++];
                    unit.DataUnitParameter = buffer[unitIndex++];
                    unit.DataUnitSize = (buffer[unitIndex++] << 16) + (buffer[unitIndex++] << 8) + buffer[unitIndex++];
                    unit.AribText = new AribText(buffer, unitIndex, languageCode, unit.DataUnitSize);
                    unitIndex += unit.DataUnitSize;
                    if (unit.UnitSeparator == 0x1f && unit.DataUnitParameter == 0x20 && unit.AribText.Texts.Count > 0) // DataUnitParameter 0x20=Text, 0x35=Bitmap data
                    {
                        CaptionTextUnits.Add(unit);
                    }
                }
            }
        }

        public override string Extension
        {
            get { return ".1HD"; }
        }

        public override string Name
        {
            get { return "ARIB"; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 3000 && fi.Length < 1024000) // not too small or too big
                {
                    string fileExt = Path.GetExtension(fileName).ToUpperInvariant();
                    if (fileExt != Extension && !AlternateExtensions.Contains(fileExt))
                    {
                        return false;
                    }

                    return base.IsMine(lines, fileName);
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName, string videoFileName, Subtitle subtitle)
        {
        }

        int RoundUp(int number, int multiple)
        {
            if (multiple == 0)
            {
                return number;
            }

            int remainder = number % multiple;
            if (remainder == 0)
            {
                return number;
            }

            return number + multiple - remainder;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            const int startPosition = 256 * 3; // First 256 bytes block is just id, two next blocks are ProgramManagementInformation, then comes the list of PageManagementInformations
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            var buffer = FileUtil.ReadAllBytesShared(fileName);
            int index = startPosition;
            string label = Encoding.ASCII.GetString(buffer, 0, 8);
            if (label != "DCAPTION" && label != "BCAPTION" && label != "MCAPTION")
            {
                return;
            }

            var programManagementInformation = new ProgramManagement(buffer, 256 + 4);
            while (index + 255 < buffer.Length)
            {
                if (buffer[index + 4] == 0x2a) // Page management information
                {
                    int blockstart = index;
                    int length = (buffer[index + 2] << 8) + buffer[index + 3];
                    index += 5;
                    var pageManagementInformationLength = (buffer[index++] << 8) + buffer[index++];
                    var pageManagementInformation = new PageManagement(buffer, index);
                    index += pageManagementInformationLength;
                    if (buffer[index] == 0x3a) // Caption text page management data
                    {
                        index++;
                        var captionTextPageManagementLength = (buffer[index++] << 8) + buffer[index++];
                        var captionTextPageManagement = new CaptionTextPageManagement(buffer, index);
                        index += captionTextPageManagementLength;

                        if (buffer[index] == 0x4a) // Caption text data
                        {
                            index++;
                            var subtitleDataLength = (buffer[index++] << 8) + buffer[index++];
                            try
                            {
                                var captionText = new CaptionText(buffer, index, captionTextPageManagement.Iso639LanguageCode);
                                var p = new Paragraph
                                {
                                    StartTime = pageManagementInformation.GetStartTime(),
                                    EndTime = pageManagementInformation.GetEndTime()
                                };
                                foreach (var unit in captionText.CaptionTextUnits)
                                {
                                    foreach (var text in unit.AribText.Texts)
                                    {
                                        p.Text = (p.Text + Environment.NewLine + text.Text).Trim();
                                    }
                                }
                                subtitle.Paragraphs.Add(p);
                            }
                            catch
                            {
                                _errorCount++;
                            }
                        }
                    }
                    index = RoundUp(blockstart + length, 256);
                }
                else
                {
                    index += 256;
                }
            }
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                var paragraph = subtitle.Paragraphs[i];
                if (Math.Abs(paragraph.EndTime.TotalMilliseconds) < 0.001)
                {
                    var next = subtitle.Paragraphs[i + 1];
                    paragraph.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
            }
            if (subtitle.Paragraphs.Count > 0 && Math.Abs(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds) < 0.001)
            {
                var p = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1];
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
            }

            subtitle.Renumber();
        }

        // 1HD = first HD subtitle, 2SD = second SD subtitle
        public override List<string> AlternateExtensions => new List<string> { ".2HD", ".1SD", ".2SD" };
    }
}
