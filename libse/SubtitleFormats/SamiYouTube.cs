using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class SamiYouTube : Sami
    {
        public override string Name
        {
            get { return "SAMI YouTube"; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }
    }
}