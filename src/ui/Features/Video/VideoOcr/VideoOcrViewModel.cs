using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.CrispEmbedSettings;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Media;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public partial class VideoOcrViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<VideoOcrEngineItem> _engines;
    [ObservableProperty] private VideoOcrEngineItem _selectedEngine;
    [ObservableProperty] private bool _isPaddleEngine;
    [ObservableProperty] private bool _isOllamaEngine;
    [ObservableProperty] private bool _isGlmEngine;
    [ObservableProperty] private bool _isLlamaCppEngine;
    [ObservableProperty] private bool _isCrispEmbedEngine;
    [ObservableProperty] private bool _isAppleVisionEngine;
    [ObservableProperty] private string _selectedEngineDescription;
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? _selectedDictionary;
    [ObservableProperty] private bool _doFixOcrErrors;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleLanguage;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _appleVisionLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedAppleVisionLanguage;
    [ObservableProperty] private string _ollamaUrl;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private string _ollamaLanguage;
    [ObservableProperty] private string _glmUrl;
    [ObservableProperty] private string _glmModel;
    [ObservableProperty] private string _glmApiKey;
    [ObservableProperty] private string _glmLanguage;
    [ObservableProperty] private ObservableCollection<LlamaCppModelDisplay> _llamaCppModels;
    [ObservableProperty] private LlamaCppModelDisplay? _selectedLlamaCppModel;
    [ObservableProperty] private string _llamaCppLanguage;
    [ObservableProperty] private string _llamaCppServerButtonText;
    [ObservableProperty] private ObservableCollection<CrispEmbedBackend> _crispEmbedBackends;
    [ObservableProperty] private CrispEmbedBackend? _selectedCrispEmbedBackend;
    [ObservableProperty] private ObservableCollection<CrispEmbedModelDisplay> _crispEmbedModels;
    [ObservableProperty] private CrispEmbedModelDisplay? _selectedCrispEmbedModel;
    [ObservableProperty] private int _framesPerSecond;
    [ObservableProperty] private int _brightnessMinimum;
    [ObservableProperty] private int _textSimilarityPercent;
    [ObservableProperty] private int _maxGapMs;
    [ObservableProperty] private int _minDurationMs;
    [ObservableProperty] private bool _addAssaPositionTag;
    [ObservableProperty] private ObservableCollection<VideoOcrLineItem> _lines;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isOkEnabled;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private Bitmap? _previewBitmap;
    [ObservableProperty] private double _previewPositionSeconds;
    [ObservableProperty] private double _durationSeconds;
    [ObservableProperty] private string _previewPositionText;
    [ObservableProperty] private int _videoWidth;
    [ObservableProperty] private int _videoHeight;
    [ObservableProperty] private int _selectionX;
    [ObservableProperty] private int _selectionY;
    [ObservableProperty] private int _selectionWidth;
    [ObservableProperty] private int _selectionHeight;
    [ObservableProperty] private string _scanAreaText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle ResultSubtitle { get; private set; } = new();
    public CropAreaSelector? CropSelector { get; set; }

    private string _videoFileName = string.Empty;

    /// <summary>The video being OCR'ed - the window shows its file name in the title.</summary>
    public string VideoFileName => _videoFileName;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly ISpellCheckManager _spellCheckManager;
    private readonly IOcrFixEngine _ocrFixEngine;
    private Process? _ffmpegProcess;
    private long _extractedFrames;
    private readonly DispatcherTimer _previewTimer;
    private bool _previewLoading;
    private bool _previewLoadQueued;

    // The CrispEmbed model the user picked this session, so coming back to a backend re-selects
    // it. Held here rather than in Se.Settings so browsing the list does not change the saved
    // choice - see OnSelectedCrispEmbedModelChanged. Empty until they pick one, and the saved
    // model is the preference until then.
    private string _lastCrispEmbedModelName = string.Empty;

    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);

    private readonly IWindowService _windowService;

    public VideoOcrViewModel(IWindowService windowService, ISpellCheckManager spellCheckManager, IOcrFixEngine ocrFixEngine)
    {
        _windowService = windowService;
        _spellCheckManager = spellCheckManager;
        _ocrFixEngine = ocrFixEngine;
        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();

        Engines = new ObservableCollection<VideoOcrEngineItem>(VideoOcrEngineItem.GetEngines());
        SelectedEngine = Engines[0];
        PaddleLanguages = new ObservableCollection<OcrLanguage2>(PaddleOcr.GetLanguages());
        SelectedPaddleLanguage = PaddleLanguages.FirstOrDefault(p => p.Code == "en");
        AppleVisionLanguages = new ObservableCollection<OcrLanguage2>(AppleVisionOcr.GetLanguages().OrderBy(p => p.ToString()));
        SelectedAppleVisionLanguage = AppleVisionLanguages.FirstOrDefault(p => p.Code == "en-US") ?? AppleVisionLanguages.FirstOrDefault();
        Lines = new ObservableCollection<VideoOcrLineItem>();

        OllamaUrl = string.Empty;
        OllamaModel = string.Empty;
        OllamaLanguage = string.Empty;
        GlmUrl = string.Empty;
        GlmModel = string.Empty;
        GlmApiKey = string.Empty;
        GlmLanguage = string.Empty;
        LlamaCppModels = new ObservableCollection<LlamaCppModelDisplay>();
        LlamaCppLanguage = string.Empty;
        LlamaCppServerButtonText = Se.Language.General.StartServer;
        // For burned-in video, only the backends that measured well are offered, best first
        // (real-footage clips with burned real SRTs as ground truth, 2026-08-26): GLM-OCR
        // 19/24 lines exact at ~1.1 s/frame; DeepSeek-OCR-2 was the close second in the
        // 2026-08-12 frame corpus; PP-OCRv6 is the light option (79 MB, detector-based) and
        // holds up on ordinary backgrounds. GOT-OCR2 (13/24, 27 phantom lines from textless
        // frames) and Qwen3-VL-2B (18/24, 22 phantom lines, 1.7 s/frame) are left to the
        // subtitle-bitmap OCR window, whose clean crops they were tuned for.
        var videoBackendNames = new[] { "GLM-OCR", "DeepSeek-OCR-2", "PP-OCRv6" };
        CrispEmbedBackends = new ObservableCollection<CrispEmbedBackend>(
            videoBackendNames
                .Select(name => CrispEmbedEngine.GetBackends().FirstOrDefault(p => p.Name == name))
                .Where(p => p != null)
                .Select(p => p!));
        CrispEmbedModels = new ObservableCollection<CrispEmbedModelDisplay>();
        ProgressText = string.Empty;
        PreviewPositionText = string.Empty;
        ScanAreaText = string.Empty;

        // LoadSettings re-selects the saved engine below, which fills this in via
        // OnSelectedEngineChanged - this is only the non-nullable seed.
        SelectedEngineDescription = string.Empty;

        // One-shot debounce for preview loading: restarted on every slider change.
        _previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _previewTimer.Tick += (s, e) =>
        {
            _previewTimer.Stop();
            LoadPreview();
        };

        LoadSettings();
    }

    public void Initialize(string videoFileName)
    {
        _videoFileName = videoFileName;
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);

        // Media probing can be slow (network shares) - keep it off the UI thread.
        Task.Run(() =>
        {
            FfmpegMediaInfo2? mediaInfo = null;
            try
            {
                mediaInfo = FfmpegMediaInfo2.Parse(_videoFileName);
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Video OCR: could not read video info from " + _videoFileName);
            }

            Dispatcher.UIThread.Post(async () =>
            {
                if (mediaInfo == null || mediaInfo.Dimension.Width <= 0 || mediaInfo.Dimension.Height <= 0 ||
                    mediaInfo.Duration == null)
                {
                    await MessageBox.Show(
                        Window!,
                        Se.Language.Video.VideoOcr.UnableToReadVideoTitle,
                        string.Format(Se.Language.Video.VideoOcr.UnableToReadVideoMessage, _videoFileName),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    Window?.Close();
                    return;
                }

                VideoWidth = mediaInfo.Dimension.Width;
                VideoHeight = mediaInfo.Dimension.Height;
                DurationSeconds = mediaInfo.Duration.TotalSeconds;

                var settings = Se.Settings.Video.VideoOcr;
                SelectionX = (int)Math.Round(settings.CropXPercent * VideoWidth / 100.0);
                SelectionY = (int)Math.Round(settings.CropYPercent * VideoHeight / 100.0);
                SelectionWidth = (int)Math.Round(settings.CropWidthPercent * VideoWidth / 100.0);
                SelectionHeight = (int)Math.Round(settings.CropHeightPercent * VideoHeight / 100.0);
                ClampSelection();

                PreviewPositionSeconds = Math.Min(DurationSeconds * 0.2, 120);
                LoadPreview();
            });
        });
    }

    partial void OnPreviewPositionSecondsChanged(double value)
    {
        PreviewPositionText = TimeSpan.FromSeconds(value).ToString(@"h\:mm\:ss");
        _previewTimer.Stop();
        _previewTimer.Start();
    }

    partial void OnSelectionXChanged(int value) => UpdateScanAreaText();
    partial void OnSelectionYChanged(int value) => UpdateScanAreaText();
    partial void OnSelectionWidthChanged(int value) => UpdateScanAreaText();
    partial void OnSelectionHeightChanged(int value) => UpdateScanAreaText();

    private void UpdateScanAreaText()
    {
        ScanAreaText = $"{SelectionWidth}x{SelectionHeight} ({SelectionX},{SelectionY})";
    }

    partial void OnSelectedEngineChanged(VideoOcrEngineItem value)
    {
        IsPaddleEngine = value.EngineType is OcrEngineType.PaddleOcrStandalone or OcrEngineType.PaddleOcrPython;
        IsOllamaEngine = value.EngineType == OcrEngineType.Ollama;
        IsGlmEngine = value.EngineType == OcrEngineType.Glm;
        IsLlamaCppEngine = value.EngineType == OcrEngineType.LlamaCpp;
        IsCrispEmbedEngine = value.EngineType == OcrEngineType.CrispEmbed;
        IsAppleVisionEngine = value.EngineType == OcrEngineType.AppleVision;
        SelectedEngineDescription = value.Description;

        if (IsLlamaCppEngine && LlamaCppModels.Count == 0)
        {
            var savedModelName = Path.GetFileName(Se.Settings.Video.VideoOcr.LlamaCppModel);
            SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(LlamaCppModels, LlamaCppServerManager.GetAllOcrModels(), savedModelName);
        }

        if (IsCrispEmbedEngine && SelectedCrispEmbedBackend == null)
        {
            SelectedCrispEmbedBackend =
                CrispEmbedBackends.FirstOrDefault(p => p.Name == Se.Settings.Video.VideoOcr.CrispEmbedBackend)
                ?? CrispEmbedBackends.FirstOrDefault();
        }
    }

    partial void OnSelectedCrispEmbedBackendChanged(CrispEmbedBackend? value)
    {
        CrispEmbedModels.Clear();
        if (value == null)
        {
            SelectedCrispEmbedModel = null;
            return;
        }

        foreach (var model in value.Models)
        {
            CrispEmbedModels.Add(new CrispEmbedModelDisplay { Backend = value, Model = model });
        }

        // The model the user last picked this session, or the saved one until they pick something.
        var preferred = string.IsNullOrEmpty(_lastCrispEmbedModelName)
            ? Se.Settings.Video.VideoOcr.CrispEmbedModel
            : _lastCrispEmbedModelName;

        SelectedCrispEmbedModel = CrispEmbedModels.FirstOrDefault(p => p.Model.Name == preferred)
                                  ?? CrispEmbedModels.FirstOrDefault(p => value.IsModelInstalled(p.Model))
                                  ?? CrispEmbedModels.FirstOrDefault();
    }

    partial void OnSelectedCrispEmbedModelChanged(CrispEmbedModelDisplay? value)
    {
        if (value == null)
        {
            return;
        }

        // Remembered in the view model, not written straight to Se.Settings: browsing the backend
        // list changed the saved engine and model even when the window was cancelled. SaveSettings
        // persists the final choice on OK.
        _lastCrispEmbedModelName = value.Model.Name;
    }

    /// <summary>
    /// Opens the CrispEmbed dialog: engine install state and hardware build, every backend's
    /// models, and the (re-)download buttons for both. Re-downloading the engine there re-asks
    /// CPU/Vulkan/CUDA, the only way to change hardware build after the first install (#13400).
    /// </summary>
    [RelayCommand]
    private async Task ShowCrispEmbedSettings()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<CrispEmbedSettingsWindow, CrispEmbedSettingsViewModel>(
            Window, vm => vm.Initialize());

        (Window as VideoOcrWindow)?.RefreshDownloadDots();
    }

    private async Task<bool> EnsureCrispEmbedReady()
    {
        if (Window == null || SelectedCrispEmbedBackend is not { } backend || SelectedCrispEmbedModel is not { } model)
        {
            return false;
        }

        return await CrispEmbedDownloadHelper.EnsureReadyAsync(
            Window, _windowService, backend, model.Model,
            onEngineDownloadClosed: () => (Window as VideoOcrWindow)?.RefreshDownloadDots(),
            onModelDownloadClosed: () => (Window as VideoOcrWindow)?.RefreshDownloadDots());
    }

    // No OnSelectedLlamaCppModelChanged: it used to write Se.Settings.Video.VideoOcr.LlamaCppModel
    // straight away, so browsing the model list changed the saved choice even when the window was
    // cancelled. SaveSettings persists the selected model when the window is accepted.

    private void UpdateLlamaCppServerButtonText()
    {
        LlamaCppServerButtonText = LlamaCppServerManager.IsServerRunning ? Se.Language.General.StopServer : Se.Language.General.StartServer;
    }

    [RelayCommand]
    private async Task DownloadLlamaCpp()
    {
        if (Window == null)
        {
            return;
        }

        var model = SelectedLlamaCppModel?.Model;
        var forceModelDownload = false;
        if (model != null && LlamaCppServerManager.IsModelInstalled(model))
        {
            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.Download,
                string.Format(Se.Language.Translate.XIsAlreadyDownloadedReDownload, model.DisplayName),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            forceModelDownload = true;
        }

        var downloaded = await LlamaCppDownloadHelper.DownloadAsync(Window, _windowService, model, forceModelDownload: forceModelDownload);
        if (downloaded != null)
        {
            var selectName = string.IsNullOrEmpty(downloaded) ? model?.FileName : downloaded;
            SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(LlamaCppModels, LlamaCppServerManager.GetAllOcrModels(), selectName);
            (Window as VideoOcrWindow)?.RefreshDownloadDots();
        }
    }

    [RelayCommand]
    private async Task ToggleLlamaCppServer()
    {
        if (Window == null)
        {
            return;
        }

        if (LlamaCppServerManager.IsServerRunning)
        {
            LlamaCppServerManager.StopServer();
            UpdateLlamaCppServerButtonText();
            return;
        }

        await EnsureLlamaCppReady();
        UpdateLlamaCppServerButtonText();
    }

    private async Task<bool> EnsureLlamaCppReady()
    {
        if (Window == null)
        {
            return false;
        }

        var model = SelectedLlamaCppModel?.Model;
        if (model == null)
        {
            return false;
        }

        var engineInstalled = LlamaCppServerManager.IsEngineInstalled();
        var modelInstalled = LlamaCppServerManager.IsModelInstalled(model);
        if (!engineInstalled || !modelInstalled)
        {
            string message;
            if (!engineInstalled && !modelInstalled)
            {
                message = Se.Language.Ocr.LlamaCppDownloadEngineAndModelPrompt;
            }
            else if (!engineInstalled)
            {
                message = Se.Language.Ocr.LlamaCppDownloadEnginePrompt;
            }
            else
            {
                message = Se.Language.Ocr.LlamaCppDownloadModelPrompt;
            }

            var answer = await MessageBox.Show(
                Window,
                Se.Language.General.Download,
                message,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await LlamaCppDownloadHelper.DownloadAsync(Window, _windowService, model);
            if (!LlamaCppServerManager.IsEngineInstalled() || !LlamaCppServerManager.IsModelInstalled(model))
            {
                return false;
            }
        }

        SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(LlamaCppModels, LlamaCppServerManager.GetAllOcrModels(), model.FileName);
        (Window as VideoOcrWindow)?.RefreshDownloadDots();

        try
        {
            await LlamaCppServerManager.EnsureServerRunningAsync(model, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                ex.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        UpdateLlamaCppServerButtonText();
        return true;
    }

    private void LoadPreview()
    {
        if (string.IsNullOrEmpty(_videoFileName) || DurationSeconds <= 0)
        {
            return;
        }

        if (_previewLoading)
        {
            _previewLoadQueued = true; // load again when the current load finishes
            return;
        }

        _previewLoading = true;
        var seconds = PreviewPositionSeconds;

        Task.Run(() =>
        {
            try
            {
                var fileName = FfmpegGenerator.GetScreenShot(_videoFileName, seconds.ToString("0.###", CultureInfo.InvariantCulture));
                if (File.Exists(fileName) && new FileInfo(fileName).Length > 0)
                {
                    var bitmap = new Bitmap(fileName);
                    Dispatcher.UIThread.Post(() =>
                    {
                        var oldBitmap = PreviewBitmap;
                        PreviewBitmap = bitmap;
                        oldBitmap?.Dispose();
                        CropSelector?.InvalidateVisual();
                    });

                    try
                    {
                        File.Delete(fileName);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Video OCR: could not load preview frame");
            }
            finally
            {
                _previewLoading = false;
                if (_previewLoadQueued)
                {
                    _previewLoadQueued = false;
                    Dispatcher.UIThread.Post(LoadPreview);
                }
            }
        });
    }

    [RelayCommand]
    private void SetScanAreaBottomThird()
    {
        CropSelector?.SetSelectionVideoRect(0, VideoHeight * 2 / 3, VideoWidth, VideoHeight / 3);
    }

    [RelayCommand]
    private void SetScanAreaBottomHalf()
    {
        CropSelector?.SetSelectionVideoRect(0, VideoHeight / 2, VideoWidth, VideoHeight / 2);
    }

    [RelayCommand]
    private void SetScanAreaFullFrame()
    {
        CropSelector?.SetSelectionVideoRect(0, 0, VideoWidth, VideoHeight);
    }

    /// <summary>
    /// OCRs only the frame at the current preview position so the user can validate the scan
    /// area, engine, and settings without scanning the whole video. The result is shown in the
    /// status text.
    /// </summary>
    [RelayCommand]
    private async Task TestOcr()
    {
        if (IsRunning || string.IsNullOrEmpty(_videoFileName) || VideoWidth <= 0 || VideoHeight <= 0)
        {
            return;
        }

        var engineOk = await EnsureEngineIsAvailable();
        if (!engineOk)
        {
            return;
        }

        ClampSelection();

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        IsRunning = true;
        ProgressText = Se.Language.Video.VideoOcr.TestOcrRunning;

        var frameFileName = Path.Combine(Path.GetTempPath(), "se_video_ocr_test_" + Guid.NewGuid() + ".jpg");
        try
        {
            await ExtractSingleFrame(frameFileName, PreviewPositionSeconds, cancellationToken);
            if (!File.Exists(frameFileName) || new FileInfo(frameFileName).Length == 0)
            {
                throw new Exception("Could not extract the current frame - see log for the ffmpeg command line.");
            }

            var group = new VideoOcrFrameGroup { RepresentativeFileName = frameFileName };
            await OcrGroups(new List<VideoOcrFrameGroup> { group }, () => { }, _ => { }, cancellationToken);

            ProgressText = string.IsNullOrWhiteSpace(group.Text)
                ? Se.Language.Video.VideoOcr.TestOcrNoTextFound
                : string.Format(Se.Language.Video.VideoOcr.TestOcrResultX, group.Text.ReplaceLineEndings(" | "));
        }
        catch (OperationCanceledException)
        {
            ProgressText = string.Empty;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Video OCR: test on current frame failed");
            ProgressText = string.Empty;

            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                exception.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            IsRunning = false;

            try
            {
                File.Delete(frameFileName);
            }
            catch
            {
                // ignore
            }
        }
    }

    private async Task ExtractSingleFrame(string outputFileName, double positionSeconds, CancellationToken cancellationToken)
    {
        // -ss before -i: seek in the demuxer, so a test frame late in a long video is still fast.
        var arguments = $"-nostdin -y -ss {positionSeconds.ToString("0.###", CultureInfo.InvariantCulture)} " +
                        $"-i \"{_videoFileName}\" " +
                        $"-vf \"{GetCropAndScaleFilter()}\" " +
                        $"-frames:v 1 -q:v 2 \"{outputFileName}\"";

        Se.WriteToolsLog("Video OCR: extracting test frame - ffmpeg " + arguments);
        var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { });

#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch
            {
                // ignore
            }

            throw;
        }
    }

    [RelayCommand]
    private async Task StartOcr()
    {
        if (IsRunning || string.IsNullOrEmpty(_videoFileName) || VideoWidth <= 0 || VideoHeight <= 0)
        {
            return;
        }

        var engineOk = await EnsureEngineIsAvailable();
        if (!engineOk)
        {
            return;
        }

        ClampSelection();
        SaveSettings();

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        IsRunning = true;
        IsOkEnabled = false;
        ProgressValue = 0;
        Lines.Clear();

        var framesFolder = Path.Combine(Path.GetTempPath(), "se_video_ocr_" + Guid.NewGuid());
        Directory.CreateDirectory(framesFolder);

        try
        {
            await ExtractFrames(framesFolder, cancellationToken);

            var frameFileNames = Directory.GetFiles(framesFolder, "*.jpg").OrderBy(p => p, StringComparer.Ordinal).ToList();
            if (frameFileNames.Count == 0)
            {
                throw new Exception("No frames were extracted from the video - see log for the ffmpeg command line.");
            }

            var lastAnalyzeUpdate = 0L;
            var groups = await Task.Run(() => VideoOcrFrameGrouper.Group(
                frameFileNames,
                BrightnessMinimum,
                Se.Settings.Video.VideoOcr.ImageSimilarityPercent,
                (current, total) =>
                {
                    var now = Environment.TickCount64;
                    if (now - lastAnalyzeUpdate > 200 || current == total)
                    {
                        lastAnalyzeUpdate = now;
                        Dispatcher.UIThread.Post(() =>
                        {
                            ProgressText = string.Format(Se.Language.Video.VideoOcr.AnalyzingFramesXY, current, total);
                            ProgressValue = total == 0 ? 0 : current * 100.0 / total;
                        });
                    }
                },
                cancellationToken), cancellationToken);

            InitializeOcrFixEngine(contextSubtitle: null);

            await RunOcr(groups, cancellationToken);

            var mergedLines = VideoOcrLineBuilder.Build(groups, FramesPerSecond, TextSimilarityPercent, MaxGapMs, MinDurationMs);

            var lastRefineUpdate = 0L;
            await VideoOcrTimingRefiner.RefineAsync(
                mergedLines,
                new VideoOcrTimingRefiner.Context
                {
                    VideoFileName = _videoFileName,
                    FramesFolder = framesFolder,
                    CoarseFps = FramesPerSecond,
                    BrightnessMinimum = BrightnessMinimum,
                    ImageSimilarityPercent = Se.Settings.Video.VideoOcr.ImageSimilarityPercent,
                    CropAndScaleFilter = GetCropAndScaleFilter(),
                },
                (current, total) =>
                {
                    var now = Environment.TickCount64;
                    if (now - lastRefineUpdate > 200 || current == total)
                    {
                        lastRefineUpdate = now;
                        Dispatcher.UIThread.Post(() =>
                        {
                            ProgressText = string.Format(Se.Language.Video.VideoOcr.RefiningTimingXY, current, total);
                            ProgressValue = total == 0 ? 0 : current * 100.0 / total;
                        });
                    }
                },
                cancellationToken);

            var positionTag = string.Empty;
            if (AddAssaPositionTag)
            {
                var relativeX = (SelectionX + SelectionWidth / 2.0) / VideoWidth;
                var relativeY = (SelectionY + SelectionHeight / 2.0) / VideoHeight;
                positionTag = VideoOcrLineBuilder.GetAssaAlignmentTag(relativeX, relativeY);
            }

            Lines.Clear();
            var number = 1;
            foreach (var line in mergedLines)
            {
                var item = new VideoOcrLineItem
                {
                    Number = number++,
                    StartTime = TimeSpan.FromMilliseconds(line.StartMs),
                    EndTime = TimeSpan.FromMilliseconds(line.EndMs),
                    Text = positionTag + line.Text,
                };
                item.PropertyChanged += LineItemPropertyChanged;
                Lines.Add(item);
            }

            ApplyOcrFixes();

            IsRunning = false;
            IsOkEnabled = Lines.Count > 0;
            ProgressValue = 0;
            ProgressText = string.Format(Se.Language.Video.VideoOcr.LinesFoundX, Lines.Count);

            if (Lines.Count == 0)
            {
                await MessageBox.Show(
                    Window!,
                    Se.Language.Video.VideoOcr.NoLinesFoundTitle,
                    Se.Language.Video.VideoOcr.NoLinesFoundMessage,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (OperationCanceledException)
        {
            IsRunning = false;
            IsOkEnabled = Lines.Count > 0;
            ProgressValue = 0;
            ProgressText = string.Empty;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Video OCR failed");

            IsRunning = false;
            ProgressValue = 0;
            ProgressText = string.Empty;

            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                exception.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            try
            {
                Directory.Delete(framesFolder, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <summary>The crop (and optional downscale) part of the extraction filter - shared by
    /// the scan, the test frame and the timing refinement so they all see the same pixels.</summary>
    private string GetCropAndScaleFilter()
    {
        var scale = string.Empty;
        var maxImageWidth = Se.Settings.Video.VideoOcr.MaxImageWidth;
        if (maxImageWidth > 0 && SelectionWidth > maxImageWidth)
        {
            scale = $",scale={maxImageWidth}:-2";
        }

        return $"crop={SelectionWidth}:{SelectionHeight}:{SelectionX}:{SelectionY}{scale}";
    }

    private async Task ExtractFrames(string framesFolder, CancellationToken cancellationToken)
    {
        // JPEG (near-lossless q=2) instead of PNG: a long video at 5 fps produces tens of
        // thousands of frames, and PNG would need gigabytes of temp disk space.
        var outputPattern = Path.Combine(framesFolder, "img%06d.jpg");
        var arguments = $"-nostdin -y -i \"{_videoFileName}\" " +
                        $"-vf \"fps={FramesPerSecond.ToString(CultureInfo.InvariantCulture)}," +
                        $"{GetCropAndScaleFilter()}\" " +
                        $"-q:v 2 -start_number 0 \"{outputPattern}\"";

        _extractedFrames = 0;
        var totalFrames = Math.Max(1, (long)Math.Round(DurationSeconds * FramesPerSecond));

        Se.WriteToolsLog("Video OCR: extracting frames - ffmpeg " + arguments);
        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, ExtractFramesOutputHandler);

#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        using var timer = new System.Timers.Timer(200);
        timer.Elapsed += (s, e) =>
        {
            var percentage = Math.Clamp(_extractedFrames * 100.0 / totalFrames, 0, 100);
            Dispatcher.UIThread.Post(() =>
            {
                ProgressValue = percentage;
                ProgressText = string.Format(Se.Language.Video.VideoOcr.ExtractingFramesX, (int)percentage);
            });
        };
        timer.Start();

        try
        {
            await _ffmpegProcess.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.Kill(true);
                }
            }
            catch
            {
                // ignore
            }

            throw;
        }
        finally
        {
            timer.Stop();
        }
    }

    private void ExtractFramesOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        var match = FrameFinderRegex.Match(outLine.Data);
        if (!match.Success)
        {
            return;
        }

        var arr = match.Value.Split('=');
        if (arr.Length == 2 && long.TryParse(arr[1].Trim(), out var f))
        {
            _extractedFrames = f;
        }
    }

    private async Task RunOcr(List<VideoOcrFrameGroup> groups, CancellationToken cancellationToken)
    {
        var ocrGroups = groups.Where(p => !p.IsBlank && !string.IsNullOrEmpty(p.RepresentativeFileName)).ToList();
        if (ocrGroups.Count == 0)
        {
            return;
        }

        var done = 0;
        void ReportOcrProgress()
        {
            var current = Interlocked.Increment(ref done);
            Dispatcher.UIThread.Post(() =>
            {
                ProgressText = string.Format(Se.Language.Video.VideoOcr.RunningOcrXY, current, ocrGroups.Count);
                ProgressValue = current * 100.0 / ocrGroups.Count;
            });
        }

        void AddPreviewLine(VideoOcrFrameGroup group)
        {
            if (string.IsNullOrWhiteSpace(group.Text))
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                var item = new VideoOcrLineItem
                {
                    Number = Lines.Count + 1,
                    StartTime = TimeSpan.FromMilliseconds(group.GetStartMs(FramesPerSecond)),
                    EndTime = TimeSpan.FromMilliseconds(group.GetEndMs(FramesPerSecond)),
                    Text = group.Text,
                };
                ApplyFixToItem(item, Lines.Count);
                Lines.Add(item);
            });
        }

        Dispatcher.UIThread.Post(() =>
        {
            ProgressValue = 0;
            ProgressText = string.Format(Se.Language.Video.VideoOcr.RunningOcrXY, 0, ocrGroups.Count);
        });

        await OcrGroups(ocrGroups, ReportOcrProgress, AddPreviewLine, cancellationToken);
    }

    /// <summary>
    /// Runs the selected OCR engine over the given frame groups, storing each result in
    /// <see cref="VideoOcrFrameGroup.Text"/>. Shared by the full scan and the single-frame test.
    /// </summary>
    private async Task OcrGroups(List<VideoOcrFrameGroup> ocrGroups, Action reportProgress, Action<VideoOcrFrameGroup> addPreviewLine, CancellationToken cancellationToken)
    {
        var engineType = SelectedEngine.EngineType;
        if (engineType is OcrEngineType.PaddleOcrStandalone or OcrEngineType.PaddleOcrPython)
        {
            var language = SelectedPaddleLanguage?.Code ?? "en";
            var mode = Se.Settings.Ocr.PaddleOcrMode;
            if (string.IsNullOrEmpty(mode))
            {
                mode = "mobile";
            }

            var progress = new Progress<PaddleOcrBatchProgress>(p =>
            {
                var group = ocrGroups.ElementAtOrDefault(p.Index);
                if (group != null)
                {
                    group.Text = VideoOcrLineBuilder.CleanOcrResult(p.Text);
                    group.Confidence = p.Confidence;
                    reportProgress();
                    addPreviewLine(group);
                }
            });

            // Black out everything below the brightness minimum before recognition, like
            // VideOCR does: Paddle's detector otherwise picks up darker scene text (shirt
            // prints, credits) and prepends it to subtitles. Only for the Paddle path -
            // vision/VLM engines measured better on the natural frames.
            var ocrFileNames = ocrGroups.Select(g => g.RepresentativeFileName).ToList();
            if (BrightnessMinimum > 0 && ocrGroups.Count > 0)
            {
                var maskedFolder = Path.Combine(
                    Path.GetDirectoryName(ocrGroups[0].RepresentativeFileName) ?? string.Empty, "masked");
                Directory.CreateDirectory(maskedFolder);
                Parallel.For(0, ocrGroups.Count,
                    new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount },
                    i =>
                    {
                        var source = ocrGroups[i].RepresentativeFileName;
                        var target = Path.Combine(maskedFolder, Path.GetFileName(source));
                        if (VideoOcrFrameGrouper.WriteMaskedCopy(source, target, BrightnessMinimum))
                        {
                            ocrFileNames[i] = target;
                        }
                    });
            }

            // The frames are already image files on disk, so pass them by file name -
            // one batch, no per-image decode/encode, memory stays flat.
            var batch = ocrGroups
                .Select((g, i) => new PaddleOcrBatchInput { Index = i, SourceFileName = ocrFileNames[i] })
                .ToList();

            var paddleOcr = new PaddleOcr
            {
                // Low-confidence regions in a video frame are nearly always background
                // clutter (scene text, logos) rather than subtitle text - same cut VideOCR
                // applies. Only for Video OCR; the subtitle-bitmap OCR window keeps everything.
                MinConfidencePercent = 75,
            };
            await paddleOcr.OcrBatch(engineType, batch, language, mode, progress, cancellationToken);
            if (!string.IsNullOrEmpty(paddleOcr.Error) && ocrGroups.All(p => string.IsNullOrEmpty(p.Text)))
            {
                throw new Exception("Paddle OCR failed: " + paddleOcr.Error);
            }
        }
        else if (engineType == OcrEngineType.Ollama)
        {
            using var ollamaOcr = new OllamaOcr(Se.Settings.Ocr.OllamaOcrTimeoutMinutes);
            await RunLlmOcr(ocrGroups, group => OcrWithBitmap(group, bitmap =>
                    ollamaOcr.Ocr(bitmap, OllamaUrl, OllamaModel, OllamaLanguage, cancellationToken)),
                () => ollamaOcr.Error, reportProgress, addPreviewLine, cancellationToken, CountUnknownWords);
        }
        else if (engineType == OcrEngineType.Glm)
        {
            var glmOcr = new GlmOcr(GlmApiKey);
            await RunLlmOcr(ocrGroups, group =>
                    glmOcr.Ocr(group.RepresentativeFileName, GlmUrl, GlmModel, GlmLanguage, cancellationToken),
                () => glmOcr.Error, reportProgress, addPreviewLine, cancellationToken, CountUnknownWords);
        }
        else if (engineType == OcrEngineType.LlamaCpp)
        {
            using var llamaCppOcr = new LlamaCppOcr(Se.Settings.Ocr.LlamaCppOcrTimeoutMinutes);
            var url = LlamaCppServerManager.ApiUrl;
            var modelName = SelectedLlamaCppModel?.Model.FileName is { } fileName
                ? Path.GetFileNameWithoutExtension(fileName)
                : "glmocr";
            var prompt = Se.Settings.Ocr.LlamaCppOcrPrompt;
            await RunLlmOcr(ocrGroups, group => OcrWithBitmap(group, bitmap =>
                    llamaCppOcr.Ocr(bitmap, url, modelName, LlamaCppLanguage, prompt, cancellationToken)),
                () => llamaCppOcr.Error, reportProgress, addPreviewLine, cancellationToken, CountUnknownWords);
        }
        else if (engineType == OcrEngineType.CrispEmbed)
        {
            await OcrGroupsWithCrispEmbed(ocrGroups, reportProgress, addPreviewLine, cancellationToken);
        }
        else if (engineType == OcrEngineType.AppleVision)
        {
            // Vision is synchronous, in-process CPU work with no server or API behind it, so
            // each frame goes to the thread pool rather than blocking the caller for the whole
            // scan. RunLlmOcr's fail-fast-on-first-frame check does nothing here (there is no
            // error string to report) but the loop, progress and preview are the same.
            var languageCode = SelectedAppleVisionLanguage?.Code ?? string.Empty;
            var brightnessMinimum = BrightnessMinimum;
            await RunLlmOcr(ocrGroups,
                group => Task.Run(() => OcrFrameWithAppleVision(group, languageCode, brightnessMinimum, cancellationToken), cancellationToken),
                () => string.Empty, reportProgress, addPreviewLine, cancellationToken, CountUnknownWords);
        }
    }

    private static string OcrFrameWithAppleVision(VideoOcrFrameGroup group, string languageCode, int brightnessMinimum, CancellationToken cancellationToken)
    {
        using var bitmap = SKBitmap.Decode(group.RepresentativeFileName);
        if (bitmap == null)
        {
            return string.Empty;
        }

        var observations = AppleVisionOcr.OcrObservations(bitmap, languageCode, fast: false, cancellationToken);
        var kept = VideoOcrObservationFilter.FilterByBrightness(observations, bitmap, brightnessMinimum);
        return AppleVisionTextLayout.Compose(kept);
    }

    /// <summary>
    /// Runs the frame groups through CrispEmbed. The VLM backends load the GGUF into one
    /// crispembed-server instance that stays up for the whole scan - a video produces hundreds of
    /// groups and the model load alone is seconds, so starting a server per frame would be
    /// unusable. PP-OCRv6 is a detector+recognizer pair driven through the CLI instead, one
    /// invocation per frame, which is what <see cref="CrispEmbedOcr"/> expects.
    /// </summary>
    private async Task OcrGroupsWithCrispEmbed(
        List<VideoOcrFrameGroup> ocrGroups,
        Action reportProgress,
        Action<VideoOcrFrameGroup> addPreviewLine,
        CancellationToken cancellationToken)
    {
        if (SelectedCrispEmbedBackend is not { } backend || SelectedCrispEmbedModel is not { } model)
        {
            throw new Exception(Se.Language.Ocr.CrispEmbedNotDownloaded);
        }

        using var engine = new CrispEmbedOcr(Se.Settings.Ocr.CrispEmbedOcrTimeoutMinutes);

        var started = backend.UsesTextDetector
            ? engine.StartCliPipeline(
                CrispEmbedEngine.GetCliExecutable(),
                backend.GetModelPath(model.Model),
                backend.GetDetectorPath(model.Model))
            : await engine.StartServerAsync(
                CrispEmbedEngine.GetServerExecutable(),
                backend.GetModelPath(model.Model),
                cancellationToken);

        if (!started)
        {
            throw new Exception(engine.Error);
        }

        await RunLlmOcr(ocrGroups, group => OcrWithBitmap(group, bitmap => engine.Ocr(bitmap, cancellationToken)),
            () => engine.Error, reportProgress, addPreviewLine, cancellationToken, CountUnknownWords);
    }

    private static async Task<string> OcrWithBitmap(VideoOcrFrameGroup group, Func<SKBitmap, Task<string>> ocr)
    {
        using var bitmap = SKBitmap.Decode(group.RepresentativeFileName);
        if (bitmap == null)
        {
            return string.Empty;
        }

        return await ocr(bitmap);
    }

    internal static async Task RunLlmOcr(
        List<VideoOcrFrameGroup> ocrGroups,
        Func<VideoOcrFrameGroup, Task<string>> ocr,
        Func<string> getError,
        Action reportProgress,
        Action<VideoOcrFrameGroup> addPreviewLine,
        CancellationToken cancellationToken,
        Func<string, int>? countUnknownWords = null)
    {
        var isFirst = true;
        foreach (var group in ocrGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();

            group.Text = VideoOcrLineBuilder.CleanOcrResult(await ocr(group));

            // An empty result on a group the mask says holds text is often just an unlucky
            // representative frame - e.g. white text drifting over a white wall mid-group -
            // so try frames from other parts of the group before giving up. Measured: the
            // one subtitle a 21-minute episode lost was read perfectly from the frame at
            // three quarters of its group.
            if (string.IsNullOrEmpty(group.Text) && group.EndFrame - group.StartFrame >= 2)
            {
                var span = group.EndFrame - group.StartFrame;
                foreach (var alternateIndex in new[] { group.StartFrame + span * 3 / 4, group.StartFrame + span / 4 })
                {
                    var alternateFileName = group.GetSiblingFrameFileName(alternateIndex);
                    if (alternateFileName == group.RepresentativeFileName || !File.Exists(alternateFileName))
                    {
                        continue;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    group.RepresentativeFileName = alternateFileName;
                    group.Text = VideoOcrLineBuilder.CleanOcrResult(await ocr(group));
                    if (!string.IsNullOrEmpty(group.Text))
                    {
                        break;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(group.Text) && group.EndFrame - group.StartFrame >= 2)
            {
                // Verify a non-empty read against a second frame of the group. Real subtitle
                // text is stable across the group's frames, so the two reads agree apart from
                // OCR jitter - while hallucinated ghosts (a vision model inventing text from a
                // logo or scoreboard) come out different on every frame. On disagreement the
                // longer read wins when it is substantial - a real line polluted by changing
                // scene text (rolling credits) must survive, as must long text whose verify
                // frame happened to be unreadable - and anything short is dropped as a ghost.
                var verified = await OcrVerificationFrame(group, ocr, cancellationToken);
                if (verified != null)
                {
                    var similarity = VideoOcrLineBuilder.GetTextSimilarityPercent(group.Text, verified);
                    if (similarity < TextSimilarityDefaultPercent)
                    {
                        var best = CountLettersAndDigits(verified) > CountLettersAndDigits(group.Text) ? verified : group.Text;
                        group.Text = CountLettersAndDigits(best) >= 10 ? best : string.Empty;
                    }
                    else if (verified != group.Text && countUnknownWords != null &&
                             countUnknownWords(verified) < countUnknownWords(group.Text))
                    {
                        // The two reads agree apart from OCR jitter ("I'think" / "I think") -
                        // the spell check arbitrates: the read the dictionary knows more of wins.
                        group.Text = verified;
                    }
                }
            }

            // Fail fast on a broken engine (wrong API key/URL) instead of grinding
            // through the whole video and reporting "no subtitles found".
            var error = getError();
            if (isFirst && string.IsNullOrEmpty(group.Text) && !string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            isFirst = false;
            reportProgress();
            addPreviewLine(group);
        }
    }

    // The verification threshold uses the default text similarity rather than the user's
    // merge setting: verification compares two reads of the SAME frame content, where only
    // OCR jitter separates them, so the bar is independent of how aggressively the user
    // wants consecutive lines merged.
    private const int TextSimilarityDefaultPercent = 80;

    private static async Task<string?> OcrVerificationFrame(
        VideoOcrFrameGroup group,
        Func<VideoOcrFrameGroup, Task<string>> ocr,
        CancellationToken cancellationToken)
    {
        var span = group.EndFrame - group.StartFrame;
        foreach (var alternateIndex in new[] { group.StartFrame + span * 3 / 4, group.StartFrame + span / 4 })
        {
            var alternateFileName = group.GetSiblingFrameFileName(alternateIndex);
            if (alternateFileName == group.RepresentativeFileName || !File.Exists(alternateFileName))
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var original = group.RepresentativeFileName;
            group.RepresentativeFileName = alternateFileName;
            var text = VideoOcrLineBuilder.CleanOcrResult(await ocr(group));
            group.RepresentativeFileName = original;
            return text;
        }

        return null;
    }

    private static int CountLettersAndDigits(string text)
    {
        var count = 0;
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                count++;
            }
        }

        return count;
    }

    private async Task<bool> EnsureEngineIsAvailable()
    {
        var engineType = SelectedEngine.EngineType;

        if (engineType == OcrEngineType.Glm && string.IsNullOrWhiteSpace(GlmApiKey))
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                "An API key is required for the GLM API engine.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (engineType is OcrEngineType.PaddleOcrStandalone or OcrEngineType.PaddleOcrPython)
        {
            return await PaddleOcrInstallHelper.EnsureInstalled(Window!, _windowService, engineType);
        }

        if (engineType == OcrEngineType.LlamaCpp)
        {
            return await EnsureLlamaCppReady();
        }

        if (engineType == OcrEngineType.CrispEmbed)
        {
            return await EnsureCrispEmbedReady();
        }

        return true;
    }

    private void ClampSelection()
    {
        if (VideoWidth <= 0 || VideoHeight <= 0)
        {
            return;
        }

        SelectionWidth = Math.Clamp(SelectionWidth, 16, VideoWidth);
        SelectionHeight = Math.Clamp(SelectionHeight, 16, VideoHeight);
        SelectionX = Math.Clamp(SelectionX, 0, VideoWidth - SelectionWidth);
        SelectionY = Math.Clamp(SelectionY, 0, VideoHeight - SelectionHeight);
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.VideoOcr;
        SelectedEngine = Engines.FirstOrDefault(p => p.EngineType.ToString() == settings.Engine) ?? Engines[0];
        OnSelectedEngineChanged(SelectedEngine);
        SelectedAppleVisionLanguage = AppleVisionLanguages.FirstOrDefault(p => p.Code == settings.AppleVisionLanguage)
                                      ?? SelectedAppleVisionLanguage;

        var paddleLanguage = PaddleOcr.NormalizeLanguageCode(settings.PaddleLanguage);
        SelectedPaddleLanguage = PaddleLanguages.FirstOrDefault(p => p.Code == paddleLanguage) ??
                                 PaddleLanguages.FirstOrDefault(p => p.Code == "en");
        OllamaUrl = settings.OllamaUrl;
        OllamaModel = settings.OllamaModel;
        OllamaLanguage = settings.OllamaLanguage;
        GlmUrl = settings.GlmUrl;
        GlmModel = settings.GlmModel;
        GlmApiKey = settings.GlmApiKey;
        GlmLanguage = settings.GlmLanguage;
        LlamaCppLanguage = string.IsNullOrEmpty(settings.LlamaCppLanguage) ? "English" : settings.LlamaCppLanguage;
        FramesPerSecond = Math.Clamp(settings.FramesPerSecond, 1, 30);
        BrightnessMinimum = Math.Clamp(settings.BrightnessMinimum, 0, 255);
        TextSimilarityPercent = Math.Clamp(settings.TextSimilarityPercent, 0, 100);
        MaxGapMs = Math.Clamp(settings.MaxGapMs, 0, 10_000);
        MinDurationMs = Math.Clamp(settings.MinDurationMs, 0, 10_000);
        AddAssaPositionTag = settings.AddAssaPositionTag;
        DoFixOcrErrors = settings.FixOcrErrors;
        LoadDictionaries(settings.DictionaryFileName);
    }

    /// <summary>
    /// Fills the spell check dictionary combo: "- None -" first, then the downloaded
    /// dictionaries. The saved pick wins; otherwise the first dictionary matching the
    /// engine's OCR language is chosen, so post-processing works without any setup for
    /// users who already have the dictionary.
    /// </summary>
    private void LoadDictionaries(string savedDictionaryFileName)
    {
        Dictionaries.Clear();
        Dictionaries.Add(new SpellCheckDictionaryDisplay
        {
            Name = $"- {Se.Language.General.None} -",
            DictionaryFileName = string.Empty,
        });

        List<SpellCheckDictionaryDisplay> languages;
        try
        {
            languages = _spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);
        }
        catch
        {
            languages = new List<SpellCheckDictionaryDisplay>();
        }

        Dictionaries.AddRange(LanguageFavoritesHelper.Order(languages, d => SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(d)));

        if (!string.IsNullOrEmpty(savedDictionaryFileName))
        {
            SelectedDictionary = Dictionaries.FirstOrDefault(d => d.DictionaryFileName == savedDictionaryFileName);
        }

        SelectedDictionary ??= Dictionaries.FirstOrDefault(d =>
                                   SpellCheckDictionaryDisplay.GetTwoLetterLanguageCode(d) == GetOcrTwoLetterLanguageCode())
                               ?? Dictionaries[0];
    }

    [RelayCommand]
    private async Task DownloadDictionary()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<GetDictionariesWindow, GetDictionariesViewModel>(Window);
        if (result.OkPressed && result.SelectedDictionary != null)
        {
            LoadDictionaries(Se.Settings.Video.VideoOcr.DictionaryFileName);

            // Select the just-downloaded dictionary by its file name - matching the display
            // name fails on non-English UIs (the list shows the localized culture name).
            var downloadedFileName = Path.GetFileName(result.SpellCheckDictionary?.DictionaryFileName ?? string.Empty);
            SelectedDictionary =
                (!string.IsNullOrEmpty(downloadedFileName)
                    ? Dictionaries.FirstOrDefault(d => string.Equals(
                        Path.GetFileName(d.DictionaryFileName), downloadedFileName, StringComparison.OrdinalIgnoreCase))
                    : null)
                ?? Dictionaries.FirstOrDefault(d =>
                    d.Name.Contains(result.SelectedDictionary.EnglishName, StringComparison.OrdinalIgnoreCase) ||
                    d.Name.Contains(result.SelectedDictionary.NativeName, StringComparison.OrdinalIgnoreCase))
                ?? SelectedDictionary;
        }
    }

    /// <summary>The current engine's OCR language as a two-letter code, for the dictionary auto-pick.</summary>
    private string GetOcrTwoLetterLanguageCode()
    {
        var engineType = SelectedEngine?.EngineType;
        if (engineType is OcrEngineType.PaddleOcrStandalone or OcrEngineType.PaddleOcrPython)
        {
            var code = SelectedPaddleLanguage?.Code ?? "en";
            return code.Length >= 2 ? code[..2] : "en";
        }

        if (engineType == OcrEngineType.AppleVision)
        {
            var code = SelectedAppleVisionLanguage?.Code ?? "en";
            return code.Length >= 2 ? code[..2] : "en";
        }

        // The VLM engines take a language name ("English"); default to English.
        return "en";
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.VideoOcr;
        settings.Engine = SelectedEngine.EngineType.ToString();
        settings.PaddleLanguage = SelectedPaddleLanguage?.Code ?? "en";
        settings.AppleVisionLanguage = SelectedAppleVisionLanguage?.Code ?? settings.AppleVisionLanguage;
        settings.OllamaUrl = OllamaUrl;
        settings.OllamaModel = OllamaModel;
        settings.OllamaLanguage = OllamaLanguage;
        settings.GlmUrl = GlmUrl;
        settings.GlmModel = GlmModel;
        settings.GlmApiKey = GlmApiKey;
        settings.GlmLanguage = GlmLanguage;
        if (SelectedLlamaCppModel != null)
        {
            settings.LlamaCppModel = LlamaCppServerManager.GetModelPath(SelectedLlamaCppModel.Model.FileName);
        }
        settings.LlamaCppLanguage = LlamaCppLanguage;
        settings.CrispEmbedBackend = SelectedCrispEmbedBackend?.Name ?? settings.CrispEmbedBackend;
        settings.CrispEmbedModel = SelectedCrispEmbedModel?.Model.Name ?? settings.CrispEmbedModel;
        settings.FramesPerSecond = FramesPerSecond;
        settings.BrightnessMinimum = BrightnessMinimum;
        settings.TextSimilarityPercent = TextSimilarityPercent;
        settings.MaxGapMs = MaxGapMs;
        settings.MinDurationMs = MinDurationMs;
        settings.AddAssaPositionTag = AddAssaPositionTag;
        settings.FixOcrErrors = DoFixOcrErrors;
        settings.DictionaryFileName = SelectedDictionary?.DictionaryFileName ?? string.Empty;

        if (VideoWidth > 0 && VideoHeight > 0)
        {
            settings.CropXPercent = SelectionX * 100.0 / VideoWidth;
            settings.CropYPercent = SelectionY * 100.0 / VideoHeight;
            settings.CropWidthPercent = SelectionWidth * 100.0 / VideoWidth;
            settings.CropHeightPercent = SelectionHeight * 100.0 / VideoHeight;
        }

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        if (IsRunning || Lines.Count == 0)
        {
            return;
        }

        var subtitle = new Subtitle();
        foreach (var line in Lines.OrderBy(p => p.StartTime))
        {
            subtitle.Paragraphs.Add(new Paragraph(line.Text, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
        }

        subtitle.Renumber();
        ResultSubtitle = subtitle;

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    /// <summary>Removes the given result lines and renumbers the rest (Delete key in the grid).</summary>
    internal void DeleteLines(List<VideoOcrLineItem> items)
    {
        if (IsRunning || items.Count == 0)
        {
            return;
        }

        foreach (var item in items)
        {
            Lines.Remove(item);
        }

        var number = 1;
        foreach (var line in Lines)
        {
            line.Number = number++;
        }

        IsOkEnabled = Lines.Count > 0;
    }

    /// <summary>Moves the preview to the given line's start time (double-click in the grid).</summary>
    /// <summary>
    /// Runs the OCR fix engine (replace lists + spell check) over the result lines: each
    /// line's text is replaced by the fixed text, and lines with words the dictionary does
    /// not know are marked so the table can tint them. No per-word prompting here - a video
    /// run produces hundreds of lines, so unknown words are marked for in-place fixing
    /// instead.
    /// </summary>
    internal void ApplyOcrFixes()
    {
        if (!_ocrFixEngine.IsLoaded() && Lines.Count > 0)
        {
            InitializeOcrFixEngine(contextSubtitle: null);
        }

        if (!_ocrFixEngine.IsLoaded())
        {
            return;
        }

        // Re-initialize with the full result as context so the engine's name lists and
        // word statistics see the whole subtitle, then fix each line.
        var contextSubtitle = new Subtitle();
        foreach (var line in Lines)
        {
            contextSubtitle.Paragraphs.Add(new Paragraph(line.Text, line.StartTime.TotalMilliseconds, line.EndTime.TotalMilliseconds));
        }

        InitializeOcrFixEngine(contextSubtitle);
        for (var i = 0; i < Lines.Count; i++)
        {
            ApplyFixToItem(Lines[i], i);
        }
    }

    /// <summary>Loads the OCR fix engine for the chosen dictionary (or unloads it when the
    /// fix is off / no dictionary is chosen). Runs before OCR starts, so lines are fixed and
    /// colored as they appear.</summary>
    private void InitializeOcrFixEngine(Subtitle? contextSubtitle)
    {
        if (!DoFixOcrErrors ||
            SelectedDictionary is not { } dictionary ||
            string.IsNullOrEmpty(dictionary.DictionaryFileName))
        {
            _ocrFixEngine.Unload();
            return;
        }

        try
        {
            _ocrFixEngine.Initialize(contextSubtitle ?? new Subtitle(), dictionary.GetThreeLetterCode(), dictionary);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Video OCR: could not initialize the OCR fix engine");
        }
    }

    /// <summary>Runs one line through the fix engine: the fixed text replaces the raw OCR
    /// text and the per-word result drives the coloring.</summary>
    private void ApplyFixToItem(VideoOcrLineItem item, int index)
    {
        if (!_ocrFixEngine.IsLoaded())
        {
            return;
        }

        try
        {
            OcrFixLineResult result;
            lock (_ocrFixEngineLock)
            {
                result = _ocrFixEngine.FixOcrErrors(index, item.Text, doTryToGuessUnknownWords: false);
            }

            item.Text = result.GetText();
            item.FixResult = result;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Video OCR: fix engine failed on a line");
        }
    }

    // The fix engine is used both from the OCR worker (spell-check arbitration between two
    // frame reads) and from the UI thread (preview-line fixes), so its calls are serialized.
    private readonly object _ocrFixEngineLock = new();

    /// <summary>How many words of the text the spell check does not know - the tiebreak
    /// between two nearly identical frame reads. 0 when no dictionary is loaded, so the
    /// arbitration never favors either read without a spell check behind it.</summary>
    private int CountUnknownWords(string text)
    {
        if (!_ocrFixEngine.IsLoaded())
        {
            return 0;
        }

        try
        {
            lock (_ocrFixEngineLock)
            {
                return _ocrFixEngine.FixOcrErrors(0, text, doTryToGuessUnknownWords: false)
                    .Words.Count(w => w.IsSpellCheckedOk == false);
            }
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Re-evaluates the coloring when a line's text changes (the Edit dialog, italic
    /// toggle). Only the coloring is updated - the text is left exactly as written.
    /// </summary>
    private void LineItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(VideoOcrLineItem.Text) ||
            sender is not VideoOcrLineItem item ||
            item.FixResult == null ||
            !_ocrFixEngine.IsLoaded())
        {
            return;
        }

        try
        {
            OcrFixLineResult result;
            lock (_ocrFixEngineLock)
            {
                result = _ocrFixEngine.FixOcrErrors(Lines.IndexOf(item), item.Text, doTryToGuessUnknownWords: false);
            }

            // Only keep the per-word coloring when the engine's view of the line matches the
            // text exactly - the cell renders the result's words, and a mismatch (the user
            // deliberately typed something the engine would "fix") would display stale text.
            item.FixResult = result.GetText() == item.Text ? result : null;
        }
        catch
        {
            // ignore - coloring is best-effort
        }
    }

    /// <summary>Wraps the lines in italic tags - or unwraps them when every line is already italic.</summary>
    internal static void ToggleItalic(List<VideoOcrLineItem> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        var allItalic = items.All(p => IsFullyItalic(p.Text));
        foreach (var item in items)
        {
            if (allItalic)
            {
                var text = item.Text.Trim();
                item.Text = text[3..^4].Trim();
            }
            else if (!IsFullyItalic(item.Text))
            {
                item.Text = "<i>" + item.Text.Trim() + "</i>";
            }
        }
    }

    private static bool IsFullyItalic(string text)
    {
        var trimmed = text.Trim();
        return trimmed.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) &&
               trimmed.EndsWith("</i>", StringComparison.OrdinalIgnoreCase) &&

               // "<i>a</i> b <i>c</i>" starts and ends with tags but is not fully italic.
               trimmed.IndexOf("</i>", StringComparison.OrdinalIgnoreCase) == trimmed.Length - 4;
    }

    /// <summary>Opens the text of a line in a small edit window (multi-line texts do not
    /// edit comfortably inside a table row).</summary>
    internal async Task EditLine(VideoOcrLineItem item)
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<Features.Shared.PromptTextBox.PromptTextBoxWindow,
            Features.Shared.PromptTextBox.PromptTextBoxViewModel>(Window, viewModel =>
        {
            viewModel.Initialize(
                string.Format(Se.Language.Video.VideoOcr.EditLineX, item.Number),
                item.Text,
                500,
                80);
        });

        if (result.OkPressed)
        {
            item.Text = result.Text.Trim().Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }

    internal void SeekPreview(VideoOcrLineItem item)
    {
        PreviewPositionSeconds = Math.Clamp(item.StartTime.TotalSeconds, 0, DurationSeconds);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (IsRunning)
        {
            var answer = await MessageBox.Show(
                Window!,
                Se.Language.Video.VideoOcr.AbortOcrTitle,
                Se.Language.Video.VideoOcr.AbortOcrMessage,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer == MessageBoxResult.Yes)
            {
                _cancellationTokenSource.Cancel();
            }

            return;
        }

        Window?.Close();
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _ = Cancel();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/video-ocr");
        }
    }

    internal void OnClosing()
    {
        _previewTimer.Stop();
        _cancellationTokenSource.Cancel();

        try
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                _ffmpegProcess.Kill(true);
            }
        }
        catch
        {
            // ignore
        }

        UiUtil.SaveWindowPosition(Window);
    }
}
