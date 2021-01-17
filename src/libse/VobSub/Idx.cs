using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class Idx
    {
        public readonly List<IdxParagraph> IdxParagraphs = new List<IdxParagraph>();
        public readonly List<Color> Palette = new List<Color>();
        public readonly List<string> Languages = new List<string>();

        private static readonly Regex TimeCodeLinePattern = new Regex(@"^timestamp: \d+:\d+:\d+:\d+, filepos: [\dabcdefABCDEF]+$", RegexOptions.Compiled);

        public Idx(string fileName)
            : this(FileUtil.ReadAllLinesShared(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName)).ToList())
        {
        }

        public Idx(List<string> lines)
        {
            int languageIndex = 0;
            foreach (string line in lines)
            {
                if (TimeCodeLinePattern.IsMatch(line))
                {
                    IdxParagraph p = GetTimeCodeAndFilePosition(line);
                    if (p != null)
                    {
                        IdxParagraphs.Add(p);
                    }
                }
                else if (line.StartsWith("palette:", StringComparison.OrdinalIgnoreCase) && line.Length > 10)
                {
                    string s = line.Substring("palette:".Length + 1);
                    string[] colors = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string hex in colors)
                    {
                        Palette.Add(HexToColor(hex));
                    }
                }
                else if (line.StartsWith("id:", StringComparison.OrdinalIgnoreCase) && line.Length > 4)
                {
                    //id: en, index: 1
                    //id: es, index: 2
                    var parts = line.Split(new[] { ':', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1)
                    {
                        string twoLetterLanguageId = parts[1];
                        string languageName = DvdSubtitleLanguage.GetLocalLanguageName(twoLetterLanguageId);
                        if (parts.Length > 3 && parts[2].Equals("index", StringComparison.OrdinalIgnoreCase))
                        {
                            int index;
                            if (int.TryParse(parts[3], out index))
                            {
                                languageIndex = index;
                            }
                        }
                        // Use U+200E (LEFT-TO-RIGHT MARK) to support right-to-left scripts
                        Languages.Add(string.Format("{0} \x200E(0x{1:x})", languageName, languageIndex + 32));
                        languageIndex++;
                    }
                }
            }
        }

        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#').Trim();
            if (hex.Length == 6)
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                return Color.FromArgb(r, g, b);
            }
            else if (hex.Length == 8)
            {
                int a = Convert.ToInt32(hex.Substring(0, 2), 16);
                int r = Convert.ToInt32(hex.Substring(2, 2), 16);
                int g = Convert.ToInt32(hex.Substring(4, 2), 16);
                int b = Convert.ToInt32(hex.Substring(6, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }
            return Color.Red;
        }

        private static IdxParagraph GetTimeCodeAndFilePosition(string line)
        {
            // timestamp: 00:00:01:401, filepos: 000000000
            string[] parts = line.Split(new[] { ',', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 7)
            {
                int hours;
                int minutes;
                int seconds;
                int milliseconds;
                if (int.TryParse(parts[1], out hours) &&
                    int.TryParse(parts[2], out minutes) &&
                    int.TryParse(parts[3], out seconds) &&
                    int.TryParse(parts[4], out milliseconds))
                {
                    return new IdxParagraph(new TimeSpan(0, hours, minutes, seconds, milliseconds), Convert.ToInt64(parts[6].Trim(), 16));
                }
            }
            return null;
        }

    }
}
