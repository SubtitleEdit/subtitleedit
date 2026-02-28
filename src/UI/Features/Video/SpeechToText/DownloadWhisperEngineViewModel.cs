using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class DownloadWhisperEngineViewModel : ObservableObject
{
    [ObservableProperty] private string _titleText;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }
    public ISpeechToTextEngine? Engine { get; internal set; }

    private readonly IWhisperDownloadService _whisperDownloadService;
    private readonly IChatLlmDownloadService _chatLlmDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly IZipUnpacker _zipUnpacker;

    private IndeterminateProgressHelper? _indeterminateProgressHelper;

    public DownloadWhisperEngineViewModel(
        IWhisperDownloadService whisperDownloadService, 
        IZipUnpacker zipUnpacker, 
        IChatLlmDownloadService chatLlmDownloadService)
    {
        _whisperDownloadService = whisperDownloadService;
        _zipUnpacker = zipUnpacker;
        _chatLlmDownloadService = chatLlmDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();

        TitleText = Se.Language.Video.AudioToText.DownloadingWhisperEngine;
        ProgressText = Se.Language.General.StartingDotDotDot;
        ProgressOpacity = 1.0;
        Error = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

    private void StartIndeterminateProgress()
    {
        _indeterminateProgressHelper?.Dispose();
        _indeterminateProgressHelper = new IndeterminateProgressHelper(
            value => ProgressValue = value,
            opacity => ProgressOpacity = opacity,
            () => _cancellationTokenSource.IsCancellationRequested);
        _indeterminateProgressHelper.Start();
    }

    private void StopIndeterminateProgress()
    {
        _indeterminateProgressHelper?.Stop();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            _timer.Stop();
            if (_downloadTask is { IsCompleted: true } && Engine != null)
            {
                if (Engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName)
                {
                    var dir = Engine.GetAndCreateWhisperFolder();
                    var tempFileName = Path.Combine(dir, Engine.Name + ".7z");

                    TitleText = Se.Language.General.Unpacking7ZipArchiveDotDotDot;
                    StartIndeterminateProgress();
                    Unpacker.Extract7Zip(tempFileName, dir, "Faster-Whisper-XXL", _cancellationTokenSource, text => ProgressText = text);
                    StopIndeterminateProgress();

                    try
                    {
                        File.Delete(tempFileName);

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            var path = Engine.GetExecutable();
                            if (File.Exists(path))
                            {
                                MacHelper.MakeExecutable(path);
                            }
                        }
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            var path = Engine.GetExecutable();
                            if (File.Exists(path))
                            {
                                LinuxHelper.MakeExecutable(path);
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Cancel();
                        return;
                    }

                    OkPressed = true;
                    Close();
                }
                else if (Engine.Name == WhisperEngineCTranslate2.StaticName)
                {
                    var dir = Engine.GetAndCreateWhisperFolder();
                    
                    TitleText = string.Format(Se.Language.General.UnpackingX, Engine.Name);
                    StartIndeterminateProgress();
                    Unpack(dir, string.Empty);
                    StopIndeterminateProgress();
                   
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Cancel();
                        return;
                    }

                    OkPressed = true;
                    Close();
                }
                else
                {
                    if (_downloadStream.Length == 0)
                    {
                        ProgressText = "Download failed";
                        Error = "No data received";
                        return;
                    }

                    var folder = Engine.GetAndCreateWhisperFolder();
                    Unpack(folder, Engine.UnpackSkipFolder);
                    OkPressed = true;
                    Close();
                }

                return;
            }

            if (_downloadTask is { IsFaulted: true })
            {
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

                return;
            }

            _timer.Start();
        }
    }

    private void Unpack(string folder, string skipFolderLevel)
    {
        _downloadStream.Position = 0;
        _zipUnpacker.UnpackZipStream(_downloadStream, folder, skipFolderLevel, false, new List<string>(), null);
        _downloadStream.Dispose();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var path = Path.Combine(folder, "whisper-cli");
            if (File.Exists(path))
            {
                MacHelper.MakeExecutable(path);
            }

            if (Engine is WhisperEnginePurfviewFasterWhisperXxl purfviewEngine)
            {
                path = Path.Combine(folder, purfviewEngine.GetExecutableFileName());
                if (File.Exists(path))
                {
                    MacHelper.MakeExecutable(path);
                }
            }
            
            if (Engine is WhisperEngineCTranslate2 cTranslate2)
            {
                path = Path.Combine(folder, cTranslate2.GetExecutable());
                if (File.Exists(path))
                {
                    MacHelper.MakeExecutable(path);
                }
            }
            
            if (Engine is ChatLlmCppEngine chatLlmCppEngine)
            {
                path = Path.Combine(folder, chatLlmCppEngine.GetExecutableFileName());
                if (File.Exists(path))
                {
                    MacHelper.MakeExecutable(path);
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var path = Path.Combine(folder, "whisper-cli");
            if (File.Exists(path))
            {
                LinuxHelper.MakeExecutable(path);
            }

            if (Engine is WhisperEnginePurfviewFasterWhisperXxl purfviewEngine)
            {
                path = Path.Combine(folder, purfviewEngine.GetExecutableFileName());
                if (File.Exists(path))
                {
                    LinuxHelper.MakeExecutable(path);
                }
            }
            
            if (Engine is ChatLlmCppEngine chatLlmCppEngine)
            {
                path = Path.Combine(folder, chatLlmCppEngine.GetExecutableFileName());
                if (File.Exists(path))
                {
                    LinuxHelper.MakeExecutable(path);
                }
            }
            
            if (Engine is WhisperEngineCTranslate2)
            {
                path = Path.Combine(folder, WhisperEngineCTranslate2.GetExecutableFileName());
                if (File.Exists(path))
                {
                    LinuxHelper.MakeExecutable(path);
                }
            }
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Cancel();
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _timer.Stop();
        StopIndeterminateProgress();
        Close();
    }

    public void StartDownload()
    {
        TitleText = string.Format(Se.Language.General.DownloadingX, Engine?.Name);

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        if (Engine is WhisperEngineCpp)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperCpp(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEngineCppCuBlas)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperCppCuBlas(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEngineCppVulkan)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperCppVulkan(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEngineCTranslate2)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperCTranslate2(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEngineConstMe)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperConstMe(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEnginePurfviewFasterWhisperXxl)
        {
            var dir = Engine.GetAndCreateWhisperFolder();
            var tempFileName = Path.Combine(dir, Engine.Name + ".7z");
            _downloadTask = _whisperDownloadService.DownloadWhisperPurfviewFasterWhisperXxl(tempFileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is ChatLlmCppEngine)
        {
            var dir = Engine.GetAndCreateWhisperFolder();
            _downloadTask = _chatLlmDownloadService.DownloadEngine(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}