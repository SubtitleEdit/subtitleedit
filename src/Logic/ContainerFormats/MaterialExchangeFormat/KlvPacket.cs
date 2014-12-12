using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Logic.VobSub;

namespace Nikse.SubtitleEdit.Logic.ContainerFormats.MaterialExchangeFormat
{

    /// <summary>
    /// Key-Length-Value - http://en.wikipedia.org/wiki/KLV
    /// </summary>
    public class KlvPacket
    {

        public byte[] Key;
        public long Length;


        public KlvPacket(Stream stream)
        {
            // read 16-bytes key
            Key = new byte[16];
            int read = stream.Read(Key, 0, Key.Length);
            if (read < Key.Length)
            {
                throw new Exception("MXF KLV packet - stream does not have 16 bytes available for key");
            }

            // Read length - never be more than 9 bytes in size (which means max 8 bytes of payload length)
            // There are four kinds of encoding for the Length field: 1-byte, 2-byte, 4-byte
            Length = GetBasicEncodingRuleLength(stream);

        }

        private long GetBasicEncodingRuleLength(Stream stream)
        {
            int first = stream.ReadByte();
            if (first > 127) // first bit set
            {
                int bytesInLength = first & Helper.B01111111;
                if (bytesInLength > 8)
                {
                    throw new Exception("MXF KLV packet - lenght bytes > 8");
                }
                Length = 0;
                for (int i = 0; i < bytesInLength; i++)
                {
                    Length = Length * 256 + stream.ReadByte();
                }
                return Length;
            }
            return first;
        }



    }
}
