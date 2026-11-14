using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Options.Settings;

/// <summary>
/// The Settings search also finds a setting that is grouped inside another item, e.g.
/// "Alignment" in the subtitle preview properties of the video player (#16614).
/// </summary>
public class SettingsSearchGroupedItemsTests
{
    [Fact]
    public void Filter_MatchesSearchTerms()
    {
        var item = new SettingsItem("Subtitle preview properties", () => null!)
        {
            SearchTerms = ["Font name", "Alignment"],
        };

        item.Filter("align");
        Assert.True(item.IsVisible);

        item.Filter("preview");
        Assert.True(item.IsVisible);

        item.Filter("deinterlace");
        Assert.False(item.IsVisible);
    }

    [AvaloniaFact]
    public void Search_Alignment_ShowsVideoPlayerSection()
    {
        var services = new ServiceCollection();
        services.AddSubtitleEditServices();
        Locator.Services = services.BuildServiceProvider();
        var vm = Locator.Services.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow(vm); // builds the sections
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var videoPlayer = vm.Sections.First(s => s.Title == Se.Language.General.VideoPlayer);
            var search = window.GetLogicalDescendants().OfType<TextBox>().First();

            search.Text = Se.Language.General.Alignment;
            Dispatcher.UIThread.RunJobs();

            Assert.True(videoPlayer.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }
}
