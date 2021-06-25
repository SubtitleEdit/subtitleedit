using System;
using System.Diagnostics;
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
                const int rectangleSize = 9;
                var backgroundImage = TextDesigner.MakeBackgroundImage(720, 480, rectangleSize, Configuration.Settings.General.UseDarkTheme);
                var tempImageFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                backgroundImage.Save(tempImageFileName, ImageFormat.Png);
                var processMakeVideo = GetFFmpegProcess(tempImageFileName, previewFileName, backgroundImage.Width, backgroundImage.Height);
                processMakeVideo.Start();
                processMakeVideo.WaitForExit();
            }
            catch
            {
                return null; 
            }

            return File.Exists(previewFileName) ? previewFileName : null;
        }

        private static Process GetFFmpegProcess(string imageFileName, string outputFileName, int videoWidth, int videoHeight)
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
                    Arguments = $"-t 3 -loop 1 -i \"{imageFileName}\" -c:v libx264 -tune stillimage -shortest -s {videoWidth}x{videoHeight} \"{outputFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }
    }
}
