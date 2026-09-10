using Avalonia.Controls;
using Avalonia.Input;
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
using System.Collections.Specialized;
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
    [ObservableProperty] private ObservableCollection<RemuxFileItem> _audioFiles = new();
    [ObservableProperty] private RemuxFileItem? _selectedAudioFile;
    [ObservableProperty] private string _audioFilesInfo = string.Empty;
    [ObservableProperty] private ObservableCollection<RemuxFileItem> _subtitleFiles = new();
    [ObservableProperty] private RemuxFileItem? _selectedSubtitleFile;
    [ObservableProperty] private string _subtitleFilesInfo = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _outputFormats;
    [ObservableProperty] private string _selectedOutputFormat = ".mp4";
    [ObservableProperty] private string _outputFileName = string.Empty;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isRemuxing;
    [ObservableProperty] private bool _isNotRemuxing = true;
    [ObservableProperty] private bool _canRemux;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private bool _isAudioRemoveEnabled;
    [ObservableProperty] private bool _isAudioMoveUpEnabled;
    [ObservableProperty] private bool _isAudioMoveDownEnabled;
    [ObservableProperty] private bool _isAudioSelectTrackVisible;
    [ObservableProperty] private bool _isSubtitleRemoveEnabled;
    [ObservableProperty] private bool _isSubtitleMoveUpEnabled;
    [ObservableProperty] private bool _isSubtitleMoveDownEnabled;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;
    private Process? _ffmpegProcess;
    private readonly StringBuilder _log = new();
    private FfmpegProgressTracker? _progressTracker;
    private bool _isCancelled;

    public RemuxVideoViewModel(IFileHelper fileHelper, IFolderHelper folderHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _windowService = windowService;
        OutputFormats = new ObservableCollection<string> { ".mp4", ".mkv" };
        SelectedOutputFormat = OutputFormats[0];

        AudioFiles.CollectionChanged += AudioFilesOnCollectionChanged;
        SubtitleFiles.CollectionChanged += SubtitleFilesOnCollectionChanged;
    }

    public void Initialize(string? currentVideoFileName)
    {
        if (!string.IsNullOrWhiteSpace(currentVideoFileName) && File.Exists(currentVideoFileName))
        {
            var ext = Path.GetExtension(currentVideoFileName);
            if (AllowedVideoExtensions.Contains(ext))
            {
                SelectedOutputFormat = string.Equals(ext, ".mkv", StringComparison.OrdinalIgnoreCase) ? ".mkv" : ".mp4";
                VideoFileName = currentVideoFileName;
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

    private bool RequiresMkv(out string reason)
    {
        if (AudioFiles.Count > 1 || SubtitleFiles.Count > 1)
        {
            reason = Se.Language.Video.RemuxVideoMultipleTracksRequiresMkv;
            return true;
        }

        var hasAss = SubtitleFiles.Any(f =>
        {
            var ext = Path.GetExtension(f.FileName);
            return string.Equals(ext, ".ass", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(ext, ".ssa", StringComparison.OrdinalIgnoreCase);
        });

        if (hasAss)
        {
            reason = Se.Language.Video.RemuxVideoAssRequiresMkv;
            return true;
        }

        reason = string.Empty;
        return false;
    }

    private static string MakeFilesInfo(ObservableCollection<RemuxFileItem> files)
    {
        if (files.Count == 0)
        {
            return string.Empty;
        }

        var totalBytes = files.Sum(f => f.SizeBytes);
        var size = Utilities.FormatBytesToDisplayFileSize(totalBytes);
        return files.Count == 1
            ? size
            : $"{string.Format(Se.Language.Video.RemuxVideoFilesX, files.Count)} ({size})";
    }

    private void UpdateCanRemux()
    {
        var videoValid = !string.IsNullOrWhiteSpace(VideoFileName) &&
                         File.Exists(VideoFileName) &&
                         AllowedVideoExtensions.Contains(Path.GetExtension(VideoFileName));

        var audioValid = AudioFiles.Count > 0 &&
                         AudioFiles.All(f => File.Exists(f.FileName) && AllowedAudioExtensions.Contains(Path.GetExtension(f.FileName)));

        var subValid = SubtitleFiles.All(f => File.Exists(f.FileName) && AllowedSubtitleExtensions.Contains(Path.GetExtension(f.FileName)));

        CanRemux = videoValid && audioValid && subValid && !IsRemuxing;
    }

    private void EnforceMkvIfRequired()
    {
        if (RequiresMkv(out _) && !string.Equals(SelectedOutputFormat, ".mkv", StringComparison.OrdinalIgnoreCase))
        {
            SelectedOutputFormat = ".mkv";
        }
    }

    private void AudioFilesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        AudioFilesInfo = MakeFilesInfo(AudioFiles);
        IsCompleted = false;
        UpdateCanRemux();
        EnforceMkvIfRequired();
        UpdateAudioListState();
    }

    private void SubtitleFilesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SubtitleFilesInfo = MakeFilesInfo(SubtitleFiles);
        IsCompleted = false;
        UpdateCanRemux();
        EnforceMkvIfRequired();
        UpdateSubtitleListState();
    }

    private void UpdateAudioListState()
    {
        var index = SelectedAudioFile == null ? -1 : AudioFiles.IndexOf(SelectedAudioFile);
        IsAudioRemoveEnabled = index >= 0;
        IsAudioMoveUpEnabled = index > 0;
        IsAudioMoveDownEnabled = index >= 0 && index < AudioFiles.Count - 1;
        IsAudioSelectTrackVisible = SelectedAudioFile?.HasMultipleTracks == true;
    }

    private void UpdateSubtitleListState()
    {
        var index = SelectedSubtitleFile == null ? -1 : SubtitleFiles.IndexOf(SelectedSubtitleFile);
        IsSubtitleRemoveEnabled = index >= 0;
        IsSubtitleMoveUpEnabled = index > 0;
        IsSubtitleMoveDownEnabled = index >= 0 && index < SubtitleFiles.Count - 1;
    }

    partial void OnSelectedAudioFileChanged(RemuxFileItem? value)
    {
        UpdateAudioListState();
    }

    partial void OnSelectedSubtitleFileChanged(RemuxFileItem? value)
    {
        UpdateSubtitleListState();
    }

    private static List<AudioTrackOption> ReadAudioTracks(FfmpegMediaInfo2 mediaInfo)
    {
        return mediaInfo.Tracks
            .Where(t => t.TrackType == FfmpegTrackType.Audio)
            .Select((t, index) => new AudioTrackOption
            {
                Index = index,
                Language = t.Language,
                Details = t.TrackInfo
            })
            .ToList();
    }

    private async Task<AudioTrackOption?> PromptSelectAudioTrack(string fileName, List<AudioTrackOption> trackOptions, string fileTypeLabel, AudioTrackOption? current = null)
    {
        if (Window == null || trackOptions.Count <= 1)
        {
            return trackOptions.FirstOrDefault();
        }

        var dialogVm = await _windowService.ShowDialogAsync<PickAudioTrackWindow, PickAudioTrackViewModel>(Window, vm =>
        {
            var name = Path.GetFileName(fileName);
            var title = string.Format(Se.Language.Video.RemuxVideoSelectAudioTrackFor, fileTypeLabel);
            var prompt = string.Format(Se.Language.Video.RemuxVideoSelectAudioTrackPrompt, name, trackOptions.Count);
            var defaultIndex = current == null ? 0 : Math.Max(0, trackOptions.IndexOf(current));
            vm.Initialize(title, prompt, trackOptions, defaultIndex);
        });

        if (dialogVm != null && dialogVm.OkPressed && dialogVm.SelectedTrack != null)
        {
            return dialogVm.SelectedTrack;
        }

        return current ?? trackOptions.FirstOrDefault();
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

        if (string.IsNullOrWhiteSpace(value) || !File.Exists(value))
        {
            return;
        }

        // The video's own audio is the default audio source, so offer it as the first
        // audio entry when the list is empty (or only holds the previous video).
        var onlyVideoAudio = AudioFiles.Count == 0 ||
                             (AudioFiles.Count == 1 && string.Equals(AudioFiles[0].FileName, _lastVideoAudioFileName, StringComparison.OrdinalIgnoreCase));
        _lastVideoAudioFileName = value;
        if (!onlyVideoAudio)
        {
            return;
        }

        try
        {
            var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(value));
            if (value != VideoFileName)
            {
                return;
            }

            var audioTracks = ReadAudioTracks(mediaInfo);
            if (audioTracks.Count == 0)
            {
                return;
            }

            var selectedTrack = audioTracks[0];
            if (audioTracks.Count > 1)
            {
                selectedTrack = await PromptSelectAudioTrack(value, audioTracks, Se.Language.Video.RemuxVideoVideoFile) ?? audioTracks[0];
            }

            if (value != VideoFileName)
            {
                return;
            }

            AudioFiles.Clear();
            var item = new RemuxFileItem(value);
            item.SetTracks(audioTracks, selectedTrack);
            AudioFiles.Add(item);
            SelectedAudioFile = item;
        }
        catch
        {
            // media info parsing failed - the user can still add audio files by hand
        }
    }

    private string? _lastVideoAudioFileName;

    partial void OnIsRemuxingChanged(bool value)
    {
        IsNotRemuxing = !value;
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
    private async Task AddAudio()
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

        foreach (var fileName in selectedFiles ?? Array.Empty<string>())
        {
            await AddAudioFile(fileName);
        }
    }

    public async Task AddAudioFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName) ||
            AudioFiles.Any(f => string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var item = new RemuxFileItem(fileName);
        try
        {
            var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(fileName));
            var audioTracks = ReadAudioTracks(mediaInfo);
            AudioTrackOption? selected = null;
            if (audioTracks.Count > 1)
            {
                selected = await PromptSelectAudioTrack(fileName, audioTracks, Se.Language.Video.RemuxVideoAudioFile);
            }
            item.SetTracks(audioTracks, selected);
        }
        catch
        {
            // media info parsing failed - map the first audio stream
        }

        AudioFiles.Add(item);
        SelectedAudioFile = item;
    }

    [RelayCommand]
    private void RemoveAudio()
    {
        RemoveSelected(AudioFiles, SelectedAudioFile, item => SelectedAudioFile = item);
    }

    [RelayCommand]
    private void ClearAudio()
    {
        AudioFiles.Clear();
        SelectedAudioFile = null;
    }

    [RelayCommand]
    private void MoveAudioUp()
    {
        MoveSelected(AudioFiles, SelectedAudioFile, ListMoveDirection.Up);
        UpdateAudioListState();
    }

    [RelayCommand]
    private void MoveAudioDown()
    {
        MoveSelected(AudioFiles, SelectedAudioFile, ListMoveDirection.Down);
        UpdateAudioListState();
    }

    [RelayCommand]
    private async Task SelectAudioTrack()
    {
        var item = SelectedAudioFile;
        if (item == null || item.Tracks.Count <= 1)
        {
            return;
        }

        var label = string.Equals(item.FileName, VideoFileName, StringComparison.OrdinalIgnoreCase)
            ? Se.Language.Video.RemuxVideoVideoFile
            : Se.Language.Video.RemuxVideoAudioFile;
        var selected = await PromptSelectAudioTrack(item.FileName, item.Tracks, label, item.SelectedTrack);
        if (selected != null)
        {
            item.SelectedTrack = selected;
            IsCompleted = false;
        }
    }

    [RelayCommand]
    private async Task AddSubtitle()
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

        foreach (var fileName in selectedFiles ?? Array.Empty<string>())
        {
            AddSubtitleFile(fileName);
        }
    }

    public void AddSubtitleFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName) ||
            SubtitleFiles.Any(f => string.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var item = new RemuxFileItem(fileName);
        SubtitleFiles.Add(item);
        SelectedSubtitleFile = item;
    }

    [RelayCommand]
    private void RemoveSubtitle()
    {
        RemoveSelected(SubtitleFiles, SelectedSubtitleFile, item => SelectedSubtitleFile = item);
    }

    [RelayCommand]
    private void ClearSubtitle()
    {
        SubtitleFiles.Clear();
        SelectedSubtitleFile = null;
    }

    [RelayCommand]
    private void MoveSubtitleUp()
    {
        MoveSelected(SubtitleFiles, SelectedSubtitleFile, ListMoveDirection.Up);
        UpdateSubtitleListState();
    }

    [RelayCommand]
    private void MoveSubtitleDown()
    {
        MoveSelected(SubtitleFiles, SelectedSubtitleFile, ListMoveDirection.Down);
        UpdateSubtitleListState();
    }

    private static void RemoveSelected(ObservableCollection<RemuxFileItem> items, RemuxFileItem? selected, Action<RemuxFileItem?> setSelected)
    {
        if (selected == null)
        {
            return;
        }

        var index = items.IndexOf(selected);
        if (index < 0)
        {
            return;
        }

        items.RemoveAt(index);
        if (items.Count == 0)
        {
            setSelected(null);
            return;
        }

        setSelected(items[Math.Min(index, items.Count - 1)]);
    }

    private static void MoveSelected(ObservableCollection<RemuxFileItem> items, RemuxFileItem? selected, ListMoveDirection direction)
    {
        if (selected == null)
        {
            return;
        }

        var index = items.IndexOf(selected);
        if (index < 0)
        {
            return;
        }

        ListReorder.Move(items, new[] { index }, direction);
    }

    /// <summary>
    /// Ctrl+Up/Ctrl+Down reorder, Delete removes - for both lists; the sender tells which one.
    /// </summary>
    internal void AudioListKeyDown(object? sender, KeyEventArgs e) => ListKeyDown(e, MoveAudioUpCommand, MoveAudioDownCommand, RemoveAudioCommand);

    internal void SubtitleListKeyDown(object? sender, KeyEventArgs e) => ListKeyDown(e, MoveSubtitleUpCommand, MoveSubtitleDownCommand, RemoveSubtitleCommand);

    private static void ListKeyDown(KeyEventArgs e, IRelayCommand moveUp, IRelayCommand moveDown, IRelayCommand remove)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Up)
        {
            e.Handled = true;
            moveUp.Execute(null);
        }
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Down)
        {
            e.Handled = true;
            moveDown.Execute(null);
        }
        else if (e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.None)
        {
            e.Handled = true;
            remove.Execute(null);
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

        var audioFiles = AudioFiles.ToList();
        if (audioFiles.Count == 0)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Video.RemuxVideoPleaseSelectBoth, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var audioFile in audioFiles)
        {
            if (!File.Exists(audioFile.FileName))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Audio file '{audioFile.FileName}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var audioExt = Path.GetExtension(audioFile.FileName);
            if (!AllowedAudioExtensions.Contains(audioExt))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Audio format '{audioExt}' is not supported (allowed: mp3, aac, ac3, wav, mkv, mka, mp4).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        var subFiles = SubtitleFiles.ToList();
        foreach (var subFile in subFiles)
        {
            if (!File.Exists(subFile.FileName))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle file '{subFile.FileName}' does not exist.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var subExt = Path.GetExtension(subFile.FileName);
            if (!AllowedSubtitleExtensions.Contains(subExt))
            {
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle format '{subExt}' is not supported (allowed: srt, ass, ssa, vtt, sub).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        if (RequiresMkv(out _) && string.Equals(SelectedOutputFormat, ".mp4", StringComparison.OrdinalIgnoreCase))
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
            audioFiles.Any(a => string.Equals(fullOutput, Path.GetFullPath(a.FileName), StringComparison.OrdinalIgnoreCase)) ||
            subFiles.Any(s => string.Equals(fullOutput, Path.GetFullPath(s.FileName), StringComparison.OrdinalIgnoreCase)))
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
            var videoFullPath = Path.GetFullPath(VideoFileName);

            var inputArgs = new StringBuilder();
            var mapArgs = new StringBuilder();
            var metadataArgs = new StringBuilder();

            // 1. Video input (index 0)
            inputArgs.Append($"-i \"{VideoFileName}\" ");
            mapArgs.Append("-map 0:v:0 ");

            // 2. Audio inputs - the video's own audio is mapped from input 0, every other
            //    file becomes its own input; the selected track decides the stream index.
            var currentInputIndex = 1;
            for (var k = 0; k < audioFiles.Count; k++)
            {
                var audioFile = audioFiles[k];
                var streamIndex = audioFile.SelectedTrack?.Index ?? 0;
                var isVideoAudio = string.Equals(Path.GetFullPath(audioFile.FileName), videoFullPath, StringComparison.OrdinalIgnoreCase);
                if (isVideoAudio)
                {
                    mapArgs.Append($"-map 0:a:{streamIndex} ");
                }
                else
                {
                    inputArgs.Append($"-i \"{audioFile.FileName}\" ");
                    mapArgs.Append($"-map {currentInputIndex}:a:{streamIndex} ");
                    currentInputIndex++;
                }

                var track = audioFile.SelectedTrack;
                var trackTitle = track != null && audioFile.Tracks.Count > 1
                    ? track.DisplayName
                    : Path.GetFileNameWithoutExtension(audioFile.FileName);
                metadataArgs.Append($"-metadata:s:a:{k} title=\"{EscapeFfmpegMetadata(trackTitle)}\" ");
                if (track != null && !string.IsNullOrWhiteSpace(track.Language) && track.Language != "und")
                {
                    metadataArgs.Append($"-metadata:s:a:{k} language=\"{track.Language}\" ");
                }
            }

            // 3. Subtitle inputs
            for (var j = 0; j < subFiles.Count; j++)
            {
                var subFile = subFiles[j];
                inputArgs.Append($"-i \"{subFile.FileName}\" ");
                mapArgs.Append($"-map {currentInputIndex}:s:0 ");
                var subTitle = Path.GetFileNameWithoutExtension(subFile.FileName);
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
                var hasWav = audioFiles.Any(f => string.Equals(Path.GetExtension(f.FileName), ".wav", StringComparison.OrdinalIgnoreCase));
                audioCodec = hasWav ? "-c:a aac -b:a 192k" : "-c:a copy";
            }

            var subCodec = string.Empty;
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && !IsRemuxing)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
