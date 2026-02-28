namespace Nikse.SubtitleEdit.Logic.Ocr;

public struct OcrPoint
{
    public int X { get; set; }
    public int Y { get; set; }

    public OcrPoint(int x, int y)
    {
        X = x;
        Y = y;
    }
}