using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
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
        public Subtitle OriginalSubtitle { get; set; }
        public string OriginalSubtitleFileName { get; set; }
        public List<Paragraph> RedoParagraphs { get; set; }
        public List<Paragraph> RedoParagraphsOriginal { get; set; }
        public int RedoLineIndex { get; set; }
        public int RedoLinePosition { get; set; }
        public int RedoLinePositionOriginal { get; set; }
        public string RedoFileName { get; set; }
        public DateTime RedoFileModified { get; set; }
        public string RedoOriginalFileName { get; set; }
        public int LineIndex { get; set; }
        public int LinePosition { get; set; }
        public int LinePositionOriginal { get; set; }

        public HistoryItem(int index, Subtitle subtitle, string description, string fileName, DateTime fileModified, string subtitleFormatFriendlyName, Subtitle originalSubtitle, string originalSubtitleFileName, int lineIndex, int linePosition, int linePositionOriginal)
        {
            Index = index;
            Timestamp = DateTime.Now;
            Subtitle = new Subtitle(subtitle);
            Description = description;
            FileName = fileName;
            FileModified = fileModified;
            SubtitleFormatFriendlyName = subtitleFormatFriendlyName;
            OriginalSubtitle = new Subtitle(originalSubtitle);
            OriginalSubtitleFileName = originalSubtitleFileName;
            LineIndex = lineIndex;
            LinePosition = linePosition;
            LinePositionOriginal = linePositionOriginal;
            RedoLineIndex = -1;
            RedoLinePosition = -1;
        }

        public string ToHHMMSS()
        {
            return $"{Timestamp.Hour:00}:{Timestamp.Minute:00}:{Timestamp.Second:00}";
        }
    }
}
