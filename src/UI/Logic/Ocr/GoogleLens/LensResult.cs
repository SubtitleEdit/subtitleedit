using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class LensResult
{
    public string Language { get; }
    public List<Segment> Segments { get; }

    public LensResult(string language, List<Segment> segments)
    {
        Language = language;
        Segments = segments;
    }
}
