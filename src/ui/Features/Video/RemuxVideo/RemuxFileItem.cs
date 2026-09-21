using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

/// <summary>
/// One audio or subtitle file in the remux lists. An audio file may carry several tracks
/// (e.g. an MKV/MKA); <see cref="SelectedTrack"/> is the one that ends up in the output.
/// </summary>
public partial class RemuxFileItem : ObservableObject
{
    public string FileName { get; }
    public string Name => Path.GetFileName(FileName);
    public string Size { get; }
    public long SizeBytes { get; }
    public TimeSpan? Duration { get; private set; }
    public string DurationDisplay { get; private set; } = string.Empty;

    public List<AudioTrackOption> Tracks { get; } = new();

    [ObservableProperty] private AudioTrackOption? _selectedTrack;
    [ObservableProperty] private string _details = string.Empty;
    [ObservableProperty] private bool _hasMultipleTracks;

    public RemuxFileItem(string fileName)
    {
        FileName = fileName;
        try
        {
            SizeBytes = new FileInfo(fileName).Length;
            Size = Utilities.FormatBytesToDisplayFileSize(SizeBytes);
        }
        catch
        {
            Size = string.Empty;
        }

        _details = Size;
    }

    public static string FormatDuration(TimeSpan duration)
    {
        var totalHours = (int)duration.TotalHours;
        return totalHours > 0
            ? $"{totalHours}:{duration.Minutes:00}:{duration.Seconds:00}"
            : $"{duration.Minutes:00}:{duration.Seconds:00}";
    }

    public void SetDuration(TimeSpan? duration)
    {
        Duration = duration;
        DurationDisplay = duration.HasValue && duration.Value.TotalMilliseconds > 0
            ? FormatDuration(duration.Value)
            : string.Empty;
        UpdateDetails();
    }

    public void SetTracks(List<AudioTrackOption> tracks, AudioTrackOption? selected)
    {
        Tracks.Clear();
        Tracks.AddRange(tracks);
        HasMultipleTracks = tracks.Count > 1;
        SelectedTrack = selected ?? (tracks.Count > 0 ? tracks[0] : null);
        UpdateDetails();
    }

    partial void OnSelectedTrackChanged(AudioTrackOption? value)
    {
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        var baseInfo = string.IsNullOrEmpty(DurationDisplay)
            ? Size
            : (string.IsNullOrEmpty(Size) ? DurationDisplay : $"{DurationDisplay}  -  {Size}");

        if (SelectedTrack == null)
        {
            Details = baseInfo;
            return;
        }

        Details = Tracks.Count > 1
            ? $"{baseInfo}  -  {SelectedTrack.DisplayName}"
            : string.IsNullOrWhiteSpace(SelectedTrack.Details) ? baseInfo : $"{baseInfo}  -  {SelectedTrack.Details}";
    }

    // A list row or combo box value is announced by ToString() unless its template is a bare
    // text block - without this a screen reader reads the class name (#12087).
    public override string ToString() => Name;
}
