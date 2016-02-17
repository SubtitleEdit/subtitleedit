using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class F4Rtf : F4Text
    {
        public override string Extension
        {
            get { return ".rtf"; }
        }

        public override string Name
        {
            get { return "F4 Rich Text Format"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return ToF4Text(subtitle).ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf", StringComparison.Ordinal))
                return;

            LoadF4TextSubtitle(subtitle, rtf.FromRtf());
        }

    }
}
