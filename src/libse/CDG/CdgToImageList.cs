using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.CDG
{
    public class CdgToImageList
    {
        public delegate void CdgToImageSubtitleCallback(int currentNumber, int totalUnique);

        public int MergeGapSmallerThanXMilliseconds { get; set; } = 999;
        public int SmallGapMilliseconds { get; set; }
        public int SkipPacketCount { get; set; } = 20;

        public List<NikseBitmap> MakeImageList(CdgGraphics cdgGraphics, Subtitle subtitle, CdgToImageSubtitleCallback callback)
        {
            subtitle.Paragraphs.Clear();
            NikseBitmap lastNBmp = null;
            int count = 0;
            var imageList = new List<NikseBitmap>();
            Paragraph p = null;
            int packetNumber = 0;
            var max = cdgGraphics.NumberOfPackets;
            while (packetNumber < max)
            {
                using (var bmp = cdgGraphics.ToBitmap(packetNumber))
                {
                    var nBmp = new NikseBitmap(bmp);
                    var timeMs = packetNumber * CdgGraphics.TimeMsFactor;
                    if (lastNBmp == null)
                    {
                        if (nBmp.Width > 0)
                        {
                            lastNBmp = nBmp;
                            p = new Paragraph(string.Empty, timeMs, timeMs);
                        }
                    }
                    else
                    {
                        if (!nBmp.IsEqualTo(lastNBmp))
                        {
                            if (lastNBmp.Width > 0)
                            {
                                p.EndTime.TotalMilliseconds = timeMs;
                                subtitle.Paragraphs.Add(p);
                                imageList.Add(lastNBmp);
                            }

                            if (nBmp.Width > 0)
                            {
                                p = new Paragraph(string.Empty, timeMs, timeMs);
                            }
                        }
                        else 
                        {
                            p.EndTime.TotalMilliseconds = timeMs;
                        }
                        lastNBmp = nBmp;
                    }
                }
                count++;
                packetNumber += SkipPacketCount;
                callback?.Invoke(count, imageList.Count);
            }

            subtitle.Renumber();

            new FixOverlappingDisplayTimes().Fix(subtitle, new EmptyFixCallback());

            // fix small gaps
            for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                var current = subtitle.Paragraphs[i];
                var next = subtitle.Paragraphs[i + 1];
                if (!next.StartTime.IsMaxTime && !current.EndTime.IsMaxTime)
                {
                    var gap = next.StartTime.TotalMilliseconds - current.EndTime.TotalMilliseconds;
                    if (gap < MergeGapSmallerThanXMilliseconds)
                    {
                        current.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - SmallGapMilliseconds;
                    }
                }
            }

            return imageList;
        }
    }
}
