using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// http://www.mpucoder.com/DVD/packhdr.html
    /// </summary>
    public class Mpeg2Header
    {
        public const int Length = 14;

        public readonly UInt32 StartCode;
        public readonly byte PackIndentifier;
        public readonly UInt64 ProgramMuxRate;
        public readonly int PackStuffingLength;

        public Mpeg2Header(byte[] buffer)
        {
            StartCode = Helper.GetEndian(buffer, 0, 3);
            PackIndentifier = buffer[3];
            ProgramMuxRate = Helper.GetEndian(buffer, 10, 3) >> 2;
            PackStuffingLength = buffer[13] & Helper.B00000111;
        }

    }
}
