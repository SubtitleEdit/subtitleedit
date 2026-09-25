using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFilesSaved;

/// <summary>
/// "Done" dialog for a run that produced several files (batch burn-in): the multi-file sibling of
/// PromptFileSaved - one card per file with play / show in folder / copy path, plus a summary.
/// </summary>
public partial class PromptFilesSavedViewModel : ObservableObject
{
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _headline = string.Empty;
    [ObservableProperty] private string _folderSummary = string.Empty;
    [ObservableProperty] private string _totalSizeChip = string.Empty;
    [ObservableProperty] private bool _hasTotalSizeChip;
    [ObservableProperty] private string _elapsedChip = string.Empty;
    [ObservableProperty] private bool _hasElapsedChip;
    [ObservableProperty] private bool _allSucceeded;
    [ObservableProperty] private bool _hasOutputFolder;

    public ObservableCollection<SavedFileItem> Files { get; } = new();

    public Window? Window { get; set; }

    private readonly IFolderHelper _folderHelper;

    // Same list as PromptFileSaved: where "open" means "play" and a duration makes sense.
    private static readonly string[] MediaExtensions =
    {
        ".wav", ".mp3", ".m4a", ".aac", ".flac", ".ogg", ".opus",
        ".mkv", ".mp4", ".mov", ".webm", ".avi", ".m2ts", ".ts", ".mpg", ".mpeg",
    };

    public PromptFilesSavedViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
    }

    internal void Initialize(string title, string headline, IEnumerable<SavedFileItem> files, string? elapsed = null)
    {
        Title = title;
        Headline = headline;

        foreach (var file in files)
        {
            Files.Add(file);
        }

        AllSucceeded = Files.All(p => p.IsSuccess);

        var folders = Files
            .Where(p => p.IsSuccess)
            .Select(p => p.FolderDisplay)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        HasOutputFolder = folders.Count > 0;
        if (folders.Count == 1)
        {
            foreach (var file in Files.Where(p => p.FolderDisplay.Equals(folders[0], StringComparison.OrdinalIgnoreCase)))
            {
                file.ShowFolder = false;
            }
        }
        FolderSummary = folders.Count switch
        {
            0 => string.Empty,
            1 => string.Format(Se.Language.General.SavedInX, folders[0]),
            _ => string.Format(Se.Language.General.SavedInXFolders, folders.Count),
        };

        long totalSize = 0;
        foreach (var file in Files.Where(p => p.IsSuccess))
        {
            try
            {
                if (File.Exists(file.FileName))
                {
                    file.FileSize = new FileInfo(file.FileName).Length;
                    file.FileSizeChip = Utilities.FormatBytesToDisplayFileSize(file.FileSize);
                    file.HasFileSizeChip = true;
                    totalSize += file.FileSize;
                }
            }
            catch
            {
                // chips are decoration - never fail the dialog over them
            }
        }

        if (totalSize > 0)
        {
            TotalSizeChip = Utilities.FormatBytesToDisplayFileSize(totalSize);
            HasTotalSizeChip = true;
        }

        if (!string.IsNullOrWhiteSpace(elapsed))
        {
            ElapsedChip = string.Format(Se.Language.Video.TextToSpeech.XElapsed, elapsed);
            HasElapsedChip = true;
        }

        var mediaFiles = Files
            .Where(p => p.HasFileSizeChip && IsMediaFile(p.FileName))
            .ToList();
        if (mediaFiles.Count > 0)
        {
            _ = ProbeDurationsAsync(mediaFiles);
        }
    }

    public static bool IsMediaFile(string fileName)
    {
        return MediaExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
    }

    private static async Task ProbeDurationsAsync(List<SavedFileItem> items)
    {
        // One ffmpeg probe at a time, off the UI thread; each chip appears as its probe lands.
        foreach (var item in items)
        {
            try
            {
                var info = await Task.Run(() => FfmpegMediaInfo2.Parse(item.FileName));
                if (info.Duration != null && info.Duration.TotalMilliseconds > 0)
                {
                    var duration = info.Duration;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        item.DurationChip = duration.Hours > 0
                            ? $"{duration.Hours}:{duration.Minutes:00}:{duration.Seconds:00}"
                            : $"{duration.Minutes}:{duration.Seconds:00}";
                        item.HasDurationChip = true;
                    });
                }
            }
            catch
            {
                // no ffmpeg / unreadable file - just no duration chip
            }
        }
    }

    [RelayCommand]
    private void PlayFile(SavedFileItem? item)
    {
        if (item == null || !item.IsSuccess)
        {
            return;
        }

        FileHelper.OpenFileWithDefaultProgram(item.FileName);
    }

    [RelayCommand]
    private async Task ShowFileInFolder(SavedFileItem? item)
    {
        if (Window == null || item == null || !item.IsSuccess)
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window, item.FileName);
    }

    [RelayCommand]
    private async Task CopyFilePath(SavedFileItem? item)
    {
        if (Window == null || item == null)
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, item.FileName);
    }

    [RelayCommand]
    private async Task OpenOutputFolder()
    {
        var first = Files.FirstOrDefault(p => p.IsSuccess);
        if (Window == null || first == null)
        {
            return;
        }

        await _folderHelper.OpenFolderWithFileSelected(Window, first.FileName);
    }

    [RelayCommand]
    private void Ok()
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
