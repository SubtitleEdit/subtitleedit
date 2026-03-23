using Nikse.SubtitleEdit.Core.Common;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public class MatroskaSubtitle
    {
        internal byte[] Data { get; set; }
        public long Start { get; set; }
        public long Duration { get; set; }

        public MatroskaSubtitle(byte[] data, long start, long duration)
        {
            Data = data;
            Start = start;
            Duration = duration;
        }

        public MatroskaSubtitle(byte[] data, long start)
            : this(data, start, 0)
        {
        }

        public long End => Start + Duration;

        /// <summary>
        /// Get data, if contentEncodingType == 0, then data is compressed with zlib
        /// </summary>
        /// <param name="matroskaTrackInfo"></param>
        /// <returns>Data byte array (uncompressed)</returns>
        public byte[] GetData(MatroskaTrackInfo matroskaTrackInfo)
        {
            // Check if compression is used and if it applies to the frame data (Scope)
            if (matroskaTrackInfo.ContentEncodingType != 0 || // 0 = Compression
                (matroskaTrackInfo.ContentEncodingScope & 1) == 0) // 1 = All frame data
            {
                return Data;
            }

            // Ensure we have enough data to check for zlib header
            if (Data == null || Data.Length < 2)
            {
                return Data;
            }

            try
            {
                return DecompressZlib(Data);
            }
            catch
            {
                // If decompression fails, return raw data as a fallback
                return Data;
            }
        }

        private static byte[] DecompressZlib(byte[] data)
        {
            // Matroska zlib blocks usually start with 0x78 0x9C (zlib header).
            // DeflateStream needs raw data, so we skip the first 2 bytes.
            var headerOffset = (data[0] == 0x78 && (data[1] == 0x9C || data[1] == 0x01 || data[1] == 0xDA)) ? 2 : 0;

            using var inStream = new MemoryStream(data, headerOffset, data.Length - headerOffset);
            using var outStream = new MemoryStream(data.Length * 2); // pre-size for typical 2x expansion
            using (var deflateStream = new DeflateStream(inStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outStream);
            }
            return outStream.ToArray();
        }

        public string GetText(MatroskaTrackInfo matroskaTrackInfo)
        {
            var data = GetData(matroskaTrackInfo);
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }

            // Find null terminator using Span for better performance
            var dataSpan = data.AsSpan();
            var nullIndex = dataSpan.IndexOf((byte)0);
            var textSpan = nullIndex >= 0 ? dataSpan.Slice(0, nullIndex) : dataSpan;

            // Decode UTF-8 directly from span
            var text = Encoding.UTF8.GetString(textSpan);

            // normalize "new line" to current OS default - also see https://github.com/SubtitleEdit/subtitleedit/issues/2838
            if (text.Contains("\\N"))
            {
                text = text.Replace("\\N", Environment.NewLine);
            }

            // Only normalize line endings if needed
            if (text.Contains('\r') || text.Contains('\n'))
            {
                text = string.Join(Environment.NewLine, text.SplitToLines());
            }

            return text;
        }
    }
}
