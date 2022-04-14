using System;
using System.Collections.Generic;
using System.Drawing;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class VobSubMergedPack : IBinaryParagraphWithPosition
    {
        public SubPicture SubPicture { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; set; }
        public int StreamId { get; private set; }
        public IdxParagraph IdxLine { get; private set; }
        public List<Color> Palette { get; set; }


        public VobSubMergedPack(byte[] subPictureData, TimeSpan presentationTimestamp, int streamId, IdxParagraph idxLine)
        {
            SubPicture = new SubPicture(subPictureData);
            StartTime = presentationTimestamp;
            StreamId = streamId;
            IdxLine = idxLine;
        }

        public bool IsForced => SubPicture.Forced;

        public Bitmap GetBitmap()
        {
            return SubPicture.GetBitmap(Palette, Color.Transparent, Color.Black, Color.White, Color.Black, false, true);
        }

        public Size GetScreenSize()
        {
            return new Size(720, 480);
        }

        public Position GetPosition()
        {
            return new Position(SubPicture.ImageDisplayArea.Left, SubPicture.ImageDisplayArea.Top);
        }

        public TimeCode StartTimeCode
        {
            get
            {
                //if (IdxLine != null)
                //{
                //    return new TimeCode(IdxLine.StartTime.TotalMilliseconds);
                //}

                return new TimeCode(StartTime.TotalMilliseconds);
            }
        }

        public TimeCode EndTimeCode
        {
            get
            {
                //if (IdxLine != null)
                //{
                //    return new TimeCode(IdxLine.StartTime.TotalMilliseconds + SubPicture.Delay.TotalMilliseconds);
                //}

                return new TimeCode(StartTime.TotalMilliseconds + SubPicture.Delay.TotalMilliseconds);
            }
        }

    }
}
