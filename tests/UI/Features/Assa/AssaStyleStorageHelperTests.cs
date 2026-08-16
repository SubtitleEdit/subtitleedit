using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for issue #11788 ("Save as" to ASS from a style-less format must use the
/// user's configured default ASSA style, not the hard-coded Arial default) and issue #13653
/// (converting to ASSA embeds the whole default storage category, like SE 4 - and the same
/// mechanism works for SSA with its own storage).
/// </summary>
public class AssaStyleStorageHelperTests
{
    private static SeAssaStyle MakeStoredStyle(string name, string fontName, decimal fontSize, bool isDefault, string category = "")
    {
        return new SeAssaStyle
        {
            Name = name,
            FontName = fontName,
            FontSize = fontSize,
            IsDefault = isDefault,
            Category = category,
            ColorPrimary = "#FFFFFF",
            ColorSecondary = "#FFFFFF",
            ColorOutline = "#000000",
            ColorShadow = "#000000",
            Alignment = "2",
        };
    }

    /// <summary>
    /// Settings with an empty style storage. A fresh <see cref="Se"/> is seeded with one default
    /// style (first start / settings reset), which these tests replace with their own fixtures.
    /// </summary>
    private static Se MakeSettingsWithEmptyStorage()
    {
        var settings = new Se();
        settings.Assa.StoredStyles.Clear();
        settings.Ssa.StoredStyles.Clear();
        return settings;
    }

    private static string MakeHeaderWithStyles(params string[] styleNames)
    {
        var styles = styleNames.Select(n => new SsaStyle { Name = n }).ToList();
        return AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(AdvancedSubStationAlpha.DefaultHeader, styles);
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
            Se.Settings = MakeSettingsWithEmptyStorage();
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
            Se.Settings = MakeSettingsWithEmptyStorage();
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
    public void SrtToAss_WholeCategoryOfTheDefaultStyleIsApplied()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            // SE 4 semantics (issue #13653): the default style's whole category is the template.
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Sign", "Verdana", 30, isDefault: false, category: "Movie"));
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Speech", "Segoe UI", 90, isDefault: true, category: "Movie"));
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Unrelated", "Courier New", 20, isDefault: false, category: "TV"));

            var subtitle = MakeSrtSubtitle();

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());

            Assert.True(applied);

            // The default style leads (it is what paragraphs point at), its category siblings
            // follow, other categories stay out.
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            Assert.Equal(new[] { "Speech", "Sign" }, styles);
            Assert.DoesNotContain("Courier New", subtitle.Header);

            Assert.All(subtitle.Paragraphs, p => Assert.Equal("Speech", p.Extra));
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void SrtToAss_NoDefaultFlag_DefaultCategoryStylesAreApplied()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            // No style is marked default, but the built-in default category (empty name) holds
            // styles - SE 4 applied those without any flag, so SE 5 does too (issue #13653).
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Narrator", "Segoe UI", 90, isDefault: false));
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Sign", "Verdana", 30, isDefault: false));
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Unrelated", "Courier New", 20, isDefault: false, category: "TV"));

            var subtitle = MakeSrtSubtitle();

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());

            Assert.True(applied);
            Assert.Equal(new[] { "Narrator", "Sign" }, AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header));
            Assert.All(subtitle.Paragraphs, p => Assert.Equal("Narrator", p.Extra));
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
            Se.Settings = MakeSettingsWithEmptyStorage();
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
            Se.Settings = MakeSettingsWithEmptyStorage();
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
    public void SsaSourceSavedAsAss_KeepsItsOwnStyles()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Default", "Segoe UI", 90, isDefault: true));

            // An SSA file carries [V4 Styles]; saving it as ASSA converts the header on write, so
            // the storage default must not clobber the file's real styles.
            var subtitle = MakeSrtSubtitle();
            var existingHeader = SubStationAlpha.DefaultHeader;
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
    public void NoDefaultFlagAndEmptyDefaultCategory_DoesNothing_PreservingArialFallback()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            // A stored style exists, but none is marked default and none is in the default
            // category - there is nothing to call "the default template".
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Some", "Segoe UI", 90, isDefault: false, category: "TV"));

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

    [Fact]
    public void SrtToSsa_AppliesConfiguredSsaDefaultStyle_NotArial()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            Se.Settings.Ssa.StoredStyles.Add(MakeStoredStyle("MySsaStyle", "Georgia", 28, isDefault: true));

            var subtitle = MakeSrtSubtitle();

            var applied = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new SubStationAlpha());

            Assert.True(applied);
            Assert.Contains("[V4 Styles]", subtitle.Header);
            Assert.DoesNotContain("[V4+ Styles]", subtitle.Header);
            Assert.Contains("Georgia", subtitle.Header);
            Assert.All(subtitle.Paragraphs, p => Assert.Equal("MySsaStyle", p.Extra));

            // The written SSA file must carry the style and reference it from the Dialogue lines.
            var text = new SubStationAlpha().ToText(subtitle, string.Empty);
            Assert.Contains("Style: MySsaStyle,Georgia,28", text);
            var dialogueLines = text.SplitToLines().Where(l => l.StartsWith("Dialogue:")).ToList();
            Assert.NotEmpty(dialogueLines);
            foreach (var line in dialogueLines)
            {
                Assert.Equal("MySsaStyle", line.Split(',')[3]);
            }
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void SsaAndAssaStoragesAreSeparate()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            // Only the ASSA storage has a default - an SSA conversion must not pick it up,
            // and vice versa (the two formats keep separate style storages).
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("AssaOnly", "Segoe UI", 90, isDefault: true));

            var subtitle = MakeSrtSubtitle();
            var appliedSsa = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new SubStationAlpha());
            Assert.False(appliedSsa);
            Assert.True(string.IsNullOrEmpty(subtitle.Header));

            Se.Settings = MakeSettingsWithEmptyStorage();
            Se.Settings.Ssa.StoredStyles.Add(MakeStoredStyle("SsaOnly", "Georgia", 28, isDefault: true));

            subtitle = MakeSrtSubtitle();
            var appliedAssa = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha());
            Assert.False(appliedAssa);
            Assert.True(string.IsNullOrEmpty(subtitle.Header));
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_SsaWithNoStorage_GetsSsaHeaderNotAssa()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();

            // Regression: typing the first line of a new SSA file used to stamp an ASSA
            // (v4.00+/[V4+ Styles]) header onto the SSA subtitle.
            var subtitle = new Subtitle();
            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new SubStationAlpha());

            Assert.Equal("Default", styleName);
            Assert.Contains("[V4 Styles]", subtitle.Header);
            Assert.DoesNotContain("[V4+ Styles]", subtitle.Header);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_UsesExistingFileStyles()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("StorageStyle", "Segoe UI", 90, isDefault: true));

            // A file that already defines styles wins over the storage default.
            var subtitle = MakeSrtSubtitle();
            AssaStyleStorageHelper.ApplyDefaultStorageStyle(subtitle, new AdvancedSubStationAlpha());
            Se.Settings.Assa.StoredStyles[0].Name = "RenamedLater";

            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new AdvancedSubStationAlpha());

            Assert.Equal("StorageStyle", styleName);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void FreshSettings_SeedTheStyleStorageWithOneDefaultStyle()
    {
        // First start and "Reset settings" both land on a plain new Se() - the storage must not
        // be empty there, or "Styles saved" shows nothing to make default.
        var settings = new Se();

        var assaStyle = Assert.Single(settings.Assa.StoredStyles);
        Assert.Equal("Default", assaStyle.Name);
        Assert.True(assaStyle.IsDefault);
        Assert.Equal(string.Empty, assaStyle.Category);

        var ssaStyle = Assert.Single(settings.Ssa.StoredStyles);
        Assert.Equal("Default", ssaStyle.Name);
        Assert.True(ssaStyle.IsDefault);
    }

    [Fact]
    public void SavedSettings_DoNotGainASecondSeededStyleOnReload()
    {
        var saved = Se.Settings;
        var settingsFileName = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"), "Settings.json");
        try
        {
            // Deserialization runs the same constructors as a fresh install, so the seeded style
            // must be replaced by what the file holds - never appended to it.
            Se.Settings = MakeSettingsWithEmptyStorage();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("MyStyle", "Segoe UI", 90, isDefault: true));
            Se.SaveSettings(settingsFileName);

            Se.LoadSettings(settingsFileName);

            var style = Assert.Single(Se.Settings.Assa.StoredStyles);
            Assert.Equal("MyStyle", style.Name);

            // An emptied storage stays empty too.
            Assert.Empty(Se.Settings.Ssa.StoredStyles);
        }
        finally
        {
            Se.Settings = saved;
            var directory = Path.GetDirectoryName(settingsFileName);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void FreshSettings_SeededStyleMatchesTheBuiltInAssaDefaultStyle()
    {
        var saved = Se.Settings;
        try
        {
            // The seed must not change what a fresh install produces: converting to ASSA with the
            // seeded storage has to yield the same styles as libse's built-in default header.
            Se.Settings = new Se();

            var subtitle = MakeSrtSubtitle();
            Assert.True(AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new AdvancedSubStationAlpha()));

            var seeded = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
            var builtIn = AdvancedSubStationAlpha.GetSsaStylesFromHeader(AdvancedSubStationAlpha.DefaultHeader);

            Assert.Equal(builtIn.Count, seeded.Count);
            Assert.Equal(builtIn[0].Name, seeded[0].Name);
            Assert.Equal(builtIn[0].FontName, seeded[0].FontName);
            Assert.Equal(builtIn[0].FontSize, seeded[0].FontSize);
            Assert.Equal(builtIn[0].Primary, seeded[0].Primary);
            Assert.Equal(builtIn[0].Secondary, seeded[0].Secondary);
            Assert.Equal(builtIn[0].Outline, seeded[0].Outline);
            Assert.Equal(builtIn[0].Background, seeded[0].Background);
            Assert.Equal(builtIn[0].Alignment, seeded[0].Alignment);
            Assert.Equal(builtIn[0].BorderStyle, seeded[0].BorderStyle);
            Assert.Equal(builtIn[0].OutlineWidth, seeded[0].OutlineWidth);
            Assert.Equal(builtIn[0].ShadowWidth, seeded[0].ShadowWidth);
            Assert.Equal(builtIn[0].MarginLeft, seeded[0].MarginLeft);
            Assert.Equal(builtIn[0].MarginRight, seeded[0].MarginRight);
            Assert.Equal(builtIn[0].MarginVertical, seeded[0].MarginVertical);
            Assert.Equal(builtIn[0].ScaleX, seeded[0].ScaleX);
            Assert.Equal(builtIn[0].ScaleY, seeded[0].ScaleY);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_KeepsTheNeighborStyle()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();

            // SE 4 parity (issue #13677): a new line keeps the style of the line it is inserted
            // next to, instead of adopting whichever style sits first in the header.
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };

            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new AdvancedSubStationAlpha(), "Jacob Andrews");

            Assert.Equal("Jacob Andrews", styleName);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_NeighborStyleIsMatchedCaseInsensitively()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();

            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews") };

            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new AdvancedSubStationAlpha(), "jacob andrews");

            // The file's own spelling wins - a Dialogue line must reference the style verbatim.
            Assert.Equal("Jacob Andrews", styleName);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_NoNeighbor_UsesStorageDefaultPresentInTheFile()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            // "Set style as default" in the storage used to be ignored the moment a file had its
            // own styles - it now decides among them when there is no neighbour to follow (#13677).
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("Both", "Segoe UI", 90, isDefault: true));

            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };

            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new AdvancedSubStationAlpha());

            Assert.Equal("Both", styleName);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_NoNeighborAndNoStorageMatch_PrefersTheStyleNamedDefault()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("NotInThisFile", "Segoe UI", 90, isDefault: true));

            // The reported case (#13677): "Default" carries the whole subtitle but sits last, so
            // every new line used to come out as "Julia Lepetit".
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews", "Both", "Default") };

            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new AdvancedSubStationAlpha());

            Assert.Equal("Default", styleName);
        }
        finally
        {
            Se.Settings = saved;
        }
    }

    [Fact]
    public void GetStyleNameForNewParagraph_UnknownNeighborAndNoDefaultStyle_FallsBackToFirstHeaderStyle()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = MakeSettingsWithEmptyStorage();

            // No "Default" style and the neighbour's style was renamed away - the first header
            // style remains the last resort.
            var subtitle = new Subtitle { Header = MakeHeaderWithStyles("Julia Lepetit", "Jacob Andrews") };

            var styleName = AssaStyleStorageHelper.GetStyleNameForNewParagraph(subtitle, new AdvancedSubStationAlpha(), "Deleted style");

            Assert.Equal("Julia Lepetit", styleName);
        }
        finally
        {
            Se.Settings = saved;
        }
    }
}
