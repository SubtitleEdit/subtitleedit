using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// #15741: every "Video, custom milliseconds ... back/forward" shortcut has an "...and pause" twin
/// that moves by the same milliseconds, and its title says so.
/// </summary>
public class VideoMoveCustomAndPauseShortcutsTests
{
    [Theory]
    [InlineData(1, "Back")]
    [InlineData(1, "Forward")]
    [InlineData(2, "Back")]
    [InlineData(2, "Forward")]
    [InlineData(3, "Back")]
    [InlineData(3, "Forward")]
    [InlineData(4, "Back")]
    [InlineData(4, "Forward")]
    public void PauseVariantHasTitleWithSameMilliseconds(int slot, string direction)
    {
        var lookup = ShortcutsMain.CommandTranslationLookup;
        var plain = lookup[$"VideoMoveCustom{slot}{direction}Command"];
        var pause = lookup[$"VideoMoveCustom{slot}{direction}AndPauseCommand"];

        var directionText = direction.ToLowerInvariant();
        Assert.Contains($" {directionText}, {slot}", plain);
        Assert.Contains($" {directionText} and pause, {slot}", pause);
        Assert.Equal(plain.Replace($" {directionText}, ", $" {directionText} and pause, "), pause);
    }
}
