using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.SevenZipExtractor;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class DownloadLibVlcViewModel : ObservableObject
{
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }

    private string _tempFileName;
    private readonly ILibVlcDownloadService _libVlcDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private IndeterminateProgressHelper? _indeterminateProgressHelper;

    public DownloadLibVlcViewModel(ILibVlcDownloadService libVlcDownloadService)
    {
        _libVlcDownloadService = libVlcDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = string.Format(Se.Language.General.DownloadingX, "libVLC");
        ProgressText = string.Empty;
        ProgressOpacity = 1.0;
        Error = string.Empty;
        _tempFileName = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

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
                _done = true;

                if (!File.Exists(_tempFileName))
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var fileInfo = new FileInfo(_tempFileName);
                if (fileInfo.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                if (!Directory.Exists(Se.VlcFolder))
                {
                    Directory.CreateDirectory(Se.VlcFolder);
                }

                StartIndeterminateProgress();
                Unpacker.Extract7Zip(_tempFileName, Se.VlcFolder, "vlc-3.0.23", _cancellationTokenSource, text => ProgressText = text);
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
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
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

        var folder = Se.DataFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _tempFileName = Path.Combine(folder, $"{Guid.NewGuid()}.7z");
        _downloadTask = _libVlcDownloadService.DownloadLibVlc(_tempFileName, downloadProgress, _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}