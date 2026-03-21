using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.BlankVideo;

public partial class BlankVideoViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private string _videoFileSize;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _useSourceResolution;
    [ObservableProperty] private ObservableCollection<double> _frameRates;
    [ObservableProperty] private double _selectedFrameRate;
    [ObservableProperty] private int _durationMinutes;
    [ObservableProperty] private bool _useCheckedImage;
    [ObservableProperty] private bool _useSolidColor;
    [ObservableProperty] private Color _solidColor;
    [ObservableProperty] private bool _useBackgroundImage;
    [ObservableProperty] private string _backgroundImageFileName;
    [ObservableProperty] private bool _generateTimeCodes;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new();
    private readonly StringBuilder _log;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _fullBackgroundImageFileName;
    private string _subtitleFileName;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public BlankVideoViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        VideoWidth = 1280;
        VideoHeight = 720;

        DurationMinutes = 2;

        UseCheckedImage = true;

        FrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        SelectedFrameRate = FrameRates[0];

        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;
        BackgroundImageFileName = string.Empty;
        JobItems = new ObservableCollection<BurnInJobItem>();

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 200;

        _subtitleFileName = string.Empty;
        _fullBackgroundImageFileName = string.Empty;
        LoadSettings();
    }

    public void Initialize(string subtitleFileName, SubtitleFormat subtitleFormat)
    {
        _subtitleFileName = subtitleFileName;
        _subtitleFormat = subtitleFormat;
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
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0,
                MidpointRounding.AwayFromZero);
            percentage = Math.Clamp(percentage, 0, 100);

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * JobItems[_jobItemIndex].TotalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            if (JobItems.Count == 1)
            {
                ProgressText = $"Generating video... {percentage}%     {estimatedLeft}";
            }
            else
            {
                ProgressText = $"Generating video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            }

            return;
        }

        _timerGenerate.Stop();
        ProgressValue = 100;
        ProgressText = string.Empty;

        var jobItem = JobItems[_jobItemIndex];

        if (!File.Exists(jobItem.OutputVideoFileName))
        {
            SeLogger.Error("Output video file not found: " + jobItem.OutputVideoFileName + Environment.NewLine +
                                 "ffmpeg: " + _ffmpegProcess.StartInfo.FileName + Environment.NewLine +
                                 "Parameters: " + _ffmpegProcess.StartInfo.Arguments + Environment.NewLine +
                                 "OS: " + Environment.OSVersion + Environment.NewLine +
                                 "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                                 "ffmpeg exit code: " + _ffmpegProcess.ExitCode + Environment.NewLine +
                                 "ffmpeg log: " + _log);

            Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!,
                    "Unable to generate blank video",
                    "Output video file not generated: " + jobItem.OutputVideoFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsGenerating = true;
                ProgressValue = 0;
            });

            return;
        }

        JobItems[_jobItemIndex].Status = Se.Language.General.Done;

        Dispatcher.UIThread.Invoke(async () =>
        {
            ProgressValue = 0;

            if (_jobItemIndex < JobItems.Count - 1)
            {
                InitAndStartJobItem(_jobItemIndex + 1);
                return;
            }

            IsGenerating = false;

            if (JobItems.Count == 1)
            {
                await _folderHelper.OpenFolderWithFileSelected(Window!, jobItem.OutputVideoFileName);
            }
            else
            {
                var sb = new StringBuilder($"Generated files ({JobItems.Count}):" + Environment.NewLine + Environment.NewLine);
                foreach (var item in JobItems)
                {
                    sb.AppendLine($"{item.OutputVideoFileName} ==> {item.Status}");
                }

                await MessageBox.Show(Window!,
                    "Generating done",
                    sb.ToString(),
                    MessageBoxButtons.OK);
            }
        });
    }

    private bool RunEncoding(BurnInJobItem jobItem)
    {
        _ffmpegProcess = GetFfmpegProcess(jobItem);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private Process GetFfmpegProcess(BurnInJobItem jobItem)
    {
        var totalMs = jobItem.TotalSeconds * 1000; // _subtitle.Paragraphs.Max(p => p.EndTime.TotalMilliseconds);
        var ts = TimeSpan.FromMilliseconds(totalMs + 2000);
        var timeCode = string.Format($"{ts.Hours:00}\\\\:{ts.Minutes:00}\\\\:{ts.Seconds:00}");

        var addTimeColor = "white";
        if (UseCheckedImage)
        {
            addTimeColor = "black";
        }

        Bitmap? bmp = null;
        if (UseBackgroundImage && !string.IsNullOrEmpty(_fullBackgroundImageFileName) && File.Exists(_fullBackgroundImageFileName))
        {
            try
            {
                bmp = new Bitmap(_fullBackgroundImageFileName);
            }
            catch (Exception ex)
            {
                Se.LogError(ex);
            }
        }

        return FfmpegGenerator.GenerateVideoFile(
               VideoFileName,
               DurationMinutes * 60,
               jobItem.Width,
               jobItem.Height,
               SolidColor,
               UseCheckedImage,
               (decimal)SelectedFrameRate,
               bmp,
               OutputHandler,
               GenerateTimeCodes,
               addTimeColor);
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        _log?.AppendLine(outLine.Data);

        var match = FrameFinderRegex.Match(outLine.Data);
        if (!match.Success)
        {
            return;
        }

        var arr = match.Value.Split('=');
        if (arr.Length != 2)
        {
            return;
        }

        if (long.TryParse(arr[1].Trim(), out var f))
        {
            _processedFrames = f;
            ProgressValue = (double)_processedFrames * 100.0 / JobItems[_jobItemIndex].TotalFrames;
        }
    }

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems()
    {
        var subtitle = new Subtitle(_subtitle);

        var srt = new SubRip();
        var subtitleFileName = Path.Combine(Path.GetTempFileName() + srt.Extension);
        if (_subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat })
        {
            var assa = new AdvancedSubStationAlpha();
            subtitleFileName = Path.Combine(Path.GetTempFileName() + assa.Extension);
            File.WriteAllText(subtitleFileName, assa.ToText(subtitle, string.Empty));
        }
        else
        {
            File.WriteAllText(subtitleFileName, srt.ToText(subtitle, string.Empty));
        }

        var jobItem = new BurnInJobItem(string.Empty, VideoWidth, VideoHeight)
        {
            InputVideoFileName = VideoFileName,
            OutputVideoFileName = VideoFileName,
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    [RelayCommand]
    private async Task Add()
    {
        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window!, Se.Language.General.OpenSubtitles);
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var videoFileName = string.Empty;
            var jobItem = new BurnInJobItem(videoFileName, VideoWidth, VideoHeight);
            jobItem.AddSubtitleFileName(fileName);
            Dispatcher.UIThread.Invoke(() => { JobItems.Add(jobItem); });
        }
    }

    [RelayCommand]
    private async Task OpenOutputFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.Settings.Video.BurnIn.OutputFolder);
    }

    [RelayCommand]
    private async Task BrowseImage()
    {
        var fileName = await _fileHelper.PickOpenImageFile(Window!, Se.Language.General.OpenImageFile);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        BackgroundImageFileName = Path.GetFileName(fileName);
        _fullBackgroundImageFileName = fileName;
    }

    [RelayCommand]
    private async Task BrowseResolution()
    {
        var result = await _windowService.ShowDialogAsync<BurnInResolutionPickerWindow, BurnInResolutionPickerViewModel>(Window!);
        if (!result.OkPressed || result.SelectedResolution == null)
        {
            return;
        }

        if (result.SelectedResolution.ItemType == ResolutionItemType.PickResolution)
        {
            var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, "Open video file");
            if (string.IsNullOrWhiteSpace(videoFileName))
            {
                return;
            }

            var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
            VideoWidth = mediaInfo.Dimension.Width;
            VideoHeight = mediaInfo.Dimension.Height;
            UseSourceResolution = false;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.UseSource)
        {
            UseSourceResolution = true;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.Resolution)
        {
            UseSourceResolution = false;
            VideoWidth = result.SelectedResolution.Width;
            VideoHeight = result.SelectedResolution.Height;
        }

        SaveSettings();
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (UseBackgroundImage && string.IsNullOrEmpty(_fullBackgroundImageFileName))
        {
            await MessageBox.Show(Window!, "Background image file not selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (UseBackgroundImage && !File.Exists(_fullBackgroundImageFileName))
        {
            await MessageBox.Show(Window!, "Background image file does not exist: " + _fullBackgroundImageFileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var videoFileName = UseSolidColor ? "blank_video_solid" : (UseCheckedImage ? "blank_video_checkered" : "blank_video_image");
        var path = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, videoFileName + ".mkv");
        if (!string.IsNullOrEmpty(_subtitleFileName))
        {
            path = Path.Combine(Path.GetDirectoryName(_subtitleFileName) ?? Se.Settings.Video.BurnIn.OutputFolder, videoFileName + ".mkv");
        }

        videoFileName = await _fileHelper.PickSaveFile(Window!, ".mkv", videoFileName, Se.Language.Video.SaveVideoAsTitle);
        if (string.IsNullOrEmpty(videoFileName))
        {
            return;
        }

        VideoFileName = videoFileName;
        if (File.Exists(VideoFileName))
        {
            File.Delete(VideoFileName);
        }

        JobItems = GetCurrentVideoAsJobItems();

        if (JobItems.Count == 0)
        {
            return;
        }

        _doAbort = false;
        _log.Clear();
        IsGenerating = true;
        _processedFrames = 0;
        ProgressValue = 0;
        SaveSettings();
        InitAndStartJobItem(0);
    }

    private void InitAndStartJobItem(int index)
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _jobItemIndex = index;
        var jobItem = JobItems[index];
        var totalFrames = (long)Math.Round(SelectedFrameRate * DurationMinutes * 60.0f);
        jobItem.TotalFrames = totalFrames;
        jobItem.TotalSeconds = DurationMinutes * 60.0f;
        jobItem.Width = VideoWidth;
        jobItem.Height = VideoHeight;
        jobItem.Status = Se.Language.General.Generating;
        jobItem.OutputVideoFileName = VideoFileName;

        var result = RunEncoding(jobItem);
        if (result)
        {
            _timerGenerate.Start();
        }
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        UseSourceResolution = settings.UseSourceResolution;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        settings.UseSourceResolution = UseSourceResolution;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
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
            UiUtil.ShowHelp("features/blank-video");
        }
    }
}