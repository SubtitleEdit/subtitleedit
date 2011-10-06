using System;

namespace Nikse.SubtitleEdit.Logic.VobSub
{
    /// <summary>
    /// http://www.mpucoder.com/DVD/packhdr.html
    /// </summary>
    public class Mpeg2Header
    {
        public const int Length = 14;

        public readonly UInt32 StartCode;
        public readonly byte PackIndentifier;
        public readonly UInt64 SystemClockReferenceQuotient;
        public readonly UInt64 ProgramMuxRate;
        public readonly int PackStuffingLength;

        public Mpeg2Header(byte[] buffer)
        {
            StartCode = Helper.GetEndian(buffer, 0, 3);
            PackIndentifier = buffer[3];

            string b4To9AsBinary = Helper.GetBinaryString(buffer, 4, 6);
            b4To9AsBinary = b4To9AsBinary.Substring(2,3) + b4To9AsBinary.Substring(6,15) + b4To9AsBinary.Substring(22,15);
            SystemClockReferenceQuotient = Helper.GetUInt32FromBinaryString(b4To9AsBinary);

            string b8And9 = Helper.GetBinaryString(buffer, 8, 2);
            b8And9 = b8And9.Substring(6, 9);
            SystemClockReferenceQuotient = Helper.GetUInt32FromBinaryString(b8And9);

            ProgramMuxRate = Helper.GetEndian(buffer, 10, 3) >> 2;

            PackStuffingLength = buffer[13] & Helper.B00000111;
        }

    }
}
