namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class MultipleReplaceTypeItem
{
    public string Name { get; set; }
    public MultipleReplaceType Type { get; set; }
    
    public MultipleReplaceTypeItem(string name, MultipleReplaceType type)
    {
        Name = name;
        Type = type;
    }
    
    public override string ToString()
    {
        return Name;
    }
}