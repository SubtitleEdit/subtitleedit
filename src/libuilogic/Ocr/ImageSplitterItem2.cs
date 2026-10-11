namespace Nikse.SubtitleEdit.UiLogic.Ocr;

public class ImageSplitterItem2
{
    public int X { get; set; }
    public int Y { get; set; }
    public int ParentY { get; set; }
    public int Top { get; set; }
    public int SpacePixels { get; set; }
    public NikseBitmap2? NikseBitmap { get; set; }
    public string? SpecialCharacter { get; set; }
    public bool CouldBeSpaceBefore { get; set; }

    /// <summary>
    /// Where the line's baseline is, in the same coordinates as <see cref="Top"/> (null when the
    /// splitter didn't work it out). The part of a glyph above it tells "д" from "Д" and "у" from
    /// "У", where the descender makes the whole glyph about as tall as a capital.
    /// </summary>
    public int? Baseline { get; set; }

    /// <summary>The glyph's height above the line's baseline, or its whole height when that's unknown.</summary>
    public int HeightAboveBaseline => Baseline.HasValue && NikseBitmap != null
        ? Math.Min(NikseBitmap.Height, Baseline.Value - Top)
        : NikseBitmap?.Height ?? 0;

    public ImageSplitterItem2(int x, int y, NikseBitmap2 bitmap, bool couldBeSpaceBefore = false)
    {
        X = x;
        Y = y;
        NikseBitmap = bitmap;
        SpecialCharacter = null;
        CouldBeSpaceBefore = couldBeSpaceBefore;
    }

    public ImageSplitterItem2(string specialCharacter)
    {
        X = 0;
        Y = 0;
        SpecialCharacter = specialCharacter;
        NikseBitmap = null;
    }
}
