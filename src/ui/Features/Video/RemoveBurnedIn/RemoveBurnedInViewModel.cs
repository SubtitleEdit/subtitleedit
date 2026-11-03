using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// Remove burned-in subtitles (opened from Video OCR) in two steps:
/// <list type="number">
/// <item>Review: a frame preview with the mask on top (or the inpainted result), a timeline with
/// the OCR lines as boxes on the waveform (drag to fix timings), and the lines list to leave lines
/// out. Every mask/model setting is previewed live.</item>
/// <item>Remove: the full run (<see cref="BurnedInSubtitleRemover"/>); when done, open the video
/// or show it in its folder.</item>
/// </list>
/// </summary>
public partial class RemoveBurnedInViewModel : ObservableObject
{
    public sealed class ModelDisplay
    {
        public required CrispEmbedModel Model { get; init; }
        public required string DisplayName { get; init; }

        public override string ToString() => $"{DisplayName} ({Model.Size})";
    }

    [ObservableProperty] private ObservableCollection<string> _methods = new();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInpaint))]
    [NotifyPropertyChangedFor(nameof(IsBlur))]
    [NotifyPropertyChangedFor(nameof(WritesVideo))]
    private int _methodIndex;
    [ObservableProperty] private ObservableCollection<ModelDisplay> _models = new();
    [ObservableProperty] private ModelDisplay? _selectedModel;
    [ObservableProperty] private string _outputFileName = string.Empty;
    [ObservableProperty] private int _brightnessMinimum;
    [ObservableProperty] private bool _requireOutline;
    [ObservableProperty] private int _grow;
    [ObservableProperty] private int _extraWidth;
    [ObservableProperty] private bool _temporal;
    [ObservableProperty] private int _crf;
    [ObservableProperty] private int _blurStrength;
    [ObservableProperty] private ObservableCollection<RemoveBurnedInLineItem> _lines = new();
    [ObservableProperty] private RemoveBurnedInLineItem? _selectedLine;
    [ObservableProperty] private Bitmap? _previewBitmap;
    [ObservableProperty] private bool _showResult;
    [ObservableProperty] private double _positionSeconds;
    [ObservableProperty] private string _positionText = string.Empty;
    [ObservableProperty] private string _previewStatus = string.Empty;
    [ObservableProperty] private bool _isPreviewBusy;
    [ObservableProperty] private string _waveformStatus = string.Empty;
    [ObservableProperty] private string _linesSummary = string.Empty;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isDone;
    [ObservableProperty] private string _doneText = string.Empty;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The black boxes as an ASSA subtitle (black box method), for the main window.</summary>
    public string BoxesAssa { get; private set; } = string.Empty;
    public AudioVisualizer? AudioVisualizer { get; set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;
    private readonly SleepInhibitorScope _sleepInhibitor = new(Se.Language.Video.RemoveBurnedIn.Title);
    private CancellationTokenSource _cancellationTokenSource = new();
    private CancellationTokenSource _previewCancellation = new();
    private readonly DispatcherTimer _timer;

    private string _videoFileName = string.Empty;
    private int _videoWidth;
    private int _videoHeight;
    private double _durationSeconds;
    private double _frameRate;
    private int _regionX;
    private int _regionY;
    private int _regionWidth;
    private int _regionHeight;
    private double _ocrFramesPerSecond;

    // Preview state: a mask builder per line (keyed by the line's times and every mask setting,
    // so a timing drag or a setting change rebuilds it), and a dirty flag the timer turns into
    // one refresh once the user stops scrubbing.
    private readonly Dictionary<Guid, (string Key, BurnedInMaskBuilder Builder, double StartSeconds)> _masks = new();
    private bool _previewDirty;
    private DateTime _previewDirtySince;
    private bool _waveformDirty = true;

    public BurnedInMethod Method => MethodIndex is >= 0 and <= (int)BurnedInMethod.Delogo ? (BurnedInMethod)MethodIndex : BurnedInMethod.Inpaint;
    public bool IsInpaint => Method == BurnedInMethod.Inpaint;
    public bool IsBlur => Method == BurnedInMethod.Blur;

    /// <summary>All methods but the black boxes (an ASSA subtitle for the main window) write a video.</summary>
    public bool WritesVideo => Method != BurnedInMethod.BlackBox;

    public RemoveBurnedInViewModel(IWindowService windowService, IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        _timer.Tick += (_, _) => OnTimerTick();
    }

    public void Initialize(string videoFileName, int videoWidth, int videoHeight, double durationSeconds,
        int regionX, int regionY, int regionWidth, int regionHeight, int brightnessMinimum, double ocrFramesPerSecond,
        IEnumerable<VideoOcrLineItem> lines)
    {
        _videoFileName = videoFileName;
        _videoWidth = videoWidth;
        _videoHeight = videoHeight;
        _durationSeconds = durationSeconds;
        _regionX = regionX;
        _regionY = regionY;
        _regionWidth = regionWidth;
        _regionHeight = regionHeight;
        _ocrFramesPerSecond = ocrFramesPerSecond;

        var settings = Se.Settings.Video.VideoOcr;
        var l = Se.Language.Video.RemoveBurnedIn;
        Methods = new ObservableCollection<string> { l.MethodRemove, l.MethodBlackBox, l.MethodBlur, l.MethodDelogo };
        MethodIndex = settings.RemoveBurnedInMethod is >= 0 and <= (int)BurnedInMethod.Delogo ? settings.RemoveBurnedInMethod : 0;
        BlurStrength = settings.RemoveBurnedInBlurStrength is >= 1 and <= 100 ? settings.RemoveBurnedInBlurStrength : 20;
        var backend = CrispEmbedEngine.GetInpaintBackend();
        Models = new ObservableCollection<ModelDisplay>(backend.Models.Select(m => new ModelDisplay
        {
            Model = m,
            DisplayName = m.Name == CrispEmbedEngine.InpaintModelMiGan
                ? Se.Language.Video.RemoveBurnedIn.ModelMiGan
                : Se.Language.Video.RemoveBurnedIn.ModelLama,
        }));
        SelectedModel = Models.FirstOrDefault(m => m.Model.Name == settings.RemoveBurnedInModel) ?? Models.First();
        BrightnessMinimum = brightnessMinimum > 0 ? brightnessMinimum : 190;
        RequireOutline = settings.RemoveBurnedInRequireOutline;
        Grow = settings.RemoveBurnedInGrow > 0 ? settings.RemoveBurnedInGrow : BurnedInMaskBuilder.DefaultGrow(videoHeight);
        ExtraWidth = settings.RemoveBurnedInExtraWidth is >= 0 and <= 100 ? settings.RemoveBurnedInExtraWidth : 5;
        Temporal = settings.RemoveBurnedInTemporal;
        Crf = settings.RemoveBurnedInCrf is >= 0 and <= 51 ? settings.RemoveBurnedInCrf : 18;

        var number = 1;
        Lines = new ObservableCollection<RemoveBurnedInLineItem>(lines
            .Where(l => !string.IsNullOrWhiteSpace(l.Text))
            .OrderBy(l => l.StartTime)
            .Select(l => new RemoveBurnedInLineItem(number++, l.StartTime, l.EndTime, l.Text)));
        foreach (var item in Lines)
        {
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RemoveBurnedInLineItem.IsIncluded))
                {
                    UpdateLinesSummary();
                    _waveformDirty = true;
                    InvalidatePreview();
                }
            };
        }

        UpdateLinesSummary();
        SelectedLine = Lines.FirstOrDefault();

        var extension = Path.GetExtension(videoFileName).ToLowerInvariant() is ".mp4" or ".mkv" or ".mov"
            ? Path.GetExtension(videoFileName)
            : ".mkv";
        OutputFileName = Path.Combine(Path.GetDirectoryName(videoFileName) ?? string.Empty,
            Path.GetFileNameWithoutExtension(videoFileName) + ".no-subs" + extension);
    }

    internal void OnLoaded()
    {
        UiUtil.RestoreWindowPosition(Window);
        _ = Task.Run(() =>
        {
            try
            {
                var info = FfmpegMediaInfo2.Parse(_videoFileName);
                var rate = (double)(info.FramesRateNonNormalized > 0 ? info.FramesRateNonNormalized : info.FramesRate);
                _frameRate = rate > 0 ? rate : 25;
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Remove burned-in subtitles: could not read the frame rate of " + _videoFileName);
                _frameRate = 25;
            }

            Dispatcher.UIThread.Post(InvalidatePreview);
        });
        _ = LoadWaveformAsync();
        _timer.Start();
    }

    // ── review: timeline ─────────────────────────────────────────────

    private async Task LoadWaveformAsync()
    {
        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            return;
        }

        var peakWaveFileName = WavePeakGenerator2.GetPeakWaveFileName(_videoFileName);
        WavePeakData2? peaks = null;
        try
        {
            if (File.Exists(peakWaveFileName))
            {
                peaks = await Task.Run(() => WavePeakData2.FromDisk(peakWaveFileName));
            }
            else if (FfmpegHelper.IsFfmpegInstalled())
            {
                WaveformStatus = Se.Language.Video.RemoveBurnedIn.GeneratingWaveform;
                peaks = await Task.Run(async () =>
                {
                    var tempWaveFileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
                    try
                    {
                        using (var process = WaveFileExtractor.GetCommandLineProcess(_videoFileName, -1, tempWaveFileName, "acodec=s16l", out _))
                        {
                            process.Start();
                            await process.WaitForExitAsync();
                        }

                        if (!File.Exists(tempWaveFileName))
                        {
                            return null;
                        }

                        using var waveFile = new WavePeakGenerator2(tempWaveFileName);
                        return waveFile.GeneratePeaks(0, peakWaveFileName);
                    }
                    finally
                    {
                        try
                        {
                            File.Delete(tempWaveFileName);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                });
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Remove burned-in subtitles: could not load the waveform");
        }

        WaveformStatus = string.Empty;
        if (AudioVisualizer != null && peaks != null)
        {
            AudioVisualizer.WavePeaks = peaks;
            _waveformDirty = true;
        }
    }

    private void OnTimerTick()
    {
        var av = AudioVisualizer;
        if (av != null)
        {
            var included = Lines.Where(l => l.IsIncluded).Select(l => l.Line).OrderBy(l => l.StartTime).ToList();
            var selected = new List<SubtitleLineViewModel> { SelectedLine?.Line ?? new SubtitleLineViewModel() };
            av.CurrentVideoPositionSeconds = PositionSeconds;
            var outside = PositionSeconds > av.EndPositionSeconds || PositionSeconds < av.StartPositionSeconds;
            if (_waveformDirty || (!av.IsScrolling && outside))
            {
                var start = outside
                    ? Math.Max(0, PositionSeconds - (av.EndPositionSeconds - av.StartPositionSeconds) / 3)
                    : av.StartPositionSeconds;
                av.SetPosition(start, included, PositionSeconds, 0, selected);
                _waveformDirty = false;
            }
            else
            {
                av.SetPosition(av.StartPositionSeconds, included, PositionSeconds, -1, selected);
            }

            av.InvalidateVisual();
        }

        if (_previewDirty && !IsPreviewBusy && (DateTime.UtcNow - _previewDirtySince).TotalMilliseconds > 220)
        {
            _previewDirty = false;
            _ = RefreshPreviewAsync();
        }
    }

    internal void AudioVisualizerPositionChanged(object sender, AudioVisualizer.PositionEventArgs e)
    {
        PositionSeconds = Math.Clamp(e.PositionInSeconds, 0, Math.Max(0, _durationSeconds));
    }

    internal void AudioVisualizerParagraphClicked(object sender, ParagraphNullableEventArgs e)
    {
        if (e.Paragraph == null)
        {
            return;
        }

        var item = Lines.FirstOrDefault(l => l.Line.Id == e.Paragraph.Id);
        if (item != null && item != SelectedLine)
        {
            SelectedLine = item;
        }
    }

    internal void AudioVisualizerDragEnded(object? sender, EventArgs e)
    {
        // A box was moved or resized: its mask is rebuilt from the new span (MaskKey changes).
        InvalidatePreview();
    }

    partial void OnSelectedLineChanged(RemoveBurnedInLineItem? value)
    {
        if (value == null)
        {
            return;
        }

        PositionSeconds = (value.Line.StartTime.TotalSeconds + value.Line.EndTime.TotalSeconds) / 2.0;
        _waveformDirty = true;
    }

    partial void OnPositionSecondsChanged(double value)
    {
        PositionText = TimeSpan.FromSeconds(value).ToString(@"h\:mm\:ss\.fff");
        InvalidatePreview();
    }

    partial void OnShowResultChanged(bool value) => InvalidatePreview();
    partial void OnBrightnessMinimumChanged(int value) => InvalidatePreview();
    partial void OnRequireOutlineChanged(bool value) => InvalidatePreview();
    partial void OnGrowChanged(int value) => InvalidatePreview();
    partial void OnExtraWidthChanged(int value) => InvalidatePreview();
    partial void OnMethodIndexChanged(int value) => InvalidatePreview();
    partial void OnBlurStrengthChanged(int value) => InvalidatePreview();

    partial void OnSelectedModelChanged(ModelDisplay? value)
    {
        if (ShowResult)
        {
            InvalidatePreview();
        }
    }

    private void InvalidatePreview()
    {
        _previewDirty = true;
        _previewDirtySince = DateTime.UtcNow;
    }

    private void UpdateLinesSummary()
    {
        LinesSummary = string.Format(Se.Language.Video.RemoveBurnedIn.LinesIncludedXY, Lines.Count(l => l.IsIncluded), Lines.Count);
    }

    [RelayCommand]
    private void PreviousLine() => MoveLine(-1);

    [RelayCommand]
    private void NextLine() => MoveLine(1);

    private void MoveLine(int delta)
    {
        if (Lines.Count == 0)
        {
            return;
        }

        var index = SelectedLine == null ? -1 : Lines.IndexOf(SelectedLine);
        SelectedLine = Lines[Math.Clamp(index + delta, 0, Lines.Count - 1)];
    }

    [RelayCommand]
    private void PreviousFrame() => StepFrames(-1);

    [RelayCommand]
    private void NextFrame() => StepFrames(1);

    private void StepFrames(int frames)
    {
        var fps = _frameRate > 0 ? _frameRate : 25;
        PositionSeconds = Math.Clamp(PositionSeconds + frames / fps, 0, Math.Max(0, _durationSeconds));
    }

    // ── review: preview ──────────────────────────────────────────────

    private string MaskKey(RemoveBurnedInLineItem item) =>
        $"{item.Line.StartTime.Ticks}|{item.Line.EndTime.Ticks}|{BrightnessMinimum}|{RequireOutline}|{Grow}|{ExtraWidth}|{_frameRate}";

    private async Task RefreshPreviewAsync()
    {
        if (string.IsNullOrEmpty(_videoFileName) || _frameRate <= 0 || IsRunning)
        {
            return;
        }

        IsPreviewBusy = true;
        await _previewCancellation.CancelAsync();
        _previewCancellation = new CancellationTokenSource();
        var token = _previewCancellation.Token;
        var seconds = PositionSeconds;
        var showResult = ShowResult;
        var options = MakeOptions(new List<(TimeSpan, TimeSpan)>());
        const double margin = 1.5; // same apply margin as the run (BurnedInSubtitleRemover.MakeMaskBuilder)
        var candidates = Lines
            .Where(l => l.IsIncluded &&
                        seconds >= l.Line.StartTime.TotalSeconds - margin &&
                        seconds <= l.Line.EndTime.TotalSeconds + margin)
            .ToList();

        try
        {
            // Mask builders for the lines around this position (cached per line + settings).
            foreach (var item in candidates)
            {
                var key = MaskKey(item);
                if (_masks.TryGetValue(item.Line.Id, out var cached) && cached.Key == key)
                {
                    continue;
                }

                PreviewStatus = Se.Language.Video.RemoveBurnedIn.FindingText;
                var o = MakeOptions(new List<(TimeSpan, TimeSpan)> { (item.Line.StartTime, item.Line.EndTime) });
                var span = BurnedInSubtitleRemover.GetSpans(o)[0];
                var startFrame = Math.Max(0, span.StartFrame);
                var startSeconds = startFrame / o.FrameRate;
                var frames = span.EndFrame - startFrame + 1;
                var builder = BurnedInSubtitleRemover.MakeMaskBuilder(o, new[] { (0L, frames - 1) });
                var ok = await Task.Run(() => new BurnedInSubtitleRemover().BuildMasksAsync(o, builder, startSeconds, null, token, frames / o.FrameRate), token);
                if (ok)
                {
                    _masks[item.Line.Id] = (key, builder, startSeconds);
                }
            }

            var rgb = await Task.Run(() => BurnedInPreviewRenderer.DecodeFrame(options, seconds), token);
            if (rgb == null || token.IsCancellationRequested)
            {
                return;
            }

            var mask = new byte[_videoWidth * _videoHeight];
            var masked = 0;
            foreach (var item in candidates)
            {
                if (!_masks.TryGetValue(item.Line.Id, out var m))
                {
                    continue;
                }

                var relative = (long)Math.Round((seconds - m.StartSeconds) * options.FrameRate);
                if (m.Builder.GetFrameMask(relative, rgb, mask))
                {
                    masked++;
                }
            }

            var pixels = mask.Count(v => v != 0);
            var method = Method;
            if (showResult && pixels > 0 && method != BurnedInMethod.Inpaint)
            {
                foreach (var item in candidates)
                {
                    if (!_masks.TryGetValue(item.Line.Id, out var m))
                    {
                        continue;
                    }

                    var relative = (long)Math.Round((seconds - m.StartSeconds) * options.FrameRate);
                    foreach (var box in m.Builder.GetLineBoxes())
                    {
                        if (relative >= box.StartFrame && relative <= box.EndFrame)
                        {
                            switch (method)
                            {
                                case BurnedInMethod.Blur:
                                    BurnedInPreviewRenderer.BlurRect(rgb, _videoWidth, box.X, box.Y, box.Width, box.Height, BlurStrength);
                                    break;
                                case BurnedInMethod.Delogo:
                                    BurnedInPreviewRenderer.DelogoRect(rgb, _videoWidth, box.X, box.Y, box.Width, box.Height);
                                    break;
                                default:
                                    BurnedInPreviewRenderer.FillBlack(rgb, _videoWidth, box.X, box.Y, box.Width, box.Height);
                                    break;
                            }
                        }
                    }
                }

                SetPreview(BurnedInPreviewRenderer.Compose(rgb, null, options, false));
                PreviewStatus = Methods[(int)method];
                return;
            }

            if (showResult && pixels > 0)
            {
                if (!await EnsureEngineReady())
                {
                    ShowResult = false;
                    return;
                }

                PreviewStatus = Se.Language.Video.RemoveBurnedIn.Inpainting;
                var stopwatch = Stopwatch.StartNew();
                var result = await BurnedInPreviewRenderer.InpaintAsync(options, rgb, mask, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                SetPreview(BurnedInPreviewRenderer.Compose(result ?? rgb, null, options, false));
                PreviewStatus = result == null
                    ? Se.Language.Video.RemoveBurnedIn.Failed
                    : string.Format(Se.Language.Video.RemoveBurnedIn.ResultInXMs, stopwatch.ElapsedMilliseconds);
                return;
            }

            SetPreview(BurnedInPreviewRenderer.Compose(rgb, mask, options, true));
            PreviewStatus = masked == 0
                ? Se.Language.Video.RemoveBurnedIn.NoSubtitleHere
                : string.Format(Se.Language.Video.RemoveBurnedIn.MaskedXPixels, pixels);
        }
        catch (OperationCanceledException)
        {
            // a newer refresh took over
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Remove burned-in subtitles: preview failed");
            PreviewStatus = exception.Message;
        }
        finally
        {
            IsPreviewBusy = false;
        }
    }

    private void SetPreview(Bitmap bitmap)
    {
        var old = PreviewBitmap;
        PreviewBitmap = bitmap;
        old?.Dispose();
    }

    // ── run ──────────────────────────────────────────────────────────

    private BurnedInRemovalOptions MakeOptions(IReadOnlyList<(TimeSpan, TimeSpan)> lines) => new()
    {
        VideoFileName = _videoFileName,
        OutputFileName = OutputFileName,
        VideoWidth = _videoWidth,
        VideoHeight = _videoHeight,
        FrameRate = _frameRate > 0 ? _frameRate : 25,
        DurationSeconds = _durationSeconds,
        RegionX = _regionX,
        RegionY = _regionY,
        RegionWidth = _regionWidth,
        RegionHeight = _regionHeight,
        Lines = lines,
        OcrFramesPerSecond = _ocrFramesPerSecond,
        BrightnessMinimum = BrightnessMinimum,
        RequireOutline = RequireOutline,
        Grow = Grow,
        ExtraWidth = ExtraWidth,
        Temporal = Temporal,
        Method = Method,
        BlurStrength = BlurStrength,
        Crf = Crf,
        CrispEmbedExecutable = CrispEmbedEngine.GetCliExecutable(),
        ModelFileName = GetModelFileName(),
    };

    private string GetModelFileName()
    {
        var backend = CrispEmbedEngine.GetInpaintBackend();
        return backend.GetModelPath(SelectedModel?.Model ?? backend.Models[0]);
    }

    private async Task<bool> EnsureEngineReady()
    {
        var backend = CrispEmbedEngine.GetInpaintBackend();
        if (!await CrispEmbedDownloadHelper.EnsureReadyAsync(Window!, _windowService, backend, SelectedModel?.Model ?? backend.Models[0]))
        {
            return false;
        }

        if (!await Task.Run(CrispEmbedEngine.SupportsInpainting))
        {
            await MessageBox.Show(Window!, Se.Language.Video.RemoveBurnedIn.Title, Se.Language.Video.RemoveBurnedIn.EngineTooOld,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task BrowseOutput()
    {
        var extension = Path.GetExtension(OutputFileName);
        var fileName = await _fileHelper.PickSaveFile(Window!, new List<(string, string)> { ("MP4", ".mp4"), ("Matroska", ".mkv") }
            .OrderByDescending(p => p.Item2.Equals(extension, StringComparison.OrdinalIgnoreCase)).ToList(),
            Path.GetFileName(OutputFileName), Se.Language.General.SaveVideoAsVideoTitle);
        if (!string.IsNullOrEmpty(fileName))
        {
            OutputFileName = fileName;
        }
    }

    [RelayCommand]
    private async Task Start()
    {
        var included = Lines.Where(l => l.IsIncluded).ToList();
        if (IsRunning || included.Count == 0)
        {
            return;
        }

        if (Method == BurnedInMethod.BlackBox)
        {
            await MakeBoxes(included);
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputFileName))
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(OutputFileName), Path.GetFullPath(_videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            await BrowseOutput();
            return;
        }

        if (IsInpaint && !await EnsureEngineReady())
        {
            return;
        }

        SaveSettings();
        await _previewCancellation.CancelAsync();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var options = MakeOptions(included.Select(l => (l.Line.StartTime, l.Line.EndTime)).ToList());
        var remover = new BurnedInSubtitleRemover();
        var progress = new Progress<BurnedInSubtitleRemover.ProgressInfo>(p =>
        {
            ProgressValue = p.Percent;
            ProgressText = p.Pass == 1
                ? string.Format(Se.Language.Video.RemoveBurnedIn.BuildingMasksX, $"{p.Percent / 15.0:0%}")
                : string.Format(options.Method == BurnedInMethod.Inpaint ? Se.Language.Video.RemoveBurnedIn.RemovingXYZ : Se.Language.Video.RemoveBurnedIn.EncodingXYZ,
                    p.Frame, p.TotalFrames, p.FramesPerSecond);
        });

        IsDone = false;
        IsRunning = true;
        ProgressValue = 0;
        ProgressText = string.Format(Se.Language.Video.RemoveBurnedIn.BuildingMasksX, string.Empty);
        var stopwatch = Stopwatch.StartNew();
        bool ok;
        var cancelled = false;
        try
        {
            ok = await Task.Run(() => remover.RunAsync(options, progress, token), token);
        }
        catch (OperationCanceledException)
        {
            ok = false;
            cancelled = true;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Remove burned-in subtitles failed");
            ok = false;
        }
        finally
        {
            IsRunning = false;
            ProgressText = string.Empty;
            ProgressValue = 0;
        }

        if (cancelled)
        {
            TryDelete(options.OutputFileName);
            return;
        }

        if (!ok)
        {
            Se.LogError("Remove burned-in subtitles failed:" + Environment.NewLine + remover.Log);
            var noText = remover.Log.Contains("Masks: 0 of", StringComparison.Ordinal);
            await MessageBox.Show(Window!, Se.Language.Video.RemoveBurnedIn.Title,
                noText ? Se.Language.Video.RemoveBurnedIn.NoTextFound : Se.Language.Video.RemoveBurnedIn.Failed + Environment.NewLine +
                Environment.NewLine + LastLines(remover.Log, 8),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var elapsed = stopwatch.Elapsed;
        DoneText = string.Format(Se.Language.Video.RemoveBurnedIn.DoneInXY, Path.GetFileName(options.OutputFileName),
            elapsed.TotalHours >= 1 ? elapsed.ToString(@"h\:mm\:ss") : elapsed.ToString(@"m\:ss"));
        IsDone = true;
        OkPressed = true;
    }

    /// <summary>Black box method: builds the masks and hands the boxes to the main window as ASSA.</summary>
    private async Task MakeBoxes(List<RemoveBurnedInLineItem> included)
    {
        SaveSettings();
        await _previewCancellation.CancelAsync();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var options = MakeOptions(included.Select(l => (l.Line.StartTime, l.Line.EndTime)).ToList());
        var remover = new BurnedInSubtitleRemover();
        var progress = new Progress<BurnedInSubtitleRemover.ProgressInfo>(p =>
        {
            ProgressValue = p.Percent;
            ProgressText = string.Format(Se.Language.Video.RemoveBurnedIn.BuildingMasksX, $"{p.Percent / 100.0:0%}");
        });

        IsDone = false;
        IsRunning = true;
        ProgressValue = 0;
        ProgressText = string.Format(Se.Language.Video.RemoveBurnedIn.BuildingMasksX, string.Empty);
        string? assa;
        try
        {
            assa = await Task.Run(() => remover.MakeBoxesAssaAsync(options, progress, token), token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Remove burned-in subtitles: black boxes failed");
            assa = null;
        }
        finally
        {
            IsRunning = false;
            ProgressText = string.Empty;
            ProgressValue = 0;
        }

        if (string.IsNullOrEmpty(assa))
        {
            Se.LogError("Remove burned-in subtitles: black boxes failed:" + Environment.NewLine + remover.Log);
            var noText = remover.Log.Contains("Masks: 0 of", StringComparison.Ordinal);
            await MessageBox.Show(Window!, Se.Language.Video.RemoveBurnedIn.Title,
                noText ? Se.Language.Video.RemoveBurnedIn.NoTextFound : Se.Language.Video.RemoveBurnedIn.Failed + Environment.NewLine +
                Environment.NewLine + LastLines(remover.Log, 8),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        BoxesAssa = assa;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void OpenVideo()
    {
        try
        {
            if (File.Exists(OutputFileName))
            {
                FileHelper.OpenFileWithDefaultProgram(OutputFileName);
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Remove burned-in subtitles: could not open " + OutputFileName);
        }
    }

    [RelayCommand]
    private async Task ShowInFolder()
    {
        if (File.Exists(OutputFileName) && Window != null)
        {
            await _folderHelper.OpenFolderWithFileSelected(Window, OutputFileName);
        }
    }

    private static string LastLines(string text, int count)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(Environment.NewLine, lines.Skip(Math.Max(0, lines.Length - count)));
    }

    private static void TryDelete(string fileName)
    {
        try
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.VideoOcr;
        settings.RemoveBurnedInRequireOutline = RequireOutline;
        settings.RemoveBurnedInGrow = Grow == BurnedInMaskBuilder.DefaultGrow(_videoHeight) ? 0 : Grow;
        settings.RemoveBurnedInTemporal = Temporal;
        settings.RemoveBurnedInExtraWidth = ExtraWidth;
        settings.RemoveBurnedInCrf = Crf;
        settings.RemoveBurnedInMethod = MethodIndex;
        settings.RemoveBurnedInBlurStrength = BlurStrength;
        settings.RemoveBurnedInModel = SelectedModel?.Model.Name ?? CrispEmbedEngine.InpaintModelLama;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        if (IsRunning)
        {
            var answer = await MessageBox.Show(Window!, Se.Language.Video.RemoveBurnedIn.AbortTitle,
                Se.Language.Video.RemoveBurnedIn.AbortMessage, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == MessageBoxResult.Yes)
            {
                await _cancellationTokenSource.CancelAsync();
            }

            return;
        }

        Window?.Close();
    }

    partial void OnIsRunningChanged(bool value) => _sleepInhibitor.SetActive(value);

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            _ = Cancel();
            return;
        }

        if (IsRunning || e.KeyModifiers != KeyModifiers.None || e.Source is TextBox or NumericUpDown or ComboBox)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Left:
                StepFrames(-1);
                e.Handled = true;
                break;
            case Key.Right:
                StepFrames(1);
                e.Handled = true;
                break;
            case Key.PageUp:
                MoveLine(-1);
                e.Handled = true;
                break;
            case Key.PageDown:
                MoveLine(1);
                e.Handled = true;
                break;
            case Key.R:
                ShowResult = !ShowResult;
                e.Handled = true;
                break;
        }
    }

    internal void OnClosing()
    {
        _timer.Stop();
        _cancellationTokenSource.Cancel();
        _previewCancellation.Cancel();
        _sleepInhibitor.Dispose();
        PreviewBitmap?.Dispose();
        UiUtil.SaveWindowPosition(Window);
    }
}
