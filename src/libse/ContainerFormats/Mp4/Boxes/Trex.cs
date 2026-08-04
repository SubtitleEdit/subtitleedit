using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Extends Box (moov/mvex/trex) - per-track defaults for movie fragments.
    /// A fragment sample without a duration/size in trun or tfhd uses these.
    /// </summary>
    public class Trex : Box
    {
        public readonly uint TrackId;
        public readonly uint DefaultSampleDescriptionIndex;
        public readonly uint DefaultSampleDuration;
        public readonly uint DefaultSampleSize;
        public readonly uint DefaultSampleFlags;

        public Trex(Stream fs, ulong size)
        {
            var bufferSize = (long)size - 8;
            if (bufferSize < 24)
            {
                return;
            }

            Buffer = new byte[24];
            var bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            // version + flags at 0
            TrackId = GetUInt(4);
            DefaultSampleDescriptionIndex = GetUInt(8);
            DefaultSampleDuration = GetUInt(12);
            DefaultSampleSize = GetUInt(16);
            DefaultSampleFlags = GetUInt(20);
        }
    }
}
