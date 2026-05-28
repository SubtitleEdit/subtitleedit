using Avalonia.Controls;
using Avalonia.Input;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

/// <summary>
/// Add/remove embedded subtitles for MP4-family containers (.mp4 / .m4v / .mov).
/// Uses ffmpeg to copy video/audio streams and rewrite the subtitle track set as mov_text.
/// </summary>
public partial class EmbeddedSubtitlesEditMp4ViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    public bool HasVideoFileName => !string.IsNullOrEmpty(VideoFileName);
    public bool CanGenerate => HasVideoFileName && !IsGenerating;
    [ObservableProperty] private ObservableCollection<EmbeddedTrack> _tracks;
    [ObservableProperty] private EmbeddedTrack? _selectedTrack;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid TracksGrid { get; internal set; }

    private readonly StringBuilder _log;
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private SubtitleFormat? _subtitleFormat;
    private Subtitle _currentSubtitle;
    private FfmpegMediaInfo2? _mediaInfo;
    private List<EmbeddedTrack> _originalTracks;
    private long _totalFrames;
    private string _outputFileName;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);

    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    public EmbeddedSubtitlesEditMp4ViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        Tracks = new ObservableCollection<EmbeddedTrack>();
        VideoFileName = string.Empty;
        ProgressText = string.Empty;
        TracksGrid = new DataGrid();

        _log = new StringBuilder();
        _timerGenerate = new Timer { Interval = 100 };
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _currentSubtitle = new Subtitle();
        _originalTracks = new List<EmbeddedTrack>();
        _outputFileName = string.Empty;
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat subtitleFormat, FfmpegMediaInfo2? mediaInfo)
    {
        VideoFileName = videoFileName;
        _currentSubtitle = subtitle;
        _subtitleFormat = subtitleFormat;
        _mediaInfo = mediaInfo;
    }

    partial void OnVideoFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(HasVideoFileName));
        OnPropertyChanged(nameof(CanGenerate));
    }

    partial void OnIsGeneratingChanged(bool value) => OnPropertyChanged(nameof(CanGenerate));

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
            if (_totalFrames > 0 && _processedFrames > 0)
            {
                var percentage = (int)Math.Round((double)_processedFrames / _totalFrames * 100.0, MidpointRounding.AwayFromZero);
                percentage = Math.Clamp(percentage, 0, 100);

                var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
                var msPerFrame = (float)durationMs / _processedFrames;
                var estimatedTotalMs = msPerFrame * _totalFrames;
                var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

                ProgressText = $"Generating video... {percentage}%     {estimatedLeft}";
            }
            else
            {
                ProgressText = "Generating video...";
            }

            return;
        }

        _timerGenerate.Stop();
        ProgressValue = 100;
        ProgressText = string.Empty;
        Se.LogError(_log.ToString());

        if (!File.Exists(_outputFileName))
        {
            SeLogger.Error("Output video file not found: " + _outputFileName + Environment.NewLine +
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
                    "Output video file not generated: " + _outputFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsGenerating = false;
                ProgressValue = 0;
            });

            return;
        }

        Dispatcher.UIThread.Invoke(async () =>
        {
            ProgressValue = 0;
            IsGenerating = false;
            await _folderHelper.OpenFolderWithFileSelected(Window!, _outputFileName);
        });
    }

    private bool RunEncoding()
    {
        if (string.IsNullOrEmpty(VideoFileName) || !File.Exists(VideoFileName))
        {
            return false;
        }

        var arguments = FfmpegGenerator.AlterEmbeddedTracksMp4(Tracks.ToList(), _originalTracks, VideoFileName, _outputFileName);
        _log.AppendLine($"FFmpeg command: {arguments}");

        _startTicks = DateTime.UtcNow.Ticks;
        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416
        _ffmpegProcess.Start();
#pragma warning restore CA1416
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();
        _timerGenerate.Start();

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
            if (_totalFrames > 0)
            {
                ProgressValue = (double)_processedFrames * 100.0 / _totalFrames;
            }
        }
    }

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = Path.GetExtension(videoFileName);
        if (string.IsNullOrEmpty(ext))
        {
            ext = ".mp4";
        }

        var suffix = Se.Settings.Video.BurnIn.BurnInSuffix;
        var dir = Se.Settings.Video.BurnIn.UseOutputFolder &&
                  !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder) &&
                  Directory.Exists(Se.Settings.Video.BurnIn.OutputFolder)
            ? Se.Settings.Video.BurnIn.OutputFolder
            : Path.GetDirectoryName(videoFileName) ?? string.Empty;

        var fileName = Path.Combine(dir, nameNoExt + suffix + ext);
        var i = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(dir, $"{nameNoExt}{suffix}_{i}{ext}");
            i++;
        }

        return fileName;
    }

    [RelayCommand]
    private async Task Add()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.OpenSubtitleFileTitle,
            Se.Language.General.SubtitleFiles,
            "*.srt;*.vtt;*.ass;*.ssa",
            Se.Language.General.AllFiles,
            "*.*");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null || subtitle.Paragraphs.Count == 0)
        {
            await MessageBox.Show(
                Window,
                "No subtitles found",
                "The selected subtitle file does not contain any subtitles.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var srtFileName = EnsureSrtForMovText(fileName, subtitle);
        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        var isoLanguage = Iso639Dash2LanguageCode.List.FirstOrDefault(l => l.TwoLetterCode.Equals(language, StringComparison.OrdinalIgnoreCase));

        Tracks.Add(new EmbeddedTrack
        {
            Format = "mov_text",
            LanguageOrTitle = isoLanguage?.ThreeLetterCode ?? language,
            Name = isoLanguage?.EnglishName ?? string.Empty,
            FileName = srtFileName,
            New = true,
        });
    }

    [RelayCommand]
    private void AddCurrent()
    {
        if (Window == null || _currentSubtitle == null || _currentSubtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var srtFormat = new SubRip();
        var tempFileName = Path.Combine(Path.GetTempPath(), "EmbeddedSubtitleEditMp4_" + Guid.NewGuid() + srtFormat.Extension);
        File.WriteAllText(tempFileName, srtFormat.ToText(_currentSubtitle, string.Empty));

        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(_currentSubtitle);
        var isoLanguage = Iso639Dash2LanguageCode.List.FirstOrDefault(l => l.TwoLetterCode.Equals(language, StringComparison.OrdinalIgnoreCase));

        Tracks.Add(new EmbeddedTrack
        {
            Format = "mov_text",
            LanguageOrTitle = isoLanguage?.ThreeLetterCode ?? language,
            Name = isoLanguage?.EnglishName ?? string.Empty,
            FileName = tempFileName,
            New = true,
        });
    }

    // mov_text in MP4 is plain text; ffmpeg can mux it from SRT directly. ASS/SSA/VTT
    // get pre-converted to SRT here so styling tags don't end up as literal text on
    // playback. Returns either the original file path (already SRT) or a new temp .srt.
    private static string EnsureSrtForMovText(string fileName, Subtitle subtitle)
    {
        if (subtitle.OriginalFormat is SubRip)
        {
            return fileName;
        }

        var srtFormat = new SubRip();
        var tempFileName = Path.Combine(Path.GetTempPath(), "EmbeddedSubtitleEditMp4_" + Guid.NewGuid() + srtFormat.Extension);
        File.WriteAllText(tempFileName, srtFormat.ToText(subtitle, string.Empty));
        return tempFileName;
    }

    [RelayCommand]
    private void Delete()
    {
        if (SelectedTrack != null)
        {
            SelectedTrack.Deleted = !SelectedTrack.Deleted;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        foreach (var track in Tracks)
        {
            track.Deleted = true;
        }
    }

    [RelayCommand]
    private async Task Edit()
    {
        var selectedTrack = SelectedTrack;
        if (Window == null || selectedTrack == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditEmbeddedTrackWindow, EditEmbeddedTrackViewModel>(Window, vm =>
        {
            vm.Initialize(selectedTrack);
        });

        if (result != null && result.OkPressed)
        {
            selectedTrack.Name = result.Name;
            selectedTrack.LanguageOrTitle = result.TitleOrlanguage;
            selectedTrack.Forced = result.IsForced;

            if (result.IsDefault)
            {
                foreach (var track in Tracks)
                {
                    if (track != selectedTrack)
                    {
                        track.Default = false;
                    }
                }
            }

            selectedTrack.Default = result.IsDefault;
        }
    }

    [RelayCommand]
    private async Task BrowseVideoFile()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.OpenVideoFileTitle,
            "MP4 files",
            "*.mp4;*.m4v;*.mov");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        VideoFileName = fileName;
        _mediaInfo = FfmpegMediaInfo2.Parse(fileName);

        Tracks.Clear();
        _originalTracks.Clear();
        var tracks = FindMp4SubtitleTracks(_mediaInfo);
        foreach (var track in tracks)
        {
            Tracks.Add(track);
            _originalTracks.Add(new EmbeddedTrack(track));
        }

        SelectAndScrollToRow(0);
    }

    [RelayCommand]
    private async Task Generate()
    {
        // Stripping all subtitles is a valid operation, so the only real "nothing to do"
        // case is when the source had no subtitles AND the user added none.
        if (Tracks.Count == 0 && _originalTracks.Count == 0)
        {
            await MessageBox.Show(
                Window!,
                "No tracks added",
                "Add one or more subtitle tracks, or load a video that already has embedded subtitles.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var outputVideoFileName = MakeOutputFileName(VideoFileName);
        outputVideoFileName = await _fileHelper.PickSaveFile(
            Window!,
            Path.GetExtension(VideoFileName).TrimStart('.'),
            outputVideoFileName,
            Se.Language.Video.SaveVideoAsTitle);
        if (string.IsNullOrEmpty(outputVideoFileName))
        {
            return;
        }

        _outputFileName = outputVideoFileName;
        _doAbort = false;
        _log.Clear();
        IsGenerating = true;
        _processedFrames = 0;
        ProgressValue = 0;
        _totalFrames = _mediaInfo?.Duration != null
            ? (long)Math.Round((double)_mediaInfo.FramesRate * _mediaInfo.Duration.TotalSeconds)
            : 0;

        IsGenerating = RunEncoding();
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
            IsGenerating = false;
            return;
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/embedded-subtitles");
        }
    }

    internal void OnClosing()
    {
        UiUtil.SaveWindowPosition(Window);
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
        if (string.IsNullOrEmpty(VideoFileName) || !File.Exists(VideoFileName))
        {
            return;
        }

        Task.Run(() =>
        {
            if (_mediaInfo == null)
            {
                _mediaInfo = FfmpegMediaInfo2.Parse(VideoFileName);
            }

            var tracks = FindMp4SubtitleTracks(_mediaInfo);
            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (var track in tracks)
                {
                    Tracks.Add(track);
                    _originalTracks.Add(new EmbeddedTrack(track));
                }

                SelectAndScrollToRow(0);
            });
        });
    }

    private static List<EmbeddedTrack> FindMp4SubtitleTracks(FfmpegMediaInfo2? mediaInfo)
    {
        var list = new List<EmbeddedTrack>();
        if (mediaInfo == null)
        {
            return list;
        }

        var subtitleIndex = 0;
        foreach (var track in mediaInfo.Tracks.Where(t => t.TrackType == FfmpegTrackType.Subtitle))
        {
            list.Add(new EmbeddedTrack
            {
                Number = subtitleIndex,
                Format = ParseCodecFromInfo(track.TrackInfo),
                // Language is lost in FfmpegMediaInfo2.ParseLog (it splits on ": " and drops the
                // "Stream #0:2(eng)" prefix that holds the language tag). User can fill it in via
                // the Edit dialog — ffmpeg keeps the original language on copied streams anyway.
                LanguageOrTitle = string.Empty,
                Name = string.Empty,
                FileName = string.Empty,
                FfmpegTrackInfo = track,
            });
            subtitleIndex++;
        }

        return list;
    }

    private static string ParseCodecFromInfo(string trackInfo)
    {
        // FfmpegMediaInfo2 strips the "Subtitle: " prefix, so trackInfo starts with the codec
        // name followed by parenthesized dispositions, e.g. "mov_text (default)". Take the
        // first whitespace-separated token.
        var firstSpace = trackInfo.IndexOf(' ');
        return firstSpace > 0 ? trackInfo.Substring(0, firstSpace) : trackInfo;
    }

    private void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Tracks.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            TracksGrid.SelectedIndex = index;
            TracksGrid.ScrollIntoView(TracksGrid.SelectedItem, null);
        }, DispatcherPriority.Background);
    }

    internal void OnTracksGridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            Delete();
            e.Handled = true;
        }
        else if (e.Key == Key.Insert)
        {
            _ = Add();
            e.Handled = true;
        }
    }
}
