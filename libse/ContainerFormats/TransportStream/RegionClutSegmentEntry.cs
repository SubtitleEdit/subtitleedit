using System.Drawing;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class RegionClutSegmentEntry
    {
        public int ClutEntryId { get; set; }
        public bool ClutEntry2BitClutEntryFlag { get; set; }
        public bool ClutEntry4BitClutEntryFlag { get; set; }
        public bool ClutEntry8BitClutEntryFlag { get; set; }
        public bool FullRangeFlag { get; set; }
        public int ClutEntryY { get; set; }
        public int ClutEntryCr { get; set; }
        public int ClutEntryCb { get; set; }
        public int ClutEntryT { get; set; }

        private static int BoundByteRange(int i)
        {
            if (i < byte.MinValue)
            {
                return byte.MinValue;
            }

            if (i > byte.MaxValue)
            {
                return byte.MaxValue;
            }

            return i;
        }

        public Color GetColor()
        {
            double y, cr, cb;
            if (FullRangeFlag)
            {
                y = ClutEntryY;
                cr = ClutEntryCr;
                cb = ClutEntryCb;
            }
            else
            {
                y = ClutEntryY * 255 / 63.0;
                cr = ClutEntryCr * 255 / 15.0;
                cb = ClutEntryCb * 255 / 15.0;
            }

            // Calculate rgb - based on Project X
            int r = (int)(y + (1.402f * (cr - 128)));
            int g = (int)(y - (0.34414 * (cb - 128)) - (0.71414 * (cr - 128)));
            int b = (int)(y + (1.722 * (cb - 128)));

            int t = byte.MaxValue - BoundByteRange(ClutEntryT);
            r = BoundByteRange(r);
            g = BoundByteRange(g);
            b = BoundByteRange(b);

            if (y < 0.1) // full transparency
            {
                t = 0;
            }

            return Color.FromArgb(t, r, g, b);
        }
    }

}
