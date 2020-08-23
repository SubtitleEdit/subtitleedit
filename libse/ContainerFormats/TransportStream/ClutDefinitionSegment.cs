using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class ClutDefinitionSegment
    {
        public int ClutId { get; set; }
        public int ClutVersionNumber { get; set; }
        public List<RegionClutSegmentEntry> Entries = new List<RegionClutSegmentEntry>();

        public ClutDefinitionSegment(byte[] buffer, int index, int segmentLength)
        {
            Entries = new List<RegionClutSegmentEntry>();
            ClutId = buffer[index];
            ClutVersionNumber = buffer[index + 1] >> 4;

            int k = index + 2;
            while (k + 6 <= index + segmentLength)
            {
                var rcse = new RegionClutSegmentEntry();
                rcse.ClutEntryId = buffer[k];
                byte flags = buffer[k + 1];

                rcse.ClutEntry2BitClutEntryFlag = (flags & 0b10000000) > 0;
                rcse.ClutEntry4BitClutEntryFlag = (flags & 0b01000000) > 0;
                rcse.ClutEntry8BitClutEntryFlag = (flags & 0b00100000) > 0;
                rcse.FullRangeFlag = (flags & 0b00000001) > 0;

                if (rcse.FullRangeFlag)
                {
                    rcse.ClutEntryY = buffer[k + 2];
                    rcse.ClutEntryCr = buffer[k + 3];
                    rcse.ClutEntryCb = buffer[k + 4];
                    rcse.ClutEntryT = buffer[k + 5];
                    k += 6;
                }
                else
                {
                    rcse.ClutEntryY = buffer[k + 2] >> 2;
                    rcse.ClutEntryCr = ((buffer[k + 2] & 0b00000011) << 2) + (buffer[k + 2]) >> 6;
                    rcse.ClutEntryCb = ((buffer[k + 3] & 0b00111111) >> 2);
                    rcse.ClutEntryT = buffer[k + 3] & 0b00000011;
                    k += 4;
                }
                Entries.Add(rcse);
            }
        }
    }
}
