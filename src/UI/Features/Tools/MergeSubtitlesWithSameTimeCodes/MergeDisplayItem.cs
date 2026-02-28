using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.MergeSubtitlesWithSameTimeCodes;

public partial class MergeDisplayItem : ObservableObject
{
    [ObservableProperty] private bool _apply;
    [ObservableProperty] private string _lines;
    [ObservableProperty] private string _mergedText;
    [ObservableProperty] private string _mergedGroup;

    public List<SubtitleLineViewModel> LinesToMerge { get; set; } = new List<SubtitleLineViewModel>();

    public MergeDisplayItem(bool apply, List<SubtitleLineViewModel> linesToMerge, string mergedText, string mergeGroup)
    {
        Apply = apply;
        Lines = string.Join(", ", linesToMerge.Select(p => p.Number).OrderBy(p => p));
        MergedText = mergedText;
        MergedGroup = mergeGroup;
        LinesToMerge = [.. linesToMerge];
    }
}

