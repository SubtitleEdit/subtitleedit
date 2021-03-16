namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CcServiceInfoSection
    {
        public byte Id { get; set; }
        public bool Start { get; set; }
        public bool Change { get; set; }
        public bool Complete { get; set; }
        public int ServiceCount { get; set; }
        public CcServiceInfoSectionElement[] CcServiceInfoSectionElement { get; set; }

        public int GetLength()
        {
            return 2 + ServiceCount * 7;
        }

        public CcServiceInfoSection(byte[] bytes, int index)
        {
            Id = bytes[index];

            Start = (bytes[index + 1] & 0b01000000) > 0;
            Change = (bytes[index + 1] & 0b00100000) > 0;
            Complete = (bytes[index + 1] & 0b00010000) > 0;
            ServiceCount = bytes[index + 1] & 0b00001111;

            CcServiceInfoSectionElement = new CcServiceInfoSectionElement[ServiceCount];
            for (int i = 0; i < ServiceCount; i++)
            {
                CcServiceInfoSectionElement[i] = new CcServiceInfoSectionElement
                {
                    CaptionServiceNumber = bytes[index + i * 7 + 1] & 0b00011111,
                    ServiceDataByte =
                    {
                        [0] = bytes[index + i * 7 + 3],
                        [1] = bytes[index + i * 7 + 4],
                        [2] = bytes[index + i * 7 + 5],
                        [3] = bytes[index + i * 7 + 6],
                        [4] = bytes[index + i * 7 + 7],
                        [5] = bytes[index + i * 7 + 8]
                    }
                };
            }
        }
    }
}
