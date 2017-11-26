using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class IsmtDfxp : SubtitleFormat
    {
        public override string Extension => ".ismt";

        public override string Name => "HBO GO";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buffer = new byte[12];
                int l = fs.Read(buffer, 0, buffer.Length);
                if (l != buffer.Length)
                    return false;
                var str = Encoding.ASCII.GetString(buffer, 4, 8);
                if (str != "ftypisml")
                    return false;
            }
            return true;            
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var mp4Parser = new MP4Parser(fileName);
            var dfxpStrings = mp4Parser.GetMdatsAsStrings();
            foreach (var xmlAsString in dfxpStrings)
            {
                try
                {
                    var sub = new Subtitle();
                    var format = new TimedText();
                    sub.ReloadLoadSubtitle(xmlAsString.SplitToLines(), null, format);

                    if (sub.Paragraphs.Count == 0)
                        continue;

                    // merge lines with same time codes
                    int numberOfMerges;
                    sub = Forms.MergeLinesWithSameTimeCodes.Merge(sub, new List<int>(), out numberOfMerges, true, false, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());

                    // adjust to last exisiting sub
                    var lastSub = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
                    if (lastSub != null)
                        sub.AddTimeToAllParagraphs(lastSub.EndTime.TimeSpan);

                    subtitle.Paragraphs.AddRange(sub.Paragraphs);
                }
                catch
                {
                    _errorCount++;
                }
            }
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported";
        }
    }
}
