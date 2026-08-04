using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace LibUiLogicTests.Export;

public class CustomTextFormatterTests
{
    // 00:01:01,160 - the example from https://github.com/SubtitleEdit/subtitleedit/discussions/12978
    private static readonly TimeCode Tc = new TimeCode(61160);

    [Theory]
    [InlineData("hh:mm:ss,zzz", "00:01:01,160")]
    [InlineData("hh:mm:ss.zzz", "00:01:01.160")]
    [InlineData("h:mm:ss.zz", "0:01:01.16")]
    [InlineData("mm:ss", "01:01")]
    public void GetTimeCode_ClockTemplates_SliceComponents(string template, string expected)
    {
        Assert.Equal(expected, CustomTextFormatter.GetTimeCode(Tc, template));
    }

    [Theory]
    [InlineData("ss.zzz", "61.160")]
    [InlineData("ss.zzzzzz", "61.160000")]
    [InlineData("ss,zzz", "61,160")]
    [InlineData("ss", "61")]
    [InlineData("sss", "061")]
    [InlineData("zzz", "61160")]
    public void GetTimeCode_LeadingSecondsOrMillisecondsRun_MeansTotal(string template, string expected)
    {
        Assert.Equal(expected, CustomTextFormatter.GetTimeCode(Tc, template));
    }

    [Fact]
    public void GetTimeCode_TotalSecondsWithFraction_DoesNotRoundUpSeconds()
    {
        Assert.Equal("61.960", CustomTextFormatter.GetTimeCode(new TimeCode(61960), "ss.zzz"));
    }

    [Fact]
    public void GetTimeCode_TotalSecondsWithoutFraction_Rounds()
    {
        Assert.Equal("62", CustomTextFormatter.GetTimeCode(new TimeCode(61960), "ss"));
        Assert.Equal("06", CustomTextFormatter.GetTimeCode(new TimeCode(5900), "ss"));
    }

    [Fact]
    public void GetTimeCode_TotalMilliseconds_PadsToRunLength()
    {
        Assert.Equal("050", CustomTextFormatter.GetTimeCode(new TimeCode(50), "zzz"));
        Assert.Equal("500", CustomTextFormatter.GetTimeCode(new TimeCode(500), "zzz"));
    }

    [Fact]
    public void GetTimeCode_Negative_PrependsSign()
    {
        Assert.Equal("-01.500", CustomTextFormatter.GetTimeCode(new TimeCode(-1500), "ss.zzz"));
    }

    [Fact]
    public void GetTimeCode_Frames()
    {
        var old = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            Configuration.Settings.General.CurrentFrameRate = 25;

            // mixed template: frames within the second
            Assert.Equal("00:01:01:04", CustomTextFormatter.GetTimeCode(Tc, "hh:mm:ss:ff"));

            // whole template "ff": total frames
            Assert.Equal("1529", CustomTextFormatter.GetTimeCode(Tc, "ff"));
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = old;
        }
    }

    [Fact]
    public void GenerateCustomText_TotalSecondsTemplate_MakesAudacityLabels()
    {
        var template = new CustomFormatTemplate
        {
            FormatHeader = string.Empty,
            FormatParagraph = "{start}\t{end}\t{text}\n",
            FormatTimeCode = "ss.zzzzzz",
            FormatNewLine = string.Empty,
            FormatFooter = string.Empty,
        };
        var paragraphs = new List<Paragraph>
        {
            new Paragraph("Every scan updates the entire supply chain.", 57921, 61160),
        };

        var result = CustomTextFormatter.GenerateCustomText(template, paragraphs, "title", string.Empty);

        Assert.Equal("57.921000\t61.160000\tEvery scan updates the entire supply chain.\n", result);
    }

    [Fact]
    public void GenerateCustomText_TotalSecondsTemplate_DurationIsTotalToo()
    {
        var template = new CustomFormatTemplate
        {
            FormatHeader = string.Empty,
            FormatParagraph = "{duration}\n",
            FormatTimeCode = "ss.zzz",
            FormatNewLine = string.Empty,
            FormatFooter = string.Empty,
        };
        var paragraphs = new List<Paragraph>
        {
            new Paragraph("Hi", 0, 75500), // 75.5 seconds - more than a minute
        };

        var result = CustomTextFormatter.GenerateCustomText(template, paragraphs, "title", string.Empty);

        Assert.Equal("75.500\n", result);
    }

    [Fact]
    public void GenerateCustomText_ClockTemplate_DurationIsShortened()
    {
        var template = new CustomFormatTemplate
        {
            FormatHeader = string.Empty,
            FormatParagraph = "{duration}\n",
            FormatTimeCode = "hh:mm:ss,zzz",
            FormatNewLine = string.Empty,
            FormatFooter = string.Empty,
        };
        var paragraphs = new List<Paragraph>
        {
            new Paragraph("Hi", 0, 75500),
        };

        var result = CustomTextFormatter.GenerateCustomText(template, paragraphs, "title", string.Empty);

        Assert.Equal("01:15,500\n", result);
    }
}
