namespace Nikse.SubtitleEdit.Core.Cea708.Commands
{
    public class PenLocation : CommandBase
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public PenLocation(int lineIndex, byte[] bytes, int index)
        {
            LineIndex = lineIndex;

            Row = bytes[index] & 0b00001111;
            Column = bytes[index + 1] & 0b00111111;
        }

        public byte[] GetBytes()
        {
            return new[] { (byte)Row, (byte)Column };
        }
    }
}
