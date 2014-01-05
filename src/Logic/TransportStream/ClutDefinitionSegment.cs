using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{

    public class ClutDefinitionSegment
    {
        private static uint[] DefaultClut2Bit =
	    {
		    0, 0xFFFFFFFF, 0xFF000000, 0xFF808080
	    };

        private static uint[] DefaultClut4Bit =
	    {
		    0, 0xFFFF0000, 0xFF00FF00, 0xFFFFFF00, 0xFF0000FF, 0xFFFF00FF,
		    0xFF00FFFF, 0xFFFFFFFF, 0xFF000000, 0xFF800000, 0xFF008000,
		    0xFF808000, 0xFF000080, 0xFF800080, 0xFF008080, 0xFF808080
	    };

        private static uint[] DefaultClut8Bit = 
        { 
            0x0, 0x40ff0000, 0x4000ff00, 0x40ffff00, 0x400000ff,
		    0x40ff00ff, 0x4000ffff, 0x40ffffff, 0x80000000, 0x80550000, 0x80005500, 0x80555500, 0x80000055, 0x80550055,
		    0x80005555, 0x80555555, 0xffaa0000, 0xffff0000, 0xffaa5500, 0xffff5500, 0xffaa0055, 0xffff0055, 0xffaa5555,
		    0xffff5555, 0x80aa0000, 0x80ff0000, 0x80aa5500, 0x80ff5500, 0x80aa0055, 0x80ff0055, 0x80aa5555, 0x80ff5555,
		    0xff00aa00, 0xff55aa00, 0xff00ff00, 0xff55ff00, 0xff00aa55, 0xff55aa55, 0xff00ff55, 0xff55ff55, 0x8000aa00,
		    0x8055aa00, 0x8000ff00, 0x8055ff00, 0x8000aa55, 0x8055aa55, 0x8000ff55, 0x8055ff55, 0xffaaaa00, 0xffffaa00,
		    0xffaaff00, 0xffffff00, 0xffaaaa55, 0xffffaa55, 0xffaaff55, 0xffffff55, 0x80aaaa00, 0x80ffaa00, 0x80aaff00,
		    0x80ffff00, 0x80aaaa55, 0x80ffaa55, 0x80aaff55, 0x80ffff55, 0xff0000aa, 0xff5500aa, 0xff0055aa, 0xff5555aa,
		    0xff0000ff, 0xff5500ff, 0xff0055ff, 0xff5555ff, 0x800000aa, 0x805500aa, 0x800055aa, 0x805555aa, 0x800000ff,
		    0x805500ff, 0x800055ff, 0x805555ff, 0xffaa00aa, 0xffff00aa, 0xffaa55aa, 0xffff55aa, 0xffaa00ff, 0xffff00ff,
		    0xffaa55ff, 0xffff55ff, 0x80aa00aa, 0x80ff00aa, 0x80aa55aa, 0x80ff55aa, 0x80aa00ff, 0x80ff00ff, 0x80aa55ff,
		    0x80ff55ff, 0xff00aaaa, 0xff55aaaa, 0xff00ffaa, 0xff55ffaa, 0xff00aaff, 0xff55aaff, 0xff00ffff, 0xff55ffff,
		    0x8000aaaa, 0x8055aaaa, 0x8000ffaa, 0x8055ffaa, 0x8000aaff, 0x8055aaff, 0x8000ffff, 0x8055ffff, 0xffaaaaaa,
		    0xffffaaaa, 0xffaaffaa, 0xffffffaa, 0xffaaaaff, 0xffffaaff, 0xffaaffff, 0xffffffff, 0x80aaaaaa, 0x80ffaaaa,
		    0x80aaffaa, 0x80ffffaa, 0x80aaaaff, 0x80ffaaff, 0x80aaffff, 0x80ffffff, 0xff808080, 0xffaa8080, 0xff80aa80,
		    0xffaaaa80, 0xff8080aa, 0xffaa80aa, 0xff80aaaa, 0xffaaaaaa, 0xff000000, 0xff2a0000, 0xff002a00, 0xff2a2a00,
		    0xff00002a, 0xff2a002a, 0xff002a2a, 0xff2a2a2a, 0xffd58080, 0xffff8080, 0xffd5aa80, 0xffffaa80, 0xffd580aa,
		    0xffff80aa, 0xffd5aaaa, 0xffffaaaa, 0xff550000, 0xff7f0000, 0xff552a00, 0xff7f2a00, 0xff55002a, 0xff7f002a,
		    0xff552a2a, 0xff7f2a2a, 0xff80d580, 0xffaad580, 0xff80ff80, 0xffaaff80, 0xff80d5aa, 0xffaad5aa, 0xff80ffaa,
		    0xffaaffaa, 0xff005500, 0xff2a5500, 0xff007f00, 0xff2a7f00, 0xff00552a, 0xff2a552a, 0xff007f2a, 0xff2a7f2a,
		    0xffd5d580, 0xffffd580, 0xffd5ff80, 0xffffff80, 0xffd5d5aa, 0xffffd5aa, 0xffd5ffaa, 0xffffffaa, 0xff555500,
		    0xff7f5500, 0xff557f00, 0xff7f7f00, 0xff55552a, 0xff7f552a, 0xff557f2a, 0xff7f7f2a, 0xff8080d5, 0xffaa80d5,
		    0xff80aad5, 0xffaaaad5, 0xff8080ff, 0xffaa80ff, 0xff80aaff, 0xffaaaaff, 0xff000055, 0xff2a0055, 0xff002a55,
		    0xff2a2a55, 0xff00007f, 0xff2a007f, 0xff002a7f, 0xff2a2a7f, 0xffd580d5, 0xffff80d5, 0xffd5aad5, 0xffffaad5,
		    0xffd580ff, 0xffff80ff, 0xffd5aaff, 0xffffaaff, 0xff550055, 0xff7f0055, 0xff552a55, 0xff7f2a55, 0xff55007f,
		    0xff7f007f, 0xff552a7f, 0xff7f2a7f, 0xff80d5d5, 0xffaad5d5, 0xff80ffd5, 0xffaaffd5, 0xff80d5ff, 0xffaad5ff,
		    0xff80ffff, 0xffaaffff, 0xff005555, 0xff2a5555, 0xff007f55, 0xff2a7f55, 0xff00557f, 0xff2a557f, 0xff007f7f,
		    0xff2a7f7f, 0xffd5d5d5, 0xffffd5d5, 0xffd5ffd5, 0xffffffd5, 0xffd5d5ff, 0xffffd5ff, 0xffd5ffff, 0xffffffff,
		    0xff555555, 0xff7f5555, 0xff557f55, 0xff7f7f55, 0xff55557f, 0xff7f557f, 0xff557f7f, 0xff7f7f7f
	    };


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

                rcse.ClutEntry2BitClutEntryFlag = (flags & Helper.B10000000) > 0;
                rcse.ClutEntry4BitClutEntryFlag = (flags & Helper.B01000000) > 0;
                rcse.ClutEntry8BitClutEntryFlag = (flags & Helper.B00100000) > 0;
                rcse.FullRangeFlag = (flags & Helper.B00000001) > 0;

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
                    rcse.ClutEntryCr = ((buffer[k + 2] & Helper.B00000011) << 2) + (buffer[k + 2]) >> 6;
                    rcse.ClutEntryCb = ((buffer[k + 3] & Helper.B00111111) >> 2);
                    rcse.ClutEntryT = buffer[k + 3] & Helper.B00000011;
                    k += 4;
                }
                Entries.Add(rcse);
            }
        }
    }

}
