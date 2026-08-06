using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Media
{
    public partial class FfmpegMediaInfo
    {
        public List<FfmpegTrackInfo> Tracks { get; set; }

        public Dimension Dimension { get; set; }
        public TimeCode Duration { get; set; }
        public decimal FramesRate { get; set; } 

        [GeneratedRegex(@"\d\d+x\d\d+")]
        private static partial Regex ResolutionRegexGen();
        private static readonly Regex ResolutionRegex = ResolutionRegexGen();

        [GeneratedRegex(@"Duration: \d+[:\.,]\d+[:\.,]\d+[:\.,]\d+")]
        private static partial Regex DurationRegexGen();
        private static readonly Regex DurationRegex = DurationRegexGen();

        [GeneratedRegex(@" \d+\.\d+ fps")]
        private static partial Regex Fps1RegexGen();
        private static readonly Regex Fps1Regex = Fps1RegexGen();

        [GeneratedRegex(@" \d+ fps")]
        private static partial Regex Fps2RegexGen();
        private static readonly Regex Fps2Regex = Fps2RegexGen();

        [GeneratedRegex(@"^Stream #\d+:(\d+)")]
        private static partial Regex StreamIndexRegexGen();
        private static readonly Regex StreamIndexRegex = StreamIndexRegexGen();

        private FfmpegMediaInfo()
        {
            Tracks = new List<FfmpegTrackInfo>();
            Duration = new TimeCode();
        }

        public static FfmpegMediaInfo Parse(string videoFileName)
        {
            if (Configuration.IsRunningOnWindows)
            {
                if (string.IsNullOrEmpty(Configuration.Settings.General.FFmpegLocation) ||
                    !File.Exists(Configuration.Settings.General.FFmpegLocation))
                {
                    return new FfmpegMediaInfo();
                }
            }

            var log = GetFfmpegLog(videoFileName);
            return ParseLog(log);
        }

        public long GetTotalFrames()
        {
            return (long)((double)FramesRate * Duration.TotalMilliseconds / TimeCode.BaseUnit);
        }

        /// <summary>
        /// True when the given audio track is 5.1/7.1/9.1 and therefore has a front-center
        /// channel. <paramref name="streamIndex"/> is the global ffmpeg stream index (the N in
        /// "Stream #0:N" / "-map 0:N"), which is what every caller has; a negative value means
        /// "no specific track" and falls back to the first audio track.
        /// </summary>
        public bool HasFrontCenterAudio(int streamIndex)
        {
            var audioTracks = Tracks.Where(track => track.TrackType == FfmpegTrackType.Audio).ToList();
            if (audioTracks.Count == 0)
            {
                return false;
            }

            var track = streamIndex < 0
                ? audioTracks[0]
                : audioTracks.FirstOrDefault(t => t.StreamIndex == streamIndex);
            if (track == null)
            {
                return false;
            }

            var info = track.TrackInfo;
            return info.Contains("5.1", StringComparison.Ordinal) ||
                   info.Contains("7.1", StringComparison.Ordinal) ||
                   info.Contains("9.1", StringComparison.Ordinal);
        }

        internal static FfmpegMediaInfo ParseLog(string log)
        {
            var info = new FfmpegMediaInfo();

            var fpsMatch = Fps1Regex.Match(log);
            if (!fpsMatch.Success)
            {
                fpsMatch = Fps2Regex.Match(log);
            }
            if (fpsMatch.Success)
            {
                var fps = fpsMatch.Value.Trim().Split(' ')[0];
                if (double.TryParse(fps, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                {
                    info.FramesRate = (decimal)f;
                }
            }

            foreach (var line in log.SplitToLines())
            {
                var s = line.Trim();
                if (s.StartsWith("Stream #", StringComparison.Ordinal))
                {
                    var resolutionMatch = ResolutionRegex.Match(s);
                    if (resolutionMatch.Success)
                    {
                        var parts = resolutionMatch.Value.Split('x');
                        if (info.Dimension.Width == 0 &&
                            parts.Length == 2 &&
                            int.TryParse(parts[0], out var w) &&
                            int.TryParse(parts[1], out var h))
                        {
                            info.Dimension = new Dimension(w, h); 
                        }
                    }

                    var arr = s.Replace(": ", "¤").Split('¤');
                    if (arr.Length == 3)
                    {
                        var trackType = arr[1].Trim();
                        var trackInfo = arr[2].Trim();
                        var streamIndexMatch = StreamIndexRegex.Match(s);
                        var streamIndex = streamIndexMatch.Success &&
                                          int.TryParse(streamIndexMatch.Groups[1].Value, out var si)
                            ? si
                            : -1;

                        if (trackType == FfmpegTrackType.Audio.ToString())
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Audio, TrackInfo = trackInfo, StreamIndex = streamIndex });
                        }
                        else if (trackType == FfmpegTrackType.Video.ToString())
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Video, TrackInfo = trackInfo, StreamIndex = streamIndex });
                        }
                        else if (trackType == FfmpegTrackType.Subtitle.ToString())
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Subtitle, TrackInfo = trackInfo, StreamIndex = streamIndex });
                        }
                        else
                        {
                            info.Tracks.Add(new FfmpegTrackInfo { TrackType = FfmpegTrackType.Other, TrackInfo = trackInfo, StreamIndex = streamIndex });
                        }
                    }
                }

                var match = DurationRegex.Match(line);
                if (match.Success)
                {
                    var timeCodeString = match.Value.Split(' ')[1];
                    info.Duration = new TimeCode(TimeCode.ParseToMilliseconds(timeCodeString));
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

        private static Process GetFFmpegProcess(string inputFileName)
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
                    Arguments = $"-i \"{inputFileName}\" -hide_banner",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
        }
    }
}
