using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Se4Setup;

namespace UITests.Logic;

/// <summary>
/// "Set up like Subtitle Edit 4" reads the classic Settings.xml; the frame rate part of it lives in
/// &lt;General&gt;: ShowFrameRate decides whether the toolbar frame rate combo box is there, and
/// DefaultFrameRate is the rate SE 4 started at. Both are global settings, so restore them.
/// </summary>
public class Se4FrameRateImportTests : IDisposable
{
    private readonly bool _originalShowFrameRate = Se.Settings.Appearance.ToolbarShowFrameRate;
    private readonly double _originalDefaultFrameRate = Se.Settings.General.DefaultFrameRate;
    private readonly double _originalCurrentFrameRate = Se.Settings.General.CurrentFrameRate;
    private readonly double _originalLibSeDefaultFrameRate = Configuration.Settings.General.DefaultFrameRate;
    private readonly double _originalLibSeCurrentFrameRate = Configuration.Settings.General.CurrentFrameRate;

    public void Dispose()
    {
        Se.Settings.Appearance.ToolbarShowFrameRate = _originalShowFrameRate;
        Se.Settings.General.DefaultFrameRate = _originalDefaultFrameRate;
        Se.Settings.General.CurrentFrameRate = _originalCurrentFrameRate;
        Configuration.Settings.General.DefaultFrameRate = _originalLibSeDefaultFrameRate;
        Configuration.Settings.General.CurrentFrameRate = _originalLibSeCurrentFrameRate;
    }

    private static string SettingsXml(string general) =>
        $"<Settings><General>{general}</General></Settings>";

    [Fact]
    public void ApplyFrameRateFromXml_ImportsShowFrameRateAndDefaultFrameRate()
    {
        Se.Settings.Appearance.ToolbarShowFrameRate = false;
        Se.Settings.General.DefaultFrameRate = 23.976;
        Se.Settings.General.CurrentFrameRate = 23.976;

        Se4SetupApplier.ApplyFrameRateFromXml(null, SettingsXml("<ShowFrameRate>True</ShowFrameRate><DefaultFrameRate>25</DefaultFrameRate>"));

        Assert.True(Se.Settings.Appearance.ToolbarShowFrameRate);
        Assert.Equal(25, Se.Settings.General.DefaultFrameRate);
        Assert.Equal(25, Se.Settings.General.CurrentFrameRate);
        Assert.Equal(25, Configuration.Settings.General.DefaultFrameRate);
        Assert.Equal(25, Configuration.Settings.General.CurrentFrameRate);
    }

    [Fact]
    public void ApplyFrameRateFromXml_KeepsToolbarHiddenWhenSe4HadItHidden()
    {
        Se.Settings.Appearance.ToolbarShowFrameRate = true;

        Se4SetupApplier.ApplyFrameRateFromXml(null, SettingsXml("<ShowFrameRate>False</ShowFrameRate>"));

        Assert.False(Se.Settings.Appearance.ToolbarShowFrameRate);
    }

    // SE 4 wrote the rate with the invariant culture but could read back a locale-mangled value
    // ("23,976" -> 23976) and clamped it; out-of-range values are ignored here instead.
    [Theory]
    [InlineData("23976")]
    [InlineData("0")]
    [InlineData("not a number")]
    [InlineData("")]
    public void ApplyFrameRateFromXml_IgnoresOutOfRangeDefaultFrameRate(string value)
    {
        Se.Settings.General.DefaultFrameRate = 23.976;
        Se.Settings.General.CurrentFrameRate = 23.976;

        Se4SetupApplier.ApplyFrameRateFromXml(null, SettingsXml($"<DefaultFrameRate>{value}</DefaultFrameRate>"));

        Assert.Equal(23.976, Se.Settings.General.DefaultFrameRate);
        Assert.Equal(23.976, Se.Settings.General.CurrentFrameRate);
    }

    [Theory]
    [InlineData("<Settings><General></General></Settings>")]
    [InlineData("<Settings></Settings>")]
    [InlineData("not xml at all")]
    public void ApplyFrameRateFromXml_LeavesSettingsAloneWhenThereIsNothingToImport(string xml)
    {
        Se.Settings.Appearance.ToolbarShowFrameRate = false;
        Se.Settings.General.DefaultFrameRate = 23.976;

        Se4SetupApplier.ApplyFrameRateFromXml(null, xml);

        Assert.False(Se.Settings.Appearance.ToolbarShowFrameRate);
        Assert.Equal(23.976, Se.Settings.General.DefaultFrameRate);
    }
}
