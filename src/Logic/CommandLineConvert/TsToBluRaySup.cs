using System;
using System.Drawing;
using System.IO;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.TransportStream;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class TsToBluRaySup
    {
        public static bool ConvertFromTsToBluRaySup(string fileName, string outputFolder, bool overwrite, StreamWriter stdOutWriter, CommandLineConverter.BatchConvertProgress progressCallback)
        {
            var tsParser = new TransportStreamParser();
            tsParser.Parse(fileName, (position, total) =>
            {
                var percent = (int)Math.Round(position * 100.0 / total);
                stdOutWriter?.Write("\rParsing transport stream: {0}%", percent);
                progressCallback?.Invoke($"{percent}%");
            });
            stdOutWriter?.Write("\r".PadRight(32, ' '));
            stdOutWriter?.Write("\r");
            var videoInfo = UiUtil.GetVideoInfo(fileName);
            int width = 720;
            int height = 576;
            if (videoInfo.Success && videoInfo.Width > 0 && videoInfo.Height > 0)
            {
                width = videoInfo.Width;
                height = videoInfo.Height;
            }

            if (Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize && 
                Configuration.Settings.Tools.BatchConvertTsScreenWidth > 0 &&
                Configuration.Settings.Tools.BatchConvertTsScreenHeight > 0)
            {
                width = Configuration.Settings.Tools.BatchConvertTsScreenWidth;
                height = Configuration.Settings.Tools.BatchConvertTsScreenHeight;
            }
            using (var form = new ExportPngXml())
            {
                if (tsParser.SubtitlePacketIds.Count == 0)
                {
                    stdOutWriter?.WriteLine($"No subtitles found");
                    progressCallback?.Invoke($"No subtitles found");
                    return false;
                }
                form.Initialize(new Subtitle(), new SubRip(), BatchConvert.BluRaySubtitle, fileName, videoInfo, fileName);
                foreach (int pid in tsParser.SubtitlePacketIds)
                {
                    var outputFileName = CommandLineConverter.FormatOutputFileNameForBatchConvert(Utilities.GetPathAndFileNameWithoutExtension(fileName) + "-" + pid + Path.GetExtension(fileName), ".sup", outputFolder, overwrite);
                    stdOutWriter?.WriteLine($"Saving PID {pid} to {outputFileName}...");
                    progressCallback?.Invoke($"Save PID {pid}");

                    var sub = tsParser.GetDvbSubtitles(pid);
                    using (var binarySubtitleFile = new FileStream(outputFileName, FileMode.Create))
                    {
                        for (int index = 0; index < sub.Count; index++)
                        {
                            var p = sub[index];
                            var pos = p.GetPosition();
                            var bmp = sub[index].GetBitmap();
                            var nbmp = new NikseBitmap(bmp);
                            pos.Top += nbmp.CropTopTransparent(0);
                            pos.Left += nbmp.CropSidesAndBottom(0, Color.FromArgb(0, 0, 0, 0), true);
                            bmp.Dispose();
                            bmp = nbmp.GetBitmap();
                            var mp = form.MakeMakeBitmapParameter(index, width, height);
                            mp.Bitmap = bmp;
                            mp.P = new Paragraph(string.Empty, p.StartMilliseconds, p.EndMilliseconds);
                            mp.ScreenWidth = width;
                            mp.ScreenHeight = height;
                            if (Configuration.Settings.Tools.BatchConvertTsOverridePosition || 
                                Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize &&
                                Configuration.Settings.Tools.BatchConvertTsScreenWidth > 0 &&
                                Configuration.Settings.Tools.BatchConvertTsScreenHeight > 0)
                            {
                                mp.BottomMargin = Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin;
                                mp.Alignment = ContentAlignment.BottomCenter;
                            }
                            else
                            {
                                mp.OverridePosition = new Point(pos.Left, pos.Top); // use original position
                            }
                            ExportPngXml.MakeBluRaySupImage(mp);
                            binarySubtitleFile.Write(mp.Buffer, 0, mp.Buffer.Length);
                            if (mp.Bitmap != null)
                            {
                                mp.Bitmap.Dispose();
                                mp.Bitmap = null;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
