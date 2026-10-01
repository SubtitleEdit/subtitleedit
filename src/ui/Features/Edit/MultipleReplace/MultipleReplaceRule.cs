namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceRule
{
    public string Find { get; set; } 
    public string ReplaceWith { get; set; } 
    public string Description { get; set; } 
    public bool Active { get; set; } = false;
    public MultipleReplaceType Type { get; set; }

    /// <summary>
    /// Only match the find text as a whole word (not inside a longer word) - ignored for
    /// regular expressions, which can use \b themselves (#15510).
    /// </summary>
    public bool WholeWord { get; set; }
    
    public MultipleReplaceRule() 
    { 
        Find = string.Empty;
        ReplaceWith = string.Empty;
        Description = string.Empty;
        Type = MultipleReplaceType.CaseInsensitive;
    }
}
