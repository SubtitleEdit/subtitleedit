using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

namespace Tests.Features;

// The width setting of the "initial text of selected line" toolbar box (#15035).
public class WaveformToolbarItemWidthTests
{
    [Fact]
    public void GetWidthOrDefault_UnsetWidth_IsTheItemsDefault()
    {
        var item = new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.InitialText };

        Assert.Equal(0, item.Width); // what a settings file from before the setting loads as
        Assert.Equal(400, item.GetWidthOrDefault());
    }

    [Fact]
    public void GetWidthOrDefault_ItemWithoutWidth_IsZeroWhateverIsStored()
    {
        var item = new SeWaveformToolbarItem { Type = SeWaveformToolbarItemType.Play, Width = 250 };

        Assert.False(SeWaveformToolbarItem.HasWidth(item.Type));
        Assert.Equal(0, item.GetWidthOrDefault());
    }

    [Fact]
    public void Dialog_WidthOnlyOfferedForItemsWithWidth()
    {
        var vm = new WaveformToolbarItemsViewModel();
        vm.Initialize(new SeWaveform().ToolbarItems);

        vm.SelectedToolbarItem = vm.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.Play);
        Assert.False(vm.IsWidthVisible);

        vm.SelectedToolbarItem = vm.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.InitialText);
        Assert.True(vm.IsWidthVisible);
        Assert.Equal(400, vm.SelectedWidth);
    }

    [Fact]
    public void Dialog_ChangedWidth_IsSavedAndCountsAsChange()
    {
        var vm = new WaveformToolbarItemsViewModel();
        vm.Initialize(new SeWaveform().ToolbarItems);
        vm.SelectedToolbarItem = vm.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.InitialText);

        vm.SelectedWidth = 900;
        vm.OkCommand.Execute(null);

        Assert.True(vm.HasChanges);
        Assert.Equal(900, vm.ResultToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.InitialText).Width);
    }

    [Fact]
    public void Dialog_WidthEditedThenOtherItemSelected_LeavesOtherItemsAlone()
    {
        var vm = new WaveformToolbarItemsViewModel();
        vm.Initialize(new SeWaveform().ToolbarItems);
        vm.SelectedToolbarItem = vm.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.InitialText);
        vm.SelectedWidth = 900;

        vm.SelectedToolbarItem = vm.ToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.Play);
        vm.SelectedWidth = 500; // what a coerced write-back from the hidden box would look like
        vm.OkCommand.Execute(null);

        Assert.Equal(0, vm.ResultToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.Play).Width);
        Assert.Equal(900, vm.ResultToolbarItems.Single(p => p.Type == SeWaveformToolbarItemType.InitialText).Width);
    }

    [Fact]
    public void Dialog_OkWithoutEdits_OnSettingsFromBeforeTheWidthSetting_IsNoChange()
    {
        var items = new SeWaveform().ToolbarItems; // InitialText has Width 0 here, like an older settings file
        var vm = new WaveformToolbarItemsViewModel();
        vm.Initialize(items);

        vm.OkCommand.Execute(null);

        Assert.False(vm.HasChanges); // no layout rebuild just for opening and closing the dialog
    }
}
