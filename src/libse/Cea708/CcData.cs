namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CcData
    {
        public bool Valid { get; set; }
        public int Type { get; set; }
        public byte Data1 { get; set; }
        public byte Data2 { get; set; }

        public string GetCcType()
        {
            switch (Type)
            {
                case 0b00000000:
                    return "NTSC line 21 field 1 Closed Captions";
                case 0b00000001:
                    return "NTSC line 21 field 2 Closed Captions";
                case 0b00000010:
                    return "DTVCC Channel Packet Data";
                case 0b00000011:
                    return "DTVCC Channel Packet Start";
                default:
                    return "unknown";
            }
        }

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
