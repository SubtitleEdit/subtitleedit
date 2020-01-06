using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic
{
    public class SceneChangesGenerator
    {
        public StringBuilder Log { get; private set; }
        public StringBuilder TimeCodes { get; private set; }
        public double LastSeconds { get; private set; }

        private static readonly Regex TimeRegex = new Regex(@"pts_time:\d+[.,]*\d*", RegexOptions.Compiled);

        public SceneChangesGenerator()
        {
            Log = new StringBuilder();
            TimeCodes = new StringBuilder();
        }

        public Process GetProcess(string videoFileName, decimal threshold)
        {
            Log = new StringBuilder();
            TimeCodes = new StringBuilder();
            var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
            {
                ffmpegLocation = "ffmpeg";
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-i \"{videoFileName}\" -vf \"select=gt(scene\\," + threshold.ToString(CultureInfo.InvariantCulture) + "),showinfo\" -vsync vfr -f null -",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += OutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            Log.AppendLine(outLine.Data);
            var match = TimeRegex.Match(outLine.Data);
            if (match.Success)
            {
                var timeCode = match.Value.Replace("pts_time:", string.Empty).Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
                if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds) && seconds > 0.2)
                {
                    TimeCodes.AppendLine(TimeCode.FromSeconds(seconds).ToShortString());
                    LastSeconds = seconds;
                }
            }
        }

        public static List<double> GetSeconds(string[] timeCodes)
        {
            char[] splitChars = { ':', '.', ',' };
            var seconds = new List<double>();
            foreach (string line in timeCodes)
            {
                // Parse string (HH:MM:SS.ms)
                string[] timeParts = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    if (timeParts.Length == 2)
                    {
                        seconds.Add(new TimeSpan(0, 0, 0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1])).TotalSeconds);
                    }
                    else if (timeParts.Length == 3)
                    {
                        seconds.Add(new TimeSpan(0, 0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2])).TotalSeconds);
                    }
                    else if (timeParts.Length == 4)
                    {
                        seconds.Add(new TimeSpan(0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]), Convert.ToInt32(timeParts[3])).TotalSeconds);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return seconds;
        }
    }
}

