using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

public partial class RemuxVideoViewModel : ObservableObject
{
    private static readonly HashSet<string> AllowedVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".avi", ".webm", ".ts"
    };

    private static readonly HashSet<string> AllowedAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".aac", ".ac3", ".wav", ".mkv", ".mka", ".mp4"
    };

    private static readonly HashSet<string> AllowedSubtitleExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".srt", ".ass", ".ssa", ".vtt", ".sub"
    };

    [ObservableProperty] private string _videoFileName = string.Empty;
    [ObservableProperty] private string _videoFileSize = string.Empty;
    [ObservableProperty] private string _audioFileName = string.Empty;
    [ObservableProperty] private string _audioFileSize = string.Empty;
    [ObservableProperty] private string _subtitleFileName = string.Empty;
    [ObservableProperty] private string _subtitleFileSize = string.Empty;
    [ObservableProperty] private ObservableCollection<AudioTrackOption> _audioFileTracks = new();
    [ObservableProperty] private AudioTrackOption? _selectedAudioFileTrack;
    [ObservableProperty] private bool _hasMultipleAudioTracks;
    [ObservableProperty] private ObservableCollection<string> _outputFormats;
    [ObservableProperty] private string _selectedOutputFormat = ".mp4";
    [ObservableProperty] private string _outputFileName = string.Empty;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isRemuxing;
    [ObservableProperty] private bool _canRemux;
    [ObservableProperty] private bool _isCompleted;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;
    private Process? _ffmpegProcess;
    private readonly StringBuilder _log = new();
    private FfmpegProgressTracker? _progressTracker;
    private bool _suppressAudioPrompt;

    public RemuxVideoViewModel(IFileHelper fileHelper, IFolderHelper folderHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _windowService = windowService;
        OutputFormats = new ObservableCollection<string> { ".mp4", ".mkv" };
        SelectedOutputFormat = OutputFormats[0];
    }

    public void Initialize(string? currentVideoFileName)
    {
        if (!string.IsNullOrWhiteSpace(currentVideoFileName) && File.Exists(currentVideoFileName))
        {
            var ext = Path.GetExtension(currentVideoFileName);
            if (AllowedVideoExtensions.Contains(ext))
            {
                VideoFileName = currentVideoFileName;
                UpdateVideoInfo();

                if (string.Equals(ext, ".mkv", StringComparison.OrdinalIgnoreCase))
                {
                    SelectedOutputFormat = ".mkv";
                }
                else
                {
                    SelectedOutputFormat = ".mp4";
                }

                OutputFileName = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
            }
        }

        UpdateCanRemux();
    }

    private void UpdateVideoInfo()
    {
        if (!string.IsNullOrWhiteSpace(VideoFileName) && File.Exists(VideoFileName))
        {
            try
            {
                var length = new FileInfo(VideoFileName).Length;
                VideoFileSize = Utilities.FormatBytesToDisplayFileSize(length);
            }
            catch
            {
                VideoFileSize = string.Empty;
            }
        }
        else
        {
            VideoFileSize = string.Empty;
        }
    }

    private void UpdateAudioInfo()
    {
        if (!string.IsNullOrWhiteSpace(AudioFileName) && File.Exists(AudioFileName))
        {
            try
            {
                var length = new FileInfo(AudioFileName).Length;
                AudioFileSize = Utilities.FormatBytesToDisplayFileSize(length);
            }
            catch
            {
                AudioFileSize = string.Empty;
            }
        }
        else
        {
            AudioFileSize = string.Empty;
        }
    }

    private void UpdateSubtitleInfo()
    {
        if (!string.IsNullOrWhiteSpace(SubtitleFileName) && File.Exists(SubtitleFileName))
        {
            try
            {
                var length = new FileInfo(SubtitleFileName).Length;
                SubtitleFileSize = Utilities.FormatBytesToDisplayFileSize(length);
            }
            catch
            {
                SubtitleFileSize = string.Empty;
            }
        }
        else
        {
            SubtitleFileSize = string.Empty;
        }
    }

    private void UpdateCanRemux()
    {
        var hasValidSubtitle = string.IsNullOrWhiteSpace(SubtitleFileName) ||
                               (File.Exists(SubtitleFileName) && AllowedSubtitleExtensions.Contains(Path.GetExtension(SubtitleFileName)));

        CanRemux = !string.IsNullOrWhiteSpace(VideoFileName) &&
                   File.Exists(VideoFileName) &&
                   AllowedVideoExtensions.Contains(Path.GetExtension(VideoFileName)) &&
                   !string.IsNullOrWhiteSpace(AudioFileName) &&
                   File.Exists(AudioFileName) &&
                   AllowedAudioExtensions.Contains(Path.GetExtension(AudioFileName)) &&
                   hasValidSubtitle &&
                   !IsRemuxing;
    }

    private async Task<AudioTrackOption?> PromptSelectAudioTrack(string fileName, List<AudioTrackOption> trackOptions, string fileTypeLabel)
    {
        if (Window == null || trackOptions == null || trackOptions.Count <= 1)
        {
            return trackOptions?.FirstOrDefault();
        }

        var dialogVm = await _windowService.ShowDialogAsync<PickAudioTrackWindow, PickAudioTrackViewModel>(Window, vm =>
        {
            var name = Path.GetFileName(fileName);
            vm.Initialize(
                $"Chọn ngôn ngữ / track audio cho {fileTypeLabel}",
                $"Tệp '{name}' có chứa {trackOptions.Count} track audio (Multitrack). Vui lòng chọn ngôn ngữ / track audio bạn muốn remux:",
                trackOptions);
        });

        if (dialogVm != null && dialogVm.OkPressed && dialogVm.SelectedTrack != null)
        {
            return dialogVm.SelectedTrack;
        }

        return trackOptions.FirstOrDefault();
    }

    async partial void OnVideoFileNameChanged(string value)
    {
        UpdateVideoInfo();
        if (string.IsNullOrWhiteSpace(OutputFileName) && !string.IsNullOrWhiteSpace(value))
        {
            OutputFileName = MakeOutputFileName(value, SelectedOutputFormat);
        }
        IsCompleted = false;
        UpdateCanRemux();

        if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
        {
            try
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(value);
                var audioTracks = mediaInfo.Tracks
                    .Where(t => t.TrackType == FfmpegTrackType.Audio)
                    .Select((t, index) => new AudioTrackOption
                    {
                        Index = index,
                        Language = t.Language,
                        Details = t.TrackInfo
                    })
                    .ToList();

                if (audioTracks.Count > 0)
                {
                    AudioTrackOption? selectedTrack = null;
                    if (audioTracks.Count > 1)
                    {
                        selectedTrack = await PromptSelectAudioTrack(value, audioTracks, "Tệp Video");
                    }

                    if (string.IsNullOrWhiteSpace(AudioFileName) || string.Equals(AudioFileName, value, StringComparison.OrdinalIgnoreCase))
                    {
                        _suppressAudioPrompt = true;
                        try
                        {
                            AudioFileName = value;
                            AudioFileTracks = new ObservableCollection<AudioTrackOption>(audioTracks);
                            SelectedAudioFileTrack = selectedTrack ?? audioTracks[0];
                            HasMultipleAudioTracks = audioTracks.Count > 1;
                        }
                        finally
                        {
                            _suppressAudioPrompt = false;
                        }
                    }
                }
            }
            catch
            {
                // Fallback
            }
        }
    }

    async partial void OnAudioFileNameChanged(string value)
    {
        UpdateAudioInfo();
        IsCompleted = false;
        UpdateCanRemux();

        if (_suppressAudioPrompt)
        {
            return;
        }

        AudioFileTracks.Clear();
        SelectedAudioFileTrack = null;
        HasMultipleAudioTracks = false;

        if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
        {
            try
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(value);
                var audioTracks = mediaInfo.Tracks
                    .Where(t => t.TrackType == FfmpegTrackType.Audio)
                    .Select((t, index) => new AudioTrackOption
                    {
                        Index = index,
                        Language = t.Language,
                        Details = t.TrackInfo
                    })
                    .ToList();

                if (audioTracks.Count > 0)
                {
                    AudioFileTracks = new ObservableCollection<AudioTrackOption>(audioTracks);
                    if (audioTracks.Count > 1)
                    {
                        HasMultipleAudioTracks = true;
                        var selected = await PromptSelectAudioTrack(value, audioTracks, "Tệp Audio");
                        SelectedAudioFileTrack = selected ?? audioTracks[0];
                    }
                    else
                    {
                        SelectedAudioFileTrack = audioTracks[0];
                    }
                }
            }
            catch
            {
                // Fallback if media info parsing fails
            }
        }
    }

    partial void OnSubtitleFileNameChanged(string value)
    {
        UpdateSubtitleInfo();
        if (!string.IsNullOrWhiteSpace(value) && File.Exists(value))
        {
            var ext = Path.GetExtension(value);
            if (string.Equals(ext, ".ass", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ext, ".ssa", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(SelectedOutputFormat, ".mkv", StringComparison.OrdinalIgnoreCase))
                {
                    SelectedOutputFormat = ".mkv";
                }
            }
        }
        IsCompleted = false;
        UpdateCanRemux();
    }

    partial void OnIsRemuxingChanged(bool value)
    {
        UpdateCanRemux();
    }

    partial void OnSelectedOutputFormatChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(OutputFileName))
        {
            var dir = Path.GetDirectoryName(OutputFileName) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(OutputFileName);
            OutputFileName = Path.Combine(dir, name + value);
        }
        IsCompleted = false;
    }

    private static string MakeOutputFileName(string videoFileName, string extension)
    {
        if (string.IsNullOrWhiteSpace(videoFileName))
        {
            return string.Empty;
        }

        var dir = Path.GetDirectoryName(videoFileName) ?? Path.GetTempPath();
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var fileName = Path.Combine(dir, nameNoExt + "_remuxed" + extension);

        var count = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(dir, $"{nameNoExt}_remuxed_{count.ToString(CultureInfo.InvariantCulture)}{extension}");
            count++;
        }

        return fileName;
    }

    [RelayCommand]
    private async Task BrowseVideo()
    {
        if (Window == null)
        {
            return;
        }

        var selectedFile = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.VideoFiles,
            "Video files (*.mp4, *.mkv, *.avi, *.webm, *.ts)",
            ".mp4;.mkv;.avi;.webm;.ts",
            Se.Language.General.AllFiles,
            "*.*");

        if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
        {
            VideoFileName = selectedFile;
            OutputFileName = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
            IsCompleted = false;
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
            "Audio files (*.mp3, *.aac, *.ac3, *.wav, *.mkv, *.mka, *.mp4)",
            ".mp3;.aac;.ac3;.wav;.mkv;.mka;.mp4",
            Se.Language.General.AllFiles,
            "*.*");

        if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
        {
            AudioFileName = selectedFile;
            IsCompleted = false;
        }
    }

    [RelayCommand]
    private async Task BrowseSubtitle()
    {
        if (Window == null)
        {
            return;
        }

        var selectedFile = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.SubtitleFiles,
            "Subtitle files (*.srt, *.ass, *.ssa, *.vtt, *.sub)",
            ".srt;.ass;.ssa;.vtt;.sub",
            Se.Language.General.AllFiles,
            "*.*");

        if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
        {
            SubtitleFileName = selectedFile;
            IsCompleted = false;
        }
    }

    [RelayCommand]
    private async Task BrowseOutputFile()
    {
        if (Window == null)
        {
            return;
        }

        var suggested = OutputFileName;
        if (string.IsNullOrWhiteSpace(suggested) && !string.IsNullOrWhiteSpace(VideoFileName))
        {
            suggested = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
        }

        var selected = await _fileHelper.PickSaveFile(Window, SelectedOutputFormat, suggested, Se.Language.General.SaveVideoAsVideoTitle);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            OutputFileName = selected;
            var ext = Path.GetExtension(selected);
            if (!string.IsNullOrEmpty(ext) && OutputFormats.Contains(ext.ToLowerInvariant()))
            {
                SelectedOutputFormat = ext.ToLowerInvariant();
            }
            IsCompleted = false;
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
    private void Play()
    {
        if (!string.IsNullOrWhiteSpace(OutputFileName) && File.Exists(OutputFileName))
        {
            FileHelper.OpenFileWithDefaultProgram(OutputFileName);
        }
    }

    [RelayCommand]
    private async Task Remux()
    {
        if (Window == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(VideoFileName) || !File.Exists(VideoFileName))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Video.RemuxVideoPleaseSelectBoth, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var videoExt = Path.GetExtension(VideoFileName);
        if (!AllowedVideoExtensions.Contains(videoExt))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, $"Video format '{videoExt}' is not supported (allowed: mp4, mkv, avi, webm, ts).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(AudioFileName) || !File.Exists(AudioFileName))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Video.RemuxVideoPleaseSelectBoth, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var audioExt = Path.GetExtension(AudioFileName);
        if (!AllowedAudioExtensions.Contains(audioExt))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, $"Audio format '{audioExt}' is not supported (allowed: mp3, aac, ac3, wav).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var hasSubtitle = !string.IsNullOrWhiteSpace(SubtitleFileName);
        if (hasSubtitle)
        {
            if (!File.Exists(SubtitleFileName))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle file '{SubtitleFileName}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var subExt = Path.GetExtension(SubtitleFileName);
            if (!AllowedSubtitleExtensions.Contains(subExt))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle format '{subExt}' is not supported (allowed: srt, ass, ssa, vtt, sub).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var isAss = string.Equals(subExt, ".ass", StringComparison.OrdinalIgnoreCase) || string.Equals(subExt, ".ssa", StringComparison.OrdinalIgnoreCase);
            if (isAss && string.Equals(SelectedOutputFormat, ".mp4", StringComparison.OrdinalIgnoreCase))
            {
                var promptMsg = "Phụ đề ASS/SSA có chứa định dạng Style (font, màu sắc, vị trí). Container MP4 không hỗ trợ lưu nhúng soft subtitle ASS đầy đủ style (MP4 chỉ hỗ trợ Timed Text làm mất hết style).\n\nBạn có muốn tự động chuyển sang định dạng MKV (.mkv) để giữ nguyên 100% style của phụ đề ASS không?";
                var choice = await MessageBox.Show(Window, Se.Language.General.Warning, promptMsg, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (choice == MessageBoxResult.Yes)
                {
                    SelectedOutputFormat = ".mkv";
                }
            }
        }

        if (string.IsNullOrWhiteSpace(OutputFileName))
        {
            OutputFileName = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
        }

        var isSameAudioVideo = !string.IsNullOrWhiteSpace(AudioFileName) && string.Equals(Path.GetFullPath(AudioFileName), Path.GetFullPath(VideoFileName), StringComparison.OrdinalIgnoreCase);

        var fullOutput = Path.GetFullPath(OutputFileName);
        if (string.Equals(fullOutput, Path.GetFullPath(VideoFileName), StringComparison.OrdinalIgnoreCase) ||
            (!isSameAudioVideo && string.Equals(fullOutput, Path.GetFullPath(AudioFileName), StringComparison.OrdinalIgnoreCase)) ||
            (hasSubtitle && string.Equals(fullOutput, Path.GetFullPath(SubtitleFileName), StringComparison.OrdinalIgnoreCase)))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.OutputFileCannotBeTheInputFile, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var ffmpegLocation = FfmpegHelper.GetFfmpegLocation();
        if (string.IsNullOrWhiteSpace(ffmpegLocation) || !File.Exists(ffmpegLocation))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, "FFmpeg was not found. Please configure FFmpeg in settings.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        IsRemuxing = true;
        IsCompleted = false;
        ProgressValue = 0;
        ProgressText = Se.Language.Video.RemuxVideoRemuxing;
        _log.Clear();

        try
        {
            double durationSeconds = 0;
            try
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(VideoFileName);
                if (mediaInfo?.Duration != null)
                {
                    durationSeconds = mediaInfo.Duration.TotalSeconds;
                }
            }
            catch
            {
                // Fallback if media info parsing fails
            }

            _progressTracker = new FfmpegProgressTracker(durationSeconds);

            // If audio is WAV and container is MP4, convert to AAC to satisfy MP4 container standards; otherwise stream copy
            var audioCodec = "-c:a copy";
            if (string.Equals(audioExt, ".wav", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(SelectedOutputFormat, ".mp4", StringComparison.OrdinalIgnoreCase))
            {
                audioCodec = "-c:a aac -b:a 192k";
            }

            var audioStreamIndex = SelectedAudioFileTrack?.Index ?? 0;

            string arguments;
            if (isSameAudioVideo)
            {
                var audioMap = $"-map 0:a:{audioStreamIndex}";
                if (hasSubtitle)
                {
                    var subCodec = "-c:s copy";
                    if (string.Equals(SelectedOutputFormat, ".mp4", StringComparison.OrdinalIgnoreCase))
                    {
                        subCodec = "-c:s mov_text";
                    }

                    arguments = $"{FfmpegProgressTracker.ProgressArguments} -y -i \"{VideoFileName}\" -i \"{SubtitleFileName}\" -map 0:v:0 {audioMap} -map 1:s:0 -c:v copy {audioCodec} {subCodec} \"{OutputFileName}\"";
                }
                else
                {
                    arguments = $"{FfmpegProgressTracker.ProgressArguments} -y -i \"{VideoFileName}\" -map 0:v:0 {audioMap} -c:v copy {audioCodec} \"{OutputFileName}\"";
                }
            }
            else
            {
                var audioMap = $"-map 1:a:{audioStreamIndex}";
                if (hasSubtitle)
                {
                    var subCodec = "-c:s copy";
                    if (string.Equals(SelectedOutputFormat, ".mp4", StringComparison.OrdinalIgnoreCase))
                    {
                        subCodec = "-c:s mov_text";
                    }

                    arguments = $"{FfmpegProgressTracker.ProgressArguments} -y -i \"{VideoFileName}\" -i \"{AudioFileName}\" -i \"{SubtitleFileName}\" -map 0:v:0 {audioMap} -map 2:s:0 -c:v copy {audioCodec} {subCodec} \"{OutputFileName}\"";
                }
                else
                {
                    arguments = $"{FfmpegProgressTracker.ProgressArguments} -y -i \"{VideoFileName}\" -i \"{AudioFileName}\" -map 0:v:0 {audioMap} -c:v copy {audioCodec} \"{OutputFileName}\"";
                }
            }

            var tcs = new TaskCompletionSource<bool>();

            _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, (_, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                _log.AppendLine(e.Data);

                if (_progressTracker != null && _progressTracker.TryGetNewPercent(e.Data, out var pct))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = pct;
                        ProgressText = $"{Se.Language.Video.RemuxVideoRemuxing} {pct}%";
                    });
                }
            });

            _ffmpegProcess.EnableRaisingEvents = true;
            _ffmpegProcess.Exited += (_, _) =>
            {
                tcs.TrySetResult(true);
            };

            _ffmpegProcess.Start();
            _ffmpegProcess.BeginOutputReadLine();
            _ffmpegProcess.BeginErrorReadLine();

            await tcs.Task;

            var exitCode = _ffmpegProcess.ExitCode;
            var fileSuccess = File.Exists(OutputFileName) && new FileInfo(OutputFileName).Length > 0;

            if (exitCode == 0 && fileSuccess)
            {
                ProgressValue = 100;
                ProgressText = Se.Language.Video.RemuxVideoCompleted;
                IsCompleted = true;

                // Show standard PromptFileSaved dialog with Play, Open folder, and Done buttons
                await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
                {
                    vm.Initialize(
                        Se.Language.Video.RemuxVideoTitle,
                        string.Format(Se.Language.General.VideoFileGeneratedX, OutputFileName),
                        OutputFileName,
                        true,
                        true);
                });
            }
            else
            {
                ProgressText = Se.Language.Video.RemuxVideoFailed;

                var errMsg = _log.ToString();
                if (errMsg.Length > 800)
                {
                    errMsg = errMsg.Substring(errMsg.Length - 800);
                }

                await MessageBox.Show(
                    Window,
                    Se.Language.General.Error,
                    $"{Se.Language.Video.RemuxVideoFailed}{Environment.NewLine}{Environment.NewLine}{errMsg}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            ProgressText = Se.Language.Video.RemuxVideoFailed;
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                $"{Se.Language.Video.RemuxVideoFailed}{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            IsRemuxing = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsRemuxing)
        {
            try
            {
                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.Kill(true);
                }
            }
            catch
            {
                // ignore
            }

            IsRemuxing = false;
            ProgressText = Se.Language.General.Cancelled;
            return;
        }

        Window?.Close();
    }

    [RelayCommand]
    private void Done()
    {
        OkPressed = true;
        Window?.Close();
    }
}
