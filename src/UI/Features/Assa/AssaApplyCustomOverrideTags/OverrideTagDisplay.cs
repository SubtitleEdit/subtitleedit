using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public class OverrideTagDisplay
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public OverrideTagDisplay(string name, string tag)
    {
        Name = name;
        Tag = tag;
    }

    public override string ToString()
    {
        return Name + " - " +Tag;
    }

    public static List<OverrideTagDisplay> List()
    {
        return new List<OverrideTagDisplay>
        {
            new OverrideTagDisplay(Se.Language.Assa.FontSizeChange, "{\\t(\\fs60)}"),
            new OverrideTagDisplay(Se.Language.Assa.MoveTextFromLeftToRight, "{\\move(350,350,1500,350)}"),
            new OverrideTagDisplay(Se.Language.Assa.ColorFromWhiteToRed, "{\\1c&HFFFFFF&\\t(\\1c&H0000FF&)}"),
            new OverrideTagDisplay(Se.Language.Assa.RotateXSlow, "{\\t(\\frx25)}"),
            new OverrideTagDisplay(Se.Language.Assa.RotateX, "{\\t(\\frx360)}"),
            new OverrideTagDisplay(Se.Language.Assa.RotateY, "{\\t(\\fry360)}"),
            new OverrideTagDisplay(Se.Language.Assa.RotateTilt, "{\\t(\\fr5\\fr0)}"),
            new OverrideTagDisplay(Se.Language.General.Fade, "{\\fad(300,300}"),
            new OverrideTagDisplay(Se.Language.Assa.SpaceIncrease, "{\\t(\\fsp4)}"),
        };
    }   
}
