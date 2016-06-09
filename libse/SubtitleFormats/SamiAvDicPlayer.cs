using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SamiAvDicPlayer : Sami
    {

        public override string Name
        {
            get { return "SAMI AVDicPlayer"; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var text = GetSamiFromAvDicPlayerText(lines);
            var subtitle = new Subtitle();
            base.LoadSubtitle(subtitle, text.SplitToLines().ToList(), fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var text = GetSamiFromAvDicPlayerText(lines);
            base.LoadSubtitle(subtitle, text.SplitToLines().ToList(), fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var text = base.ToText(subtitle, title);
            text = text.Replace("<SYNC", "<AVDicPlayer_TEDSYNⓒ_VER1.1");
            return text;
        }

        private static string GetSamiFromAvDicPlayerText(List<string> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }
            var text = sb.ToString();
            if (text.Contains("<AVDicPlayer"))
            {
                Regex regex = new Regex(@"<AVDicPlayer[^\s]*", RegexOptions.Compiled); //AVDicPlayer_TEDSYNⓒ_VER1.1
                text = regex.Replace(text, "<SYNC");
            }
            return text;
        }

    }
}
