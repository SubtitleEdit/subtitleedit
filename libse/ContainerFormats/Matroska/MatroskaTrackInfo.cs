using System.IO;
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

        internal static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[128 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

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
                var outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream);
                var inStream = new MemoryStream(CodecPrivateRaw);
                try
                {
                    CopyStream(inStream, outZStream);
                    var buffer = new byte[outZStream.TotalOut];
                    outStream.Position = 0;
                    outStream.Read(buffer, 0, buffer.Length);
                    return Encoding.UTF8.GetString(buffer);
                }
                catch
                {
                    return Encoding.UTF8.GetString(CodecPrivateRaw);
                }
                finally
                {
                    outZStream.Close();
                    inStream.Close();
                }
            }
            return Encoding.UTF8.GetString(CodecPrivateRaw);
        }
    }
}