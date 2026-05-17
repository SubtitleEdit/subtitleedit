using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

    /// <summary>
    /// When the user requested subtitle download alongside the video, this lists
    /// the subtitle files yt-dlp wrote into <see cref="TempSubtitleDirectory"/>.
    /// Empty when subtitles weren't requested or when no subtitle tracks exist
    /// for the video. Populated only after a successful download.
    /// </summary>
    public IReadOnlyList<DownloadedSubtitleInfo> DownloadedSubtitles { get; private set; } = Array.Empty<DownloadedSubtitleInfo>();

    /// <summary>
    /// Per-download GUID temp directory the subtitle sidecars live in. Scoping
    /// downloads to a fresh directory keeps the picker from accidentally listing
    /// unrelated <c>&lt;stem&gt;.&lt;lang&gt;.&lt;ext&gt;</c> files that may
    /// already sit next to the user's chosen output path. Caller is responsible
    /// for deleting it once the picked subtitle has been loaded.
    /// </summary>
    public string? TempSubtitleDirectory { get; private set; }

    private readonly IYtDlpDownloadService _ytDlpDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private string _url = string.Empty;
    private bool _downloadSubtitles;

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

    public void Initialize(string url, string outputPath, bool downloadSubtitles = false)
    {
        _url = url;
        _downloadSubtitles = downloadSubtitles;
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

        // Always route through the temp dir wrapper, even when subs are off — yt-dlp
        // doesn't always honor the extension in -o (some URLs land as .webm or .mp4
        // regardless of what we ask for), so we need FindProducedVideo to locate the
        // actual produced file and move it to the user's chosen OutputPath.
        _downloadTask = DownloadViaTempDirectoryAsync(progress, _cancellationTokenSource.Token);

        _timer.Start();
    }

    /// <summary>
    /// Routes yt-dlp's output through a fresh GUID temp directory. Two reasons:
    /// (1) subtitle sidecars yt-dlp writes can't collide with any pre-existing
    /// <c>&lt;stem&gt;.&lt;lang&gt;.&lt;ext&gt;</c> files in the user's chosen save
    /// folder; (2) yt-dlp doesn't always produce a file with the extension we
    /// asked for in <c>-o</c>, so we need to look in a known-empty directory to
    /// reliably find what it actually wrote and rename it to the user's target.
    /// The subtitle sidecars stay in the temp dir for the picker to enumerate
    /// and are cleaned up by the caller once a track has been loaded.
    /// </summary>
    private async Task DownloadViaTempDirectoryAsync(IProgress<float> progress, CancellationToken cancellationToken)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SE-dl-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        if (_downloadSubtitles)
        {
            TempSubtitleDirectory = tempDir;
        }

        // Pass yt-dlp the literal "%(ext)s" placeholder so it picks the actual
        // container extension. If we use the user's chosen extension verbatim
        // (e.g. "download.mkv"), yt-dlp treats the whole thing as the template
        // stem and writes "download.mkv.webm" after the merge — leaving us
        // unable to find the produced file by predicted name.
        var templatePath = Path.Combine(tempDir, "download.%(ext)s");

        try
        {
            await _ytDlpDownloadService.DownloadVideo(_url, templatePath, _downloadSubtitles, progress, cancellationToken);

            var actualVideoPath = FindProducedVideo(tempDir);
            if (actualVideoPath is null)
            {
                throw new FileNotFoundException(
                    "yt-dlp finished but no video file was produced." + Environment.NewLine +
                    $"Temp directory: {tempDir}" + Environment.NewLine +
                    "Contents: " + (Directory.Exists(tempDir)
                        ? string.Join(", ", Directory.EnumerateFiles(tempDir).Select(Path.GetFileName))
                        : "<missing>"));
            }

            if (File.Exists(OutputPath))
            {
                File.Delete(OutputPath);
            }
            File.Move(actualVideoPath, OutputPath);

            if (_downloadSubtitles)
            {
                DownloadedSubtitles = YtDlpDownloadService.EnumerateDownloadedSubtitles(tempDir, "download");
            }
            else
            {
                // No subs to keep — clean the temp dir right away so we don't
                // leak it. (When subs are on, the caller owns cleanup.)
                TryDeleteDirectory(tempDir);
            }
        }
        catch
        {
            // Best-effort cleanup on any failure — caller never sees the temp dir
            // in that case, so it would otherwise leak.
            TryDeleteDirectory(tempDir);
            TempSubtitleDirectory = null;
            throw;
        }
    }

    /// <summary>
    /// Scans <paramref name="tempDir"/> for the video yt-dlp wrote. Video files
    /// have the shape <c>download.&lt;ext&gt;</c> (single-segment extension);
    /// subtitle sidecars (<c>download.&lt;lang&gt;.&lt;ext&gt;</c>) and incomplete
    /// <c>.part</c> files are filtered out.
    /// </summary>
    private static string? FindProducedVideo(string tempDir)
    {
        foreach (var file in Directory.EnumerateFiles(tempDir, "download.*"))
        {
            var name = Path.GetFileName(file);
            if (!name.StartsWith("download.", StringComparison.Ordinal))
            {
                continue;
            }

            var rest = name.Substring("download.".Length);
            if (rest.Contains('.'))
            {
                // "download.<lang>.<subext>" — a subtitle sidecar, not the video.
                continue;
            }

            if (string.Equals(rest, "part", StringComparison.OrdinalIgnoreCase))
            {
                // Incomplete intermediate from a failed prior download attempt.
                continue;
            }

            return file;
        }

        return null;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // best-effort
        }
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
                // DownloadedSubtitles / TempSubtitleDirectory are populated inside
                // DownloadViaTempDirectoryAsync before this branch is reached.

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
        // After a download error the window is intentionally kept open so the
        // user can read the error (see Close()). In that state the download task
        // is already done, so cancelling the token does nothing — Cancel here
        // becomes "dismiss the error and close the window".
        if (_done)
        {
            Window?.Close();
            return;
        }

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
