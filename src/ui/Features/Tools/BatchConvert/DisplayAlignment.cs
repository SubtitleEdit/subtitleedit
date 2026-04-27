using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class DisplayAlignment
{
    public string Code { get; set; }
    public string Name { get; set; }

    public DisplayAlignment(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<DisplayAlignment> GetAll()
    {
        return
        [
            new DisplayAlignment("an1", Se.Language.General.BottomLeft),
            new DisplayAlignment("an2", Se.Language.General.BottomCenter),
            new DisplayAlignment("an3", Se.Language.General.BottomRight),
            new DisplayAlignment("an4", Se.Language.General.MiddleLeft),
            new DisplayAlignment("an5", Se.Language.General.MiddleCenter),
            new DisplayAlignment("an6", Se.Language.General.MiddleRight),
            new DisplayAlignment("an7", Se.Language.General.TopLeft),
            new DisplayAlignment("an8", Se.Language.General.TopCenter),
            new DisplayAlignment("an9", Se.Language.General.TopRight),
        ];
    }
}
