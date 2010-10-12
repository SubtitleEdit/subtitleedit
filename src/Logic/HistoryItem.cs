using System;

namespace Nikse.SubtitleEdit.Logic
{
    public class HistoryItem
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public DateTime FileModified { get; set; }
        public Subtitle Subtitle { get; set; }
        public string SubtitleFormatFriendlyName { get; set; }

        public HistoryItem(int index, Subtitle subtitle, string description, string fileName, DateTime fileModified, string subtitleFormatFriendlyName)
        {
            Index = index;
            Timestamp = DateTime.Now;
            Subtitle = new Subtitle(subtitle);
            Description = description;
            FileName = fileName;
            FileModified = fileModified;
            SubtitleFormatFriendlyName = subtitleFormatFriendlyName;
        }
    }
}
