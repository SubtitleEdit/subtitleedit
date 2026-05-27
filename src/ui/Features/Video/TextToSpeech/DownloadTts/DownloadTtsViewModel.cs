using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Nikse.SubtitleEdit.Core.AudioToText.SpeechToTextPostProcessor;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;

public partial class DownloadTtsViewModel : ObservableObject
{
    internal static IReadOnlyList<(string SourceFilePattern, string LinkFileName)> PiperLinuxSymbolicLinks { get; } =
        new[]
        {
            ("libpiper_phonemize.so.1.*", "libpiper_phonemize.so.1"),
            ("libespeak-ng.so.1.*", "libespeak-ng.so.1"),
        };

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    [ObservableProperty] private string _titleText;
    [ObservableProperty] private float _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    private Task? _downloadTask;
    private Task? _downloadTaskVoiceModel;
    private Task? _downloadTaskVoiceConfig;
    private Task? _downloadTaskQwen3TtsCpp;
    private Task? _downloadTaskQwen3TtsCppVoices;
    private Task? _downloadTaskQwen3TtsModels;
    private Task? _downloadTaskKokoroTtsCpp;
    private Task? _downloadTaskKokoroTtsModels;
    private Task? _downloadTaskChatterboxModels;
    private Task? _downloadTaskQwen3TtsCrispAsrModels;
    private Task? _downloadTaskQwen3TtsCrispAsrVoices;
    private Task? _downloadTaskVibeVoiceCrispAsrModels;
    private Task? _downloadTaskVibeVoiceCrispAsrVoices;
    private Task? _downloadTaskIndexTtsCrispAsrModels;
    private Task? _downloadTaskIndexTtsCrispAsrVoices;
    private Task? _downloadTaskOmniVoice;
    private Task? _downloadTaskOmniVoices;
    private Task? _downloadTaskOmniVoiceModels;
    private string _omniVoiceVariant = OmniVoiceDownloadService.WindowsVariantVulkan;
    private string _qwen3TtsCppVariant = Qwen3TtsCppDownloadService.WindowsVariantVulkan;
    private readonly Timer _timer = new();

    private readonly ITtsDownloadService _ttsDownloadService;
    private readonly IQwen3TtsCppDownloadService _qwen3TtsCppDownloadService;
    private readonly IKokoroTtsCppDownloadService _kokoroTtsCppDownloadService;
    private readonly IChatterboxTtsCppDownloadService _chatterboxTtsCppDownloadService;
    private readonly IQwen3TtsCrispAsrDownloadService _qwen3TtsCrispAsrDownloadService;
    private readonly IVibeVoiceCrispAsrDownloadService _vibeVoiceCrispAsrDownloadService;
    private readonly IIndexTtsCrispAsrDownloadService _indexTtsCrispAsrDownloadService;
    private readonly IOmniVoiceDownloadService _omniVoiceDownloadService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;
    private readonly MemoryStream _downloadStreamModel;
    private readonly MemoryStream _downloadStreamConfig;
    private readonly MemoryStream _downloadStreamQwen3TtsCpp;
    private readonly MemoryStream _downloadStreamQwen3TtsCppVoices;
    private readonly MemoryStream _downloadStreamKokoroTtsCpp;
    private readonly MemoryStream _downloadStreamQwen3TtsCrispAsrVoices;
    private readonly MemoryStream _downloadStreamVibeVoiceCrispAsrVoices;
    private readonly MemoryStream _downloadStreamIndexTtsCrispAsrVoices;
    private readonly MemoryStream _downloadStreamOmniVoice;
    private readonly MemoryStream _downloadStreamOmniVoices;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly object _lock = new();
    private string _modelFileName;
    private string _configFileName;

    public DownloadTtsViewModel(ITtsDownloadService ttsDownloadService, IZipUnpacker zipUnpacker,
        IQwen3TtsCppDownloadService qwen3TtsCppDownloadService,
        IKokoroTtsCppDownloadService kokoroTtsCppDownloadService,
        IChatterboxTtsCppDownloadService chatterboxTtsCppDownloadService,
        IQwen3TtsCrispAsrDownloadService qwen3TtsCrispAsrDownloadService,
        IVibeVoiceCrispAsrDownloadService vibeVoiceCrispAsrDownloadService,
        IIndexTtsCrispAsrDownloadService indexTtsCrispAsrDownloadService,
        IOmniVoiceDownloadService omniVoiceDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
        _qwen3TtsCppDownloadService = qwen3TtsCppDownloadService;
        _kokoroTtsCppDownloadService = kokoroTtsCppDownloadService;
        _chatterboxTtsCppDownloadService = chatterboxTtsCppDownloadService;
        _qwen3TtsCrispAsrDownloadService = qwen3TtsCrispAsrDownloadService;
        _vibeVoiceCrispAsrDownloadService = vibeVoiceCrispAsrDownloadService;
        _indexTtsCrispAsrDownloadService = indexTtsCrispAsrDownloadService;
        _omniVoiceDownloadService = omniVoiceDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();
        _downloadStreamModel = new MemoryStream();
        _downloadStreamConfig = new MemoryStream();
        _downloadStreamQwen3TtsCpp = new MemoryStream();
        _downloadStreamQwen3TtsCppVoices = new MemoryStream();
        _downloadStreamKokoroTtsCpp = new MemoryStream();
        _downloadStreamQwen3TtsCrispAsrVoices = new MemoryStream();
        _downloadStreamVibeVoiceCrispAsrVoices = new MemoryStream();
        _downloadStreamIndexTtsCrispAsrVoices = new MemoryStream();
        _downloadStreamOmniVoice = new MemoryStream();
        _downloadStreamOmniVoices = new MemoryStream();

        _modelFileName = string.Empty;
        _configFileName = string.Empty;
        TitleText = string.Empty;

        ProgressText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;

        _timer.Interval = 500;
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lock)
        {
            if (!_timer.Enabled)
            {
                return;
            }

            if (_downloadTask is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStream.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var folder = Piper.GetSetPiperFolder();

                try
                {
                    _downloadStream.Position = 0;
                    _zipUnpacker.UnpackZipStream(_downloadStream, folder, "piper", false, new List<string>(), null);
                    WriteInstalledHashSidecar(folder, _downloadStream, DownloadHashManager.ResolvePiperKey());
                    _downloadStream.Dispose();
                }
                catch (Exception ex)
                {
                    ProgressText = "Unpack failed: " + ex.Message;
                    Error = ex.Message;
                    Se.LogError(ex);
                    return;
                }

                var path = Piper.GetPiperExecutableFileName();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (File.Exists(path))
                    {
                        LinuxHelper.MakeExecutable(path);
                        FixSymbolicLink(path);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (File.Exists(path))
                    {
                        MacHelper.MakeExecutable(path);
                    }
                }

                OkPressed = true;
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskVoiceModel is { IsCompleted: true } && _downloadTaskVoiceConfig is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamModel.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                _downloadStreamModel.Position = 0;
                File.WriteAllBytes(_modelFileName, _downloadStreamModel.ToArray());
                _downloadStreamModel.Dispose();

                if (_downloadStreamConfig.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                _downloadStreamConfig.Position = 0;
                File.WriteAllBytes(_configFileName, _downloadStreamConfig.ToArray());
                _downloadStreamConfig.Dispose();
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskVoiceModel is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskVoiceModel.Exception?.InnerException ?? _downloadTaskVoiceModel.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskVoiceConfig is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskVoiceConfig.Exception?.InnerException ?? _downloadTaskVoiceConfig.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskQwen3TtsCpp is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamQwen3TtsCpp.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var folder = Qwen3TtsCpp.GetSetFolder();
                try
                {
                    _downloadStreamQwen3TtsCpp.Position = 0;
                    _zipUnpacker.UnpackZipStream(_downloadStreamQwen3TtsCpp, folder, string.Empty, false, new List<string>(), null);
                    WriteInstalledHashSidecar(folder, _downloadStreamQwen3TtsCpp, DownloadHashManager.ResolveQwen3TtsCppKey(_qwen3TtsCppVariant));
                    _downloadStreamQwen3TtsCpp.Dispose();
                }
                catch (Exception ex)
                {
                    ProgressText = "Unpack failed: " + ex.Message;
                    Error = ex.Message;
                    Se.LogError(ex);
                    return;
                }

                var exePath = Qwen3TtsCpp.GetExecutableFileName();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (File.Exists(exePath))
                    {
                        LinuxHelper.MakeExecutable(exePath);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (File.Exists(exePath))
                    {
                        MacHelper.MakeExecutable(exePath);
                    }
                }

                _downloadTaskQwen3TtsCpp = null;

                // Chain the voices download, unless the user already has voices installed.
                var voicesFolder = Qwen3TtsCpp.GetSetVoicesFolder();
                var voicesAlreadyInstalled = Directory.Exists(voicesFolder) &&
                                             Directory.EnumerateFiles(voicesFolder, "*.wav").Any();
                if (voicesAlreadyInstalled)
                {
                    OkPressed = true;
                    Close();
                    return;
                }

                TitleText = "Downloading Qwen3 TTS voices";
                ProgressValue = 0;
                ProgressText = Se.Language.General.StartingDotDotDot;
                var voicesProgress = new Progress<float>(number =>
                {
                    var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                    var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                    ProgressValue = percentage;
                    ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
                });
                _downloadTaskQwen3TtsCppVoices = _qwen3TtsCppDownloadService.DownloadVoices(
                    _downloadStreamQwen3TtsCppVoices, voicesProgress, _cancellationTokenSource.Token);
                _timer.Start();
            }
            else if (_downloadTaskQwen3TtsCpp is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskQwen3TtsCpp.Exception?.InnerException ?? _downloadTaskQwen3TtsCpp.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskQwen3TtsCppVoices is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamQwen3TtsCppVoices.Length > 0)
                {
                    var voicesFolder = Qwen3TtsCpp.GetSetVoicesFolder();
                    try
                    {
                        _downloadStreamQwen3TtsCppVoices.Position = 0;
                        _zipUnpacker.UnpackZipStream(_downloadStreamQwen3TtsCppVoices, voicesFolder, string.Empty, false, new List<string>(), null);
                        WriteInstalledHashSidecar(voicesFolder, _downloadStreamQwen3TtsCppVoices, DownloadHashManager.Qwen3TtsCpp.Voices);
                    }
                    catch (Exception ex)
                    {
                        // Voices are optional; log and continue so the engine is still usable.
                        Se.LogError(ex);
                    }
                    _downloadStreamQwen3TtsCppVoices.Dispose();
                }

                OkPressed = true;
                Close();
            }
            else if (_downloadTaskQwen3TtsCppVoices is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskQwen3TtsCppVoices.Exception?.InnerException ?? _downloadTaskQwen3TtsCppVoices.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                    return;
                }

                // Voices are optional — the engine is already installed. Log and close with success.
                if (ex != null)
                {
                    Se.LogError(ex);
                }
                OkPressed = true;
                Close();
            }

            if (_downloadTaskQwen3TtsModels is { IsCompleted: true })
            {
                _timer.Stop();
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskQwen3TtsModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskQwen3TtsModels.Exception?.InnerException ?? _downloadTaskQwen3TtsModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskKokoroTtsCpp is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamKokoroTtsCpp.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var folder = KokoroTtsCpp.GetSetFolder();
                try
                {
                    _downloadStreamKokoroTtsCpp.Position = 0;
                    _zipUnpacker.UnpackZipStream(_downloadStreamKokoroTtsCpp, folder, string.Empty, false, new List<string>(), null);
                    WriteInstalledHashSidecar(folder, _downloadStreamKokoroTtsCpp, DownloadHashManager.ResolveKokoroTtsCppKey());
                    _downloadStreamKokoroTtsCpp.Dispose();
                }
                catch (Exception ex)
                {
                    ProgressText = "Unpack failed: " + ex.Message;
                    Error = ex.Message;
                    Se.LogError(ex);
                    return;
                }

                var exePath = KokoroTtsCpp.GetExecutableFileName();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (File.Exists(exePath))
                    {
                        LinuxHelper.MakeExecutable(exePath);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (File.Exists(exePath))
                    {
                        MacHelper.MakeExecutable(exePath);
                    }
                }

                _downloadTaskKokoroTtsCpp = null;
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskKokoroTtsCpp is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskKokoroTtsCpp.Exception?.InnerException ?? _downloadTaskKokoroTtsCpp.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskKokoroTtsModels is { IsCompleted: true })
            {
                _timer.Stop();
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskKokoroTtsModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskKokoroTtsModels.Exception?.InnerException ?? _downloadTaskKokoroTtsModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskChatterboxModels is { IsCompleted: true })
            {
                _timer.Stop();
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskChatterboxModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskChatterboxModels.Exception?.InnerException ?? _downloadTaskChatterboxModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskQwen3TtsCrispAsrModels is { IsCompleted: true })
            {
                _timer.Stop();
                _downloadTaskQwen3TtsCrispAsrModels = null;

                // Chain the voices download, unless the user already has voices installed.
                // Qwen3TtsCrispAsr.GetSetVoicesFolder lazily seeds from qwen3-tts.cpp's
                // voices folder if any exist there, so users who already have them get
                // skipped automatically.
                var voicesFolder = Qwen3TtsCrispAsr.GetSetVoicesFolder();
                var voicesAlreadyInstalled = Directory.Exists(voicesFolder) &&
                                             Directory.EnumerateFiles(voicesFolder, "*.wav").Any();
                if (voicesAlreadyInstalled)
                {
                    OkPressed = true;
                    Close();
                    return;
                }

                TitleText = "Downloading Qwen3 TTS (CrispASR) voices";
                ProgressValue = 0;
                ProgressText = Se.Language.General.StartingDotDotDot;
                var voicesProgress = new Progress<float>(number =>
                {
                    var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                    var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                    ProgressValue = percentage;
                    ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
                });
                // Same voices.zip as qwen3-tts.cpp (24 kHz mono WAV + .txt sidecars,
                // identical format). Reuse the Qwen3 TTS .cpp download service rather
                // than adding a second copy of the URL/hash.
                _downloadTaskQwen3TtsCrispAsrVoices = _qwen3TtsCppDownloadService.DownloadVoices(
                    _downloadStreamQwen3TtsCrispAsrVoices, voicesProgress, _cancellationTokenSource.Token);
                _timer.Start();
            }
            else if (_downloadTaskQwen3TtsCrispAsrModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskQwen3TtsCrispAsrModels.Exception?.InnerException ?? _downloadTaskQwen3TtsCrispAsrModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskQwen3TtsCrispAsrVoices is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamQwen3TtsCrispAsrVoices.Length > 0)
                {
                    var voicesFolder = Qwen3TtsCrispAsr.GetSetVoicesFolder();
                    try
                    {
                        _downloadStreamQwen3TtsCrispAsrVoices.Position = 0;
                        _zipUnpacker.UnpackZipStream(_downloadStreamQwen3TtsCrispAsrVoices, voicesFolder, string.Empty, false, new List<string>(), null);
                        WriteInstalledHashSidecar(voicesFolder, _downloadStreamQwen3TtsCrispAsrVoices, DownloadHashManager.Qwen3TtsCpp.Voices);
                    }
                    catch (Exception ex)
                    {
                        // Voices are optional; log and continue so the engine is still usable.
                        Se.LogError(ex);
                    }
                    _downloadStreamQwen3TtsCrispAsrVoices.Dispose();
                }

                OkPressed = true;
                Close();
            }
            else if (_downloadTaskQwen3TtsCrispAsrVoices is { IsFaulted: true })
            {
                _timer.Stop();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    ProgressText = "Download canceled";
                    Close();
                    return;
                }

                // Voices are optional — the models are already installed. Log and close with success.
                var ex = _downloadTaskQwen3TtsCrispAsrVoices.Exception?.InnerException ?? _downloadTaskQwen3TtsCrispAsrVoices.Exception;
                if (ex != null)
                {
                    Se.LogError(ex);
                }
                OkPressed = true;
                Close();
            }

            if (_downloadTaskVibeVoiceCrispAsrModels is { IsCompleted: true })
            {
                _timer.Stop();
                _downloadTaskVibeVoiceCrispAsrModels = null;

                // Chain voices download if the engine's folder is still empty after the
                // lazy seed from qwen3-tts.cpp (user never installed Qwen3, etc.). Same
                // voices.zip the Qwen3 (CrispASR) flow uses — single source of truth.
                var voicesFolder = VibeVoiceCrispAsr.GetSetVoicesFolder();
                var voicesAlreadyInstalled = Directory.Exists(voicesFolder)
                    && Directory.EnumerateFiles(voicesFolder, "*.wav").Any();
                if (voicesAlreadyInstalled)
                {
                    OkPressed = true;
                    Close();
                    return;
                }

                TitleText = "Downloading VibeVoice (CrispASR) voices";
                ProgressValue = 0;
                ProgressText = Se.Language.General.StartingDotDotDot;
                var voicesProgress = new Progress<float>(number =>
                {
                    var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                    var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                    ProgressValue = percentage;
                    ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
                });
                _downloadTaskVibeVoiceCrispAsrVoices = _qwen3TtsCppDownloadService.DownloadVoices(
                    _downloadStreamVibeVoiceCrispAsrVoices, voicesProgress, _cancellationTokenSource.Token);
                _timer.Start();
            }
            else if (_downloadTaskVibeVoiceCrispAsrModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskVibeVoiceCrispAsrModels.Exception?.InnerException ?? _downloadTaskVibeVoiceCrispAsrModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskVibeVoiceCrispAsrVoices is { IsCompleted: true })
            {
                _timer.Stop();
                if (_downloadStreamVibeVoiceCrispAsrVoices.Length > 0)
                {
                    var voicesFolder = VibeVoiceCrispAsr.GetSetVoicesFolder();
                    try
                    {
                        _downloadStreamVibeVoiceCrispAsrVoices.Position = 0;
                        _zipUnpacker.UnpackZipStream(_downloadStreamVibeVoiceCrispAsrVoices, voicesFolder, string.Empty, false, new List<string>(), null);
                        // Voice pack ships at 16 kHz; resample to 24 kHz so VibeVoice's
                        // server doesn't have to upsample on every synth call.
                        ResampleVoicesTo24kHz(voicesFolder);
                    }
                    catch (Exception ex)
                    {
                        // Voices are optional — log and continue so the engine is still usable.
                        Se.LogError(ex);
                    }
                    _downloadStreamVibeVoiceCrispAsrVoices.Dispose();
                }
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskVibeVoiceCrispAsrVoices is { IsFaulted: true })
            {
                _timer.Stop();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    ProgressText = "Download canceled";
                    Close();
                    return;
                }
                // Voices are optional — log and close with success.
                var ex = _downloadTaskVibeVoiceCrispAsrVoices.Exception?.InnerException ?? _downloadTaskVibeVoiceCrispAsrVoices.Exception;
                if (ex != null) Se.LogError(ex);
                OkPressed = true;
                Close();
            }

            if (_downloadTaskIndexTtsCrispAsrModels is { IsCompleted: true })
            {
                _timer.Stop();
                _downloadTaskIndexTtsCrispAsrModels = null;

                var voicesFolder = IndexTtsCrispAsr.GetSetVoicesFolder();
                var voicesAlreadyInstalled = Directory.Exists(voicesFolder)
                    && Directory.EnumerateFiles(voicesFolder, "*.wav").Any();
                if (voicesAlreadyInstalled)
                {
                    OkPressed = true;
                    Close();
                    return;
                }

                TitleText = "Downloading IndexTTS (CrispASR) voices";
                ProgressValue = 0;
                ProgressText = Se.Language.General.StartingDotDotDot;
                var voicesProgress = new Progress<float>(number =>
                {
                    var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                    var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                    ProgressValue = percentage;
                    ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
                });
                _downloadTaskIndexTtsCrispAsrVoices = _qwen3TtsCppDownloadService.DownloadVoices(
                    _downloadStreamIndexTtsCrispAsrVoices, voicesProgress, _cancellationTokenSource.Token);
                _timer.Start();
            }
            else if (_downloadTaskIndexTtsCrispAsrModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskIndexTtsCrispAsrModels.Exception?.InnerException ?? _downloadTaskIndexTtsCrispAsrModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskIndexTtsCrispAsrVoices is { IsCompleted: true })
            {
                _timer.Stop();
                if (_downloadStreamIndexTtsCrispAsrVoices.Length > 0)
                {
                    var voicesFolder = IndexTtsCrispAsr.GetSetVoicesFolder();
                    try
                    {
                        _downloadStreamIndexTtsCrispAsrVoices.Position = 0;
                        _zipUnpacker.UnpackZipStream(_downloadStreamIndexTtsCrispAsrVoices, voicesFolder, string.Empty, false, new List<string>(), null);
                        ResampleVoicesTo24kHz(voicesFolder);
                    }
                    catch (Exception ex)
                    {
                        Se.LogError(ex);
                    }
                    _downloadStreamIndexTtsCrispAsrVoices.Dispose();
                }
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskIndexTtsCrispAsrVoices is { IsFaulted: true })
            {
                _timer.Stop();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    ProgressText = "Download canceled";
                    Close();
                    return;
                }
                var ex = _downloadTaskIndexTtsCrispAsrVoices.Exception?.InnerException ?? _downloadTaskIndexTtsCrispAsrVoices.Exception;
                if (ex != null) Se.LogError(ex);
                OkPressed = true;
                Close();
            }

            if (_downloadTaskOmniVoice is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamOmniVoice.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var folder = OmniVoiceTtsCpp.GetSetFolder();
                try
                {
                    _downloadStreamOmniVoice.Position = 0;
                    // The macOS and Linux OmniVoice zips wrap the binaries in a top-level
                    // folder (e.g. "omnivoice-macos-universal-cpu-metal/"), while the Windows
                    // zips are flat. Flatten on non-Windows so the binaries land directly in
                    // the OmniVoice folder where GetExecutableFileName() looks for them.
                    var flatten = !Configuration.IsRunningOnWindows;
                    _zipUnpacker.UnpackZipStream(_downloadStreamOmniVoice, folder, string.Empty, flatten, new List<string>(), null);
                    WriteInstalledHashSidecar(folder, _downloadStreamOmniVoice, DownloadHashManager.ResolveOmniVoiceKey(_omniVoiceVariant));
                    _downloadStreamOmniVoice.Dispose();
                }
                catch (Exception ex)
                {
                    ProgressText = "Unpack failed: " + ex.Message;
                    Error = ex.Message;
                    Se.LogError(ex);
                    return;
                }

                var exePath = OmniVoiceTtsCpp.GetExecutableFileName();
                var codecExePath = OmniVoiceTtsCpp.GetCodecExecutableFileName();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (File.Exists(exePath))
                    {
                        LinuxHelper.MakeExecutable(exePath);
                    }
                    if (File.Exists(codecExePath))
                    {
                        LinuxHelper.MakeExecutable(codecExePath);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (File.Exists(exePath))
                    {
                        MacHelper.MakeExecutable(exePath);
                    }
                    if (File.Exists(codecExePath))
                    {
                        MacHelper.MakeExecutable(codecExePath);
                    }
                }

                _downloadTaskOmniVoice = null;

                // Chain the voices download, unless the user already has voices installed.
                var voicesFolder = OmniVoiceTtsCpp.GetSetVoicesFolder();
                var voicesAlreadyInstalled = Directory.Exists(voicesFolder) &&
                                             Directory.EnumerateFiles(voicesFolder, "*.wav").Any();
                if (voicesAlreadyInstalled)
                {
                    OkPressed = true;
                    Close();
                    return;
                }

                TitleText = "Downloading OmniVoice TTS voices";
                ProgressValue = 0;
                ProgressText = Se.Language.General.StartingDotDotDot;
                var voicesProgress = new Progress<float>(number =>
                {
                    var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                    var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                    ProgressValue = percentage;
                    ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
                });
                _downloadTaskOmniVoices = _omniVoiceDownloadService.DownloadVoices(
                    _downloadStreamOmniVoices, voicesProgress, _cancellationTokenSource.Token);
                _timer.Start();
            }
            else if (_downloadTaskOmniVoice is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskOmniVoice.Exception?.InnerException ?? _downloadTaskOmniVoice.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }

            if (_downloadTaskOmniVoices is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamOmniVoices.Length > 0)
                {
                    var voicesFolder = OmniVoiceTtsCpp.GetSetVoicesFolder();
                    try
                    {
                        _downloadStreamOmniVoices.Position = 0;
                        _zipUnpacker.UnpackZipStream(_downloadStreamOmniVoices, voicesFolder, string.Empty, false, new List<string>(), null);
                        WriteInstalledHashSidecar(voicesFolder, _downloadStreamOmniVoices, DownloadHashManager.OmniVoice.Voices);
                    }
                    catch (Exception ex)
                    {
                        // Voices are optional; log and continue so the engine is still usable.
                        Se.LogError(ex);
                    }
                    _downloadStreamOmniVoices.Dispose();
                }

                OkPressed = true;
                Close();
            }
            else if (_downloadTaskOmniVoices is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskOmniVoices.Exception?.InnerException ?? _downloadTaskOmniVoices.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                    return;
                }

                // Voices are optional — the engine is already installed. Log and close with success.
                if (ex != null)
                {
                    Se.LogError(ex);
                }
                OkPressed = true;
                Close();
            }

            if (_downloadTaskOmniVoiceModels is { IsCompleted: true })
            {
                _timer.Stop();
                OkPressed = true;
                Close();
            }
            else if (_downloadTaskOmniVoiceModels is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskOmniVoiceModels.Exception?.InnerException ?? _downloadTaskOmniVoiceModels.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }
        }
    }

    private static void FixSymbolicLink(string path)
    {
        var folder = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(folder))
        {
            Se.LogError("FixSymbolicLink: Failed to get folder from path: " + path);
            return;
        }

        foreach (var link in PiperLinuxSymbolicLinks)
        {
            var sourcePath = FindSymbolicLinkSource(folder, link.SourceFilePattern);
            if (sourcePath == null)
            {
                Se.LogError("Source library file not found: " + Path.Combine(folder, link.SourceFilePattern));
                continue;
            }

            var linkPath = Path.Combine(folder, link.LinkFileName);
            CreateSymbolicLink(sourcePath, linkPath);
        }
    }

    internal static string? FindSymbolicLinkSource(string folder, string sourceFilePattern)
    {
        return Directory
            .GetFiles(folder, sourceFilePattern)
            .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static void CreateSymbolicLink(string sourcePath, string linkPath)
    {
        try
        {
            var processStartInfo = CreateSymbolicLinkProcessStartInfo(sourcePath, linkPath);
            using var process = Process.Start(processStartInfo);

            if (process == null)
            {
                Se.LogError("Error creating symlink: Could not start /bin/bash");
                return;
            }

            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Se.LogError($"Error creating symlink: {error}");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex);
        }
    }

    internal static ProcessStartInfo CreateSymbolicLinkProcessStartInfo(string sourcePath, string linkPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        processStartInfo.ArgumentList.Add("-c");
        processStartInfo.ArgumentList.Add($"ln -sfn -- {QuoteForBash(sourcePath)} {QuoteForBash(linkPath)}");
        return processStartInfo;
    }

    private static string QuoteForBash(string value)
    {
        return "'" + value.Replace("'", "'\"'\"'") + "'";
    }

    // In-place resample of every WAV in <paramref name="folder"/> to 24 kHz mono. The shared
    // qwen3-tts.cpp voice pack ships at 16 kHz; VibeVoice and IndexTTS clone at 24 kHz so
    // crispasr would otherwise upsample on every synth (audibly lossy and a bit slower).
    // Best-effort per file: on ffmpeg failure we leave the original 16 kHz WAV in place
    // rather than dropping the voice entirely.
    private static void ResampleVoicesTo24kHz(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        foreach (var wav in Directory.GetFiles(folder, "*.wav"))
        {
            var temp = wav + ".24k.wav";
            try
            {
                var ffmpeg = FfmpegGenerator.ConvertToMono24kHzWav(wav, temp);
                if (!ffmpeg.Start())
                {
                    continue;
                }
                ffmpeg.WaitForExit();
                if (File.Exists(temp) && new FileInfo(temp).Length > 0)
                {
                    File.Delete(wav);
                    File.Move(temp, wav);
                }
            }
            catch (Exception ex)
            {
                Se.LogError(ex, $"Resample voice to 24 kHz failed for '{wav}'; leaving original in place");
                try { if (File.Exists(temp)) File.Delete(temp); } catch { }
            }
        }
    }

    // Records the downloaded archive's hash in a .installed.sha256 sidecar so SE can later tell
    // whether the install is outdated (see DownloadHashManager.GetStatus). Best-effort - failure
    // is swallowed because the engine itself is already on disk and usable without the sidecar.
    private static void WriteInstalledHashSidecar(string folder, Stream archiveStream, string? key)
    {
        try
        {
            if (string.IsNullOrEmpty(key) || archiveStream.Length == 0)
            {
                return;
            }

            archiveStream.Position = 0;
            var hash = DownloadHashManager.ComputeSha256(archiveStream);

            var sidecar = Path.Combine(folder, ".installed.sha256");
            File.WriteAllText(sidecar, key + Environment.NewLine + hash);
        }
        catch
        {
            // ignore - hash sidecar is best-effort
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Invoke(() => { Window?.Close(); });
    }

    [RelayCommand]
    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        Close();
    }

    public void StartDownloadPiper()
    {
        TitleText = "Downloading Piper";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        _downloadTask =
            _ttsDownloadService.DownloadPiper(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadPiperVoice(PiperVoice piperVoice)
    {
        TitleText = $"Downloading voice: {piperVoice.Voice}";

        var folder = Piper.GetSetPiperFolder();
        _modelFileName = Path.Combine(folder, piperVoice.ModelShort);
        _configFileName = Path.Combine(folder, piperVoice.ConfigShort);

        var modelUrl = piperVoice.Model;
        var configUrl = piperVoice.Config;

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });
        var downloadProgressNull = new Progress<float>(_ => { });

        _downloadTaskVoiceModel = _ttsDownloadService.DownloadPiperVoice(modelUrl, _downloadStreamModel,
            downloadProgress, _cancellationTokenSource.Token);
        _downloadTaskVoiceConfig = _ttsDownloadService.DownloadPiperVoice(configUrl, _downloadStreamConfig,
            downloadProgressNull, _cancellationTokenSource.Token);
    }

    public void StartDownloadQwen3TtsCpp(string windowsVariant = Qwen3TtsCppDownloadService.WindowsVariantVulkan)
    {
        _qwen3TtsCppVariant = windowsVariant;

        TitleText = Configuration.IsRunningOnWindows
            ? $"Downloading Qwen3 TTS ({windowsVariant})"
            : "Downloading Qwen3 TTS";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        _downloadTaskQwen3TtsCpp =
            _qwen3TtsCppDownloadService.DownloadEngine(_downloadStreamQwen3TtsCpp, windowsVariant, downloadProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadQwen3TtsModels(string? modelKey = null)
    {
        var ttsModelFileName = Qwen3TtsCpp.GetModelFileName(Qwen3TtsCpp.ResolveModelKey(modelKey));
        TitleText = $"Downloading Qwen3 TTS models ({ttsModelFileName})";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskQwen3TtsModels =
            _qwen3TtsCppDownloadService.DownloadModels(Qwen3TtsCpp.GetSetModelsFolder(), ttsModelFileName, downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadKokoroTtsCpp()
    {
        TitleText = "Downloading Kokoro TTS";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        _downloadTaskKokoroTtsCpp =
            _kokoroTtsCppDownloadService.DownloadEngine(_downloadStreamKokoroTtsCpp, downloadProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadKokoroTtsModels()
    {
        TitleText = "Downloading Kokoro TTS models (~380 MB)";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskKokoroTtsModels =
            _kokoroTtsCppDownloadService.DownloadModels(KokoroTtsCpp.GetSetModelsFolder(), downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadChatterboxModels(string? modelKey = null)
    {
        var resolved = ChatterboxTtsCppDownloadService.ResolveModelKey(modelKey);
        var sizeText = resolved == ChatterboxTtsCppDownloadService.ModelKeyTurbo ? "~1 GB" : "~990 MB";
        TitleText = $"Downloading Chatterbox TTS {resolved} models ({sizeText})";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskChatterboxModels =
            _chatterboxTtsCppDownloadService.DownloadModels(ChatterboxTtsCpp.GetSetModelsFolder(), resolved, downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadQwen3TtsCrispAsrModels(string? modelKey = null)
    {
        var resolved = Qwen3TtsCrispAsr.ResolveModelKey(modelKey);
        var talkerFileName = Qwen3TtsCrispAsr.GetTalkerFileName(resolved);
        TitleText = $"Downloading Qwen3 TTS (CrispASR) models ({resolved}): {talkerFileName}";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskQwen3TtsCrispAsrModels =
            _qwen3TtsCrispAsrDownloadService.DownloadModels(Qwen3TtsCrispAsr.GetSetModelsFolder(), resolved, downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadVibeVoiceCrispAsrModels(string? modelKey = null)
    {
        var resolved = VibeVoiceCrispAsr.ResolveModelKey(modelKey);
        var talkerFileName = VibeVoiceCrispAsr.GetTalkerFileName(resolved);
        TitleText = $"Downloading VibeVoice (CrispASR) model ({resolved}): {talkerFileName}";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskVibeVoiceCrispAsrModels =
            _vibeVoiceCrispAsrDownloadService.DownloadModels(VibeVoiceCrispAsr.GetSetModelsFolder(), resolved, downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadIndexTtsCrispAsrModels(string? modelKey = null)
    {
        var resolved = IndexTtsCrispAsr.ResolveModelKey(modelKey);
        var talkerFileName = IndexTtsCrispAsr.GetTalkerFileName(resolved);
        TitleText = $"Downloading IndexTTS (CrispASR) models ({resolved}): {talkerFileName}";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskIndexTtsCrispAsrModels =
            _indexTtsCrispAsrDownloadService.DownloadModels(IndexTtsCrispAsr.GetSetModelsFolder(), resolved, downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadOmniVoice(string windowsVariant = OmniVoiceDownloadService.WindowsVariantVulkan)
    {
        _omniVoiceVariant = windowsVariant;

        string variantLabel;
        if (Configuration.IsRunningOnWindows)
        {
            variantLabel = windowsVariant;
        }
        else if (Configuration.IsRunningOnMac)
        {
            variantLabel = "macOS universal CPU+Metal";
        }
        else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            variantLabel = "Linux ARM64 CPU";
        }
        else
        {
            variantLabel = "Linux x64 CPU";
        }

        TitleText = $"Downloading OmniVoice TTS ({variantLabel})";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        _downloadTaskOmniVoice =
            _omniVoiceDownloadService.DownloadEngine(_downloadStreamOmniVoice, windowsVariant, downloadProgress, _cancellationTokenSource.Token);
    }

    public void StartDownloadOmniVoiceModels()
    {
        TitleText = "Downloading OmniVoice TTS models (~1.4 GB)";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var titleProgress = new Action<string>(title =>
        {
            Dispatcher.UIThread.Post(() => TitleText = title);
        });

        _downloadTaskOmniVoiceModels =
            _omniVoiceDownloadService.DownloadModels(OmniVoiceTtsCpp.GetSetModelsFolder(), downloadProgress, titleProgress, _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }

    // Drain everything we own when the window goes away. The happy-path
    // handlers above dispose each MemoryStream as soon as it's unpacked, but
    // cancel or an early throw can leave several streams (each holding the
    // full multi-GB download in memory) alive. Disposing here is idempotent —
    // MemoryStream.Dispose can be called twice safely.
    internal void OnClosing()
    {
        try { _cancellationTokenSource.Cancel(); } catch (ObjectDisposedException) { }
        _timer.Stop();
        _timer.Elapsed -= OnTimerOnElapsed;
        _timer.Dispose();

        DisposeQuietly(_downloadStream);
        DisposeQuietly(_downloadStreamModel);
        DisposeQuietly(_downloadStreamConfig);
        DisposeQuietly(_downloadStreamQwen3TtsCpp);
        DisposeQuietly(_downloadStreamQwen3TtsCppVoices);
        DisposeQuietly(_downloadStreamKokoroTtsCpp);
        DisposeQuietly(_downloadStreamQwen3TtsCrispAsrVoices);
        DisposeQuietly(_downloadStreamOmniVoice);
        DisposeQuietly(_downloadStreamOmniVoices);

        try { _cancellationTokenSource.Dispose(); } catch (ObjectDisposedException) { }
    }

    private static void DisposeQuietly(MemoryStream stream)
    {
        try { stream.Dispose(); } catch { /* already disposed or never opened */ }
    }
}