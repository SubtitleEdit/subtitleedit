using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private readonly Timer _timer = new();

    private readonly ITtsDownloadService _ttsDownloadService;
    private readonly IQwen3TtsCppDownloadService _qwen3TtsCppDownloadService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;
    private readonly MemoryStream _downloadStreamModel;
    private readonly MemoryStream _downloadStreamConfig;
    private readonly MemoryStream _downloadStreamQwen3TtsCpp;
    private readonly MemoryStream _downloadStreamQwen3TtsCppVoices;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly object _lock = new();
    private string _modelFileName;
    private string _configFileName;

    public DownloadTtsViewModel(ITtsDownloadService ttsDownloadService, IZipUnpacker zipUnpacker, IQwen3TtsCppDownloadService qwen3TtsCppDownloadService)
    {
        _ttsDownloadService = ttsDownloadService;
        _qwen3TtsCppDownloadService = qwen3TtsCppDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();
        _downloadStreamModel = new MemoryStream();
        _downloadStreamConfig = new MemoryStream();
        _downloadStreamQwen3TtsCpp = new MemoryStream();
        _downloadStreamQwen3TtsCppVoices = new MemoryStream();

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
        }
    }

    private void FixSymbolicLink(string path)
    {
        var folder = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(folder))
        {
            Se.LogError("FixSymbolicLink: Failed to get folder from path: " + path);
            return;
        }

        var sourcePath = Path.Combine(folder, "libpiper_phonemize.so.1.2.0");
        var linkPath = Path.Combine(folder, "libpiper_phonemize.so.1");

        try
        {
            if (File.Exists(sourcePath))
            {
                // Check if the link exists and remove it if necessary
                if (File.Exists(linkPath) || Directory.Exists(linkPath))
                {
                    File.Delete(linkPath); // Delete the existing link or file
                }

                // Create the symbolic link
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash", // Use bash shell for command execution
                    Arguments = $"-c \"ln -sf \\\"{sourcePath}\\\" \\\"{linkPath}\\\"\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(processStartInfo);

                if (process != null)
                {
                    process.WaitForExit();

                    // Check if the process completed successfully
                    if (process.ExitCode != 0)
                    {
                        var error = process.StandardError.ReadToEnd();
                        Se.LogError($"Error creating symlink: {error}");
                    }
                    else
                    {
                        Se.LogError("Symbolic link created successfully.");
                    }
                }
            }
            else
            {
                Se.LogError("Source library file not found!");
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex);
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }
}