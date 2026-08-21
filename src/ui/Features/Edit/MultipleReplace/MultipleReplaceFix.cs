using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class MultipleReplaceFix : ObservableObject
{
    // Observable because tick all / untick all / invert set it from the view model (#13502);
    // a plain property leaves the checkboxes showing the old state.
    [ObservableProperty] private bool _apply;
    public int Number { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public List<ReplaceExpression> Hits { get; set; }

    public MultipleReplaceFix()
    {
        Apply = true;
        Before = string.Empty;
        After = string.Empty;
        Hits = new List<ReplaceExpression>();
    }
}
