using Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomSearch;

namespace UITests.Features.Options.Shortcuts;

public class CustomSearchViewModelTests
{
    [Fact]
    public void OkTrimsWhatWasTypedSoAPastedUrlStillBuildsAValidAddress()
    {
        var vm = new CustomSearchViewModel();
        vm.Initialize("  Wikipedia ", " https://en.wikipedia.org/wiki?search={0} ");

        vm.OkCommand.Execute(null);

        Assert.True(vm.OkPressed);
        Assert.Equal("Wikipedia", vm.Name);
        Assert.Equal("https://en.wikipedia.org/wiki?search={0}", vm.Url);
    }

    [Fact]
    public void CancelKeepsTheSlotUnchanged()
    {
        var vm = new CustomSearchViewModel();
        vm.Initialize("Wikipedia", "https://en.wikipedia.org/wiki?search={0}");

        vm.CancelCommand.Execute(null);

        Assert.False(vm.OkPressed);
    }
}
