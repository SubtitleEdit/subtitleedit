using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class SeConvSettingsTest : IDisposable
{
    private readonly string _tempRoot;

    public SeConvSettingsTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "SeConvSettings_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private string WriteSettings(string json)
    {
        var path = Path.Combine(_tempRoot, "settings.json");
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void Load_MissingFile_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => SeConvSettings.Load(Path.Combine(_tempRoot, "missing.json")));
    }

    [Fact]
    public void Load_AllSectionsParsed()
    {
        var path = WriteSettings("""
            {
              "general": {
                "subtitleLineMaximumLength": 50,
                "currentFrameRate": 29.97
              },
              "tools": {
                "mergeShortLinesMaxGap": 500
              },
              "removeTextForHearingImpaired": {
                "removeInterjections": true,
                "removeIfAllUppercase": true,
                "removeTextBeforeColonOnlyIfUppercase": false
              }
            }
            """);

        var s = SeConvSettings.Load(path);

        Assert.Equal(50, s.General!.SubtitleLineMaximumLength);
        Assert.Equal(29.97, s.General.CurrentFrameRate);
        Assert.Equal(500, s.Tools!.MergeShortLinesMaxGap);
        Assert.True(s.RemoveTextForHearingImpaired!.RemoveInterjections);
        Assert.True(s.RemoveTextForHearingImpaired.RemoveIfAllUppercase);
        Assert.False(s.RemoveTextForHearingImpaired.RemoveTextBeforeColonOnlyIfUppercase);
    }

    [Fact]
    public void Load_PartialSection_OmittedKeysAreNull()
    {
        var path = WriteSettings("""
            { "general": { "currentFrameRate": 24.0 } }
            """);

        var s = SeConvSettings.Load(path);

        Assert.Equal(24.0, s.General!.CurrentFrameRate);
        Assert.Null(s.General.SubtitleLineMaximumLength);
        Assert.Null(s.Tools);
        Assert.Null(s.RemoveTextForHearingImpaired);
    }

    [Fact]
    public void ApplyToLibSe_OverridesOnlySetKeys()
    {
        var savedMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var savedInterj = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections;
        try
        {
            var path = WriteSettings("""
                {
                  "general": { "subtitleMinimumDisplayMilliseconds": 1234 },
                  "removeTextForHearingImpaired": { "removeInterjections": true }
                }
                """);

            SeConvSettings.Load(path).ApplyToLibSe();

            Assert.Equal(1234, Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds);
            Assert.True(Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections);
        }
        finally
        {
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = savedMin;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections = savedInterj;
        }
    }

    [Fact]
    public void ApplyToLibSe_AppliesProfileGeneralValues()
    {
        // Regression for #11874: the FixCommonErrors profile values must be mappable.
        var g = Configuration.Settings.General;
        var saved = (g.MinimumMillisecondsBetweenLines, g.MaxNumberOfLines, g.MergeLinesShorterThan,
            g.SubtitleMaximumCharactersPerSeconds, g.SubtitleOptimalCharactersPerSeconds,
            g.SubtitleMaximumWordsPerMinute, g.DialogStyle, g.ContinuationStyle);
        try
        {
            var path = WriteSettings("""
                {
                  "general": {
                    "minimumMillisecondsBetweenLines": 24,
                    "maxNumberOfLines": 3,
                    "mergeLinesShorterThan": 40,
                    "subtitleMaximumCharactersPerSeconds": 20.5,
                    "subtitleOptimalCharactersPerSeconds": 15.0,
                    "subtitleMaximumWordsPerMinute": 240.0,
                    "dialogStyle": "DashSecondLineWithSpace",
                    "continuationStyle": "nonetrailingdots"
                  }
                }
                """);

            SeConvSettings.Load(path).ApplyToLibSe();

            Assert.Equal(24, g.MinimumMillisecondsBetweenLines);
            Assert.Equal(3, g.MaxNumberOfLines);
            Assert.Equal(40, g.MergeLinesShorterThan);
            Assert.Equal(20.5, g.SubtitleMaximumCharactersPerSeconds);
            Assert.Equal(15.0, g.SubtitleOptimalCharactersPerSeconds);
            Assert.Equal(240.0, g.SubtitleMaximumWordsPerMinute);
            Assert.Equal(DialogType.DashSecondLineWithSpace, g.DialogStyle);
            Assert.Equal(ContinuationStyle.NoneTrailingDots, g.ContinuationStyle); // case-insensitive parse
        }
        finally
        {
            (g.MinimumMillisecondsBetweenLines, g.MaxNumberOfLines, g.MergeLinesShorterThan,
                g.SubtitleMaximumCharactersPerSeconds, g.SubtitleOptimalCharactersPerSeconds,
                g.SubtitleMaximumWordsPerMinute, g.DialogStyle, g.ContinuationStyle) = saved;
        }
    }

    [Fact]
    public void Apply_WithProfile_OverlaysOnTopOfBase()
    {
        var savedMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var savedMax = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
        var savedInterj = Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections;
        try
        {
            var path = WriteSettings("""
                {
                  "general": { "subtitleMinimumDisplayMilliseconds": 500, "subtitleMaximumDisplayMilliseconds": 8000 },
                  "removeTextForHearingImpaired": { "removeInterjections": false },
                  "profiles": {
                    "broadcast": {
                      "general": { "subtitleMaximumDisplayMilliseconds": 6000 },
                      "removeTextForHearingImpaired": { "removeInterjections": true }
                    }
                  }
                }
                """);

            SeConvSettings.Load(path).ApplyToLibSe("broadcast");

            // base "min" preserved
            Assert.Equal(500, Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds);
            // profile overrode "max"
            Assert.Equal(6000, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds);
            // profile overrode interjections (was false in base, true in profile)
            Assert.True(Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections);
        }
        finally
        {
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = savedMin;
            Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds = savedMax;
            Configuration.Settings.RemoveTextForHearingImpaired.RemoveInterjections = savedInterj;
        }
    }

    [Fact]
    public void Apply_UnknownProfile_Throws()
    {
        var path = WriteSettings("""
            { "profiles": { "broadcast": { "general": { "currentFrameRate": 24.0 } } } }
            """);
        var ex = Assert.Throws<InvalidOperationException>(() => SeConvSettings.Load(path).ApplyToLibSe("missing"));
        Assert.Contains("missing", ex.Message);
        Assert.Contains("broadcast", ex.Message);
    }

    [Fact]
    public void ApplyExportImages_OverridesOnlySetKeys()
    {
        var path = WriteSettings("""
            {
              "exportImages": {
                "fontName": "Verdana",
                "fontSize": 42,
                "backgroundColor": "#B4000000",
                "boxType": "box-per-line",
                "isBold": true
              }
            }
            """);

        var style = new SeConv.Core.ImageExportStyle();
        SeConvSettings.Load(path).ApplyExportImages(style);

        Assert.Equal("Verdana", style.FontName);
        Assert.Equal(42, style.FontSize);
        Assert.Equal(180, style.BackgroundColor.Alpha);
        Assert.Equal(0, style.BackgroundColor.Red);
        Assert.Equal(Nikse.SubtitleEdit.UiLogic.Export.ExportBoxType.BoxPerLine, style.BoxType);
        Assert.True(style.IsBold);
        // Untouched keys keep their defaults
        Assert.Equal(SkiaSharp.SKColors.White, style.FontColor);
        Assert.Equal(2.5, style.OutlineWidth);
        Assert.Null(style.BottomTopMargin);
    }

    [Fact]
    public void ApplyExportImages_ProfileOverlaysOnTopOfBase()
    {
        var path = WriteSettings("""
            {
              "exportImages": { "fontSize": 40, "fontName": "Verdana" },
              "profiles": {
                "uhd": { "exportImages": { "fontSize": 80 } }
              }
            }
            """);

        var style = new SeConv.Core.ImageExportStyle();
        SeConvSettings.Load(path).ApplyExportImages(style, "uhd");

        Assert.Equal(80, style.FontSize);       // profile overrode
        Assert.Equal("Verdana", style.FontName); // base preserved
    }

    [Fact]
    public void ApplyExportImages_BadValuesAreIgnored()
    {
        var path = WriteSettings("""
            {
              "exportImages": {
                "fontColor": "notacolor",
                "boxType": "roundish",
                "alignment": "somewhere",
                "fontSize": 30
              }
            }
            """);

        var style = new SeConv.Core.ImageExportStyle();
        SeConvSettings.Load(path).ApplyExportImages(style);

        Assert.Equal(SkiaSharp.SKColors.White, style.FontColor);
        Assert.Null(style.BoxType);
        Assert.Equal(Nikse.SubtitleEdit.UiLogic.Export.ExportAlignment.BottomCenter, style.Alignment);
        Assert.Equal(30, style.FontSize);
    }

    [Fact]
    public void Load_AllowsCommentsAndTrailingCommas()
    {
        var path = WriteSettings("""
            {
              // comment is OK
              "general": {
                "currentFrameRate": 25.0,
              },
            }
            """);

        var s = SeConvSettings.Load(path);

        Assert.Equal(25.0, s.General!.CurrentFrameRate);
    }

    // Unknown keys stay ignored (a file written for a newer seconv must still apply what this
    // one understands), but they have to be reportable - silently dropping them made a typo
    // look identical to a working settings file (issue #12437).
    [Fact]
    public void GetUnknownKeys_ReportsTyposAndUnknownSections()
    {
        var path = WriteSettings("""
            {
              "general": {
                "minimumMsBetweenLines": 96,
                "subtitleLineMaximumLength": 50
              },
              "bogusSection": { "x": 1 },
              "profiles": {
                "netflix": { "general": { "alsoWrong": 1 } }
              }
            }
            """);

        var s = SeConvSettings.Load(path);
        var unknown = s.GetUnknownKeys().ToList();

        Assert.Contains("general.minimumMsBetweenLines", unknown);
        Assert.Contains("bogusSection", unknown);
        Assert.Contains("profiles.netflix.general.alsoWrong", unknown);
        Assert.Equal(3, unknown.Count);

        // the known key alongside the typo is still applied
        Assert.Equal(50, s.General!.SubtitleLineMaximumLength);
    }

    [Fact]
    public void GetUnknownKeys_EmptyForAValidFile()
    {
        var path = WriteSettings("""
            {
              "general": { "minimumMillisecondsBetweenLines": 12 },
              "tools": { "mergeShortLinesMaxGap": 250 }
            }
            """);

        var s = SeConvSettings.Load(path);

        Assert.Empty(s.GetUnknownKeys());
        Assert.Equal(12, s.General!.MinimumMillisecondsBetweenLines);
    }
}
