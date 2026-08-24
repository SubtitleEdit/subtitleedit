using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace LibUiLogicTests.Export;

/// <summary>
/// Reading the ASSA transparency tags for image export: "{\fad(..)}"/"{\fade(..)}" become the
/// alpha curve the Blu-ray writer fades along, "{\alpha&amp;H..&amp;}" & co. dim the subtitle.
/// </summary>
public class ExportFadeTests
{
    private static ImageParameter Parameter(long durationMs = 2000)
    {
        return new ImageParameter
        {
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromMilliseconds(durationMs),
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            ShadowColor = SKColors.Black,
            ScreenWidth = 1920,
            ScreenHeight = 1080,
        };
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("{\\an8}Hello")]
    [InlineData("{\\pos(10,20)}Hello")]
    public void Parse_NoFadeTag_IsNull(string text)
    {
        Assert.Null(ExportFade.Parse(text, 2000));
    }

    [Fact]
    public void Parse_Fad_RampsFromInvisibleAndBackToInvisible()
    {
        var keyframes = ExportFade.Parse("{\\fad(400,600)}Hello", 2000)!;

        Assert.Equal(0, ExportFade.AlphaPercentAt(keyframes, 0));
        Assert.Equal(50, ExportFade.AlphaPercentAt(keyframes, 200));
        Assert.Equal(100, ExportFade.AlphaPercentAt(keyframes, 400));
        Assert.Equal(100, ExportFade.AlphaPercentAt(keyframes, 1400));
        Assert.Equal(50, ExportFade.AlphaPercentAt(keyframes, 1700));
        Assert.Equal(0, ExportFade.AlphaPercentAt(keyframes, 2000));
    }

    [Fact]
    public void Parse_Fad_InsideATagBlockAndWithoutTheClosingParenthesis()
    {
        // "{\fad(300,300}" is what SE's own effects write.
        Assert.Equal(100, ExportFade.AlphaPercentAt(ExportFade.Parse("{\\an8\\fad(300,300}Hello", 2000)!, 300));
    }

    [Fact]
    public void Parse_Fad_LongerThanTheLine_ShrinksBothRamps()
    {
        // Fading in for 1000 ms and out for 1000 ms on a 500 ms line would never show the text.
        var keyframes = ExportFade.Parse("{\\fad(1000,1000)}Hello", 500)!;

        Assert.Equal(100, ExportFade.AlphaPercentAt(keyframes, 250));
        Assert.Equal(0, ExportFade.AlphaPercentAt(keyframes, 0));
        Assert.Equal(0, ExportFade.AlphaPercentAt(keyframes, 500));
    }

    [Fact]
    public void Parse_Fade_UsesItsOwnAlphasAndTimes()
    {
        // Invisible until 500 ms, fully there from 1000 to 1500 ms, half faded at the end.
        var keyframes = ExportFade.Parse("{\\fade(255,0,128,500,1000,1500,2000)}Hello", 2000)!;

        Assert.Equal(0, ExportFade.AlphaPercentAt(keyframes, 0));
        Assert.Equal(0, ExportFade.AlphaPercentAt(keyframes, 500));
        Assert.Equal(50, ExportFade.AlphaPercentAt(keyframes, 750));
        Assert.Equal(100, ExportFade.AlphaPercentAt(keyframes, 1200));
        Assert.Equal(50, ExportFade.AlphaPercentAt(keyframes, 2000));
    }

    [Fact]
    public void Parse_FadeWinsOverFad()
    {
        // "\fade" starts with the letters of "\fad" - a line with both must not be read as "\fad".
        Assert.Equal(0, ExportFade.AlphaPercentAt(ExportFade.Parse("{\\fade(255,0,255,0,1000,1000,2000)}Hi", 2000)!, 0));
    }

    [Fact]
    public void CreateSteps_SamplesPerFrameAndStartsAtTheCaption()
    {
        var steps = ExportFade.CreateSteps(ExportFade.Parse("{\\fad(400,0)}Hello", 2000), 1000, 3000, 25);

        Assert.Equal(1000, steps[0].TimeMs);
        Assert.Equal(0, steps[0].AlphaPercent);
        Assert.Equal(100, steps[steps.Count - 1].AlphaPercent);
        Assert.Equal(steps.Select(s => s.TimeMs).OrderBy(t => t), steps.Select(s => s.TimeMs));
        Assert.All(steps, s => Assert.InRange(s.TimeMs, 1000, 3000));

        // 400 ms at 25 fps is ten frames, and each of them changes the alpha by 10%.
        Assert.Equal(11, steps.Count);
    }

    [Fact]
    public void CreateSteps_LongFade_SamplesCoarserRatherThanFloodingTheFile()
    {
        var steps = ExportFade.CreateSteps(ExportFade.Parse("{\\fad(30000,30000)}Hello", 60000), 0, 60000, 25);

        Assert.InRange(steps.Count, 2, ExportFade.MaxSteps);
    }

    [Fact]
    public void CreateSteps_NoFade_CostsNothing()
    {
        Assert.Empty(ExportFade.CreateSteps(ExportFade.Parse("{\\fad(0,0)}Hello", 2000), 0, 2000, 25));
        Assert.Empty(ExportFade.CreateSteps(null, 0, 2000, 25));
    }

    [Fact]
    public void ApplyTransparencyTags_Alpha_DimsTheWholeSubtitle()
    {
        var parameter = Parameter();

        ExportTextTags.ApplyTransparencyTags(parameter, "{\\alpha&H80&}Hello");

        Assert.Equal(50, parameter.AlphaPercent);
        Assert.Equal(255, parameter.FontColor.Alpha);
        Assert.Equal(255, parameter.OutlineColor.Alpha);
    }

    [Fact]
    public void ApplyTransparencyTags_PerPartAlpha_GoesOnTheColours()
    {
        var parameter = Parameter();

        ExportTextTags.ApplyTransparencyTags(parameter, "{\\1a&H80&\\3a&HFF&}Hello");

        Assert.Equal(100, parameter.AlphaPercent);
        Assert.Equal(127, parameter.FontColor.Alpha);
        Assert.Equal(0, parameter.OutlineColor.Alpha);
        Assert.Equal(255, parameter.ShadowColor.Alpha);
    }

    [Fact]
    public void ApplyTransparencyTags_NoAlphaTag_ChangesNothing()
    {
        var parameter = Parameter();

        ExportTextTags.ApplyTransparencyTags(parameter, "{\\an8}Hello");

        Assert.Equal(100, parameter.AlphaPercent);
        Assert.Equal(255, parameter.FontColor.Alpha);
        Assert.Null(parameter.FadeKeyframes);
    }
}
