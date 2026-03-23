using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class AlignmentItem
{
    public string Code { get; set; }
    public string Name { get; set; }

    public AlignmentItem(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            return Name;
        }

        return $"{Name}";
    }

    public static List<AlignmentItem> Alignments = new()
    {
        new("7", "Top-left"),
        new("8", "Top-center"),
        new("9", "Top-right"),
        new("4", "Middle-left"),
        new("5", "Middle-center"),
        new("6", "Middle-right"),
        new("1", "Bottom-left"),
        new("2", "Bottom-center"),
        new("3", "Bottom-right"),
    };
}












