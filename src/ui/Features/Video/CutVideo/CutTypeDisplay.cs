using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Video.CutVideo;

public class CutTypeDisplay
{
    public string Name { get; set; }
    public CutType CutType { get; set; }

    public CutTypeDisplay(string name, CutType cutType)
    {
        Name = name;
        CutType = cutType;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<CutTypeDisplay> GetCutTypes()
    {
        return new List<CutTypeDisplay>
        {
            new(Se.Language.Video.CutVideoCutSegments, CutType.CutSegments),
            new(Se.Language.Video.CutVideoMergeSegments, CutType.MergeSegments),
          //  new CutTypeDisplay(Se.Language.Video.CutVideoSplitSegments, CutType.SplitSegments)
        };
    }
}
