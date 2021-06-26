using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Logic
{
    public class VideoPreviewGenerator
    {
        public static string GetVideoPreviewFileName()
        {
            var previewFileName = Path.Combine(Configuration.DataDirectory, "preview.mkv");
            if (File.Exists(previewFileName))
            {
                return previewFileName;
            }

            try
            {
                var process = GenerateVideoFile(previewFileName, 3, 720, 480, Color.Black, true);
                process.Start();
                process.WaitForExit();

                return previewFileName;
            }
            catch
            {
                return null;
            }
        }

        public static Process GenerateVideoFile(string previewFileName, int seconds, int width, int height, Color color, bool checkered)
        {
            Process processMakeVideo;

            if (checkered)
            {
                const int rectangleSize = 9;
                var backgroundImage = TextDesigner.MakeBackgroundImage(width, height, rectangleSize, Configuration.Settings.General.UseDarkTheme);
                var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                backgroundImage.Save(tempImageFileName, ImageFormat.Png);
                processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, backgroundImage.Width, backgroundImage.Height, seconds);
            }
            else
            {
                processMakeVideo = GetFFmpegProcess(color, previewFileName, width, height, seconds);
            }

            return processMakeVideo;
        }

        private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds)
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-t {seconds} -loop 1 -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private static Process GetFFmpegProcess(Color color, string outputFileName, int videoWidth, int videoHeight, int seconds)
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            var htmlColor = $"#{(color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToUpperInvariant()}";

            return new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-t {seconds} -f lavfi -i color=c={htmlColor} -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }
    }
}
