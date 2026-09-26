using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;

namespace UITests.Features.Assa;

/// <summary>
/// Style files made by "Export..." in the ASSA/SSA styles window contain styles but no dialogue
/// lines, so they are not recognized as subtitle files - importing them used to fail with
/// "Nothing to import".
/// </summary>
public class StyleFileImportHelperTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string WriteTempFile(string extension, string content)
    {
        var fileName = Path.Combine(Path.GetTempPath(), "se-style-import-test-" + Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(fileName, content);
        _tempFiles.Add(fileName);
        return fileName;
    }

    // As written by SE 4's SubStationAlphaStylesCategoriesImportExport.WriteTemplate, on a
    // machine with a comma decimal separator; "White" is how System.Drawing wrote named colors.
    private const string Se4Template = """
<?xml version="1.0"?>
<Settings>
  <AssaStorageCategories>
    <Category>
      <Name>Default</Name>
      <CategoryIsDefault>True</CategoryIsDefault>
      <Style>
        <Name>Default</Name>
        <FontName>Tahoma</FontName>
        <FontSize>20,5</FontSize>
        <PrimaryColor>White</PrimaryColor>
        <SecondaryColor>#FFFF00</SecondaryColor>
        <OutlineColor>#102030</OutlineColor>
        <BackgroundColor>Black</BackgroundColor>
        <ShadowWidth>1,5</ShadowWidth>
        <OutlineWidth>3</OutlineWidth>
        <Alignment>8</Alignment>
        <MarginLeft>11</MarginLeft>
        <MarginRight>12</MarginRight>
        <MarginVertical>13</MarginVertical>
        <BorderStyle>3</BorderStyle>
      </Style>
    </Category>
    <Category>
      <Name>Anime</Name>
      <CategoryIsDefault>False</CategoryIsDefault>
      <Style>
        <Name>Default</Name>
        <FontName>Arial</FontName>
        <FontSize>30</FontSize>
        <PrimaryColor>#FF0000</PrimaryColor>
      </Style>
      <Style>
        <Name>Signs</Name>
        <FontName>Verdana</FontName>
        <FontSize>24</FontSize>
      </Style>
    </Category>
  </AssaStorageCategories>
</Settings>
""";

    /// <summary>
    /// SE 4 exported its style categories as "my_assa_categories.template" XML (#15332).
    /// </summary>
    [Fact]
    public void LoadSe4CategoriesTemplate_ReadsCategoriesAndStyles()
    {
        var fileName = WriteTempFile(".template", Se4Template);

        Assert.True(StyleFileImportHelper.IsSe4CategoriesTemplate(fileName));
        var categories = StyleFileImportHelper.LoadSe4CategoriesTemplate(fileName);

        Assert.Equal(new[] { "Default", "Anime" }, categories.Select(c => c.Name));
        Assert.True(categories[0].IsDefault);
        Assert.False(categories[1].IsDefault);

        var style = Assert.Single(categories[0].Styles);
        Assert.Equal("Default", style.Name);
        Assert.Equal("Tahoma", style.FontName);
        Assert.Equal(20.5m, style.FontSize);
        Assert.Equal(new SkiaSharp.SKColor(255, 255, 255), style.Primary);
        Assert.Equal(new SkiaSharp.SKColor(255, 255, 0), style.Secondary);
        Assert.Equal(new SkiaSharp.SKColor(0x10, 0x20, 0x30), style.Outline);
        Assert.Equal(new SkiaSharp.SKColor(0, 0, 0), style.Background);
        Assert.Equal(1.5m, style.ShadowWidth);
        Assert.Equal(3m, style.OutlineWidth);
        Assert.Equal("8", style.Alignment);
        Assert.Equal(11, style.MarginLeft);
        Assert.Equal(12, style.MarginRight);
        Assert.Equal(13, style.MarginVertical);
        Assert.Equal("3", style.BorderStyle);

        Assert.Equal(new[] { "Default", "Signs" }, categories[1].Styles.Select(p => p.Name));
        Assert.Equal(new SkiaSharp.SKColor(255, 0, 0), categories[1].Styles[0].Primary);
        Assert.Equal("2", categories[1].Styles[1].Alignment); // missing values keep the defaults
    }

    [Fact]
    public void LoadSe4CategoriesTemplate_InvalidFile_ReturnsEmpty()
    {
        var fileName = WriteTempFile(".template", "not xml");

        Assert.Empty(StyleFileImportHelper.LoadSe4CategoriesTemplate(fileName));
    }

    private static List<SsaStyle> MakeStyles()
    {
        return new List<SsaStyle>
        {
            new SsaStyle { Name = "Default", FontName = "Arial", FontSize = 20 },
            new SsaStyle { Name = "Narrator", FontName = "Verdana", FontSize = 24, Italic = true },
        };
    }

    /// <summary>
    /// Same as AssaStylesViewModel.FileExport/StorageExport.
    /// </summary>
    private static string ExportAssStyles(List<SsaStyle> styles)
    {
        var subtitle = new Subtitle
        {
            Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, styles)
        };
        return subtitle.ToText(new AdvancedSubStationAlpha());
    }

    /// <summary>
    /// Same as SsaStylesViewModel.FileExport/StorageExport.
    /// </summary>
    private static string ExportSsaStyles(List<SsaStyle> styles)
    {
        var subtitle = new Subtitle
        {
            Header = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
                AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, styles),
                string.Empty)
        };
        return subtitle.ToText(new SubStationAlpha());
    }

    [Fact]
    public void LoadStyles_ImportsExportedAssStyleFile()
    {
        var fileName = WriteTempFile(".ass", ExportAssStyles(MakeStyles()));

        var styles = StyleFileImportHelper.LoadStyles(fileName, new AdvancedSubStationAlpha());

        Assert.Equal(new[] { "Default", "Narrator" }, styles.Select(p => p.Name).ToArray());
        Assert.Equal("Verdana", styles[1].FontName);
        Assert.Equal(24, styles[1].FontSize);
        Assert.True(styles[1].Italic);
    }

    [Fact]
    public void LoadStyles_ImportsExportedSsaStyleFile()
    {
        var fileName = WriteTempFile(".ssa", ExportSsaStyles(MakeStyles()));

        var styles = StyleFileImportHelper.LoadStyles(fileName, new SubStationAlpha());

        Assert.Equal(new[] { "Default", "Narrator" }, styles.Select(p => p.Name).ToArray());
        Assert.Equal("Verdana", styles[1].FontName);
        Assert.True(styles[1].Italic);
    }

    [Fact]
    public void LoadStyles_ImportsFromNormalSubtitleFile()
    {
        var subtitle = new Subtitle
        {
            Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, MakeStyles())
        };
        subtitle.Paragraphs.Add(new Paragraph("Hello", 1000, 2000) { Extra = "Narrator" });
        var fileName = WriteTempFile(".ass", subtitle.ToText(new AdvancedSubStationAlpha()));

        var styles = StyleFileImportHelper.LoadStyles(fileName, new AdvancedSubStationAlpha());

        Assert.Equal(new[] { "Default", "Narrator" }, styles.Select(p => p.Name).ToArray());
    }

    [Fact]
    public void LoadStyles_ImportsAegisubStyFile()
    {
        var fileName = WriteTempFile(".sty",
            "Style: FromSty,Tahoma,30,&H00FFFFFF,&H0000FFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,1,1,2,10,10,10,1" + Environment.NewLine);

        var styles = StyleFileImportHelper.LoadStyles(fileName, new AdvancedSubStationAlpha());

        var style = Assert.Single(styles);
        Assert.Equal("FromSty", style.Name);
        Assert.Equal("Tahoma", style.FontName);
    }

    [Fact]
    public void LoadStyles_ReturnsEmptyForFileWithoutStyles()
    {
        var fileName = WriteTempFile(".ass", "hello world" + Environment.NewLine);

        var styles = StyleFileImportHelper.LoadStyles(fileName, new AdvancedSubStationAlpha());

        Assert.Empty(styles);
    }

    [Fact]
    public void LoadStyles_ReturnsEmptyForMissingFile()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "se-style-import-test-does-not-exist.ass");

        var styles = StyleFileImportHelper.LoadStyles(fileName, new AdvancedSubStationAlpha());

        Assert.Empty(styles);
    }

    public void Dispose()
    {
        foreach (var fileName in _tempFiles)
        {
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // ignore
            }
        }
    }
}
