using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class BinaryParagraphsToBluRaySup
    {
        public static void ConvertFromBinaryParagraphsToBluRaySup(string fileName, List<IBinaryParagraphWithPosition> sub, Subtitle subtitle, Point? resolution)
        {
            var screenWidth = 1920;
            var screenHeight = 1080;
            if (sub.Count > 0 && sub[0].GetScreenSize().Width > 0 && sub[0].GetScreenSize().Height > 0)
            {
                screenWidth = sub[0].GetScreenSize().Width;
                screenHeight = sub[0].GetScreenSize().Height;
            }
            var leftRightMargin = (int)Math.Round(screenWidth * 4.0 / 100.0);
            var bottomMargin = (int)Math.Round(screenHeight * 4.0 / 100.0);
            using (var binarySubtitleFile = new FileStream(fileName, FileMode.Create))
            {
                for (var index = 0; index < sub.Count; index++)
                {
                    var paragraph = subtitle.Paragraphs[index];
                    var p = sub[index];
                    var brSub = new BluRaySupPicture
                    {
                        StartTime = (long)Math.Round(paragraph.StartTime.TotalMilliseconds, MidpointRounding.AwayFromZero) ,
                        EndTime = (long)Math.Round(paragraph.EndTime.TotalMilliseconds, MidpointRounding.AwayFromZero),
                        Width = screenWidth,
                        Height = screenHeight,
                        IsForced = p.IsForced,
                        CompositionNumber = (index + 1) * 2,
                    };
                    var bitmap = p.GetBitmap();
                    var pos = p.GetPosition();
                    var overridePos = new Point(pos.Left, pos.Top);
                    var buffer = BluRaySupPicture.CreateSupFrame(brSub, bitmap, Configuration.Settings.General.CurrentFrameRate, bottomMargin, leftRightMargin, ContentAlignment.BottomCenter, overridePos);
                    binarySubtitleFile.Write(buffer, 0, buffer.Length);
                    bitmap?.Dispose();
                }
            }
        }
    }
}
