using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for issue #11788: "Save as" to ASS from a style-less format (e.g. SRT)
/// must use the user's configured default ASSA style, not the hard-coded Arial default.
/// </summary>
public class AssaStyleStorageHelperTests
{
    private static SeAssaStyle MakeStoredStyle(string name, string fontName, decimal fontSize, bool isDefault)
    {
        return new SeAssaStyle
        {
            Name = name,
            FontName = fontName,
            FontSize = fontSize,
            IsDefault = isDefault,
            ColorPrimary = "#FFFFFF",
            ColorSecondary = "#FFFFFF",
            ColorOutline = "#000000",
            ColorShadow = "#000000",
            Alignment = "2",
        };
    }

    private static Subtitle MakeSrtSubtitle()
    {
        // SRT has no ASS header and the paragraphs carry no style (Extra).
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("World", 1500, 2500));
        return subtitle;
    }

    [Fact]
    public void SrtToAss_AppliesConfiguredDefaultStyle_NotArial()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Default", "Segoe UI", 90, isDefault: true));

            var subtitle = MakeSrtSubtitle();

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());

            Assert.True(applied);
            Assert.Contains("[V4+ Styles]", subtitle.Header);
            Assert.Contains("Segoe UI", subtitle.Header);
            Assert.DoesNotContain("Arial", subtitle.Header);

            // The single header style must be the configured default.
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            Assert.Equal(new[] { "Default" }, styles);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void SrtToAss_WrittenFileUsesDefaultStyle_EvenWhenParagraphsHaveNoStyle()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
            // Style name intentionally not "Default" to prove the Dialogue lines follow the header style.
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("MyStyle", "Segoe UI", 90, isDefault: true));

            var subtitle = MakeSrtSubtitle();
            AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());

            // Mirror the save path: GetUpdateSubtitle rebuilds paragraphs from the grid, so they
            // reach the writer with an empty Extra (style). The writer must then fall back to the
            // header's first style - which is now the configured default - not Arial "Default".
            foreach (var p in subtitle.Paragraphs)
            {
                p.Extra = string.Empty;
            }

            var text = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);

            var styleLine = text.SplitToLines().First(l => l.StartsWith("Style: MyStyle,"));
            Assert.Contains("Segoe UI", styleLine);
            Assert.Contains("90", styleLine);

            // Every Dialogue line must reference the configured default style (field index 3).
            var dialogueLines = text.SplitToLines().Where(l => l.StartsWith("Dialogue:")).ToList();
            Assert.NotEmpty(dialogueLines);
            foreach (var line in dialogueLines)
            {
                var styleField = line.Split(',')[3];
                Assert.Equal("MyStyle", styleField);
            }
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void NonAssTarget_DoesNothing()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Default", "Segoe UI", 90, isDefault: true));

            var subtitle = MakeSrtSubtitle();

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new SubRip());

            Assert.False(applied);
            Assert.True(string.IsNullOrEmpty(subtitle.Header));
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void AlreadyHasAssStyles_DoesNothing()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Default", "Segoe UI", 90, isDefault: true));

            var subtitle = MakeSrtSubtitle();
            var existingHeader = AdvancedSubStationAlpha.DefaultHeader; // already contains [V4+ Styles] with Arial
            subtitle.Header = existingHeader;

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());

            Assert.False(applied);
            Assert.Equal(existingHeader, subtitle.Header);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void NoDefaultStorageStyle_DoesNothing_PreservingArialFallback()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
            // A stored style exists but none is marked default.
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Some", "Segoe UI", 90, isDefault: false));

            var subtitle = MakeSrtSubtitle();

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());

            Assert.False(applied);
            Assert.True(string.IsNullOrEmpty(subtitle.Header));
        }
        finally
        {
            Se.Settings = saved;
        }
    }
}
