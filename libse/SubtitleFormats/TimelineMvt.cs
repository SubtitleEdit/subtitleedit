using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Timeline - THE MOVIE TITRE EDITOR - http://www.pld.ttu.ee/~priidu/timeline/ by priidu@pld.ttu.ee
    /// </summary>
    public class TimeLineMvt : SubtitleFormat
    {
        public override string Extension => ".mvt";

        public override string Name => "Timeline mvt";

        public override string ToText(Subtitle subtitle, string title)
        {
            return string.Empty;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var bytes = File.ReadAllBytes(fileName);
            if (bytes.Length < 100 || bytes[0] != 0x54 || bytes[1] != 0x50 || bytes[2] != 0x46)
            {
                return;
            }

            // title
            int index = 9;
            while (index < bytes.Length && bytes[index] != 0x6)
            {
                index++;
            }
            if (index + 1 >= bytes.Length)
            {
                return;
            }
            string title = Encoding.UTF8.GetString(bytes, 9, index - 9);

            // language1
            index += 2;
            int start = index;
            while (index < bytes.Length && bytes[index] != 0x6)
            {
                index++;
            }
            if (index + 1 >= bytes.Length)
            {
                return;
            }
            string language1 = Encoding.UTF8.GetString(bytes, start, index - start);

            // language2
            index += 2;
            start = index;
            while (index < bytes.Length && bytes[index] != 0x6)
            {
                index++;
            }
            if (index + 1 >= bytes.Length)
            {
                return;
            }
            string language2 = Encoding.UTF8.GetString(bytes, start, index - start);

            Encoding encoding1 = GetEncodingFromLanguage(language1);
            Encoding encoding2 = GetEncodingFromLanguage(language2);

            _errorCount = 0;
            while (index < bytes.Length - 20)
            {
                if (bytes[index] == 5 && bytes[index + 1] == 0 && bytes[index + 2] == 0) // find subtitle
                {
                    // time codes
                    int timeCodeIndexStart = index + 4;
                    int timeCodeIndexEnd = index + 15;
                    index += 22;
                    while (index < bytes.Length && bytes[index] != 0x6)
                    {
                        index++;
                    }
                    index += 2;
                    while (index < bytes.Length && bytes[index] == 0x6)
                    {
                        index += 2;
                    }

                    if (index < bytes.Length - 3)
                    {
                        // first line
                        start = index;
                        while (index < bytes.Length && bytes[index] != 0x6)
                        {
                            index++;
                        }
                        if (index < bytes.Length - 3)
                        {
                            string text1 = encoding1.GetString(bytes, start, index - start);
                            index += 2;

                            // second line
                            start = index;
                            while (index < bytes.Length && bytes[index] != 0x5)
                            {
                                index++;
                            }
                            if (index + 1 < bytes.Length)
                            {
                                string text2 = encoding2.GetString(bytes, start, index - start);
                                var p = new Paragraph
                                {
                                    Text = text1 + Environment.NewLine + text2,
                                    StartTime = { TotalMilliseconds = GetTimeCode(bytes, timeCodeIndexStart) },
                                    EndTime = { TotalMilliseconds = GetTimeCode(bytes, timeCodeIndexEnd) }
                                };
                                subtitle.Paragraphs.Add(p);
                                index--;
                            }
                        }
                    }
                }
                index++;
            }
            subtitle.Renumber();
        }

        private static double GetTimeCode(byte[] bytes, int timeCodeIndex)
        {
            //TODO: figure out how to get time code from these 7 bytes!
            if (bytes == null || bytes.Length < timeCodeIndex + 8)
            {
                return 0;
            }
            return ((bytes[timeCodeIndex + 5] << 24) + (bytes[timeCodeIndex + 4] << 16) + (bytes[timeCodeIndex + 3] << 8) + (bytes[timeCodeIndex + 2])) / 1800.0;
        }

        private static Encoding GetEncodingFromLanguage(string language)
        {
            if (language == "Russian")
            {
                return Encoding.GetEncoding(1251);
            }

            if (language == "Estonian" || language == "Latvian" || language == "Lithuanian")
            {
                return Encoding.GetEncoding(1257);
            }

            return Encoding.GetEncoding(1252);
        }

    }
}
