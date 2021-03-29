using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public class CcServiceInfoSection
    {
        public byte Id { get; set; }
        public bool Start { get; set; }
        public bool Change { get; set; }
        public bool Complete { get; set; }
        public int ServiceCount { get; set; }
        public CcServiceInfoSectionElement[] CcServiceInfoSectionElements { get; set; }

        public int GetLength()
        {
            return 2 + ServiceCount * 7;
        }

        public CcServiceInfoSection()
        {
            Id = 0x73;
            Start = true;
            Change = true;
            Complete = true;
            ServiceCount = 1;
            CcServiceInfoSectionElements = new[]
            {
                new CcServiceInfoSectionElement
                {
                    CaptionServiceNumber = 0,
                    ServiceDataByte = new byte[] { 0x65, 0x6e, 0x67, 0xc1, 0xff, 0xff },
                },
            };
        }

        public CcServiceInfoSection(byte[] bytes, int index)
        {
            Id = bytes[index];

            Start = (bytes[index + 1] & 0b01000000) > 0;
            Change = (bytes[index + 1] & 0b00100000) > 0;
            Complete = (bytes[index + 1] & 0b00010000) > 0;
            ServiceCount = bytes[index + 1] & 0b00001111;

            CcServiceInfoSectionElements = new CcServiceInfoSectionElement[ServiceCount];
            for (int i = 0; i < ServiceCount; i++)
            {
                CcServiceInfoSectionElements[i] = new CcServiceInfoSectionElement
                {
                    CaptionServiceNumber = bytes[index + i * 7 + 2] & 0b00011111,
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

        public byte[] GetBytes()
        {
            var elementBytes = new List<byte>();
            foreach (var element in CcServiceInfoSectionElements)
            {
                elementBytes.Add((byte)(0b11100000 | (element.CaptionServiceNumber & 0b00011111)));
                foreach (var b in element.ServiceDataByte)
                {
                    elementBytes.Add(b);
                }
            }

            return new[]
            {
                Id,
                (byte)(0b10000000 |
                       (ServiceCount & 0b00001111) |
                       (Complete ? 0b00010000 : 0) |
                       (Change ? 0b00100000 : 0) |
                       (Start ? 0b01000000 : 0)),
            }.Concat(elementBytes).ToArray();
        }
    }
}
