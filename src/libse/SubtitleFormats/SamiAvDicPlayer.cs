using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SamiAvDicPlayer : Sami
    {
        private static readonly Regex RegexAvDicPlayer = new Regex(@"<AVDicPlayer[^\s]*", RegexOptions.Compiled); //AVDicPlayer_TEDSYNⓒ_VER1.1

        public override string Name => "SAMI AVDicPlayer";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (new SamiModern().IsMine(lines, fileName))
            {
                return false;
            }

            var text = GetSamiFromAvDicPlayerText(lines);
            var subtitle = new Subtitle();
            base.LoadSubtitle(subtitle, text.SplitToLines(), fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var text = GetSamiFromAvDicPlayerText(lines);
            base.LoadSubtitle(subtitle, text.SplitToLines(), fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var text = base.ToText(subtitle, title);
            return text.Replace("<SYNC", "<AVDicPlayer_TEDSYNⓒ_VER1.1");
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
                text = RegexAvDicPlayer.Replace(text, "<SYNC");
            }
            return text;
        }

    }
}
