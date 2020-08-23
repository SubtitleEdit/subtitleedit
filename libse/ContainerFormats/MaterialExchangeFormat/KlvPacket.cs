using System;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat
{
    /// <summary>
    /// Key-Length-Value MXF package - http://en.wikipedia.org/wiki/KLV + http://en.wikipedia.org/wiki/Material_Exchange_Format
    /// </summary>
    public class KlvPacket
    {
        public static readonly byte[] PartitionPack = { 0x06, 0x0e, 0x2b, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x01, 0xff, 0xff, 0x00 }; // 0xff can have different values
        public static readonly byte[] Preface = { 0x06, 0x0E, 0x2B, 0x34, 0x02, 0x53, 0x01, 0x01, 0x0d, 0x01, 0x01, 0x01, 0x01, 0x01, 0x2F, 0x00 };
        public static readonly byte[] EssenceElement = { 0x06, 0x0E, 0x2B, 0x34, 0x01, 0x02, 0x01, 0x01, 0x0D, 0x01, 0x03, 0x01, 0xff, 0xff, 0xff, 0xff };
        public static readonly byte[] OperationalPattern = { 0x06, 0x0E, 0x2B, 0x34, 0x04, 0x01, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x00, 0xff, 0xff, 0x00 };
        public static readonly byte[] PartitionMetadata = { 0x06, 0x0e, 0x2b, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0d, 0x01, 0x02, 0x01, 0x01, 0x04, 0x04, 0x00 };
        public static readonly byte[] StructuralMetadata = { 0x06, 0x0e, 0x2b, 0x34, 0x02, 0x53, 0x01, 0x01, 0x0d, 0x01, 0x01, 0x01, 0x00, 0xff, 0xff, 0x00 };
        public static readonly byte[] DataDefinitionVideo = { 0x06, 0x0E, 0x2B, 0x34, 0x04, 0x01, 0x01, 0x01, 0x01, 0x03, 0x02, 0x02, 0x01, 0xff, 0xff, 0x00 };
        public static readonly byte[] DataDefinitionAudio = { 0x06, 0x0E, 0x2B, 0x34, 0x04, 0x01, 0x01, 0x01, 0x01, 0x03, 0x02, 0x02, 0x02, 0xff, 0xff, 0x00 };
        public static readonly byte[] Primer = { 0x06, 0xe, 0x2b, 0x34, 0x02, 0x05, 0x1, 0xff, 0x0d, 0x01, 0x02, 0x01, 0x01, 0x05, 0x01, 0xff };

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
                bytesInLength = first & 0b01111111;
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

        public KeyIdentifier IdentifierType
        {
            get
            {
                if (IsKey(PartitionPack))
                {
                    return KeyIdentifier.PartitionPack;
                }
                if (IsKey(Preface))
                {
                    return KeyIdentifier.Preface;
                }
                if (IsKey(EssenceElement))
                {
                    return KeyIdentifier.EssenceElement;
                }
                if (IsKey(OperationalPattern))
                {
                    return KeyIdentifier.OperationalPattern;
                }
                if (IsKey(PartitionMetadata))
                {
                    return KeyIdentifier.PartitionMetadata;
                }
                if (IsKey(StructuralMetadata))
                {
                    return KeyIdentifier.StructuralMetadata;
                }
                if (IsKey(DataDefinitionVideo))
                {
                    return KeyIdentifier.DataDefinitionVideo;
                }
                if (IsKey(DataDefinitionAudio))
                {
                    return KeyIdentifier.DataDefinitionAudio;
                }
                if (IsKey(Primer))
                {
                    return KeyIdentifier.DataDefinitionAudio;
                }

                return KeyIdentifier.Unknown;
            }
        }

        private bool IsKey(byte[] key)
        {
            if (KeySize != key.Length)
            {
                return false;
            }

            for (int i = 0; i < KeySize; i++)
            {
                if (Key[i] != key[i] && key[i] != 0xff)
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
