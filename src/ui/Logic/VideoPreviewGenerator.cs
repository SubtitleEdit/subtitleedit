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

        public static Process GenerateVideoFile(string previewFileName, int seconds, int width, int height, Color color, bool checkered, decimal frameRate, DataReceivedEventHandler dataReceivedHandler = null)
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

            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

            return processMakeVideo;
        }

        private static void SetupDataReceiveHandler(DataReceivedEventHandler dataReceivedHandler, Process processMakeVideo)
        {
            if (dataReceivedHandler != null)
            {
                processMakeVideo.StartInfo.RedirectStandardOutput = true;
                processMakeVideo.StartInfo.RedirectStandardError = true;
                processMakeVideo.OutputDataReceived += dataReceivedHandler;
                processMakeVideo.ErrorDataReceived += dataReceivedHandler;
            }
        }

        /// <summary>
        /// Generate a video with a burned-in Advanced Sub Station Alpha subtitle.
        /// </summary>
        public static Process GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName, int width, int height, string videoEnding, string preset, string crf, string audioEncoding, bool forceStereo, string sampleRate, DataReceivedEventHandler dataReceivedHandler = null)
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            var audioSettings = $"-c:a {audioEncoding}";
            if (audioEncoding != "copy")
            {
                audioSettings += $" -ar {sampleRate}";
                if (forceStereo)
                {
                    audioSettings += " -ac 2";
                }
            }

            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-i \"{inputVideoFileName}\" -vf \"ass={Path.GetFileName(assaSubtitleFileName)}\",yadif,format=yuv420p -bf 2 -strict -2 -s {width}x{height} \"{outputVideoFileName}\" -c:v {videoEnding} -preset {preset}  -crf {crf} {audioSettings}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(assaSubtitleFileName),
                }
            };

            processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments.Replace("  ", " ").Trim();

            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

            return processMakeVideo;
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
