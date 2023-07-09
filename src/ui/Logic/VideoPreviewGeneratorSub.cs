using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic
{
    public class VideoPreviewGeneratorSub
    {
        public string Name { get; set; }
        public bool IsNew { get; set; }
        public string FileName { get; set; }
        public Subtitle Subtitle { get; set; }
        public SubtitleFormat SubtitleFormat { get; set; }
        public string Format { get; set; }
        public string Language { get; set; }
        public bool IsForced { get; set; }
        public bool IsDefault { get; set; }
        public object Tag { get; set; }
    }
}
