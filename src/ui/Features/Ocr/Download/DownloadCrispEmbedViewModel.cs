using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

/// <summary>
/// Downloads either the CrispEmbed engine binaries (release archive, variant-aware, with an
/// .installed.sha256 sidecar for update tracking) or a single GGUF model (streamed to disk).
/// </summary>
public partial class DownloadCrispEmbedViewModel : ObservableObject
{
    [ObservableProperty] private string _titleText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }

    private readonly ICrispEmbedDownloadService _downloadService;
    private readonly IZipUnpacker _zipUnpacker;
    private System.Threading.Tasks.Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private bool _isModelDownload;
    private string _engineVariant = string.Empty;
    private CrispEmbedModel? _model;
    private string _modelTempFileName = string.Empty;
    private string _modelFileName = string.Empty;

    public DownloadCrispEmbedViewModel(ICrispEmbedDownloadService downloadService, IZipUnpacker zipUnpacker)
    {
        _downloadService = downloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();
        _downloadStream = new MemoryStream();

        TitleText = string.Format(Se.Language.General.DownloadingX, CrispEmbedEngine.StaticName);
        ProgressText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
    }

    private readonly Lock _lockObj = new();

    /// <summary>
    /// Engine download. <paramref name="variant"/>: on Windows "cpu", "vulkan", or "cuda"
    /// (defaults to vulkan); on Linux x86_64 "cuda" or empty for the CPU build; ignored on
    /// macOS / Linux ARM64.
    /// </summary>
    public void InitializeEngine(string variant)
    {
        _isModelDownload = false;
        _engineVariant = variant;
        var displayName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || variant == "cuda"
            ? $"{CrispEmbedEngine.StaticName} {_engineVariant}".TrimEnd()
            : CrispEmbedEngine.StaticName;
        TitleText = string.Format(Se.Language.General.DownloadingX, displayName);
        StartDownload();
    }

    public void InitializeModel(CrispEmbedBackend backend, CrispEmbedModel model)
    {
        _isModelDownload = true;
        _model = model;
        _modelFileName = backend.GetModelPath(model);
        _modelTempFileName = _modelFileName + ".$tmp$";
        TitleText = string.Format(Se.Language.General.DownloadingX, model.Name);
        StartDownload();
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_done)
            {
                return;
            }

            if (_downloadTask is { IsCompletedSuccessfully: true })
            {
                _timer.Stop();
                _done = true;

                try
                {
                    if (_isModelDownload)
                    {
                        FinishModelDownload();
                    }
                    else
                    {
                        FinishEngineDownload();
                    }
                }
                catch (Exception ex)
                {
                    // e.g. a truncated archive that still had Length > 0 - without this the
                    // Timer would swallow the exception and leave the dialog stuck open.
                    ProgressText = _isModelDownload ? "Download failed" : "Unpacking failed";
                    Error = ex.Message;
                    Se.LogError(ex, "CrispEmbed download post-processing failed");
                }

                return;
            }

            if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                _done = true;
                DeleteTempModelFile();
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

    private void FinishEngineDownload()
    {
        if (_downloadStream.Length == 0)
        {
            ProgressText = "Download failed";
            Error = "No data received";
            return;
        }

        var folder = CrispEmbedEngine.GetAndCreateFolder();
        DownloadHashManager.WriteSidecar(folder, DownloadHashManager.ResolveCrispEmbedKey(_engineVariant), _downloadStream);
        RemoveStaleBinaries(folder);

        TitleText = string.Format(Se.Language.General.UnpackingX, CrispEmbedEngine.StaticName);
        // The CrispEmbed release archives have no wrapping top-level folder (binaries sit at
        // the archive root), so there is no skip-folder level to strip - unlike CrispASR.
        _downloadStream.Position = 0;
        _zipUnpacker.UnpackZipStream(_downloadStream, folder, string.Empty, false, new List<string>(), null);
        _downloadStream.Dispose();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var name in CrispEmbedEngine.BinaryBaseNames)
            {
                var path = Path.Combine(folder, name);
                if (File.Exists(path))
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        MacHelper.MakeExecutable(path);
                    }
                    else
                    {
                        LinuxHelper.MakeExecutable(path);
                    }
                }
            }
        }

        OkPressed = true;
        Close();
    }

    private void FinishModelDownload()
    {
        if (!File.Exists(_modelTempFileName) || new FileInfo(_modelTempFileName).Length == 0)
        {
            ProgressText = "Download failed";
            Error = "No data received";
            return;
        }

        if (File.Exists(_modelFileName))
        {
            File.Delete(_modelFileName);
        }

        File.Move(_modelTempFileName, _modelFileName);
        OkPressed = true;
        Close();
    }

    /// <summary>
    /// Removes leftover binaries from a previous CrispEmbed install before extracting the new
    /// archive, so switching variants (e.g. Vulkan → CPU) cannot leave orphan ggml DLLs that
    /// the new executable would load by mistake. Only top-level executables and shared
    /// libraries are removed; the models/ subfolder and the sidecar are left in place.
    /// </summary>
    private static void RemoveStaleBinaries(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        try
        {
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var name = Path.GetFileName(file);
                var isBinary = ext is ".exe" or ".dll" or ".so" or ".dylib"
                    || (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        && Array.IndexOf(CrispEmbedEngine.BinaryBaseNames, name) >= 0);
                if (!isBinary)
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // best-effort — a locked file will be overwritten by the unpack step
                }
            }
        }
        catch
        {
            // ignore — cleanup is best-effort
        }
    }

    private void StartDownload()
    {
        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        if (_isModelDownload && _model != null)
        {
            _downloadTask = _downloadService.DownloadModel(_model.Url, _modelTempFileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _downloadTask = _engineVariant switch
            {
                "cuda" => _downloadService.DownloadEngineWindowsCuda(_downloadStream, downloadProgress, _cancellationTokenSource.Token),
                "cpu" => _downloadService.DownloadEngineWindowsCpu(_downloadStream, downloadProgress, _cancellationTokenSource.Token),
                _ => _downloadService.DownloadEngineWindowsVulkan(_downloadStream, downloadProgress, _cancellationTokenSource.Token),
            };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                 && _engineVariant == "cuda"
                 && RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            _downloadTask = _downloadService.DownloadEngineLinuxCuda(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else
        {
            _downloadTask = _downloadService.DownloadEngine(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }

        _timer.Start();
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
        DeleteTempModelFile();
        Close();
    }

    private void DeleteTempModelFile()
    {
        try
        {
            if (!string.IsNullOrEmpty(_modelTempFileName) && File.Exists(_modelTempFileName))
            {
                File.Delete(_modelTempFileName);
            }
        }
        catch
        {
            // ignore
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
