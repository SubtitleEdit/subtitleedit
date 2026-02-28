using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public class MatroskaTrackInfo
    {
        public const int ContentEncodingTypeCompression = 0;
        internal const int ContentEncodingScopeTracks = 1;
        internal const int ContentEncodingScopePrivateDate = 2;

        public int TrackNumber { get; set; }
        public string Uid { get; set; }
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubtitle { get; set; }
        public bool IsDefault { get; set; }
        public bool IsForced { get; set; }
        public string CodecId { get; set; }
        internal byte[] CodecPrivateRaw { get; set; }
        public int DefaultDuration { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }

        /// <summary>
        ///  0 = zlib
        ///  1 = bzlib
        ///  2 = lzo1x
        ///  3 = Header Stripping
        /// </summary>
        public int ContentCompressionAlgorithm { get; set; }
        public int ContentEncodingType { get; set; }
        public uint ContentEncodingScope { get; set; }

        public string GetCodecPrivate()
        {
            if (CodecPrivateRaw == null || CodecPrivateRaw.Length == 0)
            {
                return string.Empty;
            }

            // ContentEncodingType 0 = Compression
            // ContentEncodingScope bit 2 (value 2) = Private Data is compressed
            if (ContentEncodingType == 0 && (ContentEncodingScope & 2) != 0 && CodecPrivateRaw.Length > 2)
            {
                try
                {
                    // Detect zlib header (usually 0x78 0x9C)
                    int headerOffset = (CodecPrivateRaw[0] == 0x78 && (CodecPrivateRaw[1] == 0x9C || CodecPrivateRaw[1] == 0x01)) ? 2 : 0;

                    using var inStream = new MemoryStream(CodecPrivateRaw, headerOffset, CodecPrivateRaw.Length - headerOffset);
                    using var outStream = new MemoryStream();
                    using (var deflateStream = new DeflateStream(inStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(outStream);
                    }

                    return Encoding.UTF8.GetString(outStream.ToArray()).TrimEnd('\0');
                }
                catch
                {
                    // Fallback to raw string if decompression fails
                    return Encoding.UTF8.GetString(CodecPrivateRaw).TrimEnd('\0');
                }
            }

            return Encoding.UTF8.GetString(CodecPrivateRaw).TrimEnd('\0');
        }
    }
}