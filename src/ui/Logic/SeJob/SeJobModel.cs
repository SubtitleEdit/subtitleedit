using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.SeJob
{
    [Serializable]
    public class SeJobModel
    {
        public string JobId { get; set; }
        public string JobName { get; set; }
        public string Version { get; set; }
        public string Message { get; set; }
        public string SubtitleFileName { get; set; }
        public string SubtitleFileFormat { get; set; }
        public string SubtitleFileFormatOriginal { get; set; }
        public string SubtitleContent { get; set; }
        public string SubtitleFileNameOriginal { get; set; }
        public string SubtitleContentOriginal { get; set; }
        public string VideoStreamingUrl { get; set; }
        public string VideoHash { get; set; }
        public SeJobRules Rules { get; set; }
        public List<SeJobBookmark> Bookmarks { get; set; }
        public List<double> ShotChanges { get; set; }
        public SeJobWaveform Waveform { get; set; }
    }
}
