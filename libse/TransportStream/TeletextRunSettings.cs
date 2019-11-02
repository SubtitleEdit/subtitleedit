using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    public class TeletextRunSettings
    {
        public Dictionary<int, Paragraph> PageNumberAndParagraph { get; set; } = new Dictionary<int, Paragraph>();
    }
}
