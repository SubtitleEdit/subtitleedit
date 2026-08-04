using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Tests.Features;

public class WaveformToolbarAudioTrackPickerTests
{
    [Fact]
    public void EnsureAllToolbarItems_AddsAudioTrackPickerVisible_ToLegacySettings()
    {
        // A settings file written before the picker existed: every type except AudioTrackPicker.
        var waveform = new SeWaveform();
        waveform.ToolbarItems.RemoveAll(p => p.Type == SeWaveformToolbarItemType.AudioTrackPicker);

        waveform.EnsureAllToolbarItems();

        var item = waveform.ToolbarItems.SingleOrDefault(p => p.Type == SeWaveformToolbarItemType.AudioTrackPicker);
        Assert.NotNull(item);
        Assert.True(item!.IsVisible); // conditional control - visible so upgrading users can discover it
    }

    [Fact]
    public void EnsureAllToolbarItems_StillAddsPlainButtonsHidden()
    {
        var waveform = new SeWaveform();
        waveform.ToolbarItems.RemoveAll(p => p.Type == SeWaveformToolbarItemType.VideoSeek);

        waveform.EnsureAllToolbarItems();

        var item = waveform.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.VideoSeek);
        Assert.False(item.IsVisible);
    }

    [Fact]
    public void ConfigureDialog_ListsAudioTrackPicker_WithRealName()
    {
        var waveform = new SeWaveform();
        waveform.EnsureAllToolbarItems();

        var vm = new WaveformToolbarItemsViewModel();
        vm.Initialize(waveform.ToolbarItems);

        var display = vm.ToolbarItems.SingleOrDefault(p => p.Type == SeWaveformToolbarItemType.AudioTrackPicker);
        Assert.NotNull(display);

        // A missing switch case would fall back to the enum name - make sure a real,
        // localized label is used.
        Assert.False(string.IsNullOrWhiteSpace(display!.Name));
        Assert.NotEqual(nameof(SeWaveformToolbarItemType.AudioTrackPicker), display.Name);
        Assert.DoesNotContain("_", display.Name);
    }
}
