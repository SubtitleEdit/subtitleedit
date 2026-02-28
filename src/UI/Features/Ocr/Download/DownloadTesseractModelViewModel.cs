using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public partial class DownloadTesseractModelViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TesseractDictionary> _tesseractDictionaryItems;
    [ObservableProperty] private TesseractDictionary? _selectedTesseractDictionaryItem;
    [ObservableProperty] private bool _isProgressVisible;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }

    private readonly ITesseractDownloadService _tesseractDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    public DownloadTesseractModelViewModel(ITesseractDownloadService tesseractDownloadService)
    {
        _tesseractDownloadService = tesseractDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();
        _downloadStream = new MemoryStream();

        TesseractDictionaryItems = new ObservableCollection<TesseractDictionary>(TesseractDictionary.List().OrderBy(p => p.ToString()));
        SelectedTesseractDictionaryItem = TesseractDictionaryItems.FirstOrDefault(p => p.Code == "eng") ?? TesseractDictionaryItems.FirstOrDefault();
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

            if (_downloadTask is { IsCompleted: true })
            {
                _timer.Stop();
                _done = true;

                if (_downloadStream.Length == 0)
                {
                    StatusText = "Download failed";
                    Error = "No data received";
                    return;
                }

                UnpackTesseract();
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
                    StatusText = "Download canceled";
                    Close();
                }
                else
                {
                    StatusText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }
        }
    }

    private void UnpackTesseract()
    {
        if (!(SelectedTesseractDictionaryItem is { } model))
        {
            return;
        }

        if (!Directory.Exists(Se.TesseractModelFolder))
        {
            Directory.CreateDirectory(Se.TesseractModelFolder);
        }

        _downloadStream.Position = 0;
        var fileName = Path.Combine(Se.TesseractModelFolder, model.Code + ".traineddata");
        File.WriteAllBytes(fileName, _downloadStream.ToArray());
        _downloadStream.Dispose();
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

    [RelayCommand]
    private void Download()
    {
        StartDownload();
    }

    public void StartDownload()
    {
        if (SelectedTesseractDictionaryItem == null)
        {
            StatusText = "Please select a Tesseract dictionary to download.";
            return;
        }

        IsProgressVisible = true;
        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            Progress = percentage;
            StatusText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        _downloadTask = _tesseractDownloadService.DownloadTesseractModel(
            SelectedTesseractDictionaryItem.Url,
            _downloadStream,
            downloadProgress,
            _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}