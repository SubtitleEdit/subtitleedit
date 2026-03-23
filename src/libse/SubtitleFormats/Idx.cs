using Nikse.SubtitleEdit.Core.Common;
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

        private readonly Hashtable _nonTimeCodes = new Hashtable();

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
            if (subtitle.OriginalFormat != null)
            {
                foreach (DictionaryEntry de in ((Idx)subtitle.OriginalFormat)._nonTimeCodes)
                {
                    tempNonTimeCodes.Add(de.Key, de.Value);
                }
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
                    if (subtitle.Paragraphs.Count == 0 ||
                        !int.TryParse(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text, out var place))
                    {
                        place = -1;
                    }

                    if (_nonTimeCodes.ContainsKey(place))
                    {
                        _nonTimeCodes[place] += Environment.NewLine + line;
                    }
                    else
                    {
                        _nonTimeCodes.Add(place, line);
                    }
                }
            }
        }

        private static Paragraph GetParagraph(string[] parts)
        {
            // timestamp: 00:00:01:401, filepos: 000000000
            if (parts.Length == 7)
            {
                if (int.TryParse(parts[1], out var hours) &&
                    int.TryParse(parts[2], out var minutes) &&
                    int.TryParse(parts[3], out var seconds) &&
                    int.TryParse(parts[4], out var milliseconds))
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
