using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Matroska
{
    public class MatroskaTrackInfo
    {
        public const int ContentEncodingTypeCompression = 0;

        public int TrackNumber { get; set; }
        public string Uid { get; set; }
        public bool IsVideo { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubtitle { get; set; }
        public string CodecId { get; set; }
        internal string CodecPrivate { get; set; }
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
            if (ContentEncodingType != ContentEncodingTypeCompression || CodecPrivateRaw == null || CodecPrivateRaw.Length < 3)
            {
                return CodecPrivate;
            }

            var outStream = new MemoryStream();
            var outZStream = new zlib.ZOutputStream(outStream);
            var inStream = new MemoryStream(CodecPrivateRaw);
            try
            {
                //inStream.CopyTo(outZStream);
                CopyStream(inStream, outZStream);
                var buffer = new byte[outZStream.TotalOut];
                outStream.Position = 0;
                outStream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer);
            }
            catch
            {
                return CodecPrivate;
            }
            finally
            {
                outZStream.Close();
                inStream.Close();
            }
        }
    }
}