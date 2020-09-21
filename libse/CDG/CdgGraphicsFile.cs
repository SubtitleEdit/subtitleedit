using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.CDG
{
    public static class CdgGraphicsFile
    {
        private const int PacketSize = 24;

        public static CdgGraphics Load(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return Load(fs);
            }
        }

        public static CdgGraphics Load(Stream stream)
        {
            stream.Position = 0;
            var subCodePackets = new List<Packet>((int)(stream.Length / PacketSize));
            var cdgPacket = new byte[PacketSize];
            while (stream.Read(cdgPacket, 0, PacketSize) == PacketSize)
            {
                subCodePackets.Add(new Packet(cdgPacket));
            }

            return new CdgGraphics(subCodePackets);
        }
    }
}