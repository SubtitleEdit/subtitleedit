using System;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class VobSubMergedPack
    {
        public SubPicture SubPicture { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; set; }
        public int StreamId { get; private set; }
        public IdxParagraph IdxLine { get; private set; }

        public VobSubMergedPack(byte[] subPictureData, TimeSpan presentationTimestamp, int streamId, IdxParagraph idxLine)
        {
            SubPicture = new SubPicture(subPictureData);
            StartTime = presentationTimestamp;
            StreamId = streamId;
            IdxLine = idxLine;
        }
    }
}
