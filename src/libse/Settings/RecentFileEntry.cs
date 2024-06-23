namespace Nikse.SubtitleEdit.Core.Settings
{
    public class RecentFileEntry
    {
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string VideoFileName { get; set; }
        public int AudioTrack { get; set; }
        public int FirstVisibleIndex { get; set; }
        public int FirstSelectedIndex { get; set; }
        public long VideoOffsetInMs { get; set; }
        public bool VideoIsSmpte { get; set; }
    }
}