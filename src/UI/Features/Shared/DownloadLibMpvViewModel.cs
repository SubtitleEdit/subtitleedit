using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Shared;

public partial class DownloadLibMpvViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public string LibMpvFileName { get; set; }

    private ILibMpvDownloadService _libMpvDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly IZipUnpacker _zipUnpacker;

    public DownloadLibMpvViewModel(ILibMpvDownloadService libMpvDownloadService, IZipUnpacker zipUnpacker)
    {
        _libMpvDownloadService = libMpvDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();

        StatusText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;
        LibMpvFileName = string.Empty;

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

                var fileName = GetLibMpvFileName();

                if (File.Exists(fileName))
                {
                    try
                    {
                        File.Delete(fileName); //TODO: might be in use... save to temp file instead
                    }
                    catch
                    {
                        // If the file is in use, we will not be able to delete it.
                        // We will just leave it and let the user know to restart SE.

                        StatusText = "Download complete...";
                        Dispatcher.UIThread.Post(async () =>
                        {
                            _ = await MessageBox.Show(
                                Window!,
                                "Error",
                                "Download complete, but could not delete existing file." + Environment.NewLine +
                                "Please restart SE to use the new libmpv.",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }, DispatcherPriority.Background);

                        UnpackLibMpv(GetFallbackLibMpvFileName(true));
                        Close();
                        return;
                    }
                }

                UnpackLibMpv(fileName);

                LibMpvFileName = fileName;
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

    private void UnpackLibMpv(string newFileName)
    {
        var folder = Path.GetDirectoryName(newFileName);
        if (folder != null)
        {
            _downloadStream.Position = 0;
            _zipUnpacker.UnpackZipStream(
                _downloadStream,
                folder,
                string.Empty,
                false,
                new System.Collections.Generic.List<string> { ".dll" },
                new System.Collections.Generic.List<string> { newFileName });
        }

        _downloadStream.Dispose();
    }


    public static string GetLibMpvFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Se.DataFolder, "libmpv-2.dll");
        }

        return Path.Combine(Se.DataFolder, "libmpv-2.so");
    }

    public static string GetFallbackLibMpvFileName(bool create)
    {
        var newFolder = Path.Combine(Se.DataFolder, "libmpv-update");
        if (!Directory.Exists(newFolder) && create)
        {
            Directory.CreateDirectory(newFolder);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(newFolder, "libmpv-2.dll");
        }

        return Path.Combine(newFolder, "libmpv-2.so");
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
            Progress = percentage;
            StatusText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var folder = Se.FfmpegFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _downloadTask = _libMpvDownloadService.DownloadLibMpv(
            _downloadStream,
            downloadProgress,
            _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        CommandCancel();
    }
}