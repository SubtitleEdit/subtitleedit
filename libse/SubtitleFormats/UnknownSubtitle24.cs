using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class UnknownSubtitle24 : UnknownSubtitle18
    {
        public override string Extension
        {
            get { return ".rtf"; }
        }

        public override string Name
        {
            get { return "Unknown 24"; }
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
            System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
            rtBox.Text = base.ToText(subtitle, title);
            string rtf = rtBox.Rtf;
            rtBox.Dispose();
            return rtf;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);

            string rtf = sb.ToString().Trim();
            if (!rtf.StartsWith("{\\rtf"))
                return;

            string[] arr = null;
            var rtBox = new System.Windows.Forms.RichTextBox();
            try
            {
                rtBox.Rtf = rtf;
                arr = rtBox.Text.Replace("\r", string.Empty).Split('\n');
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                return;
            }
            finally
            {
                rtBox.Dispose();
            }

            lines = new List<string>();
            foreach (string s in arr)
                lines.Add(s);
            base.LoadSubtitle(subtitle, lines, fileName);
        }
    }
}
