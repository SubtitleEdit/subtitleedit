namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceRule
{
    public string Find { get; set; } 
    public string ReplaceWith { get; set; } 
    public string Description { get; set; } 
    public bool Active { get; set; } = false;
    public MultipleReplaceType Type { get; set; }
    
    public MultipleReplaceRule() 
    { 
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        Type = MultipleReplaceType.CaseInsensitive;
    }
}
