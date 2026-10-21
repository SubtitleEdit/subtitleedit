using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.VideoOcr;
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
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

public partial class RemoveBurnedInViewModel : ObservableObject
{
    public sealed class LineDisplay
    {
        public required TimeSpan Start { get; init; }
        public required TimeSpan End { get; init; }
        public required string Text { get; init; }

        public override string ToString() =>
            $"{Start:hh\\:mm\\:ss\\.fff}  {Text.Replace(Environment.NewLine, " ").Replace("\n", " ")}";
    }

    [ObservableProperty] private string _outputFileName = string.Empty;
    [ObservableProperty] private int _brightnessMinimum;
    [ObservableProperty] private bool _requireOutline;
    [ObservableProperty] private int _grow;
    [ObservableProperty] private bool _temporal;
    [ObservableProperty] private int _crf;
    [ObservableProperty] private ObservableCollection<LineDisplay> _lines = new();
    [ObservableProperty] private LineDisplay? _selectedLine;
    [ObservableProperty] private Bitmap? _previewBefore;
    [ObservableProperty] private Bitmap? _previewAfter;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isPreviewing;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText = string.Empty;
    [ObservableProperty] private string _previewStatus = string.Empty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IWindowService _windowService;
    private readonly IFileHelper _fileHelper;
    private readonly SleepInhibitorScope _sleepInhibitor = new(Se.Language.Video.RemoveBurnedIn.Title);
    private CancellationTokenSource _cancellationTokenSource = new();

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

    public RemoveBurnedInViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;
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
        BrightnessMinimum = brightnessMinimum > 0 ? brightnessMinimum : 190;
        RequireOutline = settings.RemoveBurnedInRequireOutline;
        Grow = settings.RemoveBurnedInGrow > 0 ? settings.RemoveBurnedInGrow : BurnedInMaskBuilder.DefaultGrow(videoHeight);
        Temporal = settings.RemoveBurnedInTemporal;
        Crf = settings.RemoveBurnedInCrf is >= 0 and <= 51 ? settings.RemoveBurnedInCrf : 18;

        Lines = new ObservableCollection<LineDisplay>(lines
            .Where(l => !string.IsNullOrWhiteSpace(l.Text))
            .OrderBy(l => l.StartTime)
            .Select(l => new LineDisplay { Start = l.StartTime, End = l.EndTime, Text = l.Text }));
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
        Task.Run(() =>
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
        });
    }

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
        Temporal = Temporal,
        Crf = Crf,
        CrispEmbedExecutable = CrispEmbedEngine.GetCliExecutable(),
        ModelFileName = GetModelFileName(),
    };

    private static string GetModelFileName()
    {
        var backend = CrispEmbedEngine.GetInpaintBackend();
        return backend.GetModelPath(backend.Models[0]);
    }

    private async Task<bool> EnsureEngineReady()
    {
        var backend = CrispEmbedEngine.GetInpaintBackend();
        if (!await CrispEmbedDownloadHelper.EnsureReadyAsync(Window!, _windowService, backend, backend.Models[0]))
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
    private async Task Preview()
    {
        var line = SelectedLine;
        if (line == null || IsRunning || IsPreviewing || !await EnsureEngineReady())
        {
            return;
        }

        IsPreviewing = true;
        PreviewStatus = Se.Language.General.PleaseWait;
        var options = MakeOptions(new List<(TimeSpan, TimeSpan)> { (line.Start, line.End) });
        try
        {
            var (before, after, status) = await Task.Run(() => MakePreview(options, line));
            var oldBefore = PreviewBefore;
            var oldAfter = PreviewAfter;
            PreviewBefore = before;
            PreviewAfter = after;
            oldBefore?.Dispose();
            oldAfter?.Dispose();
            PreviewStatus = status;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Remove burned-in subtitles: preview failed");
            PreviewStatus = exception.Message;
        }
        finally
        {
            IsPreviewing = false;
        }
    }

    private static async Task<(Bitmap? Before, Bitmap? After, string Status)> MakePreview(BurnedInRemovalOptions o, LineDisplay line)
    {
        // Masks from the line's own span, decoded from its (padded) start.
        var span = BurnedInSubtitleRemover.GetSpans(o)[0];
        var startFrame = Math.Max(0, span.StartFrame);
        var startSeconds = startFrame / o.FrameRate;
        var frames = span.EndFrame - startFrame + 1;
        var builder = BurnedInSubtitleRemover.MakeMaskBuilder(o, new[] { (0L, frames - 1) });
        var remover = new BurnedInSubtitleRemover();
        if (!await remover.BuildMasksAsync(o, builder, startSeconds, null, CancellationToken.None, frames / o.FrameRate))
        {
            return (null, null, Se.Language.Video.RemoveBurnedIn.NoTextInPreviewFrame);
        }

        // One full frame in the middle of the line, converted exactly like the real run.
        var midSeconds = (line.Start.TotalSeconds + line.End.TotalSeconds) / 2.0;
        var midFrame = (long)Math.Round((midSeconds - startSeconds) * o.FrameRate);
        var rgb = DecodeFrame(o, midSeconds);
        if (rgb == null)
        {
            return (null, null, "Could not decode the preview frame");
        }

        var mask = new byte[o.VideoWidth * o.VideoHeight];
        if (!builder.GetFrameMask(midFrame, rgb, mask))
        {
            return (CropToBitmap(rgb, null, o), null, Se.Language.Video.RemoveBurnedIn.NoTextInPreviewFrame);
        }

        var folder = Path.Combine(Path.GetTempPath(), "se-remove-burned-in-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var framePng = Path.Combine(folder, "frame.png");
            var maskPng = Path.Combine(folder, "mask.png");
            var outPng = Path.Combine(folder, "out.png");
            SavePng(framePng, rgb, o.VideoWidth, o.VideoHeight, 3);
            SavePng(maskPng, mask, o.VideoWidth, o.VideoHeight, 1);

            var stopwatch = Stopwatch.StartNew();
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(o.CrispEmbedExecutable,
                    $"--inpaint-model \"{o.ModelFileName}\" --inpaint \"{framePng}\" --mask \"{maskPng}\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(o.CrispEmbedExecutable) ?? string.Empty,
                },
            };
            process.Start();
            var stderr = process.StandardError.ReadToEndAsync();
            await using (var file = File.Create(outPng))
            {
                await process.StandardOutput.BaseStream.CopyToAsync(file);
            }

            await process.WaitForExitAsync();
            if (process.ExitCode != 0 || new FileInfo(outPng).Length == 0)
            {
                Se.LogError("Remove burned-in subtitles: crispembed --inpaint failed: " + await stderr);
                return (CropToBitmap(rgb, mask, o), null, Se.Language.Video.RemoveBurnedIn.Failed);
            }

            using var result = SKBitmap.Decode(outPng);
            var resultRgb = new byte[rgb.Length];
            for (var y = 0; y < o.VideoHeight; y++)
            {
                for (var x = 0; x < o.VideoWidth; x++)
                {
                    var c = result.GetPixel(x, y);
                    var i = (y * o.VideoWidth + x) * 3;
                    resultRgb[i] = c.Red;
                    resultRgb[i + 1] = c.Green;
                    resultRgb[i + 2] = c.Blue;
                }
            }

            var status = $"{line.Start:hh\\:mm\\:ss\\.fff} - {stopwatch.ElapsedMilliseconds:#,##0} ms";
            return (CropToBitmap(rgb, mask, o), CropToBitmap(resultRgb, null, o), status);
        }
        finally
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    private static byte[]? DecodeFrame(BurnedInRemovalOptions o, double seconds)
    {
        var matrix = BurnedInSubtitleRemover.ColorMatrix(o.VideoHeight);
        var args = $"-nostdin -v error -ss {seconds.ToString("0.###", CultureInfo.InvariantCulture)} -i \"{o.VideoFileName}\" -map 0:v:0 -frames:v 1 " +
                   $"-vf \"scale=in_color_matrix={matrix}:in_range=tv,format=rgb24\" -f rawvideo -pix_fmt rgb24 -";
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(FfmpegHelper.GetFfmpegLocation(), args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };
        process.Start();
        process.BeginErrorReadLine();
        using var memory = new MemoryStream();
        process.StandardOutput.BaseStream.CopyTo(memory);
        process.WaitForExit();
        var bytes = memory.ToArray();
        return bytes.Length >= o.VideoWidth * o.VideoHeight * 3 ? bytes : null;
    }

    private static void SavePng(string fileName, byte[] pixels, int width, int height, int channels)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = (y * width + x) * channels;
                bitmap.SetPixel(x, y, channels == 3
                    ? new SKColor(pixels[i], pixels[i + 1], pixels[i + 2])
                    : new SKColor(pixels[i], pixels[i], pixels[i]));
            }
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(fileName);
        data.SaveTo(stream);
    }

    /// <summary>The scan area (with some margin) as an Avalonia bitmap; mask pixels tinted red.</summary>
    private static Bitmap CropToBitmap(byte[] rgb, byte[]? mask, BurnedInRemovalOptions o)
    {
        var margin = o.VideoHeight / 40;
        var x0 = Math.Max(0, o.RegionX - margin);
        var y0 = Math.Max(0, o.RegionY - margin);
        var x1 = Math.Min(o.VideoWidth, o.RegionX + o.RegionWidth + margin);
        var y1 = Math.Min(o.VideoHeight, o.RegionY + o.RegionHeight + margin);
        using var bitmap = new SKBitmap(x1 - x0, y1 - y0, SKColorType.Rgba8888, SKAlphaType.Opaque);
        for (var y = y0; y < y1; y++)
        {
            for (var x = x0; x < x1; x++)
            {
                var i = y * o.VideoWidth + x;
                var r = rgb[i * 3];
                var g = rgb[i * 3 + 1];
                var b = rgb[i * 3 + 2];
                if (mask != null && mask[i] != 0)
                {
                    r = (byte)((r + 255) / 2);
                    g = (byte)(g / 2);
                    b = (byte)(b / 2);
                }

                bitmap.SetPixel(x - x0, y - y0, new SKColor(r, g, b));
            }
        }

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());
        return new Bitmap(stream);
    }

    [RelayCommand]
    private async Task Start()
    {
        if (IsRunning || IsPreviewing || Lines.Count == 0 || string.IsNullOrWhiteSpace(OutputFileName))
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(OutputFileName), Path.GetFullPath(_videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            await BrowseOutput();
            return;
        }

        if (!await EnsureEngineReady())
        {
            return;
        }

        SaveSettings();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var options = MakeOptions(Lines.Select(l => (l.Start, l.End)).ToList());
        var remover = new BurnedInSubtitleRemover();
        var progress = new Progress<BurnedInSubtitleRemover.ProgressInfo>(p =>
        {
            ProgressValue = p.Percent;
            ProgressText = p.Pass == 1
                ? string.Format(Se.Language.Video.RemoveBurnedIn.BuildingMasksX, $"{p.Percent / 15.0:0%}")
                : string.Format(Se.Language.Video.RemoveBurnedIn.RemovingXYZ, p.Frame, p.TotalFrames, p.FramesPerSecond);
        });

        IsRunning = true;
        ProgressValue = 0;
        ProgressText = string.Format(Se.Language.Video.RemoveBurnedIn.BuildingMasksX, string.Empty);
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

        await MessageBox.Show(Window!, Se.Language.Video.RemoveBurnedIn.Title,
            string.Format(Se.Language.Video.RemoveBurnedIn.DoneX, Environment.NewLine, options.OutputFileName),
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        OkPressed = true;
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
        settings.RemoveBurnedInCrf = Crf;
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
        }
    }

    internal void OnClosing()
    {
        _cancellationTokenSource.Cancel();
        _sleepInhibitor.Dispose();
        PreviewBefore?.Dispose();
        PreviewAfter?.Dispose();
        UiUtil.SaveWindowPosition(Window);
    }
}
