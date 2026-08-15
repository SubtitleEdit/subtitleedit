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
    public void SrtToAss_WholeCategoryOfTheDefaultStyleIsApplied()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
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
            Se.Settings = new Se();
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
    public void SsaSourceSavedAsAss_KeepsItsOwnStyles()
    {
        var saved = Se.Settings;
        try
        {
            Se.Settings = new Se();
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
            Se.Settings = new Se();
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
            Se.Settings = new Se();
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
            Se.Settings = new Se();
            // Only the ASSA storage has a default - an SSA conversion must not pick it up,
            // and vice versa (the two formats keep separate style storages).
            Se.Settings.Assa.StoredStyles.Add(MakeStoredStyle("AssaOnly", "Segoe UI", 90, isDefault: true));

            var subtitle = MakeSrtSubtitle();
            var appliedSsa = AssaStyleStorageHelper.ApplyDefaultStorageStyleForFormatConversion(subtitle, new SubStationAlpha());
            Assert.False(appliedSsa);
            Assert.True(string.IsNullOrEmpty(subtitle.Header));

            Se.Settings = new Se();
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
            Se.Settings = new Se();

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
            Se.Settings = new Se();
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
}
