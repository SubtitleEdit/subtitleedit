using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class IsmtDfxp : SubtitleFormat
    {
        public override string Extension => ".ismt";

        public override string Name => "HBO GO";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length > 50_000_000)
                {
                    return false;
                }

                var buffer = new byte[12];
                int l = fs.Read(buffer, 0, buffer.Length);
                if (l != buffer.Length)
                {
                    return false;
                }

                // ftypisml, ftypdash, ftyppiff, stypiso6 or ?
                var str = Encoding.ASCII.GetString(buffer, 4, 8);
                if (!str.StartsWith("ftyp", StringComparison.Ordinal) && !str.StartsWith("styp", StringComparison.Ordinal))
                {
                    return false;
                }

                var sub = new Subtitle();
                LoadSubtitle(sub, lines, fileName);
                return sub.Paragraphs.Count > 0;
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var mp4Parser = new MP4Parser(fileName);
            var dfxpStrings = mp4Parser.GetMdatsAsStrings();
            SubtitleFormat format = new TimedText10();
            foreach (var xmlAsString in dfxpStrings)
            {
                try
                {
                    if (xmlAsString.Length < 80)
                    {
                        continue;
                    }

                    var sub = new Subtitle();
                    var mdatLines = xmlAsString.SplitToLines();
                    format = sub.ReloadLoadSubtitle(mdatLines, null, format);
                    if (sub.Paragraphs.Count == 0)
                    {
                        continue;
                    }

                    // merge lines with same time codes
                    sub = Forms.MergeLinesWithSameTimeCodes.Merge(sub, new List<int>(), out _, true, false, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());

                    // adjust to last exisiting sub
                    var lastSub = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
                    if (lastSub != null && sub.Paragraphs.Count > 0 && lastSub.StartTime.TotalMilliseconds > sub.Paragraphs[0].StartTime.TotalMilliseconds)
                    {
                        sub.AddTimeToAllParagraphs(lastSub.EndTime.TimeSpan);
                    }

                    subtitle.Paragraphs.AddRange(sub.Paragraphs);
                }
                catch
                {
                    _errorCount++;
                }
            }

            var merged = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(subtitle, false, 250);
            if (merged.Paragraphs.Count < subtitle.Paragraphs.Count)
            {
                subtitle.Paragraphs.Clear();
                subtitle.Paragraphs.AddRange(merged.Paragraphs);
            }

            subtitle.Renumber();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported";
        }
    }
}
