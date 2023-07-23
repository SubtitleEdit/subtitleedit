using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Nikse.SubtitleEdit.Logic
{
    public class TimeCodesGenerator
    {
        private List<double> _timeCodes;
        public List<double> GetTimeCodes()
        {
            lock (TimeCodesLock)
            {
                return _timeCodes;
            }
        }

        private static readonly object TimeCodesLock = new object();
        public double LastSeconds { get; private set; }

        private string line;

        public TimeCodesGenerator()
        {
            _timeCodes = new List<double>();
        }

        public Process GetProcess(string videoFileName)
        {
            lock (TimeCodesLock)
            {
                _timeCodes = new List<double>();
            }

            var ffMpegLocation = Configuration.Settings.General.FFmpegLocation;
            var ffProbePath = !string.IsNullOrWhiteSpace(ffMpegLocation) ? Path.Combine(Path.GetDirectoryName(ffMpegLocation), "ffprobe.exe") : string.Empty;
            if (!Configuration.IsRunningOnWindows && !File.Exists(ffProbePath))
            {
                ffProbePath = "ffprobe";
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = ffProbePath,
                    Arguments = $"-select_streams v -show_frames -show_entries frame=pkt_dts_time -of csv -threads 0 \"{videoFileName}\"",
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
            line = outLine.Data;

            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (line.StartsWith("frame,") && !line.Contains("N/A"))
            {
                string timeString = line.Replace("frame,", "").Trim();
                if (timeString.Contains("side_data")) timeString = timeString.Substring(0, timeString.IndexOf("side_data", StringComparison.Ordinal));
                var timeCode = timeString.Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
                if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds))
                {
                    lock (TimeCodesLock)
                    {
                        _timeCodes.Add(seconds);
                    }
                    LastSeconds = seconds;
                }
            }
        }
    }
}