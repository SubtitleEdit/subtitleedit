using System.IO;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.Mp4.Boxes
{
    /// <summary>
    /// Track Fragment Base Media Decode Time Box
    /// Provide decode start time of the fragment
    /// </summary>
    public class Tfdt : Box
    {
        public ulong BaseMediaDecodeTime { get; set; }

        // A track fragment header is well under this; anything larger means the size was misread.
        private const ulong MaxSize = 1024 * 1024;

        public Tfdt(Stream fs, ulong size)
        {
            // "size" comes straight from the file. The old "size - 8 <= 0" guard could never fire
            // on an unsigned value, so a too-small size underflowed into a ~18 exabyte allocation.
            // version/flags + a 32 or 64 bit decode time.
            if (size < 16 || size > MaxSize)
            {
                return;
            }

            var bufferSize = size - 8;

            Buffer = new byte[bufferSize];
            int bytesRead = fs.Read(Buffer, 0, Buffer.Length);
            if (bytesRead < Buffer.Length)
            {
                return;
            }

            var version = Buffer[0];
            //var flags = GetUInt(0) & 0xffffff;

            if (version == 1)
            {
                if (Buffer.Length < 12)
                {
                    return; // the 64-bit layout does not fit in what the size field declared
                }

                BaseMediaDecodeTime = GetUInt64(4);
            }
            else
            {
                BaseMediaDecodeTime = GetUInt(4);
            }
        }
    }
}
