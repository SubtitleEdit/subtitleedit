using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class FfmpegMediaInfo
    {
        public List<FfmpegTrackInfo> Tracks { get; set; }

        private FfmpegMediaInfo()
        {
            Tracks = new List<FfmpegTrackInfo>();
        }

        public static FfmpegMediaInfo Parse(string videoFileName)
        {
            if (string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) ||
                 !File.Exists(Configuration.Settings.General.FFmpegLocation))
            {
                return new FfmpegMediaInfo();
            }

            var log = GetFfmpegLog(videoFileName);
            return ParseLog(log);
        }

        public bool HasFrontCenterAudio(int trackNumber)
        {
            if (trackNumber < 0)
            {
                trackNumber = 0;
            }

            var audioTracks = Tracks.Where(track => track.TrackType == FfmpegTrackType.Audio).ToList();
            if (trackNumber >= audioTracks.Count)
            {
                return false;
            }

            var info = audioTracks[trackNumber].TrackInfo;
            return info.Contains("5.1", StringComparison.Ordinal) ||
                   info.Contains("7.1", StringComparison.Ordinal);
        }

        internal static FfmpegMediaInfo ParseLog(string log)
        {
            var info = new FfmpegMediaInfo();

            foreach (var line in log.SplitToLines())
            {
                var s = line.Trim();
                if (s.StartsWith("Stream #", StringComparison.Ordinal))
                {
                    var arr = s.Replace(": ", "¤").Split('¤');
                    if (arr.Length == 3)
                    {
                        var trackType = arr[1].Trim();
                        var trackInfo = arr[2].Trim();
                        if (trackType == FfmpegTrackType.Audio.ToString())
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Audio, TrackInfo = trackInfo });
                        }
                        else if (trackType == FfmpegTrackType.Video.ToString())
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Video, TrackInfo = trackInfo });
                        }
                        else if (trackType == FfmpegTrackType.Subtitle.ToString())
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Subtitle, TrackInfo = trackInfo });
                        }
                        else
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Other, TrackInfo = trackInfo });
                        }
                    }
                }
            }

            return info;
        }

        private static string GetFfmpegLog(string videoFileName)
        {
            var sb = new StringBuilder();
            using (var process = GetFFmpegProcess(videoFileName))
            {
                process.OutputDataReceived += (sender, args) =>
                {
                    sb.AppendLine(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    sb.AppendLine(args.Data);
                };
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit(8000);
                System.Threading.Thread.Sleep(400);
            }

            System.Threading.Thread.Sleep(100);
            return sb.ToString();
        }

        public static Process GetFFmpegProcess(string inputFileName)
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
                    Arguments = $"-i \"{inputFileName}\" - hide_banner",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }
    }
}
