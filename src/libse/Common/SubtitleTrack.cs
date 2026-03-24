namespace Nikse.SubtitleEdit.Core.Common
{
    public class SubtitleTrack
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#4B6EAF"; // hex color for UI

        public SubtitleTrack() { }

        public SubtitleTrack(int index, string name, string color)
        {
            Index = index;
            Name = name;
            Color = color;
        }
    }
}
