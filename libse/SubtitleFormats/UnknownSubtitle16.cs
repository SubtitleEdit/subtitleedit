using System.Collections.Generic;
using System.Text;

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
            using (var rtBox = new System.Windows.Forms.RichTextBox { Text = u52.ToText(subtitle, title) })
            {
                return rtBox.Rtf;
            }
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
            using (var rtBox = new System.Windows.Forms.RichTextBox())
            {
                rtBox.Rtf = text.ToString();
                var lines2 = new List<string>();
                foreach (string line in rtBox.Lines)
                    lines2.Add(line);
                var u52 = new UnknownSubtitle52();
                u52.LoadSubtitle(subtitle, lines2, fileName);
                _errorCount = u52.ErrorCount;
            }
        }
    }
}
