using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    /// <summary>
    /// The colour map a teletext writer builds up while it lays out a file: the 32 entries of
    /// ETS 300 706 table 30, where entries 16 to 31 (CLUT 2 and 3) may be redefined by a packet
    /// X/28/0 to carry colours of the writer's own choosing - the Level 2.5 mechanism FAB and
    /// other teletext editors use for their "user defined" colours.
    /// <para>
    /// A colour that is one of the eight full intensity CLUT 0 colours needs nothing beyond the
    /// Level 1 spacing attribute every decoder understands. Any other colour gets a colour map
    /// entry - an existing one when the colour is already in the map (the half intensity CLUT 1
    /// colours, for example), otherwise the next free redefinable entry - and is named at its
    /// character position by an X/26 foreground colour triplet. Once all sixteen redefinable
    /// entries are taken, further colours snap to the nearest entry in the map.
    /// </para>
    /// </summary>
    public class TeletextColorMap
    {
        private readonly int[] _map = (int[])TeletextTables.DefaultColorMap.Clone();
        private readonly Dictionary<int, int> _assignedEntries = new Dictionary<int, int>();
        private int _nextFreeEntry = 16;

        /// <summary>
        /// True once a colour landed in a redefinable entry, so the file has to carry an X/28/0
        /// packet for the colour to mean anything. Colours served by the map's own defaults
        /// (CLUT 1, or a CLUT 2/3 default hit exactly) need no packet: table 30 is the default
        /// in every decoder.
        /// </summary>
        public bool NeedsColorMapPacket { get; private set; }

        /// <summary>
        /// How a colour goes on air: the Level 1 spacing attribute that stands in for it (the
        /// nearest of the eight CLUT 0 colours - all a Level 1 decoder will show), and the colour
        /// map entry an X/26 foreground colour triplet should name, or -1 when the spacing
        /// attribute alone already is the colour.
        /// </summary>
        public readonly struct ResolvedColor
        {
            public ResolvedColor(byte spacingAttribute, int entry)
            {
                SpacingAttribute = spacingAttribute;
                Entry = entry;
            }

            public byte SpacingAttribute { get; }
            public int Entry { get; }
        }

        /// <summary>
        /// Resolves a 12 bit rgb colour (four bits per component, red in the high nibble - the
        /// resolution the teletext colour map has) to its wire form.
        /// </summary>
        public ResolvedColor Resolve(int rgb12)
        {
            var spacing = GetNearestSpacingAttribute(rgb12);
            if (_map[spacing] == rgb12)
            {
                return new ResolvedColor(spacing, -1);
            }

            if (_assignedEntries.TryGetValue(rgb12, out var assigned))
            {
                return new ResolvedColor(spacing, assigned);
            }

            // The map may already hold the colour - a CLUT 1 half intensity colour, or a CLUT 2/3
            // default - and an existing entry beats spending a redefinable one.
            for (var entry = 8; entry < _map.Length; entry++)
            {
                if (_map[entry] == rgb12)
                {
                    _assignedEntries.Add(rgb12, entry);
                    return new ResolvedColor(spacing, entry);
                }
            }

            if (_nextFreeEntry < _map.Length)
            {
                var entry = _nextFreeEntry++;
                _map[entry] = rgb12;
                _assignedEntries.Add(rgb12, entry);
                NeedsColorMapPacket = true;
                return new ResolvedColor(spacing, entry);
            }

            // All sixteen redefinable entries are taken - snap to the nearest colour in the map.
            var nearest = GetNearestEntry(rgb12);
            _assignedEntries.Add(rgb12, nearest < 8 ? -1 : nearest);
            return new ResolvedColor(spacing, nearest < 8 ? -1 : nearest);
        }

        /// <summary>
        /// The Level 1 spacing attribute nearest to a colour: each component rounded to full
        /// intensity or off, which is all the eight CLUT 0 colours can tell apart.
        /// </summary>
        public static byte GetNearestSpacingAttribute(int rgb12)
        {
            return (byte)((((rgb12 >> 8) & 0x0f) >= 8 ? 1 : 0) |
                          (((rgb12 >> 4) & 0x0f) >= 8 ? 2 : 0) |
                          ((rgb12 & 0x0f) >= 8 ? 4 : 0));
        }

        private int GetNearestEntry(int rgb12)
        {
            var best = 7;
            var bestDistance = int.MaxValue;
            for (var entry = 0; entry < _map.Length; entry++)
            {
                var r = ((_map[entry] >> 8) & 0x0f) - ((rgb12 >> 8) & 0x0f);
                var g = ((_map[entry] >> 4) & 0x0f) - ((rgb12 >> 4) & 0x0f);
                var b = (_map[entry] & 0x0f) - (rgb12 & 0x0f);
                var distance = r * r + g * g + b * b;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = entry;
                }
            }

            return best;
        }

        /// <summary>
        /// The 40 data bytes of a packet X/28/0 Format 1 carrying this colour map, ready for a
        /// data unit: the designation code, a triplet declaring Format 1 with the default G0
        /// charset, and the bit stream holding the sixteen redefinable entries - the exact layout
        /// <see cref="Teletext"/> reads back (and ZDF transmits), with the default screen and row
        /// colours black and the CLUT remapping at zero, so Level 1 spacing attributes keep
        /// meaning CLUT 0.
        /// </summary>
        public byte[] GetColorMapPacketData()
        {
            var stream = new bool[216];

            // Bits 0-4 default screen colour, bits 5-9 default row colour - both black (0).
            const int firstColorMapBit = 10;
            for (var entry = 0; entry < 16; entry++)
            {
                var rgb12 = _map[16 + entry];

                // The bit stream stores each entry with red in the low nibble.
                var value = ((rgb12 >> 8) & 0x00f) | (rgb12 & 0x0f0) | ((rgb12 & 0x00f) << 8);
                for (var bit = 0; bit < 12; bit++)
                {
                    stream[firstColorMapBit + entry * 12 + bit] = ((value >> bit) & 1) != 0;
                }
            }

            var data = new byte[40];
            data[0] = TeletextHamming.Hamming84Encode(0); // designation code 0

            // Triplet 1: page function and coding zero = Format 1, charset designation zero -
            // the default G0 set the page header declares too.
            WriteTriplet(data, 1, 0);

            for (var triplet = 0; triplet < 12; triplet++)
            {
                var value = 0;
                for (var bit = 0; bit < 18; bit++)
                {
                    if (stream[triplet * 18 + bit])
                    {
                        value |= 1 << bit;
                    }
                }

                WriteTriplet(data, 4 + triplet * 3, value);
            }

            return data;
        }

        private static void WriteTriplet(byte[] data, int index, int value)
        {
            var encoded = TeletextHamming.Hamming2418Encode(value);
            data[index] = (byte)encoded;
            data[index + 1] = (byte)(encoded >> 8);
            data[index + 2] = (byte)(encoded >> 16);
        }
    }
}
