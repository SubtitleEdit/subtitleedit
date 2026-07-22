using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class DownloadLlamaCppViewModel : ObservableObject
{
    [ObservableProperty] private string _titleText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>"cpu", "vulkan" or "cuda" - which llama.cpp build to download (Windows only).</summary>
    public string Variant { get; set; } = LlamaCppDownloadService.VariantCpu;

    /// <summary>The model to download, or null to only install the engine.</summary>
    public LlamaCppModel? Model { get; set; }

    /// <summary>Re-download the engine binary even when it is already installed (used for updates).</summary>
    public bool ForceEngineDownload { get; set; }

    /// <summary>Re-download the model file even when it is already installed.</summary>
    public bool ForceModelDownload { get; set; }

    private const string TemporaryFileExtension = ".$$$";

    private readonly ILlamaCppDownloadService _downloadService;
    private readonly IZipUnpacker _zipUnpacker;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public DownloadLlamaCppViewModel(ILlamaCppDownloadService downloadService, IZipUnpacker zipUnpacker)
    {
        _downloadService = downloadService;
        _zipUnpacker = zipUnpacker;
        TitleText = string.Format(Se.Language.General.DownloadingX, "llama.cpp");
        ProgressText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
        Close();
    }

    public async void StartDownload()
    {
        try
        {
            var folder = LlamaCppServerManager.GetAndCreateFolder();
            var token = _cancellationTokenSource.Token;

            if (ForceEngineDownload || !LlamaCppServerManager.IsEngineInstalled())
            {
                TitleText = string.Format(Se.Language.General.DownloadingX, "llama.cpp");
                using (var engineStream = new MemoryStream())
                {
                    await _downloadService.DownloadEngine(engineStream, Variant, MakeProgress(), token);
                    if (token.IsCancellationRequested)
                    {
                        Close();
                        return;
                    }

                    TitleText = string.Format(Se.Language.General.UnpackingX, "llama.cpp");
                    engineStream.Position = 0;
                    _zipUnpacker.UnpackZipStream(engineStream, folder, string.Empty, true, new List<string>(), null);

                    WriteInstalledHash(folder, engineStream);
                }

                if (LlamaCppDownloadService.VariantNeedsCudaRuntime(Variant))
                {
                    TitleText = string.Format(Se.Language.General.DownloadingX, "CUDA runtime");
                    using var cudartStream = new MemoryStream();
                    await _downloadService.DownloadCudaRuntime(cudartStream, MakeProgress(), token);
                    if (token.IsCancellationRequested)
                    {
                        Close();
                        return;
                    }

                    TitleText = string.Format(Se.Language.General.UnpackingX, "CUDA runtime");
                    cudartStream.Position = 0;
                    _zipUnpacker.UnpackZipStream(cudartStream, folder, string.Empty, true, new List<string>(), null);
                }

                MakeExecutable(LlamaCppServerManager.GetExecutable());
            }

            if (Model != null && (ForceModelDownload || !LlamaCppServerManager.IsModelInstalled(Model.FileName)))
            {
                TitleText = string.Format(Se.Language.General.DownloadingX, Model.DisplayName);
                var finalPath = LlamaCppServerManager.GetModelPath(Model.FileName);
                var tempPath = finalPath + TemporaryFileExtension;
                await _downloadService.DownloadModel(Model.Url, tempPath, MakeProgress(), token);
                if (token.IsCancellationRequested)
                {
                    TryDelete(tempPath);
                    Close();
                    return;
                }

                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }

                File.Move(tempPath, finalPath);
            }

            if (Model?.MmprojFileName != null && Model.MmprojUrl != null &&
                (ForceModelDownload || !LlamaCppServerManager.IsModelInstalled(Model.MmprojFileName)))
            {
                TitleText = string.Format(Se.Language.General.DownloadingX, Model.MmprojFileName);
                var finalPath = LlamaCppServerManager.GetModelPath(Model.MmprojFileName);
                var tempPath = finalPath + TemporaryFileExtension;
                await _downloadService.DownloadModel(Model.MmprojUrl, tempPath, MakeProgress(), token);
                if (token.IsCancellationRequested)
                {
                    TryDelete(tempPath);
                    Close();
                    return;
                }

                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }

                File.Move(tempPath, finalPath);
            }

            OkPressed = true;
            Close();
        }
        catch (OperationCanceledException)
        {
            Close();
        }
        catch (Exception ex)
        {
            ProgressText = "Download failed";
            Error = ex.Message;
            Se.LogError(ex, "Error downloading llama.cpp");
        }
    }

    private IProgress<float> MakeProgress()
    {
        return new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, percentage.ToString(CultureInfo.InvariantCulture));
        });
    }

    /// <summary>
    /// Records the downloaded engine archive's hash in a <c>.installed.sha256</c> sidecar so the
    /// auto-translate window can later tell whether the install is outdated. Best-effort.
    /// </summary>
    private void WriteInstalledHash(string folder, Stream engineStream)
    {
        try
        {
            var key = DownloadHashManager.ResolveLlamaCppKey(Variant);
            if (string.IsNullOrEmpty(key) || engineStream.Length == 0)
            {
                return;
            }

            engineStream.Position = 0;
            var hash = Sha256Util.ComputeSha256(engineStream);

            var sidecar = Path.Combine(folder, ".installed.sha256");
            File.WriteAllText(sidecar, key + Environment.NewLine + hash);
        }
        catch
        {
            // ignore — hash side-car is best-effort
        }
    }

    private static void MakeExecutable(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            MacHelper.MakeExecutable(path);
        }
        else if (OperatingSystem.IsLinux())
        {
            LinuxHelper.MakeExecutable(path);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }
}
