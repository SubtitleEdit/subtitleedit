using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;

public partial class PromptFileSavedViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _isShowInFolderVisible;
    [ObservableProperty] private bool _isShowFileVisible;
    [ObservableProperty] private string _fileNameDisplay;
    [ObservableProperty] private string _folderDisplay;
    [ObservableProperty] private string _extensionChip;
    [ObservableProperty] private string _fileSizeChip;
    [ObservableProperty] private string _durationChip;
    [ObservableProperty] private bool _hasExtensionChip;
    [ObservableProperty] private bool _hasFileSizeChip;
    [ObservableProperty] private bool _hasDurationChip;

    public Window? Window { get; set; }

    /// <summary>"Play" for audio/video files, "Open file" otherwise - same command either way.</summary>
    public string OpenFileButtonText { get; private set; }
    public bool IsMediaFile { get; private set; }

    private string _fileName;
    private readonly IFileHelper _fileHelper;
    private readonly IFolderHelper _folderHelper;

    // Extensions where "Open file" means "play it" and a duration chip makes sense.
    private static readonly string[] MediaExtensions =
    {
        ".wav", ".mp3", ".m4a", ".aac", ".flac", ".ogg", ".opus",
        ".mkv", ".mp4", ".mov", ".webm", ".avi", ".m2ts", ".ts", ".mpg", ".mpeg",
    };

    public PromptFileSavedViewModel(IFileHelper fileHelper, IFolderHelper folderHelper)
    {
        _fileHelper = fileHelper;
        _folderHelper = folderHelper;

        _fileName = string.Empty;
        Title = string.Empty;
        Text = string.Empty;
        FileNameDisplay = string.Empty;
        FolderDisplay = string.Empty;
        ExtensionChip = string.Empty;
        FileSizeChip = string.Empty;
        DurationChip = string.Empty;
        OpenFileButtonText = Se.Language.General.OpenFile;
    }

    /// <summary>
    /// <paramref name="text"/> is kept for call-site compatibility but no longer rendered: every
    /// caller passes a "saved to path" sentence whose content the window now shows structured -
    /// headline (<paramref name="title"/>), file name, folder and meta chips.
    /// </summary>
    internal void Initialize(string title, string text, string fileName, bool isShowInFolderVisible, bool isShowFileVisible)
    {
        Title = title;
        Text = text;
        _fileName = fileName;
        IsShowInFolderVisible = isShowInFolderVisible;
        IsShowFileVisible = isShowFileVisible;

        FileNameDisplay = Path.GetFileName(fileName);
        var folder = Path.GetDirectoryName(fileName);
        FolderDisplay = string.IsNullOrEmpty(folder)
            ? string.Empty
            : folder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        var extension = Path.GetExtension(fileName);
        HasExtensionChip = !string.IsNullOrEmpty(extension);
        ExtensionChip = extension.TrimStart('.').ToUpperInvariant();
        IsMediaFile = MediaExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        OpenFileButtonText = IsMediaFile ? Se.Language.General.Play : Se.Language.General.OpenFile;

        try
        {
            if (File.Exists(fileName))
            {
                FileSizeChip = Utilities.FormatBytesToDisplayFileSize(new FileInfo(fileName).Length);
                HasFileSizeChip = true;
            }
        }
        catch
        {
            // size/duration chips are decoration - never fail the dialog over them
        }

        if (IsMediaFile && HasFileSizeChip)
        {
            // ffmpeg probe off the UI thread; the chip appears when (and if) the probe lands.
            _ = ProbeDurationAsync(fileName);
        }
    }

    private async Task ProbeDurationAsync(string fileName)
    {
        try
        {
            var info = await Task.Run(() => FfmpegMediaInfo2.Parse(fileName));
            if (info.Duration != null && info.Duration.TotalMilliseconds > 0)
            {
                var duration = info.Duration;
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    DurationChip = duration.Hours > 0
                        ? $"{duration.Hours}:{duration.Minutes:00}:{duration.Seconds:00}"
                        : $"{duration.Minutes}:{duration.Seconds:00}";
                    HasDurationChip = true;
                });
            }
        }
        catch
        {
            // no ffmpeg / unreadable file - just no duration chip
        }
    }

    [RelayCommand]
    private async Task ShowInFolder()
    {
        if (Window == null)
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window, _fileName);
    }

    [RelayCommand]
    private void ShowFile()
    {
        if (Window == null)
        {
            return;
        }

        FileHelper.OpenFileWithDefaultProgram(_fileName);
    }

    [RelayCommand]
    private async Task CopyPath()
    {
        if (Window != null && !string.IsNullOrEmpty(_fileName))
        {
            await ClipboardHelper.SetTextAsync(Window, _fileName);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
