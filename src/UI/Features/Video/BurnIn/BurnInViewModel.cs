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
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
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

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInViewModel : ObservableObject
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
    [ObservableProperty] private ObservableCollection<VideoEncodingItem> _videoEncodings;
    [ObservableProperty] private VideoEncodingItem _selectedVideoEncoding;
    [ObservableProperty] private ObservableCollection<PixelFormatItem> _videoPixelFormats;
    [ObservableProperty] private PixelFormatItem? _selectedVideoPixelFormat;
    [ObservableProperty] private ObservableCollection<string> _videoPresets;
    [ObservableProperty] private string? _selectedVideoPreset;
    [ObservableProperty] private string _videoPresetText;
    [ObservableProperty] private ObservableCollection<string> _videoCrf;
    [ObservableProperty] private string? _selectedVideoCrf;
    [ObservableProperty] private string _videoCrfText;
    [ObservableProperty] private string _videoCrfHint;
    [ObservableProperty] private ObservableCollection<string> _audioEncodings;
    [ObservableProperty] private string _selectedAudioEncoding;
    [ObservableProperty] private bool _audioIsStereo;
    [ObservableProperty] private ObservableCollection<string> _audioSampleRates;
    [ObservableProperty] private string _selectedAudioSampleRate;
    [ObservableProperty] private ObservableCollection<string> _audioBitRates;
    [ObservableProperty] private string _selectedAudioBitRate;
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
    [ObservableProperty] private bool _showAssaOnlyBox;
    [ObservableProperty] private string _targetVideoBitRateInfo;
    [ObservableProperty] private string _displayEffect;
    [ObservableProperty] private string _logoInfo;
    [ObservableProperty] private bool _promptForFfmpegParameters;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public DataGrid? BatchGrid { get; internal set; }
    public BurnInLogo BurnInLogo { get; set; }

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
    private FfmpegMediaInfo2? _mediaInfo;
    private SubtitleFormat? _subtitleFormat;
    private string _inputVideoFileName;
    private List<BurnInEffectItem> _selectedEffects;

    private readonly IWindowService _windowService;
    private readonly IFolderHelper _folderHelper;
    private readonly IFileHelper _fileHelper;

    public BurnInViewModel(IFolderHelper folderHelper, IFileHelper fileHelper, IWindowService windowService)
    {
        _folderHelper = folderHelper;
        _fileHelper = fileHelper;
        _windowService = windowService;

        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFontName = FontNames.FirstOrDefault(p => p == Se.Settings.Video.BurnIn.FontName) ?? FontNames[0];

        // font factors between 0-1
        FontFactor = 0.4;
        FontFactorText = string.Empty;

        VideoPresets = new ObservableCollection<string>();

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

        AudioEncodings = new ObservableCollection<string>
        {
            "copy",
            "aac",
            "ac3",
            "mp3",
            "opus",
            "vorbis",
        };
        SelectedAudioEncoding = "copy";

        AudioSampleRates = new ObservableCollection<string>
        {
            "44100 Hz",
            "48000 Hz",
            "88200 Hz",
            "96000 Hz",
            "192000 Hz",
        };
        SelectedAudioSampleRate = AudioSampleRates[1];

        AudioBitRates = new ObservableCollection<string>
        {
            "64k",
            "96k",
            "128k",
            "160k",
            "192k",
            "256k",
            "320k",
        };
        SelectedAudioBitRate = AudioBitRates[2];

        VideoPixelFormats = new ObservableCollection<PixelFormatItem>(PixelFormatItem.PixelFormats);

        VideoEncodings = new ObservableCollection<VideoEncodingItem>(VideoEncodingItem.VideoEncodings);
        SelectedVideoEncoding = VideoEncodings[0];

        VideoCrf = new ObservableCollection<string>();

        JobItems = new ObservableCollection<BurnInJobItem>();

        VideoFileName = string.Empty;
        VideoFileSize = string.Empty;
        ProgressText = string.Empty;
        FontOutlineText = string.Empty;
        FontShadowText = string.Empty;
        VideoPresetText = string.Empty;
        VideoCrfText = string.Empty;
        VideoCrfHint = string.Empty;
        OutputFolder = string.Empty;
        TargetVideoBitRateInfo = string.Empty;
        DisplayEffect = string.Empty;
        LogoInfo = string.Empty;

        BurnInLogo = new BurnInLogo();
        _selectedEffects = new List<BurnInEffectItem>();
        _log = new StringBuilder();
        _loading = false;
        _inputVideoFileName = string.Empty;

        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 250;

        _timerAnalyze = new();
        // Change the event handler assignment for _timerAnalyze.Elapsed to use a synchronous wrapper
        _timerAnalyze.Elapsed += TimerAnalyzeElapsed;
        _timerAnalyze.Interval = 100;

        LoadSettings();
        BoxTypeChanged();
        VideoEncodingChanged();
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
            SingleMode();
            VideoFileSize = Utilities.FormatBytesToDisplayFileSize(new FileInfo(videoFileName).Length);
            _ = Task.Run(() =>
            {
                _mediaInfo = FfmpegMediaInfo2.Parse(videoFileName);
                Dispatcher.UIThread.Post(() =>
                {
                    VideoWidth = _mediaInfo.Dimension.Width;
                    VideoHeight = _mediaInfo.Dimension.Height;
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

        Dispatcher.UIThread.Invoke(async () =>
        {
            var jobItem = JobItems[_jobItemIndex];
            var process = await GetFfmpegProcess(jobItem, 2);
            if (process == null)
            {
                IsGenerating = false;
                return;
            }

            _ffmpegProcess = process;
#pragma warning disable CA1416 // Validate platform compatibility
            _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
            _ffmpegProcess.BeginOutputReadLine();
            _ffmpegProcess.BeginErrorReadLine();

            _timerGenerate.Start();
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
                ProgressText =
                    $"Generating video {_jobItemIndex + 1}/{JobItems.Count}... {percentage}%     {estimatedLeft}";
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
                await InitAndStartJobItem(_jobItemIndex + 1).ConfigureAwait(false);
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
        if (IsBatchMode)
        {
            jobItem.OutputVideoFileName = MakeOutputFileName(jobItem.InputVideoFileName);
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (BatchGrid == null || index >= JobItems.Count)
            {
                return;
            }

            BatchGrid.SelectedItem = jobItem;
            BatchGrid.ScrollIntoView(jobItem, null);
        });

        bool result;
        if (jobItem.UseTargetFileSize)
        {
            result = await RunTwoPassEncoding(jobItem);
            if (result)
            {
                _timerAnalyze.Start();
            }
        }
        else
        {
            result = await RunOnePassEncoding(jobItem);
            if (result)
            {
                _timerGenerate.Start();
            }
        }
    }

    private async Task<bool> RunTwoPassEncoding(BurnInJobItem jobItem)
    {
        var bitRate = GetVideoBitRate(jobItem);
        jobItem.VideoBitRate = bitRate.ToString(CultureInfo.InvariantCulture) + "k";

        if (bitRate < 10)
        {
            _ = Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!,
                    "Unable to generate video",
                    $"Bit rate too low: {bitRate}k",
                    MessageBoxButtons.OK);
            });
            return false;
        }

        var process = await GetFfmpegProcess(jobItem, 1);
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
        _startTicks = DateTime.UtcNow.Ticks;

        return true;
    }

    private int GetVideoBitRate(BurnInJobItem item)
    {
        if (TargetFileSize == null)
        {
            return 0;
        }

        var audioMb = 0;
        if (SelectedAudioEncoding == "copy")
        {
            audioMb = GetAudioFileSizeInMb(item);
        }

        // (MiB * 8192 [converts MiB to kBit]) / video seconds = kBit/s total bitrate
        var bitRate = (int)Math.Round(((double)TargetFileSize - audioMb) * 8192.0 / item.TotalSeconds);
        if (SelectedAudioEncoding != "copy" && !string.IsNullOrWhiteSpace(SelectedAudioBitRate))
        {
            var audioBitRate = int.Parse(SelectedAudioBitRate.RemoveChar('k').TrimEnd());
            bitRate -= audioBitRate;
        }

        return bitRate;
    }

    private int GetAudioFileSizeInMb(BurnInJobItem item)
    {
        var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
        if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
        {
            ffmpegLocation = "ffmpeg";
        }

        var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".aac");
        var process = new Process
        {
            StartInfo =
            {
                FileName = ffmpegLocation,
                Arguments = $"-i \"{item.InputVideoFileName}\" -vn -acodec copy \"{tempFileName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };


#pragma warning disable CA1416
        _ = process.Start();
#pragma warning restore CA1416
        process.WaitForExit();
        try
        {
            var length = (int)Math.Round(new FileInfo(tempFileName).Length / 1024.0 / 1024);
            try
            {
                File.Delete(tempFileName);
            }
            catch
            {
                // ignore
            }

            return length;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<bool> RunOnePassEncoding(BurnInJobItem jobItem)
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

    private async Task<Process?> GetFfmpegProcess(BurnInJobItem jobItem, int? passNumber = null, bool preview = false)
    {
        var audioCutTracks = string.Empty;
        //if (listViewAudioTracks.Visible)
        //{
        //    for (var index = 0; index < listViewAudioTracks.Items.Count; index++)
        //    {
        //        var listViewItem = listViewAudioTracks.Items[index];
        //        if (!listViewItem.Checked)
        //        {
        //            audioCutTracks += $"-map 0:a:{index} ";
        //        }
        //    }
        //}

        var pass = string.Empty;
        if (passNumber.HasValue)
        {
            pass = passNumber.Value.ToString(CultureInfo.InvariantCulture);
        }

        var cutStart = string.Empty;
        var cutEnd = string.Empty;
        if (IsCutActive && !preview)
        {
            var start = CutFrom;
            cutStart = $"-ss {start.Hours:00}:{start.Minutes:00}:{start.Seconds:00}";

            var end = CutTo;
            var duration = end - start;
            cutEnd = $"-t {duration.Hours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
        }

        var ffmpegParameters = FfmpegGenerator.GenerateHardcodedVideoFile(
            jobItem.InputVideoFileName,
            jobItem.AssaSubtitleFileName,
            jobItem.OutputVideoFileName,
            jobItem.Width,
            jobItem.Height,
            SelectedVideoEncoding.Codec,
            SelectedVideoPreset ?? string.Empty,
            SelectedVideoPixelFormat?.Codec ?? string.Empty,
            SelectedVideoCrf ?? string.Empty,
            SelectedAudioEncoding,
            AudioIsStereo,
            SelectedAudioSampleRate.Replace("Hz", string.Empty).Trim(),
            string.Empty,
            SelectedAudioBitRate,
            pass,
            jobItem.VideoBitRate,
            cutStart,
            cutEnd,
            audioCutTracks,
            BurnInLogo);

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

    private ObservableCollection<BurnInJobItem> GetCurrentVideoAsJobItems(string outputVideoFileName)
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

        _mediaInfo = FfmpegMediaInfo2.Parse(VideoFileName);
        VideoWidth = _mediaInfo.Dimension.Width;
        VideoHeight = _mediaInfo.Dimension.Height;

        var jobItem = new BurnInJobItem(VideoFileName, _mediaInfo.Dimension.Width, _mediaInfo.Dimension.Height)
        {
            InputVideoFileName = VideoFileName,
            OutputVideoFileName = outputVideoFileName,
            UseTargetFileSize = UseTargetFileSize,
            TargetFileSize = TargetFileSize ?? 0,
        };
        jobItem.AddSubtitleFileName(subtitleFileName);

        return new ObservableCollection<BurnInJobItem>(new[] { jobItem });
    }

    private string MakeAssa(string subtitleFileName)
    {
        var jobItem = JobItems[_jobItemIndex];

        if (string.IsNullOrWhiteSpace(subtitleFileName) || !File.Exists(subtitleFileName))
        {
            jobItem.Status = "Skipped";
            return string.Empty;
        }

        var isAssa = subtitleFileName.EndsWith(".ass", StringComparison.OrdinalIgnoreCase);

        var subtitle = Subtitle.Parse(subtitleFileName);

        if (!isAssa)
        {
            foreach (var s in subtitle.Paragraphs)
            {
                foreach (var effect in _selectedEffects)
                {
                    var fontSize = CalculateFontSize(jobItem.Width, jobItem.Height, FontFactor ?? 0);
                    var durationMs = (int)(s.Duration.TotalMilliseconds);
                    s.Text = effect.ApplyEffect(s.Text, VideoWidth ?? 0, VideoHeight ?? 0, fontSize, durationMs);
                }
            }

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
            "PlayResY: " + (VideoHeight ?? 1024).ToString(CultureInfo.InvariantCulture), "[Script Info]", sub.Header);
    }

    private static string MakeOutputFileName(string videoFileName)
    {
        var nameNoExt = Path.GetFileNameWithoutExtension(videoFileName);
        var ext = Path.GetExtension(videoFileName).ToLowerInvariant();
        if (ext != ".mp4" && ext != ".mkv")
        {
            ext = ".mkv";
        }

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
            if (Se.Settings.Video.BurnIn.UseOutputFolder && !string.IsNullOrEmpty(Se.Settings.Video.BurnIn.OutputFolder))
            {
                fileName = Path.Combine(Se.Settings.Video.BurnIn.OutputFolder, $"{nameNoExt}{suffix}_{i}{ext}");
            }
            else
            {
                fileName = Path.Combine(Path.GetDirectoryName(videoFileName) ?? Path.GetTempPath(), $"{nameNoExt}{suffix}_{i}{ext}");
            }

            i++;
        }

        return fileName;
    }

    public static int CalculateFontSize(int videoWidth, int videoHeight, double factor, int minSize = 8, int maxSize = 2000)
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
    private async Task ShowLogo()
    {
        if (Window == null || VideoWidth == null || VideoHeight == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<BurnInLogoWindow, BurnInLogoViewModel>(Window!, vm =>
        {
            vm.BurnInLogo = BurnInLogo;
            vm.Initialize(VideoFileName, VideoWidth.Value, VideoHeight.Value);
        });

        if (result != null && result.OkPressed)
        {
            BurnInLogo = result.BurnInLogo;
            UpdateLogoInfo();
        }
    }

    private void UpdateLogoInfo()
    {
        if (string.IsNullOrEmpty(BurnInLogo.LogoFileName))
        {
            LogoInfo = string.Empty;
        }
        else
        {
            var fileName = Path.GetFileName(BurnInLogo.LogoFileName);
            LogoInfo = $"{fileName} @ ({BurnInLogo.X}, {BurnInLogo.Y})";
        }
    }

    [RelayCommand]
    private async Task PromptFfmpegParametersAndGenerate()
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
    private async Task Help()
    {
        var codec = SelectedVideoEncoding.Codec;

        if (codec == "libx265")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("http://trac.ffmpeg.org/wiki/Encode/H.265"));
        }
        else if (codec == "libvpx-vp9")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("http://trac.ffmpeg.org/wiki/Encode/VP9"));
        }
        else if (codec is "h264_nvenc" or "hevc_nvenc")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("https://trac.ffmpeg.org/wiki/HWAccelIntro"));
        }
        else if (codec == "prores_ks")
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("https://ottverse.com/ffmpeg-convert-to-apple-prores-422-4444-hq"));
        }
        else if (codec.Contains("av1"))
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("https://trac.ffmpeg.org/wiki/Encode/AV1"));
        }
        else
        {
            await Window!.Launcher.LaunchUriAsync(new Uri("http://trac.ffmpeg.org/wiki/Encode/H.264"));
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        var fileNames = await _fileHelper.PickOpenVideoFiles(Window!, Se.Language.General.AddVideoFiles);
        if (fileNames == null || fileNames.Length == 0)
        {
            return;
        }

        var error = false;

        foreach (var fileName in fileNames)
        {
            var mediaInfo = FfmpegMediaInfo.Parse(fileName);
            var fileInfo = new FileInfo(fileName);

            if (mediaInfo.Duration == null || mediaInfo.Dimension.Width == 0 || mediaInfo.Dimension.Height == 0)
            {
                error = true;
            }
            else
            {

                var jobItem = new BurnInJobItem(fileName, mediaInfo.Dimension.Width, mediaInfo.Dimension.Height)
                {
                    OutputVideoFileName = MakeOutputFileName(fileName),
                    TotalFrames = mediaInfo.GetTotalFrames(),
                    TotalSeconds = mediaInfo.Duration.TotalSeconds,
                    Width = mediaInfo.Dimension.Width,
                    Height = mediaInfo.Dimension.Height,
                    Size = Utilities.FormatBytesToDisplayFileSize(fileInfo.Length),
                    Resolution = mediaInfo.Dimension.ToString(),
                };
                jobItem.AddSubtitleFileName(TryGetSubtitleFileName(fileName));

                Dispatcher.UIThread.Invoke(() => { JobItems.Add(jobItem); });
            }
        }

        if (error)
        {
            await MessageBox.Show(Window!,
                    "Unable to get video info",
                    "File skipped as video info was unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
    private async Task PickSubtitle()
    {
        if (SelectedJobItem == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle, false);
        if (!string.IsNullOrEmpty(fileName))
        {
            SelectedJobItem.SubtitleFileName = fileName;
            SelectedJobItem.SubtitleFileNameShort = Path.GetFileName(fileName);
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
        var result = await _windowService.ShowDialogAsync<BurnInResolutionPickerWindow, BurnInResolutionPickerViewModel>(Window!);
        if (!result.OkPressed || result.SelectedResolution == null)
        {
            return;
        }

        if (result.SelectedResolution.ItemType == ResolutionItemType.PickResolution)
        {
            var videoFileName = await _fileHelper.PickOpenVideoFile(Window!, Se.Language.General.OpenVideoFileTitle);
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
        var result = await _windowService.ShowDialogAsync<SelectVideoPositionWindow, SelectVideoPositionViewModel>(Window!, vm =>
        {
            vm.Initialize(VideoFileName);
        });

        if (!result.OkPressed)
        {
            return;
        }

        CutFrom = TimeSpan.FromSeconds((long)Math.Round(result.PositionInSeconds, MidpointRounding.AwayFromZero));
    }

    [RelayCommand]
    private async Task BrowseCutTo()
    {
        var result = await _windowService.ShowDialogAsync<SelectVideoPositionWindow, SelectVideoPositionViewModel>(Window!, vm =>
        {
            vm.Initialize(VideoFileName);
        });

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
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var nameNoExt = Path.GetFileNameWithoutExtension(VideoFileName);
            var path = Path.GetDirectoryName(VideoFileName) ?? string.Empty;
            var ext = Path.GetExtension(VideoFileName).ToLowerInvariant();
            if (ext != ".mp4" && ext != ".mkv")
            {
                ext = ".mkv";
            }

            var suggestedFileName = Path.Combine(path, nameNoExt + ext);
            var i = 2;
            while (File.Exists(suggestedFileName))
            {
                suggestedFileName = Path.Combine(path, $"{nameNoExt}_{i}{ext}");
                i++;
            }

            var outputVideoFileName = await _fileHelper.PickSaveFile(Window!, ext, suggestedFileName, Se.Language.General.SaveVideoAsVideoTitle);
            if (string.IsNullOrEmpty(outputVideoFileName))
            {
                return;
            }

            if (VideoFileName.Equals(outputVideoFileName, StringComparison.OrdinalIgnoreCase))
            {
                await MessageBox.Show(Window!,
                    "Output file error",
                    "Output video file must be different from input video file",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            JobItems = GetCurrentVideoAsJobItems(outputVideoFileName);
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

        settings.Effects = string.Join(",", _selectedEffects.Select(p => p.Name).Distinct());

        Se.SaveSettings();
    }

    [RelayCommand]
    private void SingleMode()
    {
        IsBatchMode = false;
        IsSingleModeVisible = false;

        var isAssa = _subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat };
        ShowAssaOnlyBox = isAssa;
    }

    [RelayCommand]
    private void BatchMode()
    {
        IsBatchMode = true;
        IsSingleModeVisible = !string.IsNullOrEmpty(_inputVideoFileName);
        ShowAssaOnlyBox = false;
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

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (FontFactor == null)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Font factor");
        }

        if (FontFactor < 0.1)
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
            UiUtil.ShowHelp("features/burn-in");
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

    internal void VideoEncodingChanged(object? sender, SelectionChangedEventArgs e)
    {
        VideoEncodingChanged();
    }

    private void VideoEncodingChanged()
    {
        if (_loading)
        {
            return;
        }

        FillPreset(SelectedVideoEncoding.Codec);
        FillCrf(SelectedVideoEncoding.Codec);
    }

    private void FillPreset(string videoCodec)
    {
        VideoPresetText = "Preset";
        SelectedVideoPreset = null;

        var items = new List<string>
        {
            "ultrafast",
            "superfast",
            "veryfast",
            "faster",
            "fast",
            "medium",
            "slow",
            "slower",
            "veryslow",
        };

        var defaultItem = "medium";

        if (videoCodec == "h264_nvenc")
        {
            items = new List<string>
            {
                "default",
                "slow",
                "medium",
                "fast",
                "hp",
                "hq",
                "bd",
                "ll",
                "llhq",
                "llhp",
                "lossless",
                "losslesshp",
                "p1",
                "p2",
                "p3",
                "p4",
                "p5",
                "p6",
                "p7",
            };
        }
        else if (videoCodec == "hevc_nvenc")
        {
            items = new List<string>
            {
                "default",
                "slow",
                "medium",
                "fast",
                "hp",
                "hq",
                "bd",
                "ll",
                "llhq",
                "llhp",
                "lossless",
                "losslesshp",
                "p1",
                "p2",
                "p3",
                "p4",
                "p5",
                "p6",
                "p7",
            };
        }
        else if (videoCodec == "h264_amf")
        {
            items = new List<string> { string.Empty };
        }
        else if (videoCodec == "hevc_amf")
        {
            items = new List<string> { string.Empty };
        }
        else if (videoCodec == "libvpx-vp9")
        {
            items = new List<string> { string.Empty };
        }
        else if (videoCodec == "prores_ks")
        {
            items = new List<string>
            {
                "proxy",
                "lt",
                "standard",
                "hq",
                "4444",
                "4444xq",
            };

            defaultItem = "standard";

            VideoPresetText = "Profile";
        }
        else if (videoCodec.Contains("av1"))
        {
            items = new List<string>
            {
                "",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11",
                "12",
                "13",
            };

            defaultItem = string.Empty;

            VideoPresetText = Se.Language.Video.BurnIn.Preset;
        }

        VideoPresets.Clear();
        VideoPresets.AddRange(items);
        if (VideoPresets.Contains(defaultItem))
        {
            SelectedVideoPreset = defaultItem;
        }
    }

    public void FillCrf(string videoCodec)
    {
        SelectedVideoCrf = null;
        VideoCrfText = "CRF";
        VideoCrfHint = string.Empty;

        var items = new List<string> { " " };

        if (videoCodec == "libx265")
        {
            for (var i = 0; i < 51; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "28";
        }
        else if (videoCodec == "libvpx-vp9")
        {
            for (var i = 4; i <= 63; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "10";
        }
        else if (videoCodec == "h264_nvenc" ||
                 videoCodec == "hevc_nvenc")
        {
            for (var i = 0; i <= 51; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrfText = "CQ";
            VideoCrfHint = "0=best quality, 51=best speed";
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = null;
        }
        else if (videoCodec == "h264_amf" ||
                 videoCodec == "hevc_amf")
        {
            for (var i = 0; i <= 10; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrfText = "Quality";
            VideoCrfHint = "0=best quality, 10=best speed";
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = null;
        }
        else if (videoCodec.Contains("av1"))
        {
            for (var i = 0; i <= 63; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "30";
        }
        else if (videoCodec == "prores_ks")
        {
            items = new List<string>();
            VideoCrf.Clear();
            VideoCrf.AddRange(items);
        }
        else
        {
            for (var i = 17; i <= 28; i++)
            {
                items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            VideoCrf.Clear();
            VideoCrf.AddRange(items);
            SelectedVideoCrf = "23";
        }
    }

    private void UpdateOutputProperties()
    {
        if (Se.Settings.Video.BurnIn.UseOutputFolder &&
            string.IsNullOrWhiteSpace(Se.Settings.Video.BurnIn.OutputFolder))
        {
            Se.Settings.Video.BurnIn.UseOutputFolder = true;
        }

        UseSourceFolderVisible = !Se.Settings.Video.BurnIn.UseOutputFolder;
        UseOutputFolderVisible = Se.Settings.Video.BurnIn.UseOutputFolder;
        OutputFolder = Se.Settings.Video.BurnIn.OutputFolder;
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
        if (_loading || !string.IsNullOrEmpty(GetValidationError()))
        {
            return;
        }        

        var text = "This is a test";

        if (_subtitleFormat is { Name: AdvancedSubStationAlpha.NameOfFormat } && !IsBatchMode)
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
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
                bitmap = TextToImageGenerator.AddShadowToBitmap(bitmap, (int)Math.Round(SelectedFontShadowWidth ?? 0, MidpointRounding.AwayFromZero), FontShadowColor.ToSKColor());
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

        ImagePreview = bitmap.CropTransparentColors().ToAvaloniaBitmap();
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

    internal void ParameterChanged()
    {
        UpdateNonAssaPreview();
    }

    internal void TextBoxChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateNonAssaPreview();
    }

    private int GetAudioFileSizeInMb()
    {
        var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
        if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
        {
            ffmpegLocation = "ffmpeg";
        }

        var tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".aac");
        var process = new Process
        {
            StartInfo =
                {
                    FileName = ffmpegLocation,
                    Arguments = $"-i \"{_inputVideoFileName}\" -vn -acodec copy \"{tempFileName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
        };

        process.Start();
        process.WaitForExit();
        try
        {
            var length = (int)Math.Round(new FileInfo(tempFileName).Length / 1024.0 / 1024);
            try
            {
                File.Delete(tempFileName);
            }
            catch
            {
                // ignore
            }

            return length;
        }
        catch
        {
            return 0;
        }
    }

    private int GetVideoBitRate()
    {
        if (TargetFileSize == null || TargetFileSize < 1)
        {
            return 0;
        }

        var audioMb = 0;
        if (SelectedAudioEncoding == "copy")
        {
            audioMb = GetAudioFileSizeInMb();
        }

        if (_mediaInfo == null || _mediaInfo.Duration == null)
        {
            return 0; // Avoid division by zero
        }

        // (MiB * 8192 [converts MiB to kBit]) / video seconds = kBit/s total bitrate
        var bitRate = (int)Math.Round(((double)TargetFileSize - audioMb) * 8192.0 / _mediaInfo.Duration.TotalSeconds);
        if (SelectedAudioEncoding != "copy" && !string.IsNullOrWhiteSpace(SelectedAudioBitRate))
        {
            var audioBitRate = int.Parse(SelectedAudioBitRate.RemoveChar('k').TrimEnd());
            bitRate -= audioBitRate;
        }

        return bitRate;
    }


    internal void CalculateTargetFileBitRate()
    {
        TargetVideoBitRateInfo = string.Empty;

        if (!UseTargetFileSize || _mediaInfo == null || TargetFileSize == null || TargetFileSize < 1)
        {
            return;
        }

        var videoBitRate = GetVideoBitRate();
        if (videoBitRate <= 0)
        {
            return;
        }

        var separateAudio = SelectedAudioEncoding != "copy" && !string.IsNullOrWhiteSpace(SelectedAudioBitRate);
        var audioBitRate = 0;
        if (separateAudio)
        {
            audioBitRate = int.Parse(SelectedAudioBitRate.RemoveChar('k').TrimEnd());
        }

        if (SelectedAudioEncoding == "copy")
        {
            var audioTrack = _mediaInfo.Tracks.FirstOrDefault(p => p.TrackType == FfmpegTrackType.Audio);
            if (audioTrack?.BitRate > 0)
            {
                audioBitRate = audioTrack.BitRate / 1024;
            }
            else
            {
                return;
            }
        }

        TargetVideoBitRateInfo = string.Format(Se.Language.Video.BurnIn.TotalBitRateX, $"{(videoBitRate + audioBitRate):#,###,##0}k");
        if (separateAudio)
        {
            TargetVideoBitRateInfo += $" ({videoBitRate:#,###,##0}k + {audioBitRate:#,###,##0}k)";
        }
    }

    internal void NumericUpDownTargetFileSizeChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        CalculateTargetFileBitRate();
    }

    internal void CheckBoxTargetFileChanged()
    {
        CalculateTargetFileBitRate();
    }

    internal void Loaded()
    {
        Dispatcher.UIThread.Post(() =>
        {
            UpdateNonAssaPreview();
        }); 
    }
}