namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CcData
    {
        public bool Valid { get; set; }
        public int Type { get; set; }
        public byte Data1 { get; set; }
        public byte Data2 { get; set; }

        public byte[] GetBytes()
        {
            return new[]
            {
                (byte)(0b11111000 | (Type & 0b00000011) | (Valid ? 0b00000100 : 0)),
                Data1,
                Data2
            };
        }
    }
}
