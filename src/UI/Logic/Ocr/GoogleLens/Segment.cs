namespace Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

public class Segment
{
    public string Text { get; }
    public BoundingBox BoundingBox { get; }

    public Segment(string text, BoundingBox boundingBox)
    {
        Text = text;
        BoundingBox = boundingBox;
    }
}
