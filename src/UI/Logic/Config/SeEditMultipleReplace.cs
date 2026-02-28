using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEditMultipleReplace
{
    public class MultipleReplaceCategory
    { 
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } 
        public List<MultipleReplaceRule> Rules { get; set; } = new List<MultipleReplaceRule>();
    }

    public List<MultipleReplaceCategory> Categories { get; set; } = new List<MultipleReplaceCategory>();

    public SeEditMultipleReplace()
    {

    }
}
