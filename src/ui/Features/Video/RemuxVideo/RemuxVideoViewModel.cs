using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
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
        ".mp4", ".mkv", ".mov", ".avi", ".webm", ".ts", ".mpg", ".mpeg", ".vob"
    };

    // The input video is added as the default audio source, so every video container is an audio source too
    private static readonly HashSet<string> AllowedAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".aac", ".m4a", ".ac3", ".wav", ".mka",
        ".mp4", ".mkv", ".mov", ".avi", ".webm", ".ts", ".mpg", ".mpeg", ".vob"
    };

    private static readonly HashSet<string> AllowedSubtitleExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".srt", ".ass", ".ssa", ".vtt", ".sub", ".scc", ".mcc"
    };

    [ObservableProperty] private string _videoFileName = string.Empty;
    [ObservableProperty] private string _videoFileSize = string.Empty;
    private TimeSpan? _videoDuration;
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
    [ObservableProperty] private bool _isFinalizing;
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
    [ObservableProperty] private bool _promptForFfmpegParameters;
    [ObservableProperty] private bool _mixAudio;
    [ObservableProperty] private bool _isMixAudioVisible;
    [ObservableProperty] private bool _isVolumeEnabled;
    [ObservableProperty] private bool _fastStart;
    [ObservableProperty] private bool _isFastStartVisible = true;

    /// <summary>
    /// For an .mpg: the video is not MPEG-2, and the user agreed to have it re-encoded.
    /// </summary>
    internal bool ReencodeVideoToMpeg2 { get; set; }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly IWindowService _windowService;
    private Process? _ffmpegProcess;
    private readonly StringBuilder _log = new();
    private FfmpegProgressTracker? _progressTracker;
    private bool _isCancelled;
    private bool _isAddingCaptions;

    public RemuxVideoViewModel(IFileHelper fileHelper, IFolderHelper folderHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _windowService = windowService;
        OutputFormats = new ObservableCollection<string> { ".mp4", ".mkv", ".mov", ".mpg" };
        SelectedOutputFormat = OutputFormats[0];
        FastStart = Se.Settings.Video.RemuxFastStart;

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
                SelectedOutputFormat = DefaultOutputFormat(ext);
                VideoFileName = currentVideoFileName;
                OutputFileName = MakeOutputFileName(VideoFileName, SelectedOutputFormat);
            }
        }

        UpdateCanRemux();
    }

    private void UpdateVideoInfo(TimeSpan? duration = null)
    {
        if (duration.HasValue)
        {
            _videoDuration = duration;
        }

        if (!string.IsNullOrWhiteSpace(VideoFileName) && File.Exists(VideoFileName))
        {
            try
            {
                var length = new FileInfo(VideoFileName).Length;
                var size = Utilities.FormatBytesToDisplayFileSize(length);
                if (_videoDuration.HasValue && _videoDuration.Value.TotalMilliseconds > 0)
                {
                    VideoFileSize = $"{RemuxFileItem.FormatDuration(_videoDuration.Value)}  -  {size}";
                }
                else
                {
                    VideoFileSize = size;
                }
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

    /// <summary>
    /// Mixing only happens with two or more audio files; the checkbox is hidden below that, and
    /// a single file is remuxed as it is (its volume setting is ignored).
    /// </summary>
    private bool IsMixing => MixAudio && AudioFiles.Count > 1;

    private static bool IsScc(RemuxFileItem file) =>
        string.Equals(Path.GetExtension(file.FileName), ".scc", StringComparison.OrdinalIgnoreCase);

    private static bool IsMcc(RemuxFileItem file) =>
        string.Equals(Path.GetExtension(file.FileName), ".mcc", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The container the current tracks need, or null when the selected one will do.
    /// Scenarist (.scc) goes in as a QuickTime "c608" CEA-608 closed caption track, which
    /// ffmpeg can only write to .mov (neither mp4 nor mkv has a tag for eia_608, #15382). A .mov
    /// also holds several audio and subtitle tracks, so SCC wins over the .mkv requirements;
    /// the other subtitles are converted to mov_text.
    /// An .mpg holds every subtitle as CEA-608 captions in the video and several audio tracks,
    /// so it needs nothing else (#15405).
    /// </summary>
    private string? RequiredOutputFormat(out string reason)
    {
        if (IsMpg(SelectedOutputFormat))
        {
            reason = string.Empty;
            return null;
        }

        // MacCaption (.mcc) carries CEA-608 and CEA-708 caption packets, which only the .mpg
        // output embeds (ffmpeg has no MCC reader to mux it anywhere else).
        if (SubtitleFiles.Any(IsMcc))
        {
            reason = Se.Language.Video.RemuxVideoMccRequiresMpg;
            return ".mpg";
        }

        if (SubtitleFiles.Any(IsScc))
        {
            reason = Se.Language.Video.RemuxVideoSccRequiresMov;
            return ".mov";
        }

        return RequiresMkv(out reason) ? ".mkv" : null;
    }

    /// <summary>
    /// The ISO 639-2/B code for a language tag in the subtitle file name ("movie.en.scc",
    /// "movie.eng.srt", "movie_[eng].srt"), or null. ffmpeg writes it to the mov/mp4 track
    /// header and the Matroska track, so players show "English CC" instead of "Unknown" (#15405).
    /// </summary>
    internal static string? GetSubtitleLanguageFromFileName(string fileName)
    {
        var language = OcrViewModel.ResolveIsoLanguage(OcrViewModel.DetectLanguageCodeFromFileName(fileName));
        return language?.BibliographicCode;
    }

    private bool RequiresMkv(out string reason)
    {
        var isMov = string.Equals(SelectedOutputFormat, ".mov", StringComparison.OrdinalIgnoreCase);
        if (!isMov && ((AudioFiles.Count > 1 && !IsMixing) || SubtitleFiles.Count > 1))
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
        if (files.Count == 1)
        {
            return !string.IsNullOrEmpty(files[0].DurationDisplay)
                ? $"{files[0].DurationDisplay}  -  {size}"
                : size;
        }

        return $"{string.Format(Se.Language.Video.RemuxVideoFilesX, files.Count)} ({size})";
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
        var required = RequiredOutputFormat(out _);
        if (required != null && !string.Equals(SelectedOutputFormat, required, StringComparison.OrdinalIgnoreCase))
        {
            SelectedOutputFormat = required;
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
        UpdateMixState();
    }

    private void UpdateMixState()
    {
        IsMixAudioVisible = AudioFiles.Count > 1;
        IsVolumeEnabled = IsMixing && SelectedAudioFile != null;
        foreach (var item in AudioFiles)
        {
            item.ShowVolume = IsMixing;
        }
    }

    partial void OnMixAudioChanged(bool value)
    {
        IsCompleted = false;
        UpdateMixState();
        EnforceMkvIfRequired();
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
        _videoDuration = null;
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

        try
        {
            var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(value));
            if (value != VideoFileName)
            {
                return;
            }

            if (mediaInfo.Duration != null && mediaInfo.Duration.TotalMilliseconds > 0)
            {
                UpdateVideoInfo(mediaInfo.Duration.TimeSpan);
            }

            UseOutputFormatForVideoCodec(mediaInfo.Tracks.FirstOrDefault(t => t.TrackType == FfmpegTrackType.Video)?.TrackInfo);

            if (!onlyVideoAudio)
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
            if (mediaInfo.Duration != null && mediaInfo.Duration.TotalMilliseconds > 0)
            {
                item.SetDuration(mediaInfo.Duration.TimeSpan);
            }
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

    partial void OnFastStartChanged(bool value)
    {
        IsCompleted = false;
        Se.Settings.Video.RemuxFastStart = value;
    }

    partial void OnSelectedOutputFormatChanged(string value)
    {
        IsFastStartVisible = IsMovFamily(value);
        var required = RequiredOutputFormat(out var reason);
        if (required != null && !string.Equals(value, required, StringComparison.OrdinalIgnoreCase))
        {
            Dispatcher.UIThread.Post(async () =>
            {
                SelectedOutputFormat = required;
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

            // A name that was suggested here is suggested again for the new extension, so it
            // steps past files that exist. Swapping the extension alone could land on an earlier
            // "x_remuxed.mkv", which "-y" then overwrote without a word - and Cancel deleted.
            var suggestedName = Path.GetFileNameWithoutExtension(VideoFileName ?? string.Empty) + "_remuxed";
            var isSuggestedName = !string.IsNullOrWhiteSpace(VideoFileName) &&
                                  (name == suggestedName || name.StartsWith(suggestedName + "_", StringComparison.Ordinal)) &&
                                  string.Equals(dir, Path.GetDirectoryName(VideoFileName) ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            OutputFileName = isSuggestedName
                ? MakeOutputFileName(VideoFileName!, value)
                : Path.Combine(dir, name + value);
        }
        IsCompleted = false;
    }

    /// <summary>
    /// The output format for an input video: the same container when it is one of the output
    /// formats (an MPEG program stream - .mpeg, .vob - stays .mpg), else .mp4.
    /// </summary>
    private string DefaultOutputFormat(string inputExtension)
    {
        var ext = inputExtension.ToLowerInvariant();
        if (ext == ".mpeg" || ext == ".vob")
        {
            return ".mpg";
        }

        return OutputFormats.Contains(ext) ? ext : ".mp4";
    }

    /// <summary>
    /// Switches the output to .mkv when the selected .mp4/.mov cannot hold the video as it is -
    /// a .webm's VP8 defaulted to .mp4 and "-c:v copy" failed with "Could not find tag for codec
    /// vp8". Not when the subtitles need a particular container.
    /// </summary>
    internal void UseOutputFormatForVideoCodec(string? videoTrackDetails)
    {
        if (!CanCopyVideoTo(videoTrackDetails, SelectedOutputFormat) && RequiredOutputFormat(out _) == null)
        {
            SelectedOutputFormat = ".mkv";
        }
    }

    /// <summary>
    /// False when ffmpeg cannot stream-copy the video in <paramref name="trackDetails"/> into
    /// <paramref name="outputExtension"/>: .mp4 only takes the MPEG-4 registered codecs, and
    /// .mov takes most anything but VP8. Unknown details (media info not read) count as copyable.
    /// </summary>
    internal static bool CanCopyVideoTo(string? trackDetails, string outputExtension)
    {
        var codec = GetCodecName(trackDetails);
        if (codec.Length == 0)
        {
            return true;
        }

        if (string.Equals(outputExtension, ".mp4", StringComparison.OrdinalIgnoreCase))
        {
            return codec is "h264" or "hevc" or "vvc" or "av1" or "vp9" or "mpeg4" or "mpeg2video" or "mpeg1video"
                or "mjpeg" or "vc1" or "png" or "jpeg2000" or "dirac";
        }

        if (string.Equals(outputExtension, ".mov", StringComparison.OrdinalIgnoreCase))
        {
            return codec != "vp8";
        }

        return true;
    }

    private static bool IsMpg(string extension) =>
        string.Equals(extension, ".mpg", StringComparison.OrdinalIgnoreCase);

    private static bool IsMovFamily(string extension) =>
        string.Equals(extension, ".mp4", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(extension, ".mov", StringComparison.OrdinalIgnoreCase);

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
            "Video files (*.mp4, *.mkv, *.mov, *.avi, *.webm, *.ts)",
            ".mp4;.mkv;.mov;.avi;.webm;.ts",
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
            "Audio files (*.mp3, *.aac, *.m4a, *.ac3, *.wav, *.mka, *.mp4, *.mkv, *.mov, *.avi, *.webm, *.ts, *.mpg, *.mpeg, *.vob)",
            new List<string> { "*.mp3", "*.aac", "*.m4a", "*.ac3", "*.wav", "*.mka", "*.mp4", "*.mkv", "*.mov", "*.avi", "*.webm", "*.ts", "*.mpg", "*.mpeg", "*.vob" },
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
            if (mediaInfo.Duration != null && mediaInfo.Duration.TotalMilliseconds > 0)
            {
                item.SetDuration(mediaInfo.Duration.TimeSpan);
            }
            var audioTracks = ReadAudioTracks(mediaInfo);

            // A video without sound would fail in ffmpeg on "-map N:a:0". No tracks at all means
            // ffmpeg could not read the file (or is missing) - leave that to the remux itself.
            if (audioTracks.Count == 0 && mediaInfo.Tracks.Count > 0)
            {
                if (Window != null)
                {
                    await MessageBox.Show(Window, Se.Language.General.Warning, string.Format(Se.Language.Video.RemuxVideoFileHasNoAudioX, Path.GetFileName(fileName)), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                return;
            }

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
            "Subtitle files (*.srt, *.ass, *.ssa, *.vtt, *.sub, *.scc, *.mcc)",
            new List<string> { "*.srt", "*.ass", "*.ssa", "*.vtt", "*.sub", "*.scc", "*.mcc" },
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

    /// <summary>
    /// Makes a track title safe inside the double-quoted value of "-metadata title=...".
    /// ffmpeg splits "key=value" at the first "=" only and does no unescaping of the value,
    /// so escaping "=" or "\" put the backslashes into the title verbatim ("Track 1=EN.mp3"
    /// became the title "Track 1\=EN"). Same mapping as FfmpegGenerator.EscapeFfmpegArg.
    /// </summary>
    internal static string EscapeFfmpegMetadata(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\\", "_").Replace("\"", "'");
    }

    internal static string FormatVolumeFactor(int volumePercent)
    {
        return (Math.Clamp(volumePercent, 0, 200) / 100.0).ToString("0.00", CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private async Task PromptFfmpegParametersAndRemux()
    {
        PromptForFfmpegParameters = true;
        try
        {
            await Remux();
        }
        finally
        {
            PromptForFfmpegParameters = false;
        }
    }

    /// <summary>
    /// The time part of the progress line: "00:09 elapsed · ~00:01 left" once ffmpeg has reported
    /// a percentage to extrapolate from, just "00:09 elapsed" before that (or at 100%).
    /// </summary>
    public static string FormatProgressTime(TimeSpan elapsed, double percent)
    {
        var elapsedText = RemuxFileItem.FormatDuration(elapsed);
        if (percent <= 0 || percent >= 100 || elapsed.TotalSeconds < 1)
        {
            return string.Format(Se.Language.Video.TextToSpeech.XElapsed, elapsedText);
        }

        var remaining = TimeSpan.FromSeconds(elapsed.TotalSeconds * (100.0 - percent) / percent);
        return string.Format(Se.Language.Video.TextToSpeech.XElapsedYLeft, elapsedText, RemuxFileItem.FormatDuration(remaining));
    }

    private void UpdateProgressText(TimeSpan elapsed)
    {
        if (IsFinalizing)
        {
            // ffmpeg is copying the whole file to move the mp4 index to the front (faststart);
            // there are no progress lines for that, so no percentage or time left - just the
            // elapsed time, so the user can see it is still alive (#15197).
            var elapsedOnly = string.Format(Se.Language.Video.TextToSpeech.XElapsed, RemuxFileItem.FormatDuration(elapsed));
            ProgressText = $"{Se.Language.Video.RemuxVideoFinalizing} ({elapsedOnly})";
            return;
        }

        var time = FormatProgressTime(elapsed, ProgressValue);
        var action = _isAddingCaptions ? Se.Language.Video.RemuxVideoAddingClosedCaptions : Se.Language.Video.RemuxVideoRemuxing;
        ProgressText = ProgressValue > 0
            ? $"{action} {(int)ProgressValue}% ({time})"
            : $"{action} ({time})";
    }

    /// <summary>
    /// Whether the main window should take over the remuxed file when this dialog closes: only
    /// after "Done" on a finished remux, and only when the remuxed video is the one the main
    /// window still has loaded (or it has none). The dialog is modeless and takes any video, so
    /// the user can have moved on to another video/subtitle in the meantime - that one must not
    /// be replaced behind their back.
    /// </summary>
    internal bool ShouldLoadOutputOnClose(string? currentVideoFileName)
    {
        if (!OkPressed || !IsCompleted || string.IsNullOrWhiteSpace(OutputFileName) || !File.Exists(OutputFileName))
        {
            return false;
        }

        return string.IsNullOrEmpty(currentVideoFileName) ||
               string.Equals(currentVideoFileName, VideoFileName, StringComparison.OrdinalIgnoreCase);
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
            await MessageBox.Show(Window, Se.Language.General.Error, $"Video format '{videoExt}' is not supported (allowed: mp4, mkv, mov, avi, webm, ts, mpg, mpeg, vob).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                await MessageBox.Show(Window, Se.Language.General.Error, $"Audio format '{audioExt}' is not supported (allowed: mp3, aac, m4a, ac3, wav, mka, mp4, mkv, mov, avi, webm, ts, mpg, mpeg, vob).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                await MessageBox.Show(Window, Se.Language.General.Error, $"Subtitle format '{subExt}' is not supported (allowed: srt, ass, ssa, vtt, sub, scc, mcc).", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        // SCC forces .mov, and ffmpeg's mov muxer refuses the VP9/AV1 video of a .webm
        // ("vp9 only supported in MP4"), so the remux could only fail.
        var isMpg = IsMpg(SelectedOutputFormat);
        if (!isMpg && subFiles.Any(IsScc) && string.Equals(videoExt, ".webm", StringComparison.OrdinalIgnoreCase))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Video.RemuxVideoSccNotWithWebM, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // .mpg: the subtitles become CEA-608 captions in the video - field 1 (CC1) and field 2 (CC3)
        if (isMpg && subFiles.Count > 2)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Video.RemuxVideoMpgMaxTwoCaptionTracks, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var requiredFormat = RequiredOutputFormat(out _);
        if (requiredFormat != null && !string.Equals(SelectedOutputFormat, requiredFormat, StringComparison.OrdinalIgnoreCase))
        {
            SelectedOutputFormat = requiredFormat;
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

        var captionFiles = isMpg ? subFiles : new List<RemuxFileItem>();
        var ffmpegOutputFileName = captionFiles.Count > 0
            ? Path.Combine(Path.GetDirectoryName(fullOutput) ?? string.Empty, Path.GetFileNameWithoutExtension(fullOutput) + ".se-temp.mpg")
            : OutputFileName;
        DispatcherTimer? elapsedTimer = null;
        Stopwatch? stopwatch = null;
        try
        {
            double durationSeconds = 0;
            var videoCodec = string.Empty;
            try
            {
                var mediaInfo = await Task.Run(() => FfmpegMediaInfo2.Parse(VideoFileName));
                if (mediaInfo?.Duration != null)
                {
                    durationSeconds = mediaInfo.Duration.TotalSeconds;
                }

                videoCodec = GetCodecName(mediaInfo?.Tracks.FirstOrDefault(t => t.TrackType == FfmpegTrackType.Video)?.TrackInfo);
            }
            catch
            {
                // Fallback if media info parsing fails
            }

            // A/53 captions only exist in MPEG-2 video, and an MPEG program stream holds MPEG video
            ReencodeVideoToMpeg2 = false;
            if (isMpg && videoCodec.Length > 0 && videoCodec != "mpeg2video" && (videoCodec != "mpeg1video" || captionFiles.Count > 0))
            {
                var answer = await MessageBox.Show(Window, Se.Language.Video.RemuxVideoTitle, string.Format(Se.Language.Video.RemuxVideoMpgReencodeVideoX, videoCodec), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }

                ReencodeVideoToMpeg2 = true;
            }

            _progressTracker = new FfmpegProgressTracker(durationSeconds);

            var arguments = BuildFfmpegArguments(audioFiles, subFiles, ffmpegOutputFileName);

            if (PromptForFfmpegParameters)
            {
                var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
                {
                    vm.Initialize("ffmpeg parameters", arguments, 1000, 200);
                });

                if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
                {
                    return;
                }

                arguments = result.Text.Trim();
            }

            Se.SaveSettings();
            arguments = FfmpegProgressTracker.ProgressArguments + " " + arguments;
            IsRemuxing = true;
            IsCompleted = false;
            IsFinalizing = false;
            _isCancelled = false;
            ProgressValue = 0;
            ProgressText = Se.Language.Video.RemuxVideoRemuxing;
            _log.Clear();

            stopwatch = Stopwatch.StartNew();
            elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            elapsedTimer.Tick += (_, _) =>
            {
                if (IsRemuxing)
                {
                    UpdateProgressText(stopwatch.Elapsed);
                }
            };
            elapsedTimer.Start();

            var tcs = new TaskCompletionSource<bool>();

            _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, (_, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                _log.AppendLine(e.Data);

                if (FfmpegProgressTracker.IsFinalizingLine(e.Data))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (tcs.Task.IsCompleted)
                        {
                            return; // stderr can drain after the exit; don't restart the animation
                        }

                        IsFinalizing = true;
                        UpdateProgressText(stopwatch.Elapsed);
                    });
                    return;
                }

                if (_progressTracker != null && _progressTracker.TryGetNewPercent(e.Data, out var pct))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = pct;
                        UpdateProgressText(stopwatch.Elapsed);
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

            // Stop the indeterminate bar now, not in finally: the "file saved" prompt and the
            // error box below are awaited, and the bar kept cycling behind them (#15214).
            IsFinalizing = false;
            elapsedTimer.Stop();
            stopwatch.Stop();
            var totalElapsedStr = RemuxFileItem.FormatDuration(stopwatch.Elapsed);

            if (_isCancelled)
            {
                ProgressText = Se.Language.General.Cancelled;
                DeletePartialOutputFile();
                return;
            }

            var exitCode = _ffmpegProcess.ExitCode;
            if (exitCode == 0 && captionFiles.Count > 0 && File.Exists(ffmpegOutputFileName))
            {
                elapsedTimer.Start();
                stopwatch.Start();
                await AddClosedCaptions(ffmpegOutputFileName, captionFiles, stopwatch);
                elapsedTimer.Stop();
                stopwatch.Stop();
                totalElapsedStr = RemuxFileItem.FormatDuration(stopwatch.Elapsed);
                if (_isCancelled)
                {
                    ProgressText = Se.Language.General.Cancelled;
                    DeletePartialOutputFile();
                    return;
                }
            }

            var fileSuccess = File.Exists(OutputFileName) && new FileInfo(OutputFileName).Length > 0;

            if (exitCode == 0 && fileSuccess)
            {
                ProgressValue = 100;
                ProgressText = $"{Se.Language.Video.RemuxVideoCompleted} ({totalElapsedStr})";
                IsCompleted = true;

                await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
                {
                    vm.Initialize(
                        Se.Language.Video.RemuxVideoTitle,
                        string.Format(Se.Language.General.VideoFileGeneratedX, OutputFileName),
                        OutputFileName,
                        true,
                        false,
                        totalElapsedStr);
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
            IsFinalizing = false;
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
            elapsedTimer?.Stop();
            stopwatch?.Stop();
            IsRemuxing = false;
            IsFinalizing = false;
            _isAddingCaptions = false;
            if (captionFiles.Count > 0)
            {
                DeleteFile(ffmpegOutputFileName);
            }
        }
    }

    /// <summary>
    /// Writes the output .mpg from ffmpeg's <paramref name="programStreamFileName"/> with the
    /// subtitle files as ATSC A/53 closed captions in the MPEG-2 video. The first file is field 1
    /// (CC1) - an .mcc brings all its caption data, CEA-608 field 2 and CEA-708 too; a second file
    /// replaces field 2 (CC3). A .scc/.mcc goes in as it is, other formats are converted to
    /// CEA-608 pop-on captions first.
    /// </summary>
    private async Task AddClosedCaptions(string programStreamFileName, List<RemuxFileItem> captionFiles, Stopwatch stopwatch)
    {
        _isAddingCaptions = true;
        ProgressValue = 0;
        UpdateProgressText(stopwatch.Elapsed);
        var outputFileName = OutputFileName;
        try
        {
            await Task.Run(() =>
            {
                var captions = GetClosedCaptionBytes(captionFiles);
                ProgramStreamClosedCaptionWriter.Write(programStreamFileName, outputFileName, captions, fraction =>
                {
                    if (_isCancelled)
                    {
                        throw new OperationCanceledException();
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressValue = fraction * 100.0;
                        UpdateProgressText(stopwatch.Elapsed);
                    });
                });
            });
        }
        catch (OperationCanceledException) when (_isCancelled)
        {
            // Cancel was pressed - the caller deletes the partial output
        }
        catch
        {
            DeleteFile(outputFileName); // e.g. not MPEG-2 video - no half-written file is left
            throw;
        }
        finally
        {
            _isAddingCaptions = false;
        }
    }

    /// <summary>
    /// The ffmpeg arguments for remuxing the video with <paramref name="audioFiles"/> and
    /// <paramref name="subFiles"/> into <paramref name="outputFileName"/> (default
    /// <see cref="OutputFileName"/>), without the progress arguments. For an .mpg the subtitles
    /// are left out - they are added to the video as closed captions afterwards.
    /// </summary>
    internal string BuildFfmpegArguments(List<RemuxFileItem> audioFiles, List<RemuxFileItem> subFiles, string? outputFileName = null)
    {
        var isMkv = string.Equals(SelectedOutputFormat, ".mkv", StringComparison.OrdinalIgnoreCase);
        var isMpg = IsMpg(SelectedOutputFormat);
        if (isMpg)
        {
            subFiles = new List<RemuxFileItem>();
        }

        var videoFullPath = Path.GetFullPath(VideoFileName);

        var inputArgs = new StringBuilder();
        var mapArgs = new StringBuilder();
        var metadataArgs = new StringBuilder();

        // 1. Video input (index 0). "genpts": H.264 with B-frames in .avi has packets without
        //    a pts, and copying those into .mkv aborts with "Can't write packet with unknown
        //    timestamp". Packets that have a pts are left as they are.
        inputArgs.Append($"-fflags +genpts -i \"{VideoFileName}\" ");
        mapArgs.Append("-map 0:v:0 ");

        // 2. Audio inputs - the video's own audio is mapped from input 0, every other
        //    file becomes its own input; the selected track decides the stream index.
        //    When mixing, each source goes through a volume filter into one amix track.
        var isMixing = IsMixing;
        var filterArgs = string.Empty;
        var mixFilter = new StringBuilder();
        var currentInputIndex = 1;
        for (var k = 0; k < audioFiles.Count; k++)
        {
            var audioFile = audioFiles[k];
            var streamIndex = audioFile.SelectedTrack?.Index ?? 0;
            var isVideoAudio = string.Equals(Path.GetFullPath(audioFile.FileName), videoFullPath, StringComparison.OrdinalIgnoreCase);
            string streamSpecifier;
            if (isVideoAudio)
            {
                streamSpecifier = $"0:a:{streamIndex}";
            }
            else
            {
                inputArgs.Append($"-i \"{audioFile.FileName}\" ");
                streamSpecifier = $"{currentInputIndex}:a:{streamIndex}";
                currentInputIndex++;
            }

            if (isMixing)
            {
                mixFilter.Append($"[{streamSpecifier}]volume={FormatVolumeFactor(audioFile.VolumePercent)}[a{k}];");
                continue;
            }

            mapArgs.Append($"-map {streamSpecifier} ");

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

        if (isMixing)
        {
            for (var k = 0; k < audioFiles.Count; k++)
            {
                mixFilter.Append($"[a{k}]");
            }

            // normalize=0: amix otherwise divides every input by the input count, so each
            // source would play at 1/n of the volume set for it.
            mixFilter.Append($"amix=inputs={audioFiles.Count}:duration=longest:normalize=0[aout]");
            filterArgs = $"-filter_complex \"{mixFilter}\" ";
            mapArgs.Append("-map \"[aout]\" ");

            var mixedTitle = string.Join(" + ", audioFiles.Select(f => Path.GetFileNameWithoutExtension(f.FileName)));
            metadataArgs.Append($"-metadata:s:a:0 title=\"{EscapeFfmpegMetadata(mixedTitle)}\" ");
            var languages = audioFiles
                .Select(f => f.SelectedTrack?.Language)
                .Where(lang => !string.IsNullOrWhiteSpace(lang) && lang != "und")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (languages.Count == 1)
            {
                metadataArgs.Append($"-metadata:s:a:0 language=\"{languages[0]}\" ");
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
            var subLanguage = GetSubtitleLanguageFromFileName(subFile.FileName);
            if (subLanguage != null)
            {
                metadataArgs.Append($"-metadata:s:s:{j} language={subLanguage} ");
            }

            currentInputIndex++;
        }

        // 4. Codecs
        var videoCodec = isMpg && ReencodeVideoToMpeg2
            ? "-c:v mpeg2video -q:v 2"
            : "-c:v copy";

        string audioCodec;
        if (isMpg)
        {
            // An MPEG program stream carries MPEG audio and AC-3 - not AAC, Opus or FLAC.
            audioCodec = !isMixing && audioFiles.All(f => IsMpegProgramStreamAudio(f.SelectedTrack?.Details))
                ? "-c:a copy"
                : "-c:a ac3 -b:a 192k";
        }
        else if (isMixing)
        {
            // Audio coming out of a filter graph cannot be stream-copied.
            audioCodec = "-c:a aac -b:a 192k";
        }
        else if (isMkv)
        {
            audioCodec = "-c:a copy";
        }
        else
        {
            // .mp4/.mov: audio the container cannot hold (PCM or TrueHD in .mp4, Opus/FLAC in .mov,
            // ...) would make ffmpeg fail on "-c:a copy", so it is re-encoded to AAC.
            var hasWav = audioFiles.Any(f => string.Equals(Path.GetExtension(f.FileName), ".wav", StringComparison.OrdinalIgnoreCase));
            var needsReencode = hasWav || audioFiles.Any(f => !CanCopyAudioToMovFamily(f.SelectedTrack?.Details, SelectedOutputFormat));
            audioCodec = needsReencode ? "-c:a aac -b:a 192k" : "-c:a copy";
        }

        var subCodec = string.Empty;
        if (subFiles.Count > 0)
        {
            subCodec = isMkv ? "-c:s copy" : "-c:s mov_text";

            // CEA-608 from .scc is copied as it is into a QuickTime "c608" track.
            if (!isMkv)
            {
                for (var j = 0; j < subFiles.Count; j++)
                {
                    if (IsScc(subFiles[j]))
                    {
                        subCodec += $" -c:s:{j.ToString(CultureInfo.InvariantCulture)} copy";
                    }
                }
            }

            // Matroska has no codec id for MicroDVD, so a text .sub cannot be copied in
            // ("Subtitle codec microdvd is not supported") - it goes in as SubRip. A .sub
            // with an .idx next to it is VobSub, which can be copied.
            if (isMkv)
            {
                for (var j = 0; j < subFiles.Count; j++)
                {
                    var subFileName = subFiles[j].FileName;
                    if (string.Equals(Path.GetExtension(subFileName), ".sub", StringComparison.OrdinalIgnoreCase) &&
                        !File.Exists(Path.ChangeExtension(subFileName, ".idx")))
                    {
                        subCodec += $" -c:s:{j.ToString(CultureInfo.InvariantCulture)} srt";
                    }
                }
            }
        }

        // "+faststart" moves the mp4 index to the front for web streaming, but ffmpeg then has to
        // rewrite the whole file after the last packet - optional, as local players don't need it (#15253).
        var fastStart = FastStart && IsMovFamily(SelectedOutputFormat)
            ? "-movflags +faststart "
            : string.Empty;

        // "vob" is ffmpeg's MPEG-2 program stream muxer - "mpeg" (the .mpg default) writes MPEG-1 packs.
        var muxer = isMpg ? "-f vob " : string.Empty;
        return $"-y {inputArgs}{filterArgs}{mapArgs}{videoCodec} {audioCodec} {subCodec} {metadataArgs}{fastStart}{muxer}\"{outputFileName ?? OutputFileName}\"".Trim();
    }

    /// <summary>
    /// True for ffmpeg track details ("ac3, 48000 Hz, stereo, ...") of audio that an MPEG
    /// program stream can hold as it is.
    /// </summary>
    internal static bool IsMpegProgramStreamAudio(string? trackDetails)
    {
        var codec = GetCodecName(trackDetails);
        return codec is "mp2" or "mp3" or "ac3";
    }

    /// <summary>
    /// False when ffmpeg cannot stream-copy the audio in <paramref name="trackDetails"/> into an
    /// .mp4 or .mov (<paramref name="outputExtension"/>). Unknown details (media info not read)
    /// count as copyable, as before.
    /// </summary>
    internal static bool CanCopyAudioToMovFamily(string? trackDetails, string outputExtension)
    {
        var codec = GetCodecName(trackDetails);
        if (codec.Length == 0)
        {
            return true;
        }

        if (codec is "aac" or "mp3" or "mp2" or "ac3" or "eac3" or "alac" or "dts")
        {
            return true;
        }

        return string.Equals(outputExtension, ".mov", StringComparison.OrdinalIgnoreCase)
            ? codec.StartsWith("pcm_", StringComparison.Ordinal)
            : codec is "opus" or "flac";
    }

    /// <summary>
    /// The codec name at the start of ffmpeg track details ("mpeg2video (Main), yuv420p, ...").
    /// </summary>
    internal static string GetCodecName(string? trackDetails)
    {
        if (string.IsNullOrWhiteSpace(trackDetails))
        {
            return string.Empty;
        }

        var end = trackDetails.IndexOfAny(new[] { ' ', ',', '(' });
        return (end < 0 ? trackDetails : trackDetails.Substring(0, end)).Trim().ToLowerInvariant();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsRemuxing)
        {
            AbortRemux();
            ProgressText = Se.Language.General.Cancelled;
            return;
        }

        Window?.Close();
    }

    /// <summary>
    /// Escape only guards the close while remuxing; the title-bar X does not. Closing the window
    /// mid-remux used to leave ffmpeg running to completion in the background, after which
    /// <see cref="Remux"/> tried to show its "file saved"/error dialog on the closed owner. Same
    /// fix as the re-encode, cut and embedded-subtitles dialogs.
    /// </summary>
    internal void OnClosing()
    {
        if (IsRemuxing)
        {
            AbortRemux();
        }
    }

    private void AbortRemux()
    {
        _isCancelled = true;
        try
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
#pragma warning disable CA1416
                _ffmpegProcess.Kill(true);
#pragma warning restore CA1416
            }
        }
        catch
        {
            // ignore - it may have exited in between
        }

        DeletePartialOutputFile();
    }

    /// <summary>
    /// The caption data of the first file, with CEA-608 field 1 of the second file (if any) as
    /// field 2 - CC1 and CC3.
    /// </summary>
    internal static ClosedCaptionBytes GetClosedCaptionBytes(List<RemuxFileItem> captionFiles)
    {
        var captions = ClosedCaptionBytes.FromFile(captionFiles[0].FileName);
        if (captionFiles.Count > 1)
        {
            captions.Field2 = ClosedCaptionBytes.FromFile(captionFiles[1].FileName).Field1;
        }

        return captions;
    }

    private void DeletePartialOutputFile()
    {
        DeleteFile(OutputFileName);
    }

    private static void DeleteFile(string fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
        {
            try
            {
                File.Delete(fileName);
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/remux-video");
        }
    }
}
