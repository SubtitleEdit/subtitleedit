using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
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

    public void SetTracks(List<AudioTrackOption> tracks, AudioTrackOption? selected)
    {
        Tracks.Clear();
        Tracks.AddRange(tracks);
        HasMultipleTracks = tracks.Count > 1;
        SelectedTrack = selected ?? (tracks.Count > 0 ? tracks[0] : null);
    }

    partial void OnSelectedTrackChanged(AudioTrackOption? value)
    {
        if (value == null)
        {
            Details = Size;
            return;
        }

        Details = Tracks.Count > 1
            ? $"{Size}  -  {value.DisplayName}"
            : string.IsNullOrWhiteSpace(value.Details) ? Size : $"{Size}  -  {value.Details}";
    }
}
