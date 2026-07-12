using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

public class NOcrLineWalkPointsTests
{
    /// <summary>
    /// WalkPoints/ScaledWalkPoints are allocation-free replacements used by the nOCR matcher's
    /// inner loop - they must yield exactly the same points as the iterator-based
    /// GetPoints/ScaledGetPoints they replace, or match results change.
    /// </summary>
    [Fact]
    public void WalkPoints_YieldsSamePointsAsGetPoints()
    {
        foreach (var line in MakeLines())
        {
            var expected = line.GetPoints().ToList();

            var actual = new List<OcrPoint>();
            foreach (var p in line.WalkPoints())
            {
                actual.Add(p);
            }

            AssertSamePoints(expected, actual, line);
        }
    }

    [Fact]
    public void ScaledWalkPoints_YieldsSamePointsAsScaledGetPoints()
    {
        var ocrChar = new NOcrChar { Width = 24, Height = 40, MarginTop = 3 };
        var sizes = new[] { (24, 40), (11, 17), (48, 80), (7, 60), (60, 7) };

        foreach (var line in MakeLines())
        {
            foreach (var (width, height) in sizes)
            {
                var expected = line.ScaledGetPoints(ocrChar, width, height).ToList();

                var actual = new List<OcrPoint>();
                foreach (var p in line.ScaledWalkPoints(ocrChar, width, height))
                {
                    actual.Add(p);
                }

                AssertSamePoints(expected, actual, line);
            }
        }
    }

    private static void AssertSamePoints(List<OcrPoint> expected, List<OcrPoint> actual, NOcrLine line)
    {
        Assert.True(expected.Count == actual.Count, $"Point count differs for line {line}: {expected.Count} vs {actual.Count}");
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.True(expected[i].X == actual[i].X && expected[i].Y == actual[i].Y,
                $"Point {i} differs for line {line}: ({expected[i].X},{expected[i].Y}) vs ({actual[i].X},{actual[i].Y})");
        }
    }

    private static List<NOcrLine> MakeLines()
    {
        var lines = new List<NOcrLine>
        {
            // Degenerate: single point (factor becomes NaN in both implementations).
            new(new OcrPoint(5, 5), new OcrPoint(5, 5)),
            // Pure horizontal / vertical, both directions.
            new(new OcrPoint(0, 0), new OcrPoint(10, 0)),
            new(new OcrPoint(10, 0), new OcrPoint(0, 0)),
            new(new OcrPoint(3, 2), new OcrPoint(3, 12)),
            new(new OcrPoint(3, 12), new OcrPoint(3, 2)),
            // Diagonals (45 degrees hits the |dx| == |dy| tie -> vertical walk).
            new(new OcrPoint(0, 0), new OcrPoint(9, 9)),
            new(new OcrPoint(9, 9), new OcrPoint(0, 0)),
            new(new OcrPoint(0, 9), new OcrPoint(9, 0)),
            // Shallow and steep slopes, all four directions.
            new(new OcrPoint(1, 2), new OcrPoint(20, 7)),
            new(new OcrPoint(20, 7), new OcrPoint(1, 2)),
            new(new OcrPoint(2, 1), new OcrPoint(7, 20)),
            new(new OcrPoint(7, 20), new OcrPoint(2, 1)),
            new(new OcrPoint(0, 30), new OcrPoint(25, 0)),
        };

        // Deterministic pseudo-random lines to cover odd slopes and rounding edges.
        var random = new Random(42);
        for (var i = 0; i < 500; i++)
        {
            lines.Add(new NOcrLine(
                new OcrPoint(random.Next(-5, 60), random.Next(-5, 90)),
                new OcrPoint(random.Next(-5, 60), random.Next(-5, 90))));
        }

        return lines;
    }
}
