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
}
