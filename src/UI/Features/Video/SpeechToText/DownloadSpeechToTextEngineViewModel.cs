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

public partial class DownloadSpeechToTextEngineViewModel : ObservableObject
{
    [ObservableProperty] private string _titleText;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }
    public ISpeechToTextEngine? Engine { get; internal set; }

    /// <summary>
    /// Windows-only CrispASR download variant: "cpu", "vulkan", or "cuda". Defaults to "vulkan".
    /// </summary>
    public string CrispAsrWindowsVariant { get; set; } = "vulkan";

    private readonly IWhisperDownloadService _whisperDownloadService;
    private readonly IChatLlmDownloadService _chatLlmDownloadService;
    private readonly IQwen3AsrCppDownloadService _qwen3AsrCppDownloadService;
    private readonly ICrispAsrDownloadService _crispAsrDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly IZipUnpacker _zipUnpacker;

    private IndeterminateProgressHelper? _indeterminateProgressHelper;

    public DownloadSpeechToTextEngineViewModel(
        IWhisperDownloadService whisperDownloadService,
        IZipUnpacker zipUnpacker,
        IChatLlmDownloadService chatLlmDownloadService,
        IQwen3AsrCppDownloadService qwen3AsrCppDownloadService,
        ICrispAsrDownloadService crispAsrDownloadService)
    {
        _whisperDownloadService = whisperDownloadService;
        _zipUnpacker = zipUnpacker;
        _chatLlmDownloadService = chatLlmDownloadService;
        _qwen3AsrCppDownloadService = qwen3AsrCppDownloadService;
        _crispAsrDownloadService = crispAsrDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();

        TitleText = Se.Language.Video.AudioToText.DownloadingSpeechToTextEngine;
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

                        var path = Engine.GetExecutable();
                        MakeExecutable(path);
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

                    DownloadAndUnpackSileroVad(dir);

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

                    DownloadAndUnpackSileroVad(dir);

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
                    var skipFolder = Engine is ICrispAsrEngine
                        ? RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                            ? "crispasr-linux-x86_64"
                            : CrispAsrWindowsVariant switch
                            {
                                "cuda"   => "crispasr-windows-x86_64-cuda",
                                "cpu"    => "crispasr-windows-x86_64-cpu-legacy",
                                "vulkan" => "crispasr-windows-x86_64-vulkan",
                                _        => Engine.UnpackSkipFolder,
                            }
                        : Engine.UnpackSkipFolder;
                    Unpack(folder, skipFolder);

                    if (Engine is not (ChatLlmCppEngine or Qwen3AsrCppEngine))
                    {
                        DownloadAndUnpackSileroVad(folder);
                    }

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

    private static void MakeExecutable(string path)
    {
        if (File.Exists(path))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                MacHelper.MakeExecutable(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                LinuxHelper.MakeExecutable(path);
            }
        }
    }

    private void Unpack(string folder, string skipFolderLevel)
    {
        _downloadStream.Position = 0;
        _zipUnpacker.UnpackZipStream(_downloadStream, folder, skipFolderLevel, false, new List<string>(), null);
        _downloadStream.Dispose();

        if (Engine == null || (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux)))
        {
            return;
        }

        var path = Path.Combine(folder, Engine.GetExecutableFileName());
        MakeExecutable(path);
    }

    private void DownloadAndUnpackSileroVad(string folder)
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        var sileroFileName = "ggml-silero-vad.bin";
        if (File.Exists(Path.Combine(folder, sileroFileName)))
        {
            return;
        }

        TitleText = string.Format(Se.Language.General.DownloadingX, "Silero VAD");
        ProgressValue = 0;

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, percentage.ToString(CultureInfo.InvariantCulture));
        });

        using var sileroStream = new MemoryStream();
        _whisperDownloadService.DownloadSileroVad(sileroStream, downloadProgress, _cancellationTokenSource.Token)
            .GetAwaiter().GetResult();

        if (_cancellationTokenSource.IsCancellationRequested || sileroStream.Length == 0)
        {
            return;
        }

        TitleText = string.Format(Se.Language.General.UnpackingX, "Silero VAD");
        sileroStream.Position = 0;
        _zipUnpacker.UnpackZipStream(sileroStream, folder, string.Empty, false, new List<string>(), null);
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
        else if (Engine is Qwen3AsrCppEngine)
        {
            var dir = Engine.GetAndCreateWhisperFolder();
            _downloadTask = _qwen3AsrCppDownloadService.DownloadEngine(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is ICrispAsrEngine)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _downloadTask = CrispAsrWindowsVariant switch
                {
                    "cuda" => _crispAsrDownloadService.DownloadEngineWindowsCuda(_downloadStream, downloadProgress, _cancellationTokenSource.Token),
                    "cpu"  => _crispAsrDownloadService.DownloadEngineWindowsCpu(_downloadStream, downloadProgress, _cancellationTokenSource.Token),
                    _      => _crispAsrDownloadService.DownloadEngineWindowsVulkan(_downloadStream, downloadProgress, _cancellationTokenSource.Token),
                };
            }
            else
            {
                _downloadTask = _crispAsrDownloadService.DownloadEngine(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
            }
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