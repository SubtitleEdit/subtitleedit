using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Nikse.SubtitleEdit.Logic;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class DownloadYtDlpViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }

    private IYtDlpDownloadService _ytDlpDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public DownloadYtDlpViewModel(IYtDlpDownloadService ytDlpDownloadService)
    {
        _ytDlpDownloadService = ytDlpDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;

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

            // IsCompletedSuccessfully, not the broader IsCompleted (also true for
            // Faulted/Canceled), so a failed download falls through to the IsFaulted
            // branch below instead of proceeding as if it had succeeded.
            if (_downloadTask is { IsCompletedSuccessfully: true })
            {
                _timer.Stop();
                _done = true;
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                // Only the partial download - deleting the installed binary here threw away a
                // working yt-dlp whenever an update failed (network error, checksum mismatch).
                YtDlpDownloadService.DeletePartialDownload(YtDlpDownloadService.GetFullFileName());

                _timer.Stop();
                _done = true;
                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    StatusText = Se.Language.General.DownloadCanceled;
                    Close();
                }
                else
                {
                    StatusText = Se.Language.General.DownloadFailed;
                    Error = ex?.Message ?? Se.Language.General.UnknownError;
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
    private void Retry()
    {
        lock (_lockObj)
        {
            if (!_done || _cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            Error = string.Empty;
            Progress = 0;
            StatusText = Se.Language.General.StartingDotDotDot;
            _done = false;
            StartDownload();
            _timer.Start();
        }
    }

    [RelayCommand]
    private void CommandCancel()
    {
        _cancellationTokenSource?.Cancel();
        _done = true;
        Close();
    }

    public void OnClosingCleanup()
    {
        _timer.StopAndDispose(OnTimerOnElapsed);
    }

    public void StartDownload()
    {
        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            Progress = percentage;
            StatusText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        // Se.DataFolder, not Se.FfmpegFolder: this downloads into the data folder (see
        // GetLibMpvFileName / YtDlpDownloadService.GetFullFileName). Copy/paste from the ffmpeg
        // downloader meant the guard protected the wrong directory and created a stray empty
        // "ffmpeg" folder. DownloadLibVlcViewModel gets this right.
        var folder = Se.DataFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _downloadTask = _ytDlpDownloadService.DownloadYtDlp(downloadProgress, _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        // Escape only, like the other download dialogs - any key cancelled the download,
        // including Enter/Space on the Retry button.
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}