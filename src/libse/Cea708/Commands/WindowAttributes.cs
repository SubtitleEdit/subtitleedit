namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class WindowAttributes
    {
        public int Justify { get; set; }
        public int PrintDirection { get; set; }
        public int ScrollDirection { get; set; }
        public int Wordwrap { get; set; }
        public int DisplayEffect { get; set; }
        public int EffectDirection { get; set; }
        public int EffectSpeed { get; set; }
        public int FillColor { get; set; }
        public int FillOpacity { get; set; }
        public int BorderType { get; set; }
        public int BorderColor { get; set; }

        public WindowAttributes()
        {

        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)0
            };
        }

    }
}
