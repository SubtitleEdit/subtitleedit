using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.SevenZipExtractor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Nikse.SubtitleEdit.Logic;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public partial class DownloadPaddleOcrViewModel : ObservableObject
{
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }

    private string _tempFolder;
    private Task? _downloadTask;
    private int _downloadTaskIndex;
    private List<string> _downloadTaskUrls;
    private Timer _timer = new Timer(500);
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private PaddleOcrDownloadType _downloadType;
    private IndeterminateProgressHelper? _indeterminateProgressHelper;

    public DownloadPaddleOcrViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = Se.Language.General.StartingDotDotDot;
        ProgressText = string.Empty;
        Error = string.Empty;
        _tempFolder = string.Empty;
        _downloadType = PaddleOcrDownloadType.Models;
        _downloadTaskUrls = new List<string>();
        _downloadTaskIndex = 0;
    }

    private readonly Lock _lockObj = new();

    public void Initialize(PaddleOcrDownloadType paddleOcrDownloadType)
    {
        _downloadType = paddleOcrDownloadType;
        if (_downloadType is PaddleOcrDownloadType.EngineCpu or
            PaddleOcrDownloadType.EngineGpu11 or
            PaddleOcrDownloadType.EngineGpu12 or
            PaddleOcrDownloadType.EngineCpuLinux or
            PaddleOcrDownloadType.EngineGpuLinux)
        {
            StatusText = Se.Language.Ocr.DownloadingPaddleOcrEngineDotDotDot;
        }
        else if (_downloadType == PaddleOcrDownloadType.Models)
        {
            StatusText = Se.Language.Ocr.DownloadingPaddleOcrModelsDotDotDot;
        }
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_done)
            {
                return;
            }

            if (_downloadTask is { IsCompleted: true })
            {
                _timer.Stop();

                if (_downloadTaskIndex < _downloadTaskUrls.Count - 1)
                {
                    _downloadTaskIndex++;
                    Dispatcher.UIThread.Post(() =>
                    {
                        ProgressText = $"Starting download {_downloadTaskIndex + 1} of {_downloadTaskUrls.Count}...";
                        var url = _downloadTaskUrls[_downloadTaskIndex];
                        var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
                        _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, new Progress<float>(number =>
                        {
                            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
                            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
                            ProgressValue = percentage;
                            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
                        }), _cancellationTokenSource.Token);

                    });
                    return;
                }

                _done = true;

                if (!AllFileExists())
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                StartIndeterminateProgress();
                var firstFile = Path.Combine(_tempFolder, Path.GetFileName(_downloadTaskUrls[0]));
                if (_downloadType == PaddleOcrDownloadType.Models)
                {
                    StatusText = string.Format(Se.Language.General.UnpackingX, Se.Language.General.Models);
                    Unpacker.Extract7Zip(firstFile, Se.PaddleOcrModelsFolder, "PaddleOCR.PP-OCRv5.support.files", _cancellationTokenSource, text => ProgressText = text);
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineCpu)
                {
                    StatusText = string.Format(Se.Language.General.UnpackingX, Se.Language.Ocr.PaddleOcr);
                    Unpacker.Extract7Zip(firstFile, Se.PaddleOcrFolder, "PaddleOCR-CPU-v1.4.0", _cancellationTokenSource, text => ProgressText = text);
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineGpu11)
                {
                    StatusText = string.Format(Se.Language.General.UnpackingX, Se.Language.Ocr.PaddleOcr);
                    Unpacker.Extract7Zip(firstFile, Se.PaddleOcrFolder, "PaddleOCR-GPU-v1.4.0-CUDA-11.8", _cancellationTokenSource, text => ProgressText = text);
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineGpu12)
                {
                    StatusText = string.Format(Se.Language.General.UnpackingX, Se.Language.Ocr.PaddleOcr);
                    Unpacker.Extract7Zip(firstFile, Se.PaddleOcrFolder, "PaddleOCR-GPU-v1.4.0-CUDA-12.9", _cancellationTokenSource, text => ProgressText = text);
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineCpuLinux)
                {
                    StatusText = string.Format(Se.Language.General.UnpackingX, Se.Language.Ocr.PaddleOcr);
                    Unpacker.Extract7Zip(firstFile, Se.PaddleOcrFolder, "PaddleOCR-CPU-v1.4.0-Linux", _cancellationTokenSource, text => ProgressText = text);
                    var binFile = Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin");
                    if (File.Exists(binFile))
                    {
                        LinuxHelper.MakeExecutable(binFile);
                    }                    
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineGpuLinux)
                {
                    StatusText = string.Format(Se.Language.General.UnpackingX, Se.Language.Ocr.PaddleOcr);
                    Unpacker.Extract7Zip(firstFile, Se.PaddleOcrFolder, "PaddleOCR-GPU-v1.4.0-Linux", _cancellationTokenSource, text => ProgressText = text);
                    var binFile = Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin");
                    if (File.Exists(binFile))
                    {
                        LinuxHelper.MakeExecutable(binFile);
                    }
                }

                StopIndeterminateProgress();
                OkPressed = true;
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                _done = true;
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
        }
    }

    private bool AllFileExists()
    {
        foreach (var url in _downloadTaskUrls)
        {
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            if (!File.Exists(fileName))
            {
                Se.LogError($"Expected file not found after download: {fileName}");
                return false;
            }

            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Length == 0)
            {
                Se.LogError($"Downloaded file is empty: {fileName}");
                return false;
            }
        }

        return true;
    }

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

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private void CommandCancel()
    {
        _cancellationTokenSource?.Cancel();
        _done = true;
        _timer.Stop();
        Close();
    }

    public void StartDownload()
    {
        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var folder = Se.PaddleOcrFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _tempFolder = Path.Combine(folder, $"{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempFolder);
        _downloadTaskIndex = 0;
        _downloadTaskUrls = new List<string>();

        if (_downloadType == PaddleOcrDownloadType.Models)
        {
            _downloadTaskUrls.AddRange(PaddleOcr.UrlsSupportFiles);
            var url = _downloadTaskUrls[_downloadTaskIndex];
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (_downloadType == PaddleOcrDownloadType.EngineGpu11)
        {
            _downloadTaskUrls.AddRange(PaddleOcr.UrlsWindowsGpuCuda11);
            var url = _downloadTaskUrls[_downloadTaskIndex];
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (_downloadType == PaddleOcrDownloadType.EngineGpu12)
        {
            _downloadTaskUrls.AddRange(PaddleOcr.UrlsWindowsGpuCuda12);
            var url = _downloadTaskUrls[_downloadTaskIndex];
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (_downloadType == PaddleOcrDownloadType.EngineCpu)
        {
            _downloadTaskUrls.AddRange(PaddleOcr.UrlsWindowsCpu);
            var url = _downloadTaskUrls[_downloadTaskIndex];
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, downloadProgress,
                _cancellationTokenSource.Token);
        }
        else if (_downloadType == PaddleOcrDownloadType.EngineGpuLinux)
        {
            _downloadTaskUrls.AddRange(PaddleOcr.UrlsLinuxGpu);
            var url = _downloadTaskUrls[_downloadTaskIndex];
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, downloadProgress, _cancellationTokenSource.Token);
        }            
        else if (_downloadType == PaddleOcrDownloadType.EngineCpuLinux)
        {
            _downloadTaskUrls.AddRange(PaddleOcr.UrlsLinuxCpu);
            var url = _downloadTaskUrls[_downloadTaskIndex];
            var fileName = Path.Combine(_tempFolder, Path.GetFileName(url));
            _downloadTask = DownloadHelper.DownloadFileAsync(new HttpClient(), url, fileName, downloadProgress, _cancellationTokenSource.Token);
        }         
        else
        {
            Se.LogError($"Unknown Paddle OCR download type: {_downloadType}");
            ProgressText = "Download failed";
            Error = "Unknown download type";
            return;
        }

        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}