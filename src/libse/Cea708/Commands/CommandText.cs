namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class CommandText
    {
        public string Text { get; set; }
        public Window[] Windows { get; set; }
        public PenAttributes PenAttributes { get; set; }
        public PenColor PenColor { get; set; }
        public PenLocation PenLocation { get; set; }
    }
}
