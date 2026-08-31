using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Video.ReEncodeVideo;

public partial class ReEncodeVideoViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private string _videoFileSize;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private ObservableCollection<double> _frameRates;
    [ObservableProperty] private double _selectedFrameRate;
    [ObservableProperty] private ObservableCollection<string> _videoExtensions;
    [ObservableProperty] private string _selectedVideoExtension;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _useSourceResolution;
    [ObservableProperty] private string _infoText;
    [ObservableProperty] private bool _promptForFfmpegParameters;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new();
    private readonly StringBuilder _log;
    private readonly TempSubtitleFiles _tempSubtitleFiles = new();
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public ReEncodeVideoViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        VideoWidth = 1280;
        VideoHeight = 720;

        FrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        SelectedFrameRate = FrameRates[0];

        // No .webm: this always encodes libx264, and the WebM muxer takes only VP8/VP9/AV1, so
        // picking it could only ever end in "Output video file not generated".
        VideoExtensions = new ObservableCollection<string>
        {
            ".mkv",
            ".mp4",
        };
        SelectedVideoExtension = VideoExtensions[0];

        JobItems = new ObservableCollection<BurnInJobItem>();

        InfoText = Se.Language.Video.ReEncodeInfo;

        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;

        _inputVideoFileName = string.Empty;
        LoadSettings();
    }

    public void Initialize(string videoFileName, SubtitleFormat subtitleFormat)
    {
        VideoFileName = videoFileName;
        _inputVideoFileName = videoFileName;
        _subtitleFormat = subtitleFormat;
        var fileExists = !string.IsNullOrWhiteSpace(videoFileName) && File.Exists(videoFileName);
        if (fileExists)
        {
            VideoFileSize = Utilities.FormatBytesToDisplayFileSize(new FileInfo(videoFileName).Length);
            _ = Task.Run(() =>
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
                Dispatcher.UIThread.Post(() =>
                {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
                    if (mediaInfo == null || mediaInfo.Dimension == null || mediaInfo.Dimension.Width <= 0)
                    {
                        VideoWidth = 1280;
                        VideoHeight = 720;
                        UseSourceResolution = false;
                        return;
                    }
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

                    var frameRate = FrameRateHelper.RoundToNearestCinematicFrameRate((double)mediaInfo.FramesRate);
                    SelectedFrameRate = frameRate;

                    if (mediaInfo.Dimension.Width > 1280)
                    {
                        double scaleFactor = 1280.0 / mediaInfo.Dimension.Width;
                        VideoWidth = (int)(mediaInfo.Dimension.Width * scaleFactor);
                        VideoHeight = (int)(mediaInfo.Dimension.Height * scaleFactor);
                        UseSourceResolution = false;
                    }
                });
            });
        }
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
                ProgressText = string.Format(Se.Language.Video.ReEncodeGeneratingVideoX, percentage, estimatedLeft);
            }
            else
            {
                ProgressText = string.Format(Se.Language.Video.ReEncodeGeneratingVideoXofY, _jobItemIndex + 1, JobItems.Count, percentage, estimatedLeft);
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
                    Se.Language.Video.ReEncodeUnableToGenerateVideo,
                    string.Format(Se.Language.Video.ReEncodeOutputVideoFileNotGenerated, jobItem.OutputVideoFileName, _ffmpegProcess.StartInfo.Arguments),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsGenerating = false;
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
                var result = await InitAndStartJobItem(_jobItemIndex + 1);
                return;
            }

            IsGenerating = false;

            if (JobItems.Count == 1)
            {
                await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window!, vm =>
                {
                    vm.Initialize(
                        Se.Language.General.VideoFileGenerated,
                        string.Format(Se.Language.General.VideoFileGeneratedX, jobItem.OutputVideoFileName),
                        jobItem.OutputVideoFileName,
                        true,
                        true);
                });
            }
            else
            {
                var sb = new StringBuilder(string.Format(Se.Language.Video.ReEncodeGeneratedFilesX, JobItems.Count) + Environment.NewLine +
                                           Environment.NewLine);
                foreach (var item in JobItems)
                {
                    sb.AppendLine($"{item.OutputVideoFileName} ==> {item.Status}");
                }

                await MessageBox.Show(Window!,
                    Se.Language.Video.ReEncodeGeneratingDone,
                    sb.ToString(),
                    MessageBoxButtons.OK);
            }
        });
    }

    private async Task<bool> InitAndStartJobItem(int index)
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _jobItemIndex = index;
        var jobItem = JobItems[index];
        var mediaInfo = FfmpegMediaInfo.Parse(jobItem.InputVideoFileName);
        jobItem.TotalFrames = mediaInfo.GetTotalFrames();
        jobItem.TotalSeconds = mediaInfo.Duration.TotalSeconds;

        // Only adopt the source size when the user asked for it. This dialog exists to produce a
        // SMALLER file ("high resolutions make subtitling slow"), and Initialize scales large
        // sources down to 1280 wide - unconditionally overwriting both here threw that away, so
        // every re-encode ran at the source resolution and the resolution picker did nothing.
        // The burn-in dialog was fixed the same way.
        if (mediaInfo.Dimension.Width > 0 && mediaInfo.Dimension.Height > 0 &&
            (UseSourceResolution || jobItem.Width <= 0 || jobItem.Height <= 0))
        {
            jobItem.Width = mediaInfo.Dimension.Width;
            jobItem.Height = mediaInfo.Dimension.Height;
        }
        jobItem.UseTargetFileSize = false;
        jobItem.Status = Se.Language.General.Generating;

        var result = await RunEncoding(jobItem);
        if (result)
        {
            _timerGenerate.Start();
        }

        return result;
    }

    private async Task<bool> RunEncoding(BurnInJobItem jobItem)
    {
        var ffmpegParameters = FfmpegGenerator.GetReEncodeVideoForSubtitlingParameters(
                   jobItem.InputVideoFileName,
                   jobItem.OutputVideoFileName,
                   jobItem.Width,
                   jobItem.Height,
                   SelectedFrameRate.ToString(CultureInfo.InvariantCulture));

        if (PromptForFfmpegParameters)
        {
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(Se.Language.Video.ReEncodeFfmpegParameters, ffmpegParameters, 1000, 200);
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return false;
            }

            ffmpegParameters = result.Text.Trim();
        }

        // Machine-readable progress on stdout ("frame=N" etc.) instead of scraping the
        // human-readable stderr stats, which drift between ffmpeg versions.
        ffmpegParameters = FfmpegProgressTracker.ProgressArguments + " " + ffmpegParameters;

        _ffmpegProcess = FfmpegGenerator.GetProcess(ffmpegParameters, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        if (FfmpegProgressTracker.TryGetFrame(outLine.Data, out var frame))
        {
            _processedFrames = frame;
            ProgressValue = _processedFrames * 100.0 / JobItems[_jobItemIndex].TotalFrames;
            return;
        }

        // Keep the twice-a-second "-progress" key/value spam out of the user-facing log.
        if (FfmpegProgressTracker.IsProgressLine(outLine.Data))
        {
            return;
        }

        _log?.AppendLine(outLine.Data);
    }

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems(string outputVideoFileName)
    {
        var subtitle = new Subtitle(_subtitle);

        // Tracked so the file is swept when the window closes - and not GetTempFileName() plus an
        // extension, which leaked the empty tmpXXXX.tmp it creates on top of the file written
        // (#13332).
        var subtitleFileName = _subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat }
            ? _tempSubtitleFiles.Write(subtitle, new AdvancedSubStationAlpha())
            : _tempSubtitleFiles.Write(subtitle, new SubRip());

        var jobItem = new BurnInJobItem(string.Empty, VideoWidth, VideoHeight)
        {
            InputVideoFileName = VideoFileName,
            OutputVideoFileName = outputVideoFileName,
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
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
            var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
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
    private async Task PromptFfmpegParametersAndGeenrate()
    {
        PromptForFfmpegParameters = true;
        await Generate();
        PromptForFfmpegParameters = false;
    }

    /// <summary>
    /// A distinct output name beside the source: "<name>_reencoded<ext>", with a collision
    /// counter, mirroring the cut-video dialog. Never returns the input path.
    /// </summary>
    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var folder = Path.GetDirectoryName(videoFileName) ?? Path.GetTempPath();
        var fileName = Path.Combine(folder, nameNoExt + "_reencoded" + SelectedVideoExtension);

        var i = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(folder, nameNoExt + "_reencoded_" + i.ToString(CultureInfo.InvariantCulture) + SelectedVideoExtension);
            i++;
        }

        return fileName;
    }

    [RelayCommand]
    private async Task Generate()
    {
        // Suggest a DISTINCT name: ChangeExtension on an .mkv input with the default .mkv output
        // handed the save dialog the input path itself, so accepting the suggestion pointed
        // ffmpeg at the file it was reading and destroyed the user's video. The cut-video and
        // embedded-subtitles dialogs both build a suffixed name with a collision counter.
        var outputVideoFileName = MakeOutputFileName(VideoFileName);
        outputVideoFileName = await _fileHelper.PickSaveFile(Window!, SelectedVideoExtension, outputVideoFileName, Se.Language.General.SaveVideoAsVideoTitle);
        if (string.IsNullOrEmpty(outputVideoFileName))
        {
            return;
        }

        // ...and refuse it outright if the user still picks the input, as the write-chapters
        // dialog does: ffmpeg reads the input while writing the output.
        if (string.Equals(Path.GetFullPath(outputVideoFileName), Path.GetFullPath(VideoFileName), StringComparison.OrdinalIgnoreCase))
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                Se.Language.General.OutputFileCannotBeTheInputFile,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        JobItems = GetCurrentVideoAsJobItems(outputVideoFileName);
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

        var result = await InitAndStartJobItem(0);
        if (!result)
        {
            IsGenerating = false;
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
            _timerGenerate.Stop();
            _doAbort = true;
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                _ffmpegProcess.Kill(true);
            }

            IsGenerating = false;
            return;
        }

        Window?.Close();
    }

    internal void OnClosing()
    {
        // The subtitle files handed to ffmpeg live as long as the window does - nothing else
        // removes them, and they used to pile up in the temp folder run after run (#13332).
        _tempSubtitleFiles.Delete();

        // Stop the poll timer and any still-running encode - closing the window used to leave the
        // ffmpeg process encoding to completion in the background, with the timer still firing
        // into a closed window. Same fix as the blank-video and embedded-subtitles dialogs.
        _timerGenerate.StopAndDispose(TimerGenerateElapsed);
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            try
            {
#pragma warning disable CA1416
                _ffmpegProcess.Kill(true);
#pragma warning restore CA1416
            }
            catch
            {
                // ignore - it may have exited in between
            }
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            // Route through Cancel so Escape during generation aborts the encode
            // instead of closing the window over a running ffmpeg.
            Cancel();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/re-encode-video");
        }
    }
}