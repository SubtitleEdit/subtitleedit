using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrLineAlgorithmItem
{
    public string Name { get; set; }
    public NOcrLineAlgorithm Value { get; set; }

    public NOcrLineAlgorithmItem(string name, NOcrLineAlgorithm value)
    {
        Name = name;
        Value = value;
    }

    public override string ToString() => Name;

    public static readonly NOcrLineAlgorithmItem[] Items =
    {
        new("Random", NOcrLineAlgorithm.Random),
        new("Skeleton + distance", NOcrLineAlgorithm.SkeletonDistance),
        new("Edge + Hough", NOcrLineAlgorithm.EdgeHough),
    };
}
