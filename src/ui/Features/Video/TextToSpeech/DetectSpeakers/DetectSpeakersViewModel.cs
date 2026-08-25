using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public partial class DetectSpeakersViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<DetectSpeakersRow> _rows;
    [ObservableProperty] private DetectSpeakersRow? _selectedRow;
    [ObservableProperty] private string _rowsInfo;
    [ObservableProperty] private bool _stickySpeakers;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The tags the user confirmed - these names become actors and leave the spoken text.</summary>
    public List<TextSpeakerCandidate> ConfirmedCandidates { get; private set; }

    public DetectSpeakersViewModel()
    {
        Rows = new ObservableCollection<DetectSpeakersRow>();
        ConfirmedCandidates = new List<TextSpeakerCandidate>();
        RowsInfo = string.Empty;
        // The SDH convention names only the speaker changes, so the lines between two tags belong
        // to the tag above them - which is exactly what makes this usable on a real SDH file.
        StickySpeakers = true;
    }

    public void Initialize(List<TextSpeakerCandidate> candidates)
    {
        Rows.Clear();
        foreach (var candidate in candidates)
        {
            Rows.Add(new DetectSpeakersRow(candidate));
        }

        var speakerCount = candidates.Select(c => c.Speaker).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        RowsInfo = string.Format(Se.Language.Video.TextToSpeech.DetectSpeakersFoundX, Rows.Count, speakerCount);
    }

    [RelayCommand]
    private void Ok()
    {
        ConfirmedCandidates = Rows.Where(r => r.IsSelected).Select(r => r.Candidate).ToList();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = true;
        }
    }

    [RelayCommand]
    private void InverseSelection()
    {
        foreach (var row in Rows)
        {
            row.IsSelected = !row.IsSelected;
        }
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
