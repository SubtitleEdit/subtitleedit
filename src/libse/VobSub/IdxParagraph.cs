using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class IdxParagraph
    {
        public TimeSpan StartTime { get; private set; }

        public long FilePosition { get; private set; }

        public IdxParagraph(TimeSpan startTime, long filePosition)
        {
            StartTime = startTime;
            FilePosition = filePosition;
        }
    }
}
