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
    private bool _isCancelled;

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

    public List<string> GetAudioFiles()
    {
        return ParseFileList(AudioFileName);
    }

    public List<string> GetSubtitleFiles()
    {
        return ParseFileList(SubtitleFileName);
    }

    private static List<string> ParseFileList(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        return input
            .Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().Trim('\"', '\''))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private bool RequiresMkv(out string reason)
    {
        var audioFiles = GetAudioFiles();
        var subFiles = GetSubtitleFiles();

        if (audioFiles.Count > 1)
        {
            reason = string.IsNullOrWhiteSpace(Se.Language.Video.RemuxVideoMultipleTracksRequiresMkv)
                ? "Multiple audio tracks require the MKV container. Output format has been automatically switched to .mkv."
                : Se.Language.Video.RemuxVideoMultipleTracksRequiresMkv;
            return true;
        }

        if (subFiles.Count > 1)
        {
            reason = string.IsNullOrWhiteSpace(Se.Language.Video.RemuxVideoMultipleTracksRequiresMkv)
                ? "Multiple subtitle tracks require the MKV container. Output format has been automatically switched to .mkv."
                : Se.Language.Video.RemuxVideoMultipleTracksRequiresMkv;
            return true;
        }

        var hasAss = subFiles.Any(f =>
        {
            var ext = Path.GetExtension(f);
            return string.Equals(ext, ".ass", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(ext, ".ssa", StringComparison.OrdinalIgnoreCase);
        });

        if (hasAss)
        {
            reason = string.IsNullOrWhiteSpace(Se.Language.Video.RemuxVideoAssRequiresMkv)
                ? "ASS/SSA subtitles require the MKV container to preserve all formatting and styles."
                : Se.Language.Video.RemuxVideoAssRequiresMkv;
            return true;
        }

        reason = string.Empty;
        return false;
    }

    private void UpdateAudioInfo()
    {
        var audioFiles = GetAudioFiles();
        if (audioFiles.Count == 0)
        {
            AudioFileSize = string.Empty;
            return;
        }

        long totalBytes = 0;
        int validCount = 0;
        foreach (var file in audioFiles)
        {
            if (File.Exists(file))
            {
                try
                {
                    totalBytes += new FileInfo(file).Length;
                    validCount++;
                }
                catch
                {
                }
            }
        }

        if (validCount == 0)
        {
            AudioFileSize = string.Empty;
        }
        else if (audioFiles.Count == 1)
        {
            AudioFileSize = Utilities.FormatBytesToDisplayFileSize(totalBytes);
        }
        else
        {
            AudioFileSize = $"{validCount} tracks ({Utilities.FormatBytesToDisplayFileSize(totalBytes)})";
        }
    }

    private void UpdateSubtitleInfo()
    {
        var subFiles = GetSubtitleFiles();
        if (subFiles.Count == 0)
        {
            SubtitleFileSize = string.Empty;
            return;
        }

        long totalBytes = 0;
        int validCount = 0;
        foreach (var file in subFiles)
        {
            if (File.Exists(file))
            {
                try
                {
                    totalBytes += new FileInfo(file).Length;
                    validCount++;
                }
                catch
                {
                }
            }
        }

        if (validCount == 0)
        {
            SubtitleFileSize = string.Empty;
        }
        else if (subFiles.Count == 1)
        {
            SubtitleFileSize = Utilities.FormatBytesToDisplayFileSize(totalBytes);
        }
        else
        {
            SubtitleFileSize = $"{validCount} tracks ({Utilities.FormatBytesToDisplayFileSize(totalBytes)})";
        }
    }

    private void UpdateCanRemux()
    {
        var videoValid = !string.IsNullOrWhiteSpace(VideoFileName) &&
                         File.Exists(VideoFileName) &&
                         AllowedVideoExtensions.Contains(Path.GetExtension(VideoFileName));

        var audioFiles = GetAudioFiles();
        var audioValid = audioFiles.Count > 0 &&
                         audioFiles.All(f => File.Exists(f) && AllowedAudioExtensions.Contains(Path.GetExtension(f)));

        var subFiles = GetSubtitleFiles();
        var subValid = subFiles.Count == 0 ||
                       subFiles.All(f => File.Exists(f) && AllowedSubtitleExtensions.Contains(Path.GetExtension(f)));

        CanRemux = videoValid && audioValid && subValid && !IsRemuxing;
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
            var title = string.Format(Se.Language.Video.RemuxVideoSelectAudioTrackFor, fileTypeLabel);
            var prompt = string.Format(Se.Language.Video.RemuxVideoSelectAudioTrackPrompt, name, trackOptions.Count);
            vm.Initialize(title, prompt, trackOptions);
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
                var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(value));
                if (value != VideoFileName)
                {
                    return;
                }

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
                        selectedTrack = await PromptSelectAudioTrack(value, audioTracks, Se.Language.Video.RemuxVideoVideoFile);
                    }

                    if (value != VideoFileName)
                    {
                        return;
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

        var audioFiles = GetAudioFiles();
        if (audioFiles.Count > 1)
        {
            if (!string.Equals(SelectedOutputFormat, ".mkv", StringComparison.OrdinalIgnoreCase))
            {
                SelectedOutputFormat = ".mkv";
            }
            AudioFileTracks.Clear();
            SelectedAudioFileTrack = null;
            HasMultipleAudioTracks = false;
            return;
        }

        if (_suppressAudioPrompt)
        {
            return;
        }

        AudioFileTracks.Clear();
        SelectedAudioFileTrack = null;
        HasMultipleAudioTracks = false;

        var singleFile = audioFiles.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(singleFile) && File.Exists(singleFile))
        {
            try
            {
                var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(singleFile));
                if (singleFile != GetAudioFiles().FirstOrDefault())
                {
                    return;
                }

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
                        var selected = await PromptSelectAudioTrack(singleFile, audioTracks, Se.Language.Video.RemuxVideoAudioFile);
                        if (singleFile != GetAudioFiles().FirstOrDefault())
                        {
                            return;
                        }
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
        IsCompleted = false;
        UpdateCanRemux();

        if (RequiresMkv(out _))
        {
            if (!string.Equals(SelectedOutputFormat, ".mkv", StringComparison.OrdinalIgnoreCase))
            {
                SelectedOutputFormat = ".mkv";
            }
        }
    }

    partial void OnIsRemuxingChanged(bool value)
    {
        UpdateCanRemux();
    }

    partial void OnSelectedOutputFormatChanged(string value)
    {
        if (string.Equals(value, ".mp4", StringComparison.OrdinalIgnoreCase) && RequiresMkv(out var reason))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                SelectedOutputFormat = ".mkv";
                if (Window != null)
                {
                    await MessageBox.Show(Window, Se.Language.General.Warning, reason, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
            return;
        }

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

        var selectedFiles = await _fileHelper.PickOpenFiles(
            Window,
            Se.Language.General.AudioFiles,
            "Audio files (*.mp3, *.aac, *.ac3, *.wav, *.mkv, *.mka, *.mp4)",
            new List<string> { "*.mp3", "*.aac", "*.ac3", "*.wav", "*.mkv", "*.mka", "*.mp4" },
            Se.Language.General.AllFiles,
            new List<string> { "*.*" });

        if (selectedFiles != null && selectedFiles.Length > 0)
        {
            AudioFileName = string.Join("; ", selectedFiles);
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

        var selectedFiles = await _fileHelper.PickOpenFiles(
            Window,
            Se.Language.General.SubtitleFiles,
            "Subtitle files (*.srt, *.ass, *.ssa, *.vtt, *.sub)",
            new List<string> { "*.srt", "*.ass", "*.ssa", "*.vtt", "*.sub" },
            Se.Language.General.AllFiles,
            new List<string> { "*.*" });

        if (selectedFiles != null && selectedFiles.Length > 0)
        {
            SubtitleFileName = string.Join("; ", selectedFiles);
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

    private static string EscapeFfmpegMetadata(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("=", "\\=");
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

        var audioFiles = GetAudioFiles();
        if (audioFiles.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Video.RemuxVideoPleaseSelectBoth, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var audioFile in audioFiles)
        {
            if (!File.Exists(audioFile))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Audio file '{audioFile}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var audioExt = Path.GetExtension(audioFile);
            if (!AllowedAudioExtensions.Contains(audioExt))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Audio format '{audioExt}' is not supported (allowed: mp3, aac, ac3, wav, mkv, mka, mp4).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        var subFiles = GetSubtitleFiles();
        foreach (var subFile in subFiles)
        {
            if (!File.Exists(subFile))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle file '{subFile}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var subExt = Path.GetExtension(subFile);
            if (!AllowedSubtitleExtensions.Contains(subExt))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle format '{subExt}' is not supported (allowed: srt, ass, ssa, vtt, sub).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        if (RequiresMkv(out var mkvReason) && string.Equals(SelectedOutputFormat, ".mp4", StringComparison.OrdinalIgnoreCase))
        {
            SelectedOutputFormat = ".mkv";
            OutputFileName = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
        }

        if (string.IsNullOrWhiteSpace(OutputFileName))
        {
            OutputFileName = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
        }

        var fullOutput = Path.GetFullPath(OutputFileName);
        if (string.Equals(fullOutput, Path.GetFullPath(VideoFileName), StringComparison.OrdinalIgnoreCase) ||
            audioFiles.Any(a => string.Equals(fullOutput, Path.GetFullPath(a), StringComparison.OrdinalIgnoreCase)) ||
            subFiles.Any(s => string.Equals(fullOutput, Path.GetFullPath(s), StringComparison.OrdinalIgnoreCase)))
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
        _isCancelled = false;
        ProgressValue = 0;
        ProgressText = Se.Language.Video.RemuxVideoRemuxing;
        _log.Clear();

        try
        {
            double durationSeconds = 0;
            try
            {
                var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(VideoFileName));
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

            var isMkv = string.Equals(SelectedOutputFormat, ".mkv", StringComparison.OrdinalIgnoreCase);

            var inputArgs = new StringBuilder();
            var mapArgs = new StringBuilder();
            var metadataArgs = new StringBuilder();

            // 1. Video input (index 0)
            inputArgs.Append($"-i \"{VideoFileName}\" ");
            mapArgs.Append("-map 0:v:0 ");

            // 2. Audio inputs
            int currentInputIndex = 1;
            var isSameAudioVideo = audioFiles.Count == 1 &&
                string.Equals(Path.GetFullPath(audioFiles[0]), Path.GetFullPath(VideoFileName), StringComparison.OrdinalIgnoreCase);

            if (isSameAudioVideo)
            {
                var audioStreamIndex = SelectedAudioFileTrack?.Index ?? 0;
                mapArgs.Append($"-map 0:a:{audioStreamIndex} ");
                if (SelectedAudioFileTrack != null && !string.IsNullOrWhiteSpace(SelectedAudioFileTrack.DisplayName))
                {
                    metadataArgs.Append($"-metadata:s:a:0 title=\"{EscapeFfmpegMetadata(SelectedAudioFileTrack.DisplayName)}\" ");
                    if (!string.IsNullOrWhiteSpace(SelectedAudioFileTrack.Language) && SelectedAudioFileTrack.Language != "und")
                    {
                        metadataArgs.Append($"-metadata:s:a:0 language=\"{SelectedAudioFileTrack.Language}\" ");
                    }
                }
            }
            else if (audioFiles.Count == 1)
            {
                var audioFile = audioFiles[0];
                inputArgs.Append($"-i \"{audioFile}\" ");
                var audioStreamIndex = SelectedAudioFileTrack?.Index ?? 0;
                mapArgs.Append($"-map {currentInputIndex}:a:{audioStreamIndex} ");
                var trackTitle = Path.GetFileNameWithoutExtension(audioFile);
                metadataArgs.Append($"-metadata:s:a:0 title=\"{EscapeFfmpegMetadata(trackTitle)}\" ");
                if (SelectedAudioFileTrack != null && !string.IsNullOrWhiteSpace(SelectedAudioFileTrack.Language) && SelectedAudioFileTrack.Language != "und")
                {
                    metadataArgs.Append($"-metadata:s:a:0 language=\"{SelectedAudioFileTrack.Language}\" ");
                }
                currentInputIndex++;
            }
            else
            {
                for (int k = 0; k < audioFiles.Count; k++)
                {
                    var audioFile = audioFiles[k];
                    inputArgs.Append($"-i \"{audioFile}\" ");
                    mapArgs.Append($"-map {currentInputIndex}:a:0 ");
                    var trackTitle = Path.GetFileNameWithoutExtension(audioFile);
                    metadataArgs.Append($"-metadata:s:a:{k} title=\"{EscapeFfmpegMetadata(trackTitle)}\" ");
                    currentInputIndex++;
                }
            }

            // 3. Subtitle inputs
            for (int j = 0; j < subFiles.Count; j++)
            {
                var subFile = subFiles[j];
                inputArgs.Append($"-i \"{subFile}\" ");
                mapArgs.Append($"-map {currentInputIndex}:s:0 ");
                var subTitle = Path.GetFileNameWithoutExtension(subFile);
                metadataArgs.Append($"-metadata:s:s:{j} title=\"{EscapeFfmpegMetadata(subTitle)}\" ");
                currentInputIndex++;
            }

            // 4. Codecs
            var videoCodec = "-c:v copy";

            string audioCodec;
            if (isMkv)
            {
                audioCodec = "-c:a copy";
            }
            else
            {
                var hasWav = audioFiles.Any(f => string.Equals(Path.GetExtension(f), ".wav", StringComparison.OrdinalIgnoreCase));
                audioCodec = hasWav ? "-c:a aac -b:a 192k" : "-c:a copy";
            }

            string subCodec = string.Empty;
            if (subFiles.Count > 0)
            {
                subCodec = isMkv ? "-c:s copy" : "-c:s mov_text";
            }

            var arguments = $"{FfmpegProgressTracker.ProgressArguments} -y {inputArgs}{mapArgs}{videoCodec} {audioCodec} {subCodec} {metadataArgs}\"{OutputFileName}\"".Trim();

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

            if (_isCancelled)
            {
                ProgressText = Se.Language.General.Cancelled;
                DeletePartialOutputFile();
                return;
            }

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
            if (_isCancelled)
            {
                ProgressText = Se.Language.General.Cancelled;
                DeletePartialOutputFile();
                return;
            }

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
            _isCancelled = true;
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

            ProgressText = Se.Language.General.Cancelled;
            DeletePartialOutputFile();
            return;
        }

        Window?.Close();
    }

    private void DeletePartialOutputFile()
    {
        if (!string.IsNullOrWhiteSpace(OutputFileName) && File.Exists(OutputFileName))
        {
            try
            {
                File.Delete(OutputFileName);
            }
            catch
            {
                // ignore
            }
        }
    }

    [RelayCommand]
    private void Done()
    {
        OkPressed = true;
        Window?.Close();
    }
}
