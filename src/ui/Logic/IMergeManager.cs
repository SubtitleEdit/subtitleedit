using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Nikse.SubtitleEdit.Logic.MergeManager;

namespace Nikse.SubtitleEdit.Logic;

public interface IMergeManager
{
    Subtitle MergeSelectedLines(Subtitle subtitle, int[] selectedIndices, MergeManager.BreakMode breakMode = MergeManager.BreakMode.Normal);
    void MergeSelectedLines(ObservableCollection<SubtitleLineViewModel> subtitles, List<SubtitleLineViewModel> subtitleLineViewModels, BreakMode breakMode = BreakMode.Normal);
    void MergeSelectedLinesAsDialog(ObservableCollection<SubtitleLineViewModel> subtitles, List<SubtitleLineViewModel> selectedItems);
}