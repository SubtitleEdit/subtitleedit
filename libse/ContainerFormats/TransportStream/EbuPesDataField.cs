namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class EbuPesDataFieldText
    {
        public bool FieldParity { get; set; }
        public int LineOffset { get; set; }
        public int FramingCode { get; set; }
        public int MagazineAndPacketAddress { get; set; }
        public byte[] DataBlock { get; set; }

        public EbuPesDataFieldText(byte[] buffer, int index)
        {
            FieldParity = (buffer[index] & 0b00100000) > 0;
            LineOffset = buffer[index] & 0b00011111;
            FramingCode = buffer[index + 1];
            MagazineAndPacketAddress = Helper.GetEndianWord(buffer, index + 2);
        }
    }
}
