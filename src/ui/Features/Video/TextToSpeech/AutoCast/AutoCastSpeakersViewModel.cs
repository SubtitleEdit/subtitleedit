using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

/// <summary>
/// The speakers diarization found, offered for naming before their voices are cloned.
/// </summary>
/// <remarks>
/// The step exists because diarization is confident but not always right: it splits one person
/// into two speakers when their voice changes, and it names everyone "Speaker 3". Both are cheap
/// to fix here and expensive to fix afterwards - once the voices are cloned and the cast assigned,
/// the only way back is to run the whole thing again.
///
/// Merging is renaming: two rows given the same name are cloned once, from both speakers' lines.
/// </remarks>
public partial class AutoCastSpeakersViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AutoCastSpeakerRow> _rows;
    [ObservableProperty] private ObservableCollection<ITtsEngine> _engines;
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private AutoCastSpeakerRow? _selectedRow;
    [ObservableProperty] private string _summaryText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>The speakers to clone after naming and merging, keyed by the chosen name.</summary>
    public Dictionary<string, List<Paragraph>> SpeakersToClone { get; private set; } = new();

    /// <summary>What each detected speaker ended up being called, for writing the actors back.</summary>
    public Dictionary<string, string> RenamedSpeakers { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public AutoCastSpeakersViewModel()
    {
        _rows = new ObservableCollection<AutoCastSpeakerRow>();
        _engines = new ObservableCollection<ITtsEngine>();
        _summaryText = string.Empty;
    }

    public void Initialize(IReadOnlyList<AutoCastSpeakerRow> rows, IReadOnlyList<ITtsEngine> engines, ITtsEngine? preferredEngine)
    {
        Rows = new ObservableCollection<AutoCastSpeakerRow>(rows);
        Engines = new ObservableCollection<ITtsEngine>(engines);
        SelectedEngine = engines.FirstOrDefault(e => preferredEngine != null && e.GetType() == preferredEngine.GetType())
                         ?? engines.FirstOrDefault();
        SelectedRow = Rows.FirstOrDefault();
        SummaryText = string.Format(
            Se.Language.Video.TextToSpeech.AutoCastFoundXSpeakersInYLines,
            rows.Count,
            rows.Sum(r => r.LineCount));
    }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedEngine == null)
        {
            return;
        }

        // Group by the name the user typed: same name, same voice, cloned from everything both
        // rows say. An emptied name falls back to what diarization called the speaker rather than
        // producing a nameless actor no cast row can point at.
        var byName = new Dictionary<string, List<Paragraph>>(StringComparer.OrdinalIgnoreCase);
        RenamedSpeakers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in Rows)
        {
            var name = string.IsNullOrWhiteSpace(row.Name) ? row.DetectedName : row.Name.Trim();
            RenamedSpeakers[row.DetectedName] = name;

            if (!byName.TryGetValue(name, out var lines))
            {
                lines = new List<Paragraph>();
                byName[name] = lines;
            }

            lines.AddRange(row.Lines);
        }

        SpeakersToClone = byName;
        OkPressed = true;
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
