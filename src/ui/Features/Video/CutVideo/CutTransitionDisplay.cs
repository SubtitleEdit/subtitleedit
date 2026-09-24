using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

/// <summary>
/// An ffmpeg xfade transition for joining the parts of a cut video.
/// </summary>
public class CutTransitionDisplay
{
    public string Name { get; set; }

    /// <summary>The xfade "transition" value.</summary>
    public string Code { get; set; }

    public CutTransitionDisplay(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<CutTransitionDisplay> GetTransitions()
    {
        var l = Se.Language.Video;
        return new List<CutTransitionDisplay>
        {
            new(l.CutVideoTransitionFade, "fade"),
            new(l.CutVideoTransitionFadeBlack, "fadeblack"),
            new(l.CutVideoTransitionFadeWhite, "fadewhite"),
            new(l.CutVideoTransitionFadeGrays, "fadegrays"),
            new(l.CutVideoTransitionDissolve, "dissolve"),
            new(l.CutVideoTransitionWipeLeft, "wipeleft"),
            new(l.CutVideoTransitionWipeRight, "wiperight"),
            new(l.CutVideoTransitionWipeUp, "wipeup"),
            new(l.CutVideoTransitionWipeDown, "wipedown"),
            new(l.CutVideoTransitionSlideLeft, "slideleft"),
            new(l.CutVideoTransitionSlideRight, "slideright"),
            new(l.CutVideoTransitionSlideUp, "slideup"),
            new(l.CutVideoTransitionSlideDown, "slidedown"),
            new(l.CutVideoTransitionSmoothLeft, "smoothleft"),
            new(l.CutVideoTransitionSmoothRight, "smoothright"),
            new(l.CutVideoTransitionCircleOpen, "circleopen"),
            new(l.CutVideoTransitionCircleClose, "circleclose"),
            new(l.CutVideoTransitionRadial, "radial"),
            new(l.CutVideoTransitionPixelize, "pixelize"),
            new(l.CutVideoTransitionBlur, "hblur"),
        };
    }
}
