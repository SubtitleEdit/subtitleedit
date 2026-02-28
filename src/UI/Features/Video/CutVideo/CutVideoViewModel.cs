using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

public partial class CutVideoViewModel : ObservableObject
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
    [ObservableProperty] private bool _isAudioVisualizerVisible;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _segments;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSegment;
    [ObservableProperty] private int _selectedSegmentIndex;
    [ObservableProperty] private ObservableCollection<CutTypeDisplay> _cutTypes;
    [ObservableProperty] private CutTypeDisplay _selectedCutType;
    [ObservableProperty] private bool _isSetStartEnabled;
    [ObservableProperty] private bool _isSetEndEnabled;
    [ObservableProperty] private bool _isDeleteEnabled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayer { get; internal set; }
    public AudioVisualizer AudioVisualizer { get; internal set; }
    public DataGrid SegmentGrid { get; internal set; }

    private Subtitle _subtitle = new();
    private readonly StringBuilder _log;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private Process? _ffmpegListKeyFramesProcess;
    private readonly System.Timers.Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;
    private bool _updateAudioVisualizer;
    private DispatcherTimer _positionTimer = new DispatcherTimer();
    private string _importFileName;
    private Subtitle _currentSubtitle;
    private long _lastKeyPressedMs;
    private SubtitleLineViewModel? _setEndAtKeyUpLine;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;
    private readonly IInsertService _insertService;
    private readonly IShortcutManager _shortcutManager;


    public CutVideoViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService, IInsertService insertService, IShortcutManager shortcutManager)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;
        _insertService = insertService;
        _shortcutManager = shortcutManager;

        VideoWidth = 1920;
        VideoHeight = 1080;

        FrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        SelectedFrameRate = FrameRates[0];

        VideoExtensions = new ObservableCollection<string>
        {
            ".mkv",
            ".mp4",
            ".webm",
        };
        SelectedVideoExtension = VideoExtensions[0];

        CutTypes = new ObservableCollection<CutTypeDisplay>(CutTypeDisplay.GetCutTypes());
        SelectedCutType = CutTypes[0];

        JobItems = new ObservableCollection<BurnInJobItem>();
        VideoPlayer = new VideoPlayerControl(new VideoPlayerInstanceNone());
        AudioVisualizer = new AudioVisualizer();
        SegmentGrid = new DataGrid();
        Segments = new ObservableCollection<SubtitleLineViewModel>();
        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;

        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;
        _importFileName = string.Empty;
        _inputVideoFileName = string.Empty;
        _currentSubtitle = new Subtitle();
        UpdateSelection();
        LoadSettings();
    }

    public void Initialize(
        string videoFileName,
        WavePeakData2? wavePeakData,
        Subtitle subtitle,
        SubtitleFormat subtitleFormat,
        MainViewModel mainVm,
        List<SubtitleLineViewModel>? selectedItems = null)
    {
        VideoFileName = videoFileName;
        _inputVideoFileName = videoFileName;
        _currentSubtitle = subtitle;
        _subtitleFormat = subtitleFormat;

        _ffmpegListKeyFramesProcess = FfmpegGenerator.ListKeyFrames(videoFileName, OutputHandlerKeyFrames);

#pragma warning disable CA1416 // Validate platform compatibility
        var startResult = _ffmpegListKeyFramesProcess.Start();
        if (!startResult)
        {
            SeLogger.Error("Failed to start ffmpeg process for listing key frames: " + _ffmpegListKeyFramesProcess.StartInfo.FileName + " " + _ffmpegListKeyFramesProcess.StartInfo.Arguments);
            return;
        }
#pragma warning restore CA1416 // Validate platform compatibility

        _ffmpegListKeyFramesProcess.BeginOutputReadLine();
        _ffmpegListKeyFramesProcess.BeginErrorReadLine();

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(videoFileName))
            {
                _ = VideoPlayer.Open(videoFileName);
            }

            if (wavePeakData != null)
            {
                AudioVisualizer.WavePeaks = wavePeakData;
                IsAudioVisualizerVisible = true;
            }

            if (selectedItems != null)
            {
                foreach (var item in selectedItems)
                {
                    var segment = new SubtitleLineViewModel(new Paragraph(item.Text, item.StartTime.TotalMilliseconds, item.EndTime.TotalMilliseconds), subtitleFormat);
                    _insertService.InsertInCorrectPosition(Segments, segment);
                }
            }

            _updateAudioVisualizer = true;
        });

        LoadShortcuts(mainVm);
    }

    private void StartTitleTimer()
    {
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _positionTimer.Tick += (s, e) =>
        {
            UpdateAudioVisualizer(VideoPlayer.VideoPlayerInstance, AudioVisualizer, SelectedSegmentIndex);

            if (_updateAudioVisualizer)
            {
                AudioVisualizer.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };

        _positionTimer.Start();
    }

    private void UpdateAudioVisualizer(
        IVideoPlayerInstance vp,
        AudioVisualizer av,
        int selectedParagraphIndex)
    {
        SubtitleLineViewModel? selectedParagraph = selectedParagraphIndex < 0
            ? null
            : (Segments.Count == 0 ? null : Segments[selectedParagraphIndex]);

        var subtitle = new ObservableCollection<SubtitleLineViewModel>();
        var orderedList = Segments.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var firstSelectedIndex = -1;
        for (var i = 0; i < orderedList.Count; i++)
        {
            var dp = orderedList[i];
            subtitle.Add(dp);
        }

        var mediaPlayerSeconds = vp.Position;
        var startPos = mediaPlayerSeconds - 0.01;
        if (startPos < 0)
        {
            startPos = 0;
        }

        av.CurrentVideoPositionSeconds = vp.Position;
        var isPlaying = vp.IsPlaying;

        if (!isPlaying)
        {
            startPos = av.StartPositionSeconds;
        }

        var selectedSubtitles = new List<SubtitleLineViewModel>
        {
            selectedParagraph ??  new  SubtitleLineViewModel(),
        };

        if ((isPlaying || !av.IsScrolling) && (mediaPlayerSeconds > av.EndPositionSeconds ||
                                               mediaPlayerSeconds < av.StartPositionSeconds))
        {
            av.SetPosition(startPos, subtitle, mediaPlayerSeconds, 0,
                selectedSubtitles);
        }
        else
        {
            av.SetPosition(av.StartPositionSeconds, subtitle, mediaPlayerSeconds, firstSelectedIndex,
                selectedSubtitles);
        }
    }

    private void OutputHandlerKeyFrames(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        const string marker = "pts_time:";
        var idx = outLine.Data.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var afterMarker = outLine.Data.Substring(idx + marker.Length);
            var endIdx = afterMarker.IndexOf(' ');
            var ptsValue = endIdx > 0 ? afterMarker.Substring(0, endIdx) : afterMarker;

            if (double.TryParse(ptsValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
            {
                AudioVisualizer.ShotChanges.Add(seconds);
                _updateAudioVisualizer = true;
            }
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
                    "Unable to generate video",
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
                var sb = new StringBuilder($"Generated files ({JobItems.Count}):" + Environment.NewLine +
                                           Environment.NewLine);
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

    private void InitAndStartJobItem(int index)
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _jobItemIndex = index;
        var jobItem = JobItems[index];
        var mediaInfo = FfmpegMediaInfo.Parse(jobItem.InputVideoFileName);
        jobItem.TotalFrames = mediaInfo.GetTotalFrames();
        jobItem.TotalSeconds = mediaInfo.Duration.TotalSeconds;
        jobItem.Width = mediaInfo.Dimension.Width;
        jobItem.Height = mediaInfo.Dimension.Height;
        jobItem.UseTargetFileSize = false;
        jobItem.Status = Se.Language.General.Generating;

        var result = RunEncoding(jobItem);
        if (result)
        {
            _timerGenerate.Start();
        }
    }

    private bool RunEncoding(BurnInJobItem jobItem)
    {
        string arguments;

        if (SelectedCutType.CutType == CutType.MergeSegments)
        {
            arguments = FfmpegGenerator.GetMergeSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, Segments.ToList());
        }
        else
        {
            arguments = FfmpegGenerator.GetRemoveSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, Segments.ToList());
        }

        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
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

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems(string outputVideoFileName)
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
            OutputVideoFileName = outputVideoFileName,
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = SelectedVideoExtension;
        var suffix = Se.Settings.Video.BurnIn.BurnInSuffix;
        var fileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, nameNoExt + suffix + ext);
        if (Se.Settings.Video.BurnIn.UseOutputFolder &&
            !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder) &&
            Directory.Exists(Se.Settings.Video.BurnIn.OutputFolder))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, nameNoExt + suffix + ext);
        }

        var i = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, $"{nameNoExt}{suffix}_{i}{ext}");
            i++;
        }

        return fileName;
    }

    public static int CalculateFontSize(int videoWidth, int videoHeight, double factor, int minSize = 8,
        int maxSize = 2000)
    {
        factor = Math.Clamp(factor, 0, 1);

        // Calculate the diagonal resolution
        var diagonalResolution = Math.Sqrt(videoWidth * videoWidth + videoHeight * videoHeight);

        // Calculate base size (when factor is 0.5)
        var baseSize = diagonalResolution * 0.019; // around 2% of diagonal as base size

        // Apply logarithmic scaling
        var scaleFactor = Math.Pow(maxSize / baseSize, 2 * (factor - 0.5));
        var fontSize = (int)Math.Round(baseSize * scaleFactor);

        // Clamp the font size between minSize and maxSize
        return Math.Clamp(fontSize, minSize, maxSize);
    }

    [RelayCommand]
    private void Add()
    {
        var ms = VideoPlayer.Position * 1000;
        var segment = new SubtitleLineViewModel(new Paragraph(string.Empty, ms, ms + Se.Settings.General.NewEmptyDefaultMs), _subtitleFormat ?? new SubRip());
        var index = _insertService.InsertInCorrectPosition(Segments, segment);
        SelectAndScrollToRow(index);
        Renumber();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task Import()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null || subtitle.Paragraphs.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "The selected subtitle file contains no subtitles.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _importFileName = fileName;
        foreach (var p in subtitle.Paragraphs)
        {
            var segment = new SubtitleLineViewModel(p, _subtitleFormat ?? new SubRip());
            _insertService.InsertInCorrectPosition(Segments, segment);
        }

        Renumber();
        SelectAndScrollToRow(0);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void ImportCurrent()
    {
        foreach (var p in _currentSubtitle.Paragraphs)
        {
            var segment = new SubtitleLineViewModel(p, _subtitleFormat ?? new SubRip());
            _insertService.InsertInCorrectPosition(Segments, segment);
        }

        Renumber();
        SelectAndScrollToRow(0);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void Delete()
    {
        var selectedSegments = SegmentGrid.SelectedItems.Cast<SubtitleLineViewModel>().ToList();
        if (selectedSegments.Count == 0)
        {
            return;
        }

        var idx = Segments.IndexOf(selectedSegments.First());

        foreach (var segment in selectedSegments)
        {
            Segments.Remove(segment);
        }

        if (idx < Segments.Count)
        {
            SelectedSegment = Segments[idx];
        }
        else if (idx - 1 < Segments.Count && idx > 0)
        {
            SelectedSegment = Segments[idx - 1];
        }

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void Clear()
    {
        Segments.Clear();
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (Segments.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                "No segments added",
                $"Add one or more segments - e.g. via the waveform",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        var outputVideoFileName = MakeOutputFileName(VideoFileName);
        outputVideoFileName = await _fileHelper.PickSaveFile(Window!, SelectedVideoExtension, outputVideoFileName, Se.Language.Video.SaveVideoAsTitle);
        if (string.IsNullOrEmpty(outputVideoFileName))
        {
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

        InitAndStartJobItem(0);
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video;
        SelectedCutType = CutTypes.FirstOrDefault(ct => ct.CutType.ToString() == settings.CutType) ?? CutTypes[0];
        SelectedVideoExtension = VideoExtensions.Contains(settings.CutDefaultVideoExtension)
            ? settings.CutDefaultVideoExtension
            : VideoExtensions[0];
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video;
        settings.CutType = SelectedCutType.CutType.ToString();
        settings.CutDefaultVideoExtension = SelectedVideoExtension;
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (IsGenerating)
        {
            _doAbort = true;
            IsGenerating = false;
            return;
        }

        if (Segments.Count > 0)
        {
            var message = "Are you sure you want to discard segments and close window? All segments will be lost.";
            var result = await MessageBox.Show(Window!, Se.Language.General.Cancel.Replace("_", string.Empty), message, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        Window?.Close();
    }

    private readonly Lock _onKeyDownHandlerLock = new();

    internal void OnKeyDownHandler(object? sender, KeyEventArgs keyEventArgs)
    {
        lock (_onKeyDownHandlerLock)
        {
            if (keyEventArgs.Key == Key.Escape)
            {
                keyEventArgs.Handled = true;
                _ = Cancel();
                return;
            }
            else if (keyEventArgs.Key == Key.F1)
            {
                keyEventArgs.Handled = true;
                UiUtil.ShowHelp("features/cut-video");
                return;
            }
            else if (keyEventArgs.Key == Key.Space)
            {
                keyEventArgs.Handled = true;
                VideoPlayer.TogglePlayPause();
                return;
            }

            AudioVisualizer?.SetKeyModifiers(keyEventArgs);
            var ms = Environment.TickCount64;
            var msDiff = ms - _lastKeyPressedMs;
            var k = keyEventArgs.Key;
            if (msDiff > 5000)
            {
                _shortcutManager.ClearKeys(); // reset shortcuts if no key pressed for 5 seconds
            }

            _lastKeyPressedMs = ms;

            _shortcutManager.OnKeyPressed(this, keyEventArgs);
            if (_shortcutManager.GetActiveKeys().Count == 0)
            {
                return;
            }

            if (SegmentGrid.IsFocused)
            {
                if (keyEventArgs.Key == Key.Home && keyEventArgs.KeyModifiers == KeyModifiers.None && Segments.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    SelectAndScrollToRow(0);
                    return;
                }
                else if (keyEventArgs.Key == Key.End && keyEventArgs.KeyModifiers == KeyModifiers.None && Segments.Count > 0)
                {
                    keyEventArgs.Handled = true;
                    SelectAndScrollToRow(Segments.Count - 1);
                    return;
                }
                else if (keyEventArgs.Key == Key.Enter && keyEventArgs.KeyModifiers == KeyModifiers.None)
                {
                    if (Se.Settings.General.SubtitleEnterKeyAction == SubtitleEnterKeyActionType.GoToSubtitleAndSetVideoPosition.ToString())
                    {
                        keyEventArgs.Handled = true;
                        var idx = SegmentGrid.SelectedIndex;
                        var item = SegmentGrid.SelectedItem as SubtitleLineViewModel;
                        var vp = VideoPlayer;
                        if (idx >= 0 && item != null && vp != null)
                        {
                            vp.Position = item.StartTime.TotalSeconds;
                            SelectAndScrollToRow(idx);
                            if (AudioVisualizer != null &&
                                (item.StartTime.TotalSeconds < AudioVisualizer.StartPositionSeconds ||
                                 item.StartTime.TotalSeconds + 0.2 > AudioVisualizer.EndPositionSeconds))
                            {
                                AudioVisualizer.CenterOnPosition(item);
                            }
                            else
                            {
                                AudioVisualizerCenterOnPositionIfNeeded(item.StartTime.TotalSeconds);
                            }

                            _updateAudioVisualizer = true;
                        }
                    }

                    return;
                }

                var relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.SubtitleGrid.ToString());
                if (relayCommand == null)
                {
                    relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.SubtitleGridAndTextBox.ToString());
                }

                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            if (AudioVisualizer != null && AudioVisualizer.IsFocused)
            {
                var relayCommand = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.Waveform.ToString());
                if (relayCommand != null)
                {
                    keyEventArgs.Handled = true;
                    relayCommand.Execute(null);
                    return;
                }
            }

            var keys = _shortcutManager.GetActiveKeys().Select(p => p.ToString()).ToList();
            var hashCode = ShortCut.CalculateHash(keys, ShortcutCategory.General.ToString());
            var rc = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.General.ToString().ToLowerInvariant());
            if (rc != null)
            {
                keyEventArgs.Handled = true;
                rc.Execute(null);
            }
        }
    }

    public void OnKeyUpHandler(object? sender, KeyEventArgs e)
    {
        if (_setEndAtKeyUpLine != null)
        {
            _setEndAtKeyUpLine = null;
        }

        _shortcutManager.OnKeyReleased(this, e);
        AudioVisualizer?.SetKeyModifiers(e);
    }

    internal void AudioVisualizerPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        VideoPlayer.Position = e.PositionInSeconds;
        _updateAudioVisualizer = true;
    }

    internal void OnClosing()
    {
        _positionTimer.Stop();
        VideoPlayer.VideoPlayerInstance.CloseFile();

        if (_ffmpegListKeyFramesProcess != null && !_ffmpegListKeyFramesProcess.HasExited)
        {
            try
            {
                _ffmpegListKeyFramesProcess.Kill(true);
            }
            catch
            {
                // ignore
            }
        }

        SaveSettings();
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnLoaded()
    {
        StartTitleTimer();
        _updateAudioVisualizer = true;
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void AudioVisualizerOnNewSelectionInsert(object sender, ParagraphEventArgs e)
    {
        var index = _insertService.InsertInCorrectPosition(Segments, e.Paragraph);
        SelectAndScrollToRow(index);
        Renumber();
        _updateAudioVisualizer = true;
    }

    private void Renumber()
    {
        for (var index = 0; index < Segments.Count; index++)
        {
            Segments[index].Number = index + 1;
        }
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Segments.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SegmentGrid.SelectedIndex = index;
            SegmentGrid.ScrollIntoView(SegmentGrid.SelectedItem, null);
            UpdateSelection();
        }, DispatcherPriority.Background);
    }

    internal void SegmentsGridChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        IsDeleteEnabled = SelectedSegment != null;
        IsSetStartEnabled = SegmentGrid.SelectedItems.Count == 1;
        IsSetEndEnabled = SegmentGrid.SelectedItems.Count == 1;
    }

    internal void SegmentsGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        var selectedSegment = SelectedSegment;
        if (selectedSegment == null)
        {
            return;
        }

        VideoPlayer.Position = selectedSegment.StartTime.TotalSeconds;
        AudioVisualizerCenterOnPositionIfNeeded(selectedSegment.StartTime.TotalSeconds);
    }

    internal void AudioVisualizerSelectRequested(object sender, ParagraphEventArgs e)
    {
        var s = Segments.FirstOrDefault(p => p.Id == e.Paragraph.Id);
        if (s != null)
        {
            SegmentGrid.SelectedItem = s;
        }
    }

    internal void AudioVisualizerOnPrimarySingleClicked(object sender, ParagraphNullableEventArgs e)
    {
        var vp = VideoPlayer;
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        if (Enum.TryParse<WaveformSingleClickActionType>(Se.Settings.Waveform.SingleClickAction, out var action))
        {
            switch (action)
            {
                case WaveformSingleClickActionType.SetVideoPositionAndPauseAndSelectSubtitle:
                    vp.VideoPlayerInstance.Pause();
                    vp.Position = e.Seconds;
                    AudioVisualizerCenterOnPositionIfNeeded(e.Seconds);
                    if (e.Paragraph != null)
                    {
                        var p1 = Segments.FirstOrDefault(p => p.Id == e.Paragraph.Id);
                        if (p1 != null)
                        {
                            SelectAndScrollToRow(Segments.IndexOf(p1));
                        }
                    }

                    break;
                case WaveformSingleClickActionType.SetVideopositionAndPauseAndSelectSubtitleAndCenter:
                    vp.VideoPlayerInstance.Pause();
                    vp.Position = e.Seconds;
                    AudioVisualizerCenterOnPositionIfNeeded(e.Seconds);
                    if (e.Paragraph != null)
                    {
                        var p2 = Segments.FirstOrDefault(p => p.Id == e.Paragraph.Id);
                        if (p2 != null)
                        {
                            SelectAndScrollToRow(Segments.IndexOf(p2));
                            AudioVisualizer.CenterOnPosition(e.Seconds);
                        }
                    }

                    break;
                case WaveformSingleClickActionType.SetVideoPositionAndPause:
                    vp.VideoPlayerInstance.Pause();
                    vp.Position = e.Seconds;
                    AudioVisualizerCenterOnPositionIfNeeded(e.Seconds);
                    break;
                case WaveformSingleClickActionType.SetVideopositionAndPauseAndCenter:
                    vp.VideoPlayerInstance.Pause();
                    vp.Position = e.Seconds;
                    if (e.Paragraph != null)
                    {
                        AudioVisualizer.CenterOnPosition(e.Seconds);
                    }

                    break;
                case WaveformSingleClickActionType.SetVideoposition:
                    vp.Position = e.Seconds;
                    AudioVisualizerCenterOnPositionIfNeeded(e.Seconds);
                    break;
            }

            _updateAudioVisualizer = true;
        }
    }

    internal void AudioVisualizerOnPrimaryDoubleClicked(object sender, ParagraphNullableEventArgs e)
    {
        var vp = VideoPlayer;
        if (vp == null || AudioVisualizer == null)
        {
            return;
        }

        if (Enum.TryParse<WaveformDoubleClickActionType>(Se.Settings.Waveform.DoubleClickAction, out var action))
        {
            switch (action)
            {
                case WaveformDoubleClickActionType.SelectSubtitle:
                    if (e.Paragraph != null)
                    {
                        var p = Segments.FirstOrDefault(p => Math.Abs(p.StartTime.TotalMilliseconds - e.Paragraph.StartTime.TotalMilliseconds) < 0.01);
                        if (p != null)
                        {
                            SelectAndScrollToRow(Segments.IndexOf(p));
                        }
                    }

                    break;
                case WaveformDoubleClickActionType.Center:
                    if (e.Paragraph != null)
                    {
                        AudioVisualizerCenterOnPositionIfNeeded(e.Paragraph, e.Seconds);
                    }

                    break;
                case WaveformDoubleClickActionType.Pause:
                    vp.VideoPlayerInstance.Pause();
                    break;
                case WaveformDoubleClickActionType.Play:
                    vp.VideoPlayerInstance.Play();
                    break;
            }

            _updateAudioVisualizer = true;
        }
    }

    private void AudioVisualizerCenterOnPositionIfNeeded(double seconds)
    {
        if (AudioVisualizer != null)
        {
            if (seconds <= AudioVisualizer.StartPositionSeconds ||
                seconds + 0.2 >= AudioVisualizer.EndPositionSeconds)
            {
                AudioVisualizer.CenterOnPosition(seconds);
                _updateAudioVisualizer = true;
            }
        }
    }

    private void AudioVisualizerCenterOnPositionIfNeeded(SubtitleLineViewModel selectedItem, double seconds)
    {
        if (AudioVisualizer != null)
        {
            if (seconds <= AudioVisualizer.StartPositionSeconds ||
                seconds + 0.2 >= AudioVisualizer.EndPositionSeconds)
            {
                AudioVisualizer.CenterOnPosition(selectedItem);
                _updateAudioVisualizer = true;
            }
        }
    }

    private void LoadShortcuts(MainViewModel mainVm)
    {
        Se.Settings.InitializeMainShortcuts(mainVm);
        var mainShortCuts = ShortcutsMain.GetUsedShortcuts(mainVm);

        var shortcuts = new List<ShortCut?>
        {
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.WaveformSetStartCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.WaveformSetEndCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.WaveformSetEndAndGoToNextCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.PlayCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.PauseCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.PlayNextCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.TogglePlayPauseCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.TogglePlayPause2Command),
        };

        foreach (var sc in shortcuts.Where(p => p != null))
        {
            var mappedShortcut = MappShortcut(sc, mainVm);
            if (mappedShortcut != null)
            {
                _shortcutManager.RegisterShortcut(mappedShortcut);
            }
        }
    }

    private ShortCut? MappShortcut(ShortCut? sc, MainViewModel mainVm)
    {
        if (sc == null)
        {
            return null;
        }

        var action = MapAction(sc.Action, mainVm);
        if (action == null)
        {
            return null;
        }

        return new ShortCut(sc.Name, sc.Keys, sc.Category, action);
    }

    private IRelayCommand? MapAction(IRelayCommand action, MainViewModel mainVm)
    {
        if (action == mainVm.WaveformSetStartCommand)
        {
            return SetStartCommand;
        }

        if (action == mainVm.WaveformSetEndCommand)
        {
            return SetEndCommand;
        }

        if (action == mainVm.WaveformSetEndAndGoToNextCommand)
        {
            return SetEndAndGoToNextCommand;
        }

        if (action == mainVm.PlayCommand)
        {
            return PlayCommand;
        }

        if (action == mainVm.PauseCommand)
        {
            return PauseCommand;
        }

        if (action == mainVm.PlayNextCommand)
        {
            return PlayNextCommand;
        }

        if (action == mainVm.TogglePlayPauseCommand)
        {
            return TogglePlayPauseCommand;
        }

        return null;
    }


    [RelayCommand]
    private void SetStart()
    {
        var segment = SelectedSegment;
        if (segment == null || Segments.Count == 0)
        {
            return;
        }

        var seconds = VideoPlayer.Position;
        segment.SetStartTimeOnly(TimeSpan.FromSeconds(seconds));
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SetEnd()
    {
        var segment = SelectedSegment;
        if (segment == null || Segments.Count == 0)
        {
            return;
        }

        var seconds = VideoPlayer.Position;
        segment.EndTime = TimeSpan.FromSeconds(seconds);
        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void SetEndAndGoToNext()
    {
        var s = SelectedSegment;
        var vp = VideoPlayer;
        if (s == null || vp == null)
        {
            return;
        }

        var idx = Segments.IndexOf(s);
        if (idx < 0)
        {
            return;
        }

        var videoPositionSeconds = vp.Position;
        var gap = Se.Settings.General.MinimumMillisecondsBetweenLines / 1000.0;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + gap)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);

        SelectAndScrollToRow(idx + 1);

        _updateAudioVisualizer = true;
    }

    [RelayCommand]
    private void Play()
    {
        VideoPlayer.VideoPlayerInstance.Play();
    }

    [RelayCommand]
    private void Pause()
    {
        VideoPlayer.VideoPlayerInstance.Pause();
    }

    [RelayCommand]
    private void PlayNext()
    {
        var s = SelectedSegment;
        var vp = VideoPlayer;
        if (s == null || vp == null)
        {
            return;
        }

        var idx = Segments.IndexOf(s) + 1;
        if (idx <= 0)
        {
            return;
        }

        var nextSegment = idx < Segments.Count ? Segments[idx] : null;
        if (nextSegment == null)
        {
            return;
        }

        vp.Position = nextSegment.StartTime.TotalSeconds;
        AudioVisualizerCenterOnPositionIfNeeded(nextSegment.StartTime.TotalSeconds);
        SelectAndScrollToRow(idx);
        _updateAudioVisualizer = true;
        vp.VideoPlayerInstance.Play();
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        VideoPlayer.TogglePlayPause();
    }
}