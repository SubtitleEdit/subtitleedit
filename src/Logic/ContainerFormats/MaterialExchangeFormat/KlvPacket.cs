using System;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Logic.VobSub;

namespace Nikse.SubtitleEdit.Logic.ContainerFormats.MaterialExchangeFormat
{

    /// <summary>
    /// Key-Length-Value MXF package - http://en.wikipedia.org/wiki/KLV + http://en.wikipedia.org/wiki/Material_Exchange_Format
    /// </summary>
    public class KlvPacket
    {
        public static byte[] PartitionPack = { 0x06, 0x0e, 0x2b, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x01, 0x02, 0x00, 0x00 };
        public static byte[] Preface = { 0x06, 0x0E, 0x2B, 0x34, 0x02, 0x53, 0x01, 0x01, 0x0d, 0x01, 0x01, 0x01, 0x01, 0x01, 0x2F, 0x00 };
        public static byte[] EssenceElement = { 0x06, 0x0E, 0x2B, 0x34, 0x01, 0x02, 0x01, 0x01, 0x0D, 0x01, 0x03, 0x01, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] OperationalPattern = { 0x06, 0x0E, 0x2B, 0x34, 0x04, 0x01, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] PartitionMetadata = { 0x06, 0x0e, 0x2b, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0d, 0x01, 0x02, 0x01, 0x01, 0x04, 0x04, 0x00 };
        public static byte[] StructuralMetadata = { 0x06, 0x0e, 0x2b, 0x34, 0x02, 0x53, 0x01, 0x01, 0x0d, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] DataDefinitionVideo = { 0x06, 0x0E, 0x2B, 0x34, 0x04, 0x01, 0x01, 0x01, 0x01, 0x03, 0x02, 0x02, 0x01, 0x00, 0x00, 0x00 };
        public static byte[] DataDefinitionAudio = { 0x06, 0x0E, 0x2B, 0x34, 0x04, 0x01, 0x01, 0x01, 0x01, 0x03, 0x02, 0x02, 0x02, 0x00, 0x00, 0x00 };

        private const int KeySize = 16;

        public byte[] Key;
        public long DataSize;
        public long TotalSize;
        public long DataPosition;
        public PartitionStatus PartionStatus = PartitionStatus.Unknown;

        public KlvPacket(Stream stream)
        {
            // read 16-bytes key
            Key = new byte[KeySize];
            int read = stream.Read(Key, 0, Key.Length);
            if (read < Key.Length)
            {
                throw new Exception("MXF KLV packet - stream does not have 16 bytes available for key");
            }
            int lengthSize;
            DataSize = GetBasicEncodingRuleLength(stream, out lengthSize);
            DataPosition = stream.Position;
            TotalSize = KeySize + lengthSize + DataSize;
            if (Key[14] >= 1 && Key[14] <= 4)
            {
                PartionStatus = (PartitionStatus)Key[14];
            }
        }

        /// <summary>
        /// Read length - never be more than 9 bytes in size (which means max 8 bytes of payload length)
        /// There are four kinds of encoding for the Length field: 1-byte, 2-byte, 4-byte
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytesInLength"></param>
        /// <returns></returns>
        private long GetBasicEncodingRuleLength(Stream stream, out int bytesInLength)
        {
            int first = stream.ReadByte();
            if (first > 127) // first bit set
            {
                bytesInLength = first & Helper.B01111111;
                if (bytesInLength > 8)
                {
                    throw new Exception("MXF KLV packet - lenght bytes > 8");
                }
                DataSize = 0;
                for (int i = 0; i < bytesInLength; i++)
                {
                    DataSize = DataSize * 256 + stream.ReadByte();
                }
                bytesInLength++;
                return DataSize;
            }
            bytesInLength = 1;
            return first;
        }

        public KeyIdentifier IdentifyerType
        {
            get
            {
                if (KeyIs(PartitionPack))
                {
                    return KeyIdentifier.PartitionPack;
                }
                if (KeyIs(Preface))
                {
                    return KeyIdentifier.Preface;
                }
                if (KeyIs(EssenceElement))
                {
                    return KeyIdentifier.EssenceElement;
                }
                if (KeyIs(OperationalPattern))
                {
                    return KeyIdentifier.OperationalPattern;
                }
                if (KeyIs(PartitionMetadata))
                {
                    return KeyIdentifier.PartitionMetadata;
                }
                if (KeyIs(StructuralMetadata))
                {
                    return KeyIdentifier.StructuralMetadata;
                }
                if (KeyIs(DataDefinitionVideo))
                {
                    return KeyIdentifier.DataDefinitionVideo;
                }
                if (KeyIs(DataDefinitionAudio))
                {
                    return KeyIdentifier.DataDefinitionAudio;
                }

                return KeyIdentifier.Unknown;
            }
        }

        private bool KeyIs(byte[] key)
        {
            if (KeySize != key.Length)
            {
                return false;
            }

            for (int i = 0; i < KeySize; i++)
            {
                if (Key[i] != key[i] && i != 13 && i != 14)
                {
                    return false;
                }
            }
            return true;
        }

        public string DisplayKey
        {
            get
            {
                var sb = new StringBuilder();
                for (int i = 0; i < KeySize; i++)
                {
                    sb.Append(Key[i].ToString("X2") + "-");
                }
                return sb.ToString().TrimEnd('-');
            }
        }

    }
}