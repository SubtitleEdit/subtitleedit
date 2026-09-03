using Nikse.SubtitleEdit.Features.Files.ExportDvbTeletext;

namespace UITests.Features.Files;

/// <summary>
/// The DVB teletext export dialog: page, language and the subtitle type the descriptor
/// announces (plain subtitles or subtitles for the hearing impaired). The type is a drop-down,
/// so the view model must map a bool both ways without losing the choice.
/// </summary>
public class ExportDvbTeletextViewModelTests
{
    [Fact]
    public void DefaultsToPlainSubtitles()
    {
        var vm = new ExportDvbTeletextViewModel();

        Assert.Equal(2, vm.SubtitleTypes.Count);
        Assert.False(vm.HearingImpaired);
        Assert.Equal(888, vm.PageNumber);
        Assert.Equal("eng", vm.LanguageCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializeSelectsTheSubtitleType(bool hearingImpaired)
    {
        var vm = new ExportDvbTeletextViewModel();
        vm.Initialize(777, "fre", hearingImpaired);

        Assert.Equal(777, vm.PageNumber);
        Assert.Equal("fre", vm.LanguageCode);
        Assert.Equal(hearingImpaired, vm.HearingImpaired);
        Assert.Same(vm.SubtitleTypes.First(t => t.IsHearingImpaired == hearingImpaired), vm.SelectedSubtitleType);
    }

    [Fact]
    public void PickingTheOtherTypeChangesTheResult()
    {
        var vm = new ExportDvbTeletextViewModel();
        vm.Initialize(888, "eng", false);

        vm.SelectedSubtitleType = vm.SubtitleTypes[1];

        Assert.True(vm.HearingImpaired);
    }
}
