using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// http://www.mpucoder.com/DVD/packhdr.html
    /// </summary>
    public class Mpeg2Header
    {
        public static readonly int Length = 14;

        public readonly UInt32 StartCode;
        public readonly byte PackIdentifier;
        public readonly UInt64 ProgramMuxRate;
        public readonly int PackStuffingLength;

        public Mpeg2Header(byte[] buffer)
        {
            StartCode = Helper.GetEndian(buffer, 0, 3);
            PackIdentifier = buffer[3];
            ProgramMuxRate = Helper.GetEndian(buffer, 10, 3) >> 2;
            PackStuffingLength = buffer[13] & 0b00000111;
        }
    }
}
