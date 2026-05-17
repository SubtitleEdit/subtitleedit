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
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public partial class DownloadVideoFromUrlViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;
    [ObservableProperty] private string _fileNameDisplay;

    public Window? Window { get; set; }
    public bool Success { get; private set; }
    public string OutputPath { get; private set; } = string.Empty;

    private readonly IYtDlpDownloadService _ytDlpDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private string _url = string.Empty;

    public DownloadVideoFromUrlViewModel(IYtDlpDownloadService ytDlpDownloadService)
    {
        _ytDlpDownloadService = ytDlpDownloadService;
        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;
        FileNameDisplay = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerElapsed;
    }

    public void Initialize(string url, string outputPath)
    {
        _url = url;
        OutputPath = outputPath;
        FileNameDisplay = Path.GetFileName(outputPath);
    }

    public void StartDownload()
    {
        var progress = new Progress<float>(fraction =>
        {
            var pct = (int)Math.Round(fraction * 100.0, MidpointRounding.AwayFromZero);
            Progress = pct;
            StatusText = string.Format(Se.Language.General.DownloadingXPercent, pct.ToString(CultureInfo.InvariantCulture));
        });

        var folder = Path.GetDirectoryName(OutputPath);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _downloadTask = _ytDlpDownloadService.DownloadVideo(_url, OutputPath, progress, _cancellationTokenSource.Token);
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        lock (_lockObj)
        {
            if (_done || _downloadTask is null)
            {
                return;
            }

            if (_downloadTask.IsCompletedSuccessfully)
            {
                _timer.Stop();
                _done = true;
                Success = File.Exists(OutputPath);
                if (!Success)
                {
                    Error = $"Download finished but output file was not found:{Environment.NewLine}{OutputPath}";
                }

                Close();
            }
            else if (_downloadTask.IsFaulted)
            {
                _timer.Stop();
                _done = true;

                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    StatusText = "Download canceled";
                    TryDeletePartial();
                    Close();
                }
                else
                {
                    StatusText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                    TryDeletePartial();
                    Se.LogError(ex ?? new Exception("Unknown download error"), "yt-dlp video download failed");
                }
            }
            else if (_downloadTask.IsCanceled)
            {
                _timer.Stop();
                _done = true;
                TryDeletePartial();
                Close();
            }
        }
    }

    private void TryDeletePartial()
    {
        try
        {
            if (File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    [RelayCommand]
    private void CommandCancel()
    {
        _cancellationTokenSource.Cancel();
        // Let the timer pick up the cancellation and close cleanly.
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(Error))
            {
                return; // keep window open so the user can read the error
            }

            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CommandCancel();
        }
    }
}
