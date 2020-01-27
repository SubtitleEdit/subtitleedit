using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;

namespace Nikse.SubtitleEdit.Logic.CommandLineConvert
{
    public static class TsToBluRaySup
    {
        public static bool ConvertFromTsToBluRaySup(string fileName, string outputFolder, bool overwrite, int count, StreamWriter stdOutWriter, CommandLineConverter.BatchConvertProgress progressCallback, Point? resolution)
        {
            var programMapTableParser = new ProgramMapTableParser();
            programMapTableParser.Parse(fileName); // get languages from PMT if possible
            var tsParser = new TransportStreamParser();
            tsParser.Parse(fileName, (position, total) =>
            {
                var percent = (int)Math.Round(position * 100.0 / total);
                stdOutWriter?.Write("\rParsing transport stream: {0}%", percent);
                progressCallback?.Invoke($"{percent}%");
            });
            stdOutWriter?.Write("\r".PadRight(32, ' '));
            stdOutWriter?.Write("\r");

            var overrideScreenSize = Configuration.Settings.Tools.BatchConvertTsOverrideScreenSize &&
                                     Configuration.Settings.Tools.BatchConvertTsScreenHeight > 0 &&
                                     Configuration.Settings.Tools.BatchConvertTsScreenWidth > 0 ||
                                     resolution.HasValue;

            using (var form = new ExportPngXml())
            {
                if (tsParser.SubtitlePacketIds.Count == 0)
                {
                    stdOutWriter?.WriteLine("No subtitles found");
                    progressCallback?.Invoke("No subtitles found");
                    return false;
                }
                form.Initialize(new Subtitle(), new SubRip(), BatchConvert.BluRaySubtitle, fileName, null, fileName);
                foreach (int pid in tsParser.SubtitlePacketIds)
                {
                    var language = GetFileNameEnding(programMapTableParser, pid);
                    var outputFileName = CommandLineConverter.FormatOutputFileNameForBatchConvert(Utilities.GetPathAndFileNameWithoutExtension(fileName) + language + Path.GetExtension(fileName), ".sup", outputFolder, overwrite);
                    stdOutWriter?.Write($"{count}: {Path.GetFileName(fileName)} -> PID {pid} to {outputFileName}...");
                    var sub = tsParser.GetDvbSubtitles(pid);
                    progressCallback?.Invoke($"Save PID {pid}");
                    var subtitleScreenSize = GetSubtitleScreenSize(sub, overrideScreenSize, resolution);
                    using (var binarySubtitleFile = new FileStream(outputFileName, FileMode.Create))
                    {
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
                            var mp = form.MakeMakeBitmapParameter(index, subtitleScreenSize.X, subtitleScreenSize.Y);

                            if (overrideScreenSize)
                            {
                                var widthFactor = (double)subtitleScreenSize.X / tsWidth;
                                var heightFactor = (double)subtitleScreenSize.Y / tsHeight;
                                var resizeBmp = ResizeBitmap(bmp, (int)Math.Round(bmp.Width * widthFactor), (int)Math.Round(bmp.Height * heightFactor));
                                bmp.Dispose();
                                bmp = resizeBmp;
                                pos.Left = (int)Math.Round(pos.Left * widthFactor);
                                pos.Top = (int)Math.Round(pos.Top * heightFactor);
                                progressCallback?.Invoke($"Save PID {pid}: {(index + 1) * 100 / sub.Count}%");
                            }

                            mp.Bitmap = bmp;
                            mp.P = new Paragraph(string.Empty, p.StartMilliseconds, p.EndMilliseconds);
                            mp.ScreenWidth = subtitleScreenSize.X;
                            mp.ScreenHeight = subtitleScreenSize.Y;
                            if (Configuration.Settings.Tools.BatchConvertTsOverrideXPosition || Configuration.Settings.Tools.BatchConvertTsOverrideYPosition)
                            {
                                var overrideMarginX = (int)Math.Round(Configuration.Settings.Tools.BatchConvertTsOverrideHMargin * subtitleScreenSize.X / 100.0);
                                var overrideMarginY = (int)Math.Round(Configuration.Settings.Tools.BatchConvertTsOverrideBottomMargin * subtitleScreenSize.Y / 100.0);
                                if (Configuration.Settings.Tools.BatchConvertTsOverrideXPosition && Configuration.Settings.Tools.BatchConvertTsOverrideYPosition)
                                {
                                    var x = (int)Math.Round(subtitleScreenSize.X / 2.0 - mp.Bitmap.Width / 2.0);
                                    if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("left", StringComparison.OrdinalIgnoreCase))
                                    {
                                        x = overrideMarginX;
                                    }
                                    else if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("right", StringComparison.OrdinalIgnoreCase))
                                    {
                                        x = subtitleScreenSize.X - overrideMarginX - mp.Bitmap.Width;
                                    }
                                    var y = subtitleScreenSize.Y - overrideMarginY - mp.Bitmap.Height;
                                    mp.OverridePosition = new Point(x, y);
                                }
                                else if (Configuration.Settings.Tools.BatchConvertTsOverrideXPosition)
                                {
                                    var x = (int)Math.Round(subtitleScreenSize.X / 2.0 - mp.Bitmap.Width / 2.0);
                                    if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("left", StringComparison.OrdinalIgnoreCase))
                                    {
                                        x = overrideMarginX;
                                    }
                                    else if (Configuration.Settings.Tools.BatchConvertTsOverrideHAlign.Equals("right", StringComparison.OrdinalIgnoreCase))
                                    {
                                        x = subtitleScreenSize.X - overrideMarginX - mp.Bitmap.Width;
                                    }
                                    mp.OverridePosition = new Point(x, pos.Top);
                                }
                                else
                                {
                                    var y = subtitleScreenSize.Y - overrideMarginY - mp.Bitmap.Height;
                                    mp.OverridePosition = new Point(pos.Left, y);
                                }
                            }
                            else
                            {
                                mp.OverridePosition = new Point(pos.Left, pos.Top); // use original position (can be scaled)
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
                    stdOutWriter?.WriteLine(" done.");
                }
            }
            return true;
        }

        public static string GetFileNameEnding(ProgramMapTableParser pmt, int pid)
        {
            var twoLetter = pmt.GetSubtitleLanguageTwoLetter(pid);
            var threeLetter = pmt.GetSubtitleLanguage(pid);
            if (string.IsNullOrEmpty(twoLetter))
            {
                twoLetter = threeLetter;
            }
            if (string.IsNullOrEmpty(threeLetter))
            {
                twoLetter = pid.ToString(CultureInfo.InvariantCulture);
                threeLetter = pid.ToString(CultureInfo.InvariantCulture);
            }

            return Configuration.Settings.Tools.BatchConvertTsFileNameAppend
                .Replace("{two-letter-country-code}", twoLetter)
                .Replace("{two-letter-country-code-uppercase}", twoLetter.ToUpperInvariant())
                .Replace("{three-letter-country-code}", threeLetter)
                .Replace("{three-letter-country-code-uppercase}", threeLetter);
        }

        public static Point GetSubtitleScreenSize(List<TransportStreamSubtitle> sub, bool overrideScreenSize, Point? resolution)
        {
            if (resolution.HasValue)
            {
                return new Point(resolution.Value.X, resolution.Value.Y);
            }

            if (overrideScreenSize)
            {
                return new Point(Configuration.Settings.Tools.BatchConvertTsScreenWidth, Configuration.Settings.Tools.BatchConvertTsScreenHeight);
            }

            if (sub.Count < 1)
            {
                return new Point(DvbSubPes.DefaultScreenWidth, DvbSubPes.DefaultScreenHeight);
            }

            using (var bmp = sub[0].GetBitmap())
            {
                return new Point(bmp.Width, bmp.Height);
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
