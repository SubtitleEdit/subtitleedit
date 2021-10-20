using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// .CHK subtitle file format - 128 bytes blocks, first byte in block is id (01==text)
    /// </summary>
    public class CombinedXml : SubtitleFormat
    {
        private const string findString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

        public override string Extension => ".xml";

        public const string NameOfFormat = "Combined xml";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var searchText = sb.ToString();
            var idx = searchText.IndexOf(findString, StringComparison.Ordinal);
            if (idx < 0)
            {
                return false;
            }

            if (searchText.Length > idx + findString.Length + 1 && searchText.IndexOf(findString, idx, StringComparison.Ordinal) >= 0)
            {
                var subtitle = new Subtitle();
                LoadSubtitle(subtitle, lines, fileName);
                return subtitle.Paragraphs.Count > _errorCount;
            }

            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            var sb = new StringBuilder();
            lines.ForEach(line => sb.AppendLine(line));
            var searchText = sb.ToString();
            var idx = searchText.IndexOf(findString, StringComparison.Ordinal);
            var start = idx;
            var parts = new List<string>();
            while (idx >= 0)
            {
                if (idx > start)
                {
                    var part = searchText.Substring(start, idx - start).Trim();
                    parts.Add(part);
                    start = idx;
                }

                if (idx + findString.Length >= searchText.Length)
                {
                    break;
                }

                idx = searchText.IndexOf(findString, idx + findString.Length, StringComparison.Ordinal);
                if (idx < 0)
                {
                    var part = searchText.Substring(start).Trim();
                    parts.Add(part);
                }
            }

            var formats = new List<SubtitleFormat>
            {
                new TimedText10(),
                new NetflixTimedText(),
                new ItunesTimedText(),
            };

            foreach (var xml in parts)
            {
                var xmlLines = xml.SplitToLines();
                foreach (var format in formats)
                {
                    if (format.IsMine(xmlLines, fileName))
                    {
                        var sub = new Subtitle();
                        format.LoadSubtitle(sub, xmlLines, fileName);
                        subtitle.Paragraphs.AddRange(sub.Paragraphs);
                    }
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
    }
}
