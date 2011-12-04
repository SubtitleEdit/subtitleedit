using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class F4Rtf : F4Text
    {
        public override string Extension
        {
            get { return ".rtf"; }
        }

        public override string Name
        {
            get { return "F4 Rich Text Format"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!fileName.ToLower().EndsWith(Extension))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
            rtBox.Text = ToF4Text(subtitle, title);
            return rtBox.Rtf;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
            rtBox.Rtf = sb.ToString();
            LoadF4TextSubtitle(subtitle, rtBox.Text);
        }
    }
}
