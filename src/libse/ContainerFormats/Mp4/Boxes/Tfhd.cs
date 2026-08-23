using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Fragment Header Box.
    /// </summary>
    public class Tfhd : Box
    {
        public bool BaseDataOffsetPresent;
        public bool SampleDescriptionIndexPresent;
        public bool DefaultSampleDurationPresent;
        public bool DefaultSampleSizePresent;
        public bool DefaultSampleFlagsPresent;

        public readonly uint TrackId;
        public readonly ulong? BaseDataOffset;
        public readonly uint? SampleDescriptionIndex;
        public readonly uint? DefaultSampleDuration;
        public readonly uint? DefaultSampleSize;
        public readonly uint? DefaultSampleFlags;

        //version                 // 8 unsigned bit (Should always be 0)
        //flags                   // 24 unsigned bit
        //trackID                 // 32 unsigned bit
        //
        //// OPTIONAL FIELDS
        //baseDataOffset          // 64 unsinged bit
        //sampleDescriptionIndex  // 32 unsigned bit
        //defaultSampleDuration   // 32 unsigned bit
        //defaultSampleSize       // 32 unsigned bit
        //defaultSampleFlags      // 32 unsigned bit

        // A track fragment header is well under this; anything larger means the size was misread.
        private const ulong MaxSize = 1024 * 1024;

        public Tfhd(Stream fs, ulong size)
        {
            // "size" comes straight from the file. The old "size - 8 <= 0" guard could never fire
            // on an unsigned value, so a too-small size underflowed into a ~18 exabyte allocation.
            // version/flags + track id.
            if (size < 16 || size > MaxSize)
            {
                return;
            }

            var bufferSize = size - 8;

            Buffer = new byte[bufferSize];
            var bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            var version = Buffer[0];

            var flags = GetUInt(0);
            BaseDataOffsetPresent = (flags & 0x1) == 0x1;
            SampleDescriptionIndexPresent = (flags & 0x2) == 0x2;
            DefaultSampleDurationPresent = (flags & 0x8) == 0x8;
            DefaultSampleSizePresent = (flags & 0x10) == 0x10;
            DefaultSampleFlagsPresent = (flags & 0x20) == 0x20;

            TrackId = GetUInt(4);

            // The flags claim which optional fields follow; a crafted size can declare fewer
            // bytes than they need, and the readers below index the buffer unchecked.
            var required = 8
                           + (BaseDataOffsetPresent ? 8 : 0)
                           + (SampleDescriptionIndexPresent ? 4 : 0)
                           + (DefaultSampleDurationPresent ? 4 : 0)
                           + (DefaultSampleSizePresent ? 4 : 0)
                           + (DefaultSampleFlagsPresent ? 4 : 0);
            if (Buffer.Length < required)
            {
                return;
            }

            var idx = 8;
            if (BaseDataOffsetPresent)
            {
                BaseDataOffset = GetUInt64(idx);
                idx += 8;
            }

            if (SampleDescriptionIndexPresent)
            {
                SampleDescriptionIndex = GetUInt(idx);
                idx += 4;
            }

            if (DefaultSampleDurationPresent)
            {
                DefaultSampleDuration = GetUInt(idx);
                idx += 4;
            }

            if (DefaultSampleSizePresent)
            {
                DefaultSampleSize = GetUInt(idx);
                idx += 4;
            }

            if (DefaultSampleFlagsPresent)
            {
                DefaultSampleFlags = GetUInt(idx);
                idx += 4;
            }
        }
    }
}
