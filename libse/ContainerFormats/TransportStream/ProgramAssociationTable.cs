using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ProgramAssociationTable
    {
        public int TableId { get; set; }
        public int SectionLength { get; set; }
        public int TransportStreamId { get; set; }
        public int VersionNumber { get; set; }
        public int CurrentNextIndicator { get; set; }
        public int SectionNumber { get; set; }
        public int LastSectionNumber { get; set; }
        public List<int> ProgramNumbers { get; set; }
        public List<int> ProgramIds { get; set; }

        public ProgramAssociationTable(byte[] packetBuffer, int index)
        {
            var pointer = packetBuffer[index];
            if (pointer > 0)
            {
                index += pointer;
            }
            else
            {
                index++;
            }
            TableId = packetBuffer[index];
            SectionLength = (packetBuffer[index + 1] & 0b00000011) * 256 + packetBuffer[index + 2];
            TransportStreamId = packetBuffer[index + 3] * 256 + packetBuffer[index + 4];
            VersionNumber = (packetBuffer[index + 5] & 0b00111110) >> 1;
            CurrentNextIndicator = packetBuffer[index + 5] & 1;
            SectionNumber = packetBuffer[index + 6];
            LastSectionNumber = packetBuffer[index + 7];
            ProgramNumbers = new List<int>();
            ProgramIds = new List<int>();
            index = index + 8;
            for (int i = 0; i < (SectionLength - 5) / 8; i++)
            {
                if (index + 3 < packetBuffer.Length)
                {
                    int programNumber = packetBuffer[index] * 256 + packetBuffer[index + 1];
                    int programId = (packetBuffer[index + 2] & 0b00011111) * 256 + packetBuffer[index + 3];
                    ProgramNumbers.Add(programNumber);
                    ProgramIds.Add(programId);
                    index += 8;
                }
            }
        }
    }
}
