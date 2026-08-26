using Nikse.SubtitleEdit.Features.Ocr.Engines;

namespace UITests.Features.Ocr;

/// <summary>
/// Turning Vision's observations back into subtitle text.
///
/// Vision hands back a flat set of text observations with no reading order, and it splits one
/// visual line into several observations wherever the gap between words is wide - which is
/// exactly what a two-speaker dialogue line looks like. So the two things that can go wrong are
/// reading the lines in the wrong order and breaking one line into two, and both are decided
/// purely by the geometry, with no framework involved. That makes this testable on any OS, which
/// matters because CI runs on Linux where Vision does not exist.
///
/// Coordinates below are Vision's own: normalized 0-1, origin at the BOTTOM left, so a larger Y
/// is higher up the image.
/// </summary>
public class AppleVisionTextLayoutTests
{
    private static AppleVisionObservation Obs(string text, double left, double right, double top, double bottom)
    {
        return new AppleVisionObservation(text, left, right, top, bottom);
    }

    [Fact]
    public void Compose_NoObservations_IsEmpty()
    {
        Assert.Equal(string.Empty, AppleVisionTextLayout.Compose([]));
    }

    [Fact]
    public void Compose_Null_IsEmpty()
    {
        Assert.Equal(string.Empty, AppleVisionTextLayout.Compose(null!));
    }

    [Fact]
    public void Compose_SingleObservation_IsThatText()
    {
        var text = AppleVisionTextLayout.Compose([Obs("It's a beautiful day.", 0.25, 0.75, 0.81, 0.35)]);

        Assert.Equal("It's a beautiful day.", text);
    }

    [Fact]
    public void Compose_TwoLines_ReadsTopLineFirst()
    {
        // Vision's Y grows upwards, so the upper line is the one with the LARGER Y. Reading them
        // in ascending Y - the intuitive reading for a top-left origin - would swap the lines.
        var upper = Obs("- Are you coming with us?", 0.16, 0.83, 0.90, 0.61);
        var lower = Obs("- No, I'll stay here.", 0.26, 0.74, 0.49, 0.16);

        var text = AppleVisionTextLayout.Compose([upper, lower]);

        Assert.Equal($"- Are you coming with us?{Environment.NewLine}- No, I'll stay here.", text);
    }

    [Fact]
    public void Compose_ObservationsArriveOutOfOrder_StillReadsTopToBottom()
    {
        var upper = Obs("First line", 0.16, 0.83, 0.90, 0.61);
        var middle = Obs("Second line", 0.16, 0.83, 0.58, 0.33);
        var lower = Obs("Third line", 0.26, 0.74, 0.30, 0.05);

        var text = AppleVisionTextLayout.Compose([lower, upper, middle]);

        Assert.Equal($"First line{Environment.NewLine}Second line{Environment.NewLine}Third line", text);
    }

    [Fact]
    public void Compose_OneLineSplitByAWideGap_StaysOneLine()
    {
        // The dialogue case: "- Yes.        - No." on a single visual line comes back as two
        // observations at the same height. Joined with a space, not broken onto two lines.
        var left = Obs("- Yes.", 0.10, 0.30, 0.60, 0.40);
        var right = Obs("- No.", 0.70, 0.90, 0.61, 0.41);

        var text = AppleVisionTextLayout.Compose([left, right]);

        Assert.Equal("- Yes. - No.", text);
    }

    [Fact]
    public void Compose_SplitLine_ReadsLeftToRightWhateverTheOrder()
    {
        var left = Obs("Hello", 0.10, 0.30, 0.60, 0.40);
        var right = Obs("world", 0.70, 0.90, 0.60, 0.40);

        var text = AppleVisionTextLayout.Compose([right, left]);

        Assert.Equal("Hello world", text);
    }

    [Fact]
    public void Compose_TwoSplitLines_GroupsEachLineSeparately()
    {
        var upperLeft = Obs("Upper", 0.10, 0.30, 0.90, 0.70);
        var upperRight = Obs("line", 0.70, 0.90, 0.90, 0.70);
        var lowerLeft = Obs("Lower", 0.10, 0.30, 0.40, 0.20);
        var lowerRight = Obs("line", 0.70, 0.90, 0.40, 0.20);

        var text = AppleVisionTextLayout.Compose([lowerRight, upperLeft, lowerLeft, upperRight]);

        Assert.Equal($"Upper line{Environment.NewLine}Lower line", text);
    }

    [Fact]
    public void Compose_SlightVerticalWobbleOnOneLine_DoesNotSplitIt()
    {
        // Vision leaves a little vertical drift on parts of the same line; anything under half a
        // line height has to stay one line.
        var left = Obs("Left part", 0.10, 0.40, 0.600, 0.400);
        var right = Obs("right part", 0.60, 0.90, 0.615, 0.415);

        var text = AppleVisionTextLayout.Compose([left, right]);

        Assert.Equal("Left part right part", text);
    }

    [Fact]
    public void Compose_LinesCloseTogether_AreStillTwoLines()
    {
        // Tightly set two-liner: the gap is small but the centres are still more than half a
        // line height apart, so it must not collapse into one line.
        var upper = Obs("Upper", 0.10, 0.90, 0.90, 0.62);
        var lower = Obs("Lower", 0.10, 0.90, 0.58, 0.30);

        var text = AppleVisionTextLayout.Compose([upper, lower]);

        Assert.Equal($"Upper{Environment.NewLine}Lower", text);
    }

    [Fact]
    public void Compose_BlankObservations_AreDropped()
    {
        var real = Obs("Real text", 0.10, 0.90, 0.60, 0.40);
        var blank = Obs("   ", 0.10, 0.90, 0.30, 0.10);

        var text = AppleVisionTextLayout.Compose([real, blank]);

        Assert.Equal("Real text", text);
    }

    [Fact]
    public void Compose_TrimsEachObservation()
    {
        var left = Obs("  Hello ", 0.10, 0.30, 0.60, 0.40);
        var right = Obs(" world  ", 0.70, 0.90, 0.60, 0.40);

        Assert.Equal("Hello world", AppleVisionTextLayout.Compose([left, right]));
    }
}
