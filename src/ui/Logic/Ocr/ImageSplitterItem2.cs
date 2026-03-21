namespace Nikse.SubtitleEdit.Logic.Ocr;

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
