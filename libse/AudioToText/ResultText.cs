using System;

namespace Nikse.SubtitleEdit.Core.AudioToText.PhocketSphinx
{
    public class ResultText
    {
        public string Text { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
        public double Confidence { get; set; }
    }
}
