using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Video.VideoSpeed;

public partial class VideoSpeedViewModel : ObservableObject
{
    [ObservableProperty] private string _inputVideoFileName = string.Empty;
    [ObservableProperty] private string _outputVideoFileName = string.Empty;
    [ObservableProperty] private TimeSpan _videoDuration = TimeSpan.Zero;
    [ObservableProperty] private ObservableCollection<VideoSpeedSegmentItem> _segments = new();
    [ObservableProperty] private VideoSpeedSegmentItem? _selectedSegment;
    [ObservableProperty] private bool _burnInSubtitle;
    [ObservableProperty] private string _subtitleFileName = string.Empty;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private string _progressTimeInfo = string.Empty;

    // Video Metadata Display
    [ObservableProperty] private string _videoInfoText = string.Empty;
    [ObservableProperty] private string _totalTimeInfo = string.Empty;

    // Segment Editing Fields
    [ObservableProperty] private TimeSpan _newStartTime = TimeSpan.Zero;
    [ObservableProperty] private TimeSpan _newEndTime = TimeSpan.Zero;
    [ObservableProperty] private bool _isTargetDurationMode = true;
    [ObservableProperty] private double _newTargetDurationSeconds = 5.0;
    [ObservableProperty] private double _newSpeedMultiplier = 1.0;

    // Video & Audio Encoding Fields (GPU Acceleration)
    [ObservableProperty] private ObservableCollection<VideoEncodingItem> _videoEncodings = new();
    [ObservableProperty] private VideoEncodingItem? _selectedVideoEncoding;
    [ObservableProperty] private ObservableCollection<string> _videoPresets = new();
    [ObservableProperty] private string? _selectedVideoPreset;
    [ObservableProperty] private ObservableCollection<string> _videoCrfs = new();
    [ObservableProperty] private string? _selectedVideoCrf;
    [ObservableProperty] private ObservableCollection<string> _audioEncodings = new();
    [ObservableProperty] private string _selectedAudioEncoding = "copy";
    [ObservableProperty] private ObservableCollection<string> _audioSampleRates = new();
    [ObservableProperty] private string _selectedAudioSampleRate = "Original";
    [ObservableProperty] private ObservableCollection<string> _audioBitRates = new();
    [ObservableProperty] private string _selectedAudioBitRate = "Original";
    [ObservableProperty] private bool _audioIsStereo = true;

    // Target File Size Fields (2-Pass Encoding)
    [ObservableProperty] private bool _useTargetFileSize;
    [ObservableProperty] private bool _matchSourceVideoSize = true;
    [ObservableProperty] private int? _targetFileSize = 8;
    [ObservableProperty] private string _targetVideoBitRateInfo = string.Empty;

    // Resolution & FPS Fields
    [ObservableProperty] private bool _useSourceResolution = true;
    [ObservableProperty] private int _videoWidth = 1920;
    [ObservableProperty] private int _videoHeight = 1080;
    [ObservableProperty] private int _sourceVideoWidth;
    [ObservableProperty] private int _sourceVideoHeight;
    [ObservableProperty] private ObservableCollection<double> _frameRates = new();
    [ObservableProperty] private double _selectedFrameRate = 29.97;
    [ObservableProperty] private double _sourceFrameRate;

    public Window? Window { get; set; }

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;
    private Process? _ffmpegProcess;
    private bool _doAbort;
    private Subtitle _subtitle = new();
    private SubtitleFormat? _subtitleFormat;
    private readonly Stopwatch _stopwatch = new();
    private DispatcherTimer? _uiTimer;
    private int _currentPercent;

    public VideoSpeedViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        InitializeEncodingDefaults();
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat? subtitleFormat)
    {
        InputVideoFileName = videoFileName;
        _subtitle = subtitle;
        _subtitleFormat = subtitleFormat;

        if (!string.IsNullOrWhiteSpace(videoFileName) && File.Exists(videoFileName))
        {
            var ext = Path.GetExtension(videoFileName);
            var dir = Path.GetDirectoryName(videoFileName) ?? string.Empty;
            var nameWithoutExt = Path.GetFileNameWithoutExtension(videoFileName);
            OutputVideoFileName = Path.Combine(dir, $"{nameWithoutExt}_speed{ext}");

            try
            {
                var fileInfo = new FileInfo(videoFileName);
                var sizeText = Utilities.FormatBytesToDisplayFileSize(fileInfo.Length);
                var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
                if (mediaInfo.Duration != null)
                {
                    VideoDuration = mediaInfo.Duration.TimeSpan;
                    NewStartTime = TimeSpan.Zero;
                    NewEndTime = VideoDuration;
                }

                if (mediaInfo.Dimension.Width > 0 && mediaInfo.Dimension.Height > 0)
                {
                    SourceVideoWidth = mediaInfo.Dimension.Width;
                    SourceVideoHeight = mediaInfo.Dimension.Height;
                    VideoWidth = SourceVideoWidth;
                    VideoHeight = SourceVideoHeight;
                }

                if (mediaInfo.FramesRate > 0)
                {
                    SourceFrameRate = (double)mediaInfo.FramesRate;
                    var roundedFps = FrameRateHelper.RoundToNearestCinematicFrameRate(SourceFrameRate);
                    if (!FrameRates.Contains(roundedFps))
                    {
                        FrameRates.Insert(0, roundedFps);
                    }
                    SelectedFrameRate = roundedFps;
                }

                var durationText = VideoDuration.ToString(@"hh\:mm\:ss\.fff");
                var resolutionText = (mediaInfo.Dimension.Width > 0 && mediaInfo.Dimension.Height > 0) ? $"{mediaInfo.Dimension.Width}x{mediaInfo.Dimension.Height}" : string.Empty;
                var fpsText = SourceFrameRate > 0 ? $"{SourceFrameRate:0.###} fps" : string.Empty;

                var parts = new List<string>();
                if (!string.IsNullOrEmpty(sizeText)) parts.Add($"Size: {sizeText}");
                if (VideoDuration > TimeSpan.Zero) parts.Add($"Time: {durationText}");
                if (!string.IsNullOrEmpty(resolutionText)) parts.Add($"Resolution: {resolutionText}");
                if (!string.IsNullOrEmpty(fpsText)) parts.Add($"FPS: {fpsText}");

                VideoInfoText = string.Join("   |   ", parts);
            }
            catch
            {
                // Fallback if media info parsing fails
            }

            if (TargetFileSize == null || TargetFileSize <= 0)
            {
                var sourceMb = (int)GetSourceFileSizeInMb(videoFileName);
                TargetFileSize = sourceMb > 0 ? sourceMb : 8;
            }

            UpdateTotalTimeInfo();
            CalculateTargetFileBitRate();
        }
    }

    [RelayCommand]
    private async Task BrowseResolution()
    {
        var result = await _windowService.ShowDialogAsync<BurnInResolutionPickerWindow, BurnInResolutionPickerViewModel>(Window!, vm =>
        {
            vm.RemovePickResolution();
            if (SourceVideoWidth > 0 && SourceVideoHeight > 0)
            {
                vm.SetSourceResolution(SourceVideoWidth, SourceVideoHeight);
            }
        });
        if (!result.OkPressed || result.SelectedResolution == null)
        {
            return;
        }

        if (result.SelectedResolution.ItemType == ResolutionItemType.UseSource)
        {
            if (SourceVideoWidth > 0 && SourceVideoHeight > 0)
            {
                VideoWidth = SourceVideoWidth;
                VideoHeight = SourceVideoHeight;
            }
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.Resolution)
        {
            VideoWidth = result.SelectedResolution.Width;
            VideoHeight = result.SelectedResolution.Height;
        }
    }

    [RelayCommand]
    private async Task BrowseInputVideo()
    {
        var file = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
        if (!string.IsNullOrEmpty(file))
        {
            Initialize(file, _subtitle, _subtitleFormat);
        }
    }

    [RelayCommand]
    private async Task BrowseOutputVideo()
    {
        var ext = Path.GetExtension(InputVideoFileName);
        if (string.IsNullOrEmpty(ext)) ext = ".mp4";
        var file = await _fileHelper.PickSaveFile(Window!, ext, OutputVideoFileName, Se.Language.General.SaveVideoAsVideoTitle);
        if (!string.IsNullOrEmpty(file))
        {
            OutputVideoFileName = file;
        }
    }

    private void InitializeEncodingDefaults()
    {
        VideoEncodings = new ObservableCollection<VideoEncodingItem>(VideoEncodingItem.VideoEncodings);
        SelectedVideoEncoding = VideoEncodings.FirstOrDefault() ?? new VideoEncodingItem("libx264", "H.264/AVC (CPU)");

        VideoCrfs = new ObservableCollection<string>();
        for (int i = 0; i <= 51; i++)
        {
            VideoCrfs.Add(i.ToString());
        }
        SelectedVideoCrf = "22";

        AudioEncodings = new ObservableCollection<string> { "copy", "aac", "mp3", "ac3", "opus", "flac" };
        SelectedAudioEncoding = "copy";

        AudioSampleRates = new ObservableCollection<string> { "Original", "44100 Hz", "48000 Hz", "32000 Hz", "22050 Hz" };
        SelectedAudioSampleRate = "Original";

        AudioBitRates = new ObservableCollection<string> { "Original", "64k", "96k", "128k", "160k", "192k", "256k", "320k" };
        SelectedAudioBitRate = "Original";

        AudioIsStereo = true;

        FrameRates = new ObservableCollection<double>(FrameRateHelper.StandardRates);
        SelectedFrameRate = 29.97;

        UpdatePresetOptions(SelectedVideoEncoding?.Codec ?? "libx264");
    }

    partial void OnUseTargetFileSizeChanged(bool value) => CalculateTargetFileBitRate();
    partial void OnMatchSourceVideoSizeChanged(bool value) => CalculateTargetFileBitRate();
    partial void OnTargetFileSizeChanged(int? value) => CalculateTargetFileBitRate();
    partial void OnSelectedAudioEncodingChanged(string value) => CalculateTargetFileBitRate();
    partial void OnSelectedAudioBitRateChanged(string value) => CalculateTargetFileBitRate();

    partial void OnSelectedSegmentChanged(VideoSpeedSegmentItem? value)
    {
        if (value != null)
        {
            NewStartTime = value.StartTime;
            NewEndTime = value.EndTime;
            IsTargetDurationMode = value.Mode == SpeedMode.TargetDuration;
            NewTargetDurationSeconds = value.TargetDurationSeconds;
            NewSpeedMultiplier = Math.Max(0.1, value.SpeedMultiplier);
        }
    }

    partial void OnSelectedVideoEncodingChanged(VideoEncodingItem? value)
    {
        if (value != null)
        {
            UpdatePresetOptions(value.Codec);
        }
    }

    private void UpdatePresetOptions(string codec)
    {
        VideoPresets.Clear();
        if (VideoPresetOptions.IsNvenc(codec))
        {
            foreach (var preset in VideoPresetOptions.GetNvencPresets())
            {
                VideoPresets.Add(preset);
            }
        }
        else if (VideoPresetOptions.IsAmf(codec))
        {
            foreach (var q in VideoPresetOptions.GetAmfQualities())
            {
                if (!string.IsNullOrWhiteSpace(q))
                    VideoPresets.Add(q);
            }
        }
        else
        {
            var cpuPresets = new[] { "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow" };
            foreach (var p in cpuPresets)
            {
                VideoPresets.Add(p);
            }
        }

        SelectedVideoPreset = VideoPresets.FirstOrDefault(p => p == "fast" || p == "medium" || p == "balanced") ?? VideoPresets.FirstOrDefault();
    }

    [RelayCommand]
    private void AddSegment()
    {
        if (NewEndTime <= NewStartTime)
        {
            return;
        }

        var item = new VideoSpeedSegmentItem
        {
            StartTime = NewStartTime,
            EndTime = NewEndTime,
            Mode = IsTargetDurationMode ? SpeedMode.TargetDuration : SpeedMode.Multiplier,
            TargetDurationSeconds = NewTargetDurationSeconds,
            SpeedMultiplier = Math.Max(0.1, NewSpeedMultiplier),
        };

        Segments.Add(item);
        SelectedSegment = item;
        UpdateTotalTimeInfo();
    }

    [RelayCommand]
    private void UpdateSegment()
    {
        if (SelectedSegment == null)
        {
            return;
        }

        if (NewEndTime <= NewStartTime)
        {
            return;
        }

        SelectedSegment.StartTime = NewStartTime;
        SelectedSegment.EndTime = NewEndTime;
        SelectedSegment.Mode = IsTargetDurationMode ? SpeedMode.TargetDuration : SpeedMode.Multiplier;
        SelectedSegment.TargetDurationSeconds = NewTargetDurationSeconds;
        SelectedSegment.SpeedMultiplier = Math.Max(0.1, NewSpeedMultiplier);

        // Refresh collection UI binding by resetting or triggering property updates
        var index = Segments.IndexOf(SelectedSegment);
        if (index >= 0)
        {
            Segments[index] = SelectedSegment;
        }
        UpdateTotalTimeInfo();
    }

    [RelayCommand]
    private void RemoveSegment()
    {
        if (SelectedSegment != null)
        {
            var index = Segments.IndexOf(SelectedSegment);
            Segments.Remove(SelectedSegment);
            if (Segments.Count > 0)
            {
                SelectedSegment = Segments[Math.Min(index, Segments.Count - 1)];
            }
            else
            {
                SelectedSegment = null;
            }
            UpdateTotalTimeInfo();
        }
    }

    [RelayCommand]
    private void MoveUpSegment()
    {
        if (SelectedSegment == null) return;
        var index = Segments.IndexOf(SelectedSegment);
        if (index > 0)
        {
            var item = SelectedSegment;
            Segments.RemoveAt(index);
            Segments.Insert(index - 1, item);
            SelectedSegment = item;
            UpdateTotalTimeInfo();
        }
    }

    [RelayCommand]
    private void MoveDownSegment()
    {
        if (SelectedSegment == null) return;
        var index = Segments.IndexOf(SelectedSegment);
        if (index >= 0 && index < Segments.Count - 1)
        {
            var item = SelectedSegment;
            Segments.RemoveAt(index);
            Segments.Insert(index + 1, item);
            SelectedSegment = item;
            UpdateTotalTimeInfo();
        }
    }

    [RelayCommand]
    private void AddFullVideoTargetPreset(double targetSeconds)
    {
        Segments.Clear();
        var item = new VideoSpeedSegmentItem
        {
            StartTime = TimeSpan.Zero,
            EndTime = VideoDuration > TimeSpan.Zero ? VideoDuration : TimeSpan.FromMinutes(10),
            Mode = SpeedMode.TargetDuration,
            TargetDurationSeconds = targetSeconds,
        };
        Segments.Add(item);
        SelectedSegment = item;
        UpdateTotalTimeInfo();
    }

    [RelayCommand]
    private void AddFullVideoSpeedPreset(double multiplier)
    {
        Segments.Clear();
        var item = new VideoSpeedSegmentItem
        {
            StartTime = TimeSpan.Zero,
            EndTime = VideoDuration > TimeSpan.Zero ? VideoDuration : TimeSpan.FromMinutes(10),
            Mode = SpeedMode.Multiplier,
            SpeedMultiplier = Math.Max(0.1, multiplier),
        };
        Segments.Add(item);
        SelectedSegment = item;
        UpdateTotalTimeInfo();
    }

    public void UpdateTotalTimeInfo()
    {
        if (VideoDuration <= TimeSpan.Zero)
        {
            TotalTimeInfo = string.Empty;
            CalculateTargetFileBitRate();
            return;
        }

        var origStr = VideoDuration.ToString(@"hh\:mm\:ss\.fff");
        var estimatedOut = CalculateEstimatedOutputDuration();
        var estStr = estimatedOut.ToString(@"hh\:mm\:ss\.fff");

        if (estimatedOut > TimeSpan.Zero && VideoDuration > TimeSpan.Zero)
        {
            var overallSpeed = VideoDuration.TotalSeconds / estimatedOut.TotalSeconds;
            TotalTimeInfo = $"Total Original: {origStr}  ➔  Estimated Output: {estStr}  (Overall Speed: {overallSpeed:0.00}x)";
        }
        else
        {
            TotalTimeInfo = $"Total Original: {origStr}  ➔  Estimated Output: {estStr}";
        }

        CalculateTargetFileBitRate();
    }

    public static long GetSourceFileSizeInMb(string videoFileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(videoFileName) || !File.Exists(videoFileName))
            {
                return 0;
            }
            var bytes = new FileInfo(videoFileName).Length;
            return Math.Max(1, (long)Math.Round(bytes / (1024.0 * 1024.0)));
        }
        catch
        {
            return 0;
        }
    }

    public int GetTargetVideoBitRate()
    {
        if (!UseTargetFileSize)
        {
            return 0;
        }

        var targetMb = MatchSourceVideoSize ? GetSourceFileSizeInMb(InputVideoFileName) : (TargetFileSize ?? 0);
        if (targetMb < 1)
        {
            return 0;
        }

        var outSec = CalculateEstimatedOutputDuration().TotalSeconds;
        if (outSec <= 0)
        {
            outSec = VideoDuration.TotalSeconds;
        }
        if (outSec <= 0)
        {
            return 0;
        }

        var audioBitRate = 0;
        if (SelectedAudioEncoding != "copy" && !string.IsNullOrWhiteSpace(SelectedAudioBitRate) && SelectedAudioBitRate != "Original")
        {
            _ = int.TryParse(SelectedAudioBitRate.Replace("k", "").Trim(), out audioBitRate);
        }
        else
        {
            audioBitRate = 128;
        }

        var totalBitRate = (int)Math.Round(targetMb * 8192.0 / outSec);
        var videoBitRate = totalBitRate - audioBitRate;
        return Math.Max(10, videoBitRate);
    }

    public void CalculateTargetFileBitRate()
    {
        TargetVideoBitRateInfo = string.Empty;
        if (!UseTargetFileSize)
        {
            return;
        }

        var targetMb = MatchSourceVideoSize ? GetSourceFileSizeInMb(InputVideoFileName) : (TargetFileSize ?? 0);
        if (targetMb < 1)
        {
            return;
        }

        var outSec = CalculateEstimatedOutputDuration().TotalSeconds;
        if (outSec <= 0)
        {
            outSec = VideoDuration.TotalSeconds;
        }
        if (outSec <= 0)
        {
            return;
        }

        var separateAudio = SelectedAudioEncoding != "copy" && !string.IsNullOrWhiteSpace(SelectedAudioBitRate) && SelectedAudioBitRate != "Original";
        var audioBitRate = 0;
        if (separateAudio)
        {
            _ = int.TryParse(SelectedAudioBitRate.Replace("k", "").Trim(), out audioBitRate);
        }
        else
        {
            audioBitRate = 128;
        }

        var totalBitRate = (int)Math.Round(targetMb * 8192.0 / outSec);
        var videoBitRate = totalBitRate - audioBitRate;
        if (videoBitRate < 10)
        {
            videoBitRate = 10;
        }

        TargetVideoBitRateInfo = string.Format(Se.Language.Video.BurnIn.TotalBitRateX, $"{(videoBitRate + audioBitRate):#,###,##0}k");
        if (separateAudio)
        {
            TargetVideoBitRateInfo += $" ({videoBitRate:#,###,##0}k + {audioBitRate:#,###,##0}k)";
        }
    }

    public TimeSpan CalculateEstimatedOutputDuration()
    {
        if (VideoDuration <= TimeSpan.Zero) return TimeSpan.Zero;
        if (Segments.Count == 0) return VideoDuration;

        var intervals = BuildTimelineIntervals();
        var outMs = MapOriginalTimeToNew(VideoDuration.TotalMilliseconds, intervals);
        return TimeSpan.FromMilliseconds(Math.Max(0, outMs));
    }

    private static bool IsVideoToolboxEncoder(string? codec)
    {
        return !string.IsNullOrEmpty(codec) && codec.EndsWith("_videotoolbox", StringComparison.Ordinal);
    }

    private static void DeletePassLogFiles(string? passLogFilePrefix)
    {
        if (string.IsNullOrEmpty(passLogFilePrefix)) return;
        try
        {
            var dir = Path.GetDirectoryName(passLogFilePrefix);
            var prefixName = Path.GetFileName(passLogFilePrefix);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                foreach (var file in Directory.GetFiles(dir, prefixName + "*"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (string.IsNullOrWhiteSpace(InputVideoFileName) || !File.Exists(InputVideoFileName))
        {
            await MessageBox.Show(Window!, "Speed Video Error", "Input video file does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputVideoFileName))
        {
            await MessageBox.Show(Window!, "Speed Video Error", "Output video file path is empty.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (Segments.Count == 0)
        {
            // Default: apply speed to full video
            AddFullVideoSpeedPreset(IsTargetDurationMode ? (VideoDuration.TotalSeconds / Math.Max(0.1, NewTargetDurationSeconds)) : NewSpeedMultiplier);
        }

        IsGenerating = true;
        _doAbort = false;
        ProgressValue = 0;
        _currentPercent = 0;
        _stopwatch.Restart();
        ProgressText = "Starting FFmpeg process...";
        ProgressTimeInfo = "Elapsed: 00:00:00 | Remaining: --:--:--";

        StartUiTimer();

        string? tempAssFile = null;
        try
        {
            if (BurnInSubtitle && _subtitle.Paragraphs.Count > 0)
            {
                var remappedSub = CreateSpeedAdjustedSubtitle(_subtitle);
                tempAssFile = Path.Combine(Path.GetTempPath(), $"se_speed_burnin_{Guid.NewGuid():N}.ass");
                var assFormat = new AdvancedSubStationAlpha();
                File.WriteAllText(tempAssFile, assFormat.ToText(remappedSub, string.Empty), Encoding.UTF8);
            }

            if (UseTargetFileSize)
            {
                var targetBitRate = GetTargetVideoBitRate();
                if (targetBitRate < 10)
                {
                    await MessageBox.Show(Window!, "Speed Video Error", $"Calculated bit rate too low: {targetBitRate}k", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var codec = SelectedVideoEncoding?.Codec ?? "libx264";
                var useTwoPass = !IsVideoToolboxEncoder(codec);

                if (useTwoPass)
                {
                    var passLogPrefix = Path.Combine(Path.GetTempPath(), $"se_speed_2pass_{Guid.NewGuid():N}");
                    try
                    {
                        // === PASS 1: Analysis ===
                        ProgressText = "Analyzing video (Pass 1 of 2)...";
                        _currentPercent = 0;
                        ProgressValue = 0;
                        var pass1Args = BuildFfmpegArguments(InputVideoFileName, "NUL", tempAssFile, pass: "1", bitRateK: targetBitRate, passLogPrefix: passLogPrefix);
                        var pass1FullArgs = FfmpegProgressTracker.ProgressArguments + " " + pass1Args;

                        var pass1Success = await RunFfmpegProcessAsync(pass1FullArgs, "Pass 1");
                        if (!pass1Success || _doAbort)
                        {
                            if (!_doAbort)
                            {
                                await MessageBox.Show(Window!, "Process Error", "FFmpeg Pass 1 analysis failed.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            return;
                        }

                        // === PASS 2: Encoding ===
                        ProgressText = "Encoding video (Pass 2 of 2)...";
                        _currentPercent = 0;
                        ProgressValue = 0;
                        var pass2Args = BuildFfmpegArguments(InputVideoFileName, OutputVideoFileName, tempAssFile, pass: "2", bitRateK: targetBitRate, passLogPrefix: passLogPrefix);
                        var pass2FullArgs = FfmpegProgressTracker.ProgressArguments + " " + pass2Args;

                        var pass2Success = await RunFfmpegProcessAsync(pass2FullArgs, "Pass 2");
                        if (!pass2Success || _doAbort)
                        {
                            if (!_doAbort)
                            {
                                await MessageBox.Show(Window!, "Process Error", "FFmpeg Pass 2 encoding failed.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            return;
                        }
                    }
                    finally
                    {
                        DeletePassLogFiles(passLogPrefix);
                    }
                }
                else
                {
                    // Single pass with target bitrate
                    ProgressText = "Encoding video with target bit rate...";
                    var singleArgs = BuildFfmpegArguments(InputVideoFileName, OutputVideoFileName, tempAssFile, pass: null, bitRateK: targetBitRate);
                    var singleFullArgs = FfmpegProgressTracker.ProgressArguments + " " + singleArgs;

                    var success = await RunFfmpegProcessAsync(singleFullArgs, "Encode");
                    if (!success || _doAbort)
                    {
                        if (!_doAbort)
                        {
                            await MessageBox.Show(Window!, "Process Error", "FFmpeg encoding failed.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        return;
                    }
                }
            }
            else
            {
                // Normal 1 pass CRF
                var ffmpegArgs = BuildFfmpegArguments(InputVideoFileName, OutputVideoFileName, tempAssFile);
                var fullArgs = FfmpegProgressTracker.ProgressArguments + " " + ffmpegArgs;
                var success = await RunFfmpegProcessAsync(fullArgs, "Encode");
                if (!success || _doAbort)
                {
                    if (!_doAbort)
                    {
                        await MessageBox.Show(Window!, "Process Error", "FFmpeg encoding failed.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
            }

            _stopwatch.Stop();
            StopUiTimer();

            ProgressValue = 100;
            ProgressText = "Completed successfully!";
            ProgressTimeInfo = $"Total Elapsed: {FormatDuration(_stopwatch.Elapsed)}";
            await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window!, vm =>
            {
                vm.Initialize("Video saved", "Video saved successfully.", OutputVideoFileName, true, true);
            });
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            StopUiTimer();
            ProgressText = $"Error: {ex.Message}";
            await MessageBox.Show(Window!, "Error", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsGenerating = false;
            _ffmpegProcess?.Dispose();
            _ffmpegProcess = null;

            if (tempAssFile != null && File.Exists(tempAssFile))
            {
                try { File.Delete(tempAssFile); } catch { }
            }
        }
    }

    private async Task<bool> RunFfmpegProcessAsync(string fullArgs, string stageName)
    {
        var tracker = VideoDuration.TotalSeconds > 0 ? new FfmpegProgressTracker(VideoDuration.TotalSeconds) : null;
        var ffmpegErrorLog = new List<string>();

        _ffmpegProcess = FfmpegGenerator.GetProcess(fullArgs, (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            lock (ffmpegErrorLog)
            {
                if (ffmpegErrorLog.Count >= 30)
                {
                    ffmpegErrorLog.RemoveAt(0);
                }
                ffmpegErrorLog.Add(e.Data);
            }

            if (tracker != null && tracker.TryGetNewPercent(e.Data, out var percent))
            {
                _currentPercent = percent;
                Dispatcher.UIThread.Post(() =>
                {
                    ProgressValue = Math.Clamp(percent, 0, 100);
                    UpdateProgressTimeDisplays();
                });
            }
        }, Path.GetDirectoryName(OutputVideoFileName) ?? string.Empty);

        await Task.Run(() =>
        {
            _ffmpegProcess.Start();
            _ffmpegProcess.BeginOutputReadLine(); // CRITICAL: Start reading stdout pipe
            _ffmpegProcess.BeginErrorReadLine();  // CRITICAL: Start reading stderr pipe
            _ffmpegProcess.WaitForExit();
        });

        var exitCode = _ffmpegProcess.ExitCode;
        _ffmpegProcess.Dispose();
        _ffmpegProcess = null;

        if (_doAbort || exitCode != 0)
        {
            if (!_doAbort)
            {
                var errorDetails = string.Join(Environment.NewLine, ffmpegErrorLog.TakeLast(10));
                Se.WriteToolsLog($"VideoSpeed {stageName} failed (exit {exitCode}): {errorDetails}");
            }
            return false;
        }

        return true;
    }

    private void StartUiTimer()
    {
        StopUiTimer();
        _uiTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _uiTimer.Tick += (_, _) => UpdateProgressTimeDisplays();
        _uiTimer.Start();
    }

    private void StopUiTimer()
    {
        if (_uiTimer != null)
        {
            _uiTimer.Stop();
            _uiTimer = null;
        }
    }

    private void UpdateProgressTimeDisplays()
    {
        var elapsed = _stopwatch.Elapsed;
        var elapsedStr = FormatDuration(elapsed);

        if (_currentPercent > 0 && _currentPercent < 100)
        {
            var remainingSecs = elapsed.TotalSeconds * (100.0 - _currentPercent) / _currentPercent;
            var remaining = TimeSpan.FromSeconds(remainingSecs);
            var remainingStr = FormatDuration(remaining);

            ProgressText = $"Processing: {_currentPercent}%";
            ProgressTimeInfo = $"Elapsed: {elapsedStr}  |  Remaining: ~{remainingStr}";
        }
        else if (_currentPercent >= 100)
        {
            ProgressText = "Finalizing output file...";
            ProgressTimeInfo = $"Elapsed: {elapsedStr}  |  Remaining: 00:00:00";
        }
        else
        {
            ProgressText = "Processing FFmpeg stream...";
            ProgressTimeInfo = $"Elapsed: {elapsedStr}  |  Remaining: Estimating...";
        }
    }

    private static string FormatDuration(TimeSpan ts)
    {
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsGenerating)
        {
            _doAbort = true;
            try
            {
                _ffmpegProcess?.Kill();
            }
            catch { }
        }
        else
        {
            Window?.Close();
        }
    }

    private (string VideoFlags, string AudioFlags) BuildEncoderFlags(bool hasAudioFilter, string? pass = null, int? bitRateK = null, string? passLogPrefix = null)
    {
        var codec = SelectedVideoEncoding?.Codec ?? "libx264";
        var preset = SelectedVideoPreset ?? "fast";
        var crf = SelectedVideoCrf ?? "22";

        var vSB = new StringBuilder();
        vSB.Append($"-c:v {codec}");

        if (codec == "libx265" || codec.StartsWith("hevc_", StringComparison.Ordinal))
        {
            vSB.Append(" -tag:v hvc1");
        }

        if (VideoPresetOptions.IsNvenc(codec))
        {
            if (!string.IsNullOrWhiteSpace(preset)) vSB.Append($" -preset {preset}");
            if (bitRateK != null && bitRateK > 0)
            {
                vSB.Append($" -b:v {bitRateK}k");
            }
            else if (!string.IsNullOrWhiteSpace(crf))
            {
                vSB.Append($" -cq {crf}");
            }
        }
        else if (VideoPresetOptions.IsAmf(codec))
        {
            if (!string.IsNullOrWhiteSpace(preset)) vSB.Append($" -quality {preset}");
            if (bitRateK != null && bitRateK > 0)
            {
                vSB.Append($" -b:v {bitRateK}k");
            }
            else if (!string.IsNullOrWhiteSpace(crf) && int.TryParse(crf, out var crfVal))
            {
                vSB.Append($" -rc cqp -qp_p {crfVal} -qp_i {crfVal}");
            }
        }
        else if (codec is "h264_qsv" or "hevc_qsv")
        {
            if (!string.IsNullOrWhiteSpace(preset)) vSB.Append($" -preset {preset}");
            if (bitRateK != null && bitRateK > 0)
            {
                vSB.Append($" -b:v {bitRateK}k");
            }
            else if (!string.IsNullOrWhiteSpace(crf))
            {
                vSB.Append($" -global_quality {crf}");
            }
        }
        else if (codec is "h264_videotoolbox" or "hevc_videotoolbox")
        {
            if (bitRateK != null && bitRateK > 0)
            {
                vSB.Append($" -b:v {bitRateK}k");
            }
            else if (!string.IsNullOrWhiteSpace(crf))
            {
                vSB.Append($" -q:v {crf}");
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(preset)) vSB.Append($" -preset {preset}");
            if (bitRateK != null && bitRateK > 0)
            {
                vSB.Append($" -b:v {bitRateK}k");
            }
            else if (!string.IsNullOrWhiteSpace(crf))
            {
                vSB.Append($" -crf {crf}");
            }
        }

        if (SelectedFrameRate > 0)
        {
            vSB.Append($" -r {SelectedFrameRate.ToString(CultureInfo.InvariantCulture)}");
        }

        if (!string.IsNullOrEmpty(pass))
        {
            vSB.Append($" -pass {pass}");
            if (!string.IsNullOrEmpty(passLogPrefix))
            {
                var escapedPrefix = passLogPrefix.Replace("\\", "/");
                vSB.Append($" -passlogfile \"{escapedPrefix}\"");
            }
        }

        var aSB = new StringBuilder();
        var audioCodec = SelectedAudioEncoding ?? "copy";

        // CRITICAL: FFmpeg cannot stream-copy audio if it's fed from audio filter / filtergraph
        if (hasAudioFilter && audioCodec == "copy")
        {
            audioCodec = "aac";
        }

        aSB.Append($"-c:a {audioCodec}");

        if (audioCodec != "copy")
        {
            if (!string.IsNullOrWhiteSpace(SelectedAudioSampleRate) && SelectedAudioSampleRate != "Original")
            {
                var rateStr = SelectedAudioSampleRate.Replace(" Hz", "").Trim();
                aSB.Append($" -ar {rateStr}");
            }
            if (AudioIsStereo)
            {
                aSB.Append(" -ac 2");
            }
            if (!string.IsNullOrWhiteSpace(SelectedAudioBitRate) && SelectedAudioBitRate != "Original")
            {
                aSB.Append($" -b:a {SelectedAudioBitRate}");
            }
        }

        return (vSB.ToString(), aSB.ToString());
    }

    public string BuildFfmpegArguments(
        string inputVideo,
        string outputVideo,
        string? assSubtitleFile,
        string? pass = null,
        int? bitRateK = null,
        string? passLogPrefix = null)
    {
        // Order segments chronologically
        var sortedSegments = Segments.OrderBy(s => s.StartTime).ToList();
        var isPass1 = pass == "1";

        // Check if resolution scaling is needed
        var needScale = VideoWidth > 0 && VideoHeight > 0 &&
            (SourceVideoWidth <= 0 || SourceVideoHeight <= 0 || VideoWidth != SourceVideoWidth || VideoHeight != SourceVideoHeight);
        var scaledW = VideoWidth % 2 == 1 ? VideoWidth + 1 : VideoWidth;
        var scaledH = VideoHeight % 2 == 1 ? VideoHeight + 1 : VideoHeight;

        // If only 1 segment covering 0 to end (or simple full video)
        if (sortedSegments.Count == 1 && sortedSegments[0].StartTime <= TimeSpan.FromMilliseconds(100) &&
            (VideoDuration == TimeSpan.Zero || sortedSegments[0].EndTime >= VideoDuration - TimeSpan.FromMilliseconds(100)))
        {
            var seg0 = sortedSegments[0];
            var ratio = seg0.EffectiveSpeedRatio;
            var setptsFactor = (1.0 / ratio).ToString("0.000000", CultureInfo.InvariantCulture);

            var vfList = new List<string>();
            if (needScale)
            {
                vfList.Add($"scale={scaledW}:{scaledH}");
            }
            if (!string.IsNullOrEmpty(assSubtitleFile))
            {
                var escapedPath = assSubtitleFile.Replace("\\", "/").Replace(":", "\\:");
                vfList.Add($"subtitles='{escapedPath}'");
            }
            if (seg0.IsReverse)
            {
                vfList.Add("reverse");
            }
            vfList.Add($"setpts={setptsFactor}*PTS");

            var vf = string.Join(",", vfList);
            var (vFlags, aFlags) = BuildEncoderFlags(!isPass1, pass, bitRateK, passLogPrefix);

            if (isPass1)
            {
                var nullDevice = Configuration.IsRunningOnWindows ? "NUL" : "/dev/null";
                return $"-nostdin -y -i \"{inputVideo}\" -vf \"{vf}\" {vFlags} -an -f null {nullDevice}";
            }

            var af = BuildAudioFilter(ratio, seg0.AudioMode, seg0.IsReverse);
            var audioArg = string.IsNullOrEmpty(af) ? "-an" : $"-af \"{af}\" {aFlags}";
            return $"-nostdin -y -i \"{inputVideo}\" -vf \"{vf}\" {vFlags} {audioArg} \"{outputVideo}\"";
        }

        var (vFlagsMulti, aFlagsMulti) = BuildEncoderFlags(hasAudioFilter: !isPass1, pass, bitRateK, passLogPrefix);

        // Multi-segment or partial segment filter complex construction
        var vFilters = new List<string>();
        var aFilters = new List<string>();
        var concatv = new List<string>();
        var concata = new List<string>();

        var currentTime = TimeSpan.Zero;
        int index = 0;

        foreach (var seg in sortedSegments)
        {
            // Unchanged gap before segment
            if (seg.StartTime > currentTime + TimeSpan.FromMilliseconds(100))
            {
                var startSec = currentTime.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                var endSec = seg.StartTime.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);

                vFilters.Add($"[0:v]trim=start={startSec}:end={endSec},setpts=PTS-STARTPTS[v{index}]");
                concatv.Add($"[v{index}]");

                if (!isPass1)
                {
                    aFilters.Add($"[0:a]atrim=start={startSec}:end={endSec},asetpts=PTS-STARTPTS[a{index}]");
                    concata.Add($"[a{index}]");
                }
                index++;
            }

            // Speed modified segment
            var sSec = seg.StartTime.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
            var eSec = seg.EndTime.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
            var ratio = seg.EffectiveSpeedRatio;
            var ptsFactor = (1.0 / ratio).ToString("0.000000", CultureInfo.InvariantCulture);

            if (seg.IsReverse)
            {
                vFilters.Add($"[0:v]trim=start={sSec}:end={eSec},setpts={ptsFactor}*(PTS-STARTPTS),reverse[v{index}]");
            }
            else
            {
                vFilters.Add($"[0:v]trim=start={sSec}:end={eSec},setpts={ptsFactor}*(PTS-STARTPTS)[v{index}]");
            }
            concatv.Add($"[v{index}]");

            if (!isPass1)
            {
                var afPart = BuildAudioFilter(ratio, seg.AudioMode, seg.IsReverse);
                if (!string.IsNullOrEmpty(afPart))
                {
                    aFilters.Add($"[0:a]atrim=start={sSec}:end={eSec},asetpts=PTS-STARTPTS,{afPart}[a{index}]");
                }
                else
                {
                    // Silence stream for muted audio segment
                    var targetSec = seg.OutputDuration.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
                    aFilters.Add($"aevalsrc=0:d={targetSec}[a{index}]");
                }
                concata.Add($"[a{index}]");
            }
            index++;

            currentTime = seg.EndTime;
        }

        // Unchanged segment from last segment end to video end
        if (VideoDuration > currentTime + TimeSpan.FromMilliseconds(100))
        {
            var startSec = currentTime.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture);
            vFilters.Add($"[0:v]trim=start={startSec},setpts=PTS-STARTPTS[v{index}]");
            concatv.Add($"[v{index}]");

            if (!isPass1)
            {
                aFilters.Add($"[0:a]atrim=start={startSec},asetpts=PTS-STARTPTS[a{index}]");
                concata.Add($"[a{index}]");
            }
            index++;
        }

        var filterComplex = new StringBuilder();
        var vConcatInputs = string.Concat(concatv);

        if (isPass1)
        {
            filterComplex.Append(string.Join(";", vFilters));
            filterComplex.Append($";{vConcatInputs}concat=n={concatv.Count}:v=1:a=0[vconcat]");

            var finalVMap1 = "[vconcat]";
            if (needScale)
            {
                filterComplex.Append($";{finalVMap1}scale={scaledW}:{scaledH}[vscaled]");
                finalVMap1 = "[vscaled]";
            }

            if (!string.IsNullOrEmpty(assSubtitleFile))
            {
                var escapedPath = assSubtitleFile.Replace("\\", "/").Replace(":", "\\:");
                filterComplex.Append($";{finalVMap1}subtitles='{escapedPath}'[vout]");
                finalVMap1 = "[vout]";
            }

            var nullDevice = Configuration.IsRunningOnWindows ? "NUL" : "/dev/null";
            return $"-nostdin -y -i \"{inputVideo}\" -filter_complex \"{filterComplex}\" -map \"{finalVMap1}\" {vFlagsMulti} -an -f null {nullDevice}";
        }

        filterComplex.Append(string.Join(";", vFilters.Concat(aFilters)));
        var aConcatInputs = string.Concat(concata);

        filterComplex.Append($";{vConcatInputs}concat=n={concatv.Count}:v=1:a=0[vconcat]");
        filterComplex.Append($";{aConcatInputs}concat=n={concata.Count}:v=0:a=1[aconcat]");

        var finalVMap = "[vconcat]";
        if (needScale)
        {
            filterComplex.Append($";{finalVMap}scale={scaledW}:{scaledH}[vscaled]");
            finalVMap = "[vscaled]";
        }

        if (!string.IsNullOrEmpty(assSubtitleFile))
        {
            var escapedPath = assSubtitleFile.Replace("\\", "/").Replace(":", "\\:");
            filterComplex.Append($";{finalVMap}subtitles='{escapedPath}'[vout]");
            finalVMap = "[vout]";
        }

        return $"-nostdin -y -i \"{inputVideo}\" -filter_complex \"{filterComplex}\" -map \"{finalVMap}\" -map \"[aconcat]\" {vFlagsMulti} {aFlagsMulti} \"{outputVideo}\"";
    }

    private static string BuildAudioFilter(double speedRatio, AudioSpeedMode mode, bool isReverse = false)
    {
        if (mode == AudioSpeedMode.Mute)
        {
            return string.Empty;
        }

        var filters = new List<string>();

        if (mode == AudioSpeedMode.ShiftPitch || speedRatio > 4.0 || speedRatio < 0.25)
        {
            // Resample pitch shift method
            var rate = (int)(44100 * speedRatio);
            filters.Add($"asetrate={rate}");
            filters.Add("aresample=44100");
        }
        else
        {
            // Keep pitch method using atempo (0.5 to 2.0 per filter stage)
            var r = speedRatio;
            while (r > 2.0)
            {
                filters.Add("atempo=2.0");
                r /= 2.0;
            }
            while (r < 0.5)
            {
                filters.Add("atempo=0.5");
                r /= 0.5;
            }
            filters.Add($"atempo={r.ToString("0.000", CultureInfo.InvariantCulture)}");
        }

        if (isReverse)
        {
            filters.Add("areverse");
        }

        return string.Join(",", filters);
    }

    public Subtitle CreateSpeedAdjustedSubtitle(Subtitle originalSubtitle)
    {
        var remappedSubtitle = new Subtitle
        {
            Header = originalSubtitle.Header,
            Footer = originalSubtitle.Footer
        };

        var intervals = BuildTimelineIntervals();

        foreach (var p in originalSubtitle.Paragraphs)
        {
            var newStartMs = MapOriginalTimeToNew(p.StartTime.TotalMilliseconds, intervals);
            var newEndMs = MapOriginalTimeToNew(p.EndTime.TotalMilliseconds, intervals);

            if (newEndMs < newStartMs)
            {
                (newStartMs, newEndMs) = (newEndMs, newStartMs);
            }

            // Ensure valid duration
            if (newEndMs <= newStartMs)
            {
                newEndMs = newStartMs + 50; // Minimum 50ms visibility
            }

            var newP = new Paragraph(p)
            {
                StartTime = new TimeCode(newStartMs),
                EndTime = new TimeCode(newEndMs)
            };

            remappedSubtitle.Paragraphs.Add(newP);
        }

        remappedSubtitle.Paragraphs.Sort((a, b) => a.StartTime.TotalMilliseconds.CompareTo(b.StartTime.TotalMilliseconds));

        return remappedSubtitle;
    }

    private List<TimelineInterval> BuildTimelineIntervals()
    {
        var intervals = new List<TimelineInterval>();
        var sortedSegments = Segments.OrderBy(s => s.StartTime).ToList();

        double currentOrigMs = 0;
        double currentNewMs = 0;

        foreach (var seg in sortedSegments)
        {
            var segStartMs = seg.StartTime.TotalMilliseconds;
            var segEndMs = seg.EndTime.TotalMilliseconds;

            if (segEndMs <= segStartMs)
            {
                continue;
            }

            // Unmodified gap before this segment (runs at normal 1.0x speed)
            if (segStartMs > currentOrigMs + 10)
            {
                var gapDurationMs = segStartMs - currentOrigMs;
                intervals.Add(new TimelineInterval
                {
                    OrigStartMs = currentOrigMs,
                    OrigEndMs = segStartMs,
                    SpeedRatio = 1.0,
                    IsReverse = false,
                    NewStartMs = currentNewMs,
                    NewEndMs = currentNewMs + gapDurationMs
                });

                currentNewMs += gapDurationMs;
                currentOrigMs = segStartMs;
            }

            // Speed-modified segment
            var origDurationMs = segEndMs - segStartMs;
            var ratio = seg.EffectiveSpeedRatio > 0 ? seg.EffectiveSpeedRatio : 1.0;
            var newDurationMs = origDurationMs / ratio;

            intervals.Add(new TimelineInterval
            {
                OrigStartMs = segStartMs,
                OrigEndMs = segEndMs,
                SpeedRatio = ratio,
                IsReverse = seg.IsReverse,
                NewStartMs = currentNewMs,
                NewEndMs = currentNewMs + newDurationMs
            });

            currentNewMs += newDurationMs;
            currentOrigMs = segEndMs;
        }

        // Remaining tail of the video (runs at normal 1.0x speed)
        var totalVideoMs = VideoDuration.TotalMilliseconds;
        var endBoundMs = Math.Max(currentOrigMs + 86400000, totalVideoMs); // At least 24h buffer
        if (endBoundMs > currentOrigMs)
        {
            var tailDurationMs = endBoundMs - currentOrigMs;
            intervals.Add(new TimelineInterval
            {
                OrigStartMs = currentOrigMs,
                OrigEndMs = endBoundMs,
                SpeedRatio = 1.0,
                IsReverse = false,
                NewStartMs = currentNewMs,
                NewEndMs = currentNewMs + tailDurationMs
            });
        }

        return intervals;
    }

    private static double MapOriginalTimeToNew(double origMs, List<TimelineInterval> intervals)
    {
        if (origMs <= 0 || intervals.Count == 0)
        {
            return 0;
        }

        foreach (var interval in intervals)
        {
            if (origMs >= interval.OrigStartMs && origMs <= interval.OrigEndMs)
            {
                var offset = interval.IsReverse
                    ? (interval.OrigEndMs - origMs)
                    : (origMs - interval.OrigStartMs);
                return interval.NewStartMs + (offset / interval.SpeedRatio);
            }
        }

        // If beyond last interval, extrapolate at 1.0x
        var last = intervals[^1];
        if (origMs > last.OrigEndMs)
        {
            return last.NewEndMs + (origMs - last.OrigEndMs);
        }

        return origMs;
    }

    private sealed class TimelineInterval
    {
        public double OrigStartMs { get; set; }
        public double OrigEndMs { get; set; }
        public double SpeedRatio { get; set; }
        public bool IsReverse { get; set; }
        public double NewStartMs { get; set; }
        public double NewEndMs { get; set; }
    }
}
