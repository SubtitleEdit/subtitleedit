using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

public partial class DetectSpeakersViewModel : SelectLinesViewModelBase<DetectSpeakersRow>
{
    [ObservableProperty] private bool _stickySpeakers;

    /// <summary>The tags the user confirmed - these names become actors and leave the spoken text.</summary>
    public List<TextSpeakerCandidate> ConfirmedCandidates { get; private set; }

    public DetectSpeakersViewModel()
    {
        ConfirmedCandidates = new List<TextSpeakerCandidate>();
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

    protected override void CollectResult()
    {
        ConfirmedCandidates = Rows.Where(r => r.IsSelected).Select(r => r.Candidate).ToList();
    }
}
