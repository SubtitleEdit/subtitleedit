using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
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
    private Task? _downloadTaskOmniVoice;
    private Task? _downloadTaskOmniVoiceVoices;
    private Task? _downloadTaskOmniVoiceModels;
    private readonly Timer _timer = new();

    private readonly ITtsDownloadService _ttsDownloadService;
    private readonly IQwen3TtsCppDownloadService _qwen3TtsCppDownloadService;
    private readonly IKokoroTtsCppDownloadService _kokoroTtsCppDownloadService;
    private readonly IChatterboxTtsCppDownloadService _chatterboxTtsCppDownloadService;
    private readonly IOmniVoiceDownloadService _omniVoiceDownloadService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;
    private readonly MemoryStream _downloadStreamModel;
    private readonly MemoryStream _downloadStreamConfig;
    private readonly MemoryStream _downloadStreamQwen3TtsCpp;
    private readonly MemoryStream _downloadStreamQwen3TtsCppVoices;
    private readonly MemoryStream _downloadStreamKokoroTtsCpp;
    private readonly MemoryStream _downloadStreamOmniVoice;
    private readonly MemoryStream _downloadStreamOmniVoiceVoices;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly object _lock = new();
    private string _modelFileName;
    private string _configFileName;

    public DownloadTtsViewModel(ITtsDownloadService ttsDownloadService, IZipUnpacker zipUnpacker,
        IQwen3TtsCppDownloadService qwen3TtsCppDownloadService,
        IKokoroTtsCppDownloadService kokoroTtsCppDownloadService,
        IChatterboxTtsCppDownloadService chatterboxTtsCppDownloadService,
        IOmniVoiceDownloadService omniVoiceDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
        _qwen3TtsCppDownloadService = qwen3TtsCppDownloadService;
        _kokoroTtsCppDownloadService = kokoroTtsCppDownloadService;
        _chatterboxTtsCppDownloadService = chatterboxTtsCppDownloadService;
        _omniVoiceDownloadService = omniVoiceDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();
        _downloadStreamModel = new MemoryStream();
        _downloadStreamConfig = new MemoryStream();
        _downloadStreamQwen3TtsCpp = new MemoryStream();
        _downloadStreamQwen3TtsCppVoices = new MemoryStream();
        _downloadStreamKokoroTtsCpp = new MemoryStream();
        _downloadStreamOmniVoice = new MemoryStream();
        _downloadStreamOmniVoiceVoices = new MemoryStream();

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
                _downloadTaskOmniVoiceVoices = _omniVoiceDownloadService.DownloadVoices(
                    _downloadStreamOmniVoiceVoices, voicesProgress, _cancellationTokenSource.Token);
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

            if (_downloadTaskOmniVoiceVoices is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadStreamOmniVoiceVoices.Length > 0)
                {
                    var voicesFolder = OmniVoiceTtsCpp.GetSetVoicesFolder();
                    try
                    {
                        _downloadStreamOmniVoiceVoices.Position = 0;
                        _zipUnpacker.UnpackZipStream(_downloadStreamOmniVoiceVoices, voicesFolder, string.Empty, false, new List<string>(), null);
                    }
                    catch (Exception ex)
                    {
                        // Voices are optional; log and continue so the engine is still usable.
                        Se.LogError(ex);
                    }
                    _downloadStreamOmniVoiceVoices.Dispose();
                }

                OkPressed = true;
                Close();
            }
            else if (_downloadTaskOmniVoiceVoices is { IsFaulted: true })
            {
                _timer.Stop();
                var ex = _downloadTaskOmniVoiceVoices.Exception?.InnerException ?? _downloadTaskOmniVoiceVoices.Exception;
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
        TitleText = $"Downloading Qwen3 TTS ({windowsVariant})";

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

    public void StartDownloadOmniVoice(string windowsVariant = OmniVoiceDownloadService.WindowsVariantVulkan)
    {
        string variantLabel;
        if (Configuration.IsRunningOnWindows)
        {
            variantLabel = windowsVariant;
        }
        else if (Configuration.IsRunningOnMac)
        {
            variantLabel = "macOS universal CPU+Metal";
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
}