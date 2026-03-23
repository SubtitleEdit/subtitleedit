using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceFix
{
    public bool Apply { get; set; }
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
