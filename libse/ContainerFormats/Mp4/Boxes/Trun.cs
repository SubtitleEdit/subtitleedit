using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Fragment Run Box
    /// https://msdn.microsoft.com/en-us/library/ff469478.aspx
    /// </summary>
    public class Trun : Box
    {
        public List<TimeSegment> Samples { get; set; }

        public Trun(Stream fs, ulong maximumLength)
        {
            Samples = new List<TimeSegment>();
            if (maximumLength <= 4)
            {
                return;
            }

            Buffer = new byte[maximumLength - 4];
            var readCount = fs.Read(Buffer, 0, Buffer.Length);
            if (readCount < (int)maximumLength - 4)
            {
                return;
            }

            var versionAndFlags = GetUInt(0);
            var version = versionAndFlags >> 24;
            var flags = versionAndFlags & 0xFFFFFF;

            var sampleCount = GetUInt(4);
            int pos = 8;

            if ((flags & 0x000001) > 0)
            {
                pos += 4;
            }

            // skip "first_sample_flags" if present
            if ((flags & 0x000004) > 0)
            {
                pos += 4;
            }

            for (var sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                var sample = new TimeSegment();
                if (pos > Buffer.Length - 4)
                {
                    return;
                }

                // read "sample duration" if present
                if ((flags & 0x000100) > 0)
                {
                    sample.Duration = GetUInt(pos);
                    pos += 4;
                }
                if (pos > Buffer.Length - 4)
                {
                    return;
                }

                // skip "sample_size" if present
                if ((flags & 0x000200) > 0)
                {
                    pos += 4;
                }
                if (pos > Buffer.Length - 4)
                {
                    return;
                }

                // skip "sample_flags" if present
                if ((flags & 0x000400) > 0)
                {
                    pos += 4;
                }
                if (pos > Buffer.Length - 4)
                {
                    return;
                }

                // read "sample_time_offset" if present
                if ((flags & 0x000800) > 0)
                {
                    sample.TimeOffset = version == 0 ? GetUInt(pos) : (uint)GetUInt(pos);
                    pos += 4;
                }
                Samples.Add(sample);
            }
        }
    }
}