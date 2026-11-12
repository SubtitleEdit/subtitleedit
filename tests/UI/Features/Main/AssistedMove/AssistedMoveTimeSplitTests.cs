using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.AssistedMove;

namespace UITests.Features.Main.AssistedMove;

/// <summary>
/// Accepting a suggestion writes its boundary times into the two lines, so they have to be
/// whole milliseconds (#14056) - the proportional split gave e.g. 1733.333 ms, which the
/// format writers truncate.
/// </summary>
public class AssistedMoveTimeSplitTests
{
    [Fact]
    public void Generate_BoundaryTimes_AreWholeMilliseconds()
    {
        var current = new SubtitleLineViewModel
        {
            Text = "I told you that we would go to the",
            StartTime = TimeSpan.FromMilliseconds(1000),
            EndTime = TimeSpan.FromMilliseconds(3000),
        };
        var next = new SubtitleLineViewModel
        {
            Text = "beach tomorrow morning.",
            StartTime = TimeSpan.FromMilliseconds(3100),
            EndTime = TimeSpan.FromMilliseconds(5000),
        };

        var candidates = AssistedMoveCandidateGenerator.Generate(current, null, next, "en")
            .Where(c => c.NewFirstEnd != null && c.NewSecondStart != null)
            .ToList();

        Assert.NotEmpty(candidates);
        foreach (var candidate in candidates)
        {
            var firstEnd = candidate.NewFirstEnd!.Value.TotalMilliseconds;
            var secondStart = candidate.NewSecondStart!.Value.TotalMilliseconds;
            Assert.Equal(Math.Round(firstEnd), firstEnd);
            Assert.Equal(Math.Round(secondStart), secondStart);
            Assert.Equal(100, secondStart - firstEnd); // the gap is kept
        }
    }
}
