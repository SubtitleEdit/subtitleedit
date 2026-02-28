using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.Media;

public class FfmpegMediaInfo2
{
    public List<FfmpegTrackInfo> Tracks { get; set; }
    public Dimension Dimension { get; set; }
    public TimeCode? Duration { get; set; }
    public decimal FramesRate { get; set; }
    public decimal FramesRateNonNormalized { get; set; }

    private static readonly Regex ResolutionRegex = new Regex(@"\d\d+x\d\d+", RegexOptions.Compiled);
    private static readonly Regex DurationRegex = new Regex(@"Duration: \d+[:\.,]\d+[:\.,]\d+[:\.,]\d+", RegexOptions.Compiled);
    private static readonly Regex Fps1Regex = new Regex(@" \d+\.\d+ fps", RegexOptions.Compiled);
    private static readonly Regex Fps2Regex = new Regex(@" \d+ fps", RegexOptions.Compiled);

    private FfmpegMediaInfo2()
    {
        Tracks = new List<FfmpegTrackInfo>();
    }

    public static FfmpegMediaInfo2 Parse(string videoFileName)
    {
        if (Configuration.IsRunningOnWindows)
        {
            if (string.IsNullOrEmpty(Se.Settings.General.FfmpegPath) ||
                !File.Exists(Se.Settings.General.FfmpegPath))
            {
                return new FfmpegMediaInfo2();
            }
        }

        var log = GetFfmpegLog(videoFileName);

        return ParseLog(log);
    }

    public long GetTotalFrames()
    {
        if (Duration == null)
        {
            return 0;
        }

        return (long)((double)FramesRateNonNormalized * Duration.TotalMilliseconds / TimeCode.BaseUnit);
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
               info.Contains("7.1", StringComparison.Ordinal) ||
               info.Contains("9.1", StringComparison.Ordinal);
    }

    private static FfmpegMediaInfo2 ParseLog(string log)
    {
        var info = new FfmpegMediaInfo2();

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
                info.FramesRateNonNormalized = (decimal)f;
                info.FramesRate = NormalizeFrameRate(f);
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

            var match = DurationRegex.Match(line);
            if (match.Success)
            {
                var timeCodeString = match.Value.Split(' ')[1];
                info.Duration = new TimeCode(TimeCode.ParseToMilliseconds(timeCodeString));
            }
        }

        return info;
    }

    /// <summary>
    /// Rounded to nearest known frame rate if close.
    /// </summary>
    private static decimal NormalizeFrameRate(double frameRate)
    {
        // Common video frame rates (including NTSC variants)
        double[] knownRates =
        {
            23.976, 24.0, 25.0, 29.97, 30.0, 48.0, 50.0, 59.94, 60.0, 120.0
        };

        for (double tolerance = 0.05; tolerance < 2; tolerance += 0.5)
        {
            foreach (var known in knownRates)
            {
                if (Math.Abs(frameRate - known) < tolerance)
                {
                    return (decimal)known;
                }
            }
        }

        // Not close to any known frame rate, return as-is
        return (decimal)frameRate;
    }

    private static string GetFfmpegLog(string videoFileName)
    {
        var log = RunFfmpegOnce(videoFileName, out int exitCode);

        if (exitCode != 0)
        {
            var retryLog = RunFfmpegOnce(videoFileName, out int retryExitCode);
            log += $"{Environment.NewLine}--- RETRY ({retryExitCode}) ---{Environment.NewLine}{retryLog}";
        }

        return log;
    }

    private static string RunFfmpegOnce(string videoFileName, out int exitCode)
    {
        var sb = new StringBuilder();
        exitCode = -1;

        using (var process = GetFFmpegProcess(videoFileName))
        {
            process.OutputDataReceived += (_, args) => { if (args.Data != null) sb.AppendLine(args.Data); };
            process.ErrorDataReceived += (_, args) => { if (args.Data != null) sb.AppendLine(args.Data); };
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(10_000))
            {
                try { process.Kill(true); } catch { /* ignore */ }
                sb.AppendLine("FFmpeg timed out after 10 seconds.");
            }

            process.WaitForExit(); // ensure we flush async output
            exitCode = process.ExitCode;
        }

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
                CreateNoWindow = true
            }
        };
    }
}