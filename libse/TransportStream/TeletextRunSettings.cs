using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.TransportStream
{
    public class TeletextRunSettings
    {
        public int PageNumber { get; set; }
        public int PageNumberBcd { get; set; }
        public int TransmissionMode { get; set; }
        public HashSet<int> PageNumbersInt { get; set; } = new HashSet<int>();
        public HashSet<int> PageNumbersBcd { get; set; } = new HashSet<int>();
        public Dictionary<int, Paragraph> PageNumberAndParagraph { get; set; } = new Dictionary<int, Paragraph>();
    }
}
