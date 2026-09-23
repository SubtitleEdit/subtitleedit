using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Video.AddAudioToVideo;

public partial class AddAudioToVideoViewModel : ObservableObject
{
    private const string GeneratePtsForVideoCopy = "-fflags +genpts ";
    private static readonly Regex FfmpegTimeRegex = new(@"time=\s*(\d+):(\d+):(\d+(?:\.\d+)?)", RegexOptions.Compiled);

    [ObservableProperty] private string _inputMediaFileName = string.Empty;
    [ObservableProperty] private string _mediaInfoText = string.Empty;
    [ObservableProperty] private string _audioFileName = string.Empty;
    [ObservableProperty] private string _audioInfoText = string.Empty;
    [ObservableProperty] private string _outputFileName = string.Empty;
    [ObservableProperty] private bool _isVideoInput;
    [ObservableProperty] private bool _hasOriginalAudio;
    [ObservableProperty] private bool _isDuckingEnabled = true;
    [ObservableProperty] private int _duckingVolumePercent = 15;
    [ObservableProperty] private int _addedAudioVolumePercent = 100;
    [ObservableProperty] private bool _replaceOriginalAudio;
    [ObservableProperty] private bool _canReplaceOriginalAudio;
    [ObservableProperty] private bool _isDuckingControlsEnabled = true;
    [ObservableProperty] private bool _isDuckingVolumeEnabled = true;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isNotGenerating = true;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private bool _canGenerate;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;

    private double _mediaDurationSeconds;
    private Process? _ffmpegProcess;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly StringBuilder _log = new();

    public AddAudioToVideoViewModel(IFileHelper fileHelper, IFolderHelper folderHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _windowService = windowService;
    }

    public void Initialize(string? currentVideoFileName)
    {
        if (!string.IsNullOrWhiteSpace(currentVideoFileName) && File.Exists(currentVideoFileName))
        {
            SetInputMedia(currentVideoFileName);
        }
        else
        {
            UpdateCanGenerate();
        }
    }

    partial void OnInputMediaFileNameChanged(string value)
    {
        UpdateCanGenerate();
    }

    partial void OnAudioFileNameChanged(string value)
    {
        UpdateCanGenerate();
    }

    partial void OnOutputFileNameChanged(string value)
    {
        UpdateCanGenerate();
    }

    partial void OnReplaceOriginalAudioChanged(bool value)
    {
        UpdateDuckingControlsState();
    }

    partial void OnIsDuckingEnabledChanged(bool value)
    {
        UpdateDuckingControlsState();
    }

    private void UpdateDuckingControlsState()
    {
        IsDuckingControlsEnabled = !ReplaceOriginalAudio && HasOriginalAudio;
        IsDuckingVolumeEnabled = IsDuckingControlsEnabled && IsDuckingEnabled;
    }

    public void SetInputMedia(string fileName)
    {
        InputMediaFileName = fileName;
        IsCompleted = false;

        Task.Run(() =>
        {
            try
            {
                var info = FfmpegMediaInfo2.Parse(fileName);
                var duration = info.Duration?.TotalSeconds ?? 0;
                var hasVideo = info.Tracks.Any(t => t.TrackType == FfmpegTrackType.Video);
                var hasAudio = info.Tracks.Any(t => t.TrackType == FfmpegTrackType.Audio);

                Dispatcher.UIThread.Post(() =>
                {
                    _mediaDurationSeconds = duration;
                    IsVideoInput = hasVideo;
                    HasOriginalAudio = hasAudio;
                    CanReplaceOriginalAudio = hasVideo && hasAudio;
                    if (!CanReplaceOriginalAudio)
                    {
                        ReplaceOriginalAudio = false;
                    }
                    UpdateDuckingControlsState();

                    var durationSpan = TimeSpan.FromSeconds(duration);
                    var durationString = durationSpan.TotalHours >= 1
                        ? durationSpan.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                        : durationSpan.ToString(@"mm\:ss", CultureInfo.InvariantCulture);

                    var fileLength = new FileInfo(fileName).Length;
                    var fileSizeString = Utilities.FormatBytesToDisplayFileSize(fileLength);

                    var typeDescription = hasVideo && hasAudio ? "Video + Audio" :
                                          hasVideo ? "Video only" : "Audio only";

                    MediaInfoText = $"{typeDescription} | {durationString} | {fileSizeString}";

                    if (string.IsNullOrWhiteSpace(OutputFileName) || !File.Exists(OutputFileName))
                    {
                        OutputFileName = GenerateDefaultOutputFileName(InputMediaFileName, hasVideo);
                    }

                    UpdateCanGenerate();
                });
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "AddAudioToVideo: reading media info failed");
                Dispatcher.UIThread.Post(() =>
                {
                    MediaInfoText = string.Empty;
                    UpdateCanGenerate();
                });
            }
        });
    }

    public void SetAudioFile(string fileName)
    {
        AudioFileName = fileName;
        IsCompleted = false;

        Task.Run(() =>
        {
            try
            {
                var info = FfmpegMediaInfo2.Parse(fileName);
                var duration = info.Duration?.TotalSeconds ?? 0;
                var durationSpan = TimeSpan.FromSeconds(duration);
                var durationString = durationSpan.TotalHours >= 1
                    ? durationSpan.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
                    : durationSpan.ToString(@"mm\:ss", CultureInfo.InvariantCulture);

                var fileLength = new FileInfo(fileName).Length;
                var fileSizeString = Utilities.FormatBytesToDisplayFileSize(fileLength);

                Dispatcher.UIThread.Post(() =>
                {
                    AudioInfoText = $"{Path.GetFileName(fileName)} ({durationString} | {fileSizeString})";
                    UpdateCanGenerate();
                });
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "AddAudioToVideo: reading audio info failed");
                Dispatcher.UIThread.Post(() =>
                {
                    AudioInfoText = Path.GetFileName(fileName);
                    UpdateCanGenerate();
                });
            }
        });
    }

    private static string GenerateDefaultOutputFileName(string inputFileName, bool isVideo)
    {
        var dir = Path.GetDirectoryName(inputFileName) ?? string.Empty;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(inputFileName);
        var ext = Path.GetExtension(inputFileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = isVideo ? ".mp4" : ".mp3";
        }

        var suffix = isVideo ? ".with-audio" : ".mixed";
        var candidate = Path.Combine(dir, $"{nameWithoutExt}{suffix}{ext}");
        var counter = 2;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(dir, $"{nameWithoutExt}{suffix}_{counter}{ext}");
            counter++;
        }

        return candidate;
    }

    private void UpdateCanGenerate()
    {
        CanGenerate = !string.IsNullOrWhiteSpace(InputMediaFileName) &&
                      File.Exists(InputMediaFileName) &&
                      !string.IsNullOrWhiteSpace(AudioFileName) &&
                      File.Exists(AudioFileName) &&
                      !string.IsNullOrWhiteSpace(OutputFileName) &&
                      IsNotGenerating;
    }

    [RelayCommand]
    private async Task BrowseInputMedia()
    {
        if (Window == null)
        {
            return;
        }

        var selectedFile = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.VideoFiles,
            "Media files (*.mp4;*.mkv;*.avi;*.webm;*.mov;*.mp3;*.wav;*.m4a;*.flac;*.aac;*.ogg)",
            ".mp4;.mkv;.avi;.webm;.mov;.ts;.m2ts;.mp3;.wav;.m4a;.flac;.aac;.ogg;.opus;.wma",
            Se.Language.General.AllFiles,
            "*.*");

        if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
        {
            SetInputMedia(selectedFile);
        }
    }

    [RelayCommand]
    private async Task BrowseAudio()
    {
        if (Window == null)
        {
            return;
        }

        var selectedFile = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.AudioFiles,
            "Audio files (*.mp3;*.wav;*.m4a;*.flac;*.aac;*.ogg;*.opus;*.wma)",
            ".mp3;.wav;.m4a;.flac;.aac;.ogg;.opus;.wma",
            Se.Language.General.AllFiles,
            "*.*");

        if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
        {
            SetAudioFile(selectedFile);
        }
    }

    [RelayCommand]
    private async Task BrowseOutputFile()
    {
        if (Window == null)
        {
            return;
        }

        var ext = Path.GetExtension(OutputFileName);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = IsVideoInput ? ".mp4" : ".mp3";
        }

        var selected = await _fileHelper.PickSaveFile(
            Window,
            ext,
            OutputFileName,
            Se.Language.Video.AddAudioOutputFile);

        if (!string.IsNullOrWhiteSpace(selected))
        {
            OutputFileName = selected;
            IsCompleted = false;
        }
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (!CanGenerate || Window == null)
        {
            return;
        }

        if (string.Equals(InputMediaFileName, OutputFileName, StringComparison.OrdinalIgnoreCase))
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                "Output file cannot be identical to the input file.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        IsGenerating = true;
        IsNotGenerating = false;
        IsCompleted = false;
        ProgressValue = 0;
        ProgressText = "Processing...";
        UpdateCanGenerate();

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        _log.Clear();

        Stopwatch? stopwatch = null;
        DispatcherTimer? elapsedTimer = null;

        try
        {
            var ffmpegPath = FfmpegHelper.GetFfmpegLocation();
            if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath))
            {
                throw new FileNotFoundException("ffmpeg not found");
            }

            var arguments = BuildFfmpegArguments();
            _log.AppendLine($"Executing: ffmpeg {arguments}");

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            stopwatch = Stopwatch.StartNew();
            elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            elapsedTimer.Tick += (_, _) => UpdateProgressTimeText(stopwatch);
            elapsedTimer.Start();

            _ffmpegProcess = new Process { StartInfo = startInfo };
            _ffmpegProcess.ErrorDataReceived += (s, e) => OnFfmpegDataReceived(s, e, stopwatch);
            _ffmpegProcess.OutputDataReceived += (s, e) => OnFfmpegDataReceived(s, e, stopwatch);

            _ffmpegProcess.Start();
            _ffmpegProcess.BeginErrorReadLine();
            _ffmpegProcess.BeginOutputReadLine();

            await _ffmpegProcess.WaitForExitAsync(cancellationToken);

            elapsedTimer.Stop();
            stopwatch.Stop();
            var totalElapsedStr = FormatTimeSpan(stopwatch.Elapsed);

            if (_ffmpegProcess.ExitCode == 0 && File.Exists(OutputFileName) && new FileInfo(OutputFileName).Length > 0)
            {
                ProgressValue = 100;
                ProgressText = $"{Se.Language.Video.AddAudioCompleted} ({totalElapsedStr})";
                IsCompleted = true;
            }
            else
            {
                var errorMsg = _log.ToString();
                Se.WriteToolsLog($"AddAudioToVideo failed. ExitCode={_ffmpegProcess.ExitCode}. Log: {errorMsg}", true);
                ProgressText = Se.Language.Video.AddAudioFailed;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await MessageBox.Show(
                        Window,
                        Se.Language.General.Error,
                        Se.Language.Video.AddAudioFailed + Environment.NewLine + Environment.NewLine +
                        (errorMsg.Length > 300 ? errorMsg[^300..] : errorMsg),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        catch (OperationCanceledException)
        {
            ProgressText = "Cancelled";
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "AddAudioToVideo: Execution error");
            ProgressText = Se.Language.Video.AddAudioFailed;
            if (Window != null)
            {
                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    ex.Message,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        finally
        {
            elapsedTimer?.Stop();
            stopwatch?.Stop();

            _ffmpegProcess?.Dispose();
            _ffmpegProcess = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            IsGenerating = false;
            IsNotGenerating = true;
            UpdateCanGenerate();
        }
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        return ts.TotalHours >= 1
            ? ts.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
            : ts.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    private void UpdateProgressTimeText(Stopwatch? stopwatch)
    {
        if (!IsGenerating || stopwatch == null)
        {
            return;
        }

        var elapsedStr = FormatTimeSpan(stopwatch.Elapsed);
        string timeInfo;

        if (ProgressValue > 0 && ProgressValue < 100)
        {
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
            var totalEstimatedMs = elapsedMs / (ProgressValue / 100.0);
            var remainingMs = Math.Max(0, totalEstimatedMs - elapsedMs);
            var remainingStr = FormatTimeSpan(TimeSpan.FromMilliseconds(remainingMs));
            timeInfo = $"{elapsedStr} / remaining {remainingStr}";
        }
        else
        {
            timeInfo = elapsedStr;
        }

        ProgressText = $"Processing... {(int)ProgressValue}% ({timeInfo})";
    }

    private string BuildFfmpegArguments()
    {
        var inputMedia = InputMediaFileName;
        var audioToAdd = AudioFileName;
        var output = OutputFileName;

        var addedVolFactor = Math.Clamp(AddedAudioVolumePercent / 100.0, 0.0, 2.0).ToString("0.00", CultureInfo.InvariantCulture);

        if (IsVideoInput)
        {
            var ptsCopy = GeneratePtsForVideoCopy;
            if (ReplaceOriginalAudio || !HasOriginalAudio)
            {
                // Replace audio track or video had no audio
                return $"-nostdin -y {ptsCopy}-i \"{inputMedia}\" -i \"{audioToAdd}\" -filter_complex \"[1:a]volume={addedVolFactor}[aout]\" -map 0:v:0 -map \"[aout]\" -c:v copy -c:a aac -b:a 192k \"{output}\"";
            }

            var origVolFactor = IsDuckingEnabled
                ? Math.Clamp(DuckingVolumePercent / 100.0, 0.0, 2.0).ToString("0.00", CultureInfo.InvariantCulture)
                : "1.00";

            return $"-nostdin -y {ptsCopy}-i \"{inputMedia}\" -i \"{audioToAdd}\" -filter_complex \"[0:a]volume={origVolFactor}[orig];[1:a]volume={addedVolFactor}[added];[orig][added]amix=inputs=2:duration=longest:normalize=0[aout]\" -map 0:v:0 -map \"[aout]\" -c:v copy -c:a aac -b:a 192k \"{output}\"";
        }
        else
        {
            // Audio + Audio mixing
            var origVolFactor = IsDuckingEnabled
                ? Math.Clamp(DuckingVolumePercent / 100.0, 0.0, 2.0).ToString("0.00", CultureInfo.InvariantCulture)
                : "1.00";

            return $"-nostdin -y -i \"{inputMedia}\" -i \"{audioToAdd}\" -filter_complex \"[0:a]volume={origVolFactor}[orig];[1:a]volume={addedVolFactor}[added];[orig][added]amix=inputs=2:duration=longest:normalize=0[aout]\" -map \"[aout]\" \"{output}\"";
        }
    }

    private void OnFfmpegDataReceived(object sender, DataReceivedEventArgs e, Stopwatch? stopwatch)
    {
        if (string.IsNullOrWhiteSpace(e.Data))
        {
            return;
        }

        _log.AppendLine(e.Data);

        var match = FfmpegTimeRegex.Match(e.Data);
        if (match.Success && _mediaDurationSeconds > 0)
        {
            if (int.TryParse(match.Groups[1].Value, out var hours) &&
                int.TryParse(match.Groups[2].Value, out var minutes) &&
                double.TryParse(match.Groups[3].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
            {
                var currentSeconds = (hours * 3600) + (minutes * 60) + seconds;
                var progress = Math.Clamp((currentSeconds / _mediaDurationSeconds) * 100.0, 0.0, 99.0);

                Dispatcher.UIThread.Post(() =>
                {
                    ProgressValue = progress;
                    UpdateProgressTimeText(stopwatch);
                });
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            try
            {
                _ffmpegProcess.Kill(true);
            }
            catch
            {
                // ignore
            }
        }
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        if (Window != null && !string.IsNullOrWhiteSpace(OutputFileName) && File.Exists(OutputFileName))
        {
            await _folderHelper.OpenFolderWithFileSelected(Window, OutputFileName);
        }
    }

    [RelayCommand]
    private void Done()
    {
        OkPressed = true;
        Window?.Close();
    }

    public void OnClosing()
    {
        Cancel();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (IsGenerating)
            {
                Cancel();
            }
            else
            {
                Window?.Close();
            }

            e.Handled = true;
        }
    }
}
