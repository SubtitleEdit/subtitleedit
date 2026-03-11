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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Nikse.SubtitleEdit.Core.AudioToText.AudioToTextPostProcessor;
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
    private readonly Timer _timer = new();

    private readonly ITtsDownloadService _ttsDownloadService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;
    private readonly MemoryStream _downloadStreamModel;
    private readonly MemoryStream _downloadStreamConfig;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly object _lock = new();
    private string _modelFileName;
    private string _configFileName;

    public DownloadTtsViewModel(ITtsDownloadService ttsDownloadService, IZipUnpacker zipUnpacker)
    {
        _ttsDownloadService = ttsDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();
        _downloadStreamModel = new MemoryStream();
        _downloadStreamConfig = new MemoryStream();

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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }
}