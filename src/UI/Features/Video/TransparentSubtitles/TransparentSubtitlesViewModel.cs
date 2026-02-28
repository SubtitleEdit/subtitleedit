using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;

namespace Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;

public partial class TransparentSubtitlesViewModel : ObservableObject
{
    [ObservableProperty] private string _videoFileName;
    [ObservableProperty] private string _videoFileSize;
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private double? _fontFactor;
    [ObservableProperty] private string _fontFactorText;
    [ObservableProperty] private bool _fontIsBold;
    [ObservableProperty] private decimal? _selectedFontOutline;
    [ObservableProperty] private string _fontOutlineText;
    [ObservableProperty] private decimal? _selectedFontShadowWidth;
    [ObservableProperty] private string _fontShadowText;
    [ObservableProperty] private ObservableCollection<FontBoxItem> _fontBoxTypes;
    [ObservableProperty] private FontBoxItem _selectedFontBoxType;
    [ObservableProperty] private Color _fontTextColor;
    [ObservableProperty] private Color _fontBoxColor;
    [ObservableProperty] private Color _fontOutlineColor;
    [ObservableProperty] private Color _fontShadowColor;
    [ObservableProperty] private int? _fontMarginHorizontal;
    [ObservableProperty] private int? _fontMarginVertical;
    [ObservableProperty] private bool _fontFixRtl;
    [ObservableProperty] private ObservableCollection<AlignmentItem> _fontAlignments;
    [ObservableProperty] private AlignmentItem _selectedFontAlignment;
    [ObservableProperty] private string _fontAssaInfo;
    [ObservableProperty] private int? _videoWidth;
    [ObservableProperty] private int? _videoHeight;
    [ObservableProperty] private ObservableCollection<double> _frameRates;
    [ObservableProperty] private double _selectedFrameRate;
    [ObservableProperty] private ObservableCollection<string> _videoExtensions;
    [ObservableProperty] private string _selectedVideoExtension;
    [ObservableProperty] private string _outputFolder;
    [ObservableProperty] private bool _useOutputFolderVisible;
    [ObservableProperty] private bool _useSourceFolderVisible;
    [ObservableProperty] private bool _isSingleModeVisible;
    [ObservableProperty] private bool _isCutActive;
    [ObservableProperty] private TimeSpan _cutFrom;
    [ObservableProperty] private TimeSpan _cutTo;
    [ObservableProperty] private bool _useTargetFileSize;
    [ObservableProperty] private int? _targetFileSize;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private ObservableCollection<BurnInJobItem> _jobItems;
    [ObservableProperty] private BurnInJobItem? _selectedJobItem;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _isBatchMode;
    [ObservableProperty] private Bitmap? _imagePreview;
    [ObservableProperty] private bool _useSourceResolution;
    [ObservableProperty] private string _displayEffect;
    [ObservableProperty] private bool _promptForFfmpegParameters;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new();
    private bool _loading = true;
    private readonly StringBuilder _log;
    private static readonly Regex FrameFinderRegex = new(@"[Ff]rame=\s*\d+", RegexOptions.Compiled);
    private long _startTicks;
    private long _processedFrames;
    private Process? _ffmpegProcess;
    private readonly Timer _timerAnalyze;
    private readonly Timer _timerGenerate;
    private bool _doAbort;
    private int _jobItemIndex = -1;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;
    private List<BurnInEffectItem> _selectedEffects;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public TransparentSubtitlesViewModel(IFolderHelper folderHelper, IFileHelper fileHelper,
        IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontName = FontNames.FirstOrDefault(p => p == Se.Settings.Video.BurnIn.FontName) ?? FontNames[0];

        // font factors between 0-1
        FontFactor = 0.4;
        FontFactorText = string.Empty;

        FontBoxTypes = new ObservableCollection<FontBoxItem>
        {
            new(FontBoxType.None, Se.Language.General.None),
            new(FontBoxType.OneBox, Se.Language.Video.BurnIn.OneBox),
            new(FontBoxType.BoxPerLine, Se.Language.Video.BurnIn.BoxPerLine),
        };
        SelectedFontBoxType = FontBoxTypes[0];

        FontMarginHorizontal = 10;
        FontMarginVertical = 10;

        FontAlignments = new ObservableCollection<AlignmentItem>(AlignmentItem.Alignments);
        SelectedFontAlignment = AlignmentItem.Alignments[7];

        FontAssaInfo = string.Empty;

        VideoWidth = 1920;
        VideoHeight = 1080;

        FrameRates = new ObservableCollection<double> { 23.976, 24, 25, 29.97, 30, 50, 59.94, 60 };
        SelectedFrameRate = FrameRates[0];

        VideoExtensions = new ObservableCollection<string>
        {
            ".mov",
            ".mkv",
            ".mp4",
            ".webm",
        };
        SelectedVideoExtension = VideoExtensions[0];

        JobItems = new ObservableCollection<BurnInJobItem>();

        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;
        FontOutlineText = string.Empty;
        FontShadowText = string.Empty;
        OutputFolder = string.Empty;
        DisplayEffect = string.Empty;

        _selectedEffects = new List<BurnInEffectItem>();
        _log = new StringBuilder();
        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;

        _timerAnalyze = new();
        _timerAnalyze.Elapsed += TimerAnalyzeElapsed;
        _timerAnalyze.Interval = 100;

        _loading = false;
        _inputVideoFileName = string.Empty;
        LoadSettings();
        BoxTypeChanged();
        UpdateOutputProperties();
    }

    public void Initialize(string videoFileName, Subtitle subtitle, SubtitleFormat subtitleFormat)
    {
        VideoFileName = videoFileName;
        _inputVideoFileName = videoFileName;
        _subtitle = new Subtitle(subtitle, false);
        _subtitleFormat = subtitleFormat;

        var fileExists = !string.IsNullOrWhiteSpace(videoFileName) && File.Exists(videoFileName);
        if (fileExists)
        {
            VideoFileSize = Utilities.FormatBytesToDisplayFileSize(new FileInfo(videoFileName).Length);
            _ = Task.Run(() =>
            {
                var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
                Dispatcher.UIThread.Post(() =>
                {
                    VideoWidth = mediaInfo.Dimension.Width;
                    VideoHeight = mediaInfo.Dimension.Height;
                    UseSourceResolution = false;
                });
            });
        }
        else
        {
            BatchMode();
        }
    }

    private void TimerAnalyzeElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        if (_doAbort)
        {
            _timerAnalyze.Stop();
#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416

            IsGenerating = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0,
                MidpointRounding.AwayFromZero);
            percentage = Math.Clamp(percentage, 0, 100);

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * JobItems[_jobItemIndex].TotalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            if (JobItems.Count == 1)
            {
                ProgressText = $"Analyzing video... {percentage}%     {estimatedLeft}";
            }
            else
            {
                ProgressText = $"Analyzing video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            }

            return;
        }

        _timerAnalyze.Stop();

        Dispatcher.UIThread.Post(async void () =>
        {
            try
            {
                var jobItem = JobItems[_jobItemIndex];
                var process = await GetFfmpegProcess(jobItem);
                if (process == null)
                {
                    return;
                }

                _ffmpegProcess = process;
#pragma warning disable CA1416 // Validate platform compatibility
                _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();

                _timerGenerate.Start();
            }
            catch (Exception exception)
            {
                Se.LogError(exception);
            }
        });
    }

    private void TimerGenerateElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        if (_doAbort)
        {
            _timerGenerate.Stop();
#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416

            IsGenerating = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            var percentage = (int)Math.Round((double)_processedFrames / JobItems[_jobItemIndex].TotalFrames * 100.0,
                MidpointRounding.AwayFromZero);
            percentage = Math.Clamp(percentage, 0, 100);

            var durationMs = (DateTime.UtcNow.Ticks - _startTicks) / 10_000;
            var msPerFrame = (float)durationMs / _processedFrames;
            var estimatedTotalMs = msPerFrame * JobItems[_jobItemIndex].TotalFrames;
            var estimatedLeft = ProgressHelper.ToProgressTime(estimatedTotalMs - durationMs);

            if (JobItems.Count == 1)
            {
                ProgressText = $"Generating video... {percentage}%     {estimatedLeft}";
            }
            else
            {
                ProgressText = $"Generating video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
            }

            return;
        }

        _timerGenerate.Stop();
        ProgressValue = 100;
        ProgressText = string.Empty;

        var jobItem = JobItems[_jobItemIndex];

        if (!File.Exists(jobItem.OutputVideoFileName))
        {
            SeLogger.WhisperInfo("Output video file not found: " + jobItem.OutputVideoFileName + Environment.NewLine +
                                 "ffmpeg: " + _ffmpegProcess.StartInfo.FileName + Environment.NewLine +
                                 "Parameters: " + _ffmpegProcess.StartInfo.Arguments + Environment.NewLine +
                                 "OS: " + Environment.OSVersion + Environment.NewLine +
                                 "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                                 "ffmpeg exit code: " + _ffmpegProcess.ExitCode + Environment.NewLine +
                                 "ffmpeg log: " + _log);

            Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!,
                    "Unable to generate video",
                    "Output video file not generated: " + jobItem.OutputVideoFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                IsGenerating = true;
                ProgressValue = 0;
            });

            return;
        }

        JobItems[_jobItemIndex].Status = Se.Language.General.Done;

        Dispatcher.UIThread.Invoke(async () =>
        {
            ProgressValue = 0;

            if (_jobItemIndex < JobItems.Count - 1)
            {
                await InitAndStartJobItem(_jobItemIndex + 1);
                return;
            }

            IsGenerating = false;

            if (JobItems.Count == 1)
            {
                await _folderHelper.OpenFolderWithFileSelected(Window!, jobItem.OutputVideoFileName);
            }
            else
            {
                var sb = new StringBuilder($"Generated files ({JobItems.Count}):" + Environment.NewLine + Environment.NewLine);
                foreach (var item in JobItems)
                {
                    sb.AppendLine($"{item.OutputVideoFileName} ==> {item.Status}");
                }

                await MessageBox.Show(Window!,
                    "Generating done",
                    sb.ToString(),
                    MessageBoxButtons.OK);
            }
        });
    }

    private async Task InitAndStartJobItem(int index)
    {
        _startTicks = DateTime.UtcNow.Ticks;
        _jobItemIndex = index;
        var jobItem = JobItems[index];
        var mediaInfo = FfmpegMediaInfo.Parse(jobItem.InputVideoFileName);
        jobItem.TotalFrames = mediaInfo.GetTotalFrames();
        jobItem.TotalSeconds = mediaInfo.Duration.TotalSeconds;
        jobItem.Width = mediaInfo.Dimension.Width;
        jobItem.Height = mediaInfo.Dimension.Height;
        jobItem.UseTargetFileSize = UseTargetFileSize;
        jobItem.TargetFileSize = UseTargetFileSize ? TargetFileSize ?? 0 : 0;
        jobItem.AssaSubtitleFileName = MakeAssa(jobItem.SubtitleFileName);
        jobItem.Status = Se.Language.General.Generating;
        jobItem.OutputVideoFileName = MakeOutputFileName(jobItem.InputVideoFileName);

        var result = await RunEncoding(jobItem);
        if (result)
        {
            _timerGenerate.Start();
        }
    }

    private async Task<bool> RunEncoding(BurnInJobItem jobItem)
    {
        var process = await GetFfmpegProcess(jobItem);
        if (process == null)
        {
            return false;
        }

        _ffmpegProcess = process;
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();

        return true;
    }

    private async Task<Process?> GetFfmpegProcess(BurnInJobItem jobItem)
    {
        var totalMs = _subtitle.Paragraphs.Max(p => p.EndTime.TotalMilliseconds);
        var ts = TimeSpan.FromMilliseconds(totalMs + 2000);
        var timeCode = string.Format($"{ts.Hours:00}\\\\:{ts.Minutes:00}\\\\:{ts.Seconds:00}");

        var ffmpegParameters = FfmpegGenerator.GenerateTransparentVideoFile(
            jobItem.AssaSubtitleFileName,
            jobItem.OutputVideoFileName,
            jobItem.Width,
            jobItem.Height,
            SelectedFrameRate.ToString(CultureInfo.InvariantCulture),
            timeCode);

        if (PromptForFfmpegParameters)
        {
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize("ffmpeg parameters", ffmpegParameters, 1000, 200);
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return null;
            }

            ffmpegParameters = result.Text.Trim();
        }

        var workingDirectory = Path.GetDirectoryName(jobItem.AssaSubtitleFileName) ?? string.Empty;
        return FfmpegGenerator.GetProcess(ffmpegParameters, OutputHandler, workingDirectory);
    }

    private Subtitle GetSubtitleBasedOnCut(Subtitle inputSubtitle)
    {
        if (!IsCutActive)
        {
            return inputSubtitle;
        }

        var subtitle = new Subtitle();
        foreach (var p in inputSubtitle.Paragraphs)
        {
            if (p.StartTime.TotalMilliseconds >= CutFrom.TotalMilliseconds &&
                p.EndTime.TotalMilliseconds <= CutTo.TotalMilliseconds)
            {
                subtitle.Paragraphs.Add(new Paragraph(p));
            }
        }

        subtitle.AddTimeToAllParagraphs(TimeSpan.FromMilliseconds(-CutFrom.TotalMilliseconds));

        return subtitle;
    }

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (FontFactor == null || FontFactor < 0.1)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Font factor");
        }

        if (SelectedFontOutline == null)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Font outline width");
        }


        if (SelectedFontShadowWidth == null)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Font shadow width");
        }

        if (FontMarginHorizontal == null)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Font margin horizontal");
        }

        if (FontMarginVertical == null)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Font margin vertical");
        }

        if (VideoWidth == null || VideoWidth <= 1)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Video width");
        }

        if (VideoHeight == null || VideoHeight <= 1)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Video height");
        }

        if (UseTargetFileSize)
        {
            if (TargetFileSize == null || TargetFileSize < 1)
            {
                return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Target file size");
            }
        }

        return string.Empty;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        _log?.AppendLine(outLine.Data);

        var match = FrameFinderRegex.Match(outLine.Data);
        if (!match.Success)
        {
            return;
        }

        var arr = match.Value.Split('=');
        if (arr.Length != 2)
        {
            return;
        }

        if (long.TryParse(arr[1].Trim(), out var f))
        {
            _processedFrames = f;
            ProgressValue = (double)_processedFrames * 100.0 / JobItems[_jobItemIndex].TotalFrames;
        }
    }

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems()
    {
        var subtitle = new Subtitle(_subtitle);

        var srt = new SubRip();
        var subtitleFileName = Path.Combine(Path.GetTempFileName() + srt.Extension);
        if (_subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat })
        {
            var assa = new AdvancedSubStationAlpha();
            subtitleFileName = Path.Combine(Path.GetTempFileName() + assa.Extension);
            File.WriteAllText(subtitleFileName, assa.ToText(subtitle, string.Empty));
        }
        else
        {
            File.WriteAllText(subtitleFileName, srt.ToText(subtitle, string.Empty));
        }

        var jobItem = new BurnInJobItem(string.Empty, VideoWidth ?? 0, VideoHeight ?? 0)
        {
            InputVideoFileName = VideoFileName,
            OutputVideoFileName = MakeOutputFileName(_subtitle.FileName),
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    private string MakeAssa(string subtitleFileName)
    {
        if (string.IsNullOrWhiteSpace(subtitleFileName) || !File.Exists(subtitleFileName))
        {
            JobItems[_jobItemIndex].Status = "Skipped";
            return string.Empty;
        }

        var isAssa = subtitleFileName.EndsWith(".ass", StringComparison.OrdinalIgnoreCase);

        var subtitle = Subtitle.Parse(subtitleFileName);

        subtitle = GetSubtitleBasedOnCut(subtitle);

        if (!isAssa)
        {
            SetStyleForNonAssa(subtitle);
        }

        var assa = new AdvancedSubStationAlpha();
        var assaFileName = Path.Combine(Path.GetTempFileName() + assa.Extension);
        File.WriteAllText(assaFileName, assa.ToText(subtitle, string.Empty));
        return assaFileName;
    }

    private void SetStyleForNonAssa(Subtitle sub)
    {
        sub.Header = AdvancedSubStationAlpha.DefaultHeader;
        var style = AdvancedSubStationAlpha.GetSsaStyle("Default", sub.Header);
        style.FontSize = CalculateFontSize(JobItems[_jobItemIndex].Width, JobItems[_jobItemIndex].Height, FontFactor ?? 0);
        style.Bold = FontIsBold;
        style.FontName = SelectedFontName;
        style.Background = FontShadowColor.ToSKColor();
        style.Primary = FontTextColor.ToSKColor();
        style.Outline = FontOutlineColor.ToSKColor();
        style.OutlineWidth = SelectedFontOutline ?? 0;
        style.ShadowWidth = SelectedFontShadowWidth ?? 0;
        style.Alignment = SelectedFontAlignment.Code;
        style.MarginLeft = FontMarginHorizontal ?? 0;
        style.MarginRight = FontMarginHorizontal ?? 0;
        style.MarginVertical = FontMarginVertical ?? 0;

        if (SelectedFontBoxType.BoxType == FontBoxType.None)
        {
            style.BorderStyle = "0"; // bo box
        }
        else if (SelectedFontBoxType.BoxType == FontBoxType.BoxPerLine)
        {
            style.BorderStyle = "3"; // box - per line
        }
        else
        {
            style.BorderStyle = "4"; // box - multi line
        }

        sub.Header =
            AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(sub.Header,
                new List<SsaStyle> { style });
        sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX",
            "PlayResX: " + (VideoWidth ?? 1920).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
        sub.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY",
            "PlayResY: " + (VideoHeight ?? 1920).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
    }

    private string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = SelectedVideoExtension;
        var suffix = Se.Settings.Video.BurnIn.BurnInSuffix;
        var fileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, nameNoExt + suffix + ext);
        if (Se.Settings.Video.BurnIn.UseOutputFolder &&
            !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder) &&
            Directory.Exists(Se.Settings.Video.BurnIn.OutputFolder))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, nameNoExt + suffix + ext);
        }

        var i = 2;
        while (File.Exists(fileName))
        {
            fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, $"{nameNoExt}{suffix}_{i}{ext}");
            i++;
        }

        return fileName;
    }

    public static int CalculateFontSize(int videoWidth, int videoHeight, double factor, int minSize = 8,
        int maxSize = 2000)
    {
        factor = Math.Clamp(factor, 0, 1);

        // Calculate the diagonal resolution
        var diagonalResolution = Math.Sqrt(videoWidth * videoWidth + videoHeight * videoHeight);

        // Calculate base size (when factor is 0.5)
        var baseSize = diagonalResolution * 0.019; // around 2% of diagonal as base size

        // Apply logarithmic scaling
        var scaleFactor = Math.Pow(maxSize / baseSize, 2 * (factor - 0.5));
        var fontSize = (int)Math.Round(baseSize * scaleFactor);

        // Clamp the font size between minSize and maxSize
        return Math.Clamp(fontSize, minSize, maxSize);
    }

    [RelayCommand]
    private async Task PromptFfmpegParametersAndGeenrate()
    {
        if (Window == null)
        {
            return;
        }

        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        PromptForFfmpegParameters = true;
        await Generate();
        PromptForFfmpegParameters = false;
    }


    [RelayCommand]
    private async Task Add()
    {
        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window!, Se.Language.General.OpenSubtitles);
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var videoFileName = string.Empty;
            var jobItem = new BurnInJobItem(videoFileName, VideoWidth ?? 0, VideoHeight ?? 0);
            jobItem.AddSubtitleFileName(fileName);
            Dispatcher.UIThread.Invoke(() => { JobItems.Add(jobItem); });
        }
    }

    [RelayCommand]
    private async Task OpenOutputFolder()
    {
        await _folderHelper.OpenFolder(Window!, Se.Settings.Video.BurnIn.OutputFolder);
    }

    [RelayCommand]
    private void Remove()
    {
        if (SelectedJobItem != null)
        {
            var idx = JobItems.IndexOf(SelectedJobItem);
            JobItems.Remove(SelectedJobItem);
        }
    }

    [RelayCommand]
    private void Clear()
    {
        JobItems.Clear();
    }

    [RelayCommand]
    private async Task PickVideoFile()
    {
        if (SelectedJobItem == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenVideoFile(Window!, "Open video file");
        if (!string.IsNullOrEmpty(fileName))
        {
            SelectedJobItem.InputVideoFileName = fileName;
            SelectedJobItem.InputVideoFileName = Path.GetFileName(fileName);
        }
    }

    [RelayCommand]
    private async Task OutputProperties()
    {
        await _windowService.ShowDialogAsync<BurnInSettingsWindow, BurnInSettingsViewModel>(Window!);
        UpdateOutputProperties();
    }

    [RelayCommand]
    private async Task BrowseResolution()
    {
        var result =
            await _windowService
                .ShowDialogAsync<BurnInResolutionPickerWindow, BurnInResolutionPickerViewModel>(Window!);
        if (!result.OkPressed || result.SelectedResolution == null)
        {
            return;
        }

        if (result.SelectedResolution.ItemType == ResolutionItemType.PickResolution)
        {
            var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, "Open video file");
            if (string.IsNullOrWhiteSpace(videoFileName))
            {
                return;
            }

            var mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
            VideoWidth = mediaInfo.Dimension.Width;
            VideoHeight = mediaInfo.Dimension.Height;
            UseSourceResolution = false;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.UseSource)
        {
            UseSourceResolution = true;
        }
        else if (result.SelectedResolution.ItemType == ResolutionItemType.Resolution)
        {
            UseSourceResolution = false;
            VideoWidth = result.SelectedResolution.Width;
            VideoHeight = result.SelectedResolution.Height;
        }

        SaveSettings();
    }

    [RelayCommand]
    private async Task BrowseCutFrom()
    {
        var result =
            await _windowService.ShowDialogAsync<SelectVideoPositionWindow, SelectVideoPositionViewModel>(Window!,
                vm => { vm.Initialize(VideoFileName); });

        if (!result.OkPressed)
        {
            return;
        }

        CutFrom = TimeSpan.FromSeconds((long)Math.Round(result.PositionInSeconds, MidpointRounding.AwayFromZero));
    }

    [RelayCommand]
    private async Task BrowseCutTo()
    {
        var result =
            await _windowService.ShowDialogAsync<SelectVideoPositionWindow, SelectVideoPositionViewModel>(Window!,
                vm => { vm.Initialize(VideoFileName); });

        if (!result.OkPressed)
        {
            return;
        }

        CutTo = TimeSpan.FromSeconds((long)Math.Round(result.PositionInSeconds, MidpointRounding.AwayFromZero));
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (Window == null)
        {
            return;
        }

        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (IsCutActive && CutFrom >= CutTo)
        {
            await MessageBox.Show(Window!,
                "Cut settings error",
                "Cut end time must be after cut start time",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        if (!IsBatchMode)
        {
            JobItems = GetCurrentVideoAsJobItems();
        }

        if (IsBatchMode && JobItems.Count == 0)
        {
            await Add();

            if (IsBatchMode && JobItems.Count == 0)
            {
                return;
            }
        }

        if (JobItems.Count == 0)
        {
            return;
        }

        // check that all jobs have subtitles
        foreach (var jobItem in JobItems)
        {
            if (string.IsNullOrWhiteSpace(jobItem.SubtitleFileName))
            {
                await MessageBox.Show(Window!,
                    "Missing subtitle",
                    "Please add a subtitle to all batch items",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }
        }

        _doAbort = false;
        _log.Clear();
        IsGenerating = true;
        _processedFrames = 0;
        ProgressValue = 0;
        SaveSettings();

        await InitAndStartJobItem(0);
    }

    private void LoadSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        FontFactor = settings.FontFactor;
        FontIsBold = settings.FontBold;
        SelectedFontOutline = settings.OutlineWidth;
        SelectedFontShadowWidth = settings.ShadowWidth;
        SelectedFontName = settings.FontName;
        FontTextColor = settings.NonAssaTextColor.FromHexToColor();
        FontOutlineColor = settings.NonAssaOutlineColor.FromHexToColor();
        FontBoxColor = settings.NonAssaBoxColor.FromHexToColor();
        FontShadowColor = settings.NonAssaShadowColor.FromHexToColor();
        FontFixRtl = settings.NonAssaFixRtlUnicode;
        SelectedFontAlignment = FontAlignments.First(p => p.Code == settings.NonAssaAlignment);
        OutputFolder = settings.OutputFolder;
        UseOutputFolderVisible = settings.UseSourceResolution;
        UseSourceFolderVisible = !settings.UseOutputFolder;
        UseSourceResolution = settings.UseSourceResolution;

        var effectsAsStringArray = settings.Effects?.Split(',') ?? [];
        _selectedEffects = BurnInEffectItem.List().Where(p => effectsAsStringArray.Contains(p.Name)).ToList();
        DisplayEffect = string.Join(", ", _selectedEffects.Select(p => p.Name));
    }

    private void SaveSettings()
    {
        var settings = Se.Settings.Video.BurnIn;
        settings.FontFactor = FontFactor ?? 0;
        settings.FontBold = FontIsBold;
        settings.OutlineWidth = SelectedFontOutline ?? 0;
        settings.ShadowWidth = SelectedFontShadowWidth ?? 0;
        settings.FontName = SelectedFontName;
        settings.NonAssaTextColor = FontTextColor.FromColorToHex();
        settings.NonAssaOutlineColor = FontOutlineColor.FromColorToHex();
        settings.NonAssaBoxColor = FontBoxColor.FromColorToHex();
        settings.NonAssaShadowColor = FontShadowColor.FromColorToHex();
        settings.NonAssaFixRtlUnicode = FontFixRtl;
        settings.NonAssaAlignment = SelectedFontAlignment.Code;
        settings.UseSourceResolution = UseSourceResolution;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void SingleMode()
    {
        IsBatchMode = false;
        IsSingleModeVisible = false;
    }

    [RelayCommand]
    private void BatchMode()
    {
        IsBatchMode = true;
        IsSingleModeVisible = !string.IsNullOrEmpty(_inputVideoFileName);
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        if (IsGenerating)
        {
            _doAbort = true;
            return;
        }

        Window?.Close();
    }

    [RelayCommand]
    private async Task ShowEffects()
    {
        var result = await _windowService.ShowDialogAsync<BurnInEffectWindow, BurnInEffectViewModel>(Window!, vm =>
        {
            vm.Initialize(VideoFileName, _selectedEffects);
        });

        if (!result.OkPressed)
        {
            return;
        }

        _selectedEffects = result.SelectedEffects.ToList();
        DisplayEffect = string.Join(", ", _selectedEffects.Select(p => p.Name));
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/transparent-subtitles");
        }
    }

    private static string TryGetSubtitleFileName(string fileName)
    {
        var srt = Path.ChangeExtension(fileName, ".srt");
        if (File.Exists(srt))
        {
            return srt;
        }

        var assa = Path.ChangeExtension(fileName, ".ass");
        if (File.Exists(srt))
        {
            return assa;
        }

        var dir = Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(dir))
        {
            return string.Empty;
        }

        var searchPath = Path.GetFileNameWithoutExtension(fileName);
        var files = Directory.GetFiles(dir, searchPath + "*");
        var subtitleExtensions = SubtitleFormat.AllSubtitleFormats.Select(p => p.Extension).Distinct();
        foreach (var ext in subtitleExtensions)
        {
            foreach (var file in files)
            {
                if (file.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                {
                    return file;
                }
            }
        }

        return string.Empty;
    }

    private void UpdateOutputProperties()
    {
        if (Se.Settings.Video.Transparent.UseOutputFolder &&
            string.IsNullOrWhiteSpace(Se.Settings.Video.Transparent.OutputFolder))
        {
            Se.Settings.Video.Transparent.UseOutputFolder = true;
        }

        UseSourceFolderVisible = !Se.Settings.Video.Transparent.UseOutputFolder;
        UseOutputFolderVisible = Se.Settings.Video.Transparent.UseOutputFolder;
        OutputFolder = Se.Settings.Video.Transparent.OutputFolder;
    }

    public void BoxTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        BoxTypeChanged();
    }

    private void BoxTypeChanged()
    {
        if (SelectedFontBoxType.BoxType == FontBoxType.None)
        {
            FontOutlineText = "Outline";
            FontShadowText = "Shadow";
        }

        if (SelectedFontBoxType.BoxType == FontBoxType.OneBox)
        {
            FontOutlineText = "Outline";
            FontShadowText = "Box";
        }

        if (SelectedFontBoxType.BoxType == FontBoxType.BoxPerLine)
        {
            FontOutlineText = "Box";
            FontShadowText = "Shadow";
        }

        UpdateNonAssaPreview();
    }

    private void UpdateNonAssaPreview()
    {
        if (_loading || Window == null || !string.IsNullOrEmpty(GetValidationError()))
        {
            return;
        }

        var text = "This is a test";

        if (_subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat } && !IsBatchMode)
        {
            ImagePreview = new SKBitmap(1, 1).ToAvaloniaBitmap();
            return;
        }


        var fontSize = (float)CalculateFontSize(VideoWidth ?? 0, VideoHeight ?? 0, FontFactor ?? 0);
        SKBitmap bitmap;

        if (SelectedFontBoxType.BoxType == FontBoxType.BoxPerLine)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                SelectedFontName,
                fontSize,
                FontIsBold,
                FontTextColor.ToSKColor(),
                FontShadowColor.ToSKColor(),
                FontOutlineColor.ToSKColor(),
                FontOutlineColor.ToSKColor(),
                0,
                (float)(SelectedFontShadowWidth ?? 0));

            if (SelectedFontShadowWidth > 0)
            {
                bitmap = TextToImageGenerator.AddShadowToBitmap(bitmap,
                    (int)Math.Round(SelectedFontShadowWidth ?? 0, MidpointRounding.AwayFromZero),
                    FontShadowColor.ToSKColor());
            }
        }
        else if (SelectedFontBoxType.BoxType == FontBoxType.OneBox)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                SelectedFontName,
                fontSize,
                FontIsBold,
                FontTextColor.ToSKColor(),
                FontOutlineColor.ToSKColor(),
                SKColors.Red,
                FontShadowColor.ToSKColor(),
                (float)(SelectedFontOutline ?? 0),
                0,
                1.0f,
                (int)Math.Round(SelectedFontShadowWidth ?? 0));
        }
        else // FontBoxType.None
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                SelectedFontName,
                fontSize,
                FontIsBold,
                FontTextColor.ToSKColor(),
                FontOutlineColor.ToSKColor(),
                FontShadowColor.ToSKColor(),
                SKColors.Transparent,
                (float)(SelectedFontOutline ?? 0),
                (float)(SelectedFontShadowWidth ?? 0));
        }

        ImagePreview = bitmap.ToAvaloniaBitmap();
    }

    internal void ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        UpdateNonAssaPreview();
    }

    internal void ComboBoxChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateNonAssaPreview();
    }

    internal void NumericUpDownChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        UpdateNonAssaPreview();
    }

    internal void CheckBoxChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateNonAssaPreview();
    }

    internal void TextBoxChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateNonAssaPreview();
    }
}