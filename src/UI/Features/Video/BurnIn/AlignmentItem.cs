using Nikse.SubtitleEdit.Logic.Config;
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
        new("7", Se.Language.General.TopLeft),
        new("8", Se.Language.General.TopCenter),
        new("9", Se.Language.General.TopRight),
        new("4", Se.Language.General.MiddleLeft),
        new("5", Se.Language.General.MiddleCenter),
        new("6", Se.Language.General.MiddleRight),
        new("1", Se.Language.General.BottomLeft),
        new("2", Se.Language.General.BottomCenter),
        new("3", Se.Language.General.BottomRight),
    };
}












