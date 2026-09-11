using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle37 : UnknownSubtitle36
    {
        public override string Extension => ".rtf";

        public override string Name => "Unknown 37";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // UnknownSubtitle36.IsMine rejects RTF input outright, so this RTF-wrapped sibling
            // could never be detected: unwrap first and run the plain reader on the result.
            var rtf = string.Join(Environment.NewLine, lines).Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return false;
            }

            var subtitle = new Subtitle();
            base.LoadSubtitle(subtitle, rtf.FromRtf().SplitToLines(), fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return base.ToText(subtitle, title).ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return;
            }

            var list = rtf.FromRtf().SplitToLines();
            base.LoadSubtitle(subtitle, list, fileName);
        }

    }
}
