using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

/// <summary>
/// One speaker diarization found in the video, as shown in the speakers dialog before cloning.
/// </summary>
public partial class AutoCastSpeakerRow : ObservableObject
{
    /// <summary>What diarization called this speaker ("Speaker 1") - the key its lines carry.</summary>
    public string DetectedName { get; }

    /// <summary>The name the user gives the voice. Two rows sharing one name become one voice.</summary>
    [ObservableProperty] private string _name;

    public IReadOnlyList<Paragraph> Lines { get; }

    public int LineCount => Lines.Count;

    /// <summary>How much of this speaker there is to clone from, over all their lines.</summary>
    public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(Lines.Sum(p => p.Duration.TotalMilliseconds));

    /// <summary>The duration column: mm:ss is enough, and dialogue is rarely hours per speaker.</summary>
    public string TotalDurationDisplay => $"{(int)TotalDuration.TotalMinutes:00}:{TotalDuration.Seconds:00}";

    /// <summary>A line of theirs, so the user can tell who this speaker is without playing anything.</summary>
    public string SampleText { get; }

    public AutoCastSpeakerRow(string detectedName, IReadOnlyList<Paragraph> lines)
    {
        DetectedName = detectedName;
        _name = detectedName;
        Lines = lines;

        // The longest line: the most likely to be a recognisable, complete sentence.
        var longest = lines.OrderByDescending(p => p.Duration.TotalMilliseconds).FirstOrDefault();
        var text = Utilities.UnbreakLine(HtmlUtil.RemoveHtmlTags(longest?.Text ?? string.Empty, alsoSsaTags: true));
        SampleText = text.Length > 90 ? text[..90] + "…" : text;
    }
}
