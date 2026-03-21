namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class MultiSelectItem
{
    public bool Apply { get; set; }
    public string Name { get; set; }

    public MultiSelectItem(string name)
    {
        Name = name;
    }
}
