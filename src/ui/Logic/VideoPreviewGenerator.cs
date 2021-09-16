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
        public static Process GenerateHardcodedVideoFile(string inputVideoFileName, string assaSubtitleFileName, string outputVideoFileName, int width, int height, string videoEncoding, string preset, string crf, string audioEncoding, bool forceStereo, string sampleRate, string tune, string audioBitRate, string pass, string twoPassBitRate, DataReceivedEventHandler dataReceivedHandler = null)
        {
            var videoEncodingSettings = string.Empty;
            if (!string.IsNullOrEmpty(videoEncoding))
            {
                videoEncodingSettings = $"-c:v {videoEncoding}";
                if (videoEncoding == "libx265")
                {
                    videoEncodingSettings += " -tag:v hvc1";
                }
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

            var presetSettings = string.Empty;
            if (!string.IsNullOrEmpty(preset))
            {
                presetSettings = $" -preset {preset}";
            }

            var crfSettings = string.Empty;
            if (!string.IsNullOrEmpty(crf) && string.IsNullOrEmpty(pass))
            {
                crfSettings = $" -crf {crf}";
            }

            var tuneParameter = string.Empty;
            if (!string.IsNullOrEmpty(tune))
            {
                tuneParameter = $" -tune {tune}";
            }

            outputVideoFileName = $"\"{outputVideoFileName}\"";

            var passSettings = string.Empty;
            if (!string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(twoPassBitRate))
            {
                passSettings = $" -b:v {twoPassBitRate} -pass {pass}";

                if (!string.IsNullOrEmpty(audioBitRate))
                {
                    passSettings += $" -b:a {audioBitRate}";
                }

                if (pass == "1")
                {
                    outputVideoFileName = Configuration.IsRunningOnWindows ? "-f null NUL" : "-f null /dev/null";
                }
            }

            var processMakeVideo = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-i \"{inputVideoFileName}\" -vf \"ass={Path.GetFileName(assaSubtitleFileName)}\",yadif,format=yuv420p -g 30 -bf 2 -s {width}x{height} {videoEncodingSettings} {passSettings} {presetSettings} {crfSettings} {audioSettings}{tuneParameter} -use_editlist 0 -movflags +faststart {outputVideoFileName}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(assaSubtitleFileName) ?? string.Empty,
                }
            };

            processMakeVideo.StartInfo.Arguments = processMakeVideo.StartInfo.Arguments
                .Replace("  ", " ")
                .Replace("  ", " ")
                .Trim();

            SetupDataReceiveHandler(dataReceivedHandler, processMakeVideo);

            return processMakeVideo;
        }

        private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-t {seconds} -loop 1 -r {frameRate.ToString(CultureInfo.InvariantCulture)} -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private static Process GetFFmpegProcess(Color color, string outputFileName, int videoWidth, int videoHeight, int seconds, decimal frameRate)
        {
            var htmlColor = $"#{(color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToUpperInvariant()}";

            return new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-t {seconds} -f lavfi -i color=c={htmlColor}:r={frameRate.ToString(CultureInfo.InvariantCulture)}:s={videoWidth}x{videoHeight} -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        public static string GetScreenShot(string inputFileName, string timeCode)
        {
            var outputFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            var process = new Process
            {
                StartInfo =
                {
                    FileName = GetFfmpegLocation(),
                    Arguments = $"-ss {timeCode} -i \"{inputFileName}\" -frames:v 1 -q:v 2 \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return outputFileName;
        }

        private static string GetFfmpegLocation()
        {
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            return ffmpegLocation;
        }
    }
}
