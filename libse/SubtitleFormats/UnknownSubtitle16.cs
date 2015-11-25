using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle16 : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".cip"; }
        }

        public override string Name
        {
            get { return "Unknown 16"; }
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
            var u52 = new UnknownSubtitle52();
            return u52.ToText(subtitle, title).ToRtf();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            if (lines.Count == 0 || !lines[0].TrimStart().StartsWith("{\\rtf1"))
                return;

            // load as text via RichTextBox
            var text = new StringBuilder();
            foreach (string s in lines)
                text.AppendLine(s);
            
            var lines2 = text.ToString().FromRtf().SplitToLines().ToList();
            var u52 = new UnknownSubtitle52();
            u52.LoadSubtitle(subtitle, lines2, fileName);
            _errorCount = u52.ErrorCount;
        }
    }
}
