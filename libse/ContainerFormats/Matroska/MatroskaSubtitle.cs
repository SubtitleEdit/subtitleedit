using System;
using System.IO;

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

        /// <summary>
        /// Get data, if contentEncodingType == 0, then data is compressed with zlib
        /// </summary>
        /// <param name="matroskaTrackInfo"></param>
        /// <returns>Data byte array (uncompressed)</returns>
        public byte[] GetData(MatroskaTrackInfo matroskaTrackInfo)
        {
            if (matroskaTrackInfo.ContentEncodingType != MatroskaTrackInfo.ContentEncodingTypeCompression ||  // no compression
                (matroskaTrackInfo.ContentEncodingScope & MatroskaTrackInfo.ContentEncodingScopeTracks) == 0) // tracks not compressed
            {
                return Data;
            }

            var outStream = new MemoryStream();
            var outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream);
            var inStream = new MemoryStream(Data);
            byte[] buffer;
            try
            {
                MatroskaTrackInfo.CopyStream(inStream, outZStream);
                buffer = new byte[outZStream.TotalOut];
                outStream.Position = 0;
                outStream.Read(buffer, 0, buffer.Length);
            }
            finally
            {
                outZStream.Close();
                inStream.Close();
            }
            return buffer;
        }

        public MatroskaSubtitle(byte[] data, long start)
            : this(data, start, 0)
        {
        }

        public long End => Start + Duration;

        public string GetText(MatroskaTrackInfo matroskaTrackInfo)
        {
            var data = GetData(matroskaTrackInfo);

            if (data != null)
            {
                // terminate string at first binary zero - https://github.com/Matroska-Org/ebml-specification/blob/master/specification.markdown#terminating-elements
                // https://github.com/SubtitleEdit/subtitleedit/issues/2732
                int max = data.Length;
                for (int i = 0; i < max; i++)
                {
                    if (data[i] == 0)
                    {
                        max = i;
                        break;
                    }
                }
                var text = System.Text.Encoding.UTF8.GetString(data, 0, max);

                // normalize "new line" to current OS default - also see https://github.com/SubtitleEdit/subtitleedit/issues/2838
                text = text.Replace("\\N", Environment.NewLine);
                text = string.Join(Environment.NewLine, text.SplitToLines());

                return text;
            }
            return string.Empty;
        }
    }
}
