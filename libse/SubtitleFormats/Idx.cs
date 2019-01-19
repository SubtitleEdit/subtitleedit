using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    // TODO: Working on added edit capabilities for idx files...
    public class Idx : SubtitleFormat
    {
        // timestamp: 00:00:01:401, filepos: 000000000
        private static readonly Regex RegexTimeCodes = new Regex(@"^timestamp: \d+:\d+:\d+:\d+, filepos: [\dabcdefABCDEF]+$", RegexOptions.Compiled);

        public Hashtable NonTimeCodes = new Hashtable();

        public override string Extension => ".idx";

        public override string Name => "VobSub index file";

        public override bool IsMine(List<string> lines, string fileName)
        {
            int subtitleCount = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("timestamp: ", StringComparison.Ordinal))
                {
                    if (++subtitleCount > 10)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            // timestamp: 00:00:01:401, filepos: 000000000

            const string paragraphWriteFormat = "timestamp: {0}, filepos: {1}";

            var tempNonTimeCodes = new Hashtable();
            foreach (DictionaryEntry de in (subtitle.OriginalFormat as Idx).NonTimeCodes)
            {
                tempNonTimeCodes.Add(de.Key, de.Value);
            }

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var removeList = new List<int>();
                foreach (DictionaryEntry de in tempNonTimeCodes)
                {
                    if (Convert.ToInt32(de.Key) < Convert.ToInt32(p.Text))
                    {
                        sb.AppendLine(de.Value.ToString());
                        removeList.Add(Convert.ToInt32(de.Key));
                    }
                }

                foreach (int key in removeList)
                {
                    tempNonTimeCodes.Remove(key);
                }

                sb.AppendLine(string.Format(paragraphWriteFormat, p.StartTime, p.Text));
            }
            foreach (DictionaryEntry de in tempNonTimeCodes)
            {
                sb.AppendLine(de.Value.ToString());
            }

            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChars = { ',', ':' };
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    Paragraph p = GetParagraph(line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries));
                    if (p != null)
                    {
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    int place;
                    if (subtitle.Paragraphs.Count == 0 ||
                        !int.TryParse(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text, out place))
                    {
                        place = -1;
                    }

                    if (NonTimeCodes.ContainsKey(place))
                    {
                        NonTimeCodes[place] += Environment.NewLine + line;
                    }
                    else
                    {
                        NonTimeCodes.Add(place, line);
                    }
                }
            }
        }

        private static Paragraph GetParagraph(string[] parts)
        {
            // timestamp: 00:00:01:401, filepos: 000000000
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
                    return new Paragraph
                    {
                        StartTime = { TimeSpan = new TimeSpan(0, hours, minutes, seconds, milliseconds) },
                        Text = parts[6]
                    };
                }
            }
            return null;
        }

    }
}
