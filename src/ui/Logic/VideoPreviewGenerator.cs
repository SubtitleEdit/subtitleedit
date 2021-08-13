using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

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
                var process = GenerateVideoFile(previewFileName, 3, 720, 480, Color.Black, true, 25);
                process.Start();
                process.WaitForExit();

                return previewFileName;
            }
            catch
            {
                return null;
            }
        }

        public static Process GenerateVideoFile(string previewFileName, int seconds, int width, int height, Color color, bool checkered, decimal frameRate)
        {
            Process processMakeVideo;

            if (checkered)
            {
                const int rectangleSize = 9;
                var backgroundImage = TextDesigner.MakeBackgroundImage(width, height, rectangleSize, Configuration.Settings.General.UseDarkTheme);
                var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                backgroundImage.Save(tempImageFileName, ImageFormat.Png);
                processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, backgroundImage.Width, backgroundImage.Height, seconds, frameRate);
            }
            else
            {
                processMakeVideo = GetFFmpegProcess(color, previewFileName, width, height, seconds, frameRate);
            }

            return processMakeVideo;
        }

        /// <summary>
        /// Generate a video with a burned-in Advanced Sub Station Alpha subtitle.
        /// </summary>
        /// <param name="inputVideoFileName">Source video file name</param>
        /// <param name="assaSubtitleFileName">Source subtitle file name</param>
        /// <param name="outputVideoFileName">Output video file name with burned-in subtitle</param>
        public static Process GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName)
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
                    Arguments = $"-i \"{inputVideoFileName}\" -vf \"ass={Path.GetFileName(assaSubtitleFileName)}\" -strict -2 \"{outputVideoFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(assaSubtitleFileName),
                }
            };
        }

        private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
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
                    Arguments = $"-t {seconds} -loop 1 -r {frameRate.ToString(CultureInfo.InvariantCulture)} -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private static Process GetFFmpegProcess(Color color, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
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
                    Arguments = $"-t {seconds} -f lavfi -i color=c={htmlColor}:r={frameRate.ToString(CultureInfo.InvariantCulture)}:s={videoWidth}x{videoHeight} -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }
    }
}
