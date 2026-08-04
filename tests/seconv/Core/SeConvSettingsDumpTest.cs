using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Tests for <c>seconv dump-settings</c> (<see cref="SeConvSettings.DumpDefaults"/>).
/// The dump exists to teach the schema (issue #11037): a user who tried the SE GUI's
/// <c>Settings.json</c> keys should be able to run one command and get a correct starter
/// file. The core guarantee is that the dump round-trips through <see cref="SeConvSettings.Load"/>
/// with zero unknown keys — it can never advertise a key the loader would reject.
/// </summary>
public class SeConvSettingsDumpTest
{
    [Fact]
    public void DumpDefaults_IsValidJson_WithAllFourSections()
    {
        var json = SeConvSettings.DumpDefaults();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.True(root.TryGetProperty("general", out _));
        Assert.True(root.TryGetProperty("tools", out _));
        Assert.True(root.TryGetProperty("removeTextForHearingImpaired", out _));
        Assert.True(root.TryGetProperty("exportImages", out _));
    }

    [Fact]
    public void DumpDefaults_RoundTripsThroughLoad_WithNoUnknownKeys()
    {
        var json = SeConvSettings.DumpDefaults();

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            var loaded = SeConvSettings.Load(path);

            // The whole point: every key the dump emits is one the loader recognizes.
            Assert.Empty(loaded.GetUnknownKeys());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DumpDefaults_UsesSeconvKeyNames_NotGuiKeyNames()
    {
        // Issue #11037: the reporter fed seconv the GUI's "IsRemoveTextUppercaseLineOn".
        // The dump must show seconv's actual key, "removeIfAllUppercase".
        var json = SeConvSettings.DumpDefaults();

        Assert.Contains("removeIfAllUppercase", json);
        Assert.DoesNotContain("IsRemoveTextUppercaseLineOn", json);
    }

    [Fact]
    public void DumpDefaults_ReflectsCurrentLibseDefaults()
    {
        var json = SeConvSettings.DumpDefaults();

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            var loaded = SeConvSettings.Load(path);

            // Values come straight from Configuration.Settings, so they must match it.
            var general = Configuration.Settings.General;
            Assert.Equal(general.SubtitleLineMaximumLength, loaded.General!.SubtitleLineMaximumLength);
            Assert.Equal(general.MinimumMillisecondsBetweenLines, loaded.General!.MinimumMillisecondsBetweenLines);
            Assert.Equal(general.MaxNumberOfLines, loaded.General!.MaxNumberOfLines);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DumpDefaults_ExportImagesColorsAndEnums_ParseBack()
    {
        // Colors are emitted as #AARRGGBB and enums as their names; both must survive a
        // load + apply cycle so a dumped file actually drives image export.
        var json = SeConvSettings.DumpDefaults();

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            var loaded = SeConvSettings.Load(path);

            var style = new ImageExportStyle();
            loaded.ApplyExportImages(style);

            // A fresh style applied with dumped defaults stays the default (Arial, white text).
            Assert.Equal("Arial", style.FontName);
            Assert.Equal(SkiaSharp.SKColors.White, style.FontColor);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
