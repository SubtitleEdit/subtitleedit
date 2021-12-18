﻿using System.IO;
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

            if (ContentEncodingType == ContentEncodingTypeCompression &&
                (ContentEncodingScope & ContentEncodingScopePrivateDate) > 0 && // second bit set = private data encoded
                CodecPrivateRaw.Length > 2)
            {
                var outStream = new MemoryStream();
                var reader = new StreamReader(outStream);
#if NET6_0
                var outZStream = new System.IO.Compression.ZLibStream(outStream, System.IO.Compression.CompressionMode.Decompress);
#else
                var outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream);
#endif
                try
                {
                    outZStream.Write(CodecPrivateRaw, 0, CodecPrivateRaw.Length);
                    outZStream.Flush();
                    outStream.Position = 0;
                    return reader.ReadToEnd().TrimEnd('\0');
                }
                catch
                {
                    return Encoding.UTF8.GetString(CodecPrivateRaw).TrimEnd('\0');
                }
                finally
                {
                    outStream.Close();
                    reader.Close();
                    outZStream.Close();
                }
            }

            return Encoding.UTF8.GetString(CodecPrivateRaw).TrimEnd('\0');
        }
    }
}
