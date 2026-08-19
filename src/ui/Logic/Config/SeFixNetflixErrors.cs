using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFixNetflixErrors
{
    public List<string> SelectedRules { get; set; } = new();
    public bool IsChildrenProgram { get; set; }
    public bool IsSdh { get; set; }
}
