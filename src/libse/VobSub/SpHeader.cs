using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class SpHeader
    {
        public const int SpHeaderLength = 14;

        public string Identifier { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public int NextBlockPosition { get; private set; }
        public int ControlSequencePosition { get; private set; }
        public SubPicture Picture { get; private set; }

        public SpHeader(byte[] buffer)
        {
            Identifier = System.Text.Encoding.ASCII.GetString(buffer, 0, 2);
            int startMilliseconds = (int)Math.Round(Helper.GetLittleEndian32(buffer, 2) / 90.0);
            StartTime = TimeSpan.FromMilliseconds(startMilliseconds);
            NextBlockPosition = Helper.GetEndianWord(buffer, 10) - 4;
            ControlSequencePosition = Helper.GetEndianWord(buffer, 12) - 4;
        }

        public SubPicture AddPicture(byte[] buffer)
        {
            Picture = new SubPicture(buffer, ControlSequencePosition, -4);
            return Picture;
        }

    }
}
