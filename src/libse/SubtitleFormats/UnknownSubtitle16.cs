using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle16 : SubtitleFormat
    {
        public override string Extension => ".cip";

        public override string Name => "Unknown 16";

        public override string ToText(Subtitle subtitle, string title)
        {
            var u52 = new UnknownSubtitle52();
            return u52.ToText(subtitle, title).ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            if (lines.Count == 0 || !lines[0].TrimStart().StartsWith("{\\rtf1"))
            {
                return;
            }

            // load as text via RichTextBox
            var text = new StringBuilder();
            foreach (string s in lines)
            {
                text.AppendLine(s);
            }

            var lines2 = text.ToString().FromRtf().SplitToLines();
            var u52 = new UnknownSubtitle52();
            u52.LoadSubtitle(subtitle, lines2, fileName);
            _errorCount = u52.ErrorCount;
        }
    }
}
