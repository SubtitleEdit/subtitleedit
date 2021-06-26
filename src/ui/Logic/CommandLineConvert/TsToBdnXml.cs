using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class TsToBdnXml
    {
        internal static void WriteTrack(string fileName, string outputFolder, bool overwrite, StreamWriter stdOutWriter, CommandLineConverter.BatchConvertProgress progressCallback, Point? resolution, ProgramMapTableParser programMapTableParser, int pid, TransportStreamParser tsParser)
        {
            var overrideScreenSize = Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize &&
                                     Configuration.Settings.Tools.BatchConvertTsScreenHeight > 0 &&
                                     Configuration.Settings.Tools.BatchConvertTsScreenWidth > 0 ||
                                     resolution.HasValue;

            using (var form = new ExportPngXml())
            {
                var language = TsToBluRaySup.GetFileNameEnding(programMapTableParser, pid);
                var nameNoExt = Utilities.GetFileNameWithoutExtension(fileName) + "." + language;
                var folder = Path.Combine(outputFolder, nameNoExt);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var outputFileName = CommandLineConverter.FormatOutputFileNameForBatchConvert(nameNoExt + Path.GetExtension(fileName), ".xml", folder, overwrite);
                stdOutWriter?.WriteLine($"Saving PID {pid} to {outputFileName}...");
                progressCallback?.Invoke($"Save PID {pid}");
                var sub = tsParser.GetDvbSubtitles(pid);
                var subtitle = new Subtitle();
                foreach (var p in sub)
                {
                    subtitle.Paragraphs.Add(new Paragraph(string.Empty, p.StartMilliseconds, p.EndMilliseconds));
                }

                var res = TsToBluRaySup.GetSubtitleScreenSize(sub, overrideScreenSize, resolution);
                var videoInfo = new VideoInfo { Success = true, Width = res.X, Height = res.Y };
                form.Initialize(subtitle, new SubRip(), BatchConvert.BdnXmlSubtitle, fileName, videoInfo, fileName);
                var sb = new StringBuilder();
                var imagesSavedCount = 0;
                for (int index = 0; index < sub.Count; index++)
                {
                    var p = sub[index];
                    var pos = p.GetPosition();
                    var bmp = sub[index].GetBitmap();
                    var tsWidth = bmp.Width;
                    var tsHeight = bmp.Height;
                    var nBmp = new NikseBitmap(bmp);
                    pos.Top += nBmp.CropTopTransparent(0);
                    pos.Left += nBmp.CropSidesAndBottom(0, Color.FromArgb(0, 0, 0, 0), true);
                    bmp.Dispose();
                    bmp = nBmp.GetBitmap();
                    var mp = form.MakeMakeBitmapParameter(index, videoInfo.Width, videoInfo.Height);

                    if (overrideScreenSize)
                    {
                        var widthFactor = (double)videoInfo.Width / tsWidth;
                        var heightFactor = (double)videoInfo.Height / tsHeight;
                        var resizeBmp = ResizeBitmap(bmp, (int)Math.Round(bmp.Width * widthFactor), (int)Math.Round(bmp.Height * heightFactor));
                        bmp.Dispose();
                        bmp = resizeBmp;
                        pos.Left = (int)Math.Round(pos.Left * widthFactor);
                        pos.Top = (int)Math.Round(pos.Top * heightFactor);
                        progressCallback?.Invoke($"Save PID {pid}: {(index + 1) * 100 / sub.Count}%");
                    }

                    mp.Bitmap = bmp;
                    mp.P = new Paragraph(string.Empty, p.StartMilliseconds, p.EndMilliseconds);
                    mp.ScreenWidth = videoInfo.Width;
                    mp.ScreenHeight = videoInfo.Height;
                    int bottomMarginInPixels;
                    if (Configuration.Settings.Tools.BatchConvertTsOverrideXPosition || Configuration.Settings.Tools.BatchConvertTsOverrideYPosition)
                    {
                        if (Configuration.Settings.Tools.BatchConvertTsOverrideXPosition && Configuration.Settings.Tools.BatchConvertTsOverrideYPosition)
                        {
                            var x = (int)Math.Round(videoInfo.Width / 2.0 - mp.Bitmap.Width / 2.0);
                            if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("left", StringComparison.OrdinalIgnoreCase))
                            {
                                x = Configuration.Settings.Tools.BatchConvertTsOverrideHMargin;
                            }
                            else if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("right", StringComparison.OrdinalIgnoreCase))
                            {
                                x = videoInfo.Width - Configuration.Settings.Tools.BatchConvertTsOverrideHMargin - mp.Bitmap.Width;
                            }

                            var y = videoInfo.Height - Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin - mp.Bitmap.Height;
                            mp.OverridePosition = new Point(x, y);
                        }
                        else if (Configuration.Settings.Tools.BatchConvertTsOverrideXPosition)
                        {
                            var x = (int)Math.Round(videoInfo.Width / 2.0 - mp.Bitmap.Width / 2.0);
                            if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("left", StringComparison.OrdinalIgnoreCase))
                            {
                                x = Configuration.Settings.Tools.BatchConvertTsOverrideHMargin;
                            }
                            else if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("right", StringComparison.OrdinalIgnoreCase))
                            {
                                x = videoInfo.Width - Configuration.Settings.Tools.BatchConvertTsOverrideHMargin - mp.Bitmap.Width;
                            }

                            mp.OverridePosition = new Point(x, pos.Top);
                        }
                        else
                        {
                            var y = videoInfo.Height - Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin - mp.Bitmap.Height;
                            mp.OverridePosition = new Point(pos.Left, y);
                        }

                        bottomMarginInPixels = Configuration.Settings.Tools.BatchConvertTsScreenHeight - pos.Top - mp.Bitmap.Height;
                    }
                    else
                    {
                        mp.OverridePosition = new Point(pos.Left, pos.Top); // use original position
                        bottomMarginInPixels = Configuration.Settings.Tools.BatchConvertTsScreenHeight - pos.Top - mp.Bitmap.Height;
                    }

                    imagesSavedCount = form.WriteBdnXmlParagraph(videoInfo.Width, sb, bottomMarginInPixels, videoInfo.Height, imagesSavedCount, mp, index, Path.GetDirectoryName(outputFileName));
                }

                form.WriteBdnXmlFile(imagesSavedCount, sb, outputFileName);
            }
        }

        private static Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(b, 0, 0, width, height);
            }

            return result;
        }
    }
}
