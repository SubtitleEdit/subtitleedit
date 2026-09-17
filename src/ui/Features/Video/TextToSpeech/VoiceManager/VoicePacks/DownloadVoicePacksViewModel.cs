using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;

public partial class VoicePackItem : ObservableObject
{
    public VoicePack Pack { get; }
    public string Name => Pack.Name;
    public string Description => Pack.Description;
    public string Details { get; }
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private string _status = string.Empty;

    public VoicePackItem(VoicePack pack)
    {
        Pack = pack;
        Details = string.Format(Se.Language.Video.TextToSpeech.VoicePackDetailsXVoicesYSizeZLicense,
            pack.VoiceCount, Utilities.FormatBytesToDisplayFileSize(pack.SizeBytes), pack.License);
    }

    // A list row or combo box value is announced by ToString() unless its template is a bare
    // text block - without this a screen reader reads the class name (#12087).
    public override string ToString() => Name;
}

/// <summary>
/// Picks voice packs and a target cloning engine, downloads each pack, and imports every WAV
/// in it (with its transcript sidecar) through the engine's own import, which resamples to what
/// that engine wants. Voices the engine already lists by that name are skipped rather than
/// duplicated with a numeric suffix.
/// </summary>
public partial class DownloadVoicePacksViewModel : ObservableObject
{
    public ObservableCollection<VoicePackItem> Packs { get; } = new();
    public ObservableCollection<ITtsEngine> TargetEngines { get; } = new();

    [ObservableProperty] private ITtsEngine? _selectedTargetEngine;
    [ObservableProperty] private bool _isDownloading;
    [ObservableProperty] private bool _isIdle = true;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private string _error = string.Empty;

    public Window? Window { get; set; }
    public int InstalledCount { get; private set; }
    public int SkippedCount { get; private set; }
    public ITtsEngine? TargetEngine { get; private set; }

    private readonly IVoicePackDownloadService _downloadService;
    private readonly IWindowService _windowService;
    private CancellationTokenSource? _cts;

    public DownloadVoicePacksViewModel(IVoicePackDownloadService downloadService, IWindowService windowService)
    {
        _downloadService = downloadService;
        _windowService = windowService;
        foreach (var pack in VoicePackCatalog.All)
        {
            Packs.Add(new VoicePackItem(pack));
        }
    }

    internal void Initialize(List<ITtsEngine> cloningEngines, ITtsEngine selected)
    {
        foreach (var engine in cloningEngines)
        {
            TargetEngines.Add(engine);
        }

        SelectedTargetEngine = TargetEngines.FirstOrDefault(e => ReferenceEquals(e, selected)) ?? TargetEngines.FirstOrDefault();
    }

    partial void OnIsDownloadingChanged(bool value)
    {
        IsIdle = !value;
    }

    [RelayCommand]
    private async Task DownloadAndInstall()
    {
        var target = SelectedTargetEngine;
        var selected = Packs.Where(p => p.IsSelected).ToList();
        if (Window == null || target == null || IsDownloading)
        {
            return;
        }

        if (selected.Count == 0)
        {
            Error = Se.Language.Video.TextToSpeech.SelectAtLeastOneVoicePack;
            return;
        }

        var consent = await VoiceCloneConsentPrompt.EnsureAsync(target, Window,
            () => _windowService.ShowDialogAsync<VoiceCloneConsentWindow, VoiceCloneConsentViewModel>(Window, _ => { }));
        if (!consent)
        {
            return;
        }

        Error = string.Empty;
        IsDownloading = true;
        TargetEngine = target;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            var existing = (await target.GetVoices(string.Empty))
                .Select(v => v.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var item in selected)
            {
                token.ThrowIfCancellationRequested();
                ProgressValue = 0;
                ProgressText = string.Format(Se.Language.Video.TextToSpeech.DownloadingXDotDotDot, item.Name);
                item.Status = Se.Language.General.Download + "...";

                using var stream = new MemoryStream();
                var progress = new Progress<float>(p => ProgressValue = Math.Clamp(p * 100, 0, 100));
                await _downloadService.DownloadPack(item.Pack, stream, progress, token);

                ProgressText = string.Format(Se.Language.Video.TextToSpeech.InstallingXDotDotDot, item.Name);
                var installProgress = CreateInstallProgress();
                var (installed, skipped) = await Task.Run(() => InstallPack(target, stream, existing, token, installProgress), token);
                InstalledCount += installed;
                SkippedCount += skipped;
                item.Status = string.Format(Se.Language.Video.TextToSpeech.XVoicesInstalledYSkipped, installed, skipped);
                item.IsSelected = false;
            }

            ProgressText = string.Format(Se.Language.Video.TextToSpeech.XVoicesInstalledYSkipped, InstalledCount, SkippedCount);
            Se.WriteToolsLog($"Voice packs: installed {InstalledCount} voices into {target.Name} ({SkippedCount} already present)");
            Window.Close();
        }
        catch (OperationCanceledException)
        {
            ProgressText = string.Empty;
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Voice pack download/install failed");
            Error = ex.Message;
            ProgressText = string.Empty;
        }
        finally
        {
            IsDownloading = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// The install runs on a thread-pool thread, but <see cref="ProgressValue"/> is bound to the
    /// progress bar, so it must only change on the UI thread. <see cref="Progress{T}"/> captures
    /// the UI thread's synchronization context here and marshals every report back to it.
    /// </summary>
    internal IProgress<double> CreateInstallProgress()
    {
        return new Progress<double>(p => ProgressValue = p);
    }

    internal static (int installed, int skipped) InstallPack(ITtsEngine target, Stream zipStream, HashSet<string> existing, CancellationToken token, IProgress<double> progress)
    {
        zipStream.Position = 0;
        var tempFolder = Path.Combine(Path.GetTempPath(), "se-voice-pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempFolder);
        try
        {
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true))
            {
                foreach (var entry in archive.Entries)
                {
                    // Flat pack: ignore folders, and never let an entry name climb out of temp.
                    var name = Path.GetFileName(entry.FullName);
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    entry.ExtractToFile(Path.Combine(tempFolder, name), overwrite: true);
                }
            }

            var wavs = Directory.GetFiles(tempFolder, "*.wav").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            var installed = 0;
            var skipped = 0;
            for (var i = 0; i < wavs.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var wav = wavs[i];
                var voiceName = Path.GetFileNameWithoutExtension(wav).Replace('_', ' ');
                if (existing.Contains(voiceName))
                {
                    skipped++;
                }
                else
                {
                    var transcript = VoiceReferenceTranscript.Read(wav) ?? string.Empty;
                    if (VoiceCloneImporter.Import(target, wav, transcript))
                    {
                        installed++;
                        existing.Add(voiceName);
                    }
                    else
                    {
                        Se.WriteToolsLog($"Voice packs: '{Path.GetFileName(wav)}' could not be imported into {target.Name}");
                    }
                }

                progress.Report(100.0 * (i + 1) / wavs.Count);
            }

            return (installed, skipped);
        }
        finally
        {
            try
            {
                Directory.Delete(tempFolder, recursive: true);
            }
            catch
            {
                // temp leftovers are harmless
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsDownloading)
        {
            _cts?.Cancel();
            return;
        }

        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }

    internal void OnClosing()
    {
        _cts?.Cancel();
    }
}
