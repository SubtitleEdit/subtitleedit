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
