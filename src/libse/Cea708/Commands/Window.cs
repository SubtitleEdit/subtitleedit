namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class Window
    {
        public bool Active { get; set; }

        public int WindowId { get; set; }
        public int Priority { get; set; }
        public bool ColumnLock { get; set; }
        public bool RowLock { get; set; }
        public bool Visible { get; set; }
        public int AnchorVertical { get; set; }
        public bool RelativePositioning { get; set; }
        public int AnchorHorizontal { get; set; }
        public int RowCount { get; set; }
        public int AnchorId { get; set; }
        public int ColumnCount { get; set; }
        public int PenStyleId { get; set; }
        public int WindowStyleId { get; set; }

        public Window()
        {
            //TODO: set default values...
        }

        public Window(byte[] bytes, int index)
        {
            Priority = bytes[index] & 0b00000111;
            ColumnLock = (bytes[index] & 0b00001000) > 0;
            RowLock = (bytes[index] & 0b00010000) > 0;
            Visible = (bytes[index] & 0b00100000) > 0;
            AnchorVertical = bytes[index + 1] & 0b01111111;
            RelativePositioning = (bytes[index + 1] & 0b10000000) > 0;
            AnchorHorizontal = bytes[index + 2];
            RowCount = bytes[index + 3] & 0b00001111;
            AnchorId = bytes[index + 3] >> 4;
            ColumnCount = bytes[index + 3] & 0b00111111;
            PenStyleId = bytes[index + 4] & 0b00000111;
            WindowStyleId = bytes[index + 4] >> 3;
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
