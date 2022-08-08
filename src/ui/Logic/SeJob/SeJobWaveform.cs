using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.SeJob
{
    [Serializable]
    public class SeJobWaveform
    {
        public double HighestPeak { get; set; }
        public List<short> PeakMins { get; set; }
        public List<short> PeakMaxs { get; set; }
        public double SampleRate { get; set; }

        public SeJobWaveform()
        {
            PeakMins = new List<short>();
            PeakMaxs = new List<short>();
        }
    }
}
