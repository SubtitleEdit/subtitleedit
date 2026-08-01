using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText.ForcedAlignerSetup;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Nikse.SubtitleEdit.UiLogic.Media;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public partial class ImportPlainTextViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<DisplayFile> _files;
    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<string> _splitAtOptions;
    [ObservableProperty] private string _selectedSplitAtOption;
    [ObservableProperty] private bool _isImportFilesVisible;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private bool _useFixedDuration;
    [ObservableProperty] private int _fixedDurationMs;
    [ObservableProperty] private string _plainText;
    [ObservableProperty] private bool _isAligning;
    [ObservableProperty] private string _alignProgress;
    [ObservableProperty] private double _alignPercent;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private string _videoFileName = string.Empty;
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private static readonly string[] TextFileExtensions = { ".txt", ".rtf" };
    private static readonly List<string> TextFilePatterns = TextFileExtensions.Select(e => "*" + e).ToList();
    private bool _dirty;
    private TaskCompletionSource? _previewFlushed;
    private readonly System.Threading.Lock _previewFlushLock = new();
    private readonly System.Timers.Timer _timerUpdatePreview;

    public ImportPlainTextViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        Files = new ObservableCollection<DisplayFile>();
        SplitAtOptions = new ObservableCollection<string>
        {
            Se.Language.General.Auto,
            Se.Language.File.Import.BlankLines,
            Se.Language.File.Import.OneLineIsOneSubtitle,
            Se.Language.File.Import.TwoLinesAreOneSubtitle,
        };
        SelectedSplitAtOption = SplitAtOptions[0];
        PlainText = string.Empty;
        MinGapMs = Se.Settings.General.MinimumBetweenLines.GetMilliseconds();
        AlignProgress = string.Empty;
        FixedDurationMs = (int)Math.Round(Se.Settings.Tools.AdjustDurations.AdjustDurationFixed * 1000.0, MidpointRounding.AwayFromZero);

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        _timerUpdatePreview.Start();
    }

    private void MarkDirty()
    {
        lock (_previewFlushLock)
        {
            _dirty = true;
            // Invalidate any prior flush wait so a fresh AlignScript call waits for the
            // upcoming update, not a stale one.
            _previewFlushed = null;
        }
    }

    private Task EnsurePreviewFlushedAsync()
    {
        lock (_previewFlushLock)
        {
            if (!_dirty)
            {
                return Task.CompletedTask;
            }

            _previewFlushed ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _previewFlushed.Task;
        }
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            var subtitles = new List<SubtitleLineViewModel>();
            if (IsImportFilesVisible)
            {
                subtitles = UpdatePreviewFiles(Files.ToList());
            }
            else
            {
                subtitles = UpdatePreviewText(SelectedSplitAtOption, PlainText);
            }

            if (!HasTimeCodes(subtitles))
            {
                if (UseFixedDuration)
                {
                    subtitles = TimeCodeCalculator.CalculateFixedTimeCodes(subtitles, FixedDurationMs, MinGapMs);
                }
                else
                {
                    subtitles = TimeCodeCalculator.CalculateTimeCodes(
                        subtitles,
                        Se.Settings.General.SubtitleOptimalCharactersPerSeconds,
                        Se.Settings.General.SubtitleMaximumCharactersPerSeconds,
                        MinGapMs,
                        Se.Settings.General.SubtitleMinimumDisplayMilliseconds,
                        Se.Settings.General.SubtitleMaximumDisplayMilliseconds);
                }
            }

            Dispatcher.UIThread.Post(() =>
            {
                Subtitles.Clear();
                for (int i = 0; i < subtitles.Count; i++)
                {
                    var subtitle = subtitles[i];
                    subtitle.Number = i + 1;
                    Subtitles.Add(subtitle);
                }

                TaskCompletionSource? toSignal;
                lock (_previewFlushLock)
                {
                    _dirty = false;
                    toSignal = _previewFlushed;
                    _previewFlushed = null;
                }
                toSignal?.TrySetResult();
            });
        }
    }

    private static bool HasTimeCodes(List<SubtitleLineViewModel> subtitles)
    {
        return subtitles.Any(s => s.StartTime != TimeSpan.Zero || s.EndTime != TimeSpan.Zero);
    }

    private static List<SubtitleLineViewModel> UpdatePreviewFiles(List<DisplayFile> list)
    {
        var subtitles = new List<SubtitleLineViewModel>();

        foreach (var file in list)
        {
            try
            {
                var text = File.ReadAllText(file.FullPath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var (start, end) = file.GetTimeCodes();
                    subtitles.Add(new SubtitleLineViewModel
                    {
                        Text = text.Trim(),
                        StartTime = start,
                        EndTime = end,
                    });
                }
            }
            catch
            {
                // ignore
            }
        }

        return subtitles;
    }

    private static List<SubtitleLineViewModel> UpdatePreviewText(string splitAtOption, string plainText)
    {
        var subtitles = new List<SubtitleLineViewModel>();

        if (splitAtOption == Se.Language.General.Auto)
        {
            return PlainTextSplitter.AutomaticSplit(plainText, Se.Settings.General.MaxNumberOfLines, Se.Settings.General.SubtitleLineMaximumLength);
        }
        else if (splitAtOption == Se.Language.File.Import.BlankLines)
        {
            var blocks = plainText.SplitToLines();
            var sb = new System.Text.StringBuilder();
            foreach (var block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block))
                {
                    if (sb.Length > 0)
                    {
                        subtitles.Add(new SubtitleLineViewModel { Text = sb.ToString().TrimEnd() });
                        sb.Clear();
                    }
                }
                else
                {
                    sb.AppendLine(block.Trim());
                }
            }

            if (sb.Length > 0)
            {
                subtitles.Add(new SubtitleLineViewModel { Text = sb.ToString().TrimEnd() });
                sb.Clear();
            }
        }
        else if (splitAtOption == Se.Language.File.Import.OneLineIsOneSubtitle)
        {
            var lines = plainText.SplitToLines();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = line.Trim() });
                }
            }
        }
        else if (splitAtOption == Se.Language.File.Import.TwoLinesAreOneSubtitle)
        {
            var lines = plainText.SplitToLines().Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            for (int i = 0; i < lines.Count; i += 2)
            {
                var text = lines[i].Trim();
                if (i + 1 < lines.Count)
                {
                    text += Environment.NewLine + lines[i + 1].Trim();
                }
                if (!string.IsNullOrWhiteSpace(text))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = text });
                }
            }
        }

        return subtitles;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task AlignScriptWithTimestampsFromWhisper()
    {
        if (Window == null)
        {
            return;
        }

        // Wait for any pending preview update to finish populating Subtitles. Deterministic
        // (no fixed Task.Delay) — the preview timer signals via _previewFlushed when done.
        await EnsurePreviewFlushedAsync();

        if (Subtitles.Count == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            var picked = await _fileHelper.PickOpenVideoFile(Window, "Open video file for transcription");
            if (string.IsNullOrEmpty(picked))
            {
                return;
            }

            _videoFileName = picked;
        }

        var whisperVm = await _windowService.ShowDialogAsync<SpeechToTextWindow, SpeechToTextViewModel>(
            Window,
            viewModel => { viewModel.Initialize(_videoFileName, -1); });

        if (!whisperVm.OkPressed || whisperVm.TranscribedSubtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var result = ScriptSyncService.SyncScript(Subtitles.ToList(), whisperVm.TranscribedSubtitle);

        // If more than a quarter of the script couldn't be matched to the transcription,
        // it's likely the transcription is for the wrong audio / language / model. Tell
        // the user so they don't silently end up with bad time codes.
        if (result.TotalLines > 0 && result.UnmatchedLines > result.TotalLines / 4)
        {
            await MessageBox.Show(
                Window,
                Se.Language.General.Warning,
                $"Alignment matched {result.MatchedLines} of {result.TotalLines} lines. "
                + $"{result.UnmatchedLines} line(s) had no match in the transcription and were timed by "
                + "interpolating between their neighbours, so their time codes are approximate. "
                + "If the audio language or content doesn't match the script, the transcription may be off.",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }


    /// <summary>
    /// Aligns the script against the video's audio with a CTC forced aligner instead of a
    /// transcription. The audio is aligned in bounded windows so length is not a limit -
    /// the aligner allocates its encoder activations up front and cannot take a two hour
    /// file in one pass.
    /// </summary>
    [RelayCommand]
    private async Task AlignScriptWithForcedAligner()
    {
        if (Window == null)
        {
            return;
        }

        await EnsurePreviewFlushedAsync();

        if (Subtitles.Count == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(_videoFileName) || !File.Exists(_videoFileName))
        {
            var picked = await _fileHelper.PickOpenVideoFile(Window, "Open video file to align against");
            if (string.IsNullOrEmpty(picked))
            {
                return;
            }

            _videoFileName = picked;
        }

        // Let the user pick the aligner (and install or update the engine) rather than
        // silently using whatever speech-to-text happens to be configured with.
        var setupVm = await _windowService.ShowDialogAsync<ForcedAlignerSetupWindow, ForcedAlignerSetupViewModel>(
            Window, viewModel => viewModel.Initialize());

        if (!setupVm.OkPressed)
        {
            return;
        }

        await RunForcedAlignerAsync(setupVm.ExecutablePath, setupVm.AlignerModelPath);
    }

    private async Task RunForcedAlignerAsync(string executable, string alignerPath)
    {
        var workFolder = Path.Combine(Path.GetTempPath(), "se-forced-align-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workFolder);
        var audioFileName = Path.Combine(workFolder, "audio.wav");

        try
        {
            IsAligning = true;
            AlignPercent = 0;
            AlignProgress = "Extracting audio...";

            if (!await ExtractAudioForAlignmentAsync(audioFileName))
            {
                await MessageBox.Show(
                    Window!,
                    Se.Language.General.Error,
                    "Could not extract audio from the video file.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var totalSeconds = GetDurationInSeconds(audioFileName);
            if (totalSeconds <= 0)
            {
                await MessageBox.Show(
                    Window!,
                    Se.Language.General.Error,
                    "The extracted audio appears to be empty.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var lines = Subtitles.ToList();
            using var audio = new FfmpegWindowAudioSource(GetFfmpegPath(), audioFileName, totalSeconds, workFolder);
            var runner = new CrispAsrAlignOnlyRunner(executable, alignerPath, Se.WriteToolsLog);
            var forcedAligner = new ForcedAligner(runner, audio);

            var progress = new Progress<ForcedAligner.Progress>(p => Dispatcher.UIThread.Post(() =>
            {
                AlignProgress = string.Format(
                    Se.Language.File.Import.ForcedAlignerProgress,
                    p.WindowIndex, p.WindowCount, p.LinesAligned, p.LinesTotal);
                AlignPercent = p.Percent;
            }));

            var result = await forcedAligner.AlignAsync(lines, progress);

            RefreshSubtitlesFrom(lines);

            if (result.UnalignedLines > 0)
            {
                await MessageBox.Show(
                    Window!,
                    Se.Language.General.Warning,
                    $"Aligned {result.AlignedLines} of {result.TotalLines} lines."
                    + Environment.NewLine
                    + $"{result.UnalignedLines} line(s) at the end kept their previous time codes - the script "
                    + "is probably longer than what is spoken in the video.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (ForcedAlignerException exception)
        {
            await MessageBox.Show(
                Window!,
                Se.Language.General.Error,
                exception.Message,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            IsAligning = false;
            AlignProgress = string.Empty;
            AlignPercent = 0;
            TryDeleteFolder(workFolder);
        }
    }

    private void RefreshSubtitlesFrom(List<SubtitleLineViewModel> lines)
    {
        // The aligner mutates the same view model instances the grid is bound to, but the
        // grid does not observe TimeSpan changes, so re-seat them to force a redraw.
        Subtitles.Clear();
        for (var i = 0; i < lines.Count; i++)
        {
            lines[i].Number = i + 1;
            Subtitles.Add(lines[i]);
        }
    }

    private async Task<bool> ExtractAudioForAlignmentAsync(string audioFileName)
    {
        // 16 kHz mono PCM - what every CTC aligner expects.
        var arguments =
            $"-hide_banner -nostats -y -i \"{_videoFileName}\" -vn -ar 16000 -ac 1 -acodec pcm_s16le \"{audioFileName}\"";

        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo(GetFfmpegPath(), arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            },
        };

        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode == 0 && File.Exists(audioFileName);
    }

    private static double GetDurationInSeconds(string audioFileName)
    {
        try
        {
            var info = FfmpegMediaInfo2.Parse(audioFileName);
            if (info.Duration != null && info.Duration.TotalSeconds > 0)
            {
                return info.Duration.TotalSeconds;
            }
        }
        catch
        {
            // Fall through to the size-based estimate.
        }

        try
        {
            // We extracted this file ourselves as 16 kHz mono 16-bit PCM, so the byte
            // count gives the length directly if ffprobe could not be reached.
            const double bytesPerSecond = 16000 * 2;
            var dataBytes = new FileInfo(audioFileName).Length - 44;
            return dataBytes > 0 ? dataBytes / bytesPerSecond : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string GetFfmpegPath()
    {
        var path = Se.Settings.General.FfmpegPath;
        return File.Exists(path) ? path : "ffmpeg";
    }

    private static void TryDeleteFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
        catch
        {
            // Temp cleanup is best effort.
        }
    }

    [RelayCommand]
    private async Task FileImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, "Choose text file", Se.Language.General.TextFiles, ".txt", Se.Language.General.TextFiles);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var text = await File.ReadAllTextAsync(fileName);
        PlainText = text;
        MarkDirty();
    }

    [RelayCommand]
    private async Task FilesImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(Window, "Choose text files", Se.Language.General.TextFiles, TextFilePatterns, string.Empty, new List<string>());
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames.OrderBy(p => p))
        {
            var fileInfo = new FileInfo(fileName);
            var displayFile = new DisplayFile(fileName, fileInfo.Length);
            Files.Add(displayFile);
        }

        MarkDirty();
    }

    [RelayCommand]
    private void FilesClear()
    {
        Files.Clear();
        MarkDirty();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    public void OnClosingCleanup()
    {
        _timerUpdatePreview.StopAndDispose(TimerUpdatePreviewElapsed);
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/import-plain-text");
        }
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void FileGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLowerInvariant();
                        if (!TextFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var fileInfo = new FileInfo(path);
                        var displayFile = new DisplayFile(path, fileInfo.Length);
                        Files.Add(displayFile);
                    }
                }
                MarkDirty();
            });
        }
    }

    internal void PlainTextChanged()
    {
        MarkDirty();
    }

    internal void SplitAtOptionChanged()
    {
        MarkDirty();
    }

    internal void CheckBoxImportFilesChanged()
    {
        MarkDirty();
    }

    internal void Initialize(Subtitle subtitle, string? videoFileName)
    {
        _videoFileName = videoFileName ?? string.Empty;
    }

    internal void GapChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        MarkDirty();
    }
}
