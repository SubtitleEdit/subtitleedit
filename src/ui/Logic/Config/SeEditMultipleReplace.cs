using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEditMultipleReplace
{
    public class MultipleReplaceCategory
    { 
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsExpanded { get; set; }
        public List<MultipleReplaceRule> Rules { get; set; } = new List<MultipleReplaceRule>();
    }

    public List<MultipleReplaceCategory> Categories { get; set; } = new List<MultipleReplaceCategory>();

    /// <summary>
    /// The category the rules list showed last, so rules can be added to the same one next time.
    /// </summary>
    public string LastCategory { get; set; } = string.Empty;

    /// <summary>
    /// Show the rules of a category last rule first - new rules go to the end, so they come on top.
    /// </summary>
    public bool NewestFirst { get; set; }

    public SeEditMultipleReplace()
    {

    }
}
