using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
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

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public partial class VideoOcrViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<VideoOcrEngineItem> _engines;
    [ObservableProperty] private VideoOcrEngineItem _selectedEngine;
    [ObservableProperty] private bool _isPaddleEngine;
    [ObservableProperty] private bool _isOllamaEngine;
    [ObservableProperty] private bool _isGlmEngine;
    [ObservableProperty] private ObservableCollection<OcrLanguage2> _paddleLanguages;
    [ObservableProperty] private OcrLanguage2? _selectedPaddleLanguage;
    [ObservableProperty] private string _ollamaUrl;
    [ObservableProperty] private string _ollamaModel;
    [ObservableProperty] private string _ollamaLanguage;
    [ObservableProperty] private string _glmUrl;
    [ObservableProperty] private string _glmModel;
    [ObservableProperty] private string _glmApiKey;
    [ObservableProperty] private string _glmLanguage;
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
    private CancellationTokenSource _cancellationTokenSource = new();
    private Process? _ffmpegProcess;
    private long _extractedFrames;
    private readonly DispatcherTimer _previewTimer;
    private bool _previewLoading;
    private bool _previewLoadQueued;

    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);

    private readonly IWindowService _windowService;

    public VideoOcrViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Engines = new ObservableCollection<VideoOcrEngineItem>(VideoOcrEngineItem.GetEngines());
        SelectedEngine = Engines[0];
        PaddleLanguages = new ObservableCollection<OcrLanguage2>(PaddleOcr.GetLanguages());
        SelectedPaddleLanguage = PaddleLanguages.FirstOrDefault(p => p.Code == "en");
        Lines = new ObservableCollection<VideoOcrLineItem>();

        OllamaUrl = string.Empty;
        OllamaModel = string.Empty;
        OllamaLanguage = string.Empty;
        GlmUrl = string.Empty;
        GlmModel = string.Empty;
        GlmApiKey = string.Empty;
        GlmLanguage = string.Empty;
        ProgressText = string.Empty;
        PreviewPositionText = string.Empty;
        ScanAreaText = string.Empty;

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

            await RunOcr(groups, cancellationToken);

            var mergedLines = VideoOcrLineBuilder.Build(groups, FramesPerSecond, TextSimilarityPercent, MaxGapMs, MinDurationMs);

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
                Lines.Add(new VideoOcrLineItem
                {
                    Number = number++,
                    StartTime = TimeSpan.FromMilliseconds(line.StartMs),
                    EndTime = TimeSpan.FromMilliseconds(line.EndMs),
                    Text = positionTag + line.Text,
                });
            }

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

    private async Task ExtractFrames(string framesFolder, CancellationToken cancellationToken)
    {
        var scale = string.Empty;
        var maxImageWidth = Se.Settings.Video.VideoOcr.MaxImageWidth;
        if (maxImageWidth > 0 && SelectionWidth > maxImageWidth)
        {
            scale = $",scale={maxImageWidth}:-2";
        }

        // JPEG (near-lossless q=2) instead of PNG: a long video at 5 fps produces tens of
        // thousands of frames, and PNG would need gigabytes of temp disk space.
        var outputPattern = Path.Combine(framesFolder, "img%06d.jpg");
        var arguments = $"-nostdin -y -i \"{_videoFileName}\" " +
                        $"-vf \"fps={FramesPerSecond.ToString(CultureInfo.InvariantCulture)}," +
                        $"crop={SelectionWidth}:{SelectionHeight}:{SelectionX}:{SelectionY}{scale}\" " +
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
                Lines.Add(new VideoOcrLineItem
                {
                    Number = Lines.Count + 1,
                    StartTime = TimeSpan.FromMilliseconds(group.GetStartMs(FramesPerSecond)),
                    EndTime = TimeSpan.FromMilliseconds(group.GetEndMs(FramesPerSecond)),
                    Text = group.Text,
                });
            });
        }

        Dispatcher.UIThread.Post(() =>
        {
            ProgressValue = 0;
            ProgressText = string.Format(Se.Language.Video.VideoOcr.RunningOcrXY, 0, ocrGroups.Count);
        });

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
                    ReportOcrProgress();
                    AddPreviewLine(group);
                }
            });

            // The frames are already image files on disk, so pass them by file name -
            // one batch, no per-image decode/encode, memory stays flat.
            var batch = ocrGroups
                .Select((g, i) => new PaddleOcrBatchInput { Index = i, SourceFileName = g.RepresentativeFileName })
                .ToList();

            var paddleOcr = new PaddleOcr();
            await paddleOcr.OcrBatch(engineType, batch, language, mode, progress, cancellationToken);
            if (!string.IsNullOrEmpty(paddleOcr.Error) && ocrGroups.All(p => string.IsNullOrEmpty(p.Text)))
            {
                throw new Exception("Paddle OCR failed: " + paddleOcr.Error);
            }
        }
        else if (engineType == OcrEngineType.Ollama)
        {
            var ollamaOcr = new OllamaOcr(Se.Settings.Ocr.OllamaOcrTimeoutMinutes);
            await RunLlmOcr(ocrGroups, group => OcrWithBitmap(group, bitmap =>
                    ollamaOcr.Ocr(bitmap, OllamaUrl, OllamaModel, OllamaLanguage, cancellationToken)),
                () => ollamaOcr.Error, ReportOcrProgress, AddPreviewLine, cancellationToken);
        }
        else if (engineType == OcrEngineType.Glm)
        {
            var glmOcr = new GlmOcr(GlmApiKey);
            await RunLlmOcr(ocrGroups, group =>
                    glmOcr.Ocr(group.RepresentativeFileName, GlmUrl, GlmModel, GlmLanguage, cancellationToken),
                () => glmOcr.Error, ReportOcrProgress, AddPreviewLine, cancellationToken);
        }
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

    private static async Task RunLlmOcr(
        List<VideoOcrFrameGroup> ocrGroups,
        Func<VideoOcrFrameGroup, Task<string>> ocr,
        Func<string> getError,
        Action reportProgress,
        Action<VideoOcrFrameGroup> addPreviewLine,
        CancellationToken cancellationToken)
    {
        var isFirst = true;
        foreach (var group in ocrGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();

            group.Text = VideoOcrLineBuilder.CleanOcrResult(await ocr(group));

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
        SelectedPaddleLanguage = PaddleLanguages.FirstOrDefault(p => p.Code == settings.PaddleLanguage) ??
                                 PaddleLanguages.FirstOrDefault(p => p.Code == "en");
        OllamaUrl = settings.OllamaUrl;
        OllamaModel = settings.OllamaModel;
        OllamaLanguage = settings.OllamaLanguage;
        GlmUrl = settings.GlmUrl;
        GlmModel = settings.GlmModel;
        GlmApiKey = settings.GlmApiKey;
        GlmLanguage = settings.GlmLanguage;
        FramesPerSecond = Math.Clamp(settings.FramesPerSecond, 1, 30);
        BrightnessMinimum = Math.Clamp(settings.BrightnessMinimum, 0, 255);
        TextSimilarityPercent = Math.Clamp(settings.TextSimilarityPercent, 0, 100);
        MaxGapMs = Math.Clamp(settings.MaxGapMs, 0, 10_000);
        MinDurationMs = Math.Clamp(settings.MinDurationMs, 0, 10_000);
        AddAssaPositionTag = settings.AddAssaPositionTag;
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.VideoOcr;
        settings.Engine = SelectedEngine.EngineType.ToString();
        settings.PaddleLanguage = SelectedPaddleLanguage?.Code ?? "en";
        settings.OllamaUrl = OllamaUrl;
        settings.OllamaModel = OllamaModel;
        settings.OllamaLanguage = OllamaLanguage;
        settings.GlmUrl = GlmUrl;
        settings.GlmModel = GlmModel;
        settings.GlmApiKey = GlmApiKey;
        settings.GlmLanguage = GlmLanguage;
        settings.FramesPerSecond = FramesPerSecond;
        settings.BrightnessMinimum = BrightnessMinimum;
        settings.TextSimilarityPercent = TextSimilarityPercent;
        settings.MaxGapMs = MaxGapMs;
        settings.MinDurationMs = MinDurationMs;
        settings.AddAssaPositionTag = AddAssaPositionTag;

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
        else if (e.Key == Key.F1)
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
