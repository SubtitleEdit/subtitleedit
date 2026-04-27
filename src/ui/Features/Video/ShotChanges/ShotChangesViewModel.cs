using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public partial class ShotChangesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TimeItem> _ffmpegLines;
    [ObservableProperty] private TimeItem? _selectedFfmpegLine;
    [ObservableProperty] private string? _importText;
    [ObservableProperty] private double _sensitivity;
    [ObservableProperty] private bool _timeCodeFrames;
    [ObservableProperty] private bool _timeCodeSeconds;
    [ObservableProperty] private bool _timeCodeMilliseconds;
    [ObservableProperty] private bool _timeCodeHhMmSsMs;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private bool _isGenerating;

    public Window? Window { get; set; }
    public double LastSeconds { get; private set; }
    public bool OkPressed { get; private set; }
    private StringBuilder Log { get; set; } = new StringBuilder();
    public DataGrid DataGridFfmpegLines { get; set; }
    public static readonly Regex TimeRegex = new Regex(@"pts_time:\d+[.,]*\d*", RegexOptions.Compiled);

    private string _videoFileName = string.Empty;
    private Process? _ffmpegProcess;
    private readonly System.Timers.Timer _timerGenerate;
    private bool _doAbort;
    private FfmpegMediaInfo2? _mediaInfo;
    private Lock TimeCodesLock = new Lock();
    private TimeCode? _duration;
    private double _frameRate;
    private static readonly char[] SplitChars = { ':', '.', ',' };

    public ShotChangesViewModel()
    {
        FfmpegLines = new ObservableCollection<TimeItem>();
        DataGridFfmpegLines = new DataGrid();
        ProgressText = string.Empty;
        TimeCodeSeconds = true;
        _frameRate = 23.976;
        LoadSettings();

        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;
    }

    private void LoadSettings()
    {
        Sensitivity = Se.Settings.Waveform.ShotChangesSensitivity;

        var timeCodeFormat = Se.Settings.Waveform.ShotChangesImportTimeCodeFormat;
        TimeCodeFrames = timeCodeFormat == "Frames";
        TimeCodeHhMmSsMs = timeCodeFormat == "HH:MM:SS.FFF";
        TimeCodeMilliseconds = timeCodeFormat == "Milliseconds";
        TimeCodeSeconds = !TimeCodeFrames && !TimeCodeHhMmSsMs && !TimeCodeMilliseconds;
    }

    private void SaveSettings()
    {
        Se.Settings.Waveform.ShotChangesSensitivity = Sensitivity;

        var timeCodeFormat = "Seconds";
        if (TimeCodeFrames)
        {
            timeCodeFormat = "Frames";
        }
        else if (TimeCodeHhMmSsMs)
        {
            timeCodeFormat = "HH:MM:SS.FFF";
        }
        else if (TimeCodeMilliseconds)
        {
            timeCodeFormat = "Milliseconds";
        }
        Se.Settings.Waveform.ShotChangesImportTimeCodeFormat = timeCodeFormat;

        Se.SaveSettings();
    }

    private void TimerGenerateElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        if (_doAbort)
        {
            _timerGenerate.Stop();
#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416

            IsGenerating = false;
            _doAbort = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            return;
        }

        _timerGenerate.Stop();
        if (FfmpegLines.Count > 0)
        {
            Ok();
        }
    }

    [RelayCommand]
    private void GenerateShotChangesFfmpeg()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            FfmpegLines.Clear();
        });

        IsGenerating = true;

        var threshold = Sensitivity.ToString(CultureInfo.InvariantCulture);
        var argumentsFormat = Se.Settings.Video.ShowChangesFFmpegArguments;
        var arguments = string.Format(argumentsFormat, _videoFileName, threshold);

        ProgressValue = 0;
        ProgressText = Se.Language.General.StartingDotDotDot;
        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        _timerGenerate.Start();
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
                lock (TimeCodesLock)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var item = new TimeItem(seconds, FfmpegLines.Count);
                        FfmpegLines.Add(item);
                        DataGridFfmpegLines.ScrollIntoView(item, null);

                        if (_duration != null)
                        {
                            var pct = seconds / _duration.TotalSeconds * 100;
                            ProgressValue = seconds / _duration.TotalSeconds * 100;
                            ProgressText = $"{pct:0}%";
                        }
                    });
                }
                LastSeconds = seconds;
            }
        }
    }

    [RelayCommand]
    private async Task ImportFromTextFile()
    {
        var fileName = await PickOpenFile();
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await LoadTextFile(fileName);
    }

    [RelayCommand]
    private void Ok()
    {
        if (!string.IsNullOrWhiteSpace(ImportText))
        {
            FfmpegLines.Clear();
            var lines = ImportText.SplitToLines().Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            foreach (var line in lines)
            {
                var s = line;
                if (TimeCodeHhMmSsMs)
                {
                    var seconds = GetSecondsFromHhMmSsMs(s);
                    if (seconds != null)
                    {
                        FfmpegLines.Add(new TimeItem((double)seconds, FfmpegLines.Count));
                    }
                    continue;
                }

                s = s.Replace(",", ".");
                if (double.TryParse(line, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
                {
                    if (TimeCodeFrames)
                    {
                        FfmpegLines.Add(new TimeItem(Math.Round(d / _frameRate, 3, MidpointRounding.AwayFromZero), FfmpegLines.Count));
                    }
                    else if (TimeCodeSeconds)
                    {
                        FfmpegLines.Add(new TimeItem(d, FfmpegLines.Count));
                    }
                    else if (TimeCodeMilliseconds)
                    {
                        FfmpegLines.Add(new TimeItem(d / TimeCode.BaseUnit, FfmpegLines.Count));
                    }
                }
            }
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            SaveSettings();
            OkPressed = true;
            Window?.Close();
        });
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsGenerating)
        {
            _doAbort = true;
            return;
        }

        Window?.Close();
    }

    /// <summary>
    /// Parse string (HH:MM:SS.ms) to seconds.
    /// </summary>
    private static double? GetSecondsFromHhMmSsMs(string line)
    {
        char[] splitChars = { ':', '.', ',' };

        string[] timeParts = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
        try
        {
            if (timeParts.Length == 2)
            {
                return new TimeSpan(0, 0, 0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1])).TotalSeconds;
            }
            else if (timeParts.Length == 3)
            {
                return new TimeSpan(0, 0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2])).TotalSeconds;
            }
            else if (timeParts.Length == 4)
            {
                return new TimeSpan(0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]), Convert.ToInt32(timeParts[3])).TotalSeconds;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private async Task LoadTextFile(string fileName)
    {
        try
        {
            var res = LoadFromMatroskaChapterFile(fileName);
            if (!string.IsNullOrEmpty(res))
            {
                ImportText = res;
                TimeCodeHhMmSsMs = true;
                return;
            }

            res = LoadFromEZTitlesShotchangesFile(fileName);
            if (!string.IsNullOrEmpty(res))
            {
                ImportText = res;
                TimeCodeFrames = true;
                return;
            }

            res = LoadFromJsonShotChangesFile(fileName);
            if (!string.IsNullOrEmpty(res))
            {
                ImportText = res;
                TimeCodeSeconds = true;
                return;
            }

            res = LoadFromJsonShotChangesFile2(fileName);
            if (!string.IsNullOrEmpty(res))
            {
                ImportText = res;
                TimeCodeSeconds = true;
                return;
            }

            var encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
            var s = System.IO.File.ReadAllText(fileName, encoding).Trim();
            if (s.Contains('.'))
            {
                TimeCodeSeconds = true;
            }

            if (s.Contains('.') && s.Contains(':'))
            {
                TimeCodeHhMmSsMs = true;
            }

            if (!s.Contains(Environment.NewLine) && s.Contains(';'))
            {
                var sb = new StringBuilder();
                foreach (var line in s.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        sb.AppendLine(line.Trim());
                    }
                }
                ImportText = sb.ToString();
            }
            else
            {
                ImportText = s;
            }
        }
        catch (Exception ex)
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, ex.Message);
        }
    }

    private static string? LoadFromMatroskaChapterFile(string fileName)
    {
        try
        {
            var x = new XmlDocument();
            x.Load(fileName);
            var xmlNodeList = x.SelectNodes("//ChapterAtom");
            var sb = new StringBuilder();
            if (xmlNodeList != null)
            {
                foreach (XmlNode chapter in xmlNodeList)
                {
                    var start = chapter.SelectSingleNode("ChapterTimeStart");
                    string[] timeParts = start?.InnerText?.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries) ?? [];
                    if (timeParts?.Length == 4)
                    {
                        if (timeParts[3].Length > 3)
                        {
                            timeParts[3] = timeParts[3].Substring(0, 3);
                        }

                        var ts = new TimeSpan(0, Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]), Convert.ToInt32(timeParts[2]), Convert.ToInt32(timeParts[3]));
                        sb.AppendLine(new TimeCode(ts).ToShortStringHHMMSSFF());
                    }
                }
            }
            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? LoadFromEZTitlesShotchangesFile(string fileName)
    {
        try
        {
            var x = new XmlDocument();
            x.Load(fileName);
            var xmlNodeList = x.SelectNodes("/shotchanges/shotchanges_list/shotchange");
            var sb = new StringBuilder();
            if (xmlNodeList != null)
            {
                foreach (XmlNode shotChange in xmlNodeList)
                {
                    if (shotChange.Attributes != null)
                    {
                        sb.AppendLine(shotChange.Attributes["frame"]?.InnerText);
                    }
                }
            }

            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string? LoadFromJsonShotChangesFile(string fileName)
    {
        try
        {
            var text = FileUtil.ReadAllTextShared(fileName, Encoding.UTF8);
            var list = new List<double>();
            foreach (var line in text.Split(','))
            {
                var s = line.Trim() + "}";
                var start = Json.ReadTag(s, "frame_time");
                if (start != null)
                {
                    if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds))
                    {
                        list.Add(startSeconds * 1000.0);
                    }
                }
            }

            var sb = new StringBuilder();
            foreach (var ms in list.OrderBy(p => p))
            {
                sb.AppendLine(new TimeCode(ms).ToShortStringHHMMSSFF());
            }

            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string? LoadFromJsonShotChangesFile2(string fileName)
    {
        try
        {
            var text = FileUtil.ReadAllTextShared(fileName, Encoding.UTF8);
            var list = new List<double>();
            var parser = new SeJsonParser();
            var arr = parser.GetArrayElementsByName(text, "shotchanges");

            foreach (var line in arr)
            {
                if (double.TryParse(line, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds))
                {
                    list.Add(startSeconds);
                }
            }

            var sb = new StringBuilder();
            foreach (var seconds in list.OrderBy(p => p))
            {
                sb.AppendLine(seconds.ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/shot-changes");
        }
    }

    internal void Initialize(string videoFileName)
    {
        _videoFileName = videoFileName;

        _ = Task.Run(() =>
        {
            _mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
            Dispatcher.UIThread.Post(() =>
            {
                _duration = _mediaInfo.Duration;
                if (_mediaInfo.FramesRate > 1)
                {
                    _frameRate = FrameRateHelper.RoundToNearestCinematicFrameRate((double)_mediaInfo.FramesRate);
                }
            });
        });
    }

    public async Task<string> PickOpenFile()
    {
        var topLevel = TopLevel.GetTopLevel(Window)!;

        var fileTypes = new List<FilePickerFileType>();
        fileTypes.Add(new FilePickerFileType(Se.Language.General.TextFiles)
        {
            Patterns = new List<string> { "*.txt", "*.shotchanges", "*.xml", "*.json", "*.dat" },
        });
        fileTypes.Add(new FilePickerFileType(Se.Language.General.AllFiles)
        {
            Patterns = new List<string> { "*.*" },
        });

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Se.Language.Video.ShotChanges.OpenShotChangesFile,
            AllowMultiple = false,
            FileTypeFilter = fileTypes,
        });

        if (files.Count >= 1)
        {
            return files[0].Path.LocalPath;
        }

        return string.Empty;
    }
}