namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class DefineWindow : ICea708Command
    {
        public static readonly int IdStart = 0x98;
        public static readonly int IdEnd = 0x9F;

        public int LineIndex { get; set; }

        public const int AnchorUpperLeft = 0;
        public const int AnchorUpperCenter = 1;
        public const int AnchorUpperRight = 2;
        public const int AnchorMiddleLeft = 3;
        public const int AnchorMiddleCenter = 4;
        public const int AnchorMiddleRight = 5;
        public const int AnchorLowerLeft = 6;
        public const int AnchorLowerCenter = 7;
        public const int AnchorLowerRight = 8;

        public int Id { get; set; }
        public int Priority { get; set; }
        public bool ColumnLock { get; set; }
        public bool RowLock { get; set; }
        public bool Visible { get; set; }
        public int AnchorVertical { get; set; }
        public bool RelativePositioning { get; set; }
        public int AnchorHorizontal { get; set; }

        /// <summary>
        /// The range is 0-15.
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// The range is 0-31 for 4:3 streams, and 0-41 for 16:9 streams.
        /// </summary>
        public int RowCount { get; set; }

        public int AnchorId { get; set; }
        public int PenStyleId { get; set; }
        public int WindowStyleId { get; set; }

        public DefineWindow(int numberOfLines)
        {
            Id = 0x99;
            AnchorId = 0;
            AnchorVertical = 65;
            AnchorHorizontal = 70;
            ColumnCount = 41;
            RowCount = numberOfLines - 1;
            RowLock = false;
            ColumnLock = false;
            PenStyleId = 1;
            Priority = 0;
            RelativePositioning = false;
            Visible = false;
            WindowStyleId = 2;
        }

        public DefineWindow(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;

            Id = bytes[index];
            Priority = bytes[index + 1] & 0b00000111;
            ColumnLock = (bytes[index + 1] & 0b00001000) > 0;
            RowLock = (bytes[index + 1] & 0b00010000) > 0;
            Visible = (bytes[index + 1] & 0b00100000) > 0;

            AnchorVertical = bytes[index + 2] & 0b01111111;
            RelativePositioning = (bytes[index + 2] & 0b10000000) > 0;

            AnchorHorizontal = bytes[index + 3];

            RowCount = bytes[index + 4] & 0b00001111;
            AnchorId = bytes[index + 4] >> 4;

            ColumnCount = bytes[index + 5] & 0b00111111;

            PenStyleId = bytes[index + 6] & 0b00000111;
            WindowStyleId = bytes[index + 6] >> 3;
        }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)Id,
                (byte)(Priority |
                       (ColumnLock ? 0b00001000 : 0) |
                       (RowLock ? 0b00010000 : 0) |
                       (Visible ? 0b00100000 : 0)),
                (byte)(AnchorVertical |
                       (RelativePositioning ? 0b10000000 : 0)),
                (byte)AnchorHorizontal,
                (byte)(RowCount |
                       (AnchorId << 4)),
                (byte)ColumnCount,
                (byte)(PenStyleId |
                       (WindowStyleId << 3)),
            };
        }
    }
}
