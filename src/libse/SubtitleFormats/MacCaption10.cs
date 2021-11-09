using Nikse.SubtitleEdit.Core.Cea708;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MacCaption10 : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".mcc";

        public override string Name => "MacCaption 1.0";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"File Format=MacCaption_MCC V1.0
///////////////////////////////////////////////////////////////////////////////////
// Computer Prompting and Captioning Company
// Ancillary Data Packet Transfer File
//
// Permission to generate this format is granted provided that
// 1. This ANC Transfer file format is used on an as-is basis and no warranty is given, and
// 2. This entire descriptive information text is included in a generated .mcc file.
//
// General file format:
// HH:MM:SS:FF(tab)[Hexadecimal ANC data in groups of 2 characters]
// Hexadecimal data starts with the Ancillary Data Packet DID (Data ID defined in S291M)
// and concludes with the Check Sum following the User Data Words.
// Each time code line must contain at most one complete ancillary data packet.
// To transfer additional ANC Data successive lines may contain identical time code.
// Time Code Rate=[24, 25, 30, 30DF, 50, 60]
//
// ANC data bytes may be represented by one ASCII character according to the following schema:
// G FAh 00h 00h
// H 2 x (FAh 00h 00h)
// I 3 x (FAh 00h 00h)
// J 4 x (FAh 00h 00h)
// K 5 x (FAh 00h 00h)
// L 6 x (FAh 00h 00h)
// M 7 x (FAh 00h 00h)
// N 8 x (FAh 00h 00h)
// O 9 x (FAh 00h 00h)
// P FBh 80h 80h
// Q FCh 80h 80h
// R FDh 80h 80h
// S 96h 69h
// T 61h 01h
// U E1h 00h 00h
// Z 00h
//
///////////////////////////////////////////////////////////////////////////////////");
            sb.AppendLine();
            sb.AppendLine("UUID=" + Guid.NewGuid().ToString().ToUpperInvariant());// UUID=9F6112F4-D9D0-4AAF-AA95-854710D3B57A
            sb.AppendLine("Creation Program=Subtitle Edit");
            sb.AppendLine($"Creation Date={DateTime.Now.ToLongDateString()}");
            sb.AppendLine($"Creation Time={DateTime.Now:HH:mm:ss}");
            sb.AppendLine("Time Code Rate=30DF"); // 30DF = 30 drop frame = 29.97
            sb.AppendLine();

            Configuration.Settings.General.CurrentFrameRate = GetFrameForCalculation(29.97);

            var timeCode = new TimeCode();
            if (subtitle.Paragraphs.Count > 0 && subtitle.Paragraphs[0].StartTime.Hours > 0)
            {
                timeCode = new TimeCode(subtitle.Paragraphs[0].StartTime.Hours, 0, 0, 0);
            }

            var i = 0;
            var counter = 0;
            int frameNo = 0;
            while (i < subtitle.Paragraphs.Count)
            {
                var p = subtitle.Paragraphs[i];
                if (timeCode.TotalMilliseconds < p.StartTime.TotalMilliseconds)
                {
                    // write empty lines (filler)
                    var empty = VancDataWriter.GenerateEmpty(counter++);
                    var s = $"{ToTimeCode(timeCode.TotalMilliseconds)}\t{CompressHex(empty)}";
                    sb.AppendLine(s);
                    frameNo = StepToNextFrame(frameNo, timeCode);
                    continue;
                }

                // write text lines
                var lines = VancDataWriter.GenerateLinesFromText(p.Text, counter);
                counter += lines.Length;
                foreach (var line in lines)
                {
                    sb.AppendLine($"{ToTimeCode(p.StartTime.TotalMilliseconds)}\t{CompressHex(line)}");
                }
                frameNo = StepToNextFrame(frameNo, timeCode);

                // filler between start/end text
                while (timeCode.TotalMilliseconds < p.EndTime.TotalMilliseconds)
                {
                    // write empty lines (filler)
                    var empty = VancDataWriter.GenerateEmpty(counter++);
                    sb.AppendLine($"{ToTimeCode(timeCode.TotalMilliseconds)}\t{CompressHex(empty)}");
                    frameNo = StepToNextFrame(frameNo, timeCode);
                }

                // write end text
                var endTimeText = VancDataWriter.GenerateTextInit(counter++);
                sb.AppendLine($"{ToTimeCode(p.EndTime.TotalMilliseconds)}\t{CompressHex(endTimeText)}");
                frameNo = StepToNextFrame(frameNo, timeCode);
                i++;
            }

            //int counter = 0;
            //for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            //{
            //    var p = subtitle.Paragraphs[i];
            //    if (i == 0)
            //    {
            //        var firstText = VancDataWriter.GenerateTextInit(counter++);
            //        sb.AppendLine($"{ToTimeCode(p.StartTime.TotalMilliseconds)}\t{CompressHex(firstText)}");
            //    }
            //    var lines = VancDataWriter.GenerateLinesFromText(p.Text, counter);
            //    counter += lines.Length;
            //    foreach (var line in lines)
            //    {
            //        sb.AppendLine($"{ToTimeCode(p.StartTime.TotalMilliseconds)}\t{CompressHex(line)}");
            //    }
            //    var endTimeText = VancDataWriter.GenerateTextInit(counter++);
            //    sb.AppendLine($"{ToTimeCode(p.EndTime.TotalMilliseconds)}\t{CompressHex(endTimeText)}");
            //}

            var lastLine = VancDataWriter.GenerateEmpty(counter);
            sb.AppendLine($"{ToTimeCode(timeCode.TotalMilliseconds)}\t{CompressHex(lastLine)}");

            return sb.ToString();
        }

        private static int StepToNextFrame(int frameNo, TimeCode timeCode)
        {
            frameNo++;
            if (frameNo > 29)
            {
                timeCode.TotalMilliseconds = new TimeCode(timeCode.Hours, timeCode.Minutes, timeCode.Seconds + 1, 0).TotalMilliseconds;
                frameNo = 0;
            }
            else
            {
                timeCode.TotalMilliseconds += FramesToMilliseconds(1);
            }

            return frameNo;
        }

        private static string ToTimeCode(double totalMilliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}:{MillisecondsToFramesMaxFrameRate(ts.Milliseconds):00}";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var timeCodeList = new List<TimeCode>();
            var header = new StringBuilder();
            var state = new CommandState();
            char[] splitChars = { ':', ';', ',' };
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var s = line.Trim();
                if (string.IsNullOrEmpty(s) || s.StartsWith("//", StringComparison.Ordinal) || s.StartsWith("File Format=MacCaption_MCC", StringComparison.Ordinal) || s.StartsWith("UUID=", StringComparison.Ordinal) ||
                    s.StartsWith("Creation Program=") || s.StartsWith("Creation Date=") || s.StartsWith("Creation Time=") ||
                    s.StartsWith("Code Rate=", StringComparison.Ordinal) || s.StartsWith("Time Code Rate=", StringComparison.Ordinal))
                {
                    header.AppendLine(line);
                }
                else
                {
                    var match = RegexTimeCodes.Match(s);
                    if (!match.Success)
                    {
                        continue;
                    }

                    var startTime = DecodeTimeCodeFrames(s.Substring(0, match.Length - 1), splitChars);
                    var text = GetText(timeCodeList.Count, s.Substring(match.Index + match.Length).Trim(), index == lines.Count - 1, state);
                    timeCodeList.Add(startTime);
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var p = new Paragraph(new TimeCode(timeCodeList[state.StartLineIndex].TotalMilliseconds), new TimeCode(startTime.TotalMilliseconds), text);
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        public static string GetText(int lineIndex, string input, bool flush, CommandState state)
        {
            var hexString = GetHex(input);
            var bytes = HexStringToByteArray(hexString);
            if (bytes.Length < 10)
            {
                return string.Empty;
            }

            var cea708 = new Smpte291M(bytes);
            return cea708.GetText(lineIndex, flush, state);
        }

        private static string GetHex(string input)
        {
            // ANC data bytes may be represented by one ASCII character according to the following schema:
            var dictionary = new Dictionary<char, string>
            {
                { 'G', "FA0000" },
                { 'H', "FA0000FA0000" },
                { 'I', "FA0000FA0000FA0000" },
                { 'J', "FA0000FA0000FA0000FA0000" },
                { 'K', "FA0000FA0000FA0000FA0000FA0000" },
                { 'L', "FA0000FA0000FA0000FA0000FA0000FA0000" },
                { 'M', "FA0000FA0000FA0000FA0000FA0000FA0000FA0000" },
                { 'N', "FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000" },
                { 'O', "FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000" },
                { 'P', "FB8080" },
                { 'Q', "FC8080" },
                { 'R', "FD8080" },
                { 'S', "9669" },
                { 'T', "6101" },
                { 'U', "E1000000" },
                { 'Z', "00" },
            };

            var sb = new StringBuilder();
            foreach (var ch in input)
            {
                if (dictionary.TryGetValue(ch, out var hexValue))
                {
                    sb.Append(hexValue);
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private static string CompressHex(string input)
        {
            return input
                .Replace("FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000", "O")
                .Replace("FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000", "N")
                .Replace("FA0000FA0000FA0000FA0000FA0000FA0000FA0000", "M")
                .Replace("FA0000FA0000FA0000FA0000FA0000FA0000", "L")
                .Replace("FA0000FA0000FA0000FA0000FA0000", "K")
                .Replace("FA0000FA0000FA0000FA0000", "J")
                .Replace("FA0000FA0000FA0000", "I")
                .Replace("FA0000FA0000", "H")
                .Replace("FA0000", "G")
                .Replace("FB8080", "P")
                .Replace("FC8080", "Q")
                .Replace("FD8080", "R")
                .Replace("9669", "S")
                .Replace("6101", "T")
                .Replace("E1000000", "U")
                .Replace("00", "Z");
        }

        private static byte[] HexStringToByteArray(string hex)
        {
            try
            {
                var numberChars = hex.Length;
                var bytes = new byte[numberChars / 2];
                for (var i = 0; i < numberChars - 1; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return bytes;
            }
            catch
            {
                return new byte[] { };
            }
        }
    }
}
