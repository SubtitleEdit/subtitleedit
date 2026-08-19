using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public partial class WriteChaptersToVideoViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private string _inputFileNameDisplay = string.Empty;
    [ObservableProperty] private string _outputFileName = string.Empty;
    [ObservableProperty] private string _chapterCountDisplay = "0";
    [ObservableProperty] private bool _isWriting;
    [ObservableProperty] private string _progressText = string.Empty;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly System.Timers.Timer _timer;
    private readonly StringBuilder _log = new();

    private string _videoFileName = string.Empty;
    private List<Chapter> _chapters = new();
    private Process? _ffmpegProcess;
    private string _metadataFileName = string.Empty;

    public WriteChaptersToVideoViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        _timer = new System.Timers.Timer(200);
        _timer.Elapsed += TimerElapsed;
    }

    public void Initialize(string videoFileName, List<Chapter> chapters)
    {
        _videoFileName = videoFileName;
        _chapters = chapters;

        InputFileNameDisplay = Path.GetFileName(videoFileName);
        ChapterCountDisplay = chapters.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        OutputFileName = GetSuggestedOutputFileName(videoFileName);
    }

    /// <summary>
    /// ffmpeg cannot write its output over the file it is reading, so the suggestion is a sibling
    /// name that does not exist yet.
    /// </summary>
    internal static string GetSuggestedOutputFileName(string videoFileName)
    {
        var directory = Path.GetDirectoryName(videoFileName) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(videoFileName);
        var extension = Path.GetExtension(videoFileName);

        var candidate = Path.Combine(directory, $"{name}_chapters{extension}");
        var counter = 2;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(directory, $"{name}_chapters_{counter}{extension}");
            counter++;
        }

        return candidate;
    }

    [RelayCommand]
    private async Task BrowseOutputFileName()
    {
        if (Window == null)
        {
            return;
        }

        var extension = Path.GetExtension(_videoFileName);
        var fileName = await _fileHelper.PickSaveFile(
            Window,
            extension,
            Path.GetFileName(OutputFileName),
            Se.Language.Video.Chapters.OutputFileName);

        if (!string.IsNullOrEmpty(fileName))
        {
            OutputFileName = fileName;
        }
    }

    [RelayCommand]
    private async Task Write()
    {
        if (Window == null || IsWriting || string.IsNullOrEmpty(OutputFileName))
        {
            return;
        }

        if (string.Equals(Path.GetFullPath(OutputFileName), Path.GetFullPath(_videoFileName), StringComparison.OrdinalIgnoreCase))
        {
            // ffmpeg reads the input while writing the output, so the two cannot be the same file.
            await MessageBox.Show(
                Window,
                Se.Language.Video.Chapters.WriteToVideoTitle,
                Se.Language.Video.Chapters.UnableToWriteChapters,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            _metadataFileName = Path.Combine(Path.GetTempPath(), $"se_chapters_{Guid.NewGuid():N}.ffmeta");
            await File.WriteAllTextAsync(
                _metadataFileName,
                FfmpegMetadataChapters.ToFfmpegMetadata(_chapters),
                new UTF8Encoding(false));
        }
        catch (Exception exception)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, exception.Message);
            return;
        }

        var arguments = FfmpegGenerator.GetWriteChaptersParameters(_videoFileName, _metadataFileName, OutputFileName);
        _log.AppendLine($"FFmpeg command: {arguments}");

        IsWriting = true;
        ProgressText = Se.Language.Video.Chapters.Writing;

        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416
        _ffmpegProcess.Start();
#pragma warning restore CA1416
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();
        _timer.Start();
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null || !_ffmpegProcess.HasExited)
        {
            return;
        }

        _timer.Stop();
        DeleteMetadataFile();

        var exitCode = _ffmpegProcess.ExitCode;
        var outputExists = File.Exists(OutputFileName);

        Dispatcher.UIThread.Invoke(async () =>
        {
            IsWriting = false;
            ProgressText = string.Empty;

            if (exitCode != 0 || !outputExists)
            {
                SeLogger.Error(
                    "Unable to write chapters to video: " + OutputFileName + Environment.NewLine +
                    "Parameters: " + _ffmpegProcess.StartInfo.Arguments + Environment.NewLine +
                    "ffmpeg exit code: " + exitCode + Environment.NewLine +
                    "ffmpeg log: " + _log);

                await MessageBox.Show(
                    Window!,
                    Se.Language.Video.Chapters.WriteToVideoTitle,
                    Se.Language.Video.Chapters.UnableToWriteChapters,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            OkPressed = true;
            Window?.Close();
        });
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrWhiteSpace(outLine.Data))
        {
            _log.AppendLine(outLine.Data);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OkPressed = false;
        Window?.Close();
    }

    private void DeleteMetadataFile()
    {
        try
        {
            if (!string.IsNullOrEmpty(_metadataFileName) && File.Exists(_metadataFileName))
            {
                File.Delete(_metadataFileName);
            }
        }
        catch
        {
            // A leftover temp file is not worth bothering the user about.
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && !IsWriting)
        {
            e.Handled = true;
            Cancel();
        }
    }

    public void OnClosingCleanup()
    {
        _timer.Stop();
        _timer.Dispose();

        try
        {
            if (_ffmpegProcess is { HasExited: false })
            {
#pragma warning disable CA1416
                _ffmpegProcess.Kill(true);
#pragma warning restore CA1416
            }
        }
        catch
        {
            // Already gone.
        }

        DeleteMetadataFile();
    }
}
