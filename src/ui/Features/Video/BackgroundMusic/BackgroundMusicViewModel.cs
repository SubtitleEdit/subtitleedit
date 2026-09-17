using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.BackgroundMusic;

/// <summary>
/// Video &gt; More &gt; Generate background music: ACE-Step 1.5 (audio.cpp) generates a clip from a
/// prompt, <see cref="MusicLooper"/> loops it seamlessly to the video's length, and ffmpeg puts it
/// on the video — replacing the existing audio by default, or mixed under it.
/// </summary>
public partial class BackgroundMusicViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BackgroundMusicPreset> _presets;
    [ObservableProperty] private BackgroundMusicPreset? _selectedPreset;
    [ObservableProperty] private string _prompt;
    [ObservableProperty] private int _bpm;
    [ObservableProperty] private int _generateSeconds;
    [ObservableProperty] private bool _useRandomSeed;
    [ObservableProperty] private long _seed;
    [ObservableProperty] private int _musicVolumePercent;
    [ObservableProperty] private bool _removeExistingAudioTracks;
    [ObservableProperty] private int _originalAudioVolumePercent;
    [ObservableProperty] private string _outputLengthText;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isNotBusy;
    [ObservableProperty] private bool _hasMusic;
    [ObservableProperty] private bool _canAddToVideo;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _showOriginalAudioVolume;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _resultText;
    [ObservableProperty] private bool _isTextToSpeechMode;
    [ObservableProperty] private bool _isVideoMode;

    public Window? Window { get; set; }

    /// <summary>The last generated clip (un-looped) — handed back to the TTS window so it can reuse it.</summary>
    public GeneratedMusic? Generated { get; private set; }

    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IAceStepAudioCppDownloadService _downloadService;

    private string _videoFileName = string.Empty;
    private double _videoDurationSeconds;
    private bool _videoHasAudio;
    private MusicAudio? _music;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string _tempFolder;
    private readonly DispatcherTimer _statusTimer;
    private readonly Stopwatch _stopwatch = new();
    private double _estimatedRemainingMs = -1;
    private double _estimateMadeAtMs;
    private string _phaseText = string.Empty;
    private readonly Lock _playLock = new();
    private LibMpvDynamicPlayer? _player;
    private bool _isLoadingPreset;

    private static readonly Regex FfmpegTimeRegex = new(@"time=\s*(\d+):(\d+):(\d+(?:\.\d+)?)", RegexOptions.Compiled);

    public BackgroundMusicViewModel(IWindowService windowService, IFileHelper fileHelper, IAceStepAudioCppDownloadService downloadService)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _downloadService = downloadService;

        Presets = new ObservableCollection<BackgroundMusicPreset>(BackgroundMusicPreset.GetAll());
        Prompt = string.Empty;
        OutputLengthText = string.Empty;
        ProgressText = string.Empty;
        ResultText = string.Empty;
        IsNotBusy = true;
        IsVideoMode = true;

        _tempFolder = Path.Combine(Path.GetTempPath(), "SubtitleEdit-BackgroundMusic-" + Guid.NewGuid().ToString("N"));

        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _statusTimer.Tick += (_, _) => OnStatusTimerTick();

        LoadSettings();
    }

    public void Initialize(string? videoFileName)
    {
        _videoFileName = videoFileName ?? string.Empty;
        UpdateOutputLengthText();

        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                var info = FfmpegMediaInfo2.Parse(_videoFileName);
                var duration = info.Duration?.TotalSeconds ?? 0;
                var hasAudio = info.Tracks.Any(t => t.TrackType == FfmpegTrackType.Audio);
                Dispatcher.UIThread.Post(() =>
                {
                    _videoDurationSeconds = duration;
                    _videoHasAudio = hasAudio;
                    UpdateOutputLengthText();
                    UpdateCanAddToVideo();
                });
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "Background music: reading video info failed");
            }
        });
    }

    /// <summary>
    /// Opened from the TTS window's background music settings: the music goes under the dubbed
    /// speech there, so the video-only options are hidden, the volume is the level under speech,
    /// and music generated here is offered back for the TTS run to reuse.
    /// </summary>
    public void InitializeForTextToSpeech(string? videoFileName, GeneratedMusic? previous)
    {
        IsTextToSpeechMode = true;
        IsVideoMode = false;
        ShowOriginalAudioVolume = false;
        MusicVolumePercent = Math.Clamp(Se.Settings.Video.BackgroundMusic.TextToSpeechMusicVolumePercent, 0, 200);
        Initialize(videoFileName);

        if (previous != null && previous.Matches(Prompt, Bpm, GenerateSeconds))
        {
            Generated = previous;
            var target = TargetSeconds;
            Task.Run(() => previous.Render(target)).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _music = t.Result;
                        HasMusic = true;
                    });
                }
            });
        }
    }

    private double TargetSeconds => _videoDurationSeconds > 0 ? _videoDurationSeconds : GenerateSeconds;

    private void UpdateOutputLengthText()
    {
        if (IsTextToSpeechMode)
        {
            OutputLengthText = Se.Language.Video.BackgroundMusic.OutputLengthTextToSpeech;
            return;
        }

        OutputLengthText = _videoDurationSeconds > 0
            ? string.Format(Se.Language.Video.BackgroundMusic.OutputLengthVideoX, FormatDuration(_videoDurationSeconds))
            : FormatDuration(GenerateSeconds);
    }

    private void UpdateCanAddToVideo()
    {
        CanAddToVideo = HasMusic && !IsBusy && _videoDurationSeconds > 0 && !IsTextToSpeechMode;
    }

    private static string FormatDuration(double seconds) =>
        new TimeCode(TimeSpan.FromSeconds(Math.Round(seconds))).ToShortDisplayString();

    partial void OnSelectedPresetChanged(BackgroundMusicPreset? value)
    {
        if (value == null || value.IsCustom || _isLoadingPreset)
        {
            return;
        }

        Prompt = value.Prompt;
        Bpm = value.Bpm;
    }

    partial void OnGenerateSecondsChanged(int value) => UpdateOutputLengthText();

    partial void OnRemoveExistingAudioTracksChanged(bool value) => ShowOriginalAudioVolume = !value && !IsTextToSpeechMode;

    partial void OnIsBusyChanged(bool value)
    {
        IsNotBusy = !value;
        UpdateCanAddToVideo();
    }

    partial void OnHasMusicChanged(bool value) => UpdateCanAddToVideo();

    [RelayCommand]
    private async Task Generate()
    {
        if (Window == null || IsBusy || string.IsNullOrWhiteSpace(Prompt))
        {
            return;
        }

        var l = Se.Language.Video.BackgroundMusic;
        SaveSettings();
        StopPlayback();

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        try
        {
            var installed = await BackgroundMusicGenerator.EnsureInstalledAsync(
                Window,
                _windowService,
                _downloadService,
                () => StartBusy(l.DownloadingModel),
                new Progress<float>(p => SetProgress(p * 100.0)),
                token);
            if (!installed)
            {
                return;
            }
        }
        catch (OperationCanceledException)
        {
            ResultText = Se.Language.General.DownloadCanceled;
            return;
        }
        finally
        {
            StopBusy();
        }

        var seed = UseRandomSeed ? Random.Shared.NextInt64(1, int.MaxValue) : Seed;
        Seed = seed;
        var target = TargetSeconds;
        var generateSeconds = BackgroundMusicGenerator.GetGenerateSeconds(GenerateSeconds, target);

        HasMusic = false;
        _music = null;
        Generated = null;
        ResultText = string.Empty;
        StartBusy(l.LoadingModel);

        try
        {
            var progress = new Progress<MusicGenerationProgress>(p =>
            {
                SetProgress(p.Percent);
                _phaseText = p.Phase switch
                {
                    MusicGenerationPhase.LoadingModel => l.LoadingModel,
                    MusicGenerationPhase.Composing => l.Composing,
                    MusicGenerationPhase.Generating => l.GeneratingAudio,
                    MusicGenerationPhase.Decoding => l.DecodingAudio,
                    _ => l.CreatingLoop,
                };
            });

            var generated = await BackgroundMusicGenerator.GenerateAsync(Prompt, Bpm, generateSeconds, seed, _tempFolder, progress, token);
            _phaseText = l.CreatingLoop;
            var music = await Task.Run(() => generated.Render(target), token);

            Generated = generated;
            _music = music;
            HasMusic = true;
            var loop = generated.Loop;
            ResultText = target > loop.EndSeconds && loop.IsBarAligned
                ? string.Format(l.ResultLoopXYZ,
                    FormatDuration(loop.LengthSeconds),
                    Math.Round(loop.MeasuredBpm).ToString(CultureInfo.CurrentCulture),
                    Math.Round(loop.SeamScore * 100).ToString(CultureInfo.CurrentCulture))
                : string.Format(l.ResultNoLoopX, FormatDuration(music.DurationSeconds));
            Se.WriteToolsLog($"Background music: generated in {_stopwatch.Elapsed.TotalSeconds:0} s, output {music.DurationSeconds:0.0} s");
        }
        catch (OperationCanceledException)
        {
            ResultText = Se.Language.General.Cancelled;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Background music: generation failed");
            await MessageBox.Show(Window, l.UnableToGenerateMusic, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            StopBusy();
        }
    }

    private void StartBusy(string phaseText)
    {
        IsBusy = true;
        ProgressValue = 0;
        _phaseText = phaseText;
        _stopwatch.Restart();
        _estimatedRemainingMs = -1;
        _statusTimer.Start();
    }

    private void StopBusy()
    {
        _stopwatch.Stop();
        IsBusy = false;
        ProgressText = string.Empty;
        ProgressValue = 0;
        if (!IsPlaying)
        {
            _statusTimer.Stop();
        }
    }

    /// <summary>
    /// Updates the bar and re-estimates the time left from the average pace so far. Generation
    /// reports in uneven steps (whole phases, then decode chunks), so the estimate is only renewed
    /// when progress moves and counts down in between.
    /// </summary>
    private void SetProgress(double percent)
    {
        percent = Math.Clamp(percent, 0, 100);
        if (percent <= ProgressValue && _estimatedRemainingMs >= 0)
        {
            return;
        }

        ProgressValue = percent;
        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        if (percent >= 1 && elapsedMs > 2000)
        {
            _estimatedRemainingMs = elapsedMs * (100 - percent) / percent;
            _estimateMadeAtMs = elapsedMs;
        }
    }

    private void OnStatusTimerTick()
    {
        var text = _phaseText;
        if (ProgressValue > 0)
        {
            text += " " + Math.Floor(ProgressValue).ToString(CultureInfo.CurrentCulture) + "%";
        }

        if (_estimatedRemainingMs >= 0)
        {
            var remaining = Math.Max(0, _estimatedRemainingMs - (_stopwatch.Elapsed.TotalMilliseconds - _estimateMadeAtMs));
            text += "     " + ProgressHelper.ToProgressTime(remaining);
        }

        ProgressText = text;

        lock (_playLock)
        {
            if (IsPlaying && (_player == null || _player.IsPaused))
            {
                IsPlaying = false;
            }
        }

        if (!IsBusy && !IsPlaying)
        {
            _statusTimer.Stop();
        }
    }

    /// <summary>The generated music with the current volume applied, as 16-bit WAV.</summary>
    private string WriteMusicWithVolume(string fileName)
    {
        var music = _music ?? throw new InvalidOperationException("No music generated");
        var withVolume = MusicLooper.ApplyGain(music, Math.Clamp(MusicVolumePercent, 0, 200) / 100.0);
        withVolume.WritePcm16Wav(fileName);
        return fileName;
    }

    [RelayCommand]
    private async Task PlayStop()
    {
        if (IsPlaying)
        {
            StopPlayback();
            return;
        }

        if (_music == null)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(_tempFolder);
            var fileName = await Task.Run(() => WriteMusicWithVolume(Path.Combine(_tempFolder, "preview-" + Guid.NewGuid().ToString("N") + ".wav")));

            DisposePlayer();
            LibMpvDynamicPlayer player;
            lock (_playLock)
            {
                player = new LibMpvDynamicPlayer();
                player.LoadLib();
                var err = player.Initialize();
                if (err < 0)
                {
                    throw new InvalidOperationException($"Failed to initialize mpv: {player.GetErrorString(err)}");
                }

                _player = player;
            }

            await player.LoadAudio(fileName);
            IsPlaying = true;
            _statusTimer.Start();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Background music: preview playback failed");
            IsPlaying = false;
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, "Unable to play audio: " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void StopPlayback()
    {
        DisposePlayer();
        IsPlaying = false;
    }

    /// <summary>
    /// mpv is torn down on a worker thread: destroying the core on the UI thread corrupts state
    /// that later crashes when a window closes (#13567, #13376 — same as the TTS window).
    /// </summary>
    private void DisposePlayer()
    {
        LibMpvDynamicPlayer? player;
        lock (_playLock)
        {
            player = _player;
            _player = null;
        }

        if (player == null)
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                player.Stop();
                player.Dispose();
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Background music: disposing the preview player failed");
            }
        });
    }

    [RelayCommand]
    private async Task SaveAudio()
    {
        if (Window == null || _music == null || IsBusy)
        {
            return;
        }

        var l = Se.Language.Video.BackgroundMusic;
        var suggested = string.IsNullOrEmpty(_videoFileName)
            ? "background-music.wav"
            : Path.Combine(Path.GetDirectoryName(_videoFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(_videoFileName) + "_music.wav");
        var fileName = await _fileHelper.PickSaveFile(Window, ".wav", suggested, l.SaveAudioTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            await Task.Run(() => WriteMusicWithVolume(fileName));
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Background music: saving audio failed");
            await MessageBox.Show(Window, Se.Language.General.Error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(l.AudioFileSaved, string.Format(l.AudioFileSavedX, fileName), fileName, true, true);
        });
    }

    [RelayCommand]
    private async Task AddToVideo()
    {
        if (Window == null || _music == null || IsBusy || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var l = Se.Language.Video.BackgroundMusic;
        var extension = Path.GetExtension(_videoFileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".mkv";
        }

        var suggested = Path.Combine(Path.GetDirectoryName(_videoFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(_videoFileName) + "_music" + extension);
        var outputFileName = await _fileHelper.PickSaveFile(Window, extension, suggested, Se.Language.General.SaveVideoAsVideoTitle);
        if (string.IsNullOrEmpty(outputFileName))
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(outputFileName), Path.GetFullPath(_videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            await MessageBox.Show(Window, l.UnableToAddMusicToVideo, "The output file cannot be the input video.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        StopPlayback();

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        IsBusy = true;
        ProgressValue = 0;
        _phaseText = l.AddingMusicToVideo;
        _stopwatch.Restart();
        _estimatedRemainingMs = -1;
        _statusTimer.Start();

        Process? process = null;
        var ffmpegLog = new System.Text.StringBuilder();
        try
        {
            Directory.CreateDirectory(_tempFolder);
            var musicFileName = await Task.Run(() => WriteMusicWithVolume(Path.Combine(_tempFolder, "music.wav")), token);
            var mix = !RemoveExistingAudioTracks && _videoHasAudio;
            var arguments = BuildAddToVideoArguments(_videoFileName, musicFileName, outputFileName, mix, OriginalAudioVolumePercent);

            var duration = _videoDurationSeconds;
            process = FfmpegGenerator.GetProcess(arguments, (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                lock (ffmpegLog)
                {
                    ffmpegLog.AppendLine(e.Data);
                }

                var match = FfmpegTimeRegex.Match(e.Data);
                if (match.Success && duration > 0)
                {
                    var seconds = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * 3600 +
                                  int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) * 60 +
                                  double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                    var percent = Math.Clamp(seconds * 100.0 / duration, 0, 100);
                    Dispatcher.UIThread.Post(() => SetProgress(percent));
                }
            });

            Se.WriteToolsLog("Background music: ffmpeg " + process.StartInfo.Arguments);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0 || !File.Exists(outputFileName))
            {
                string tail;
                lock (ffmpegLog)
                {
                    tail = string.Join(Environment.NewLine, ffmpegLog.ToString().Split('\n').TakeLast(8));
                }

                Se.WriteToolsLog("Background music: ffmpeg failed: " + tail);
                await MessageBox.Show(Window, l.UnableToAddMusicToVideo, tail, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        catch (OperationCanceledException)
        {
            try
            {
                process?.Kill(true);
            }
            catch
            {
                // it may have exited in between
            }

            TryDelete(outputFileName);
            return;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Background music: adding music to video failed");
            await MessageBox.Show(Window, l.UnableToAddMusicToVideo, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        finally
        {
            process?.Dispose();
            _statusTimer.Stop();
            _stopwatch.Stop();
            IsBusy = false;
            ProgressText = string.Empty;
            ProgressValue = 0;
        }

        await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window, vm =>
        {
            vm.Initialize(
                Se.Language.General.VideoFileGenerated,
                string.Format(Se.Language.General.VideoFileGeneratedX, outputFileName),
                outputFileName,
                true,
                true);
        });
    }

    /// <summary>
    /// Video streams are copied. With <paramref name="mix"/> the first audio track is kept at
    /// <paramref name="originalVolumePercent"/> and the music is mixed under it; otherwise the
    /// music replaces every audio track. The music is already rendered to the video's length.
    /// </summary>
    public static string BuildAddToVideoArguments(string videoFileName, string musicFileName, string outputFileName, bool mix, int originalVolumePercent)
    {
        var extension = Path.GetExtension(outputFileName).ToLowerInvariant();
        var audioCodec = extension == ".webm" ? "-c:a libopus -b:a 160k" : "-c:a aac -b:a 192k";

        if (mix)
        {
            var volume = Math.Clamp(originalVolumePercent / 100.0, 0.0, 2.0).ToString("0.00", CultureInfo.InvariantCulture);
            return $"-y -i \"{videoFileName}\" -i \"{musicFileName}\" " +
                   $"-filter_complex \"[0:a:0]volume={volume}[orig];[orig][1:a:0]amix=inputs=2:duration=first:normalize=0[aout]\" " +
                   $"-map 0:v -map \"[aout]\" -c:v copy {audioCodec} \"{outputFileName}\"";
        }

        return $"-y -i \"{videoFileName}\" -i \"{musicFileName}\" -map 0:v -map 1:a:0 -c:v copy {audioCodec} \"{outputFileName}\"";
    }

    [RelayCommand]
    private void Ok()
    {
        if (IsBusy)
        {
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsBusy)
        {
            _cancellationTokenSource?.Cancel();
            return;
        }

        Window?.Close();
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.BackgroundMusic;
        _isLoadingPreset = true;
        SelectedPreset = Presets.FirstOrDefault(p => p.Key == settings.Preset) ?? Presets[0];
        _isLoadingPreset = false;

        Prompt = string.IsNullOrWhiteSpace(settings.Prompt) ? SelectedPreset.Prompt : settings.Prompt;
        Bpm = Math.Clamp(settings.Bpm > 0 ? settings.Bpm : SelectedPreset.Bpm, 40, 200);
        GenerateSeconds = Math.Clamp(settings.GenerateSeconds, 20, 240);
        UseRandomSeed = settings.UseRandomSeed;
        Seed = settings.Seed;
        MusicVolumePercent = Math.Clamp(settings.MusicVolumePercent, 0, 200);
        RemoveExistingAudioTracks = settings.RemoveExistingAudioTracks;
        ShowOriginalAudioVolume = !RemoveExistingAudioTracks;
        OriginalAudioVolumePercent = Math.Clamp(settings.OriginalAudioVolumePercent, 0, 200);
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.BackgroundMusic;
        settings.Preset = SelectedPreset?.Key ?? BackgroundMusicPreset.CustomKey;
        settings.Prompt = Prompt;
        settings.Bpm = Bpm;
        settings.GenerateSeconds = GenerateSeconds;
        settings.UseRandomSeed = UseRandomSeed;
        settings.Seed = Seed;
        if (IsTextToSpeechMode)
        {
            settings.TextToSpeechMusicVolumePercent = MusicVolumePercent;
        }
        else
        {
            settings.MusicVolumePercent = MusicVolumePercent;
        }

        settings.RemoveExistingAudioTracks = RemoveExistingAudioTracks;
        settings.OriginalAudioVolumePercent = OriginalAudioVolumePercent;
        Se.SaveSettings();
    }

    internal void OnClosing()
    {
        SaveSettings();
        _cancellationTokenSource?.Cancel();
        _statusTimer.Stop();
        DisposePlayer();

        var folder = _tempFolder;
        Task.Run(async () =>
        {
            // The preview player is released on a worker thread; give it a moment before its
            // file is removed (Windows keeps open files locked).
            await Task.Delay(2000);
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
            catch
            {
                // best effort
            }
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }

    private static void TryDelete(string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch
        {
            // best effort
        }
    }
}
