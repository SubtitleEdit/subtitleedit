using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Edit.Replace;

public class ReplaceScopeDisplay
{
    public string Name { get; set; }
    public FindScope Scope { get; set; }

    public ReplaceScopeDisplay(string name, FindScope scope)
    {
        Name = name;
        Scope = scope;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<ReplaceScopeDisplay> List()
    {
        return
        [
            new(Se.Language.Edit.Find.ReplaceInTextAndOriginal, FindScope.TextAndOriginal),
            new(Se.Language.Edit.Find.ReplaceInTextOnly, FindScope.TextOnly),
            new(Se.Language.Edit.Find.ReplaceInOriginalOnly, FindScope.OriginalOnly),
        ];
    }
}
