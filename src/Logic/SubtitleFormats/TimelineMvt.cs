using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// Timeline - THE MOVIE TITLE EDITOR - http://www.pld.ttu.ee/~priidu/timeline/
    /// </summary>
    public class TimeLineMvt : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".mvt"; }
        }

        public override string Name
        {
            get { return "Timeline mtv"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

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
            Console.WriteLine(title);

            // language1
            index+=2;
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
            Console.WriteLine(language1);

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
            Console.WriteLine(language2);

            Encoding encoding1 = GetEncodingFromLanguage(language1);
            Encoding encoding2 = GetEncodingFromLanguage(language2);

            _errorCount = 0;
            while (index < bytes.Length - 10)
            {
                if (bytes[index] == 5 && bytes[index + 1] == 0 && bytes[index + 2] == 0) // find subtitle
                {
                    // codes...
                    int frames = bytes[index + 8] * 256; // + bytes[index + 9] * 256;
                    index += 20;
                    while (index < bytes.Length && bytes[index] != 0x6)
                    {
                        index++;
                    }
                    index += 2;
                    if (index < bytes.Length && bytes[index] == 0x6)
                    {
                        index+=2;
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
                                var p = new Paragraph { Text = text1 + Environment.NewLine + text2 };
                                p.StartTime.TotalMilliseconds = FramesToMilliseconds(frames);
                                p.EndTime.TotalMilliseconds = frames;
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

        private Encoding GetEncodingFromLanguage(string language)
        {
            if (language == "Russian")
                return Encoding.GetEncoding(1251);
            if (language == "Estonian" || language == "Latvian"  || language == "Lithuanian")
                return Encoding.GetEncoding(1257);
            return Encoding.GetEncoding(1252);
        }

    }
}