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
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
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
using Nikse.SubtitleEdit.UiLogic.Media;

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
    [ObservableProperty] private ObservableCollection<CutTypeDisplay> _cutTypes;
    [ObservableProperty] private CutTypeDisplay _selectedCutType;
    [ObservableProperty] private bool _isSetStartEnabled;
    [ObservableProperty] private bool _isSetEndEnabled;
    [ObservableProperty] private bool _isDeleteEnabled;
    [ObservableProperty] private bool _cutSubtitleToo;
    [ObservableProperty] private bool _isCutSubtitleVisible;
    [ObservableProperty] private bool _transitionEnabled;
    [ObservableProperty] private ObservableCollection<CutTransitionDisplay> _transitions;
    [ObservableProperty] private CutTransitionDisplay _selectedTransition;
    [ObservableProperty] private double? _transitionDuration;
    [ObservableProperty] private bool _fadeInEnabled;
    [ObservableProperty] private double? _fadeInDuration;
    [ObservableProperty] private bool _fadeOutEnabled;
    [ObservableProperty] private double? _fadeOutDuration;
    [ObservableProperty] private string _transitionInfo;
    [ObservableProperty] private bool _isPreviewing;
    [ObservableProperty] private bool _isPreviewTransitionEnabled;
    [ObservableProperty] private bool _isStatusTextVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl VideoPlayer { get; internal set; }
    public AudioVisualizer AudioVisualizer { get; internal set; }
    public TableView SegmentGrid { get; internal set; }

    private Subtitle _subtitle = new();
    private readonly StringBuilder _log;
    private readonly TempSubtitleFiles _tempSubtitleFiles = new();
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
    private bool _isClosing;

    // What is being generated, frozen when Generate starts: copies of the segments sorted by
    // start time, plus the cut type. The ffmpeg arguments used to take the rows in grid order
    // (SetStart/waveform edits can leave them out of time order) while the cut subtitle sorted
    // them - and read the live rows at completion time, after the user may have edited them -
    // so the video and its subtitle could be cut differently.
    private List<SubtitleLineViewModel> _generateSegments = new();
    private CutType _generateCutType;

    // The transition/fade settings of the run, completed with the input's frame rate and
    // duration once the job starts - the video and the cut subtitle are both built from them.
    private CutVideoTransitionOptions _generateTransitions = new();

    // ffmpeg's stdout and stderr readers both call OutputHandlerKeyFrames, on two thread pool
    // threads, while the waveform render thread walks AudioVisualizer.ShotChanges every frame.
    // List<T>.Add publishes the grown array and the new size non-atomically, so adding straight
    // into the live list could throw IndexOutOfRangeException out of Render. Collect here under a
    // lock and hand the visualizer a finished list on the UI thread instead - what every other
    // shot-change writer does - coalescing the publishes so a long file cannot flood the queue.
    private readonly List<double> _keyFrameSeconds = new List<double>();
    private bool _keyFramePublishPending;
    private UiTickPump _positionTimer = new(TimeSpan.FromMilliseconds(150)); // posted ticks, not a DispatcherTimer - see UiTickPump
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

        // No .webm: the video path always encodes libx264, which the WebM muxer cannot carry.
        // (.mp3/.wav take the audio-only branch and are fine.)
        VideoExtensions = new ObservableCollection<string>
        {
            ".mkv",
            ".mp4",
            ".mp3",
            ".wav",
        };
        SelectedVideoExtension = VideoExtensions[0];

        CutTypes = new ObservableCollection<CutTypeDisplay>(CutTypeDisplay.GetCutTypes());
        SelectedCutType = CutTypes[0];

        Transitions = new ObservableCollection<CutTransitionDisplay>(CutTransitionDisplay.GetTransitions());
        SelectedTransition = Transitions[0];
        TransitionInfo = string.Empty;

        JobItems = new ObservableCollection<BurnInJobItem>();
        VideoPlayer = new VideoPlayerControl(new EmptyVideoPlayer());
        AudioVisualizer = new AudioVisualizer();
        SegmentGrid = new TableView();
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
        IsCutSubtitleVisible = subtitle.Paragraphs.Count > 0;

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

        if (videoFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            SelectedVideoExtension = ".mp3";
        }
        else if (videoFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            SelectedVideoExtension = ".wav";
        }
        else if (SelectedVideoExtension == ".mp3" || SelectedVideoExtension == ".wav")
        {
            SelectedVideoExtension = VideoExtensions[0];
        }
    }

    private void StartTitleTimer()
    {
        _positionTimer = new UiTickPump(TimeSpan.FromMilliseconds(150));
        _positionTimer.Tick += (s, e) =>
        {
            // Derive the index from the row the grid actually binds. SelectedSegmentIndex is
            // written nowhere in the repo and is not bound, so it stayed 0 and the waveform
            // always highlighted the first segment no matter which row was selected.
            UpdateAudioVisualizer(VideoPlayer.VideoPlayer, AudioVisualizer, SelectedSegment == null ? -1 : Segments.IndexOf(SelectedSegment));

            if (_updateAudioVisualizer)
            {
                AudioVisualizer.InvalidateVisual();
                _updateAudioVisualizer = false;
            }
        };

        _positionTimer.Start();
    }

    private void UpdateAudioVisualizer(
        IVideoPlayer vp,
        AudioVisualizer av,
        int selectedParagraphIndex)
    {
        SubtitleLineViewModel? selectedParagraph = selectedParagraphIndex < 0
            ? null
            : (Segments.Count == 0 ? null : Segments[selectedParagraphIndex]);

        var subtitle = Segments.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
        var firstSelectedIndex = -1;

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
                lock (_keyFrameSeconds)
                {
                    _keyFrameSeconds.Add(seconds);
                    if (_keyFramePublishPending)
                    {
                        return;
                    }

                    _keyFramePublishPending = true;
                }

                Dispatcher.UIThread.Post(() =>
                {
                    List<double> snapshot;
                    lock (_keyFrameSeconds)
                    {
                        _keyFramePublishPending = false;
                        snapshot = new List<double>(_keyFrameSeconds);
                    }

                    AudioVisualizer.ShotChanges = snapshot;
                    _updateAudioVisualizer = true;
                }, DispatcherPriority.Background);
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

            // The half-written output used to stay on disk after an abort, looking like a
            // finished video. Remove it once ffmpeg is gone (it holds the file open until then).
            KillFfmpegProcess();
            DeletePartialOutputFile();

            IsGenerating = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            // TotalFrames is 0 for audio-only input (no frame rate) and nothing is processed in
            // the first ticks - dividing by either gave NaN/Infinity.
            var totalFrames = JobItems[_jobItemIndex].TotalFrames;
            var percentage = totalFrames > 0
                ? (int)Math.Round((double)_processedFrames / totalFrames * 100.0, MidpointRounding.AwayFromZero)
                : 0;
            percentage = Math.Clamp(percentage, 0, 100);

            var estimatedLeft = string.Empty;
            if (totalFrames > 0 && _processedFrames > 0)
            {
                var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                var msPerFrame = (double)durationMs / _processedFrames;
                var estimatedTotalMs = msPerFrame * totalFrames;
                estimatedLeft = ProgressHelper.ToProgressTime(Math.Max(0, estimatedTotalMs - durationMs));
            }

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

        // "The output file exists" is not success: it also holds for a file the user chose to
        // overwrite and for the 0-byte stub a failed ffmpeg leaves behind, and both were reported
        // as "Video file generated". ffmpeg has exited here (HasExited above), so ExitCode is safe.
        var exitCode = _ffmpegProcess.ExitCode;
        var outputFileInfo = new FileInfo(jobItem.OutputVideoFileName);
        if (exitCode != 0 || !outputFileInfo.Exists || outputFileInfo.Length == 0)
        {
            SeLogger.Error("Output video file not generated: " + jobItem.OutputVideoFileName + Environment.NewLine +
                                 "ffmpeg: " + _ffmpegProcess.StartInfo.FileName + Environment.NewLine +
                                 "Parameters: " + _ffmpegProcess.StartInfo.Arguments + Environment.NewLine +
                                 "OS: " + Environment.OSVersion + Environment.NewLine +
                                 "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                                 "ffmpeg exit code: " + exitCode + Environment.NewLine +
                                 "ffmpeg log: " + _log);

            if (_isClosing)
            {
                return; // the window killed ffmpeg on its way out - nobody is left to tell
            }

            Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!,
                    "Unable to generate video",
                    "Output video file not generated: " + jobItem.OutputVideoFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments,
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
                InitAndStartJobItem(_jobItemIndex + 1);
                return;
            }

            IsGenerating = false;

            if (JobItems.Count == 1)
            {
                var message = string.Format(Se.Language.General.VideoFileGeneratedX, jobItem.OutputVideoFileName);

                var cutSubtitleFileName = WriteCutSubtitle(jobItem.OutputVideoFileName, jobItem.TotalSeconds);
                if (!string.IsNullOrEmpty(cutSubtitleFileName))
                {
                    message += Environment.NewLine + Environment.NewLine +
                               string.Format(Se.Language.Video.CutVideoSubtitleFileGeneratedX, cutSubtitleFileName);
                }

                await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window!, vm =>
                {
                    vm.Initialize(
                        Se.Language.General.VideoFileGenerated,
                        message,
                        jobItem.OutputVideoFileName,
                        true,
                        true);
                });
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

    /// <summary>
    /// Kills a still running ffmpeg and waits briefly for it to go away (it keeps the output
    /// file open until then). Returns true when there was a running process to kill.
    /// </summary>
    private bool KillFfmpegProcess()
    {
        try
        {
            if (_ffmpegProcess == null || _ffmpegProcess.HasExited)
            {
                return false;
            }

#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416
            _ffmpegProcess.WaitForExit(3000);
            return true;
        }
        catch
        {
            // ignore - it may have exited in between
            return false;
        }
    }

    private void DeletePartialOutputFile()
    {
        if (_jobItemIndex < 0 || _jobItemIndex >= JobItems.Count)
        {
            return;
        }

        var outputFileName = JobItems[_jobItemIndex].OutputVideoFileName;
        if (string.IsNullOrWhiteSpace(outputFileName) || !File.Exists(outputFileName))
        {
            return;
        }

        try
        {
            File.Delete(outputFileName);
        }
        catch
        {
            // ignore
        }
    }

    private void InitAndStartJobItem(int index)
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _jobItemIndex = index;
        var jobItem = JobItems[index];
        var mediaInfo = FfmpegMediaInfo.Parse(jobItem.InputVideoFileName);

        // ffmpeg's "frame=" counts OUTPUT frames, so measuring it against the whole input made
        // keeping one minute of sixty finish at ~2 %. Scale the frame total to what is kept.
        // TotalSeconds stays the full input duration - the subtitle cutter needs it.
        var inputFrames = mediaInfo.GetTotalFrames();
        jobItem.TotalFrames = inputFrames > 0
            ? Math.Max(1, (long)Math.Round(inputFrames * GetKeptFraction(mediaInfo.Duration.TotalSeconds)))
            : 0;
        jobItem.TotalSeconds = mediaInfo.Duration.TotalSeconds;
        _generateTransitions.FrameRate = (double)mediaInfo.FramesRate;
        _generateTransitions.InputDurationSeconds = mediaInfo.Duration.TotalSeconds;
        jobItem.Width = mediaInfo.Dimension.Width;
        jobItem.Height = mediaInfo.Dimension.Height;
        jobItem.UseTargetFileSize = false;
        jobItem.Status = Se.Language.General.Generating;

        var result = RunEncoding(jobItem, mediaInfo);
        if (result)
        {
            _timerGenerate.Start();
        }
    }

    /// <summary>
    /// The share (0-1] of the input that ends up in the output: the summed segment durations for
    /// "merge", the input minus the union of the segments for "remove". 1 when the input
    /// duration is unknown.
    /// </summary>
    private double GetKeptFraction(double totalSeconds)
    {
        if (totalSeconds <= 0)
        {
            return 1;
        }

        double keptSeconds;
        if (_generateCutType == CutType.MergeSegments)
        {
            // Every segment is encoded, overlapping or not, so this is a plain sum.
            keptSeconds = _generateSegments.Sum(s => Math.Max(0, Math.Min(s.EndTime.TotalSeconds, totalSeconds) - Math.Max(0, s.StartTime.TotalSeconds)));
        }
        else
        {
            // Union of the removed ranges - _generateSegments is sorted by start time, and
            // overlapping segments must not be subtracted twice.
            var removedSeconds = 0d;
            var position = 0d;
            foreach (var segment in _generateSegments)
            {
                var start = Math.Max(position, segment.StartTime.TotalSeconds);
                var end = Math.Min(totalSeconds, segment.EndTime.TotalSeconds);
                if (end > start)
                {
                    removedSeconds += end - start;
                    position = end;
                }
            }

            keptSeconds = totalSeconds - removedSeconds;
        }

        // Every transition overlaps the two parts it joins.
        if (_generateTransitions.UsesPlan)
        {
            var plan = CutVideoTransitionPlan.Create(GetGenerateRanges(), _generateTransitions);
            keptSeconds -= Math.Max(0, plan.Ranges.Count - 1) * plan.TransitionSeconds;
        }

        // Merge can legitimately exceed the input (overlapping segments are encoded twice), so
        // only the lower bound is clamped hard; above 1 the frame total simply grows with it.
        return Math.Max(0.001, keptSeconds / totalSeconds);
    }

    private bool RunEncoding(BurnInJobItem jobItem, FfmpegMediaInfo mediaInfo)
    {
        string arguments;

        var outputIsAudio = jobItem.OutputVideoFileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                            jobItem.OutputVideoFileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
        var (hasVideo, hasAudio) = GetStreams(mediaInfo, outputIsAudio);

        if (_generateCutType == CutType.MergeSegments)
        {
            arguments = FfmpegGenerator.GetMergeSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, _generateSegments, hasVideo, hasAudio, _generateTransitions);
        }
        else
        {
            arguments = FfmpegGenerator.GetRemoveSegmentsParameters(jobItem.InputVideoFileName, jobItem.OutputVideoFileName, _generateSegments, hasVideo, hasAudio, _generateTransitions);
        }

        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private (bool HasVideo, bool HasAudio) GetStreams(FfmpegMediaInfo mediaInfo, bool outputIsAudio)
    {
        var inputIsAudioByExtension = Utilities.AudioFileExtensions.Contains(Path.GetExtension(_inputVideoFileName).ToLowerInvariant());

        // Ask the file which streams it has. Going by an .mp3/.wav extension alone sent
        // .flac/.m4a/.ogg/.opus/.mka down the video branch, where ffmpeg fails on [0:v], and a
        // video without an audio track failed on [0:a] ("Stream specifier ':a' matches no
        // streams"). Cover art is a "Video" stream too ("attached pic") - it is not a video.
        var hasVideo = mediaInfo.Tracks.Any(t => t.TrackType == FfmpegTrackType.Video &&
                                                 !t.TrackInfo.Contains("attached pic", StringComparison.OrdinalIgnoreCase)) &&
                       !inputIsAudioByExtension;
        var hasAudio = mediaInfo.Tracks.Any(t => t.TrackType == FfmpegTrackType.Audio);

        if (!hasVideo && !hasAudio)
        {
            // No usable media info (ffmpeg could not be run or parsed) - go by the extension, as before.
            hasVideo = !inputIsAudioByExtension;
            hasAudio = true;
        }

        if (outputIsAudio)
        {
            hasVideo = false;
        }

        return (hasVideo, hasAudio);
    }

    private List<(double? Start, double? End)> GetGenerateRanges()
    {
        return _generateCutType == CutType.MergeSegments
            ? FfmpegGenerator.GetMergeRanges(_generateSegments)
            : FfmpegGenerator.GetRemoveRanges(_generateSegments);
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

            // A zero total (no frame rate known) made this Infinity/NaN on the progress bar.
            var totalFrames = JobItems[_jobItemIndex].TotalFrames;
            ProgressValue = totalFrames > 0
                ? Math.Clamp(_processedFrames * 100.0 / totalFrames, 0, 100)
                : 0;
        }
    }

    /// <summary>
    /// Writes the loaded subtitle re-timed to the cut video's timeline, next to the
    /// output video (same base name). Returns the written file name, or null when the
    /// option is off, there is no subtitle, or writing failed (best-effort - a subtitle
    /// problem must not fail the finished video).
    /// </summary>
    private string? WriteCutSubtitle(string outputVideoFileName, double totalDurationSeconds)
    {
        if (!CutSubtitleToo || _currentSubtitle.Paragraphs.Count == 0)
        {
            return null;
        }

        try
        {
            // The segments the video was actually cut with - not the live rows, which the user
            // may have edited while ffmpeg was running.
            var segments = _generateSegments
                .Select(p => (p.StartTime.TotalSeconds, p.EndTime.TotalSeconds))
                .ToList();

            Subtitle cut;
            if (_generateTransitions.UsesPlan)
            {
                // The video was cut from the plan's ranges (whole frames, joins overlapping by
                // the transition) - re-time the subtitle from exactly the same ones.
                var plan = CutVideoTransitionPlan.Create(GetGenerateRanges(), _generateTransitions);
                var lastParagraphEnd = _currentSubtitle.Paragraphs.Max(p => p.EndTime.TotalSeconds);
                var kept = plan.Ranges
                    .Select(r => (r.Start, r.End ?? Math.Max(totalDurationSeconds, lastParagraphEnd)))
                    .ToList();
                cut = SubtitleSegmentCutter.KeepSegments(_currentSubtitle, kept, plan.TransitionSeconds);
            }
            else
            {
                cut = _generateCutType == CutType.MergeSegments
                    ? SubtitleSegmentCutter.KeepSegments(_currentSubtitle, segments)
                    : SubtitleSegmentCutter.RemoveSegments(_currentSubtitle, segments, totalDurationSeconds);
            }

            SubtitleFormat format = _subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat }
                ? new AdvancedSubStationAlpha()
                : new SubRip();

            // Never write over an existing file: with movie.mkv + movie.srt and an output named
            // movie.mp4 this silently replaced the user's original movie.srt with the cut one.
            var fileName = Path.ChangeExtension(outputVideoFileName, format.Extension);
            var i = 2;
            while (File.Exists(fileName))
            {
                fileName = Path.Combine(
                    Path.GetDirectoryName(outputVideoFileName) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(outputVideoFileName)}_{i}{format.Extension}");
                i++;
            }

            File.WriteAllText(fileName, format.ToText(cut, string.Empty));
            return fileName;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to write cut subtitle");
            return null;
        }
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

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = SelectedVideoExtension;
        var suffix = Se.Settings.Video.BurnIn.BurnInSuffix;

        // Decide the folder once - the collision loop below used to combine with
        // BurnIn.OutputFolder unconditionally, so with the default (empty, unused) output
        // folder the "_2" fallback became a bare relative name resolved against the process
        // working directory instead of the video's folder.
        var useOutputFolder = Se.Settings.Video.BurnIn.UseOutputFolder &&
                              !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder) &&
                              Directory.Exists(Se.Settings.Video.BurnIn.OutputFolder);
        var outputFolder = useOutputFolder
            ? Se.Settings.Video.BurnIn.OutputFolder
            : Path.GetDirectoryName(videoFileName) ?? Path.GetTempPath();

        var fileName = Path.Combine(outputFolder, nameNoExt + suffix + ext);

        var i = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(outputFolder, $"{nameNoExt}{suffix}_{i}{ext}");
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
        var selectedSegments = SegmentGrid.SelectedItems?.Cast<SubtitleLineViewModel>().ToList() ?? new List<SubtitleLineViewModel>();
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
        outputVideoFileName = await _fileHelper.PickSaveFile(Window!, SelectedVideoExtension, outputVideoFileName, Se.Language.General.SaveVideoAsVideoTitle);
        if (string.IsNullOrEmpty(outputVideoFileName))
        {
            return;
        }

        // Refuse the input as output, as the re-encode and remux dialogs do. ffmpeg refuses it
        // too ("Output same as Input"), but the input is still there afterwards - which used to
        // count as "video file generated".
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

        // One snapshot for the whole run - see _generateSegments.
        _generateSegments = Segments
            .OrderBy(s => s.StartTime.TotalMilliseconds)
            .Select(s => new SubtitleLineViewModel(s))
            .ToList();
        _generateCutType = SelectedCutType.CutType;
        _generateTransitions = MakeTransitionOptions();

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
        CutSubtitleToo = settings.CutAlsoCutSubtitle;
        TransitionEnabled = settings.CutTransitionEnabled;
        SelectedTransition = Transitions.FirstOrDefault(t => t.Code == settings.CutTransition) ?? Transitions[0];
        TransitionDuration = settings.CutTransitionDuration;
        FadeInEnabled = settings.CutFadeIn;
        FadeInDuration = settings.CutFadeInDuration;
        FadeOutEnabled = settings.CutFadeOut;
        FadeOutDuration = settings.CutFadeOutDuration;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video;
        settings.CutType = SelectedCutType.CutType.ToString();
        settings.CutDefaultVideoExtension = SelectedVideoExtension;
        settings.CutAlsoCutSubtitle = CutSubtitleToo;
        settings.CutTransitionEnabled = TransitionEnabled;
        settings.CutTransition = SelectedTransition.Code;
        settings.CutTransitionDuration = TransitionDuration ?? settings.CutTransitionDuration;
        settings.CutFadeIn = FadeInEnabled;
        settings.CutFadeInDuration = FadeInDuration ?? settings.CutFadeInDuration;
        settings.CutFadeOut = FadeOutEnabled;
        settings.CutFadeOutDuration = FadeOutDuration ?? settings.CutFadeOutDuration;
        Se.SaveSettings();
    }

    /// <summary>
    /// The transition and the fades from/to black as set in the window (frame rate and input
    /// duration are filled in when the input is probed). The fades are independent of the
    /// transition - they only touch the start and the end of the output.
    /// </summary>
    private CutVideoTransitionOptions MakeTransitionOptions()
    {
        return new CutVideoTransitionOptions
        {
            Transition = TransitionEnabled ? SelectedTransition.Code : string.Empty,
            TransitionSeconds = TransitionEnabled ? TransitionDuration ?? 0 : 0,
            FadeInSeconds = FadeInEnabled ? FadeInDuration ?? 0 : 0,
            FadeOutSeconds = FadeOutEnabled ? FadeOutDuration ?? 0 : 0,
        };
    }

    partial void OnIsGeneratingChanged(bool value) => UpdatePreviewState();
    partial void OnIsPreviewingChanged(bool value) => UpdatePreviewState();
    partial void OnTransitionEnabledChanged(bool value) => UpdatePreviewState();

    private void UpdatePreviewState()
    {
        IsPreviewTransitionEnabled = TransitionEnabled && !IsGenerating && !IsPreviewing;
        IsStatusTextVisible = IsGenerating || IsPreviewing;
    }

    partial void OnTransitionDurationChanged(double? value)
    {
        TransitionInfo = string.Format(Se.Language.Video.CutVideoTransitionInfoX, (value ?? 0).ToString("0.0##", CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Renders the join nearest the selected segment (or the play head) with the chosen transition
    /// into a short temporary clip and plays it.
    /// </summary>
    [RelayCommand]
    private async Task PreviewTransition()
    {
        if (IsPreviewing || IsGenerating || Window == null)
        {
            return;
        }

        var segments = Segments
            .OrderBy(s => s.StartTime.TotalMilliseconds)
            .Select(s => new SubtitleLineViewModel(s))
            .ToList();
        var cutType = SelectedCutType.CutType;
        var ranges = cutType == CutType.MergeSegments
            ? FfmpegGenerator.GetMergeRanges(segments)
            : FfmpegGenerator.GetRemoveRanges(segments);

        IsPreviewing = true;
        ProgressText = Se.Language.General.Generating;
        string? previewFileName = null;
        try
        {
            var inputFileName = _inputVideoFileName;
            var mediaInfo = await Task.Run(() => FfmpegMediaInfo.Parse(inputFileName));
            var options = MakeTransitionOptions();
            options.FadeInSeconds = 0;
            options.FadeOutSeconds = 0;
            options.FrameRate = (double)mediaInfo.FramesRate;
            options.InputDurationSeconds = mediaInfo.Duration.TotalSeconds;

            var plan = CutVideoTransitionPlan.Create(ranges, options);
            if (plan.Ranges.Count < 2)
            {
                ProgressText = string.Empty;
                await MessageBox.Show(Window, Se.Language.Video.CutVideoPreviewTransition, Se.Language.Video.CutVideoPreviewNeedsJoin, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // The join nearest what the user looks at: for "merge" a join is where a selected
            // segment ends, for "cut" where the cut-out segment starts.
            var reference = VideoPlayer.Position;
            if (SelectedSegment is { } selected)
            {
                reference = cutType == CutType.MergeSegments ? selected.EndTime.TotalSeconds : selected.StartTime.TotalSeconds;
            }

            var joinIndex = 0;
            for (var i = 1; i < plan.Ranges.Count - 1; i++)
            {
                if (Math.Abs(plan.Ranges[i].End!.Value - reference) < Math.Abs(plan.Ranges[joinIndex].End!.Value - reference))
                {
                    joinIndex = i;
                }
            }

            var (hasVideo, hasAudio) = GetStreams(mediaInfo, outputIsAudio: false);
            previewFileName = Path.Combine(Path.GetTempPath(), "se_cut_preview_" + Guid.NewGuid() + (hasVideo ? ".mp4" : ".wav"));
            options.TransitionSeconds = plan.TransitionSeconds;
            var arguments = FfmpegGenerator.GetCutTransitionPreviewParameters(
                inputFileName,
                previewFileName,
                plan.Ranges[joinIndex],
                plan.Ranges[joinIndex + 1],
                2,
                hasVideo,
                hasAudio,
                options);

            var log = new StringBuilder();
            var process = FfmpegGenerator.GetProcess(arguments, (_, e) =>
            {
                lock (log)
                {
                    log.AppendLine(e.Data);
                }
            });
#pragma warning disable CA1416 // Validate platform compatibility
            process.Start();
#pragma warning restore CA1416 // Validate platform compatibility
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            ProgressText = string.Empty;
            if (_isClosing)
            {
                return;
            }

            if (process.ExitCode != 0 || !File.Exists(previewFileName))
            {
                SeLogger.Error("Cut video transition preview failed: " + arguments + Environment.NewLine + log);
                await MessageBox.Show(Window, Se.Language.General.Error, "Unable to generate preview" + Environment.NewLine + Environment.NewLine + arguments, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            VideoPlayer.VideoPlayer.Pause();
            var fileName = previewFileName;
            await _windowService.ShowDialogAsync<CutVideoPreviewWindow, CutVideoPreviewViewModel>(Window, vm => vm.Initialize(fileName));
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Cut video transition preview failed");
            ProgressText = string.Empty;
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsPreviewing = false;
            ProgressText = string.Empty;
            if (previewFileName != null)
            {
                try
                {
                    File.Delete(previewFileName);
                }
                catch
                {
                    // ignore - the player may still hold it; it is in the temp folder
                }
            }
        }
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
            else if (UiUtil.IsHelp(keyEventArgs))
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

            // Focus sits on the TableView row container (the DataGrid took focus itself),
            // so check for focus anywhere inside the grid.
            if (SegmentGrid.IsKeyboardFocusWithin)
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

            var rc = _shortcutManager.CheckShortcuts(keyEventArgs, ShortcutCategory.General.ToString().ToLowerInvariant());
            if (rc != null)
            {
                keyEventArgs.Handled = true;
                rc.Execute(null);
                return;
            }

            // The main window's default vertical zoom binding is Shift+Add/Subtract, which only the
            // numeric keypad produces. Let the main-row plus and minus keys (OemPlus/OemMinus on
            // every layout, a laptop or a Spanish keyboard has no other) zoom too, as the sync
            // dialogs do (#14419 comment).
            if (keyEventArgs.Key == Key.OemPlus && keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                keyEventArgs.Handled = true;
                WaveformVerticalZoomIn();
            }
            else if (keyEventArgs.Key == Key.OemMinus && keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                keyEventArgs.Handled = true;
                WaveformVerticalZoomOut();
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
        // The subtitle files handed to ffmpeg live as long as the window does - nothing else
        // removes them, and they used to pile up in the temp folder run after run (#13332).
        _tempSubtitleFiles.Delete();

        _isClosing = true;
        _positionTimer.Stop();
        _timerGenerate.StopAndDispose(TimerGenerateElapsed);
        VideoPlayer.CloseAndDisposePlayer();

        // Closing with the title-bar X while generating never goes through Cancel, so ffmpeg was
        // left encoding in the background with nothing to stop it. Kill it, and drop the partial
        // output it leaves.
        if (KillFfmpegProcess())
        {
            DeletePartialOutputFile();
        }

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
            if (SegmentGrid.SelectedItem is { } selectedItem)
            {
                SegmentGrid.ScrollIntoView(selectedItem);
            }
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
        IsSetStartEnabled = SegmentGrid.SelectedItems?.Count == 1;
        IsSetEndEnabled = SegmentGrid.SelectedItems?.Count == 1;
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
                    vp.VideoPlayer.Pause();
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
                    vp.VideoPlayer.Pause();
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
                    vp.VideoPlayer.Pause();
                    vp.Position = e.Seconds;
                    AudioVisualizerCenterOnPositionIfNeeded(e.Seconds);
                    break;
                case WaveformSingleClickActionType.SetVideopositionAndPauseAndCenter:
                    vp.VideoPlayer.Pause();
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
                    vp.VideoPlayer.Pause();
                    break;
                case WaveformDoubleClickActionType.Play:
                    vp.VideoPlayer.Play();
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
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.WaveformVerticalZoomInCommand),
            mainShortCuts.FirstOrDefault(p => p.Action == mainVm.WaveformVerticalZoomOutCommand),
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

        if (action == mainVm.WaveformVerticalZoomInCommand)
        {
            return WaveformVerticalZoomInCommand;
        }

        if (action == mainVm.WaveformVerticalZoomOutCommand)
        {
            return WaveformVerticalZoomOutCommand;
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
        var gap = Se.Settings.General.MinimumBetweenLines.GetMilliseconds() / 1000.0;
        if (videoPositionSeconds < s.StartTime.TotalSeconds + gap)
        {
            return;
        }

        s.EndTime = TimeSpan.FromSeconds(videoPositionSeconds);

        SelectAndScrollToRow(idx + 1);

        _updateAudioVisualizer = true;
    }

    /// <summary>
    /// Mirrors the main window's waveform vertical zoom (Shift +/-): scales the waveform's
    /// amplitude in place, so zooming in for readability does not shrink the video (#14419 comment).
    /// </summary>
    [RelayCommand]
    private void WaveformVerticalZoomIn()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        AudioVisualizer.VerticalZoomFactor = Math.Max(Math.Min(AudioVisualizer.VerticalZoomFactor - 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
    }

    [RelayCommand]
    private void WaveformVerticalZoomOut()
    {
        if (AudioVisualizer == null)
        {
            return;
        }

        AudioVisualizer.VerticalZoomFactor = Math.Max(Math.Min(AudioVisualizer.VerticalZoomFactor + 0.1, AudioVisualizer.MaxZoomFactor), AudioVisualizer.MinZoomFactor);
    }

    [RelayCommand]
    private void Play()
    {
        VideoPlayer.VideoPlayer.Play();
    }

    [RelayCommand]
    private void Pause()
    {
        VideoPlayer.VideoPlayer.Pause();
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
        vp.VideoPlayer.Play();
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        VideoPlayer.TogglePlayPause();
    }
}