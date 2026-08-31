using Nikse.SubtitleEdit.Logic.Config;
using System.Text.Json;

namespace UITests.Logic.Config;

/// <summary>
/// While EBU STL is the active format, frame mode is forced on through a session-only override
/// (#14076). The user's own choice must survive that: the override wins while set, but only the
/// persisted value may ever reach the settings file.
/// </summary>
public class UseFrameModeOverrideTests
{
    [Fact]
    public void OverrideWinsWhileSet_AndSetterOnlyTouchesThePersistedValue()
    {
        var general = new SeGeneral
        {
            UseFrameModePersisted = false,
        };

        Assert.False(general.UseFrameMode);

        general.UseFrameModeOverride = true;
        Assert.True(general.UseFrameMode);
        Assert.False(general.UseFrameModePersisted);

        // An explicit set (settings dialog OK) targets the persisted value; the override keeps
        // ruling the effective value until the main view clears it.
        general.UseFrameMode = false;
        Assert.True(general.UseFrameMode);
        Assert.False(general.UseFrameModePersisted);

        general.UseFrameModeOverride = null;
        Assert.False(general.UseFrameMode);
    }

    [Fact]
    public void ActiveOverrideIsNotSerialized()
    {
        var se = new Se();
        se.General.UseFrameModePersisted = false;
        se.General.UseFrameModeOverride = true;

        var json = JsonSerializer.Serialize(se, SeJsonContext.Default.Se);

        var general = JsonDocument.Parse(json).RootElement.GetProperty("General");
        Assert.False(general.GetProperty("UseFrameMode").GetBoolean());
        Assert.False(general.TryGetProperty("UseFrameModeOverride", out _));
        Assert.False(general.TryGetProperty("UseFrameModePersisted", out _));
    }

    [Fact]
    public void SettingsFileValueLoadsIntoThePersistedValue()
    {
        // The key kept its "UseFrameMode" name, so files written before the override existed
        // (and by SE versions without it) round-trip unchanged.
        var se = JsonSerializer.Deserialize("""{"General":{"UseFrameMode":true}}""", SeJsonContext.Default.Se)!;

        Assert.True(se.General.UseFrameModePersisted);
        Assert.Null(se.General.UseFrameModeOverride);
        Assert.True(se.General.UseFrameMode);
    }
}
