using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl.PickOnlineSubtitle;

public partial class PickOnlineSubtitleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<OnlineSubtitleTrackDisplay> _tracks;
    [ObservableProperty] private OnlineSubtitleTrackDisplay? _selectedTrack;
    [ObservableProperty] private ObservableCollection<OnlineSubtitleCueDisplay> _previewRows;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isOkEnabled;

    public Window? Window { get; set; }
    public DataGrid TracksGrid { get; set; }
    public bool OkPressed { get; private set; }
    public string? SelectedSubtitlePath { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private string? _suggestedFileNameBase;

    public PickOnlineSubtitleViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Tracks = new ObservableCollection<OnlineSubtitleTrackDisplay>();
        PreviewRows = new ObservableCollection<OnlineSubtitleCueDisplay>();
        StatusText = string.Empty;
        TracksGrid = new DataGrid();
    }

    /// <summary>
    /// Initializes the picker with pre-downloaded subtitle files. The picker
    /// itself never spawns yt-dlp — it's purely a chooser over what's on disk.
    /// <paramref name="suggestedFileNameBase"/> is the video-derived name (no
    /// extension) used as the save-as suggestion for the "Save..." button; the
    /// downloaded files themselves have useless temp-dir names like "sub.en.vtt".
    /// </summary>
    public void Initialize(IReadOnlyList<DownloadedSubtitleInfo> subtitles, string? suggestedFileNameBase = null)
    {
        _suggestedFileNameBase = suggestedFileNameBase;
        Tracks.Clear();
        PreviewRows.Clear();

        if (subtitles.Count == 0)
        {
            StatusText = Se.Language.Video.PickOnlineSubtitleNoneFound;
            IsOkEnabled = false;
            return;
        }

        foreach (var sub in subtitles)
        {
            Tracks.Add(new OnlineSubtitleTrackDisplay
            {
                Language = FormatLanguage(sub.LanguageCode),
                LanguageCode = sub.LanguageCode,
                Name = string.Empty,
                Format = sub.Format,
                FilePath = sub.FilePath,
            });
        }

        StatusText = string.Empty;
        var initial = Tracks.First();
        SelectedTrack = initial;
        TracksGrid.SelectedItem = initial;
        TracksGrid.ScrollIntoView(initial, null);
    }

    partial void OnSelectedTrackChanged(OnlineSubtitleTrackDisplay? value)
    {
        IsOkEnabled = value != null;
        LoadPreview(value);
    }

    private void LoadPreview(OnlineSubtitleTrackDisplay? track)
    {
        PreviewRows.Clear();
        if (track is null || string.IsNullOrEmpty(track.FilePath))
        {
            return;
        }

        try
        {
            var subtitle = Subtitle.Parse(track.FilePath);
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                StatusText = Se.Language.Video.PickOnlineSubtitleNoneFound;
                return;
            }

            // Skip empty cues — VTT tracks often use text-less paragraphs as
            // timing markers. Cap at the first 200 non-empty cues so the preview
            // stays snappy for very long subtitles.
            const int maxPreview = 200;
            var displayed = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (string.IsNullOrWhiteSpace(p.Text))
                {
                    continue;
                }

                displayed++;
                PreviewRows.Add(new OnlineSubtitleCueDisplay
                {
                    Number = displayed,
                    Show = p.StartTime.TimeSpan,
                    Duration = TimeSpan.FromMilliseconds(Math.Max(0, p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds)),
                    Text = p.Text,
                });

                if (displayed >= maxPreview)
                {
                    break;
                }
            }

            StatusText = displayed == 0 ? Se.Language.Video.PickOnlineSubtitleNoneFound : string.Empty;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Failed to parse subtitle for preview: {track.FilePath}");
            StatusText = ex.Message;
        }
    }

    /// <summary>
    /// Saves the selected track's file to a user-chosen location as-is (a plain
    /// copy, no format conversion). Needed because the downloaded files live in
    /// a temp directory the caller deletes right after the picker closes.
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        var selected = SelectedTrack;
        if (selected is null || string.IsNullOrEmpty(selected.FilePath) || Window is null)
        {
            return;
        }

        var baseName = string.IsNullOrEmpty(_suggestedFileNameBase)
            ? Path.GetFileNameWithoutExtension(selected.FilePath)
            : _suggestedFileNameBase;
        var suggestedFileName = string.IsNullOrEmpty(selected.LanguageCode)
            ? baseName
            : baseName + "." + selected.LanguageCode;

        var fileName = await _fileHelper.PickSaveSubtitleFile(
            Window, "." + selected.Format, suggestedFileName, Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            File.Copy(selected.FilePath, fileName, overwrite: true);
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Failed to save downloaded subtitle to {fileName}");
            await MessageBox.Show(Window, Se.Language.General.Error, ex.Message);
            return;
        }

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
            vm => { vm.Initialize(Se.Language.General.SubtitleFileSaved, string.Format(Se.Language.General.SubtitleFileSavedToX, fileName), fileName, true, true); });
    }

    [RelayCommand]
    private void Ok()
    {
        var selected = SelectedTrack;
        if (selected is null || string.IsNullOrEmpty(selected.FilePath) || Window is null)
        {
            return;
        }

        SelectedSubtitlePath = selected.FilePath;
        OkPressed = true;
        Window.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && IsOkEnabled)
        {
            Ok();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Turns a BCP-47 / ISO language code into a friendly display like
    /// "English (en)". Falls back to just the raw code for tags yt-dlp invents
    /// such as <c>en-orig</c> or <c>live_chat</c>.
    /// </summary>
    private static string FormatLanguage(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        try
        {
            var ci = CultureInfo.GetCultureInfo(code.Replace('_', '-'));
            return $"{ci.DisplayName} ({code})";
        }
        catch (CultureNotFoundException)
        {
            return code;
        }
    }
}
