using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceGroup
{
    public string Name { get; set; } 
    public List<MultipleReplaceRule> Rules { get; set; } 
    public bool Active { get; set; } 
    
    public MultipleReplaceGroup() 
    { 
        Name = string.Empty;
        Rules = new List<MultipleReplaceRule>();
        Active = true;
    }
}
