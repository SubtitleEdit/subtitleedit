using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.BatchConvert;

namespace UITests.Features.Tools.BatchConvert;

/// <summary>
/// ASS to ASS with the batch ASSA header template: the template header is applied when "Use
/// source styles" is off, and the source's embedded fonts (footer) are kept unless "Keep source
/// embedded fonts" is off - replacing the footer outright used to strip them.
/// </summary>
public class BatchConverterAssaHeaderTemplateTests
{
    private const string Template = @"[Script Info]
ScriptType: v4.00+
PlayResX: 640
PlayResY: 360

[V4+ Styles]
Format: Name,Fontname,Fontsize,PrimaryColour,SecondaryColour,OutlineColour,BackColour,Bold,Italic,Underline,Strikeout,ScaleX,ScaleY,Spacing,Angle,BorderStyle,Outline,Shadow,Alignment,MarginL,MarginR,MarginV,Encoding
Style: Default,Trebuchet MS,24,&H00FFFFFF,&H000000FF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,2,1,2,0010,0010,0015,1";

    private const string InputAss = @"[Script Info]
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H0300FFFF,&H00000000,&H02000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Hello there.
";

    private static readonly byte[] SourceFontBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private static readonly byte[] TemplateFontBytes = { 9, 8, 7, 6, 5, 4, 3, 2, 1 };

    private static async Task<string> Convert(bool useSourceStyles, bool keepSourceFonts, string templateFooter = "")
    {
        var dir = Directory.CreateTempSubdirectory("se-assa-header-template-test");
        try
        {
            var inputFile = Path.Combine(dir.FullName, "input.ass");
            var sourceFooter = AssaFontEmbedder.AddFontToFooter(null, "source.ttf", SourceFontBytes);
            await File.WriteAllTextAsync(inputFile, InputAss + Environment.NewLine + sourceFooter, TestContext.Current.CancellationToken);
            var outputFolder = Path.Combine(dir.FullName, "out");
            Directory.CreateDirectory(outputFolder);

            var subtitle = Subtitle.Parse(inputFile);
            Assert.Contains("source.ttf", subtitle.Footer); // the fixture really has an embedded font

            var config = new BatchConvertConfig
            {
                SaveInSourceFolder = false,
                OutputFolder = outputFolder,
                Overwrite = true,
                TargetFormatName = AdvancedSubStationAlpha.NameOfFormat,
                AssaUseSourceStylesIfPossible = useSourceStyles,
                AssaHeader = Template,
                AssaFooter = templateFooter,
                AssaKeepSourceEmbeddedFonts = keepSourceFonts,
            };
            var converter = new BatchConverter(null!, null!, null!);
            converter.Initialize(config);

            var item = new BatchConvertItem(inputFile, new FileInfo(inputFile).Length, new AdvancedSubStationAlpha().Name, subtitle);
            await converter.Convert(item, TestContext.Current.CancellationToken);

            return await File.ReadAllTextAsync(Directory.GetFiles(outputFolder).Single(), TestContext.Current.CancellationToken);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task UseSourceStyles_KeepsSourceHeaderAndFonts()
    {
        var output = await Convert(useSourceStyles: true, keepSourceFonts: false);

        Assert.Contains("Style: Default,Arial,20", output);
        Assert.DoesNotContain("Trebuchet MS", output);
        Assert.Contains("fontname: source.ttf", output);
    }

    [Fact]
    public async Task Template_KeepSourceFonts_AppliesHeaderAndKeepsFonts()
    {
        var output = await Convert(useSourceStyles: false, keepSourceFonts: true);

        Assert.Contains("Trebuchet MS", output);
        Assert.Contains("PlayResX: 640", output);
        Assert.Contains("fontname: source.ttf", output);
        Assert.Contains("Hello there.", output);
    }

    [Fact]
    public async Task Template_DoNotKeepSourceFonts_DropsThem()
    {
        var output = await Convert(useSourceStyles: false, keepSourceFonts: false);

        Assert.Contains("Trebuchet MS", output);
        Assert.DoesNotContain("source.ttf", output);
    }

    [Fact]
    public async Task Template_WithOwnFonts_MergesWithSourceFonts()
    {
        var templateFooter = AssaFontEmbedder.AddFontToFooter(null, "template.ttf", TemplateFontBytes);

        var output = await Convert(useSourceStyles: false, keepSourceFonts: true, templateFooter);

        Assert.Contains("fontname: template.ttf", output);
        Assert.Contains("fontname: source.ttf", output);
    }

    [Fact]
    public void MakeAssaFooter_SameFontName_TemplateWins()
    {
        var template = AssaFontEmbedder.AddFontToFooter(null, "same.ttf", TemplateFontBytes);
        var source = AssaFontEmbedder.AddFontToFooter(null, "same.ttf", SourceFontBytes);

        var footer = BatchConverter.MakeAssaFooter(template, source, keepSourceEmbeddedFonts: true);

        var font = Assert.Single(AssaFontEmbedder.GetEmbeddedFonts(footer));
        Assert.Equal(TemplateFontBytes, font.Bytes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MakeAssaFooter_NoSourceFooter_IsTemplate(bool keep)
    {
        Assert.Equal("T", BatchConverter.MakeAssaFooter("T", "", keep));
    }
}
