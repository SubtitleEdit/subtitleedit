using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ProgramMapTable
    {
        public int TableId { get; set; }
        public int SectionLength { get; set; }
        public int ProgramNumber { get; set; }
        public int VersionNumber { get; set; }
        public int CurrentNextIndicator { get; set; }
        public int SectionNumber { get; set; }
        public int LastSectionNumber { get; set; }
        public int PcrId { get; set; }
        public List<ProgramMapTableDescriptor> Descriptors { get; set; }
        public List<ProgramMapTableStream> Streams { get; set; }

        public ProgramMapTable(byte[] packetBuffer, int index)
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
            SectionLength = (packetBuffer[index + 1] & Helper.B00000011) * 256 + packetBuffer[index + 2];
            ProgramNumber = packetBuffer[index + 3] * 256 + packetBuffer[index + 4];
            VersionNumber = (packetBuffer[index + 5] & Helper.B00111110) >> 1;
            CurrentNextIndicator = packetBuffer[index + 5] & 1;
            SectionNumber = packetBuffer[index + 6];
            LastSectionNumber = packetBuffer[index + 7];
            PcrId = (packetBuffer[index + 8] & Helper.B00011111) * 256 + packetBuffer[index + 9];
            var programInfoLength = (packetBuffer[index + 10] & Helper.B00001111) * 256 + packetBuffer[index + 11];

            Descriptors = ProgramMapTableDescriptor.ReadDescriptors(packetBuffer, programInfoLength, index + 12);
            var newIndex = index + 12 +  Descriptors.Sum(p => p.Size);
            Streams = new List<ProgramMapTableStream>();
            while (newIndex - index < SectionLength - 4)
            {
                var stream = new ProgramMapTableStream(packetBuffer, newIndex);
                Streams.Add(stream);
                newIndex += stream.Size;
            }
        }
    }
}
