namespace Nikse.SubtitleEdit.Core.Common
{
    public class VideoInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double TotalMilliseconds { get; set; }
        public double TotalSeconds { get; set; }
        public double FramesPerSecond { get; set; }
        public double TotalFrames { get; set; }
        public string VideoCodec { get; set; }
        public string FileType { get; set; }
        public bool Success { get; set; }
    }
}
